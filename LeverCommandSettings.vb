Public Class LeverCommandSettings

    Private ReadOnly commandBoxes As TextBox()

    Public Sub New()
        InitializeComponent()
        commandBoxes = {
            txtLeverLetter1, txtLeverRetract1, txtLeverLetter2, txtLeverRetract2,
            txtLeverLetter3, txtLeverRetract3, txtLeverLetter4, txtLeverRetract4,
            txtLeverLetter5, txtLeverRetract5, txtLeverLetter6, txtLeverRetract6,
            txtLight1On, txtLight1Off, txtLight2On, txtLight2Off,
            txtLight3On, txtLight3Off, txtLight4On, txtLight4Off,
            txtHouseLightOn, txtHouseLightOff, txtToneOn, txtToneOff, txtPumpOn, txtPumpOff,
            txtFeederPulse, txtPumpPulse, txtTonePulse, txtAllOff
        }
    End Sub

    Private Sub LoadCommands(sender As Object, e As EventArgs) Handles MyBase.Load
        If DesignMode OrElse System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then Return
        DisplayMap(ArduinoCommands.Snapshot())
    End Sub

    Private Sub DisplayMap(settings As ArduinoCommandMap)
        For index As Integer = 0 To commandBoxes.Length - 1
            commandBoxes(index).Text = settings.Codes(index).ToString()
        Next
    End Sub

    Private Sub RestoreDefaults(sender As Object, e As EventArgs) Handles btnDefaults.Click
        DisplayMap(New ArduinoCommandMap())
    End Sub

    Private Sub ApplyCommands(sender As Object, e As EventArgs) Handles btnApply.Click
        For index As Integer = 0 To commandBoxes.Length - 1
            If commandBoxes(index).Text.Length <> 1 Then
                MessageBox.Show(Me, "Enter one letter for " & ArduinoCommandMap.CommandName(index) & ".", Text)
                commandBoxes(index).Focus()
                Exit Sub
            End If
        Next
        Dim settings As New ArduinoCommandMap With {.Codes = String.Concat(commandBoxes.Select(Function(box) box.Text))}
        Dim validationError As String = settings.ValidationError()
        If validationError <> "" Then
            MessageBox.Show(Me, validationError, Text)
            Exit Sub
        End If
        ArduinoCommands.Configure(settings)
        TestsPerformed = False
        DialogResult = DialogResult.OK
        Close()
    End Sub
End Class
