Imports System.IO
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading

Friend Class CloudSetupInfo
    Public Property Id As String
    Public Property FileName As String
    Public Property SavedAtUtc As String
    Public Property DestinationId As String

    Public Overrides Function ToString() As String
        Return FileName & "  |  " & SavedAtUtc
    End Function
End Class

Friend Enum SessionRestoreResult
    NoSession
    Connected
    RetryLater
    SignInRequired
    LocalOnly
End Enum

'Local-first storage. Pending manifests survive process exit and are scoped to a database AND account.
Partial Friend Class CloudStorage
    Private Shared ReadOnly instance As New Lazy(Of CloudStorage)(Function() New CloudStorage(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReinforcementSchedules")))
    Public Shared ReadOnly Property Current As CloudStorage
        Get
            Return instance.Value
        End Get
    End Property

    Private ReadOnly storageRoot As String
    Private ReadOnly transport As HttpClient
    Private ReadOnly gate As New Object()
    Private ReadOnly uploadLock As New SemaphoreSlim(1, 1)
    Private active As FirebaseClient
    Private activeCancellation As CancellationTokenSource
    Private sessionNeedsCheck As Boolean
    Private statusValue As String = "Local only"
    Public Event StatusChanged As EventHandler

    Public Sub New(root As String, Optional http As HttpClient = Nothing)
        storageRoot = root
        transport = If(http, New HttpClient(New HttpClientHandler With {.AllowAutoRedirect = False}) With {.Timeout = TimeSpan.FromSeconds(20)})
    End Sub

    Public ReadOnly Property IsConnected As Boolean
        Get
            SyncLock gate
                Return active IsNot Nothing
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property Status As String
        Get
            SyncLock gate
                Return statusValue
            End SyncLock
        End Get
    End Property

    Private Sub SetStatus(value As String)
        SyncLock gate
            statusValue = value
        End SyncLock
        RaiseEvent StatusChanged(Me, EventArgs.Empty)
    End Sub

    Public Function CaptureDestination() As CloudDestination
        SyncLock gate
            Return If(active Is Nothing, Nothing, active.Destination)
        End SyncLock
    End Function

    Public Function SavedConnection() As Dictionary(Of String, Object)
        Try
            Dim file = Path.Combine(storageRoot, "connection.json")
            If IO.File.Exists(file) Then
                Dim saved = StorageJson.Decode(IO.File.ReadAllText(file))
                If saved IsNot Nothing AndAlso saved.Values.All(Function(value) TypeOf value Is String) Then Return saved
            End If
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
            TypeOf ex Is ArgumentException OrElse TypeOf ex Is InvalidOperationException
            'An unreadable preference must never prevent local operation.
        End Try
        Return New Dictionary(Of String, Object)()
    End Function

    Public Async Function ConnectAsync(url As String, apiKey As String, email As String, password As String, cancellation As CancellationToken) As Task
        Dim profile As New FirebaseProfile(url, apiKey)
        Dim candidate As New FirebaseClient(profile, transport)
        Await candidate.SignInAsync(email, password, cancellation)
        Await candidate.CheckAccessAsync(cancellation)
        cancellation.ThrowIfCancellationRequested()
        SyncLock gate
            cancellation.ThrowIfCancellationRequested()
            PersistSession(candidate, profile, email)
            ActivateClient(candidate, profile, email)
        End SyncLock
        SetStatus("RTDB connected · " & New Uri(profile.DatabaseUrl).Host)
    End Function

    Private Shared Function SessionEntropy(profile As FirebaseProfile) As Byte()
        Return Encoding.UTF8.GetBytes("ReinforcementSchedules|" & profile.DatabaseUrl & "|" & profile.ApiKey)
    End Function

    Private Sub PersistSession(client As FirebaseClient, profile As FirebaseProfile, email As String)
        Dim clear = Encoding.UTF8.GetBytes(StorageJson.Encode(New With {
            .refreshToken = client.SessionRefreshToken, .userId = client.Destination.UserId}))
        Try
            Dim encrypted = ProtectedData.Protect(clear, SessionEntropy(profile), DataProtectionScope.CurrentUser)
            StorageJson.WriteAtomically(Path.Combine(storageRoot, "connection.json"), StorageJson.Encode(New With {
                .databaseUrl = profile.DatabaseUrl, .apiKey = profile.ApiKey, .email = email.Trim(),
                .mode = "rtdb", .protectedSession = Convert.ToBase64String(encrypted)}))
        Finally
            Array.Clear(clear, 0, clear.Length)
        End Try
    End Sub

    'Called under gate, so a late token renewal cannot undo sign-out or overwrite a new account.
    Private Sub ActivateClient(client As FirebaseClient, profile As FirebaseProfile, email As String)
        If activeCancellation IsNot Nothing Then activeCancellation.Cancel()
        activeCancellation = New CancellationTokenSource()
        active = client
        sessionNeedsCheck = False
        AddHandler client.TokenRenewed, Sub()
                                           SyncLock gate
                                               If Object.ReferenceEquals(active, client) Then PersistSession(client, profile, email)
                                           End SyncLock
                                       End Sub
    End Sub

    Public Async Function RestoreSessionAsync() As Task(Of SessionRestoreResult)
        Dim candidate As FirebaseClient = Nothing
        Dim cancellation As CancellationToken
        Dim activated As Boolean
        Try
            Dim saved = SavedConnection()
            If saved.ContainsKey("mode") AndAlso CStr(saved("mode")) = "local" Then Return SessionRestoreResult.LocalOnly
            If Not saved.ContainsKey("protectedSession") Then Return SessionRestoreResult.NoSession
            Dim profile As New FirebaseProfile(CStr(saved("databaseUrl")), CStr(saved("apiKey")))
            Dim clear = ProtectedData.Unprotect(Convert.FromBase64String(CStr(saved("protectedSession"))),
                                               SessionEntropy(profile), DataProtectionScope.CurrentUser)
            Try
                Dim session = StorageJson.Decode(Encoding.UTF8.GetString(clear))
                candidate = New FirebaseClient(profile, transport)
                candidate.RestoreSession(CStr(session("refreshToken")), CStr(session("userId")))
            Finally
                Array.Clear(clear, 0, clear.Length)
            End Try
            SyncLock gate
                ActivateClient(candidate, profile, CStr(saved("email")))
                sessionNeedsCheck = True
                activated = True
                cancellation = activeCancellation.Token
            End SyncLock
            SetStatus("Reconnecting to saved RTDB…")
            'The protected identity is available while offline, so new files keep their original destination.
            Await candidate.ReadAsync("configIndex", cancellation)
            SyncLock gate
                If Not Object.ReferenceEquals(active, candidate) Then Return SessionRestoreResult.RetryLater
                sessionNeedsCheck = False
            End SyncLock
            SetStatus("RTDB connected · " & New Uri(profile.DatabaseUrl).Host)
            Return SessionRestoreResult.Connected
        Catch ex As FirebaseSignInRequiredException
            SyncLock gate
                If activated Then
                    If Not Object.ReferenceEquals(active, candidate) Then Return SessionRestoreResult.RetryLater
                    SuspendConnection()
                End If
            End SyncLock
            SetStatus(ex.Message)
            Return SessionRestoreResult.SignInRequired
        Catch ex As OperationCanceledException
            Return SessionRestoreResult.RetryLater
        Catch ex As Exception
            If cancellation.IsCancellationRequested Then Return SessionRestoreResult.RetryLater
            If activated Then
                'Network and RTDB rule failures must not erase a valid saved login.
                SetStatus("Saved RTDB unavailable · files will wait locally. " & ex.Message)
                Return SessionRestoreResult.RetryLater
            End If
            SetStatus("Could not restore the saved session. Sign in using Configure RTDB.")
            Return SessionRestoreResult.SignInRequired
        End Try
    End Function

    Public Sub Disconnect()
        'Choosing local-only explicitly forgets the saved sign-in, while retaining non-secret fields.
        SyncLock gate
            SuspendConnection()
            Dim saved = SavedConnection()
            saved.Remove("protectedSession")
            saved("mode") = "local"
            StorageJson.WriteAtomically(Path.Combine(storageRoot, "connection.json"), StorageJson.Encode(saved))
        End SyncLock
        SetStatus("Local only · pending uploads are kept for their original account and database")
    End Sub

    Public Sub SuspendConnection()
        'Application shutdown cancels work without forgetting the saved session.
        SyncLock gate
            active = Nothing
            If activeCancellation IsNot Nothing Then activeCancellation.Cancel()
        End SyncLock
    End Sub

    Private Function PendingDirectory(destination As CloudDestination) As String
        Return Path.Combine(storageRoot, "firebase", destination.Id, "pending")
    End Function

    Public Function PendingCount(destination As CloudDestination) As Integer
        If destination Is Nothing Then Return 0
        Dim folder = PendingDirectory(destination)
        Return If(Directory.Exists(folder), Directory.GetFiles(folder, "*.json").Length, 0)
    End Function

    Private Sub Enqueue(destination As CloudDestination, item As Dictionary(Of String, Object))
        item("databaseUrl") = destination.DatabaseUrl
        item("userId") = destination.UserId
        Dim name = DateTime.UtcNow.Ticks.ToString("D19") & "_" & Guid.NewGuid().ToString("N") & ".json"
        StorageJson.WriteAtomically(Path.Combine(PendingDirectory(destination), name), StorageJson.Encode(item))
        SetStatus("Saved locally · RTDB upload pending")
    End Sub

    Public Function QueueSetup(filePath As String) As Boolean
        Dim destination = CaptureDestination()
        If destination Is Nothing Then Return False
        Dim settings = SetupSaveData.Read(filePath)
        Dim name = Path.ChangeExtension(Path.GetFileName(filePath), ".json")
        Dim id = StorageJson.Hash(name.ToLowerInvariant())
        Dim metadata As New Dictionary(Of String, Object) From {
            {"fileName", name}, {"savedAtUtc", settings.SavedAtUtc.ToUniversalTime().ToString("o")}, {"schemaVersion", 2}}
        Dim setupObject = StorageJson.Decode(settings.ToJson())
        Dim arrayShapes As New List(Of Object)()
        CaptureArrayShapes(setupObject, "", arrayShapes)
        Dim config As New Dictionary(Of String, Object)(metadata) From {{"setup", setupObject}, {"arrayShapes", arrayShapes}}
        Enqueue(destination, New Dictionary(Of String, Object) From {
            {"updates", New Dictionary(Of String, Object) From {{"configs/" & id, config}, {"configIndex/" & id, metadata}}}})
        Return True
    End Function

    Public Sub QueueSession(destination As CloudDestination, sessionId As String, box As String, subject As String,
                            sessionName As String, status As String, rawPath As String, summaryPath As String, chartPath As String)
        If destination Is Nothing Then Return
        Dim parsedId As Guid
        If Not Guid.TryParseExact(sessionId, "N", parsedId) Then Throw New ArgumentException("Invalid session ID.")
        If Not File.Exists(rawPath) OrElse Not File.Exists(summaryPath) Then Throw New IOException("Session files are incomplete; RTDB upload was not queued.")
        Dim files As New Dictionary(Of String, Object) From {{"raw", rawPath}, {"summary", summaryPath}}
        If File.Exists(chartPath) Then files.Add("chart", chartPath)
        Enqueue(destination, New Dictionary(Of String, Object) From {
            {"sessionId", sessionId}, {"files", files},
            {"metadata", New Dictionary(Of String, Object) From {
                {"schemaVersion", 1}, {"sessionUid", sessionId}, {"box", box}, {"subject", subject},
                {"sessionName", sessionName}, {"status", status}, {"savedAtUtc", DateTime.UtcNow.ToString("o")}}}})
    End Sub

    Public Async Function SynchronizeAsync() As Task
        'Disk reads and uploads run away from the WinForms acquisition timers.
        Await Task.Run(Async Function()
                           If Not Await uploadLock.WaitAsync(0) Then Return
                           Dim client As FirebaseClient = Nothing
                           Dim cancellation As CancellationToken
                           Dim checkSession As Boolean
                           Try
                               SyncLock gate
                                   client = active
                                   If client Is Nothing Then Return
                                   cancellation = activeCancellation.Token
                                   checkSession = sessionNeedsCheck
                               End SyncLock
                               If checkSession Then
                                   Await client.ReadAsync("configIndex", cancellation)
                                   SyncLock gate
                                       If Not Object.ReferenceEquals(active, client) Then Return
                                       sessionNeedsCheck = False
                                   End SyncLock
                                   SetStatus("RTDB connected · " & New Uri(client.Destination.DatabaseUrl).Host)
                               End If
                               Dim destination = client.Destination
                               Dim folder = PendingDirectory(destination)
                               If Not Directory.Exists(folder) Then Return
                               Dim localError As String = Nothing
                               For Each file In Directory.GetFiles(folder, "*.json").OrderBy(Function(name) name, StringComparer.Ordinal)
                                   cancellation.ThrowIfCancellationRequested()
                                   Dim updates As Dictionary(Of String, Object)
                                   Try
                                       updates = PendingUpdates(file, destination)
                                   Catch ex As Exception
                                       'Keep this record for repair, but let independent sessions upload.
                                       localError = ex.Message
                                       Continue For
                                   End Try
                                   Await client.PatchAsync(updates, cancellation)
                                   'Delete only after an acknowledged atomic write. The session UID makes retries idempotent.
                                   IO.File.Delete(file)
                               Next
                               If Not cancellation.IsCancellationRequested Then
                                   Dim remaining = PendingCount(destination)
                                   SetStatus(If(remaining = 0, "RTDB synchronized · " & New Uri(destination.DatabaseUrl).Host,
                                                "Saved locally · " & remaining & " RTDB upload(s) pending") &
                                             If(localError Is Nothing, "", " · Check pending files: " & localError))
                               End If
                           Catch ex As OperationCanceledException
                               'A disconnect or account change leaves unacknowledged manifests on disk.
                           Catch ex As FirebaseSignInRequiredException
                               SyncLock gate
                                   If Not Object.ReferenceEquals(active, client) Then Return
                                   SuspendConnection()
                               End SyncLock
                               SetStatus(ex.Message)
                           Catch ex As Exception
                               If Not cancellation.IsCancellationRequested Then SetStatus("Saved locally · RTDB pending: " & ex.Message)
                           Finally
                               uploadLock.Release()
                           End Try
                       End Function)
    End Function

    Private Function PendingUpdates(file As String, destination As CloudDestination) As Dictionary(Of String, Object)
        Dim pending = StorageJson.Decode(IO.File.ReadAllText(file))
        If CStr(pending("databaseUrl")) <> destination.DatabaseUrl OrElse CStr(pending("userId")) <> destination.UserId Then
            Throw New IOException("A pending upload belongs to another account or database.")
        End If
        If Not pending.ContainsKey("sessionId") Then Return DirectCast(pending("updates"), Dictionary(Of String, Object))
        Dim data = DirectCast(pending("metadata"), Dictionary(Of String, Object))
        Dim files As New Dictionary(Of String, Object)()
        For Each entry In DirectCast(pending("files"), Dictionary(Of String, Object))
            Dim source = CStr(entry.Value)
            files(entry.Key) = New With {
                .fileName = Path.GetFileName(source), .encoding = If(entry.Key = "chart", "base64", "utf-8"),
                .content = If(entry.Key = "chart", Convert.ToBase64String(IO.File.ReadAllBytes(source)), IO.File.ReadAllText(source))}
        Next
        Dim sessionMetadata As New Dictionary(Of String, Object)(data)
        data("files") = files
        Return New Dictionary(Of String, Object) From {{"sessions/" & CStr(pending("sessionId")), data}, {"sessionIndex/" & CStr(pending("sessionId")), sessionMetadata}}
    End Function

    Public Async Function ListSetupsAsync() As Task(Of List(Of CloudSetupInfo))
        Dim client As FirebaseClient
        SyncLock gate
            client = active
        End SyncLock
        If client Is Nothing Then Throw New IOException("Connect to RTDB first, or load a local file.")
        Dim index = Await client.ReadAsync("configIndex", CancellationToken.None)
        EnsureCurrentClient(client)
        Dim result As New List(Of CloudSetupInfo)()
        If index Is Nothing Then Return result
        For Each entry In index
            ValidateConfigId(entry.Key)
            Dim info = TryCast(entry.Value, Dictionary(Of String, Object))
            If info Is Nothing Then Throw New InvalidDataException("Invalid RTDB configuration index.")
            Dim name = CStr(info("fileName"))
            ValidateFileName(name)
            result.Add(New CloudSetupInfo With {.Id = entry.Key, .FileName = name, .SavedAtUtc = CStr(info("savedAtUtc")), .DestinationId = client.Destination.Id})
        Next
        Return result.OrderBy(Function(info) info.FileName).ToList()
    End Function

    Public Async Function DownloadSetupAsync(info As CloudSetupInfo) As Task(Of String)
        ValidateConfigId(info.Id)
        Dim client As FirebaseClient
        SyncLock gate
            client = active
        End SyncLock
        If client Is Nothing Then Throw New IOException("Connect to RTDB first, or load a local file.")
        If info.DestinationId <> client.Destination.Id Then Throw New IOException("The RTDB connection changed. Open the setup list again.")
        Dim result = Await client.ReadAsync("configs/" & info.Id, CancellationToken.None)
        EnsureCurrentClient(client)
        If result Is Nothing Then Throw New IOException("This configuration is no longer in RTDB.")
        Dim schemaVersion = Convert.ToInt32(result("schemaVersion"))
        If schemaVersion <> 1 AndAlso schemaVersion <> 2 Then Throw New InvalidDataException("Unsupported RTDB configuration format.")
        Dim name = CStr(result("fileName"))
        ValidateFileName(name)
        Dim content As String
        If schemaVersion = 1 Then
            content = CStr(result("xml"))
            SetupSaveData.ReadXml(content)
        Else
            If Not TypeOf result("setup") Is Dictionary(Of String, Object) Then Throw New InvalidDataException("Invalid RTDB setup object.")
            Dim setupObject = DirectCast(result("setup"), Dictionary(Of String, Object))
            If result.ContainsKey("arrayShapes") Then RestoreArrayShapes(setupObject, result("arrayShapes"))
            content = StorageJson.Encode(setupObject)
            content = SetupSaveData.ReadJson(content).ToIndentedJson()
        End If
        'Validate before replacing any cached file or changing the editor.
        If StorageJson.Hash(name.ToLowerInvariant()) <> info.Id Then Throw New InvalidDataException("RTDB configuration name does not match its ID.")
        Dim cache = Path.Combine(storageRoot, "firebase", client.Destination.Id, "configs", name)
        StorageJson.WriteAtomically(cache, content)
        Return cache
    End Function

    'RTDB removes null children and empty arrays and may return sparse arrays as
    'numeric-keyed objects. Keep only array dimensions alongside the readable
    'setup object so downloaded configurations preserve all positions exactly.
    Private Shared Sub CaptureArrayShapes(value As Object, path As String, shapes As List(Of Object))
        Dim fields = TryCast(value, Dictionary(Of String, Object))
        If fields IsNot Nothing Then
            For Each field In fields
                CaptureArrayShapes(field.Value, path & "/" & field.Key, shapes)
            Next
            Return
        End If
        Dim items = TryCast(value, System.Collections.IList)
        If items Is Nothing Then Return
        shapes.Add(New With {.path = path, .length = items.Count})
        For index As Integer = 0 To items.Count - 1
            CaptureArrayShapes(items(index), path & "/" & index.ToString(Globalization.CultureInfo.InvariantCulture), shapes)
        Next
    End Sub

    Private Shared Sub RestoreArrayShapes(root As Dictionary(Of String, Object), metadata As Object)
        Dim shapes = TryCast(metadata, System.Collections.IList)
        If shapes Is Nothing OrElse shapes.Count > 100000 Then Throw New InvalidDataException("Invalid RTDB array metadata.")
        Dim totalLength As Long = 0
        For Each entry In shapes
            Dim shape = TryCast(entry, Dictionary(Of String, Object))
            If shape Is Nothing Then Throw New InvalidDataException("Invalid RTDB array metadata.")
            Dim path = CStr(shape("path"))
            Dim length = Convert.ToInt32(shape("length"))
            totalLength += length
            If length < 0 OrElse totalLength > 1000000 OrElse path Is Nothing OrElse Not path.StartsWith("/", StringComparison.Ordinal) Then Throw New InvalidDataException("Invalid RTDB array dimensions.")
            Dim parts = path.Substring(1).Split("/"c)
            If parts.Length > 64 Then Throw New InvalidDataException("Invalid RTDB array path.")
            Dim parent As Object = root
            For i As Integer = 0 To parts.Length - 2
                Dim fields = TryCast(parent, Dictionary(Of String, Object))
                If fields IsNot Nothing Then
                    parent = fields(parts(i))
                Else
                    parent = DirectCast(parent, Object())(Integer.Parse(parts(i), Globalization.CultureInfo.InvariantCulture))
                End If
            Next
            Dim key = parts(parts.Length - 1)
            Dim oldValue As Object = Nothing
            Dim dictionary = TryCast(parent, Dictionary(Of String, Object))
            If dictionary IsNot Nothing Then
                dictionary.TryGetValue(key, oldValue)
            Else
                oldValue = DirectCast(parent, Object())(Integer.Parse(key, Globalization.CultureInfo.InvariantCulture))
            End If
            Dim restored(length - 1) As Object
            Dim oldArray = TryCast(oldValue, System.Collections.IList)
            Dim oldFields = TryCast(oldValue, Dictionary(Of String, Object))
            If oldArray IsNot Nothing Then
                If oldArray.Count > length Then Throw New InvalidDataException("RTDB array is longer than declared.")
                For i As Integer = 0 To oldArray.Count - 1
                    restored(i) = oldArray(i)
                Next
            ElseIf oldFields IsNot Nothing Then
                For Each item In oldFields
                    Dim index As Integer
                    If Not Integer.TryParse(item.Key, index) OrElse index < 0 OrElse index >= length Then Throw New InvalidDataException("Invalid RTDB array index.")
                    restored(index) = item.Value
                Next
            ElseIf oldValue IsNot Nothing Then
                Throw New InvalidDataException("Invalid RTDB array value.")
            End If
            If dictionary IsNot Nothing Then
                dictionary(key) = restored
            Else
                DirectCast(parent, Object())(Integer.Parse(key, Globalization.CultureInfo.InvariantCulture)) = restored
            End If
        Next
    End Sub

    Private Sub EnsureCurrentClient(client As FirebaseClient)
        SyncLock gate
            If Not Object.ReferenceEquals(active, client) Then Throw New IOException("The RTDB connection changed. Open the setup list again.")
        End SyncLock
    End Sub

    Private Shared Sub ValidateConfigId(id As String)
        If Not System.Text.RegularExpressions.Regex.IsMatch(If(id, ""), "^[a-f0-9]{64}$") Then
            Throw New InvalidDataException("Invalid RTDB configuration ID.")
        End If
    End Sub

    Private Shared Sub ValidateFileName(name As String)
        If String.IsNullOrWhiteSpace(name) OrElse name <> Path.GetFileName(name) OrElse
            name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 OrElse Not (name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) OrElse name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) Then
            Throw New InvalidDataException("Invalid RTDB configuration filename.")
        End If
    End Sub
End Class
