Imports System.IO.Ports
Imports System.Windows.Forms.DataVisualization.Charting

Public Class Tests
    Public Arduino As SerialPort

    Private ReadOnly ResponseCounts(MAX_INPUTS - 1) As Integer
    Private ReadOnly InputValueLabels(MAX_INPUTS - 1) As Label
    Private ReadOnly InputCountLabels(MAX_INPUTS - 1) As Label
    Private ReadOnly InputOutputButtons(MAX_INPUTS - 1) As Button
    Private ReadOnly InputOutputState(MAX_INPUTS - 1) As Boolean
    Private WithEvents tmrChartTests As Timer = New Timer
    Private chrtTime As Integer
    Private pumpOn As Boolean
    Private lightsOn As Boolean
    Private closingTest As Boolean
    Private SimulationMode As Boolean = False
    Private WithEvents acquisitionTimer As New Timer With {.Interval = 10}
    Private serialBuffer As String = ""
    Private ReadOnly Previous_Response() As String = Enumerable.Repeat("1", MAX_INPUTS).ToArray()

    Private Sub AcquisitionTick(sender As Object, e As EventArgs) Handles acquisitionTimer.Tick
        If closingTest OrElse SimulationMode Then Return
        Try
            If Arduino Is Nothing OrElse Not Arduino.IsOpen Then Throw New IO.IOException("The COM connection was closed.")
            serialBuffer &= Arduino.ReadExisting()
            Dim processed As Integer = 0
            Dim newline As Integer = serialBuffer.IndexOf(vbLf, StringComparison.Ordinal)
            While newline >= 0 AndAlso processed < 256
                Dim values() As String = serialBuffer.Substring(0, newline).Trim().Split(","c)
                serialBuffer = serialBuffer.Substring(newline + 1)
                If values.Length <= MAX_INPUTS AndAlso values.All(Function(value) value = "0" OrElse value = "1") Then
                    For inputIndex As Integer = 0 To values.Length - 1
                        InputValueLabels(inputIndex).Text = values(inputIndex)
                        If values(inputIndex) = "0" AndAlso Previous_Response(inputIndex) = "1" Then TestResponse(inputIndex)
                        Previous_Response(inputIndex) = values(inputIndex)
                    Next
                End If
                processed += 1
                newline = serialBuffer.IndexOf(vbLf, StringComparison.Ordinal)
            End While
            If serialBuffer.Length > 65536 Then Throw New IO.IOException("Invalid Arduino response stream.")
        Catch ex As Exception
            StopTestHardware()
            MessageBox.Show(Me, ex.Message, "Hardware tests")
            Close()
        End Try
    End Sub

    Private Function TestInputName(inputIndex As Integer) As String
        For componentIndex As Integer = 1 To Math.Max(vCC, 1)
            If AC(componentIndex).InputName IsNot Nothing AndAlso
               inputIndex <= AC(componentIndex).InputName.Length - 1 AndAlso
               AC(componentIndex).InputName(inputIndex) <> "" Then
                Return AC(componentIndex).InputName(inputIndex)
            End If
        Next

        Return "Input " & (inputIndex + 1)
    End Function

    Private Sub MarkTested(button As Button)
        If button Is Nothing Then Exit Sub
        button.BackColor = Color.LightGreen
        button.UseVisualStyleBackColor = False
    End Sub

    Private Sub SafeWrite(command As String)
        If SimulationMode Then Exit Sub
        If command = "" Then Exit Sub
        If Arduino Is Nothing OrElse Arduino.IsOpen = False Then Exit Sub
        Try
            Arduino.WriteLine(command)
        Catch ex As Exception
            If closingTest Then Return
            StopTestHardware()
            MessageBox.Show(Me, ex.Message, "Hardware tests")
            Close()
        End Try
    End Sub

    Private Function OpenArduinoOrSimulation() As Boolean
        Try
            Arduino = New SerialPort(SetUp.txtCOM.Text, 9600) With {.WriteTimeout = 100, .ReadTimeout = 100}
            Arduino.Open()
            SimulationMode = False
            Return True
        Catch ex As Exception
            Dim result As DialogResult = MessageBox.Show(
                "No Arduino was found on " & SetUp.txtCOM.Text & "." & Environment.NewLine &
                "If you continue, tests will run in simulation mode without hardware input/output." & Environment.NewLine &
                "Choose Cancel to close this test window.",
                "Arduino not found",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning)

            If result = DialogResult.OK Then
                SimulationMode = True
                Arduino = Nothing
                Return True
            End If

            Return False
        End Try
    End Function

    Private Sub ConfigureChart()
        Chart1.Series.Clear()
        Chart1.Location = New Point(520, 18)
        Chart1.Size = New Size(575, 620)

        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Dim series As New Series("Input " & (inputIndex + 1))
            series.ChartArea = "ChartArea1"
            series.ChartType = SeriesChartType.StepLine
            series.Legend = "Legend1"
            series.LegendText = TestInputName(inputIndex)
            Chart1.Series.Add(series)
        Next
    End Sub

    Private Sub ConfigureDynamicControls()
        Me.ClientSize = New Size(1107, 655)
        btnLights.Location = New Point(263, 18)
        btnTone.Location = New Point(263, 108)
        btnFeeder.Location = New Point(263, 198)
        btnPumpOn.Location = New Point(263, 288)
        btnClose.Location = New Point(263, 558)

        Dim startX As Integer = 13
        Dim y As Integer = 18

        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Dim nameLabel As New Label()
            nameLabel.AutoSize = False
            nameLabel.Location = New Point(startX, y + (inputIndex * 36))
            nameLabel.Size = New Size(110, 22)
            nameLabel.Text = TestInputName(inputIndex) & ":"
            Controls.Add(nameLabel)

            Dim valueLabel As New Label()
            valueLabel.AutoSize = False
            valueLabel.BorderStyle = BorderStyle.FixedSingle
            valueLabel.Location = New Point(startX + 115, y + (inputIndex * 36))
            valueLabel.Size = New Size(45, 22)
            valueLabel.Text = "-"
            valueLabel.TextAlign = ContentAlignment.MiddleCenter
            Controls.Add(valueLabel)
            InputValueLabels(inputIndex) = valueLabel

            Dim countLabel As New Label()
            countLabel.AutoSize = False
            countLabel.BorderStyle = BorderStyle.FixedSingle
            countLabel.Location = New Point(startX + 165, y + (inputIndex * 36))
            countLabel.Size = New Size(55, 22)
            countLabel.Text = "0"
            countLabel.TextAlign = ContentAlignment.MiddleCenter
            Controls.Add(countLabel)
            InputCountLabels(inputIndex) = countLabel
        Next

        Dim outputY As Integer = y + (MAX_INPUTS * 36) + 16
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Dim outputButton As New Button()
            outputButton.Location = New Point(startX, outputY + (inputIndex * 36))
            outputButton.Size = New Size(230, 30)
            outputButton.Tag = inputIndex

            If InputOutputCommand(inputIndex, True) = "" Then
                outputButton.Text = TestInputName(inputIndex) & " output N/A"
                outputButton.Enabled = False
            Else
                outputButton.Text = TestInputName(inputIndex) & " output OFF"
                AddHandler outputButton.Click, AddressOf btnInputOutput_Click
            End If

            Controls.Add(outputButton)
            InputOutputButtons(inputIndex) = outputButton
        Next
    End Sub

    Private Sub Tests_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ConfigureChart()
        ConfigureDynamicControls()
    End Sub

    Private Sub tmrStart_Tick(sender As Object, e As EventArgs) Handles tmrStart.Tick
        tmrStart.Enabled = False
        If OpenArduinoOrSimulation() = False Then
            Me.Close()
            Exit Sub
        End If

        SafeWrite("p")
        tmrChartTests.Interval = 1000
        tmrChartTests.Enabled = True

        acquisitionTimer.Start()
    End Sub

    Private Sub TestResponse(inputIndex As Integer)
        ResponseCounts(inputIndex) += 1

        If InputCountLabels(inputIndex) IsNot Nothing Then
            InputCountLabels(inputIndex).Text = CStr(ResponseCounts(inputIndex))
            InputCountLabels(inputIndex).BackColor = Color.LightGreen
        End If
    End Sub

    Private Sub btnInputOutput_Click(sender As Object, e As EventArgs)
        Dim outputButton As Button = DirectCast(sender, Button)
        Dim inputIndex As Integer = CInt(outputButton.Tag)

        InputOutputState(inputIndex) = Not InputOutputState(inputIndex)
        SafeWrite(InputOutputCommand(inputIndex, InputOutputState(inputIndex)))

        MarkTested(outputButton)
        outputButton.Text = TestInputName(inputIndex) & If(InputOutputState(inputIndex), " output ON", " output OFF")
    End Sub

    Private Sub btnFeeder_Click(sender As Object, e As EventArgs) Handles btnFeeder.Click
        tmrFeeder.Enabled = Not tmrFeeder.Enabled
        MarkTested(btnFeeder)
    End Sub

    Private Sub tmrFeeder_Tick(sender As Object, e As EventArgs) Handles tmrFeeder.Tick
        SafeWrite("R")
    End Sub

    Private Sub btnPumpOn_Click(sender As Object, e As EventArgs) Handles btnPumpOn.Click
        pumpOn = Not pumpOn
        SafeWrite(If(pumpOn, "P", "p"))
        MarkTested(btnPumpOn)
        btnPumpOn.Text = If(pumpOn, "Pump ON", "Pump OFF")
    End Sub



    Private Sub btnLights_Click(sender As Object, e As EventArgs) Handles btnLights.Click
        lightsOn = Not lightsOn
        SafeWrite(If(lightsOn, "ABEFH", "abefh"))
        MarkTested(btnLights)
    End Sub

    Private Sub btnTone_Click(sender As Object, e As EventArgs) Handles btnTone.Click
        SafeWrite("Z")
        MarkTested(btnTone)
    End Sub

    Private Sub tmrChrt_Tick(sender As Object, e As EventArgs) Handles tmrChartTests.Tick
        chrtTime += 1

        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Chart1.Series("Input " & (inputIndex + 1)).Points.AddXY(chrtTime, ResponseCounts(inputIndex))
        Next
    End Sub

    Private Sub StopTestHardware()
        If closingTest Then Exit Sub
        closingTest = True
        acquisitionTimer.Stop()
        tmrFeeder.Enabled = False
        tmrChartTests.Enabled = False
        Try
            SafeWrite("abefht" & AllInputsOffCommand() & "p")
        Catch
        Finally
            If Arduino IsNot Nothing Then Arduino.Dispose()
        End Try
        acquisitionTimer.Dispose()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        StopTestHardware()
        Me.Hide()
    End Sub

    Private Sub Tests_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        StopTestHardware()
    End Sub
End Class
