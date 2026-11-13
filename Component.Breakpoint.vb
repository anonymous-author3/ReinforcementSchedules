Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Partial Public Class Component
    Private breakpointPanel As GroupBox
    Private breakpointProgression As ComboBox
    Private breakpointStart As NumericUpDown
    Private breakpointStep As NumericUpDown
    Private breakpointStepLabel As Label
    Private breakpointPreview As Label

    Private Function IsBreakpointScheduleMode() As Boolean
        Return cbbScheduleMode IsNot Nothing AndAlso cbbScheduleMode.Text = "Breakpoint Schedules"
    End Function

    Private Sub InitializeBreakpointEditor()
        If breakpointPanel IsNot Nothing Then Return
        breakpointPanel = New GroupBox With {.Name = "grpBreakpointSchedule", .Text = "Progressive ratio", .Location = New Point(18, 50), .Size = New Size(430, 168), .Visible = False}
        breakpointProgression = New ComboBox With {.Name = "cbbBreakpointProgression", .DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(145, 23), .Width = 150}
        breakpointProgression.Items.AddRange(New Object() {"Arithmetic", "Geometric"})
        breakpointStart = New NumericUpDown With {.Name = "numBreakpointStart", .Minimum = 1, .Maximum = Integer.MaxValue, .Value = 2, .Location = New Point(145, 53), .Width = 150, .ThousandsSeparator = True}
        breakpointStep = New NumericUpDown With {.Name = "numBreakpointStep", .Minimum = 1, .Maximum = Integer.MaxValue, .Value = 2, .Location = New Point(145, 83), .Width = 150, .ThousandsSeparator = True}
        breakpointStepLabel = New Label With {.Text = "Increment", .AutoSize = True, .Location = New Point(10, 86)}
        breakpointPreview = New Label With {.Name = "lblBreakpointPreview", .Location = New Point(10, 116), .Size = New Size(410, 43)}
        breakpointPanel.Controls.AddRange(New Control() {
            New Label With {.Text = "Progression", .AutoSize = True, .Location = New Point(10, 26)}, breakpointProgression,
            New Label With {.Text = "Initial responses", .AutoSize = True, .Location = New Point(10, 56)}, breakpointStart,
            breakpointStepLabel, breakpointStep, breakpointPreview})
        GroupBox1.Controls.Add(breakpointPanel)
        ComponentToolTip.SetToolTip(breakpointStart, "Response requirement before the first programmed reinforcer.")
        ComponentToolTip.SetToolTip(breakpointStep, "Arithmetic adds a whole-number increment. Geometric multiplies the previous integer requirement by a factor greater than 1 (up to 4 decimal places), then rounds UP to the next whole response. Example: 3 x 1.5 = 4.5 becomes 5. The ratio restarts when the component is presented again.")
        AddHandler breakpointProgression.SelectedIndexChanged, AddressOf BreakpointProgressionChanged
        AddHandler breakpointStart.ValueChanged, Sub() UpdateBreakpointPreview()
        AddHandler breakpointStep.ValueChanged, Sub() UpdateBreakpointPreview()
        breakpointProgression.SelectedIndex = 0
        If Not cbbScheduleMode.Items.Contains("Breakpoint Schedules") Then cbbScheduleMode.Items.Add("Breakpoint Schedules")
    End Sub

    Private Sub BreakpointProgressionChanged(sender As Object, e As EventArgs)
        Dim geometric = breakpointProgression.Text = "Geometric"
        breakpointStepLabel.Text = If(geometric, "Multiplier", "Increment")
        breakpointStep.DecimalPlaces = If(geometric, 4, 0)
        breakpointStep.Increment = If(geometric, 0.1D, 1D)
        breakpointStep.Minimum = If(geometric, 1.0001D, 1D)
        If Not geometric Then breakpointStep.Value = Math.Max(1D, Decimal.Ceiling(breakpointStep.Value))
        UpdateBreakpointPreview()
    End Sub

    Private Function BuildBreakpointDefinition() As BreakpointDefinition
        Return New BreakpointDefinition With {.Progression = breakpointProgression.Text, .StartRequirement = CInt(breakpointStart.Value), .StepValue = breakpointStep.Value}
    End Function

    Private Function BreakpointEditorSummary(definition As BreakpointDefinition) As String
        Return "Breakpoint " & definition.Progression & " / start " & definition.StartRequirement.ToString(CultureInfo.InvariantCulture) &
            If(definition.Progression = "Geometric", " / multiply by ", " / add ") & definition.StepValue.ToString(CultureInfo.InvariantCulture)
    End Function

    Private Sub UpdateBreakpointPreview()
        If breakpointPreview Is Nothing OrElse breakpointProgression.SelectedIndex < 0 Then Return
        Dim definition = BuildBreakpointDefinition()
        Dim validation = BreakpointConfiguration.ValidateDefinition(definition)
        If Not String.IsNullOrEmpty(validation) Then
            breakpointPreview.Text = validation
            Return
        End If
        Dim requirements As New List(Of String)
        Dim current = definition.StartRequirement
        For i = 0 To 5
            requirements.Add(current.ToString(CultureInfo.InvariantCulture))
            If i = 5 Then Exit For
            Try
                current = BreakpointConfiguration.NextRequirement(current, definition)
            Catch ex As OverflowException
                requirements.Add("limit reached")
                Exit For
            End Try
        Next
        breakpointPreview.Text = "Requirements: " & String.Join(" → ", requirements)
    End Sub

    Private Sub UpdateBreakpointModeState()
        If breakpointPanel Is Nothing Then Return
        breakpointPanel.Visible = IsBreakpointScheduleMode()
    End Sub

    Private Sub ResetBreakpointEditor()
        If breakpointPanel Is Nothing Then Return
        breakpointProgression.SelectedIndex = 0
        breakpointStart.Value = 2
        breakpointStep.Value = 2
        UpdateBreakpointPreview()
    End Sub
End Class
