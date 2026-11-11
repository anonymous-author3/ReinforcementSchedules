Option Strict On

'One instance per component per session. Only the comparison window is retained in memory;
'every completed sequence (including failures) is written to the raw session file.
Friend NotInheritable Class LagSequenceTracker
    Private ReadOnly sequenceLength As Integer
    Private ReadOnly lag As Integer
    Private ReadOnly pendingSequence As New List(Of Integer)
    Private ReadOnly history As New Queue(Of String)
    Private ReadOnly occurrences As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
    Public Property CompletedCount As Long
        Get
            Return completed
        End Get
        Private Set(value As Long)
            completed = value
        End Set
    End Property
    Private completed As Long
    Public ReadOnly Property PartialCount As Integer
        Get
            Return pendingSequence.Count
        End Get
    End Property
    Public ReadOnly Property HistoryCount As Integer
        Get
            Return history.Count
        End Get
    End Property

    Public Sub New(responseCount As Integer, lagValue As Integer)
        If responseCount < 1 OrElse responseCount > 10000 Then Throw New ArgumentOutOfRangeException(NameOf(responseCount))
        If lagValue < 0 OrElse lagValue > 100000 Then Throw New ArgumentOutOfRangeException(NameOf(lagValue))
        sequenceLength = responseCount
        lag = lagValue
    End Sub

    Public Sub SeedHistory(sequences As IEnumerable(Of String))
        If CompletedCount <> 0 OrElse pendingSequence.Count <> 0 OrElse history.Count <> 0 Then Throw New InvalidOperationException("History can only be seeded before the session starts.")
        Dim values As String() = sequences.ToArray()
        If values.Any(Function(value) String.IsNullOrEmpty(value) OrElse value.Length <> sequenceLength OrElse value.Any(Function(digit) digit < "1"c OrElse digit > "6"c)) Then Throw New ArgumentException("Invalid prior sequence history.")
        For Each value As String In values.Skip(Math.Max(0, values.Length - lag))
            history.Enqueue(value)
            If Not occurrences.ContainsKey(value) Then occurrences.Add(value, 0)
            occurrences(value) += 1
        Next
    End Sub

    Public Function AddResponse(physicalInput As Integer) As LagSequenceResult
        If physicalInput < 1 OrElse physicalInput > MAX_INPUTS Then Throw New ArgumentOutOfRangeException(NameOf(physicalInput))
        pendingSequence.Add(physicalInput)
        If pendingSequence.Count < sequenceLength Then Return Nothing
        Dim sequence As String = String.Join("", pendingSequence)
        'Compare before insertion. Failed complete sequences enter the same moving window.
        Dim result As New LagSequenceResult(sequence, Not occurrences.ContainsKey(sequence), history.Count, CompletedCount + 1)
        CompletedCount += 1
        pendingSequence.Clear()
        If lag > 0 Then
            history.Enqueue(sequence)
            If Not occurrences.ContainsKey(sequence) Then occurrences.Add(sequence, 0)
            occurrences(sequence) += 1
            If history.Count > lag Then
                Dim expired As String = history.Dequeue()
                occurrences(expired) -= 1
                If occurrences(expired) = 0 Then occurrences.Remove(expired)
            End If
        End If
        Return result
    End Function

    Public Function DiscardPartial() As String
        Dim value As String = String.Join("", pendingSequence)
        pendingSequence.Clear()
        Return value
    End Function
End Class

Friend NotInheritable Class LagSequenceResult
    Public ReadOnly Property Sequence As String
    Public ReadOnly Property MeetsCriterion As Boolean
    Public ReadOnly Property ComparedCount As Integer
    Public ReadOnly Property SequenceNumber As Long
    Public Sub New(value As String, passed As Boolean, compared As Integer, number As Long)
        Sequence = value
        MeetsCriterion = passed
        ComparedCount = compared
        SequenceNumber = number
    End Sub
End Class

Friend NotInheritable Class LagConfiguration
    Private Sub New()
    End Sub
    Private Shared Function ValidSeconds(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value >= 0 AndAlso value <= Integer.MaxValue / 1000.0
    End Function
    Public Shared Function ValidationError(component As ComponentBlueprint) As String
        Dim anyLag As Boolean = component.ScheduleType IsNot Nothing AndAlso component.ScheduleType.Any(Function(value) String.Equals(value, "Lag", StringComparison.OrdinalIgnoreCase))
        If Not component.LagEnabled Then Return If(anyLag, "Lag inputs require Lag component settings.", "")
        If Not ValidSeconds(component.ComponentDuration) OrElse component.ComponentDuration <= 0 Then Return "Lag component duration must be positive and finite within the timer limit."
        If component.ComponentIteration = 0 Then Return "A Lag component needs at least one iteration."
        If component.LagMaxSequences < 0 Then Return "Maximum Lag sequences cannot be negative."
        If component.MaxRefs < 0 Then Return "Maximum reinforcers cannot be negative."
        If Not ValidSeconds(component.ComponentLightIntermittency) OrElse Not ValidSeconds(component.ComponentStimDuration) Then Return "Component stimulus durations must be finite, nonnegative values within the timer limit."
        If component.LagSequenceLength < 1 OrElse component.LagSequenceLength > 10000 Then Return "Responses per sequence must be between 1 and 10000."
        If component.LagValue < 0 OrElse component.LagValue > 100000 Then Return "Lag must be between 0 and 100000."
        If Not ValidSeconds(component.LagTimeoutSeconds) OrElse (component.LagUseTimeout AndAlso component.LagTimeoutSeconds <= 0) Then Return "Lag timeout must be a positive, finite duration within the timer limit."
        If component.COD <> 0 Then Return "COD is not used with Lag schedules."
        If Not String.IsNullOrEmpty(component.TimeScheduleType) AndAlso component.TimeScheduleType <> "None" Then Return "Independent FT/VT schedules cannot run inside a Lag component."
        If component.InputCount < 2 OrElse component.InputCount > MAX_INPUTS Then Return "Select at least two inputs for the Lag sequence."
        For Each field In GetType(ComponentBlueprint).GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public)
            If Not field.FieldType.IsArray OrElse field.Name = NameOf(ComponentBlueprint.ComponentDuration_measured) Then Continue For
            Dim values As Array = TryCast(field.GetValue(component), Array)
            If values Is Nothing OrElse values.Length <> MAX_INPUTS Then Return "Incomplete Lag input configuration: " & field.Name & "."
        Next
        Dim first As Integer = -1
        Dim count As Integer
        For i As Integer = 0 To MAX_INPUTS - 1
            Dim schedule As String = If(component.ScheduleType(i), "")
            If schedule = "N/A" Then Continue For
            If i >= component.InputCount OrElse schedule <> "Lag" Then Return "A Lag component may contain only Lag inputs or N/A slots."
            count += 1
            If component.Magnitude(i) <= 0 OrElse Not {"Pellet", "Liquid", "Random"}.Contains(component.Reinforcer(i)) Then Return "Select a reinforcer and a positive magnitude for Lag."
            If component.DeliveryP(i) < 0 OrElse component.DeliveryP(i) > 100 OrElse component.PelletP(i) < 0 OrElse component.PelletP(i) > 100 Then Return "Reinforcer probabilities must be between 0 and 100."
            If Not ValidSeconds(component.FeedbackDuration(i)) OrElse Not ValidSeconds(component.DelayDuration(i)) OrElse Not ValidSeconds(component.DelaySignalDuration(i)) Then Return "Feedback and delay durations must be finite, nonnegative values within the timer limit."
            If If(component.FeedbackType(i), "").Contains("Time Out") Then Return "Use the Lag failure timeout instead of response-feedback Time Out."
            If first < 0 Then
                first = i
            ElseIf component.Magnitude(i) <> component.Magnitude(first) OrElse component.Reinforcer(i) <> component.Reinforcer(first) OrElse component.DeliveryP(i) <> component.DeliveryP(first) OrElse component.PelletP(i) <> component.PelletP(first) OrElse component.DelayDuration(i) <> component.DelayDuration(first) OrElse component.DelaySignalDuration(i) <> component.DelaySignalDuration(first) OrElse component.DelayType(i) <> component.DelayType(first) OrElse component.DelayRetract(i) <> component.DelayRetract(first) Then
                Return "All Lag inputs must share the same sequence reinforcer and delay settings."
            End If
        Next
        If count < 2 Then Return "Select at least two inputs for the Lag sequence."
        If component.LagHistoryReference IsNot Nothing Then
            Dim historyError As String = component.LagHistoryReference.ValidateHistory(component.LagSequenceLength, Enumerable.Range(1, MAX_INPUTS).Where(Function(input) component.ScheduleType(input - 1) = "Lag"))
            If historyError <> "" Then Return historyError
        End If
        Return ""
    End Function
End Class
