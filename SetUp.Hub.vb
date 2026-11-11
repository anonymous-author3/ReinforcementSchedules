Partial Public Class SetUp
    Private sessionHub As Main
    Private nextBoxNumber As Integer = 1

    Private Sub ConfigureHubControls()
        SetUpToolTip.SetToolTip(btnComenzar, "Add this box and a copy of the current program to the hub. Start it from its row in the hub.")
    End Sub

    Private Sub OpenHubClick(sender As Object, e As EventArgs) Handles btnOpenHub.Click
        ShowHub()
    End Sub

    Private Function ShowHub() As Main
        If sessionHub Is Nothing OrElse sessionHub.IsDisposed Then sessionHub = New Main()
        sessionHub.Show()
        sessionHub.Activate()
        Return sessionHub
    End Function

    Private Function ReadSessionSeconds(text As String, label As String) As Integer
        Dim seconds As Integer
        If Not Integer.TryParse(text, seconds) OrElse seconds < 0 OrElse seconds > Integer.MaxValue \ 1000 Then
            Throw New ArgumentException(label & " must be a nonnegative number of seconds.")
        End If
        Return seconds
    End Function

    Private Sub AddConfiguredChamber()
        Try
            If vCC <= 0 Then Throw New ArgumentException("Add at least one component before adding a box.")
            If String.IsNullOrWhiteSpace(txtBoxName.Text) OrElse String.IsNullOrWhiteSpace(txtSubject.Text) OrElse String.IsNullOrWhiteSpace(txtSession.Text) Then
                Throw New ArgumentException("Enter a box name, subject, and session.")
            End If
            Dim comPort As String = txtCOM.Text.Trim().ToUpperInvariant()
            If Not System.Text.RegularExpressions.Regex.IsMatch(comPort, "^COM[1-9][0-9]*$") Then
                Throw New ArgumentException("Enter a COM port such as COM3.")
            End If
            For i As Integer = 1 To vCC
                Dim lagError As String = LagConfiguration.ValidationError(AC(i))
                If lagError <> "" Then Throw New ArgumentException("Component " & i & ": " & lagError)
                If AC(i).ComponentDuration <= 0 OrElse AC(i).ComponentDuration > Integer.MaxValue / 1000.0 OrElse AC(i).ComponentIteration <= 0 Then
                    Throw New ArgumentException("Component " & i & " needs a positive duration and at least one iteration.")
                End If
            Next
            Dim orderError As String = ManualComponentSequenceValidationError()
            If orderError <> "" Then Throw New ArgumentException(orderError)
            Dim settings As New ChamberConfiguration With {
                .BoxName = txtBoxName.Text.Trim(), .ComPort = comPort,
                .Subject = txtSubject.Text.Trim(), .SessionName = txtSession.Text.Trim(), .Weight = txtWeight.Text,
                .StartDelaySeconds = ReadSessionSeconds(txbStart.Text, "Start delay"),
                .PostSessionSeconds = ReadSessionSeconds(txbPostSession.Text, "Post-session duration"),
                .ICISeconds = If(chkICIEnabled.Checked, ReadSessionSeconds(txbICI.Text, "ICI duration"), 0),
                .RetractDuringICI = ICIRetractInputs(), .ICIStimulus = ICIStimulusType(),
                .RandomPresentation = CheckBox1.Checked, .ManualSequence = ManualComponentSequence(),
                .CommandMap = ArduinoCommands.Snapshot(), .Components = SnapshotComponents(), .ComponentCount = vCC,
                .SnapshotLines = ConfigurationSnapshotLines(False)
            }
            If ShowHub().AddChamber(settings) Then
                nextBoxNumber += 1
                txtBoxName.Text = "Box " & nextBoxNumber
                txtSubject.Clear()
                txtSession.Clear()
                txtWeight.Clear()
                txtCOM.Clear()
            End If
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Configure box")
        End Try
    End Sub

    Private Sub SetupClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If sessionHub Is Nothing OrElse sessionHub.IsDisposed Then Return
        sessionHub.Close()
        If Not sessionHub.IsDisposed Then e.Cancel = True
    End Sub
End Class
