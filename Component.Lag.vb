Imports System.Drawing
Imports System.Numerics
Imports System.Windows.Forms

Partial Public Class Component
    Private componentEditorSubmitted As Boolean
    Private componentEditorSlot As Integer = -1

    Private Sub ComponentEditorClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If e.Cancel OrElse componentEditorSubmitted OrElse componentEditorSlot < 1 Then Return
        If CInt(vCC) = componentEditorSlot Then
            AC(componentEditorSlot) = New ComponentBlueprint
            vCC = CByte(componentEditorSlot - 1)
        End If
        componentEditorSlot = -1
    End Sub

    Private lagPanel As GroupBox
    Private lagInputs As CheckedListBox
    Private lagLength As NumericUpDown
    Private lagValue As NumericUpDown
    Private lagFailure As ComboBox
    Private lagTimeout As NumericUpDown
    Private lagTimeoutTone As CheckBox
    Private lagFeedbackInput As ComboBox
    Private lagPossibleSequences As Label
    Private lagMaxSequences As NumericUpDown
    Private lagMaxSequencesLabel As Label
    Private ReadOnly lagFeedbackPhysicalInputs As New List(Of Integer)
    Private lagFeedbackIndex As Integer = -1
    Private lagSwitchingFeedback As Boolean
    Private lagModeWasActive As Boolean
    Private ReadOnly lagFeedbackTypes(MAX_INPUTS - 1) As String
    Private ReadOnly lagFeedbackDurations(MAX_INPUTS - 1) As String

    Private Function IsLagScheduleMode() As Boolean
        Return cbbScheduleMode IsNot Nothing AndAlso cbbScheduleMode.Text = "Lag Schedules"
    End Function

    Private Sub InitializeLagEditor()
        If lagPanel IsNot Nothing Then Return
        lagMaxSequencesLabel = New Label With {.Text = "Max sequences / presentation (0 = off)", .AutoSize = True, .Location = New Point(18, 636), .Visible = False}
        lagMaxSequences = New NumericUpDown With {.Name = "numLagMaxSequences", .Minimum = 0, .Maximum = Integer.MaxValue, .Value = 0, .Location = New Point(255, 632), .Width = 100, .Visible = False}
        Controls.Add(lagMaxSequencesLabel)
        Controls.Add(lagMaxSequences)
        ComponentToolTip.SetToolTip(lagMaxSequences, "Completed sequences, both reinforced and not reinforced, in this component presentation. 0 disables this limit. A positive sequence limit replaces MaxRefs. The final sequence's delay or timeout finishes unless duration ends the component first.")
        lagPanel = New GroupBox With {.Name = "grpLagSchedule", .Text = "Lag sequence", .Location = New Point(18, 50), .Size = New Size(430, 168), .Visible = False}
        lagInputs = New CheckedListBox With {.Name = "clbLagInputs", .CheckOnClick = True, .Location = New Point(10, 34), .Size = New Size(140, 94)}
        For i As Integer = 0 To MAX_INPUTS - 1
            lagInputs.Items.Add("Input " & (i + 1) & " / Pin " & (i + 2), i < 2)
            lagFeedbackTypes(i) = "None"
            lagFeedbackDurations(i) = "1"
        Next
        lagLength = New NumericUpDown With {.Name = "numLagLength", .Minimum = 1, .Maximum = 10000, .Value = 4, .Location = New Point(324, 23), .Width = 90}
        lagValue = New NumericUpDown With {.Name = "numLagValue", .Minimum = 0, .Maximum = 100000, .Value = 5, .Location = New Point(324, 51), .Width = 90}
        lagFailure = New ComboBox With {.Name = "cbbLagFailure", .DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(167, 101), .Width = 151}
        lagFailure.Items.AddRange(New Object() {"Omit reinforcer", "Time Out (seconds)"})
        lagFailure.SelectedIndex = 0
        lagTimeout = New NumericUpDown With {.Name = "numLagTimeout", .DecimalPlaces = 3, .Increment = 0.1D, .Minimum = 0.001D, .Maximum = 2147483D, .Value = 5, .Location = New Point(324, 101), .Width = 90, .Enabled = False}
        lagTimeoutTone = New CheckBox With {.Name = "chkLagTimeoutTone", .Text = "Tono", .AutoSize = True, .Location = New Point(350, 78), .Enabled = False, .Checked = False}
        AddHandler lagFailure.SelectedIndexChanged,
            Sub()
                lagTimeout.Enabled = lagFailure.SelectedIndex = 1
                lagTimeoutTone.Enabled = lagFailure.SelectedIndex = 1
            End Sub
        ComponentToolTip.SetToolTip(lagTimeoutTone, "Keep the tone on throughout Time Out. It stops when Time Out ends or the component/session is interrupted.")
        lagPossibleSequences = New Label With {.Name = "lblLagPossibleSequences", .AutoSize = False, .Location = New Point(10, 128), .Size = New Size(410, 16)}
        lagPanel.Controls.AddRange(New Control() {
            New Label With {.Text = "Participating inputs", .AutoSize = True, .Location = New Point(10, 18)},
            lagInputs, lagPossibleSequences,
            New Label With {.Text = "Responses / sequence", .AutoSize = True, .Location = New Point(167, 26)},
            lagLength,
            New Label With {.Text = "Lag (previous sequences)", .AutoSize = True, .Location = New Point(167, 54)},
            lagValue,
            New Label With {.Text = "If sequence repeats:", .AutoSize = True, .Location = New Point(167, 81)},
            lagFailure, lagTimeout, lagTimeoutTone,
            New Label With {.Text = "Compare with the last N completed sequences. Lag 0 accepts all.", .AutoSize = True, .Location = New Point(10, 143)}
        })
        lagFeedbackInput = New ComboBox With {.Name = "cbbLagFeedbackInput", .DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(14, 20), .Width = 230, .Visible = False, .TabIndex = 0}
        AddHandler lagFeedbackInput.SelectedIndexChanged, AddressOf LagFeedbackInputChanged
        AddHandler lagInputs.ItemCheck, AddressOf LagInputsChanged
        AddHandler lagLength.ValueChanged, Sub() UpdateLagSequenceCount(lagInputs.CheckedItems.Count)
        GroupBox1.Controls.Add(lagPanel)
        grpStimL1.Controls.Add(lagFeedbackInput)
        lagFeedbackInput.BringToFront()
        UpdateLagSequenceCount(lagInputs.CheckedItems.Count)
        ComponentToolTip.SetToolTip(lagInputs, "Choose at least two physical inputs. Unchecked inputs are unavailable. Input numbers identify sequence responses.")
        ComponentToolTip.SetToolTip(lagFeedbackInput, "Select an input to edit its response feedback below. Each input keeps its own outputs and duration.")
        ComponentToolTip.SetToolTip(lagValue, "Compare the new sequence with up to this many previous complete sequences, including failed ones. Lag 0 accepts every sequence.")
        ComponentToolTip.SetToolTip(lagFailure, "A repeated sequence never delivers a reinforcer. Optionally start a Time Out; component end cancels it.")
    End Sub

    Private Sub UpdateLagModeState()
        If lagPanel Is Nothing Then Return
        Dim enabled As Boolean = IsLagScheduleMode()
        If lagModeWasActive AndAlso Not enabled Then SaveLagFeedback()
        lagPanel.Visible = enabled
        lagMaxSequences.Visible = enabled
        lagMaxSequencesLabel.Visible = enabled
        lagFeedbackInput.Visible = enabled
        txbInputName.Visible = Not enabled
        GroupBox1.Text = If(enabled, "Lag schedules", "Input")
        grpMagnitude.Text = If(enabled, "Reinforcer (per successful sequence)", "Reinforcer")
        GroupBox2.Text = If(enabled, "Delay (per successful sequence)", "Delay")
        btnAddInput.Text = If(enabled, "Apply Lag inputs", "Add Input")
        clbFeedbackStim.Location = If(enabled, New Point(14, 48), New Point(14, 24))
        clbFeedbackStim.Size = If(enabled, New Size(400, 64), New Size(240, 49))
        txbStimDurL1.Location = If(enabled, New Point(320, 20), New Point(80, 88))
        Label12.Location = If(enabled, New Point(260, 23), New Point(14, 91))
        ComponentToolTip.SetToolTip(txbStimDurL1, If(enabled,
            "Response feedback duration in seconds. 0 keeps the selected outputs on from that input's response until the sequence ends (or the component/session ends). Positive values produce a timed pulse, restarted by each response on this input.",
            "Response feedback duration in seconds. Decimal values such as 1.5 are allowed."))
        clbFeedbackStim.ColumnWidth = If(enabled, 125, 0)
        clbDelayStim.Location = If(enabled, New Point(14, 44), New Point(14, 50))
        clbDelayStim.Size = If(enabled, New Size(400, 64), New Size(240, 49))
        chkRetractL1.Location = If(enabled, New Point(235, 141), New Point(174, 75))
        clbDelayStim.ColumnWidth = If(enabled, 125, 0)
        grpTimeSchedule.Enabled = Not enabled
        txbCOD.Enabled = Not enabled
        If enabled Then
            chkFixedTime.Checked = False
            chkVariableTime.Checked = False
            txbCOD.Text = "0"
            If clbFeedbackStim.Items.Contains("Time Out") Then clbFeedbackStim.Items.Remove("Time Out")
            If Not lagModeWasActive Then
                RefreshLagFeedbackInputs(saveCurrent:=False)
            End If
        Else
            clbFeedbackStim.Enabled = True
            If Not clbFeedbackStim.Items.Contains("Time Out") Then clbFeedbackStim.Items.Add("Time Out")
            grpStimL1.Text = "Response feedback"
        End If
        UpdateFeedbackState()
        lagModeWasActive = enabled
    End Sub

    Private Sub SaveLagFeedback()
        If lagFeedbackIndex < 0 OrElse lagSwitchingFeedback Then Return
        lagFeedbackTypes(lagFeedbackIndex) = CheckedListText(clbFeedbackStim)
        lagFeedbackDurations(lagFeedbackIndex) = txbStimDurL1.Text
    End Sub

    Private Sub LagFeedbackInputChanged(sender As Object, e As EventArgs)
        If lagSwitchingFeedback Then Return
        SaveLagFeedback()
        lagFeedbackIndex = If(lagFeedbackInput.SelectedIndex < 0, -1, lagFeedbackPhysicalInputs(lagFeedbackInput.SelectedIndex))
        If lagFeedbackIndex < 0 Then
            clbFeedbackStim.Enabled = False
            txbStimDurL1.Enabled = False
            grpStimL1.Text = "Response feedback: select participating inputs"
            Return
        End If
        clbFeedbackStim.Enabled = True
        lagSwitchingFeedback = True
        Try
            Dim selected As String = lagFeedbackTypes(lagFeedbackIndex)
            For i As Integer = 0 To clbFeedbackStim.Items.Count - 1
                clbFeedbackStim.SetItemChecked(i, selected.Split(New String() {" + "}, StringSplitOptions.None).Contains(CStr(clbFeedbackStim.Items(i))))
            Next
            txbStimDurL1.Text = lagFeedbackDurations(lagFeedbackIndex)
            grpStimL1.Text = "Response feedback: Input " & (lagFeedbackIndex + 1)
        Finally
            lagSwitchingFeedback = False
        End Try
        UpdateFeedbackState()
    End Sub

    Private Sub LagInputsChanged(sender As Object, e As ItemCheckEventArgs)
        Dim count As Integer = lagInputs.CheckedItems.Count
        If lagInputs.GetItemChecked(e.Index) Then count -= 1
        If e.NewValue = CheckState.Checked Then count += 1
        UpdateLagSequenceCount(count)
        If IsLagScheduleMode() Then RefreshLagFeedbackInputs(e.Index, e.NewValue = CheckState.Checked)
    End Sub

    Private Sub RefreshLagFeedbackInputs(Optional changedIndex As Integer = -1, Optional isChecked As Boolean = False, Optional saveCurrent As Boolean = True)
        If saveCurrent Then SaveLagFeedback()
        Dim previous As Integer = lagFeedbackIndex
        lagSwitchingFeedback = True
        Try
            lagFeedbackInput.Items.Clear()
            lagFeedbackPhysicalInputs.Clear()
            For i As Integer = 0 To MAX_INPUTS - 1
                If If(i = changedIndex, isChecked, lagInputs.GetItemChecked(i)) Then
                    lagFeedbackPhysicalInputs.Add(i)
                    lagFeedbackInput.Items.Add("Input " & (i + 1) & " / Pin " & (i + 2))
                End If
            Next
            lagFeedbackInput.Enabled = lagFeedbackPhysicalInputs.Count > 0
            If lagFeedbackInput.Enabled Then
                Dim selected As Integer = lagFeedbackPhysicalInputs.IndexOf(previous)
                lagFeedbackInput.SelectedIndex = If(selected < 0, 0, selected)
            End If
        Finally
            lagSwitchingFeedback = False
        End Try
        lagFeedbackIndex = -1
        LagFeedbackInputChanged(lagFeedbackInput, EventArgs.Empty)
    End Sub

    Private Sub UpdateLagSequenceCount(inputCount As Integer)
        If lagPossibleSequences Is Nothing Then Return
        Dim length As Integer = CInt(lagLength.Value)
        'BigInteger avoids overflow even at all six inputs and 10,000 responses.
        Dim exact As String = BigInteger.Pow(New BigInteger(inputCount), length).ToString(Globalization.CultureInfo.InvariantCulture)
        Dim countText As String = exact
        If exact.Length > 18 Then countText = "~" & exact.Substring(0, 1) & "." & exact.Substring(1, 5) & " x 10^" & (exact.Length - 1)
        lagPossibleSequences.Text = "Possible sequences: " & countText & " (" & inputCount & "^" & length & ")"
        ComponentToolTip.SetToolTip(lagPossibleSequences, "Participating inputs raised to responses per sequence. Repetition is allowed and order matters. Large counts are shown approximately; the formula is exact.")
    End Sub

    Private Function LagInputError(message As String) As Boolean
        MessageBox.Show(Me, message, "Lag schedule", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Return False
    End Function

    Private Shared Function ValidateLagReinforcerText(magnitudeText As String, deliveryText As String, pelletText As String, reinforcer As String) As String
        Dim magnitude As Integer
        Dim delivery As Integer
        Dim pellet As Integer
        If Not Integer.TryParse(magnitudeText, magnitude) OrElse magnitude <= 0 Then Return "Enter a whole-number reinforcer magnitude greater than 0."
        If Not Integer.TryParse(deliveryText, delivery) OrElse delivery < 0 OrElse delivery > 100 Then Return "Enter a whole-number delivery probability between 0 and 100."
        If reinforcer = "Random" AndAlso (Not Integer.TryParse(pelletText, pellet) OrElse pellet < 0 OrElse pellet > 100) Then Return "Enter a whole-number pellet probability between 0 and 100."
        Return ""
    End Function

    Private Function ApplyLagInputs() As Boolean
        If lagInputs.CheckedItems.Count < 2 Then Return LagInputError("Select at least two inputs for Lag sequences.")
        SaveLagFeedback()
        Dim rawError As String = ValidateLagReinforcerText(txbMagL1.Text, txbDeliveryProbability1.Text, txbPelletProbability1.Text, cbbReinforcer1.Text)
        If rawError <> "" Then Return LagInputError(rawError)
        Dim common As ComponentInputConfiguration = BuildInputFromEditor(1)
        If common.Reinforcer = "Random" Then
            Dim selectionError = ReinforcerConfiguration.ValidateSelection(common.ReinforcerSelection)
            If Not String.IsNullOrEmpty(selectionError) Then Return LagInputError(selectionError)
        End If
        If common.Reinforcer = "" OrElse common.Reinforcer = "N/A" OrElse common.Magnitude <= 0 Then Return LagInputError("Select a reinforcer and enter a magnitude greater than 0.")
        If common.DeliveryP < 0 OrElse common.DeliveryP > 100 OrElse (common.Reinforcer = "Random" AndAlso (common.PelletP < 0 OrElse common.PelletP > 100)) Then Return LagInputError("Reinforcer probabilities must be between 0 and 100.")
        Dim delay As Double = ToDoubleSafe(txbDelayDurL1.Text, -1)
        Dim signal As Double = ToDoubleSafe(txbDelaySignalDurationL1.Text, -1)
        If HasDelaySelection() AndAlso (Double.IsNaN(delay) OrElse Double.IsInfinity(delay) OrElse delay < 0 OrElse delay > 2147483) Then Return LagInputError("Enter a valid, nonnegative delay in seconds (maximum 2147483).")
        If HasDelaySelection() AndAlso Not chkUnsignaledDelay.Checked AndAlso (Double.IsNaN(signal) OrElse Double.IsInfinity(signal) OrElse signal < 0 OrElse signal > 2147483) Then Return LagInputError("Enter a valid, nonnegative delay signal duration.")
        For i As Integer = 0 To MAX_INPUTS - 1
            If lagInputs.GetItemChecked(i) Then
                Dim duration As Double = ToDoubleSafe(lagFeedbackDurations(i), -1)
                If lagFeedbackTypes(i) <> "None" AndAlso (Double.IsNaN(duration) OrElse Double.IsInfinity(duration) OrElse duration < 0 OrElse duration > 2147483) Then Return LagInputError("Enter valid response feedback seconds for Input " & (i + 1) & ".")
            End If
        Next
        AddedInputs.Clear()
        deletedInputSlots.Clear()
        lstInputs.Items.Clear()
        lstInputs.Items.Add("Lag " & lagValue.Value & " / " & lagLength.Value & " responses")
        lstInputs.Items.Add("Failure: " & If(lagFailure.SelectedIndex = 1, "TO " & lagTimeout.Value & " s" & If(lagTimeoutTone.Checked, " + Tono", ""), "omit reinforcer"))
        For i As Integer = 0 To MAX_INPUTS - 1
            Dim inputInfo As ComponentInputConfiguration = BuildInputFromEditor(i + 1)
            inputInfo.Name = "Arduino Pin " & (i + 2)
            inputInfo.ScheduleValue = 0
            If lagInputs.GetItemChecked(i) Then
                inputInfo.ScheduleType = "Lag"
                inputInfo.FeedbackType = lagFeedbackTypes(i)
                inputInfo.FeedbackDuration = If(inputInfo.FeedbackType = "None", 0, ToDoubleSafe(lagFeedbackDurations(i), 0))
            Else
                inputInfo = BuildDefaultInput(i + 1)
                inputInfo.Name = "Arduino Pin " & (i + 2)
                inputInfo.ScheduleType = "N/A"
            End If
            AddedInputs.Add(inputInfo)
            lstInputs.Items.Add("Input " & (i + 1) & ": " & If(inputInfo.ScheduleType = "Lag", "Lag / " & inputInfo.FeedbackType & " (" & If(inputInfo.FeedbackType <> "None" AndAlso inputInfo.FeedbackDuration = 0, "until sequence ends", inputInfo.FeedbackDuration & " s") & ")", "N/A"))
        Next
        cbbScheduleMode.Enabled = False
        UpdateDeleteInputButton()
        Return True
    End Function
End Class