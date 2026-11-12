Option Strict On

'Pure interval calculations: seconds remain Double until the timer's millisecond boundary.
Friend NotInheritable Class IntervalScheduleTiming
    Public Const VariableIntervalCount As Integer = 10
    Public Const MaximumVariableCount As Integer = 1000
    Public Const MaximumFixedSeconds As Integer = Integer.MaxValue \ 1000
    Public Shared ReadOnly MaximumVariableMeanSeconds As Integer =
        CInt(Math.Floor((Integer.MaxValue / 1000.0) / (1.0 + Math.Log(VariableIntervalCount))))

    Private Sub New()
    End Sub

    Public Shared Function ToTimerMilliseconds(seconds As Double) As Integer
        If Double.IsNaN(seconds) OrElse Double.IsInfinity(seconds) OrElse
            seconds < 0.001 OrElse seconds > Integer.MaxValue / 1000.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(seconds), "An interval must fit the timer range of 1 to 2147483647 milliseconds.")
        End If
        'One explicit rounding step, never whole-second rounding and never an added second.
        Return CInt(Math.Round(seconds * 1000.0, MidpointRounding.AwayFromZero))
    End Function

    Public Shared Function GenerateVariableSeconds(meanSeconds As Double, Optional count As Integer = VariableIntervalCount) As List(Of Double)
        ValidateCount(count)
        Dim values As New List(Of Double)(count)
        Dim n As Integer = count
        For m As Integer = 1 To n
            Dim seconds As Double
            If m = n Then
                seconds = meanSeconds * (1.0 + Math.Log(n))
            Else
                seconds = meanSeconds * (1.0 + Math.Log(n) +
                    (n - m) * Math.Log(n - m) - (n - m + 1) * Math.Log(n - m + 1))
            End If
            'Reject unsupported requests instead of silently clamping their distribution.
            ToTimerMilliseconds(seconds)
            values.Add(seconds)
        Next
        Return values
    End Function

    Public Shared Function ParseWholeSeconds(scheduleType As String, text As String) As Integer
        Dim value As Integer
        If Not Integer.TryParse(text, value) Then
            Throw New ArgumentException("Enter a whole-number interval value in seconds for " & scheduleType & ".")
        End If
        Dim problem As String = ValidationError(scheduleType, value)
        If problem <> "" Then Throw New ArgumentException(problem)
        Return value
    End Function

    Public Shared Function ValidationError(scheduleType As String, value As Integer, Optional count As Integer = VariableIntervalCount) As String
        Dim kind As String = If(scheduleType, "").Trim().ToLowerInvariant()
        Dim maximum As Integer
        If kind = "fixed interval" Then
            maximum = MaximumFixedSeconds
        ElseIf kind.Contains("variable interval") Then
            If count < 1 OrElse count > MaximumVariableCount Then Return "Choose 1 to " & MaximumVariableCount & " values per variable series."
            maximum = CInt(Math.Floor((Integer.MaxValue / 1000.0) / (1.0 + Math.Log(count))))
        ElseIf kind = "variable ratio" Then
            If count < 1 OrElse count > MaximumVariableCount Then Return "Choose 1 to " & MaximumVariableCount & " values per variable series."
            maximum = CInt(Math.Floor((Integer.MaxValue - 1.0) / (1.0 + Math.Log(count))))
            If value < 1 OrElse value > maximum Then Return "Variable ratio mean must be a whole number from 1 to " & maximum & "."
            Return ""
        Else
            Return ""
        End If
        If value < 1 OrElse value > maximum Then
            Return "Enter a whole-number interval value from 1 to " & maximum & " seconds for " & scheduleType & "."
        End If
        If kind.Contains("variable interval") Then
            Try
                GenerateVariableSeconds(value, count)
            Catch ex As ArgumentException
                Return "This mean and series count produce an interval outside the timer range. Increase the mean or reduce the number of values."
            End Try
        End If
        Return ""
    End Function

    Public Shared Function ValidationError(component As ComponentBlueprint) As String
        If component.ScheduleType Is Nothing OrElse component.ScheduleValue Is Nothing Then
            Return "Missing input schedule settings."
        End If
        If component.InputCount < 0 OrElse component.InputCount > component.ScheduleType.Length OrElse
            component.InputCount > component.ScheduleValue.Length Then Return "Incomplete input schedule settings."
        If component.VariableValueCount IsNot Nothing AndAlso component.VariableValueCount.Length <> MAX_INPUTS Then Return "Incomplete variable series counts."
        For i As Integer = 0 To component.InputCount - 1
            Dim problem As String = ValidationError(component.ScheduleType(i), component.ScheduleValue(i), InputCount(component, i))
            If problem <> "" Then Return "Input " & (i + 1) & ": " & problem
        Next
        Return TimeValidationError(component.TimeScheduleType, component.TimeScheduleValue, TimeCount(component))
    End Function
    Public Shared Sub ValidateCount(count As Integer)
        If count < 1 OrElse count > MaximumVariableCount Then Throw New ArgumentOutOfRangeException(NameOf(count), "Choose 1 to 1000 values per variable series.")
    End Sub

    Public Shared Function InputCount(component As ComponentBlueprint, index As Integer) As Integer
        If component.VariableValueCount Is Nothing Then Return VariableIntervalCount
        Return component.VariableValueCount(index)
    End Function

    Public Shared Function TimeCount(component As ComponentBlueprint) As Integer
        Return If(component.TimeVariableValueCount = 0, VariableIntervalCount, component.TimeVariableValueCount)
    End Function

    Public Shared Sub NormalizeLegacyCounts(ByRef component As ComponentBlueprint)
        If component.VariableValueCount Is Nothing Then component.VariableValueCount = Enumerable.Repeat(VariableIntervalCount, MAX_INPUTS).ToArray()
        If component.TimeVariableValueCount = 0 Then component.TimeVariableValueCount = VariableIntervalCount
    End Sub

    Public Shared Function TimeValidationError(kind As String, value As Double, count As Integer) As String
        kind = If(kind, "").Trim().ToLowerInvariant()
        If kind <> "fixed time" AndAlso kind <> "variable time" Then Return ""
        Try
            If kind = "variable time" Then
                GenerateVariableSeconds(value, count)
            Else
                ToTimerMilliseconds(value)
            End If
        Catch ex As ArgumentException
            Return "Invalid time schedule: " & ex.Message
        End Try
        Return ""
    End Function

    Public Shared Function GenerateVariableRatios(mean As Integer, Optional count As Integer = VariableIntervalCount) As List(Of Integer)
        ValidateCount(count)
        Dim problem = ValidationError("Variable Ratio", mean, count)
        If problem <> "" Then Throw New ArgumentOutOfRangeException(NameOf(mean), problem)
        'Generate dimensionless Fleshler-Hoffman values, then quantize to positive
        'integers while preserving the exact total mean * count. Zero-response
        'ratios would violate the response contingency and are never generated.
        Dim raw As New List(Of Double)(count)
        For m As Integer = 1 To count
            Dim value = CDbl(mean) * (1.0 + Math.Log(count))
            If m < count Then value += CDbl(mean) * ((count - m) * Math.Log(count - m) - (count - m + 1) * Math.Log(count - m + 1))
            raw.Add(value)
        Next
        Dim result = raw.Select(Function(value) Math.Max(1, CInt(Math.Floor(value)))).ToList()
        Dim difference As Long = CLng(mean) * count - result.Sum(Function(value) CLng(value))
        While difference <> 0
            Dim selected As Integer = -1
            Dim best As Double = Double.NegativeInfinity
            For i As Integer = 0 To count - 1
                If difference < 0 AndAlso result(i) <= 1 Then Continue For
                Dim score = If(difference > 0, raw(i) - result(i), result(i) - raw(i))
                If score > best Then
                    best = score
                    selected = i
                End If
            Next
            If selected < 0 Then Throw New InvalidOperationException("Cannot quantize the requested ratio series.")
            Dim stepValue As Integer = If(difference > 0, 1, -1)
            result(selected) += stepValue
            difference -= stepValue
        End While
        Return result
    End Function
End Class
