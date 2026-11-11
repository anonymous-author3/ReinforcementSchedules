Option Strict On
Friend NotInheritable Class ReferenceConfiguration
    Private Sub New()
    End Sub
    Public Shared Function ValidateSelection(component As ComponentBlueprint, reference As SessionReferenceData) As String
        If reference Is Nothing Then Return "Choose a Yoked reference session."
        Dim problem As String = reference.ValidateYoked(component.LagEnabled)
        If problem <> "" Then Return problem
        If component.ScheduleType Is Nothing OrElse component.ScheduleType.Length <> MAX_INPUTS Then Return "Incomplete input configuration."
        If component.LagEnabled Then
            Return reference.ValidateHistory(component.LagSequenceLength, Enumerable.Range(1, MAX_INPUTS).Where(Function(n) component.ScheduleType(n - 1) = "Lag"))
        End If
        If reference.Sequences.Count > 0 Then Return "Choose a Classic/T reference for a Classic/T component, or use Lag Yoked for a sequence reference."
        If reference.Deliveries.Any(Function(d) d.Input < 1 OrElse d.Input > component.InputCount OrElse component.ScheduleType(d.Input - 1) = "N/A") Then Return "Reference delivery inputs must be active in this component."
        Return ""
    End Function
    Public Shared Function ValidationError(component As ComponentBlueprint) As String
        If component.YokedEnabled Then
            If component.LagHistoryReference IsNot Nothing Then Return "Choose either prior Lag history or Yoked, not both."
            Return ValidateSelection(component, component.YokedReference)
        End If
        If component.YokedReference IsNot Nothing Then Return "Disable and clear the unused Yoked reference."
        If component.LagHistoryReference IsNot Nothing AndAlso Not component.LagEnabled Then Return "Prior sequence history requires a Lag component."
        Return ""
    End Function
End Class
