Option Strict On
Imports System.IO
Imports System.Globalization
Imports Microsoft.VisualBasic.FileIO

<Serializable>
Public NotInheritable Class SessionReferenceSequence
    Public Property Component As Integer
    Public Property Presentation As Integer
    Public Property Number As Long
    Public Property TimeMs As Long
    Public Property Value As String
    Public Property CriterionPassed As Boolean
    Public Property DeliveredCount As Integer?
    Public Property Outcome As String
    Public Function Copy() As SessionReferenceSequence
        Return DirectCast(MemberwiseClone(), SessionReferenceSequence)
    End Function
End Class

<Serializable>
Public NotInheritable Class SessionReferenceDelivery
    Public Property Component As Integer
    Public Property Presentation As Integer
    Public Property Input As Integer
    Public Property TimeMs As Long
    Public Property ComponentTimeMs As Long
    Public Property SequenceNumber As Long
    Public Property ReinforcerType As String
    Public Property Count As Integer = 1
    Public Property Source As String
    Public Property BatchNumber As Long
    Public Function Copy() As SessionReferenceDelivery
        Return DirectCast(MemberwiseClone(), SessionReferenceDelivery)
    End Function
End Class

<Serializable>
Public NotInheritable Class SessionReferencePresentation
    Public Property Number As Integer
    Public Property Component As Integer
    Public Property StartTimeMs As Long
    Public Property EndTimeMs As Long?
    Public Function Copy() As SessionReferencePresentation
        Return DirectCast(MemberwiseClone(), SessionReferencePresentation)
    End Function
End Class
'Only explicit delivered-command records establish reinforcement. Criterion Pass never does.
<Serializable>
Public NotInheritable Class SessionReferenceData
    Public Property SessionUid As String = ""
    Public Property Subject As String = ""
    Public Property SessionName As String = ""
    Public Property SourceDescription As String = ""
    Public Property CompletionStatus As String = ""
    Public Property SelectedComponent As Integer
    Public Property HasExactOutcomes As Boolean
    Public Property HasExactDeliveries As Boolean
    Public Property Sequences As New List(Of SessionReferenceSequence)
    Public Property Deliveries As New List(Of SessionReferenceDelivery)
    Public Property Presentations As New List(Of SessionReferencePresentation)
    Public Function Copy() As SessionReferenceData
        Dim result = DirectCast(MemberwiseClone(), SessionReferenceData)
        result.Sequences = Sequences.Select(Function(s) s.Copy()).ToList()
        result.Deliveries = Deliveries.Select(Function(d) d.Copy()).ToList()
        result.Presentations = Presentations.Select(Function(p) p.Copy()).ToList()
        Return result
    End Function
    Public Function SelectComponent(number As Integer) As SessionReferenceData
        If number < 1 OrElse number > 10 Then Throw New InvalidDataException("Invalid reference component.")
        Dim result = Copy()
        result.SelectedComponent = number
        result.Sequences = result.Sequences.Where(Function(s) s.Component = number).ToList()
        result.Deliveries = result.Deliveries.Where(Function(d) d.Component = number).ToList()
        result.Presentations = result.Presentations.Where(Function(p) p.Component = number).ToList()
        result.HasExactOutcomes = HasExactDeliveries AndAlso result.Sequences.Count > 0 AndAlso result.Sequences.All(Function(s) s.DeliveredCount.HasValue)
        Return result
    End Function
    Public Function ValidateIntegrity() As String
        Dim uid As Guid
        If Not Guid.TryParseExact(SessionUid, "N", uid) Then Return "Reference session UID is missing or invalid."
        If CompletionStatus Is Nothing OrElse Not {"", "Completed", "Error", "Disposed"}.Contains(CompletionStatus) Then Return "Reference closure status is invalid."
        If Subject Is Nothing OrElse SessionName Is Nothing OrElse SourceDescription Is Nothing Then Return "Reference metadata is incomplete."
        If SelectedComponent < 0 OrElse SelectedComponent > 10 OrElse Sequences Is Nothing OrElse Deliveries Is Nothing OrElse Presentations Is Nothing Then Return "Reference component or event lists are invalid."
        If Sequences.Count > 1000000 OrElse Deliveries.Count > 1000000 Then Return "Reference contains too many events."
        Dim presentationMap As New Dictionary(Of Integer, SessionReferencePresentation)
        Dim priorNumber As Integer
        Dim priorStart As Long = -1
        For Each entry In Presentations
            If entry Is Nothing OrElse entry.Number <= priorNumber OrElse entry.Component < 1 OrElse entry.Component > 10 OrElse (SelectedComponent > 0 AndAlso entry.Component <> SelectedComponent) OrElse entry.StartTimeMs < 0 OrElse entry.StartTimeMs < priorStart OrElse (entry.EndTimeMs.HasValue AndAlso entry.EndTimeMs.Value < entry.StartTimeMs) Then Return "Reference presentation boundaries are invalid."
            presentationMap.Add(entry.Number, entry)
            priorNumber = entry.Number
            priorStart = entry.StartTimeMs
        Next
        If HasExactDeliveries AndAlso Presentations.Count = 0 Then Return "Reference presentation boundaries are missing."
        Dim deliveryCounts = Deliveries.Where(Function(d) d IsNot Nothing).GroupBy(Function(d) d.Component & ":" & d.SequenceNumber).ToDictionary(Function(g) g.Key, Function(g) g.Count())
        Dim seen As New HashSet(Of String)
        Dim numbers As New Dictionary(Of Integer, Long)
        Dim last As Long = -1
        For Each item In Sequences
            If item Is Nothing OrElse item.Component < 1 OrElse item.Component > 10 OrElse (SelectedComponent > 0 AndAlso item.Component <> SelectedComponent) OrElse item.Presentation < 1 OrElse item.Number < 1 OrElse item.TimeMs < last Then Return "Reference sequence ordering or component is invalid."
            If Presentations.Count > 0 Then
                Dim boundary As SessionReferencePresentation = Nothing
                If Not presentationMap.TryGetValue(item.Presentation, boundary) OrElse boundary.Component <> item.Component OrElse item.TimeMs < boundary.StartTimeMs OrElse (boundary.EndTimeMs.HasValue AndAlso item.TimeMs > boundary.EndTimeMs.Value) Then Return "Reference sequence falls outside its presentation."
            End If
            If item.Value Is Nothing OrElse item.Value.Length < 1 OrElse item.Value.Length > 10000 OrElse item.Value.Any(Function(c) c < "1"c OrElse c > "6"c) Then Return "Reference physical-input sequence is invalid."
            If numbers.ContainsKey(item.Component) AndAlso item.Number <= numbers(item.Component) Then Return "Reference sequence numbers are not increasing."
            numbers(item.Component) = item.Number
            seen.Add(item.Component & ":" & item.Number)
            last = item.TimeMs
            If item.DeliveredCount.HasValue Then
                If item.DeliveredCount.Value < 0 OrElse item.Outcome Is Nothing Then Return "Reference outcome is invalid."
                If Not {"Delivered", "Omitted", "Cancelled"}.Contains(item.Outcome) AndAlso Not item.Outcome.StartsWith("Cancelled:", StringComparison.Ordinal) Then Return "Reference outcome is unknown."
                If (item.Outcome = "Delivered" AndAlso item.DeliveredCount.Value = 0) OrElse (item.Outcome = "Omitted" AndAlso item.DeliveredCount.Value <> 0) Then Return "Reference outcome and delivery count disagree."
                Dim delivered As Integer
                deliveryCounts.TryGetValue(item.Component & ":" & item.Number, delivered)
                If item.DeliveredCount.Value <> delivered Then Return "Reference outcome and delivery records disagree."
            End If
        Next
        last = -1
        For Each item In Deliveries
            If item Is Nothing OrElse item.Component < 1 OrElse item.Component > 10 OrElse (SelectedComponent > 0 AndAlso item.Component <> SelectedComponent) OrElse item.Presentation < 1 OrElse item.TimeMs < last OrElse item.ComponentTimeMs < 0 OrElse item.ComponentTimeMs > item.TimeMs OrElse item.Input < 0 OrElse item.Input > 6 OrElse item.Count <> 1 Then Return "Reference delivery timing, count or component is invalid."
            If Presentations.Count > 0 Then
                Dim boundary As SessionReferencePresentation = Nothing
                If Not presentationMap.TryGetValue(item.Presentation, boundary) OrElse boundary.Component <> item.Component OrElse item.TimeMs < boundary.StartTimeMs OrElse item.ComponentTimeMs <> item.TimeMs - boundary.StartTimeMs OrElse (boundary.EndTimeMs.HasValue AndAlso item.TimeMs > boundary.EndTimeMs.Value) Then Return "Reference delivery falls outside its presentation."
            End If
            If Not ReinforcerOutputs.IsConcrete(item.ReinforcerType) OrElse Not {"Scheduled", "Time", "Manual", "Yoked"}.Contains(item.Source) OrElse item.SequenceNumber < 0 OrElse item.BatchNumber < 0 Then Return "Reference delivery type or source is invalid."
            If item.SequenceNumber > 0 AndAlso (item.Source = "Manual" OrElse Not seen.Contains(item.Component & ":" & item.SequenceNumber)) Then Return "Reference delivery has no matching trial."
            last = item.TimeMs
        Next
        If HasExactOutcomes AndAlso (Not HasExactDeliveries OrElse Sequences.Count = 0 OrElse Sequences.Any(Function(s) Not s.DeliveredCount.HasValue)) Then Return "Reference has incomplete reinforcement outcomes."
        Return ""
    End Function
    Public Function ValidateExactOutcomes() As String
        Dim errorText = ValidateIntegrity()
        If errorText <> "" Then Return errorText
        If Not HasExactOutcomes Then Return "This reference does not contain explicit, complete reinforcement outcomes. Use a newly recorded session."
        Return ""
    End Function
    Public Function ValidateYoked(lagMode As Boolean) As String
        Dim errorText = ValidateIntegrity()
        If errorText <> "" Then Return errorText
        If SelectedComponent < 1 Then Return "Choose a reference component."
        If CompletionStatus <> "Completed" Then Return "Yoked requires a reference session closed normally as Completed. Active, truncated, failed or disposed sessions cannot be used."
        If Not HasExactDeliveries Then Return "This historical file does not identify actual reinforcement commands. Use a newly recorded reference session."
        If Deliveries.Any(Function(d) d.Source = "Manual") Then Return "Manual deliveries cannot be used in a yoked reference."
        If lagMode Then
            If Sequences.Any(Function(s) s.Outcome IsNot Nothing AndAlso s.Outcome.StartsWith("Cancelled", StringComparison.Ordinal) AndAlso s.DeliveredCount.GetValueOrDefault() > 0) Then Return "The reference contains a partially delivered, cancelled trial."
            Return ValidateExactOutcomes()
        End If
        If Presentations.Count <> 1 Then Return "Classic/T Yoked currently supports exactly one presentation of the selected reference component. Repeated presentations, including presentations without reinforcement, are not supported."
        If Deliveries.Count = 0 Then Return "This component contains no recorded reinforcer deliveries."
        If Deliveries.Any(Function(d) d.Input = 0) Then Return "FT/VT deliveries have no response input and cannot be used for response-contingent Classic yoking."
        If Deliveries.Any(Function(d) d.BatchNumber <= 0) Then Return "This historical file does not identify reinforcement batches. Use a newly recorded reference session."
        Dim lastBatch As Long
        For Each delivery In Deliveries
            If delivery.BatchNumber < lastBatch Then Return "Reference batches are not in their recorded order."
            lastBatch = delivery.BatchNumber
        Next
        For Each batch In Deliveries.GroupBy(Function(d) d.BatchNumber)
            Dim first = batch.First()
            If batch.Any(Function(d) d.Component <> first.Component OrElse d.Presentation <> first.Presentation OrElse d.Input <> first.Input OrElse d.SequenceNumber <> first.SequenceNumber OrElse d.Source <> first.Source) Then Return "A reference batch spans incompatible deliveries."
        Next
        Return ""
    End Function
    Public Function ValidateHistory(length As Integer, inputs As IEnumerable(Of Integer)) As String
        Dim errorText = ValidateIntegrity()
        If errorText <> "" Then Return errorText
        If SelectedComponent < 1 OrElse Sequences Is Nothing OrElse Sequences.Count = 0 Then Return "Choose a reference component containing completed sequences."
        Dim allowed = New HashSet(Of Integer)(inputs)
        If Sequences.Any(Function(s) s Is Nothing OrElse s.Value Is Nothing OrElse s.Value.Length <> length OrElse s.Value.Any(Function(c) Not allowed.Contains(AscW(c) - AscW("0"c)))) Then Return "Reference sequence length or physical inputs do not match this Lag component."
        Return ""
    End Function
    Public Function ComponentNumbers() As IEnumerable(Of Integer)
        Return Sequences.Select(Function(s) s.Component).Union(Deliveries.Select(Function(d) d.Component)).Union(Presentations.Select(Function(p) p.Component)).OrderBy(Function(n) n).ToArray()
    End Function
    Public Function LastSequences(count As Integer) As IEnumerable(Of String)
        If count < 0 Then Throw New ArgumentOutOfRangeException(NameOf(count))
        Return Sequences.Skip(Math.Max(0, Sequences.Count - count)).Select(Function(s) s.Value).ToArray()
    End Function
    Private Shared Function Number(text As String, maximum As Long) As Long
        Dim result As Long
        If Not Long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, result) OrElse result < 0 OrElse result > maximum Then Throw New InvalidDataException("Invalid numeric reference field.")
        Return result
    End Function
    Private Shared Sub SequenceValue(value As String)
        If String.IsNullOrEmpty(value) OrElse value.Length > 10000 OrElse value.Any(Function(c) c < "1"c OrElse c > "6"c) Then Throw New InvalidDataException("Invalid physical-input sequence.")
    End Sub
    Private Shared Function Rows(text As String) As List(Of String())
        If text Is Nothing OrElse text.Length > 32 * 1024 * 1024 Then Throw New InvalidDataException("Reference file is missing or larger than 32 MB.")
        Dim result As New List(Of String())
        Using parser As New TextFieldParser(New StringReader(text))
            parser.SetDelimiters(",")
            parser.HasFieldsEnclosedInQuotes = True
            parser.TrimWhiteSpace = False
            While Not parser.EndOfData
                result.Add(parser.ReadFields())
            End While
        End Using
        Return result
    End Function
    Private Shared Sub Metadata(result As SessionReferenceData, rows As IEnumerable(Of String()))
        For Each row In rows
            If row.Length <> 1 Then Continue For
            Dim value = row(0)
            If value.StartsWith("Session UID: ", StringComparison.Ordinal) Then
                Dim uid = value.Substring(13)
                If result.SessionUid <> "" AndAlso result.SessionUid <> uid Then Throw New InvalidDataException("Reference raw and summary session IDs do not match.")
                result.SessionUid = uid
            ElseIf value.StartsWith("Subject: ", StringComparison.Ordinal) Then
                result.Subject = value.Substring(9)
            ElseIf value.StartsWith("Session: ", StringComparison.Ordinal) Then
                result.SessionName = value.Substring(9)
            End If
        Next
    End Sub
    Public Shared Function ParseRaw(raw As String, summary As String, Optional source As String = "") As SessionReferenceData
        Dim result As New SessionReferenceData With {.SourceDescription = source}
        Dim records = Rows(raw)
        Metadata(result, records)
        If Not String.IsNullOrEmpty(summary) Then Metadata(result, Rows(summary))
        For Each row In records
            If row.Length > 0 AndAlso row(0) = "ReferenceClosed" Then
                If row.Length <> 2 OrElse Not {"Completed", "Error", "Disposed"}.Contains(row(1)) Then Throw New InvalidDataException("Invalid reference closure record.")
                If result.CompletionStatus <> "" AndAlso result.CompletionStatus <> row(1) Then Throw New InvalidDataException("Conflicting reference closure records.")
                result.CompletionStatus = row(1)
            End If
        Next
        Dim version2 = records.Any(Function(r) r.Length = 2 AndAlso r(0) = "ReferenceFormat" AndAlso r(1) = "2")
        Dim presentation As Integer
        Dim starts As New Dictionary(Of Integer, Long)
        Dim sequences As New Dictionary(Of String, SessionReferenceSequence)
        Dim lastTime As Long
        For Each row In records
            If row.Length < 3 Then Continue For
            Dim code = row(2)
            Dim relevant = code.StartsWith("StartComponent", StringComparison.Ordinal) OrElse code.StartsWith("EndComponent", StringComparison.Ordinal) OrElse code = "LagSequence" OrElse code = "LagOutcome" OrElse code = "DeliveredReinforcer"
            If Not relevant Then Continue For
            Dim time = Number(row(0), Long.MaxValue)
            Dim component = CInt(Number(row(1), 10))
            If component = 0 OrElse time < lastTime Then Throw New InvalidDataException("Invalid component or nonchronological reference events.")
            lastTime = time
            If code = "StartComponent" & component.ToString(CultureInfo.InvariantCulture) Then
                presentation += 1
                starts(component) = time
                result.Presentations.Add(New SessionReferencePresentation With {.Number = presentation, .Component = component, .StartTimeMs = time})
            ElseIf code = "EndComponent" & component.ToString(CultureInfo.InvariantCulture) Then
                Dim boundary = result.Presentations.LastOrDefault()
                If boundary Is Nothing OrElse boundary.Component <> component OrElse boundary.EndTimeMs.HasValue Then Throw New InvalidDataException("Component end has no matching open presentation.")
                boundary.EndTimeMs = time
            ElseIf code = "LagSequence" Then
                If row.Length <> 8 OrElse (row(5) <> "Pass" AndAlso row(5) <> "Fail") Then Throw New InvalidDataException("Invalid Lag sequence record.")
                SequenceValue(row(4))
                Dim item As New SessionReferenceSequence With {.Component = component, .Presentation = presentation, .TimeMs = time, .Number = Number(row(3), Long.MaxValue), .Value = row(4), .CriterionPassed = row(5) = "Pass"}
                Dim key = component & ":" & item.Number
                If item.Number = 0 OrElse sequences.ContainsKey(key) Then Throw New InvalidDataException("Duplicate or invalid Lag sequence number.")
                sequences.Add(key, item)
                result.Sequences.Add(item)
            ElseIf code = "LagOutcome" Then
                If row.Length <> 6 Then Throw New InvalidDataException("Invalid Lag outcome record.")
                Dim key = component & ":" & Number(row(3), Long.MaxValue)
                Dim item As SessionReferenceSequence = Nothing
                If Not sequences.TryGetValue(key, item) OrElse item.DeliveredCount.HasValue Then Throw New InvalidDataException("Missing sequence or duplicate Lag outcome.")
                item.DeliveredCount = CInt(Number(row(4), Integer.MaxValue))
                item.Outcome = row(5)
                If (Not {"Delivered", "Omitted", "Cancelled"}.Contains(item.Outcome) AndAlso Not item.Outcome.StartsWith("Cancelled:", StringComparison.Ordinal)) OrElse (item.Outcome = "Delivered" AndAlso item.DeliveredCount.Value = 0) OrElse (item.Outcome = "Omitted" AndAlso item.DeliveredCount.Value <> 0) Then Throw New InvalidDataException("Inconsistent Lag outcome.")
            ElseIf code = "DeliveredReinforcer" Then
                If (row.Length <> 8 AndAlso row.Length <> 9) OrElse Not starts.ContainsKey(component) OrElse Not ReinforcerOutputs.IsConcrete(row(5)) Then Throw New InvalidDataException("Invalid delivered reinforcer record.")
                Dim item As New SessionReferenceDelivery With {.TimeMs = time, .Component = component, .Presentation = CInt(Number(row(3), Integer.MaxValue)), .Input = CInt(Number(row(4), 6)), .ReinforcerType = row(5), .SequenceNumber = Number(row(6), Long.MaxValue), .ComponentTimeMs = time - starts(component), .Source = row(7), .BatchNumber = If(row.Length = 9, Number(row(8), Long.MaxValue), 0)}
                If Not {"Scheduled", "Time", "Manual", "Yoked"}.Contains(item.Source) Then Throw New InvalidDataException("Invalid delivery source.")
                If item.Source = "Manual" AndAlso item.SequenceNumber <> 0 Then Throw New InvalidDataException("Manual delivery cannot belong to a Lag trial.")
                If item.Presentation <> presentation Then Throw New InvalidDataException("Delivery presentation does not match the reference component.")
                result.Deliveries.Add(item)
            End If
        Next
        If version2 Then
            Dim deliveryCounts = result.Deliveries.GroupBy(Function(d) d.Component & ":" & d.SequenceNumber).ToDictionary(Function(g) g.Key, Function(g) g.Count())
            For Each item In result.Sequences
                Dim delivered As Integer
                deliveryCounts.TryGetValue(item.Component & ":" & item.Number, delivered)
                If item.DeliveredCount.HasValue AndAlso item.DeliveredCount.Value <> delivered Then Throw New InvalidDataException("Lag outcome does not match delivered reinforcer records.")
            Next
        End If
        result.HasExactDeliveries = version2 AndAlso starts.Count > 0
        result.HasExactOutcomes = version2 AndAlso result.Sequences.Count > 0 AndAlso result.Sequences.All(Function(s) s.DeliveredCount.HasValue)
        Return result
    End Function
    Public Shared Function ParseLagCsv(csv As String, Optional source As String = "") As SessionReferenceData
        Dim records = Rows(csv)
        If records.Count = 0 OrElse records(0).Length < 19 OrElse records(0)(0) <> "RecordType" OrElse records(0)(7) <> "Sequence" Then Throw New InvalidDataException("This is not a Lag sequence report.")
        Dim result As New SessionReferenceData With {.SourceDescription = source}
        Dim keys As New HashSet(Of String)
        For Each row In records.Skip(1)
            If row.Length < 19 Then Throw New InvalidDataException("Incomplete Lag report row.")
            If result.SessionUid <> "" AndAlso result.SessionUid <> row(15) Then Throw New InvalidDataException("Mixed session IDs in Lag report.")
            result.SessionUid = row(15)
            result.Subject = row(17)
            result.SessionName = row(18)
            If row(0) <> "Sequence" Then Continue For
            SequenceValue(row(7))
            Dim item As New SessionReferenceSequence With {.Component = CInt(Number(row(3), 10)), .Presentation = CInt(Number(row(5), Integer.MaxValue)), .Number = Number(row(6), Long.MaxValue), .TimeMs = Number(row(2), Long.MaxValue), .Value = row(7), .CriterionPassed = row(8) = "Pass"}
            If item.Component = 0 OrElse item.Number = 0 OrElse Not {"Pass", "Fail"}.Contains(row(8)) OrElse Not keys.Add(item.Component & ":" & item.Number) Then Throw New InvalidDataException("Invalid or duplicate sequence in Lag report.")
            result.Sequences.Add(item)
        Next
        Return result
    End Function
    Public Shared Function ReadLocal(filePath As String) As SessionReferenceData
        Dim path = IO.Path.GetFullPath(filePath)
        If New FileInfo(path).Length > 32 * 1024 * 1024 Then Throw New InvalidDataException("Reference exceeds 32 MB.")
        If path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) Then Return ParseLagCsv(File.ReadAllText(path), path)
        Dim rawPath = path
        Dim summaryPath As String = Nothing
        If path.EndsWith("_Summary.txt", StringComparison.OrdinalIgnoreCase) Then
            summaryPath = path
            Dim name = IO.Path.GetFileName(path).Substring(0, IO.Path.GetFileName(path).Length - "_Summary.txt".Length) & "_Raw.txt"
            rawPath = IO.Path.Combine(IO.Path.GetDirectoryName(path), name)
            If Not File.Exists(rawPath) Then rawPath = IO.Path.Combine(IO.Path.GetDirectoryName(IO.Path.GetDirectoryName(path)), "Raw", name)
        ElseIf path.EndsWith("_Raw.txt", StringComparison.OrdinalIgnoreCase) Then
            Dim name = IO.Path.GetFileName(path).Substring(0, IO.Path.GetFileName(path).Length - "_Raw.txt".Length) & "_Summary.txt"
            summaryPath = IO.Path.Combine(IO.Path.GetDirectoryName(path), name)
            If Not File.Exists(summaryPath) Then summaryPath = IO.Path.Combine(IO.Path.GetDirectoryName(IO.Path.GetDirectoryName(path)), "Summary", name)
        End If
        If Not File.Exists(rawPath) Then Throw New FileNotFoundException("Choose the raw session file or place its matching raw file beside this summary.")
        If New FileInfo(rawPath).Length > 32 * 1024 * 1024 OrElse (summaryPath IsNot Nothing AndAlso File.Exists(summaryPath) AndAlso New FileInfo(summaryPath).Length > 32 * 1024 * 1024) Then Throw New InvalidDataException("Reference exceeds 32 MB.")
        Return ParseRaw(File.ReadAllText(rawPath), If(summaryPath IsNot Nothing AndAlso File.Exists(summaryPath), File.ReadAllText(summaryPath), ""), path)
    End Function
End Class
