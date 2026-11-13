Imports System.Drawing
Imports System.Windows.Forms

Partial Public Class Component
    Private deleteInputButton As Button
    Private lastInputSuggestion As String = ""
    'Only explicitly deleted slots are reusable. A deliberately added N/A input is not a hole.
    Private ReadOnly deletedInputSlots As New HashSet(Of Integer)

    Private Sub InitializeInputDeletion()
        If deleteInputButton IsNot Nothing Then Return
        deleteInputButton = New Button With {.Name = "btnDeleteInput", .Text = "Delete Input", .Size = btnAddInput.Size, .Location = New Point(btnAddInput.Right + 10, btnAddInput.Top), .Enabled = False, .TabIndex = btnAddInput.TabIndex + 1}
        btnSubmit.Location = New Point(btnAddInput.Left, btnClose.Top)
        btnSubmit.TabIndex = deleteInputButton.TabIndex + 1
        btnClose.TabIndex = btnSubmit.TabIndex + 1
        Controls.Add(deleteInputButton)
        AddHandler deleteInputButton.Click, AddressOf DeleteSelectedInput
        AddHandler lstInputs.SelectedIndexChanged, AddressOf InputSelectionChanged
        ComponentToolTip.SetToolTip(deleteInputButton, "Delete the selected input configuration. Other physical input numbers do not change. Add Input only restores a deleted slot when that row is selected. Lag inputs are deselected instead of renumbered.")
        ComponentToolTip.SetToolTip(btnAddInput, "Add this configuration to the next physical input number. To restore a deleted input instead, select its deleted row first. At six input positions, select a deleted row to reuse it.")
    End Sub

    Private Function NextInputSlot() As Integer
        Dim selected = lstInputs.SelectedIndex
        If Not IsLagScheduleMode() AndAlso selected >= 0 AndAlso selected < AddedInputs.Count AndAlso deletedInputSlots.Contains(selected) Then Return selected
        Return AddedInputs.Count
    End Function

    Private Function AllInputsDeleted() As Boolean
        Return AddedInputs.Count > 0 AndAlso deletedInputSlots.Count = AddedInputs.Count
    End Function

    Private Function SuggestedInputName(slot As Integer) As String
        Return If(slot < MAX_INPUTS, "Arduino Pin " & (slot + 2), "Select a deleted input")
    End Function

    Private Sub SetDefaultInputName()
        lastInputSuggestion = SuggestedInputName(NextInputSlot())
        txbInputName.Text = lastInputSuggestion
    End Sub

    Private Sub InputSelectionChanged(sender As Object, e As EventArgs)
        UpdateDeleteInputButton()
        UpdateAddInputTarget()
    End Sub

    Private Sub UpdateAddInputTarget()
        If IsLagScheduleMode() Then
            btnAddInput.Enabled = True
            GroupBox1.Enabled = True
            Return
        End If
        Dim target = NextInputSlot()
        Dim canAdd = target < MAX_INPUTS
        btnAddInput.Enabled = canAdd
        txbInputName.Enabled = canAdd
        'An empty component may still switch to Lag even when all six tombstones are shown.
        GroupBox1.Enabled = canAdd OrElse AllInputsDeleted()
        If txbInputName.Text = lastInputSuggestion Then
            lastInputSuggestion = SuggestedInputName(target)
            txbInputName.Text = lastInputSuggestion
        End If
    End Sub

    Private Sub ClearDeletedInputsWhenEnteringLag()
        If IsLagScheduleMode() AndAlso AllInputsDeleted() Then
            AddedInputs.Clear()
            deletedInputSlots.Clear()
            RefreshAddedInputRows()
        End If
    End Sub

    Private Function SelectedInputSlot() As Integer
        Dim index = lstInputs.SelectedIndex
        If IsLagScheduleMode() Then index -= 2 'The first two rows describe the Lag schedule, not inputs.
        If index < 0 OrElse index >= AddedInputs.Count Then Return -1
        If IsLagScheduleMode() Then
            If AddedInputs(index).ScheduleType <> "Lag" Then Return -1
        ElseIf deletedInputSlots.Contains(index) Then
            Return -1
        End If
        Return index
    End Function

    Private Sub UpdateDeleteInputButton()
        If deleteInputButton IsNot Nothing Then deleteInputButton.Enabled = SelectedInputSlot() >= 0
    End Sub

    Private Sub DeleteSelectedInput(sender As Object, e As EventArgs)
        Dim index = SelectedInputSlot()
        If index < 0 Then Return
        Dim message = "Delete Input " & (index + 1) & " (" & AddedInputs(index).Name & ") and its schedule, feedback and delay settings?" & Environment.NewLine & Environment.NewLine &
            "Other physical input numbers and configurations will not change."
        If MessageBox.Show(Me, message, "Delete input", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
        DeleteInputAt(index)
    End Sub

    Private Function EmptyDeletedInput(index As Integer) As ComponentInputConfiguration
        Dim item = BuildDefaultInput(index + 1)
        item.Name = "Arduino Pin " & (index + 2)
        item.ScheduleType = "N/A"
        item.Magnitude = 0
        item.DeliveryP = 0
        item.DelayDuration = 0
        item.DelaySignalDuration = 0
        Return item
    End Function

    Private Sub RefreshAddedInputRows(Optional selected As Integer = -1)
        lstInputs.BeginUpdate()
        Try
            lstInputs.Items.Clear()
            If IsLagScheduleMode() AndAlso AddedInputs.Count > 0 Then
                lstInputs.Items.Add("Lag " & lagValue.Value & " / " & lagLength.Value & " responses")
                lstInputs.Items.Add("Failure: " & If(lagFailure.SelectedIndex = 1, "TO " & lagTimeout.Value & " s" & If(lagTimeoutTone.Checked, " + Tono", ""), "omit reinforcer"))
                For i As Integer = 0 To AddedInputs.Count - 1
                    Dim item = AddedInputs(i)
                    lstInputs.Items.Add("Input " & (i + 1) & ": " & If(item.ScheduleType = "Lag", "Lag / " & item.FeedbackType & " (" & If(item.FeedbackType <> "None" AndAlso item.FeedbackDuration = 0, "until sequence ends", item.FeedbackDuration & " s") & ")", "N/A"))
                Next
                If selected >= 0 AndAlso selected < AddedInputs.Count Then lstInputs.SelectedIndex = selected + 2
            Else
                For i As Integer = 0 To AddedInputs.Count - 1
                    lstInputs.Items.Add("Input " & (i + 1) & ": " & If(deletedInputSlots.Contains(i), "deleted (N/A); select this row to restore with Add Input", InputSummary(AddedInputs(i))))
                Next
                If selected >= 0 AndAlso selected < AddedInputs.Count Then lstInputs.SelectedIndex = selected
            End If
        Finally
            lstInputs.EndUpdate()
        End Try
        UpdateDeleteInputButton()
    End Sub

    Private Function DeleteInputAt(index As Integer) As Boolean
        If index < 0 OrElse index >= AddedInputs.Count Then Return False
        If IsLagScheduleMode() Then
            If AddedInputs(index).ScheduleType <> "Lag" Then Return False
            'Unchecking preserves every other physical input and feedback assignment.
            lagInputs.SetItemChecked(index, False)
            lagFeedbackTypes(index) = "None"
            lagFeedbackDurations(index) = "1"
            AddedInputs(index) = EmptyDeletedInput(index)
            If Not AddedInputs.Any(Function(item) item.ScheduleType = "Lag") Then
                AddedInputs.Clear()
                deletedInputSlots.Clear()
                cbbScheduleMode.Enabled = True
            End If
            RefreshAddedInputRows()
            Return True
        End If
        If deletedInputSlots.Contains(index) Then Return False
        AddedInputs(index) = EmptyDeletedInput(index)
        deletedInputSlots.Add(index)
        'Keep tombstones, including the tail: no physical number is reused without selecting it.
        cbbScheduleMode.Enabled = True
        If AllInputsDeleted() AndAlso Not cbbScheduleMode.Items.Contains("Lag Schedules") Then cbbScheduleMode.Items.Add("Lag Schedules")
        RefreshAddedInputRows() 'No automatic selection of the slot that was just deleted.
        UpdateAddInputTarget()
        Return True
    End Function
End Class
