Option Strict On

Partial Public Class ChamberSession
    Private ReadOnly lagTrackers(MAX_COMPONENTS) As LagSequenceTracker
    Private ReadOnly lagSuccesses(MAX_COMPONENTS) As Long
    Private ReadOnly lagFailures(MAX_COMPONENTS) As Long
    Private ReadOnly lagTimeoutResponses(MAX_COMPONENTS, MAX_INPUTS - 1) As Integer
    Private lagTimeoutTimer As Timer
    Private lagTimeoutActive As Boolean
    Private lagTimeoutToneActive As Boolean
    Private lagTimeoutPresentation As Long
    Private lagTimeoutDeadline As Long
    Private componentPresentation As Long
    Private lagComponentDeadline As Long
    Private ReadOnly lagPersistentFeedback(MAX_INPUTS - 1) As Boolean
    Private lagOutputsEnabled As Boolean
    Private lagPresentationSequenceCount As Integer
    Private lagSequenceLimitReached As Boolean

    Private Function IsLagComponent() As Boolean
        Return vCC > 0 AndAlso vCC <= MAX_COMPONENTS AndAlso AC(vCC).LagEnabled
    End Function

    Private Shared Function DeadlineAfter(seconds As Double) As Long
        Return Stopwatch.GetTimestamp() + CLng(seconds * Stopwatch.Frequency)
    End Function

    Private Sub BeginLagComponent()
        If Not IsLagComponent() Then Return
        BeginLagChartPresentation()
        lagPresentationSequenceCount = 0
        lagSequenceLimitReached = False
        If lagTrackers(vCC) Is Nothing Then
            lagTrackers(vCC) = New LagSequenceTracker(AC(vCC).LagSequenceLength, AC(vCC).LagValue)
            If AC(vCC).LagHistoryReference IsNot Nothing Then
                Dim reference As SessionReferenceData = AC(vCC).LagHistoryReference
                lagTrackers(vCC).SeedHistory(reference.LastSequences(AC(vCC).LagValue))
                WriteLine(1, vTimeNow, vCC, "LagHistorySeed", reference.SessionUid, reference.SelectedComponent, lagTrackers(vCC).HistoryCount)
                WriteLine(2, "Lag prior history component " & vCC & ": " & reference.SourceDescription & "; session UID=" & reference.SessionUid & "; source component=" & reference.SelectedComponent & "; sequences loaded=" & lagTrackers(vCC).HistoryCount)
            End If
        End If
        Array.Clear(lagPersistentFeedback, 0, lagPersistentFeedback.Length)
        lagOutputsEnabled = True
        lagTrackers(vCC).DiscardPartial()
        lagComponentDeadline = DeadlineAfter(AC(vCC).ComponentDuration)
        If IsYokedComponent() Then WriteLine(2, "Yoked Lag: criterion is recorded descriptively only. Reference trial outcomes determine exact delivered commands; current probability, magnitude, delay, timeout and MaxRefs are bypassed.")
        WriteLine(2, "Lag component " & vCC & ": compare against the previous " & AC(vCC).LagValue &
            " complete sequences; history retained across component presentations. Failure: " &
            If(AC(vCC).LagUseTimeout, "Time Out " & AC(vCC).LagTimeoutSeconds & " s" & If(AC(vCC).LagTimeoutTone, " + Tono", ""), "omit reinforcer") &
            ". Inputs arriving in the same Arduino frame are processed in ascending physical input order.")
    End Sub

    Private Sub ProcessLagResponse(inputIndex As Integer)
        'Response already counted in the cumulative plot. Preserve every physical input in raw data.
        If Stopwatch.GetTimestamp() >= lagComponentDeadline Then
            WriteRawEvent("LagBoundaryResponse" & (inputIndex + 1))
            ResponseCount(vCC, inputIndex) += 1
            ComponentDuration_Code()
            Return
        End If
        RecordLagChartResponse(inputIndex)
        If lagTimeoutActive Then
            WriteRawEvent("LagTOResponse" & (inputIndex + 1))
            ResponseCount(vCC, inputIndex) += 1
            lagTimeoutResponses(vCC, inputIndex) += 1
            RefreshInputStatusTable()
            Return
        End If
        If AnyDelayActive() Then
            WriteRawEvent("LagDelayResponse" & (inputIndex + 1))
            ResponseCountDel(vCC, inputIndex) += 1
            RefreshInputStatusTable()
            Return
        End If
        WriteRawEvent(CStr(inputIndex + 1))
        ResponseCount(vCC, inputIndex) += 1
        StartLagResponseFeedback(inputIndex)
        Dim result As LagSequenceResult = lagTrackers(vCC).AddResponse(inputIndex + 1)
        RefreshInputStatusTable()
        If result Is Nothing Then Return
        EndLagPersistentFeedback()
        RecordLagChartSequence(result.Sequence)
        WriteLine(1, vTimeNow, vCC, "LagSequence", result.SequenceNumber, result.Sequence,
            If(result.MeetsCriterion, "Pass", "Fail"), result.ComparedCount, AC(vCC).LagValue)
        lagPresentationSequenceCount += 1
        lagSequenceLimitReached = Not IsYokedComponent() AndAlso AC(vCC).LagMaxSequences > 0 AndAlso lagPresentationSequenceCount >= AC(vCC).LagMaxSequences
        Dim responsePresentation As Long = componentPresentation
        BeginLagOutcome(result.SequenceNumber, inputIndex)
        If IsYokedComponent() Then
            If result.MeetsCriterion Then
                lagSuccesses(vCC) += 1
            Else
                lagFailures(vCC) += 1
            End If
            DeliverYokedLagTrial(inputIndex)
            Return
        End If
        If result.MeetsCriterion Then
            lagSuccesses(vCC) += 1
            'All participating inputs share reinforcer/delay; attribute delivery to the last response.
            'Reinforce can synchronously change component on MaxRefs. Do nothing afterwards.
            Reinforce(inputIndex, False)
        Else
            lagFailures(vCC) += 1
            CompleteLagOutcome("Omitted")
            If AC(vCC).LagUseTimeout Then StartLagTimeout()
        End If
        If responsePresentation = componentPresentation AndAlso lagSequenceLimitReached AndAlso Not AnyDelayActive() AndAlso Not lagTimeoutActive AndAlso Not SessionEnding AndAlso Not tmrICI.Enabled Then
            WriteRawEvent("LagSequenceLimit")
            ComponentDuration_Code()
        End If
    End Sub

    Private Sub StartLagTimeout()
        If lagTimeoutTimer Is Nothing Then
            lagTimeoutTimer = New Timer()
            AddHandler lagTimeoutTimer.Tick, AddressOf LagTimeoutTick
        End If
        Array.Clear(lagPersistentFeedback, 0, lagPersistentFeedback.Length)
        lagTimeoutActive = True
        lagTimeoutPresentation = componentPresentation
        lagTimeoutDeadline = DeadlineAfter(AC(vCC).LagTimeoutSeconds)
        StopComponentStimulus()
        For i As Integer = 0 To MAX_INPUTS - 1
            FeedbackTimers(i).Stop()
        Next
        SendArduino("abefht")
        TurnOffAllInputs()
        lagTimeoutToneActive = AC(vCC).LagTimeoutTone
        If lagTimeoutToneActive Then ActivateStimulus("Tone", True)
        lagTimeoutTimer.Interval = Math.Max(1, CInt(AC(vCC).LagTimeoutSeconds * 1000))
        lagTimeoutTimer.Start()
        WriteRawEvent("LagTOStart")
        RefreshInputStatusTable()
    End Sub

    Private Sub LagTimeoutTick(sender As Object, e As EventArgs)
        If SessionClosed OrElse Not lagTimeoutActive Then Return
        Try
            UpdateSessionClock()
            If lagTimeoutPresentation <> componentPresentation OrElse Not SessionStarted OrElse SessionEnding OrElse tmrICI.Enabled Then
                StopLagActivity("Interrupted")
                Return
            End If
            'A component deadline wins over timeout expiry, even if Windows queues the TO tick first.
            If Stopwatch.GetTimestamp() >= lagComponentDeadline Then
                ComponentDuration_Code()
                Return
            End If
            If Stopwatch.GetTimestamp() < lagTimeoutDeadline Then Return
            lagTimeoutTimer.Stop()
            lagTimeoutActive = False
            WriteRawEvent("LagTOEnd")
            If lagSequenceLimitReached Then
                WriteRawEvent("LagSequenceLimit")
                ComponentDuration_Code()
                Return
            End If
            RestoreLagInputs()
            StartComponentStimulus()
            'Restore component ownership before releasing the timeout's tone, avoiding an off pulse.
            StopLagTimeoutTone()
            If AC(vCC).HouselightOnOff Then SendArduino("H")
            RefreshInputStatusTable()
        Catch ex As Exception
            FailSession(ex)
        End Try
    End Sub

    Private Sub StopLagTimeoutTone()
        If Not lagTimeoutToneActive Then Return
        lagTimeoutToneActive = False
        ActivateStimulus("Tone", False)
    End Sub

    Private Sub StopLagActivity(reason As String)
        CompleteLagOutcome("Cancelled:" & reason)
        lagOutputsEnabled = False
        Array.Clear(lagPersistentFeedback, 0, lagPersistentFeedback.Length)
        If lagTimeoutTimer IsNot Nothing Then lagTimeoutTimer.Stop()
        If lagTimeoutActive Then
            lagTimeoutActive = False
            WriteRawEvent("LagTOInterrupted:" & reason)
        End If
        StopLagTimeoutTone()
        If vCC > 0 AndAlso vCC <= MAX_COMPONENTS AndAlso lagTrackers(vCC) IsNot Nothing Then
            Dim pendingSequence As String = lagTrackers(vCC).DiscardPartial()
            If pendingSequence <> "" Then WriteLine(1, vTimeNow, vCC, "LagPartialDiscarded", pendingSequence, reason)
        End If
    End Sub

    Private Sub CompleteLagDelay(inputIndex As Integer)
        If Not DelayTimers(inputIndex).Enabled Then Return
        If Stopwatch.GetTimestamp() >= lagComponentDeadline Then
            ComponentDuration_Code()
            Return
        End If
        Dim presentation As Long = componentPresentation
        DelayTimers(inputIndex).Stop()
        DelaySignalStartTimers(inputIndex).Stop()
        DelaySignalTimers(inputIndex).Stop()
        ActivateStimulus(AC(vCC).DelayType(inputIndex), False)
        If DelayOnset(inputIndex) >= 0 AndAlso DelayComp(inputIndex) = vCC Then
            ObtainedDelayDurations(vCC, inputIndex).Add(Math.Max(0, vTimeNow - DelayOnset(inputIndex)))
        End If
        DelayOnset(inputIndex) = -1
        DelayComp(inputIndex) = -1
        Reinforce(inputIndex, True)
        If presentation <> componentPresentation OrElse SessionEnding OrElse SessionClosed OrElse tmrICI.Enabled Then Return
        If lagSequenceLimitReached Then
            WriteRawEvent("LagSequenceLimit")
            ComponentDuration_Code()
            Return
        End If
        If AC(vCC).DelayRetract(inputIndex) Then RestoreLagInputs()
        RestoreLagStimuli()
        RefreshInputStatusTable()
    End Sub

    Private Sub RestoreLagInputs()
        If lagTimeoutActive OrElse SessionEnding OrElse SessionClosed OrElse tmrICI.Enabled Then Return
        For i As Integer = 0 To MAX_INPUTS - 1
            PalIO(i) = IsInputAvailable(i)
            SetInputOutput(i, PalIO(i))
        Next
    End Sub

    Private Sub RestoreLagStimuli()
        If Not IsLagComponent() OrElse lagTimeoutActive OrElse SessionEnding OrElse SessionClosed OrElse tmrICI.Enabled Then Return
        Dim componentStim As String = If(AC(vCC).ComponentStimType, "")
        If AC(vCC).ComponentLightIntermittency <= 0 OrElse ComponentLightStimOn Then ActivateStimulus(componentStim, True, False)
        If componentStim.Contains("Tone") AndAlso (AC(vCC).ComponentStimDuration <= 0 OrElse ComponentToneStimOn) Then ActivateStimulus("Tone", True)
        If AC(vCC).HouselightOnOff Then SendArduino("H")
        For i As Integer = 0 To MAX_INPUTS - 1
            If FeedbackTimers(i).Enabled OrElse lagPersistentFeedback(i) Then ActivateStimulus(AC(vCC).FeedbackType(i), True)
            If DelayTimers(i).Enabled AndAlso (DelaySignalTimers(i).Enabled OrElse AC(vCC).DelaySignalDuration(i) <= 0 OrElse AC(vCC).DelaySignalDuration(i) >= AC(vCC).DelayDuration(i)) Then ActivateStimulus(AC(vCC).DelayType(i), True)
        Next
    End Sub

    Private Sub StartLagResponseFeedback(inputIndex As Integer)
        Dim feedbackStimulus As String = If(AC(vCC).FeedbackType(inputIndex), "None")
        If feedbackStimulus = "None" OrElse feedbackStimulus = "" Then Return
        FeedbackTimers(inputIndex).Stop()
        If AC(vCC).FeedbackDuration(inputIndex) = 0 Then
            lagPersistentFeedback(inputIndex) = True
            ActivateStimulus(feedbackStimulus, True)
        ElseIf AC(vCC).FeedbackDuration(inputIndex) > 0 Then
            Stimulus(inputIndex)
        End If
    End Sub

    Private Sub EndLagPersistentFeedback()
        Dim ended As New List(Of String)
        For i As Integer = 0 To MAX_INPUTS - 1
            If lagPersistentFeedback(i) Then ended.Add(AC(vCC).FeedbackType(i))
            lagPersistentFeedback(i) = False
        Next
        'Release all sequence owners before evaluating outputs still owned by timed pulses/component stimuli.
        For Each stimulus As String In ended
            ActivateStimulus(stimulus, False)
        Next
    End Sub

    Private Function LagOutputRequired(output As String) As Boolean
        If Not IsLagComponent() OrElse Not lagOutputsEnabled OrElse SessionEnding OrElse SessionClosed OrElse tmrICI.Enabled Then Return False
        'Timeout owns only its optional tone; late feedback/component off callbacks cannot cut it short.
        If lagTimeoutActive Then Return output = "Tone" AndAlso lagTimeoutToneActive
        If output = "Houselight" AndAlso AC(vCC).HouselightOnOff Then Return True
        Dim componentStim As String = If(AC(vCC).ComponentStimType, "")
        If componentStim.Contains(output) Then
            If output = "Tone" Then
                If AC(vCC).ComponentStimDuration <= 0 OrElse ComponentToneStimOn Then Return True
            ElseIf AC(vCC).ComponentLightIntermittency <= 0 OrElse ComponentLightStimOn Then
                Return True
            End If
        End If
        For i As Integer = 0 To MAX_INPUTS - 1
            If (lagPersistentFeedback(i) OrElse FeedbackTimers(i).Enabled) AndAlso If(AC(vCC).FeedbackType(i), "").Contains(output) Then Return True
            If DelayTimers(i).Enabled AndAlso (DelaySignalTimers(i).Enabled OrElse AC(vCC).DelaySignalDuration(i) <= 0 OrElse AC(vCC).DelaySignalDuration(i) >= AC(vCC).DelayDuration(i)) AndAlso If(AC(vCC).DelayType(i), "").Contains(output) Then Return True
        Next
        Return False
    End Function

    Private Function LagStatusText() As String
        If lagTimeoutActive Then Return "Time Out"
        If AnyDelayActive() Then Return "Reinforcer delay"
        If lagTrackers(vCC) Is Nothing Then Return "Ready"
        Return "Sequence " & lagTrackers(vCC).PartialCount & "/" & AC(vCC).LagSequenceLength &
            " | history " & lagTrackers(vCC).HistoryCount & "/" & AC(vCC).LagValue
    End Function

    Private Sub WriteLagSummary()
        For component As Integer = 1 To MAXvCC
            If Not AC(component).LagEnabled Then Continue For
            WriteLine(2, "Lag component " & component & ": complete=" & (lagSuccesses(component) + lagFailures(component)) &
                "; criterion passed=" & lagSuccesses(component) & "; criterion failed=" & lagFailures(component))
            For input As Integer = 0 To MAX_INPUTS - 1
                WriteLine(2, "Lag timeout responses component " & component & " input " & (input + 1) & ": " & lagTimeoutResponses(component, input))
            Next
        Next
    End Sub
End Class
