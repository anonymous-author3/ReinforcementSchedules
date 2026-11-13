Option Strict On

<Serializable>
Public Class ReinforcerSelectionDefinition
    Public Property Feeder1Percent As Integer = 50
    Public Property Pump1Percent As Integer = 50
    Public Property Feeder2Percent As Integer
    Public Property Pump2Percent As Integer
    Public Function Copy() As ReinforcerSelectionDefinition
        Return DirectCast(MemberwiseClone(), ReinforcerSelectionDefinition)
    End Function
End Class

Friend NotInheritable Class ReinforcerConfiguration
    Public Shared ReadOnly Names As String() = ReinforcerOutputs.Names
    Private Sub New()
    End Sub
    Public Shared Function CanonicalName(kind As String) As String
        Return ReinforcerOutputs.CanonicalName(kind)
    End Function
    Public Shared Function IsConcrete(kind As String) As Boolean
        Return ReinforcerOutputs.IsConcrete(kind)
    End Function
    Public Shared Function Command(kind As String) As String
        Return ReinforcerOutputs.Command(kind)
    End Function
    Public Shared Function Percentages(definition As ReinforcerSelectionDefinition) As Integer()
        If definition Is Nothing Then Throw New ArgumentNullException(NameOf(definition))
        Return {definition.Feeder1Percent, definition.Pump1Percent, definition.Feeder2Percent, definition.Pump2Percent}
    End Function
    Public Shared Function ValidateSelection(definition As ReinforcerSelectionDefinition) As String
        If definition Is Nothing Then Return "Choose the Random reinforcer outputs and their probabilities."
        Dim values = Percentages(definition)
        If values.Any(Function(value) value < 0 OrElse value > 100) Then Return "Every output probability must be between 0 and 100%."
        If values.Sum() <> 100 Then Return "Random output probabilities must add up to 100%."
        Return ""
    End Function
    Public Shared Function Choose(definition As ReinforcerSelectionDefinition, draw As Integer) As String
        Dim problem = ValidateSelection(definition)
        If problem <> "" Then Throw New ArgumentException(problem)
        If draw < 0 OrElse draw >= 100 Then Throw New ArgumentOutOfRangeException(NameOf(draw))
        Dim cumulative As Integer
        Dim values = Percentages(definition)
        For index As Integer = 0 To values.Length - 1
            cumulative += values(index)
            If draw < cumulative Then Return Names(index)
        Next
        Throw New InvalidOperationException("Invalid reinforcer distribution.")
    End Function
    Public Shared Function Summary(definition As ReinforcerSelectionDefinition) As String
        If definition Is Nothing Then Return "Legacy Feeder 1 / Pump 1 probabilities"
        Dim values = Percentages(definition)
        Return String.Join(", ", Enumerable.Range(0, Names.Length).Where(Function(i) values(i) > 0).Select(Function(i) Names(i) & " " & values(i) & "%"))
    End Function
    Public Shared Sub NormalizeLegacy(ByRef component As ComponentBlueprint)
        If component.ReinforcerSelections Is Nothing Then component.ReinforcerSelections = New ReinforcerSelectionDefinition(MAX_INPUTS - 1) {}
    End Sub
    Public Shared Function InputSelection(component As ComponentBlueprint, input As Integer) As ReinforcerSelectionDefinition
        If component.ReinforcerSelections Is Nothing OrElse input < 0 OrElse input >= component.ReinforcerSelections.Length Then Return Nothing
        Return component.ReinforcerSelections(input)
    End Function
    Public Shared Function SameSelection(first As ReinforcerSelectionDefinition, second As ReinforcerSelectionDefinition) As Boolean
        If first Is Nothing OrElse second Is Nothing Then Return first Is Nothing AndAlso second Is Nothing
        Return Percentages(first).SequenceEqual(Percentages(second))
    End Function
    Public Shared Function ValidationError(component As ComponentBlueprint) As String
        If component.ReinforcerSelections IsNot Nothing AndAlso component.ReinforcerSelections.Length <> MAX_INPUTS Then Return "Incomplete reinforcer selections."
        For input As Integer = 0 To Math.Min(component.InputCount, MAX_INPUTS) - 1
            Dim kind = If(component.Reinforcer Is Nothing, "", component.Reinforcer(input))
            Dim selection = InputSelection(component, input)
            If kind <> "" AndAlso kind <> "N/A" AndAlso kind <> "Random" AndAlso Not IsConcrete(kind) Then Return "Unknown reinforcer on input " & (input + 1) & "."
            If selection IsNot Nothing Then
                If kind <> "Random" Then Return "Random probabilities assigned to a non-Random input."
                Dim problem = ValidateSelection(selection)
                If problem <> "" Then Return "Input " & (input + 1) & ": " & problem
            End If
        Next
        If component.TimeReinforcer <> "" AndAlso component.TimeReinforcer <> "N/A" AndAlso component.TimeReinforcer <> "Random" AndAlso Not IsConcrete(component.TimeReinforcer) Then Return "Unknown independent-time reinforcer."
        If component.TimeReinforcerSelection IsNot Nothing Then
            If component.TimeReinforcer <> "Random" Then Return "Random probabilities assigned to a non-Random time reinforcer."
            Return ValidateSelection(component.TimeReinforcerSelection)
        End If
        Return ""
    End Function
End Class
