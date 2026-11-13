Public Class LeverCommandSettings

    Private ReadOnly commandBoxes As TextBox()

    Public Sub New()
        InitializeComponent()
        Dim feeder2 As New TextBox With {.Name = "txtFeeder2Pulse", .Location = New Drawing.Point(212, 382), .Size = New Drawing.Size(48, 20), .MaxLength = 1}
        Dim pump2 As New TextBox With {.Name = "txtPump2Pulse", .Location = New Drawing.Point(212, 412), .Size = New Drawing.Size(48, 20), .MaxLength = 1}
        grpOutputs.Controls.Add(New Label With {.Text = "Feeder 2 pulse", .Location = New Drawing.Point(14, 385), .AutoSize = True})
        grpOutputs.Controls.Add(New Label With {.Text = "Pump 2 pulse", .Location = New Drawing.Point(14, 415), .AutoSize = True})
        grpOutputs.Controls.Add(feeder2)
        grpOutputs.Controls.Add(pump2)
        grpOutputs.Height += 60
        For Each button In {btnDefaults, btnApply, btnCancel}
            button.Top += 60
        Next
        ClientSize = New Drawing.Size(ClientSize.Width, ClientSize.Height + 60)
        lblPump.Text = "Pump 1"
        lblFeederPulse.Text = "Feeder 1 pulse"
        lblPumpPulse.Text = "Pump 1 pulse"
        lblInstructions.Text = "Enter one letter per command. Case matters. First-output defaults match the bundled sketch. Feeder 2 and Pump 2 require matching firmware and wiring; changing letters does not program Arduino."
        commandBoxes = {
            txtLeverLetter1, txtLeverRetract1, txtLeverLetter2, txtLeverRetract2,
            txtLeverLetter3, txtLeverRetract3, txtLeverLetter4, txtLeverRetract4,
            txtLeverLetter5, txtLeverRetract5, txtLeverLetter6, txtLeverRetract6,
            txtLight1On, txtLight1Off, txtLight2On, txtLight2Off,
            txtLight3On, txtLight3Off, txtLight4On, txtLight4Off,
            txtHouseLightOn, txtHouseLightOff, txtToneOn, txtToneOff, txtPumpOn, txtPumpOff,
            txtFeederPulse, txtPumpPulse, txtTonePulse, txtAllOff, feeder2, pump2
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
