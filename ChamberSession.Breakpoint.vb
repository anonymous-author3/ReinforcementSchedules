Imports System.Globalization

Partial Public Class ChamberSession
    Private Class BreakpointProgress
        Public Definition As BreakpointDefinition
        Public Requirement As Integer
        Public Responses As Integer
        Public Presentation As Integer
        Public Active As Boolean
        Public PendingReinforcer As Boolean
        Public CompletedRatios As Integer
        Public ReinforcedRatios As Integer
        Public LargestCompleted As Integer
        Public LargestReinforced As Integer
    End Class
    Private breakpointStates(MAX_COMPONENTS, MAX_INPUTS - 1) As BreakpointProgress

    Private Function IsBreakpointSchedule(input As Integer) As Boolean
        Return input >= 0 AndAlso input < ActiveInputCount() AndAlso NormalizedScheduleType(input) = "breakpoint"
    End Function
    Private Function CurrentBreakpoint(input As Integer) As BreakpointProgress
        If input < 0 OrElse input >= MAX_INPUTS Then Return Nothing
        Return breakpointStates(vCC, input)
    End Function
    Private Sub BeginBreakpoint(input As Integer)
        If Not IsBreakpointSchedule(input) OrElse Not ChainComponentAlive() Then Return
        Dim definition = BreakpointConfiguration.GetDefinition(AC(vCC), input)
        Dim state = CurrentBreakpoint(input)
        If state IsNot Nothing AndAlso state.Active AndAlso state.Presentation = componentPresentation Then Return
        If state Is Nothing Then
            state = New BreakpointProgress With {.Definition = definition.Copy(), .Requirement = definition.StartRequirement}
            breakpointStates(vCC, input) = state
        Else
            state.Requirement = definition.StartRequirement
            state.Responses = 0
        End If
        state.Presentation = componentPresentation
        state.Active = True
        state.PendingReinforcer = False
        WriteLine(1, vTimeNow, vCC, "BreakpointStart", componentPresentation, input + 1, state.Requirement, state.Responses, definition.Progression, definition.StepValue)
        RefreshInputStatusTable()
    End Sub
    Private Sub ProcessBreakpointResponse(input As Integer, allowTimeoutTrigger As Boolean)
        Dim state = CurrentBreakpoint(input)
        If state Is Nothing OrElse Not state.Active OrElse state.PendingReinforcer OrElse state.Presentation <> componentPresentation Then Return
        If Not ChainComponentAlive() OrElse (ClassicTimeoutActive() AndAlso Not allowTimeoutTrigger) Then Return
        state.Responses += 1
        If state.Responses >= state.Requirement Then
            state.CompletedRatios += 1
            state.LargestCompleted = Math.Max(state.LargestCompleted, state.Requirement)
            state.PendingReinforcer = True
            WriteLine(1, vTimeNow, vCC, "BreakpointRatioComplete", componentPresentation, input + 1, state.Requirement, state.CompletedRatios)
            Reinforce(input, False)
        End If
        RefreshInputStatusTable()
    End Sub
    Private Sub CompleteBreakpointDelivery(input As Integer, deliveredCount As Integer)
        Dim state = CurrentBreakpoint(input)
        If state Is Nothing OrElse Not state.Active OrElse Not state.PendingReinforcer OrElse state.Presentation <> componentPresentation Then Return
        WriteLine(1, vTimeNow, vCC, "BreakpointReinforcementResolved", componentPresentation, input + 1, state.Requirement, deliveredCount)
        state.PendingReinforcer = False
        state.Responses = 0
        If deliveredCount > 0 Then
            state.ReinforcedRatios += 1
            state.LargestReinforced = Math.Max(state.LargestReinforced, state.Requirement)
            'Magnitude is one delivery batch: a successful earned ratio advances once.
            'Manual and independent time deliveries do not call this method.
            If Not MaximumReinforcersReached() Then state.Requirement = BreakpointConfiguration.NextRequirement(state.Requirement, state.Definition)
        End If
        If Not MaximumReinforcersReached() Then WriteLine(1, vTimeNow, vCC, "BreakpointRequirement", componentPresentation, input + 1, state.Requirement)
        RefreshInputStatusTable()
    End Sub
    Private Sub StopBreakpoints(reason As String)
        For input As Integer = 0 To MAX_INPUTS - 1
            Dim state = CurrentBreakpoint(input)
            If state Is Nothing OrElse Not state.Active Then Continue For
            state.Active = False
            Try
                WriteLine(1, vTimeNow, vCC, "BreakpointStop", state.Presentation, input + 1, state.Requirement, state.Responses, If(state.PendingReinforcer, "Pending delivery cancelled", "Incomplete ratio"), reason)
            Catch ex As Exception
                If reason <> "SessionStop" Then Throw
                currentStatus = "Could not log breakpoint cancellation: " & ex.Message
            End Try
            'A cancelled delivery is not carried into a later presentation.
            If state.PendingReinforcer Then state.Responses = 0
            state.PendingReinforcer = False
        Next
    End Sub
    Private Function BreakpointStatusText(input As Integer) As String
        Dim state = CurrentBreakpoint(input)
        If state Is Nothing Then Return "Breakpoint ready"
        If state.PendingReinforcer Then Return "Ratio " & state.Requirement & " complete / reinforcement delay"
        Return state.Responses & "/" & state.Requirement & " responses" & If(state.Active, "", " (stopped)")
    End Function
    Private Sub WriteBreakpointSummary()
        For component As Integer = 1 To MAXvCC
            For input As Integer = 0 To MAX_INPUTS - 1
                Dim state = breakpointStates(component, input)
                If state Is Nothing Then Continue For
                WriteLine(2, "Breakpoint Component " & component & " " & InputLabel(input, component) & ": completed ratios=" & state.CompletedRatios & "; reinforced ratios=" & state.ReinforcedRatios & "; largest completed requirement=" & state.LargestCompleted & "; largest reinforced requirement=" & state.LargestReinforced & "; final requirement=" & state.Requirement & "; partial responses=" & state.Responses)
            Next
        Next
    End Sub
End Class
