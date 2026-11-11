Option Strict On
Imports System.IO
Imports System.Threading
Imports System.Globalization

Friend NotInheritable Class CloudSessionInfo
    Public Property Id As String
    Public Property Subject As String
    Public Property SessionName As String
    Public Property SavedAtUtc As String
    Public Property DestinationId As String
    Public Property Suggested As Boolean
    Public Overrides Function ToString() As String
        Return If(Suggested, "Suggested: ", "") & Subject & " | " & SessionName & " | " & SavedAtUtc & " | " & Id
    End Function
End Class

Partial Friend Class CloudStorage
    Private Shared Function SessionString(data As Dictionary(Of String, Object), key As String) As String
        Dim value As Object = Nothing
        If Not data.TryGetValue(key, value) OrElse Not TypeOf value Is String Then Throw New InvalidDataException("Incomplete RTDB session metadata: " & key & ".")
        Return DirectCast(value, String)
    End Function
    Private Shared Sub ValidateSessionId(id As String)
        Dim value As Guid
        If Not Guid.TryParseExact(id, "N", value) Then Throw New InvalidDataException("Invalid reference session ID.")
    End Sub
    Public Async Function ListSessionsAsync(Optional subject As String = "") As Task(Of List(Of CloudSessionInfo))
        Dim client As FirebaseClient
        Dim cancellation As CancellationToken
        SyncLock gate
            client = active
            If client Is Nothing Then Throw New IOException("Connect to RTDB first, or choose a local session file.")
            cancellation = activeCancellation.Token
        End SyncLock
        Dim sessions = Await client.ReadAsync("sessionIndex", cancellation)
        EnsureCurrentClient(client)
        If sessions Is Nothing Then sessions = New Dictionary(Of String, Object)
        Dim legacyKeys = Await client.ReadShallowAsync("sessions", cancellation)
        EnsureCurrentClient(client)
        If legacyKeys IsNot Nothing Then
            For Each id In legacyKeys.Keys
                ValidateSessionId(id)
                If sessions.ContainsKey(id) Then Continue For
                sessions(id) = Await client.ReadSessionMetadataAsync(id, cancellation)
                EnsureCurrentClient(client)
            Next
        End If
        Dim result As New List(Of CloudSessionInfo)
        For Each entry In sessions
            ValidateSessionId(entry.Key)
            Dim data = TryCast(entry.Value, Dictionary(Of String, Object))
            If data Is Nothing Then Throw New InvalidDataException("Invalid RTDB session record.")
            If SessionString(data, "sessionUid") <> entry.Key Then Throw New InvalidDataException("RTDB session ID mismatch.")
            Dim saved = SessionString(data, "savedAtUtc")
            Dim parsed As DateTimeOffset
            If Not DateTimeOffset.TryParse(saved, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, parsed) Then Throw New InvalidDataException("Invalid RTDB session date.")
            result.Add(New CloudSessionInfo With {.Id = entry.Key, .Subject = SessionString(data, "subject"), .SessionName = SessionString(data, "sessionName"), .SavedAtUtc = parsed.UtcDateTime.ToString("o"), .DestinationId = client.Destination.Id})
        Next
        result = result.OrderByDescending(Function(s) String.Equals(s.Subject, subject, StringComparison.Ordinal) AndAlso subject <> "").ThenByDescending(Function(s) s.SavedAtUtc, StringComparer.Ordinal).ToList()
        If result.Count > 0 AndAlso subject <> "" AndAlso String.Equals(result(0).Subject, subject, StringComparison.Ordinal) Then result(0).Suggested = True
        Return result
    End Function
    Public Async Function ReadSessionReferenceAsync(info As CloudSessionInfo) As Task(Of SessionReferenceData)
        If info Is Nothing Then Throw New ArgumentNullException(NameOf(info))
        ValidateSessionId(info.Id)
        Dim client As FirebaseClient
        Dim cancellation As CancellationToken
        SyncLock gate
            client = active
            If client Is Nothing Then Throw New IOException("Connect to RTDB first, or choose a local session file.")
            If client.Destination.Id <> info.DestinationId Then Throw New IOException("RTDB account or database changed. Open the session list again.")
            cancellation = activeCancellation.Token
        End SyncLock
        Dim data = Await client.ReadAsync("sessions/" & info.Id, cancellation)
        EnsureCurrentClient(client)
        If data Is Nothing Then Throw New IOException("This reference session no longer exists in RTDB.")
        If SessionString(data, "sessionUid") <> info.Id Then Throw New InvalidDataException("RTDB session ID mismatch.")
        Dim files = TryCast(data("files"), Dictionary(Of String, Object))
        If files Is Nothing Then Throw New InvalidDataException("Missing RTDB session files.")
        Dim raw = ReadReferenceBlob(files, "raw")
        Dim summary = ReadReferenceBlob(files, "summary")
        Dim result = SessionReferenceData.ParseRaw(raw, summary, "RTDB session " & info.Id & " (" & info.DestinationId & ")")
        If result.SessionUid <> info.Id Then Throw New InvalidDataException("Reference files do not match the RTDB session ID.")
        If result.Subject <> "" AndAlso result.Subject <> SessionString(data, "subject") Then Throw New InvalidDataException("Reference subject does not match session metadata.")
        result.Subject = SessionString(data, "subject")
        result.SessionName = SessionString(data, "sessionName")
        Return result
    End Function
    Private Shared Function ReadReferenceBlob(files As Dictionary(Of String, Object), key As String) As String
        Dim value As Object = Nothing
        If Not files.TryGetValue(key, value) Then Throw New InvalidDataException("Missing reference " & key & " file.")
        Dim blob = TryCast(value, Dictionary(Of String, Object))
        If blob Is Nothing OrElse SessionString(blob, "encoding") <> "utf-8" Then Throw New InvalidDataException("Unsupported reference file encoding.")
        Dim text = SessionString(blob, "content")
        If text.Length > 32 * 1024 * 1024 Then Throw New InvalidDataException("Reference file exceeds 32 MB.")
        Return text
    End Function
End Class
