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

    Private Function InputOutputCommand(inputIndex As Integer, turnOn As Boolean) As String
        Dim onCommands() As String = {"L", "M", "C", "D", "N", "O"}
        Dim offCommands() As String = {"l", "m", "c", "d", "n", "o"}

        If inputIndex < 0 OrElse inputIndex > onCommands.Length - 1 Then Return ""
        Return If(turnOn, onCommands(inputIndex), offCommands(inputIndex))
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
        Arduino.WriteLine(command)
    End Sub

    Private Function OpenArduinoOrSimulation() As Boolean
        Try
            Arduino = New SerialPort(SetUp.txtCOM.Text, 9600)
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

        Do While closingTest = False
            Try
                If SimulationMode = False AndAlso Arduino IsNot Nothing AndAlso Arduino.IsOpen AndAlso Arduino.BytesToRead > 0 Then
                    Dim readValues() As String = Split(Arduino.ReadLine(), ",")

                    For inputIndex As Integer = 0 To MAX_INPUTS - 1
                        If inputIndex <= readValues.Length - 1 Then
                            Actual_Response(inputIndex) = readValues(inputIndex).Trim()
                        End If

                        If InputValueLabels(inputIndex) IsNot Nothing Then
                            InputValueLabels(inputIndex).Text = Actual_Response(inputIndex)
                        End If

                        If Actual_Response(inputIndex) <> Previous_Response(inputIndex) AndAlso
                           Actual_Response(inputIndex) <> "1" Then
                            TestResponse(inputIndex)
                        End If

                        Previous_Response(inputIndex) = Actual_Response(inputIndex)
                    Next
                End If
            Catch ex As Exception
                If tmrChartTests.Enabled = False Then Exit Do
            End Try

            My.Application.DoEvents()
        Loop

        If Me.IsDisposed = False AndAlso Me.Disposing = False Then Me.Close()
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
        SafeWrite("abefhtlmcdnop")
        If Arduino IsNot Nothing AndAlso Arduino.IsOpen Then Arduino.Close()
        tmrFeeder.Enabled = False
        tmrChartTests.Enabled = False
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        StopTestHardware()
        Me.Hide()
    End Sub

    Private Sub Tests_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        StopTestHardware()
    End Sub
End Class
