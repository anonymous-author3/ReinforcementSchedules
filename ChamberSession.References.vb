Option Strict On

Partial Public Class ChamberSession
    Private referenceClosureWritten As Boolean
    Private Sub MarkReferenceClosed(status As String)
        If referenceClosureWritten OrElse rawWriter Is Nothing Then Return
        WriteLine(1, "ReferenceClosed", status)
        referenceClosureWritten = True
        WriteLine(2, "ReferenceClosed", status)
    End Sub

    Private deliveryBatchNumber As Long
    Private componentStartedTimestamp As Long
    Private ReadOnly yokedTrialCursor(MAX_COMPONENTS) As Integer
    Private ReadOnly yokedPresentationCursor(MAX_COMPONENTS) As Integer
    Private yokedBatches As New List(Of List(Of SessionReferenceDelivery))
    Private ReadOnly yokedCollected As New HashSet(Of Long)
    Private yokedLastPresentation As Boolean
    Private ReadOnly yokedLagDeliveries(MAX_COMPONENTS) As Dictionary(Of Long, List(Of SessionReferenceDelivery))

    Private Function IsYokedComponent() As Boolean
        Return vCC > 0 AndAlso vCC <= MAX_COMPONENTS AndAlso AC(vCC).YokedEnabled
    End Function

    Private Function MaximumReinforcersReached() As Boolean
        If IsYokedComponent() OrElse (IsLagComponent() AndAlso AC(vCC).LagMaxSequences > 0) Then Return False
        Return AC(vCC).MaxRefs > 0 AndAlso ComponentRefTotal() >= AC(vCC).MaxRefs
    End Function

    Private Sub BeginYokedComponent()
        If Not IsYokedComponent() Then Return
        Dim source = AC(vCC).YokedReference
        WriteLine(2, "Yoked reference component " & vCC & ": " & source.SourceDescription & "; UID=" & source.SessionUid & "; source component=" & source.SelectedComponent)
        WriteLine(1, vTimeNow, vCC, "YokedReference", source.SessionUid, source.SelectedComponent, If(IsLagComponent(), "Trial", "Availability"))
        If IsLagComponent() Then
            If yokedLagDeliveries(vCC) Is Nothing Then yokedLagDeliveries(vCC) = source.Deliveries.GroupBy(Function(d) d.SequenceNumber).ToDictionary(Function(g) g.Key, Function(g) g.ToList())
            Return
        End If
        Dim presentations = source.Deliveries.Select(Function(d) d.Presentation).Distinct().OrderBy(Function(p) p).ToArray()
        Dim index As Integer = yokedPresentationCursor(vCC)
        If index >= presentations.Length Then
            EndYokedSession()
            Return
        End If
        yokedPresentationCursor(vCC) += 1
        yokedLastPresentation = index = presentations.Length - 1
        Dim sourcePresentation As Integer = presentations(index)
        yokedBatches = source.Deliveries.Where(Function(d) d.Presentation = sourcePresentation).GroupBy(Function(d) d.BatchNumber).Select(Function(g) g.ToList()).OrderBy(Function(g) g(0).ComponentTimeMs).ToList()
        yokedCollected.Clear()
    End Sub

    Private Sub EndYokedSession()
        WriteRawEvent("YokedReferenceExhausted")
        WriteLine(2, "Yoked reference exhausted: session ended without recycling the reference.")
        StartPostSession()
    End Sub

    Private Sub DeliverYokedLagTrial(inputIndex As Integer)
        Dim source = AC(vCC).YokedReference
        Dim index As Integer = yokedTrialCursor(vCC)
        If index >= source.Sequences.Count Then Throw New InvalidOperationException("Yoked reference exhausted before a trial.")
        Dim trial = source.Sequences(index)
        deliveryBatchNumber += 1
        Dim delivered As Integer
        Dim units As List(Of SessionReferenceDelivery) = Nothing
        If yokedLagDeliveries(vCC).TryGetValue(trial.Number, units) Then
            For Each item In units
                If ReinforcerDelivery(item.ReinforcerType, 100, 0, inputIndex + 1, "Yoked") Then
                    delivered += 1
                    AddYokedReinforcerMarker(inputIndex)
                End If
            Next
        End If
        RefCount(vCC, inputIndex) += delivered
        RefCount_i(inputIndex) += delivered
        If delivered > 0 Then WriteRawEvent("R" & (inputIndex + 1))
        CompleteLagOutcome(If(delivered > 0, "Delivered", "Omitted"))
        WriteLine(1, vTimeNow, vCC, "YokedTrial", index + 1, trial.Number, delivered)
        yokedTrialCursor(vCC) += 1
        RefreshInputStatusTable()
        If yokedTrialCursor(vCC) >= source.Sequences.Count Then EndYokedSession()
    End Sub

    Private Sub ProcessYokedClassicResponse(inputIndex As Integer)
        ResponseCount(vCC, inputIndex) += 1
        WriteRawEvent("YokedResponse" & (inputIndex + 1))
        Dim elapsed As Double = (Stopwatch.GetTimestamp() - componentStartedTimestamp) * 1000.0 / Stopwatch.Frequency
        If elapsed >= AC(vCC).ComponentDuration * 1000.0 Then
            ComponentDuration_Code()
            Return
        End If
        If AC(vCC).FeedbackDuration(inputIndex) > 0 Then Stimulus(inputIndex)
        Dim batch = yokedBatches.FirstOrDefault(Function(items) items(0).Input = inputIndex + 1 AndAlso Not yokedCollected.Contains(items(0).BatchNumber) AndAlso items(0).ComponentTimeMs <= elapsed)
        If batch IsNot Nothing Then
            deliveryBatchNumber += 1
            Dim delivered As Integer
            For Each item In batch
                If ReinforcerDelivery(item.ReinforcerType, 100, 0, inputIndex + 1, "Yoked") Then
                    delivered += 1
                    AddYokedReinforcerMarker(inputIndex)
                End If
            Next
            yokedCollected.Add(batch(0).BatchNumber)
            RefCount(vCC, inputIndex) += delivered
            RefCount_i(inputIndex) += delivered
            WriteRawEvent("R" & (inputIndex + 1))
            WriteLine(1, vTimeNow, vCC, "YokedBatchCollected", batch(0).BatchNumber, delivered)
            If yokedCollected.Count = yokedBatches.Count Then
                If yokedLastPresentation Then
                    EndYokedSession()
                Else
                    ComponentDuration_Code()
                End If
                Return
            End If
        End If
        RefreshInputStatusTable()
    End Sub

    Private lagPendingSequenceNumber As Long
    Private lagPendingComponent As Integer
    Private lagPendingInput As Integer
    Private lagPendingDelivered As Integer

    Private Sub AddYokedReinforcerMarker(inputIndex As Integer)
        Dim name As String = ReinforcerSeriesName(inputIndex)
        If Chart1.Series.IndexOf(name) >= 0 Then Chart1.Series(name).Points.AddXY(chartTime(inputIndex), chartResponse(inputIndex))
    End Sub

    Private Function YokedStatusText(inputIndex As Integer) As String
        If IsLagComponent() Then Return "Yoked trial " & yokedTrialCursor(vCC) & "/" & AC(vCC).YokedReference.Sequences.Count & " | sequence " & If(lagTrackers(vCC) Is Nothing, 0, lagTrackers(vCC).PartialCount) & "/" & AC(vCC).LagSequenceLength
        Dim elapsed As Double = (Stopwatch.GetTimestamp() - componentStartedTimestamp) * 1000.0 / Stopwatch.Frequency
        Dim available As Integer = yokedBatches.Where(Function(items) items(0).Input = inputIndex + 1 AndAlso Not yokedCollected.Contains(items(0).BatchNumber) AndAlso items(0).ComponentTimeMs <= elapsed).Count()
        Return "Yoked batches available: " & available
    End Function

    Private Sub BeginLagOutcome(sequenceNumber As Long, inputIndex As Integer)
        If lagPendingSequenceNumber <> 0 Then Throw New InvalidOperationException("Previous Lag outcome has not been resolved.")
        lagPendingSequenceNumber = sequenceNumber
        lagPendingComponent = vCC
        lagPendingInput = inputIndex + 1
        lagPendingDelivered = 0
    End Sub

    Private Sub CompleteLagOutcome(result As String)
        If lagPendingSequenceNumber = 0 Then Return
        WriteLine(1, vTimeNow, lagPendingComponent, "LagOutcome", lagPendingSequenceNumber, lagPendingDelivered, result)
        WriteLine(2, vTimeNow, lagPendingComponent, "LagOutcome", lagPendingSequenceNumber, lagPendingDelivered, result)
        lagPendingSequenceNumber = 0
        lagPendingDelivered = 0
    End Sub

    Private Sub RecordDeliveredReinforcer(physicalInput As Integer, reinforcerType As String, source As String)
        UpdateSessionClock()
        Dim sequenceNumber As Long = 0
        If (source = "Scheduled" OrElse source = "Yoked") AndAlso lagPendingSequenceNumber > 0 AndAlso lagPendingComponent = vCC AndAlso physicalInput = lagPendingInput Then
            sequenceNumber = lagPendingSequenceNumber
            lagPendingDelivered += 1
        End If
        WriteLine(1, vTimeNow, vCC, "DeliveredReinforcer", componentPresentation, physicalInput, reinforcerType, sequenceNumber, source, deliveryBatchNumber)
        WriteLine(2, vTimeNow, vCC, "DeliveredReinforcer", componentPresentation, physicalInput, reinforcerType, sequenceNumber, source, deliveryBatchNumber)
    End Sub
End Class
