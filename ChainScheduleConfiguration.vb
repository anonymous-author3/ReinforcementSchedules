Option Strict On
Imports System.Globalization

<Serializable>
Public Class ClassicChainLink
    Public Property ScheduleType As String = "Fixed Ratio"
    Public Property Value As Double = 1
    Public Property VariableValueCount As Integer = 10
    Public Property StimulusType As String = "None"
    Public Property LightIntermittency As Double
    Public Property ToneIntermittency As Double
    Public Property RetractInput As Boolean = False
    Public Function Copy() As ClassicChainLink
        Return DirectCast(MemberwiseClone(), ClassicChainLink)
    End Function
End Class

<Serializable>
Public Class ClassicChainDefinition
    Public Property Links As New List(Of ClassicChainLink)
    Public Function Copy() As ClassicChainDefinition
        Return New ClassicChainDefinition With {.Links = If(Links Is Nothing, Nothing, Links.Select(Function(item) If(item Is Nothing, Nothing, item.Copy())).ToList())}
    End Function
End Class

Friend NotInheritable Class ClassicChainConfiguration
    Public Shared ReadOnly Kinds As String() = {"Fixed Ratio", "Variable Ratio", "Fixed Interval", "Variable Interval", "Fixed Time", "Variable Time"}
    Public Shared ReadOnly Outputs As String() = {"Light 1", "Light 2", "Light 3", "Light 4", "Tone", "Houselight"}
    Private Sub New()
    End Sub
    Public Shared Sub NormalizeLegacy(ByRef component As ComponentBlueprint)
        If component.ClassicChains Is Nothing Then component.ClassicChains = New ClassicChainDefinition(MAX_INPUTS - 1) {}
    End Sub
    Public Shared Function GetChain(component As ComponentBlueprint, input As Integer) As ClassicChainDefinition
        If component.ClassicChains Is Nothing OrElse input < 0 OrElse input >= component.ClassicChains.Length Then Return Nothing
        Dim chain = component.ClassicChains(input)
        If chain Is Nothing OrElse chain.Links Is Nothing OrElse chain.Links.Count = 0 Then Return Nothing
        Return chain
    End Function
    Public Shared Function LinkError(link As ClassicChainLink) As String
        If link Is Nothing Then Return "Missing chain link."
        If Not Kinds.Contains(link.ScheduleType) Then Return "Choose a supported classic schedule for every link."
        If Double.IsNaN(link.Value) OrElse Double.IsInfinity(link.Value) OrElse link.Value <= 0 Then Return "Every link needs a finite value greater than zero."
        If link.ScheduleType.Contains("Ratio") OrElse link.ScheduleType.Contains("Interval") Then
            If link.Value <> Math.Truncate(link.Value) OrElse link.Value > Integer.MaxValue Then Return "Ratio and interval values must be positive whole numbers."
            If link.ScheduleType = "Fixed Ratio" AndAlso link.Value > Integer.MaxValue - 1 Then Return "Fixed ratio is too large."
            Dim problem = IntervalScheduleTiming.ValidationError(link.ScheduleType, CInt(link.Value), link.VariableValueCount)
            If problem <> "" Then Return problem
        Else
            Dim problem = IntervalScheduleTiming.TimeValidationError(link.ScheduleType, link.Value, link.VariableValueCount)
            If problem <> "" Then Return problem
        End If
        If link.RetractInput AndAlso link.ScheduleType <> "Fixed Time" AndAlso link.ScheduleType <> "Variable Time" Then Return "Input retraction is only available for FT/VT links."
        If link.VariableValueCount < 1 OrElse link.VariableValueCount > 1000 Then Return "Series values must be between 1 and 1000."
        For Each period In {link.LightIntermittency, link.ToneIntermittency}
            If Double.IsNaN(period) OrElse Double.IsInfinity(period) OrElse period < 0 OrElse period > 2147483 Then Return "Signal intermittency must be finite and between 0 and 2147483 seconds."
            If period > 0 AndAlso period < 0.05 Then Return "Signal intermittency must be zero or at least 0.05 seconds (software-timed, not hardware precision)."
        Next
        If link.StimulusType Is Nothing Then Return "Missing link stimulus."
        If link.StimulusType <> "None" Then
            For Each name In link.StimulusType.Split({" + "}, StringSplitOptions.None)
                If Not Outputs.Contains(name) Then Return "Unknown link stimulus: " & name
            Next
        End If
        Return ""
    End Function
    Public Shared Function ValidationError(component As ComponentBlueprint) As String
        If component.ClassicChains Is Nothing Then Return ""
        If component.ClassicChains.Length <> MAX_INPUTS Then Return "Incomplete classic chain settings."
        For input As Integer = 0 To MAX_INPUTS - 1
            Dim chain = component.ClassicChains(input)
            If chain Is Nothing Then Continue For
            If chain.Links Is Nothing Then Return "Missing chain links for input " & (input + 1)
            If chain.Links.Count = 0 Then Continue For
            If chain.Links.Count > 100 Then Return "Use no more than 100 links per input."
            If input >= component.InputCount Then Return "A chain is assigned to an inactive input."
            If component.LagEnabled OrElse component.YokedEnabled OrElse (component.ScheduleType IsNot Nothing AndAlso component.ScheduleType.Any(Function(kind) kind = "T Schedule")) Then Return "Chains are only available for non-yoked classic schedules."
            If chain.Links(0) Is Nothing Then Return "Missing first chain link."
            If component.ScheduleType Is Nothing OrElse component.ScheduleType(input) <> chain.Links(0).ScheduleType Then Return "The base schedule type must match the first chain link on input " & (input + 1) & "."
            For n As Integer = 0 To chain.Links.Count - 1
                Dim problem = LinkError(chain.Links(n))
                If problem <> "" Then Return "Input " & (input + 1) & ", link " & (n + 1) & ": " & problem
            Next
        Next
        Return ""
    End Function
    Public Shared Function Summary(chain As ClassicChainDefinition) As String
        If chain Is Nothing OrElse chain.Links Is Nothing Then Return ""
        Return String.Join(" -> ", chain.Links.Select(Function(link) link.ScheduleType & " " & link.Value.ToString("0.###", CultureInfo.InvariantCulture))) & " -> Reinforcer"
    End Function
    Private Shared Function UsesOutput(selection As String, output As String) As Boolean
        Return If(selection, "").Split({" + "}, StringSplitOptions.None).Contains(output)
    End Function
    Public Shared Function ConflictWarnings(component As ComponentBlueprint) As List(Of String)
        Dim warnings As New List(Of String)
        For i As Integer = 0 To Math.Min(component.InputCount, MAX_INPUTS) - 1
            Dim chain = GetChain(component, i)
            If chain Is Nothing Then Continue For
            For Each output In Outputs
                If Not chain.Links.Any(Function(link) UsesOutput(link.StimulusType, output)) Then Continue For
                If UsesOutput(component.ComponentStimType, output) OrElse (output = "Houselight" AndAlso component.HouselightOnOff) Then warnings.Add("Input " & (i + 1) & " chain and component both use " & output & ".")
                For j As Integer = 0 To Math.Min(component.InputCount, MAX_INPUTS) - 1
                    If component.FeedbackType IsNot Nothing AndAlso UsesOutput(component.FeedbackType(j), output) Then warnings.Add("Input " & (i + 1) & " chain shares " & output & " with feedback on input " & (j + 1) & ".")
                    If component.DelayType IsNot Nothing AndAlso If(component.DelayType(j), "").Contains(output) Then warnings.Add("Input " & (i + 1) & " chain shares " & output & " with delay signaling on input " & (j + 1) & ".")
                    If j <= i Then Continue For
                    Dim other = GetChain(component, j)
                    If other IsNot Nothing AndAlso other.Links.Any(Function(link) UsesOutput(link.StimulusType, output)) Then warnings.Add("Chains on inputs " & (i + 1) & " and " & (j + 1) & " both use " & output & ".")
                Next
            Next
        Next
        Return warnings.Distinct().ToList()
    End Function
End Class
