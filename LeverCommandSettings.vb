Public Class LeverCommandSettings

    Private ReadOnly letterBoxes(MAX_INPUTS - 1) As TextBox
    Private ReadOnly commandPreviews(MAX_INPUTS - 1) As Label

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub LoadCommands(sender As Object, e As EventArgs) Handles MyBase.Load
        If DesignMode OrElse System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then Return
        Dim boxes As TextBox() = {txtLeverLetter1, txtLeverLetter2, txtLeverLetter3, txtLeverLetter4, txtLeverLetter5, txtLeverLetter6}
        Dim previews As Label() = {lblCommandPreview1, lblCommandPreview2, lblCommandPreview3, lblCommandPreview4, lblCommandPreview5, lblCommandPreview6}
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            letterBoxes(inputIndex) = boxes(inputIndex)
            commandPreviews(inputIndex) = previews(inputIndex)
            letterBoxes(inputIndex).Text = InputOutputCommand(inputIndex, True)
            LetterChanged(letterBoxes(inputIndex), EventArgs.Empty)
        Next
    End Sub

    Private Sub LetterChanged(sender As Object, e As EventArgs) Handles txtLeverLetter1.TextChanged, txtLeverLetter2.TextChanged, txtLeverLetter3.TextChanged, txtLeverLetter4.TextChanged, txtLeverLetter5.TextChanged, txtLeverLetter6.TextChanged
        Dim letterBox As TextBox = DirectCast(sender, TextBox)
        Dim inputIndex As Integer = CInt(letterBox.Tag)
        If commandPreviews(inputIndex) Is Nothing Then Return
        commandPreviews(inputIndex).Text = letterBox.Text & " / " & letterBox.Text.ToLowerInvariant()
    End Sub

    Private Sub RestoreDefaults(sender As Object, e As EventArgs) Handles btnDefaults.Click
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            letterBoxes(inputIndex).Text = DefaultLeverCommandLetters(inputIndex).ToString()
        Next
    End Sub

    Private Sub ApplyCommands(sender As Object, e As EventArgs) Handles btnApply.Click
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            If letterBoxes(inputIndex).Text.Length <> 1 Then
                MessageBox.Show(Me, "Enter one letter for input " & (inputIndex + 1) & ".", Text)
                letterBoxes(inputIndex).Focus()
                Exit Sub
            End If
        Next
        Dim letters As String = String.Concat(letterBoxes.Select(Function(box) box.Text))
        Dim validationError As String = LeverCommandValidationError(letters)
        If validationError <> "" Then
            MessageBox.Show(Me, validationError, Text)
            Exit Sub
        End If
        ConfigureLeverCommands(letters)
        TestsPerformed = False
        DialogResult = DialogResult.OK
        Close()
    End Sub
End Class
