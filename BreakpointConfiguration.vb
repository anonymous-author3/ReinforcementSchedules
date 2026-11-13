Option Strict On
Imports System.Globalization

<Serializable>
Public Class BreakpointDefinition
    Public Property Progression As String = "Arithmetic"
    Public Property StartRequirement As Integer = 2
    Public Property StepValue As Decimal = 1D
    Public Function Copy() As BreakpointDefinition
        Return DirectCast(MemberwiseClone(), BreakpointDefinition)
    End Function
End Class

Friend NotInheritable Class BreakpointConfiguration
    Private Sub New()
    End Sub
    Public Shared Sub NormalizeLegacy(ByRef component As ComponentBlueprint)
        If component.BreakpointSchedules Is Nothing Then component.BreakpointSchedules = New BreakpointDefinition(MAX_INPUTS - 1) {}
    End Sub
    Public Shared Function GetDefinition(component As ComponentBlueprint, input As Integer) As BreakpointDefinition
        If component.BreakpointSchedules Is Nothing OrElse input < 0 OrElse input >= component.BreakpointSchedules.Length Then Return Nothing
        Return component.BreakpointSchedules(input)
    End Function
    Public Shared Function ValidateDefinition(definition As BreakpointDefinition) As String
        If definition Is Nothing Then Return "Missing breakpoint settings."
        If definition.Progression <> "Arithmetic" AndAlso definition.Progression <> "Geometric" Then Return "Choose Arithmetic or Geometric progression."
        If definition.StartRequirement < 1 Then Return "The starting ratio must be a positive whole number."
        If definition.Progression = "Arithmetic" AndAlso definition.StepValue <> Decimal.Truncate(definition.StepValue) Then Return "Use a whole-number arithmetic increment."
        If definition.StepValue > Integer.MaxValue Then Return "The progression step is too large."
        If definition.Progression = "Arithmetic" AndAlso definition.StepValue < 1D Then Return "The arithmetic increment must be at least 1."
        If definition.Progression = "Geometric" AndAlso definition.StepValue <= 1D Then Return "The geometric multiplier must be greater than 1."
        Return Nothing
    End Function
    Public Shared Function ValidationError(component As ComponentBlueprint) As String
        If component.BreakpointSchedules IsNot Nothing AndAlso component.BreakpointSchedules.Length <> MAX_INPUTS Then Return "Incomplete breakpoint settings."
        For i As Integer = 0 To MAX_INPUTS - 1
            Dim definition = GetDefinition(component, i)
            Dim kind = If(component.ScheduleType IsNot Nothing AndAlso i < component.ScheduleType.Length, component.ScheduleType(i), "")
            If If(kind, "").Trim().Equals("Breakpoint", StringComparison.OrdinalIgnoreCase) AndAlso kind <> "Breakpoint" Then Return "Use the canonical schedule name: Breakpoint."
            If kind = "Breakpoint" Then
                If i >= component.InputCount Then Return "A breakpoint schedule is assigned to an inactive input."
                Dim problem = ValidateDefinition(definition)
                If problem IsNot Nothing Then Return "Input " & (i + 1) & ": " & problem
                If component.LagEnabled OrElse component.YokedEnabled Then Return "Breakpoint schedules cannot be combined with Lag or Yoked mode."
                If ClassicChainConfiguration.GetChain(component, i) IsNot Nothing Then Return "Breakpoint is a separate mode, not a classic chain."
                If component.ScheduleValue Is Nothing OrElse component.ScheduleValue.Length <> MAX_INPUTS OrElse component.ScheduleValue(i) <> definition.StartRequirement Then Return "The breakpoint starting ratio does not match the input value."
            ElseIf definition IsNot Nothing Then
                Return "Unused breakpoint settings on input " & (i + 1) & "."
            End If
        Next
        Return ""
    End Function
    Public Shared Function NextRequirement(current As Integer, definition As BreakpointDefinition) As Integer
        Dim problem = ValidateDefinition(definition)
        If problem IsNot Nothing Then Throw New ArgumentException(problem)
        If current < 1 Then Throw New ArgumentOutOfRangeException(NameOf(current))
        Dim result As Decimal = If(definition.Progression = "Arithmetic", CDec(current) + definition.StepValue, CDec(current) * definition.StepValue)
        If result > Integer.MaxValue Then Throw New OverflowException("The next breakpoint ratio exceeds 2147483647 responses. The session was stopped to avoid an incorrect requirement.")
        Return CInt(Decimal.Ceiling(result))
    End Function
    Public Shared Function Summary(definition As BreakpointDefinition) As String
        If definition Is Nothing Then Return "Breakpoint (missing settings)"
        Return "Breakpoint " & definition.Progression & " " & definition.StartRequirement.ToString(CultureInfo.InvariantCulture) & If(definition.Progression = "Arithmetic", " +", " x") & definition.StepValue.ToString(CultureInfo.InvariantCulture)
    End Function
End Class
