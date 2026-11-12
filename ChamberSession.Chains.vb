Imports System.Diagnostics
Imports System.Globalization

Partial Public Class ChamberSession
    Private Class ChainProgress
        Public Definition As ClassicChainDefinition
        Public LinkIndex As Integer
        Public RatioCount As Integer
        Public Criterion As Double
        Public Deadline As Long
        Public Started As Long
        Public Ready As Boolean
        Public PendingReinforcer As Boolean
        Public Active As Boolean
        Public Presentation As Integer
        Public Bags As New Dictionary(Of Integer, List(Of Double))
    End Class
    Private chainStates(MAX_INPUTS - 1) As ChainProgress
    Private chainStimulusTimer As Timer
    Private chainOutputState As New Dictionary(Of String, Boolean)
    Private refreshingChainOutputs As Boolean
    Private Shared ReadOnly chainOutputNames() As String = {"Light 1", "Light 2", "Light 3", "Light 4", "Tone", "Houselight"}
    Private Shared ReadOnly chainOutputOn() As String = {"A", "B", "E", "F", "T", "H"}

    Private Function HasClassicChain(input As Integer) As Boolean
        Return input >= 0 AndAlso input < ActiveInputCount() AndAlso Not IsLagComponent() AndAlso Not IsYokedComponent() AndAlso ClassicChainConfiguration.GetChain(AC(vCC), input) IsNot Nothing
    End Function

    Private Function AnyClassicChainActive() As Boolean
        Return chainStates.Any(Function(state) state IsNot Nothing AndAlso state.Active)
    End Function

    Private Function ChainComponentAlive() As Boolean
        If SessionClosed OrElse SessionEnding OrElse Not SessionStarted OrElse tmrStart.Enabled OrElse tmrICI.Enabled OrElse Not tmrComponentDuration.Enabled Then Return False
        If componentStartedTimestamp > 0 AndAlso (Stopwatch.GetTimestamp() - componentStartedTimestamp) / CDbl(Stopwatch.Frequency) >= AC(vCC).ComponentDuration Then
            ComponentDuration_Code()
            Return False
        End If
        Return True
    End Function

    Private Function ClassicTimeoutActive() As Boolean
        For i As Integer = 0 To ActiveInputCount() - 1
            If FeedbackTimers(i).Enabled AndAlso If(AC(vCC).FeedbackType(i), "").Contains("Time Out") Then Return True
        Next
        Return False
    End Function

    Private Sub BeginClassicChain(input As Integer)
        If Not HasClassicChain(input) OrElse Not ChainComponentAlive() Then Return
        Dim state = chainStates(input)
        If state Is Nothing OrElse state.Presentation <> componentPresentation Then
            state = New ChainProgress With {.Definition = ClassicChainConfiguration.GetChain(AC(vCC), input).Copy(), .Presentation = componentPresentation}
            chainStates(input) = state
        End If
        state.Active = True
        state.PendingReinforcer = False
        state.LinkIndex = 0
        WriteLine(1, vTimeNow, vCC, "ChainStart", componentPresentation, input + 1)
        StartChainLink(input)
        If chainStimulusTimer Is Nothing Then
            chainStimulusTimer = New Timer With {.Interval = 20}
            AddHandler chainStimulusTimer.Tick, AddressOf ChainStimulusTick
        End If
        chainStimulusTimer.Start()
    End Sub

    Private Sub StartChainLink(input As Integer)
        Dim state = chainStates(input)
        If state Is Nothing OrElse Not state.Active OrElse Not ChainComponentAlive() Then Return
        Dim link = state.Definition.Links(state.LinkIndex)
        state.Started = Stopwatch.GetTimestamp()
        Dim extend = Not link.RetractInput AndAlso Not ClassicTimeoutActive()
        If PalIO(input) <> extend Then SetInputOutput(input, extend)
        state.Ready = False
        state.RatioCount = 0
        state.PendingReinforcer = False
        state.Criterion = link.Value
        Dim kind = link.ScheduleType.ToLowerInvariant()
        If kind.StartsWith("variable") Then
            Dim bag As List(Of Double) = Nothing
            If Not state.Bags.TryGetValue(state.LinkIndex, bag) Then
                bag = New List(Of Double)
                state.Bags.Add(state.LinkIndex, bag)
            End If
            If bag.Count = 0 Then
                If kind = "variable ratio" Then
                    bag.AddRange(IntervalScheduleTiming.GenerateVariableRatios(CInt(link.Value), link.VariableValueCount).Select(Function(n) CDbl(n)))
                Else
                    bag.AddRange(IntervalScheduleTiming.GenerateVariableSeconds(link.Value, link.VariableValueCount))
                End If
            End If
            Dim selected = SessionRandom.Next(bag.Count)
            state.Criterion = bag(selected)
            bag.RemoveAt(selected)
        End If
        ScheduleTimers(input).Stop()
        If kind.Contains("interval") OrElse kind.Contains("time") Then
            Dim milliseconds = IntervalScheduleTiming.ToTimerMilliseconds(state.Criterion)
            state.Deadline = state.Started + CLng(Math.Ceiling(milliseconds * CDbl(Stopwatch.Frequency) / 1000.0))
            ScheduleTimers(input).Interval = milliseconds
            ScheduleTimers(input).Start()
        Else
            state.Deadline = 0
        End If
        WriteLine(1, vTimeNow, vCC, "ChainLinkStart", componentPresentation, input + 1, state.LinkIndex + 1, link.ScheduleType, state.Criterion)
        RefreshChainOutputs()
        RefreshInputStatusTable()
    End Sub

    Private Sub ProcessChainResponse(input As Integer, Optional allowTimeoutTrigger As Boolean = False)
        Dim state = chainStates(input)
        If state Is Nothing OrElse Not state.Active OrElse state.PendingReinforcer OrElse state.Presentation <> componentPresentation Then Return
        If Not ChainComponentAlive() OrElse (ClassicTimeoutActive() AndAlso Not allowTimeoutTrigger) Then Return
        Dim kind = state.Definition.Links(state.LinkIndex).ScheduleType.ToLowerInvariant()
        If kind.Contains("ratio") Then
            state.RatioCount += 1
            If state.RatioCount >= state.Criterion Then CompleteChainLink(input)
        ElseIf kind.Contains("interval") AndAlso Stopwatch.GetTimestamp() >= state.Deadline Then
            CompleteChainLink(input)
        End If
    End Sub

    Private Sub ChainScheduleTick(input As Integer)
        Dim state = chainStates(input)
        If state Is Nothing OrElse Not state.Active OrElse state.PendingReinforcer OrElse state.Presentation <> componentPresentation Then Return
        If Not ChainComponentAlive() Then Return
        Dim remaining = state.Deadline - Stopwatch.GetTimestamp()
        If remaining > 0 Then
            'A queued tick from an earlier link must not complete the new one.
            ScheduleTimers(input).Interval = Math.Max(1, CInt(Math.Ceiling(remaining * 1000.0 / Stopwatch.Frequency)))
            ScheduleTimers(input).Start()
            Return
        End If
        Dim kind = state.Definition.Links(state.LinkIndex).ScheduleType.ToLowerInvariant()
        If kind.Contains("time") Then
            If ClassicTimeoutActive() OrElse AnyDelayActive() Then
                ScheduleTimers(input).Interval = 20
                ScheduleTimers(input).Start()
            Else
                CompleteChainLink(input)
            End If
        ElseIf kind.Contains("interval") Then
            state.Ready = True
            ScheduleTimers(input).Stop()
            RefreshInputStatusTable()
        End If
    End Sub

    Private Sub CompleteChainLink(input As Integer)
        Dim state = chainStates(input)
        If state Is Nothing OrElse Not state.Active OrElse state.PendingReinforcer OrElse Not ChainComponentAlive() Then Return
        Dim presentation = componentPresentation
        ScheduleTimers(input).Stop()
        WriteLine(1, vTimeNow, vCC, "ChainLinkComplete", presentation, input + 1, state.LinkIndex + 1)
        If state.LinkIndex + 1 < state.Definition.Links.Count Then
            state.LinkIndex += 1
            StartChainLink(input)
            Return
        End If
        state.PendingReinforcer = True
        WriteLine(1, vTimeNow, vCC, "ChainComplete", presentation, input + 1)
        RefreshChainOutputs()
        Reinforce(input, False)
    End Sub

    Private Sub StopClassicChains(reason As String)
        Dim hadChains = AnyClassicChainActive() OrElse chainOutputState.Count > 0
        If chainStimulusTimer IsNot Nothing Then chainStimulusTimer.Stop()
        For input As Integer = 0 To MAX_INPUTS - 1
            Dim state = chainStates(input)
            If state IsNot Nothing AndAlso state.Active Then
                If reason = "SessionStop" Then
                    Try
                        WriteLine(1, vTimeNow, vCC, "ChainCancelled", state.Presentation, input + 1, state.LinkIndex + 1, reason)
                    Catch ex As Exception
                        currentStatus = "Could not log chain cancellation: " & ex.Message
                    End Try
                Else
                    WriteLine(1, vTimeNow, vCC, "ChainCancelled", state.Presentation, input + 1, state.LinkIndex + 1, reason)
                End If
                state.Active = False
                If ScheduleTimers(input) IsNot Nothing Then ScheduleTimers(input).Stop()
            End If
            chainStates(input) = Nothing
        Next
        chainOutputState.Clear()
        If hadChains Then
            If reason = "SessionStop" Then
                Try
                    SendArduino("abefht")
                Catch ex As Exception
                    currentStatus = "Could not switch off chain signals: " & ex.Message
                End Try
            Else
                SendArduino("abefht")
            End If
        End If
    End Sub

    Private Function ChainInputShouldBeExtended(input As Integer) As Boolean
        Dim state = chainStates(input)
        If state Is Nothing OrElse Not state.Active OrElse state.PendingReinforcer Then Return True
        Return Not state.Definition.Links(state.LinkIndex).RetractInput
    End Function

    Private Function ChainScheduleText(input As Integer) As String
        Dim definition = ClassicChainConfiguration.GetChain(AC(vCC), input)
        If definition Is Nothing Then Return ""
        Return String.Join(" -> ", definition.Links.Select(Function(link) link.ScheduleType & " " & link.Value.ToString(CultureInfo.InvariantCulture)))
    End Function

    Private Function ChainStatusText(input As Integer) As String
        Dim state = chainStates(input)
        If state Is Nothing OrElse Not state.Active Then Return "Chain ready"
        If state.PendingReinforcer Then Return "Chain complete / reinforcement delay"
        Dim kind = state.Definition.Links(state.LinkIndex).ScheduleType
        Return "Link " & (state.LinkIndex + 1) & "/" & state.Definition.Links.Count & " " & kind & " " & state.Criterion.ToString("0.###", CultureInfo.InvariantCulture) & If(kind.Contains("Ratio"), " (" & state.RatioCount & " responses)", " s")
    End Function

    Private Sub ChainStimulusTick(sender As Object, e As EventArgs)
        If SessionClosed Then Return
        Try
            UpdateSessionClock()
            If Not ChainComponentAlive() Then Return
            RefreshChainOutputs()
        Catch ex As Exception
            FailSession(ex)
        End Try
    End Sub

    Private Function ChainOutputRequired(name As String) As Boolean
        If ClassicTimeoutActive() Then Return False
        For Each state In chainStates
            If state Is Nothing OrElse Not state.Active OrElse state.PendingReinforcer OrElse state.Presentation <> componentPresentation Then Continue For
            Dim link = state.Definition.Links(state.LinkIndex)
            If Not If(link.StimulusType, "").Contains(name) Then Continue For
            Dim period = If(name = "Tone", link.ToneIntermittency, link.LightIntermittency)
            If period <= 0 Then Return True
            Dim elapsed = (Stopwatch.GetTimestamp() - state.Started) / CDbl(Stopwatch.Frequency)
            If CLng(Math.Floor(elapsed / period)) Mod 2 = 0 Then Return True
        Next
        Return False
    End Function

    Private Function OtherOutputRequired(name As String) As Boolean
        If ClassicTimeoutActive() Then Return False
        If name = "Houselight" AndAlso AC(vCC).HouselightOnOff Then Return True
        If If(AC(vCC).ComponentStimType, "").Contains(name) Then
            If name = "Tone" Then
                If AC(vCC).ComponentStimDuration <= 0 OrElse ComponentToneStimOn Then Return True
            ElseIf AC(vCC).ComponentLightIntermittency <= 0 OrElse ComponentLightStimOn Then
                Return True
            End If
        End If
        For input As Integer = 0 To ActiveInputCount() - 1
            If FeedbackTimers(input).Enabled AndAlso If(AC(vCC).FeedbackType(input), "").Contains(name) Then Return True
            If DelayTimers(input).Enabled AndAlso If(AC(vCC).DelayType(input), "").Contains(name) Then
                Dim signal = AC(vCC).DelaySignalDuration(input)
                If signal <= 0 OrElse signal >= AC(vCC).DelayDuration(input) OrElse DelaySignalTimers(input).Enabled Then Return True
            End If
        Next
        Return False
    End Function

    Private Sub RefreshChainOutputs(Optional force As Boolean = False)
        If refreshingChainOutputs OrElse Not AnyClassicChainActive() OrElse SessionClosed OrElse SessionEnding OrElse tmrICI.Enabled Then Return
        refreshingChainOutputs = True
        Try
            For index As Integer = 0 To chainOutputNames.Length - 1
                Dim name = chainOutputNames(index)
                Dim required = ChainOutputRequired(name) OrElse OtherOutputRequired(name)
                Dim previous As Boolean
                If force OrElse Not chainOutputState.TryGetValue(name, previous) OrElse previous <> required Then
                    SendArduino(If(required, chainOutputOn(index), chainOutputOn(index).ToLowerInvariant()))
                    chainOutputState(name) = required
                End If
            Next
        Finally
            refreshingChainOutputs = False
        End Try
    End Sub
End Class
