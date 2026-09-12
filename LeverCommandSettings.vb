Public Class LeverCommandSettings
    Inherits Form

    Private ReadOnly letterBoxes(MAX_INPUTS - 1) As TextBox
    Private ReadOnly commandPreviews(MAX_INPUTS - 1) As Label

    Public Sub New()
        Text = "Lever commands"
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterParent
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        AutoScaleMode = AutoScaleMode.Font
        AutoSize = True
        AutoSizeMode = AutoSizeMode.GrowAndShrink

        Dim layout As New TableLayoutPanel With {
            .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .Dock = DockStyle.Fill, .Padding = New Padding(12), .ColumnCount = 3
        }
        Dim instructions As New Label With {
            .AutoSize = True, .MaximumSize = New Size(360, 0),
            .Text = "Choose the letter each Arduino input already uses. Uppercase inserts the lever; lowercase retracts it. These letters are included when you save the program.",
            .Margin = New Padding(3, 3, 3, 12)
        }
        layout.Controls.Add(instructions, 0, 0)
        layout.SetColumnSpan(instructions, 3)
        layout.Controls.Add(New Label With {.Text = "Input / lever", .AutoSize = True}, 0, 1)
        layout.Controls.Add(New Label With {.Text = "Letter", .AutoSize = True}, 1, 1)
        layout.Controls.Add(New Label With {.Text = "Insert / Retract", .AutoSize = True}, 2, 1)

        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            layout.Controls.Add(New Label With {
                .Text = "Input " & (inputIndex + 1), .AutoSize = True,
                .Anchor = AnchorStyles.Left, .Margin = New Padding(3, 6, 18, 6)
            }, 0, inputIndex + 2)
            Dim letterBox As New TextBox With {
                .Name = "txtLeverLetter" & (inputIndex + 1), .Width = 45,
                .MaxLength = 1, .CharacterCasing = CharacterCasing.Upper,
                .TextAlign = HorizontalAlignment.Center, .Tag = inputIndex,
                .Text = InputOutputCommand(inputIndex, True)
            }
            letterBoxes(inputIndex) = letterBox
            commandPreviews(inputIndex) = New Label With {.AutoSize = True, .Anchor = AnchorStyles.Left}
            layout.Controls.Add(letterBox, 1, inputIndex + 2)
            layout.Controls.Add(commandPreviews(inputIndex), 2, inputIndex + 2)
            AddHandler letterBox.TextChanged, AddressOf LetterChanged
            LetterChanged(letterBox, EventArgs.Empty)
        Next

        Dim buttons As New FlowLayoutPanel With {
            .AutoSize = True, .FlowDirection = FlowDirection.RightToLeft,
            .Dock = DockStyle.Fill, .Margin = New Padding(0, 12, 0, 0)
        }
        Dim cancel As New Button With {.Text = "Cancel", .AutoSize = True, .DialogResult = DialogResult.Cancel}
        Dim apply As New Button With {.Text = "Apply", .AutoSize = True}
        Dim defaults As New Button With {.Text = "Defaults", .AutoSize = True}
        AddHandler apply.Click, AddressOf ApplyCommands
        AddHandler defaults.Click, AddressOf RestoreDefaults
        buttons.Controls.AddRange(New Control() {cancel, apply, defaults})
        layout.Controls.Add(buttons, 0, MAX_INPUTS + 2)
        layout.SetColumnSpan(buttons, 3)
        Controls.Add(layout)
        AcceptButton = apply
        CancelButton = cancel
    End Sub

    Private Sub LetterChanged(sender As Object, e As EventArgs)
        Dim letterBox As TextBox = DirectCast(sender, TextBox)
        Dim inputIndex As Integer = CInt(letterBox.Tag)
        commandPreviews(inputIndex).Text = letterBox.Text & " / " & letterBox.Text.ToLowerInvariant()
    End Sub

    Private Sub RestoreDefaults(sender As Object, e As EventArgs)
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            letterBoxes(inputIndex).Text = DefaultLeverCommandLetters(inputIndex).ToString()
        Next
    End Sub

    Private Sub ApplyCommands(sender As Object, e As EventArgs)
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
