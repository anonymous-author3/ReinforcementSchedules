Imports System.IO.Ports
Imports System.Windows.Forms.DataVisualization.Charting

Public Class Tests
    Public Arduino As SerialPort

    Private ReadOnly ResponseCounts(MAX_INPUTS - 1) As Integer
    Private ReadOnly InputValueLabels(MAX_INPUTS - 1) As Label
    Private ReadOnly InputCountLabels(MAX_INPUTS - 1) As Label
    Private ReadOnly InputOutputButtons(MAX_INPUTS - 1) As Button
    Private ReadOnly InputOutputState(MAX_INPUTS - 1) As Boolean
    Private chrtTime As Integer
    Private pumpOn As Boolean
    Private lightsOn As Boolean
    Private closingTest As Boolean
    Private SimulationMode As Boolean = False
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
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Dim series As Series = Chart1.Series("Input " & (inputIndex + 1))
            series.Points.Clear()
            series.LegendText = TestInputName(inputIndex)
        Next
    End Sub

    Private Sub BindInputControls()
        Dim names As Label() = {lblInputName1, lblInputName2, lblInputName3, lblInputName4, lblInputName5, lblInputName6}
        Dim values As Label() = {lblInputValue1, lblInputValue2, lblInputValue3, lblInputValue4, lblInputValue5, lblInputValue6}
        Dim counts As Label() = {lblInputCount1, lblInputCount2, lblInputCount3, lblInputCount4, lblInputCount5, lblInputCount6}
        Dim outputs As Button() = {btnInputOutput1, btnInputOutput2, btnInputOutput3, btnInputOutput4, btnInputOutput5, btnInputOutput6}
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            names(inputIndex).Text = TestInputName(inputIndex) & ":"
            InputValueLabels(inputIndex) = values(inputIndex)
            InputCountLabels(inputIndex) = counts(inputIndex)
            InputOutputButtons(inputIndex) = outputs(inputIndex)
            Dim hasOutput As Boolean = InputOutputCommand(inputIndex, True) <> ""
            outputs(inputIndex).Enabled = hasOutput
            outputs(inputIndex).Text = TestInputName(inputIndex) & If(hasOutput, " output OFF", " output N/A")
        Next
    End Sub

    Private Sub Tests_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If DesignMode OrElse System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then Return
        ConfigureChart()
        BindInputControls()
        tmrStart.Start()
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

    Private Sub btnInputOutput_Click(sender As Object, e As EventArgs) Handles btnInputOutput1.Click, btnInputOutput2.Click, btnInputOutput3.Click, btnInputOutput4.Click, btnInputOutput5.Click, btnInputOutput6.Click
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
