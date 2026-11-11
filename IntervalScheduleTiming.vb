Option Strict On

'Pure interval calculations: seconds remain Double until the timer's millisecond boundary.
Friend NotInheritable Class IntervalScheduleTiming
    Public Const VariableIntervalCount As Integer = 10
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

    Public Shared Function GenerateVariableSeconds(meanSeconds As Double) As List(Of Double)
        Dim values As New List(Of Double)(VariableIntervalCount)
        Dim n As Integer = VariableIntervalCount
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

    Public Shared Function ValidationError(scheduleType As String, value As Integer) As String
        Dim kind As String = If(scheduleType, "").Trim().ToLowerInvariant()
        Dim maximum As Integer
        If kind = "fixed interval" Then
            maximum = MaximumFixedSeconds
        ElseIf kind.Contains("variable interval") Then
            maximum = MaximumVariableMeanSeconds
        Else
            Return ""
        End If
        If value < 1 OrElse value > maximum Then
            Return "Enter a whole-number interval value from 1 to " & maximum & " seconds for " & scheduleType & "."
        End If
        Return ""
    End Function

    Public Shared Function ValidationError(component As ComponentBlueprint) As String
        If component.ScheduleType Is Nothing OrElse component.ScheduleValue Is Nothing Then
            Return "Missing input schedule settings."
        End If
        If component.InputCount < 0 OrElse component.InputCount > component.ScheduleType.Length OrElse
            component.InputCount > component.ScheduleValue.Length Then Return "Incomplete input schedule settings."
        For i As Integer = 0 To component.InputCount - 1
            Dim problem As String = ValidationError(component.ScheduleType(i), component.ScheduleValue(i))
            If problem <> "" Then Return "Input " & (i + 1) & ": " & problem
        Next
        Return ""
    End Function
End Class
