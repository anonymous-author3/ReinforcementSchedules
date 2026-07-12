Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar

Public Class Component

    Private ComponentStimType As String = ""
    Private HouselightOnOff As Boolean = False
    Private ReadOnly AddedInputs As New List(Of ComponentInputConfiguration)
    Private ReadOnly ComponentToolTip As New ToolTip()

    Private Class ComponentInputConfiguration
        Public Property Name As String
        Public Property ScheduleType As String
        Public Property ScheduleValue As Integer
        Public Property Reinforcer As String
        Public Property Magnitude As Integer
        Public Property DeliveryP As Integer
        Public Property PelletP As Integer
        Public Property TDurationD As Double
        Public Property TDurationDelta As Double
        Public Property TProbabilityD As Integer
        Public Property TProbabilityDelta As Integer
        Public Property TCycles As Integer
        Public Property TStartPeriod As String
        Public Property TInterPeriod As Double
        Public Property TStimD As String
        Public Property TStimDelta As String
        Public Property FeedbackDuration As Double
        Public Property FeedbackType As String
        Public Property DelayDuration As Double
        Public Property DelayType As String
        Public Property DelayRetract As Boolean
        Public Property DelaySignalDuration As Double
    End Class

    ' ---------- Helpers (sin casts implícitos) ----------
    Private Function Clean(s As String) As String
        Return If(s, "") _
            .Replace("""", "") _
            .Replace(vbCr, " ") _
            .Replace(vbLf, " ") _
            .Trim()
    End Function

    Private Function ToIntSafe(txt As String, Optional defaultValue As Integer = 0) As Integer
        Dim s As String = Clean(txt)
        Dim n As Integer
        If Integer.TryParse(s, n) Then Return n
        Return defaultValue
    End Function

    Private Function ToDoubleSafe(txt As String, Optional defaultValue As Double = 0) As Double
        Dim s As String = Clean(txt)
        Dim n As Double
        If Double.TryParse(s, n) Then Return n
        If Double.TryParse(s.Replace("."c, ","c), n) Then Return n
        If Double.TryParse(s.Replace(","c, "."c), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, n) Then Return n
        Return defaultValue
    End Function

    Private Function ToByteSafe(txt As String, Optional defaultValue As Byte = 0) As Byte
        Dim s As String = Clean(txt)
        Dim n As Integer
        If Integer.TryParse(s, n) Then
            If n < 0 Then n = 0
            If n > 255 Then n = 255
            Return CByte(n)
        End If
        Return defaultValue
    End Function

    Private Function ToStrSafe(txt As String, Optional defaultValue As String = "") As String
        Dim s As String = Clean(txt)
        If s = "" Then Return defaultValue
        Return s
    End Function

    Private Function GetSelectedScheduleType() As String
        If cbbScheduleMode IsNot Nothing AndAlso cbbScheduleMode.Text = "T Schedules" Then Return "T Schedule"
        If rdoNAL1.Checked Then Return "N/A"
        If rdoFRL1.Checked Then Return "Fixed Ratio"
        If rdoVRL1.Checked Then Return "Variable Ratio"
        If rdoFIL1.Checked Then Return "Fixed Interval"
        If rdoVIL1.Checked Then Return "Variable Interval"
        Return "Extinction"
    End Function

    Private Function GetSelectedFeedbackType() As String
        Return CheckedListText(clbFeedbackStim)
    End Function

    Private Function GetSelectedDelayType() As String
        If chkUnsignaledDelay.Checked Then Return "Unsignaled"

        Return CheckedListText(clbDelayStim)
    End Function

    Private Function CheckedListText(list As CheckedListBox) As String
        Dim selected As New List(Of String)
        For Each item As Object In list.CheckedItems
            selected.Add(CStr(item))
        Next
        If selected.Count = 0 Then Return "None"
        Return String.Join(" + ", selected)
    End Function

    Private Function CheckedListContains(list As CheckedListBox, itemText As String) As Boolean
        If list Is Nothing Then Return False
        For Each item As Object In list.CheckedItems
            If CStr(item) = itemText Then Return True
        Next
        Return False
    End Function

    Private Function CheckedListHasAny(list As CheckedListBox) As Boolean
        Return list IsNot Nothing AndAlso list.CheckedItems.Count > 0
    End Function

    Private Function CheckedListHasLight(list As CheckedListBox) As Boolean
        If list Is Nothing Then Return False
        For Each item As Object In list.CheckedItems
            If CStr(item).StartsWith("Light ") Then Return True
        Next
        Return False
    End Function

    Private Sub ClearCheckedList(list As CheckedListBox)
        If list Is Nothing Then Exit Sub
        For i As Integer = 0 To list.Items.Count - 1
            list.SetItemChecked(i, False)
        Next
    End Sub

    Private Function IsTScheduleMode() As Boolean
        Return cbbScheduleMode IsNot Nothing AndAlso cbbScheduleMode.Text = "T Schedules"
    End Function

    Private Function TAbsoluteDuration(td As Double, tDelta As Double, cycles As Integer, gap As Double) As Double
        If cycles <= 0 Then Return 0
        Dim periodCount As Integer = cycles * 2
        Dim gapCount As Integer = Math.Max(0, periodCount - 1)
        Return (cycles * (td + tDelta)) + (gapCount * Math.Max(0, gap))
    End Function

    Private Function TAbsoluteDuration(inputInfo As ComponentInputConfiguration) As Double
        Return TAbsoluteDuration(inputInfo.TDurationD, inputInfo.TDurationDelta, inputInfo.TCycles, inputInfo.TInterPeriod)
    End Function

    Private Function GetSelectedComponentStimType() As String
        Return CheckedListText(clbComponentStim)
    End Function

    Private Function HasFeedbackSelection() As Boolean
        Return CheckedListHasAny(clbFeedbackStim)
    End Function

    Private Function HasDelaySelection() As Boolean
        Return chkUnsignaledDelay.Checked OrElse CheckedListHasAny(clbDelayStim)
    End Function

    Private Function HasComponentLightSelection() As Boolean
        Return CheckedListHasLight(clbComponentStim)
    End Function

    Private Function HasComponentToneSelection() As Boolean
        Return CheckedListContains(clbComponentStim, "Tone")
    End Function

    Private Sub UpdateFeedbackState()
        Dim enabled As Boolean = HasFeedbackSelection()
        txbStimDurL1.Enabled = enabled
        txbStimDurL1.Visible = enabled
        Label12.Visible = enabled
    End Sub

    Private Sub UpdateDelayState()
        Dim enabled As Boolean = HasDelaySelection()
        Dim signaled As Boolean = enabled AndAlso chkUnsignaledDelay.Checked = False

        clbDelayStim.Enabled = chkUnsignaledDelay.Checked = False
        rdoLightDelay1L1.Enabled = chkUnsignaledDelay.Checked = False
        rdoLightDelay2L1.Enabled = chkUnsignaledDelay.Checked = False
        rdoLightDelay3L1.Enabled = chkUnsignaledDelay.Checked = False
        rdoLightDelay4L1.Enabled = chkUnsignaledDelay.Checked = False
        rdoToneDelayL1.Enabled = chkUnsignaledDelay.Checked = False
        rdoHouselightDelayL1.Enabled = chkUnsignaledDelay.Checked = False

        If chkUnsignaledDelay.Checked Then
            ClearCheckedList(clbDelayStim)
            rdoLightDelay1L1.Checked = False
            rdoLightDelay2L1.Checked = False
            rdoLightDelay3L1.Checked = False
            rdoLightDelay4L1.Checked = False
            rdoToneDelayL1.Checked = False
            rdoHouselightDelayL1.Checked = False
        End If

        txbDelayDurL1.Enabled = enabled
        txbDelaySignalDurationL1.Enabled = signaled
        txbDelayDurL1.Visible = enabled
        txbDelaySignalDurationL1.Visible = signaled
        Label9.Visible = enabled
        Label16.Visible = signaled
        chkRetractL1.Visible = enabled
        If enabled = False Then
            chkRetractL1.Checked = False
        End If
        If signaled = False Then txbDelaySignalDurationL1.Text = "0"
    End Sub

    Private Sub UpdateComponentIntermittencyState()
        txbLightIntermittency.Enabled = HasComponentLightSelection()
        txbToneIntermittency.Enabled = HasComponentToneSelection()
        HouselightOnOff = CheckedListContains(clbComponentStim, "Houselight")
        chkHouselightOnOff.Checked = HouselightOnOff

        If HasComponentLightSelection() = False Then txbLightIntermittency.Text = "0"
        If HasComponentToneSelection() = False Then txbToneIntermittency.Text = "0"
    End Sub

    Private Sub TimeSchedule_CheckedChanged(sender As Object, e As EventArgs) Handles chkFixedTime.CheckedChanged, chkVariableTime.CheckedChanged
        If sender Is chkFixedTime AndAlso chkFixedTime.Checked Then chkVariableTime.Checked = False
        If sender Is chkVariableTime AndAlso chkVariableTime.Checked Then chkFixedTime.Checked = False
        UpdateTimeScheduleState()
    End Sub

    Private Sub TimeReinforcer_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbbTimeReinforcer.SelectedIndexChanged
        UpdateTimeReinforcerState()
    End Sub

    Private Sub UpdateTimeScheduleState()
        Dim enabled As Boolean = chkFixedTime.Checked OrElse chkVariableTime.Checked
        txbTimeScheduleValue.Enabled = enabled
        cbbTimeReinforcer.Enabled = enabled
        txbTimeMagnitude.Enabled = enabled
        txbTimeDeliveryProbability.Enabled = enabled
        UpdateTimeReinforcerState()
    End Sub

    Private Sub UpdateTimeReinforcerState()
        If txbTimePelletProbability Is Nothing Then Exit Sub
        txbTimePelletProbability.Enabled = (chkFixedTime.Checked OrElse chkVariableTime.Checked) AndAlso cbbTimeReinforcer.Text = "Random"
    End Sub

    Private Function SelectedTimeScheduleType() As String
        If chkFixedTime IsNot Nothing AndAlso chkFixedTime.Checked Then Return "Fixed Time"
        If chkVariableTime IsNot Nothing AndAlso chkVariableTime.Checked Then Return "Variable Time"
        Return "None"
    End Function

    Private Function BuildInputFromEditor(inputNumber As Integer) As ComponentInputConfiguration
        Dim inputName As String = ToStrSafe(txbInputName.Text, "Input " & inputNumber)
        Dim scheduleType As String = GetSelectedScheduleType()
        Dim noInputProgram As Boolean = scheduleType = "Extinction" OrElse scheduleType = "N/A"
        Dim isTSchedule As Boolean = scheduleType = "T Schedule"

        Return New ComponentInputConfiguration With {
            .Name = inputName,
            .ScheduleType = scheduleType,
            .ScheduleValue = If(noInputProgram OrElse isTSchedule, 0, ToIntSafe(txbValueL1.Text, 0)),
            .Reinforcer = If(noInputProgram, "N/A", ToStrSafe(cbbReinforcer1.Text, "Pellet")),
            .Magnitude = If(noInputProgram, 0, ToIntSafe(txbMagL1.Text, 0)),
            .DeliveryP = If(noInputProgram, 0, ToIntSafe(txbDeliveryProbability1.Text, 100)),
            .PelletP = If(noInputProgram, 0, ToIntSafe(txbPelletProbability1.Text, 0)),
            .TDurationD = If(isTSchedule, ToDoubleSafe(txbTD.Text, 0), 0),
            .TDurationDelta = If(isTSchedule, ToDoubleSafe(txbTDelta.Text, 0), 0),
            .TProbabilityD = If(isTSchedule, ToIntSafe(txbTPD.Text, 100), 0),
            .TProbabilityDelta = If(isTSchedule, ToIntSafe(txbTPDelta.Text, 0), 0),
            .TCycles = If(isTSchedule, ToIntSafe(txbTCycles.Text, 1), 0),
            .TStartPeriod = If(isTSchedule, ToStrSafe(cbbTStartPeriod.Text, "TD"), ""),
            .TInterPeriod = If(isTSchedule, ToDoubleSafe(txbTInterPeriod.Text, 0), 0),
            .TStimD = If(isTSchedule, CheckedListText(clbTDStim), "None"),
            .TStimDelta = If(isTSchedule, CheckedListText(clbTDeltaStim), "None"),
            .FeedbackDuration = If(HasFeedbackSelection(), ToDoubleSafe(txbStimDurL1.Text, 1), 0),
            .FeedbackType = GetSelectedFeedbackType(),
            .DelayDuration = If(HasDelaySelection(), ToDoubleSafe(txbDelayDurL1.Text, 1), 0),
            .DelayType = GetSelectedDelayType(),
            .DelayRetract = chkRetractL1.Checked,
            .DelaySignalDuration = If(chkUnsignaledDelay.Checked, 0, ToDoubleSafe(txbDelaySignalDurationL1.Text, 0))
        }
    End Function

    Private Function BuildDefaultInput(inputNumber As Integer) As ComponentInputConfiguration
        Return New ComponentInputConfiguration With {
            .Name = "Input " & inputNumber,
            .ScheduleType = "Extinction",
            .ScheduleValue = 0,
            .Reinforcer = "N/A",
            .Magnitude = 1,
            .DeliveryP = 100,
            .PelletP = 0,
            .TDurationD = 0,
            .TDurationDelta = 0,
            .TProbabilityD = 100,
            .TProbabilityDelta = 0,
            .TCycles = 0,
            .TStartPeriod = "TD",
            .TInterPeriod = 0,
            .TStimD = "None",
            .TStimDelta = "None",
            .FeedbackDuration = 0,
            .FeedbackType = "None",
            .DelayDuration = 1,
            .DelayType = "None",
            .DelayRetract = False,
            .DelaySignalDuration = 1
        }
    End Function

    Private Function InputSummary(inputInfo As ComponentInputConfiguration) As String
        If inputInfo.ScheduleType = "Extinction" Then
            Return inputInfo.Name & " - Extinction / N/A"
        End If
        If inputInfo.ScheduleType = "N/A" Then
            Return inputInfo.Name & " - N/A"
        End If
        If inputInfo.ScheduleType = "T Schedule" Then
            Return inputInfo.Name & " - T Schedule TD " & inputInfo.TDurationD &
                " s / TΔ " & inputInfo.TDurationDelta & " s / P " &
                inputInfo.TProbabilityD & "%|" & inputInfo.TProbabilityDelta & "%"
        End If

        Return inputInfo.Name & " - " & inputInfo.ScheduleType & " " & inputInfo.ScheduleValue &
            " / " & inputInfo.Magnitude & " " & inputInfo.Reinforcer & " / Deliver " & inputInfo.DeliveryP & "%"
    End Function

    Private Sub UpdateTTotal()
        If lblTTotal Is Nothing Then Exit Sub
        Dim cycleTotal As Double = ToDoubleSafe(txbTD.Text, 0) + ToDoubleSafe(txbTDelta.Text, 0)
        Dim cycles As Integer = ToIntSafe(txbTCycles.Text, 1)
        Dim gap As Double = ToDoubleSafe(txbTInterPeriod.Text, 0)
        Dim absoluteTotal As Double = TAbsoluteDuration(ToDoubleSafe(txbTD.Text, 0), ToDoubleSafe(txbTDelta.Text, 0), cycles, gap)
        lblTTotal.Text = "Cycle: " & cycleTotal & " s / Total: " & absoluteTotal & " s"
    End Sub

    Private Sub UpdateScheduleModeState()
        Dim tMode As Boolean = IsTScheduleMode()

        rdoExt1.Visible = Not tMode
        rdoNAL1.Visible = Not tMode
        rdoFRL1.Visible = Not tMode
        rdoVRL1.Visible = Not tMode
        rdoFIL1.Visible = Not tMode
        rdoVIL1.Visible = Not tMode
        Label13.Visible = Not tMode
        txbValueL1.Visible = Not tMode
        grpTSchedule.Visible = tMode

        If tMode Then
            grpMagnitude.Location = New System.Drawing.Point(18, 226)
            grpStimL1.Location = New System.Drawing.Point(18, 308)
            GroupBox2.Location = New System.Drawing.Point(18, 450)
            GroupBox1.Height = 638
        Else
            grpMagnitude.Location = New System.Drawing.Point(18, 132)
            grpStimL1.Location = New System.Drawing.Point(18, 214)
            GroupBox2.Location = New System.Drawing.Point(18, 356)
            GroupBox1.Height = 548
        End If

        UpdateExtinctionState()
        UpdateTTotal()
    End Sub

    Private Sub UpdateExtinctionState()
        Dim noInputProgram As Boolean = (rdoExt1.Checked OrElse rdoNAL1.Checked) AndAlso IsTScheduleMode() = False

        txbValueL1.Enabled = Not noInputProgram AndAlso IsTScheduleMode() = False
        grpMagnitude.Enabled = Not noInputProgram

        If noInputProgram Then
            txbValueL1.Text = ""
            cbbReinforcer1.Text = ""
            txbMagL1.Text = ""
            txbDeliveryProbability1.Text = ""
            txbPelletProbability1.Text = ""
            txbDeliveryProbability1.Enabled = False
            txbPelletProbability1.Enabled = False
        Else
            If txbValueL1.Text = "" Then txbValueL1.Text = "0"
            If cbbReinforcer1.Text = "" Then cbbReinforcer1.Text = "Pellet"
            If txbMagL1.Text = "" Then txbMagL1.Text = "1"
            If txbDeliveryProbability1.Text = "" Then txbDeliveryProbability1.Text = "100"
            If txbPelletProbability1.Text = "" Then txbPelletProbability1.Text = "50"
            txbDeliveryProbability1.Enabled = True
            txbPelletProbability1.Enabled = (cbbReinforcer1.Text = "Random")
        End If
    End Sub

    Private Sub ResetInputEditor()
        txbInputName.Text = "Arduino Pin " & (AddedInputs.Count + 2)
        If cbbScheduleMode.Items.Count > 0 Then cbbScheduleMode.SelectedIndex = 0
        rdoExt1.Checked = True
        txbTD.Text = "0"
        txbTDelta.Text = "0"
        txbTPD.Text = "100"
        txbTPDelta.Text = "0"
        txbTCycles.Text = "1"
        txbTInterPeriod.Text = "0"
        If cbbTStartPeriod.Items.Count > 0 Then cbbTStartPeriod.SelectedIndex = 0
        ClearCheckedList(clbTDStim)
        ClearCheckedList(clbTDeltaStim)
        UpdateScheduleModeState()
        UpdateExtinctionState()
        txbStimDurL1.Text = "1"
        ClearCheckedList(clbFeedbackStim)
        rdoLight1L1.Checked = False
        rdoLight2L1.Checked = False
        rdoLight3L1.Checked = False
        rdoLight4L1.Checked = False
        rdoToneL1.Checked = False
        rdoHouselightL1.Checked = False
        rdoTOL1.Checked = False
        txbDelayDurL1.Text = "1"
        chkRetractL1.Checked = False
        chkUnsignaledDelay.Checked = False
        ClearCheckedList(clbDelayStim)
        rdoLightDelay1L1.Checked = False
        rdoLightDelay2L1.Checked = False
        rdoLightDelay3L1.Checked = False
        rdoLightDelay4L1.Checked = False
        rdoToneDelayL1.Checked = False
        rdoHouselightDelayL1.Checked = False
        txbDelaySignalDurationL1.Text = "0"
        UpdateFeedbackState()
        UpdateDelayState()
    End Sub

    Private Sub btnAddInput_Click(sender As Object, e As EventArgs) Handles btnAddInput.Click
        If AddedInputs.Count >= MAX_INPUTS Then
            MsgBox("This version supports up to " & MAX_INPUTS & " inputs.")
            Exit Sub
        End If

        Dim inputInfo As ComponentInputConfiguration = BuildInputFromEditor(AddedInputs.Count + 1)
        If inputInfo.ScheduleType = "T Schedule" Then
            If inputInfo.TDurationD <= 0 OrElse inputInfo.TDurationDelta <= 0 Then
                MsgBox("Please enter TD and TΔ durations greater than 0.")
                Exit Sub
            End If
            If inputInfo.TProbabilityD < 0 OrElse inputInfo.TProbabilityD > 100 OrElse inputInfo.TProbabilityDelta < 0 OrElse inputInfo.TProbabilityDelta > 100 Then
                MsgBox("Please enter T schedule probabilities between 0 and 100.")
                Exit Sub
            End If
            If inputInfo.TCycles <= 0 Then
                MsgBox("Please enter at least 1 T schedule cycle.")
                Exit Sub
            End If
            If inputInfo.TInterPeriod < 0 Then
                MsgBox("The time between T periods cannot be negative.")
                Exit Sub
            End If
            If inputInfo.TProbabilityDelta > inputInfo.TProbabilityD Then
                Dim result As DialogResult = MessageBox.Show(
                    "P in TΔ is greater than P in TD. This may be an error. Continue?",
                    "T schedule probability warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning)
                If result <> DialogResult.Yes Then Exit Sub
            End If
        End If

        If inputInfo.ScheduleType <> "Extinction" AndAlso inputInfo.ScheduleType <> "N/A" AndAlso inputInfo.ScheduleType <> "T Schedule" Then
            If inputInfo.ScheduleValue <= 0 Then
                MsgBox("Please enter a schedule value greater than 0 for this input.")
                Exit Sub
            End If

            If inputInfo.Reinforcer = "" OrElse inputInfo.Reinforcer = "N/A" Then
                MsgBox("Please select a reinforcer for this input.")
                Exit Sub
            End If

            If inputInfo.Magnitude <= 0 Then
                MsgBox("Please enter a reinforcer magnitude greater than 0 for this input.")
                Exit Sub
            End If

            If inputInfo.DeliveryP < 0 OrElse inputInfo.DeliveryP > 100 Then
                MsgBox("Please enter a delivery probability between 0 and 100.")
                Exit Sub
            End If

            If inputInfo.Reinforcer = "Random" AndAlso (inputInfo.PelletP < 0 OrElse inputInfo.PelletP > 100) Then
                MsgBox("Please enter a pellet probability between 0 and 100 for random reinforcers.")
                Exit Sub
            End If
        End If

        AddedInputs.Add(inputInfo)
        lstInputs.Items.Add(InputSummary(inputInfo))

        If AddedInputs.Count >= MAX_INPUTS Then
            btnAddInput.Enabled = False
            txbInputName.Enabled = False
            GroupBox1.Enabled = False
        Else
            ResetInputEditor()
        End If
    End Sub

    Private Sub btnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click

        ' === Validaciones fuertes (parse real) ===
        Dim componentDurationSec As Double = ToDoubleSafe(txbComponentDuration.Text, -1)
        If componentDurationSec <= 0 Then
            MsgBox("Please input Component duration (must be > 0).")
            Exit Sub
        End If

        Dim componentIterations As Byte = ToByteSafe(txbComponentIterations.Text, 0)
        If componentIterations = 0 Then
            MsgBox("Please input Component iterations (must be > 0).")
            Exit Sub
        End If

        ComponentStimType = GetSelectedComponentStimType()
        Dim componentLightIntermittencySec As Double = 0
        Dim componentToneIntermittencySec As Double = 0
        If HasComponentLightSelection() Then componentLightIntermittencySec = ToDoubleSafe(txbLightIntermittency.Text, 0)
        If HasComponentToneSelection() Then componentToneIntermittencySec = ToDoubleSafe(txbToneIntermittency.Text, 0)

        If AddedInputs.Count = 0 Then
            MsgBox("Please add at least one input before submitting the component.")
            Exit Sub
        End If

        Dim longestTDuration As Double = 0
        For Each inputInfo As ComponentInputConfiguration In AddedInputs
            If inputInfo.ScheduleType = "T Schedule" Then
                longestTDuration = Math.Max(longestTDuration, TAbsoluteDuration(inputInfo))
            End If
        Next

        If longestTDuration > componentDurationSec Then
            Dim result As DialogResult = MessageBox.Show(
                "The programmed T Schedule requires " & longestTDuration & " seconds, which exceeds the current component duration of " & componentDurationSec & " seconds." & Environment.NewLine &
                "For the full T Schedule to run correctly, set the component duration to " & longestTDuration & " seconds." & Environment.NewLine & Environment.NewLine &
                "Yes: use " & longestTDuration & " seconds as the component duration." & Environment.NewLine &
                "No: keep the current duration and continue." & Environment.NewLine &
                "Cancel: return to editing.",
                "T Schedule duration warning",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning)

            If result = DialogResult.Cancel Then Exit Sub
            If result = DialogResult.Yes Then
                componentDurationSec = longestTDuration
                txbComponentDuration.Text = CStr(longestTDuration)
            End If
        End If

        ' ===== Asignación a tu blueprint (tipos correctos) =====
        AC(vCC).ComponentName = ToStrSafe(txbComponentName.Text, "Component " & vCC)
        AC(vCC).ComponentDuration = componentDurationSec
        AC(vCC).ComponentIteration = componentIterations
        AC(vCC).ComponentLightIntermittency = componentLightIntermittencySec
        AC(vCC).ComponentStimDuration = componentToneIntermittencySec
        AC(vCC).ComponentStimType = ComponentStimType
        AC(vCC).TimeScheduleType = SelectedTimeScheduleType()
        AC(vCC).TimeScheduleValue = If(AC(vCC).TimeScheduleType = "None", 0, ToDoubleSafe(txbTimeScheduleValue.Text, 0))
        AC(vCC).TimeReinforcer = If(AC(vCC).TimeScheduleType = "None", "N/A", ToStrSafe(cbbTimeReinforcer.Text, "Pellet"))
        AC(vCC).TimeMagnitude = If(AC(vCC).TimeScheduleType = "None", 0, ToIntSafe(txbTimeMagnitude.Text, 1))
        AC(vCC).TimeDeliveryP = If(AC(vCC).TimeScheduleType = "None", 0, ToIntSafe(txbTimeDeliveryProbability.Text, 100))
        AC(vCC).TimePelletP = If(AC(vCC).TimeScheduleType = "None", 0, If(AC(vCC).TimeReinforcer = "Random", ToIntSafe(txbTimePelletProbability.Text, 50), 0))

        If AC(vCC).TimeScheduleType <> "None" Then
            If AC(vCC).TimeScheduleValue <= 0 Then
                MsgBox("Please enter a time schedule value greater than 0.")
                Exit Sub
            End If
            If AC(vCC).TimeMagnitude <= 0 Then
                MsgBox("Please enter a time schedule reinforcer magnitude greater than 0.")
                Exit Sub
            End If
            If AC(vCC).TimeDeliveryP < 0 OrElse AC(vCC).TimeDeliveryP > 100 OrElse AC(vCC).TimePelletP < 0 OrElse AC(vCC).TimePelletP > 100 Then
                MsgBox("Time schedule probabilities must be between 0 and 100.")
                Exit Sub
            End If
        End If

        AC(vCC).InputCount = Math.Min(AddedInputs.Count, MAX_INPUTS)

        ReDim AC(vCC).InputName(MAX_INPUTS - 1)
        ReDim AC(vCC).ScheduleType(MAX_INPUTS - 1)
        ReDim AC(vCC).ScheduleValue(MAX_INPUTS - 1)
        ReDim AC(vCC).Magnitude(MAX_INPUTS - 1)
        ReDim AC(vCC).Reinforcer(MAX_INPUTS - 1)
        ReDim AC(vCC).DeliveryP(MAX_INPUTS - 1)
        ReDim AC(vCC).PelletP(MAX_INPUTS - 1)
        ReDim AC(vCC).TDurationD(MAX_INPUTS - 1)
        ReDim AC(vCC).TDurationDelta(MAX_INPUTS - 1)
        ReDim AC(vCC).TProbabilityD(MAX_INPUTS - 1)
        ReDim AC(vCC).TProbabilityDelta(MAX_INPUTS - 1)
        ReDim AC(vCC).TCycles(MAX_INPUTS - 1)
        ReDim AC(vCC).TStartPeriod(MAX_INPUTS - 1)
        ReDim AC(vCC).TInterPeriod(MAX_INPUTS - 1)
        ReDim AC(vCC).TStimD(MAX_INPUTS - 1)
        ReDim AC(vCC).TStimDelta(MAX_INPUTS - 1)
        ReDim AC(vCC).FeedbackDuration(MAX_INPUTS - 1)
        ReDim AC(vCC).FeedbackType(MAX_INPUTS - 1)
        ReDim AC(vCC).DelayDuration(MAX_INPUTS - 1)
        ReDim AC(vCC).DelayType(MAX_INPUTS - 1)
        ReDim AC(vCC).DelayRetract(MAX_INPUTS - 1)
        ReDim AC(vCC).DelaySignalDuration(MAX_INPUTS - 1)

        ' OJO: tu array measured es Integer(), dimensionamos con Iteration (Byte) => se convierte a Integer sin problema
        ReDim AC(vCC).ComponentDuration_measured(CInt(componentIterations))

        Dim storedInputs(MAX_INPUTS - 1) As ComponentInputConfiguration
        For i As Integer = 0 To MAX_INPUTS - 1
            storedInputs(i) = BuildDefaultInput(i + 1)
        Next

        For i As Integer = 0 To AC(vCC).InputCount - 1
            storedInputs(i) = AddedInputs(i)
        Next

        For i As Integer = 0 To MAX_INPUTS - 1
            AC(vCC).InputName(i) = storedInputs(i).Name
            AC(vCC).ScheduleType(i) = storedInputs(i).ScheduleType
            AC(vCC).ScheduleValue(i) = storedInputs(i).ScheduleValue
            AC(vCC).Reinforcer(i) = storedInputs(i).Reinforcer
            AC(vCC).Magnitude(i) = storedInputs(i).Magnitude
            AC(vCC).DeliveryP(i) = storedInputs(i).DeliveryP
            AC(vCC).PelletP(i) = storedInputs(i).PelletP
            AC(vCC).TDurationD(i) = storedInputs(i).TDurationD
            AC(vCC).TDurationDelta(i) = storedInputs(i).TDurationDelta
            AC(vCC).TProbabilityD(i) = storedInputs(i).TProbabilityD
            AC(vCC).TProbabilityDelta(i) = storedInputs(i).TProbabilityDelta
            AC(vCC).TCycles(i) = storedInputs(i).TCycles
            AC(vCC).TStartPeriod(i) = storedInputs(i).TStartPeriod
            AC(vCC).TInterPeriod(i) = storedInputs(i).TInterPeriod
            AC(vCC).TStimD(i) = storedInputs(i).TStimD
            AC(vCC).TStimDelta(i) = storedInputs(i).TStimDelta
            AC(vCC).FeedbackDuration(i) = storedInputs(i).FeedbackDuration
            AC(vCC).FeedbackType(i) = storedInputs(i).FeedbackType
            AC(vCC).DelayDuration(i) = storedInputs(i).DelayDuration
            AC(vCC).DelayType(i) = storedInputs(i).DelayType
            AC(vCC).DelayRetract(i) = storedInputs(i).DelayRetract
            AC(vCC).DelaySignalDuration(i) = storedInputs(i).DelaySignalDuration
        Next

        ' Global
        AC(vCC).HouselightOnOff = CheckedListContains(clbComponentStim, "Houselight")

        Dim codSec As Double = ToDoubleSafe(txbCOD.Text, 0)
        AC(vCC).COD = CDbl(codSec) * 1000.0 ' tu COD es Double (ms)

        AC(vCC).MaxRefs = ToIntSafe(txbMaxRefs.Text, 0)

        SetUp.RefreshComponentSummaryTable()

        If vCC >= 2 Then SetUp.CheckBox1.Enabled = True

        Me.Close()
    End Sub

    Private Sub Component_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoScroll = False
        Me.StartPosition = FormStartPosition.CenterParent
        GroupBox1.Text = "Input"
        txbComponentName.Text = "Component " & vCC
        ComponentToolTip.InitialDelay = 300
        ComponentToolTip.ReshowDelay = 100
        ComponentToolTip.AutoPopDelay = 7000
        ComponentToolTip.SetToolTip(txbComponentDuration, "Component duration in seconds. Decimal values such as 1.5 are allowed.")
        ComponentToolTip.SetToolTip(txbMaxRefs, "Maximum reinforcers for this component iteration. Reaching this number ends the current iteration; later iterations can deliver up to this maximum again.")
        ComponentToolTip.SetToolTip(txbCOD, "Changeover delay in seconds. Decimal values such as 1.5 are allowed.")
        ComponentToolTip.SetToolTip(cbbReinforcer1, "Select Random to use pellet probability P for this input's reinforcer.")
        ComponentToolTip.SetToolTip(txbDeliveryProbability1, "Probability that a scheduled reinforcer is actually delivered.")
        ComponentToolTip.SetToolTip(txbPelletProbability1, "Pellet probability P when Random is selected.")
        ComponentToolTip.SetToolTip(txbStimDurL1, "Response feedback duration in seconds. Decimal values such as 1.5 are allowed.")
        ComponentToolTip.SetToolTip(rdoTOL1, "Time Out stops component stimulation, turns off chamber stimuli, and retracts all active inputs for the feedback duration.")
        ComponentToolTip.SetToolTip(txbDelayDurL1, "Delay duration in seconds. Decimal values such as 1.5 are allowed.")
        ComponentToolTip.SetToolTip(txbDelaySignalDurationL1, "Delay signal duration in seconds. It may be shorter or longer than the programmed delay; equal/greater values keep the signal on for the whole delay.")
        ComponentToolTip.SetToolTip(txbLightIntermittency, "0 keeps selected lights continuously on. Values greater than 0 set the on/off intermittency in seconds.")
        ComponentToolTip.SetToolTip(txbToneIntermittency, "0 keeps the tone continuously on. Values greater than 0 set the on/off intermittency in seconds.")
        ComponentToolTip.SetToolTip(txbTimeScheduleValue, "Time schedule value in seconds. FT delivers after this value; VT samples around this value.")
        ComponentToolTip.SetToolTip(txbTimeDeliveryProbability, "Probability that a time-schedule reinforcer is actually delivered.")
        ComponentToolTip.SetToolTip(txbTimePelletProbability, "Pellet probability P when the time-schedule reinforcer is Random.")
        ComponentToolTip.SetToolTip(cbbScheduleMode, "Classic schedules use FR/VR/FI/VI/Extinction/N/A. T Schedules use TD and TΔ cycles for this input.")
        ComponentToolTip.SetToolTip(rdoExt1, "Extinction: the input is available, responses are recorded, but responses do not produce reinforcers.")
        ComponentToolTip.SetToolTip(rdoNAL1, "N/A: this input is not available during the component and does not run a response schedule.")
        ComponentToolTip.SetToolTip(rdoFRL1, "Fixed Ratio: each fixed number of responses produces a scheduled reinforcer.")
        ComponentToolTip.SetToolTip(rdoVRL1, "Variable Ratio: the response requirement varies around the programmed value.")
        ComponentToolTip.SetToolTip(rdoFIL1, "Fixed Interval: a reinforcer becomes available after the fixed time; the next response collects it.")
        ComponentToolTip.SetToolTip(rdoVIL1, "Variable Interval: the availability interval varies around the programmed value; the next response collects it.")
        ComponentToolTip.SetToolTip(txbTD, "TD duration in seconds. Responses during TD use P TD to determine reinforcer delivery.")
        ComponentToolTip.SetToolTip(txbTDelta, "TΔ duration in seconds. Responses during TΔ use P TΔ to determine reinforcer delivery.")
        ComponentToolTip.SetToolTip(txbTPD, "Probability that each response during TD delivers a reinforcer.")
        ComponentToolTip.SetToolTip(txbTPDelta, "Probability that each response during TΔ delivers a reinforcer.")
        ComponentToolTip.SetToolTip(txbTCycles, "Number of complete TD + TΔ cycles to present during this component.")
        ComponentToolTip.SetToolTip(txbTInterPeriod, "Optional time in seconds between T schedule periods.")
        If cbbTimeReinforcer.Text = "" Then cbbTimeReinforcer.Text = "Pellet"
        If cbbScheduleMode.Items.Count > 0 AndAlso cbbScheduleMode.SelectedIndex < 0 Then cbbScheduleMode.SelectedIndex = 0
        If cbbTStartPeriod.Items.Count > 0 AndAlso cbbTStartPeriod.SelectedIndex < 0 Then cbbTStartPeriod.SelectedIndex = 0
        ResetInputEditor()
        UpdateComponentIntermittencyState()
        UpdateTimeScheduleState()
        UpdateScheduleModeState()
    End Sub

    Private Sub chkHouselightOnOff_CheckedChanged(sender As Object, e As EventArgs) Handles chkHouselightOnOff.CheckedChanged
        HouselightOnOff = chkHouselightOnOff.Checked
    End Sub

    Private Sub Schedule_CheckedChanged(sender As Object, e As EventArgs) Handles rdoExt1.CheckedChanged, rdoNAL1.CheckedChanged, rdoFRL1.CheckedChanged, rdoVRL1.CheckedChanged, rdoFIL1.CheckedChanged, rdoVIL1.CheckedChanged
        UpdateExtinctionState()
    End Sub

    Private Sub cbbScheduleMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbbScheduleMode.SelectedIndexChanged
        UpdateScheduleModeState()
    End Sub

    Private Sub TScheduleTextChanged(sender As Object, e As EventArgs) Handles txbTD.TextChanged, txbTDelta.TextChanged, txbTCycles.TextChanged, txbTInterPeriod.TextChanged
        UpdateTTotal()
    End Sub

    ' Cancel: limpia slot y baja contador (evita componentes a medias)
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        If vCC >= 1 Then
            AC(vCC) = New ComponentBlueprint
            vCC -= 1
        End If
        Me.Close()
    End Sub

    Private Sub cbbReinforcer1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbbReinforcer1.SelectedIndexChanged
        If rdoExt1.Checked OrElse rdoNAL1.Checked Then
            txbPelletProbability1.Text = ""
            txbDeliveryProbability1.Text = ""
            txbDeliveryProbability1.Enabled = False
            txbPelletProbability1.Enabled = False
            Exit Sub
        End If

        If cbbReinforcer1.Text = "Random" Then
            txbPelletProbability1.Text = "50"
            txbPelletProbability1.Enabled = True
        Else
            txbPelletProbability1.Text = "0"
            txbPelletProbability1.Enabled = False
        End If
    End Sub

    Private Sub Feedback_CheckedChanged(sender As Object, e As EventArgs) Handles rdoLight1L1.CheckedChanged, rdoLight2L1.CheckedChanged, rdoLight3L1.CheckedChanged, rdoLight4L1.CheckedChanged, rdoToneL1.CheckedChanged, rdoHouselightL1.CheckedChanged, rdoTOL1.CheckedChanged
        UpdateFeedbackState()
    End Sub

    Private Sub clbFeedbackStim_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles clbFeedbackStim.ItemCheck
        BeginInvoke(New MethodInvoker(AddressOf UpdateFeedbackState))
    End Sub

    Private Sub Delay_CheckedChanged(sender As Object, e As EventArgs) Handles chkUnsignaledDelay.CheckedChanged, rdoLightDelay1L1.CheckedChanged, rdoLightDelay2L1.CheckedChanged, rdoLightDelay3L1.CheckedChanged, rdoLightDelay4L1.CheckedChanged, rdoToneDelayL1.CheckedChanged, rdoHouselightDelayL1.CheckedChanged
        UpdateDelayState()
    End Sub

    Private Sub clbDelayStim_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles clbDelayStim.ItemCheck
        BeginInvoke(New MethodInvoker(AddressOf UpdateDelayState))
    End Sub

    Private Sub ComponentStim_CheckedChanged(sender As Object, e As EventArgs) Handles rdoComponentTone.CheckedChanged, rdoComponentStimLight1.CheckedChanged, rdoComponentStimLight2.CheckedChanged, rdoComponentStimLight3.CheckedChanged, rdoComponentStimLight4.CheckedChanged
        UpdateComponentIntermittencyState()
    End Sub

    Private Sub clbComponentStim_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles clbComponentStim.ItemCheck
        BeginInvoke(New MethodInvoker(AddressOf UpdateComponentIntermittencyState))
    End Sub


End Class
