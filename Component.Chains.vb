Imports System.Drawing
Imports System.Windows.Forms

Partial Public Class Component
    Private currentChain As ClassicChainDefinition
    Private ReadOnly chainButton As New Button With {.Text = "Chain...", .Location = New Point(390, 72), .Size = New Size(72, 26)}
    Private ReadOnly chainLabel As New Label With {.Location = New Point(18, 102), .Size = New Size(440, 28), .AutoEllipsis = True}

    Private Sub InitializeChainEditor()
        GroupBox1.Controls.Add(chainButton)
        GroupBox1.Controls.Add(chainLabel)
        AddHandler chainButton.Click, AddressOf EditCurrentChain
        ComponentToolTip.SetToolTip(chainButton, "Add or edit linked classic schedules. The last link produces the programmed reinforcer.")
        UpdateChainEditorState()
    End Sub
    Private Sub EditCurrentChain(sender As Object, e As EventArgs)
        Dim initial = currentChain
        If initial Is Nothing Then
            Dim schedule = GetSelectedScheduleType()
            If Not ClassicChainConfiguration.Kinds.Contains(schedule) Then schedule = "Fixed Ratio"
            initial = New ClassicChainDefinition
            initial.Links.Add(New ClassicChainLink With {.ScheduleType = schedule, .Value = Math.Max(1, ToDoubleSafe(txbValueL1.Text, 1)), .VariableValueCount = CInt(variableValues.Value)})
        End If
        Using editor As New ChainScheduleEditor(initial)
            If editor.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim previous = currentChain
            currentChain = If(editor.Result Is Nothing, Nothing, editor.Result.Copy())
            If currentChain Is Nothing AndAlso editor.SimpleSchedule IsNot Nothing Then
                Dim first = editor.SimpleSchedule
                Select Case first.ScheduleType
                    Case "Fixed Ratio" : rdoFRL1.Checked = True
                    Case "Variable Ratio" : rdoVRL1.Checked = True
                    Case "Fixed Interval" : rdoFIL1.Checked = True
                    Case "Variable Interval" : rdoVIL1.Checked = True
                    Case Else : rdoFRL1.Checked = True
                End Select
                txbValueL1.Text = CStr(Math.Max(1, Math.Round(first.Value)))
                variableValues.Value = first.VariableValueCount
            End If
            If currentChain IsNot Nothing Then
                'Keep the ordinary reinforcer controls enabled for the chain's endpoint.
                rdoFRL1.Checked = True
                If cbbReinforcer1.Text = "" Then cbbReinforcer1.Text = "Feeder 1"
                If txbMagL1.Text = "" Then txbMagL1.Text = "1"
            End If
            UpdateScheduleModeState()
        End Using
    End Sub
    Private Sub UpdateChainEditorState()
        If chainButton Is Nothing OrElse rdoFRL1 Is Nothing Then Return
        Dim classic = Not IsTScheduleMode() AndAlso Not IsLagScheduleMode() AndAlso Not IsBreakpointScheduleMode()
        chainButton.Visible = classic
        Dim hasChain = classic AndAlso currentChain IsNot Nothing AndAlso currentChain.Links.Count > 0
        chainLabel.Visible = hasChain
        chainLabel.Text = If(hasChain, ClassicChainConfiguration.Summary(currentChain), "")
        ComponentToolTip.SetToolTip(chainLabel, chainLabel.Text)
        chainButton.Text = If(hasChain, "Chain (" & currentChain.Links.Count & ")", "Chain...")
        For Each radio As Control In {rdoExt1, rdoNAL1, rdoFRL1, rdoVRL1, rdoFIL1, rdoVIL1}
            radio.Visible = classic AndAlso Not hasChain
        Next
        txbValueL1.Visible = classic AndAlso Not hasChain
        Label13.Visible = classic AndAlso Not hasChain
        If hasChain Then
            variableValues.Visible = False
            variableValuesLabel.Visible = False
        End If
        'Classic chains replace the delay editor for newly created classic inputs.
        'Loaded legacy setup delays remain untouched by the loader/runtime.
        GroupBox2.Visible = Not classic
        If classic Then GroupBox1.Height = 362
    End Sub
    Private Function ConfirmChainWarnings() As Boolean
        Dim component As New ComponentBlueprint With {
            .InputCount = AddedInputs.Count, .ClassicChains = New ClassicChainDefinition(MAX_INPUTS - 1) {},
            .ScheduleType = New String(MAX_INPUTS - 1) {}, .FeedbackType = New String(MAX_INPUTS - 1) {},
            .DelayType = New String(MAX_INPUTS - 1) {}, .ComponentStimType = GetSelectedComponentStimType(),
            .HouselightOnOff = CheckedListContains(clbComponentStim, "Houselight"), .LagEnabled = IsLagScheduleMode()}
        For i As Integer = 0 To AddedInputs.Count - 1
            component.ClassicChains(i) = AddedInputs(i).Chain
            component.ScheduleType(i) = AddedInputs(i).ScheduleType
            component.FeedbackType(i) = AddedInputs(i).FeedbackType
            component.DelayType(i) = AddedInputs(i).DelayType
        Next
        Dim problem = ClassicChainConfiguration.ValidationError(component)
        If problem <> "" Then
            MessageBox.Show(Me, problem, "Invalid chain", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Dim warnings = ClassicChainConfiguration.ConflictWarnings(component)
        If warnings.Count = 0 Then Return True
        Return MessageBox.Show(Me, String.Join(Environment.NewLine, warnings) & Environment.NewLine & Environment.NewLine &
            "Shared outputs stay ON while any active signal requires them. This can change the intended intermittency or make a link signal indistinguishable. Continue with these shared outputs?",
            "Chain stimulus conflicts", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes
    End Function
End Class
