Imports System.Drawing
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports ReinforcementSchedules
Imports Microsoft.VisualBasic

Module StorageTests
    Private assertions As Integer
    Private ReadOnly key As String = New String("k"c, 32)
    Private Const database As String = "https://test-default-rtdb.firebaseio.com"
    Private Const email As String = "researcher@example.test"
    Private Const password As String = "test-only-password"

    <STAThread>
    Sub Main()
        Try
            Dim root = Path.Combine(Environment.CurrentDirectory, "obj", "st", Guid.NewGuid().ToString("N").Substring(0, 8))
            Directory.CreateDirectory(root)
            RunStorageTests(root).GetAwaiter().GetResult()
            RunRememberedSessionTests(Path.Combine(root, "login")).GetAwaiter().GetResult()
            RunUiTests(root)
            Console.WriteLine("PASS: " & assertions & " assertions. Test files: " & root)
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub

    Private Sub Check(condition As Boolean, message As String)
        If Not condition Then Throw New Exception("FAIL: " & message)
        assertions += 1
    End Sub

    Private Async Function Reject(action As Func(Of Task), message As String) As Task
        Dim rejected As Boolean
        Try
            Await action()
        Catch ex As Exception
            rejected = True
        End Try
        Check(rejected, message)
    End Function

    Private Function SampleSetup() As SetupSaveData
        Dim components(MAX_COMPONENTS) As ComponentBlueprint
        Dim boxed As ValueType = New ComponentBlueprint With {.InputCount = MAX_INPUTS, .ComponentDuration = 60, .ComponentIteration = 2, .ComponentName = "Test program"}
        For Each field In GetType(ComponentBlueprint).GetFields(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance)
            If field.FieldType.IsArray Then field.SetValue(boxed, Array.CreateInstance(field.FieldType.GetElementType(), MAX_INPUTS))
        Next
        components(1) = DirectCast(boxed, ComponentBlueprint)
        components(1).InputName(0) = "Left lever"
        components(1).ScheduleType(0) = "Fixed Ratio"
        components(1).ScheduleValue(0) = 5
        components(1).ClassicChains(0) = New ClassicChainDefinition With {.Links = New List(Of ClassicChainLink) From {New ClassicChainLink With {.ScheduleType = "Fixed Ratio", .Value = 5}, New ClassicChainLink With {.ScheduleType = "Variable Time", .Value = 10, .VariableValueCount = 7, .RetractInput = True, .StimulusType = "Tone"}}}
        components(1).ClassicChains(4) = components(1).ClassicChains(0).Copy()
        components(1).ScheduleType(4) = "Fixed Ratio"
        components(1).ScheduleValue(4) = 5
        Return New SetupSaveData With {
            .BoxName = "Box A", .SavedAtUtc = DateTime.UtcNow, .StartDelay = "0", .PostSession = "0", .ICIDuration = "5",
            .ICIEnabled = True, .ICIRetractInputs = True, .ICIStimulus = "None", .ComponentOrder = "1,1",
            .CommandMap = New ArduinoCommandMap(), .ComponentCount = 1, .Components = components}
    End Function

    Private Function SampleJson(filePath As String) As String
        Return SetupSaveData.Read(filePath).ToJson()
    End Function

    Private Function FirebasePrune(value As Object) As Object
        Dim fields = TryCast(value, Dictionary(Of String, Object))
        If fields IsNot Nothing Then
            Dim result As New Dictionary(Of String, Object)
            For Each field In fields
                Dim child = FirebasePrune(field.Value)
                If child IsNot Nothing Then result(field.Key) = child
            Next
            Return If(result.Count = 0, Nothing, result)
        End If
        Dim array = TryCast(value, System.Collections.IList)
        If array IsNot Nothing Then
            Dim result As New Dictionary(Of String, Object)
            For i As Integer = 0 To array.Count - 1
                Dim child = FirebasePrune(array(i))
                If child IsNot Nothing Then result(i.ToString()) = child
            Next
            Return If(result.Count = 0, Nothing, result)
        End If
        Return value
    End Function

    Private Async Function RunStorageTests(root As String) As Task
        Dim server As New FakeFirebase()
        Using http As New HttpClient(server) With {.Timeout = TimeSpan.FromSeconds(2)}
            Dim storage As New CloudStorage(root, http)
            Dim preferencePath = Path.Combine(root, "connection.json")
            For Each corrupt In New String() {"null", "{", "{""email"":[]}"}
                File.WriteAllText(preferencePath, corrupt)
                Check(storage.SavedConnection().Count = 0, "Corrupt saved preferences fall back to empty fields")
            Next
            File.Delete(preferencePath)
            Dim setupPath = Path.Combine(root, "Schedule #1.json")
            Dim readableSetup = SampleSetup()
            readableSetup.BoxName = "Quotes ""and"" backslash \ and punctuation { [ , : } ] " & vbLf & "Accents: áé"
            readableSetup.Save(setupPath)
            Check(File.ReadAllLines(setupPath).Length > 10 AndAlso File.ReadAllText(setupPath).Contains("  ""BoxName"": "), "Saved JSON uses readable indentation")
            Check(SetupSaveData.Read(setupPath).ToJson() = readableSetup.ToJson(), "Formatting preserves string escapes, dates and all values")
            Check(Not storage.IsConnected AndAlso storage.CaptureDestination() Is Nothing, "Starts local-only")
            Check(Not storage.QueueSetup(setupPath), "Local saves do not queue without a destination")
            Await storage.SynchronizeAsync()
            Check(server.Requests.Count = 0, "Local-only mode sends no HTTP requests")
            Check(SetupSaveData.Read(setupPath).Components(1).ScheduleValue(0) = 5, "New JSON format loads")

            For Each invalid In New String() {"http://test.firebaseio.com", database & "/configs", database & "/?auth=secret", "https://other.example", "https://a@b.firebaseio.com"}
                Dim rejected As Boolean
                Try
                    Dim profile As New FirebaseProfile(invalid, key)
                Catch ex As ArgumentException
                    rejected = True
                End Try
                Check(rejected, "Reject invalid RTDB URL " & invalid)
            Next
            server.RejectLogin = True
            Await Reject(Function() storage.ConnectAsync(database, key, email, password, CancellationToken.None), "Invalid login is rejected")
            Check(Not storage.IsConnected AndAlso Not File.Exists(Path.Combine(root, "connection.json")), "Failed login does not activate or persist credentials")
            server.RejectLogin = False
            server.DenyWrites = True
            Await Reject(Function() storage.ConnectAsync(database, key, email, password, CancellationToken.None), "Read-only RTDB is not accepted as ready for uploads")
            Check(Not storage.IsConnected, "Permission failure preserves local mode")
            server.DenyWrites = False
            Await storage.ConnectAsync(database, key, email, password, CancellationToken.None)
            Dim target = storage.CaptureDestination()
            Check(storage.IsConnected, "Valid login and permission checks activate RTDB")
            Check(Not server.Values.Keys.Any(Function(path) path.Contains("connectionChecks/")), "Connection probe is cleaned up")
            Dim preferences = File.ReadAllText(Path.Combine(root, "connection.json"))
            Check(Not preferences.Contains(password) AndAlso Not preferences.Contains("id-token") AndAlso Not preferences.Contains("refresh-token"), "No passwords or plaintext tokens on disk")

            Check(storage.QueueSetup(setupPath), "Connected save queues setup")
            server.Offline = True
            Await storage.SynchronizeAsync()
            Check(storage.PendingCount(target) = 1 AndAlso File.Exists(setupPath), "Network failure preserves local setup and pending upload")
            Check(storage.Status.Contains("pending"), "Upload failure is visible")
            Dim restarted As New CloudStorage(root, http)
            Check(Not restarted.IsConnected, "Restart begins before automatic restoration")
            Check(restarted.PendingCount(target) = 1, "Pending upload survives restart")
            server.Offline = False
            Check(Await restarted.RestoreSessionAsync() = SessionRestoreResult.Connected, "Restart restores the encrypted sign-in")
            Await restarted.SynchronizeAsync()
            Check(restarted.PendingCount(target) = 0, "Successful retry acknowledges pending setup")
            Dim entries = Await restarted.ListSetupsAsync()
            Check(entries.Count = 1 AndAlso entries(0).FileName = Path.GetFileName(setupPath), "RTDB index lists the uploaded configuration")
            Dim cached = Await restarted.DownloadSetupAsync(entries(0))
            Check(SampleJson(cached) = SampleJson(setupPath), "RTDB configuration round-trips without losing fields (JSON property ordering is not data)")
            Check(SetupSaveData.Read(cached).Components(1).ScheduleValue(0) = 5, "Downloaded configuration uses existing validation")

            Dim missingDuration = StorageJson.Decode(File.ReadAllText(setupPath))
            Dim missingComponents = DirectCast(missingDuration("Components"), System.Collections.IList)
            DirectCast(missingComponents(1), Dictionary(Of String, Object)).Remove("ComponentDuration")
            Await Reject(Function()
                             SetupSaveData.ReadJson(StorageJson.Encode(missingDuration))
                             Return Task.CompletedTask
                         End Function, "Missing JSON component duration cannot silently default to zero")

            Dim configKey = database & "/configs/" & entries(0).Id
            Dim remote = DirectCast(server.Values(configKey), Dictionary(Of String, Object))
            Check(Convert.ToInt32(remote("schemaVersion")) = 2 AndAlso TypeOf remote("setup") Is Dictionary(Of String, Object), "RTDB stores a structured JSON setup")
            Check(Not remote.ContainsKey("xml"), "New RTDB setup has no embedded XML string")
            Dim originalSetup = remote("setup")
            Dim originalCached = File.ReadAllText(cached)
            remote("setup") = New Dictionary(Of String, Object) From {{"FormatVersion", 6}}
            Await Reject(Async Function()
                             Await restarted.DownloadSetupAsync(entries(0))
                         End Function, "Incomplete remote JSON is rejected")
            Check(File.ReadAllText(cached) = originalCached, "Bad remote JSON does not replace cached configuration")
            remote("setup") = originalSetup
            remote("fileName") = "..\outside.json"
            Await Reject(Async Function()
                             Await restarted.DownloadSetupAsync(entries(0))
                         End Function, "Remote filenames cannot traverse local directories")
            remote("fileName") = entries(0).FileName

            'Simulate RTDB pruning nulls / empty arrays and returning numeric-key objects.
            remote("setup") = FirebasePrune(originalSetup)
            Dim restoredPath = Await restarted.DownloadSetupAsync(entries(0))
            Check(SetupSaveData.Read(restoredPath).ToJson() = SampleJson(setupPath), "RTDB null and empty-array pruning preserves all setup values")
            remote("setup") = originalSetup

            Dim legacyPath = Path.Combine(root, "Legacy.xml")
            Using writer = System.Xml.XmlWriter.Create(legacyPath)
                Dim serializer As New System.Runtime.Serialization.DataContractSerializer(GetType(SetupSaveData))
                Dim legacySettings = SampleSetup()
                legacySettings.FormatVersion = 5
                serializer.WriteObject(writer, legacySettings)
            End Using
            Dim legacyXml = File.ReadAllText(legacyPath)
            Check(SetupSaveData.ReadXml(legacyXml).Components(1).ScheduleValue(0) = 5, "Legacy XML still imports")
            Dim legacyId = StorageJson.Hash("legacy.xml")
            server.Values(database & "/configs/" & legacyId) = New Dictionary(Of String, Object) From {
                {"fileName", "Legacy.xml"}, {"schemaVersion", 1}, {"xml", legacyXml}}
            Dim legacyInfo As New CloudSetupInfo With {.Id = legacyId, .FileName = "Legacy.xml", .DestinationId = target.Id}
            Dim legacyCache = Await restarted.DownloadSetupAsync(legacyInfo)
            Check(File.ReadAllText(legacyCache) = legacyXml, "Old RTDB XML records remain readable without migration")
            DirectCast(server.Values(database & "/configs/" & legacyId), Dictionary(Of String, Object))("xml") = "<!DOCTYPE x [<!ENTITY a SYSTEM 'file:///secret'>]><x>&a;</x>"
            Await Reject(Async Function()
                             Await restarted.DownloadSetupAsync(legacyInfo)
                         End Function, "Unsafe legacy XML is rejected")
            Check(File.ReadAllText(legacyCache) = legacyXml, "Unsafe XML cannot replace cached legacy file")

            Dim raw = Path.Combine(root, "raw.txt")
            Dim summary = Path.Combine(root, "summary.txt")
            Dim chart = Path.Combine(root, "chart.png")
            File.WriteAllText(raw, "0,Start" & vbLf & "125,1", New UTF8Encoding(False))
            File.WriteAllText(summary, "Subject: rat-1; responses: 1", New UTF8Encoding(False))
            File.WriteAllBytes(chart, New Byte() {137, 80, 78, 71})
            Dim sessionId = Guid.NewGuid().ToString("N")
            restarted.QueueSession(target, sessionId, "A", "rat-1", "1", "Completed", raw, summary, chart)
            server.LoseNextPatchAcknowledgement = True
            Await restarted.SynchronizeAsync()
            Check(restarted.PendingCount(target) = 1, "Unacknowledged write remains pending even if server received it")
            Await restarted.SynchronizeAsync()
            Check(restarted.PendingCount(target) = 0, "Lost acknowledgement can be retried")
            Check(server.Values.Keys.Where(Function(path) path.Contains("/sessions/")).Count() = 1, "Session retries use stable UID without duplicates")
            Dim session = DirectCast(server.Values(database & "/sessions/" & sessionId), Dictionary(Of String, Object))
            Dim uploaded = DirectCast(session("files"), Dictionary(Of String, Object))
            Check(CStr(DirectCast(uploaded("raw"), Dictionary(Of String, Object))("content")) = File.ReadAllText(raw), "Raw data is preserved")
            Check(CStr(DirectCast(uploaded("summary"), Dictionary(Of String, Object))("content")) = File.ReadAllText(summary), "Summary is preserved")
            Check(CStr(DirectCast(uploaded("chart"), Dictionary(Of String, Object))("content")) = Convert.ToBase64String(File.ReadAllBytes(chart)), "Chart bytes are preserved as base64")

            Dim secondId = Guid.NewGuid().ToString("N")
            restarted.QueueSession(target, secondId, "A", "rat-1", "2", "Error", raw, summary, Nothing)
            Dim otherDb = "https://other-default-rtdb.firebaseio.com"
            Await restarted.ConnectAsync(otherDb, key, email, password, CancellationToken.None)
            Await restarted.SynchronizeAsync()
            Check(restarted.PendingCount(target) = 1 AndAlso Not server.Values.ContainsKey(otherDb & "/sessions/" & secondId), "Switching databases cannot send old sessions to the new database")
            Await Reject(Async Function()
                             Await restarted.DownloadSetupAsync(entries(0))
                         End Function, "An old picker cannot load from a different database")
            Await restarted.ConnectAsync(database, key, "other@example.test", password, CancellationToken.None)
            Await restarted.SynchronizeAsync()
            Check(restarted.PendingCount(target) = 1, "Another account cannot drain the original account's queue")
            Await restarted.ConnectAsync(database, key, email, password, CancellationToken.None)
            Await restarted.SynchronizeAsync()
            Check(restarted.PendingCount(target) = 0, "Original database and account recover their pending session")

            Dim missingRaw = Path.Combine(root, "removed-raw.txt")
            File.Copy(raw, missingRaw)
            restarted.QueueSession(target, Guid.NewGuid().ToString("N"), "A", "rat-1", "3", "Error", missingRaw, summary, Nothing)
            File.Delete(missingRaw)
            restarted.QueueSetup(setupPath)
            Await restarted.SynchronizeAsync()
            Check(restarted.PendingCount(target) = 1, "A missing data file stays pending without blocking unrelated setup uploads")
            Check(restarted.Status.Contains("Check pending files"), "Missing data file reports a repairable problem")
            File.Copy(raw, missingRaw)
            Await restarted.SynchronizeAsync()
            Check(restarted.PendingCount(target) = 0, "Restoring the referenced file allows retry")

            server.ExpiresIn = "1"
            Dim refreshes = server.Refreshes
            Await restarted.ConnectAsync(database, key, email, password, CancellationToken.None)
            Check(server.Refreshes > refreshes, "Expired ID tokens are renewed before database requests")
            server.ExpiresIn = "3600"
            restarted.Disconnect()
            Dim calls = server.Requests.Count
            Await restarted.SynchronizeAsync()
            Check(Not restarted.QueueSetup(setupPath) AndAlso server.Requests.Count = calls, "Disconnect returns to fully local operation")
            Using cancellation As New CancellationTokenSource()
                cancellation.Cancel()
                Await Reject(Function() restarted.ConnectAsync(database, key, email, password, cancellation.Token), "Cancelled connection stays cancelled")
            End Using
            Check(Not restarted.IsConnected, "Cancelled login cannot activate RTDB")
        End Using
    End Function

    Private Async Function RunRememberedSessionTests(root As String) As Task
        Dim server As New FakeFirebase()
        Using http As New HttpClient(server)
            Dim first As New CloudStorage(root, http)
            Check(Await first.RestoreSessionAsync() = SessionRestoreResult.NoSession, "First use requires configuration")
            Check(server.Requests.Count = 0, "First use does not attempt anonymous authentication")
            Await first.ConnectAsync(database, key, email, password, CancellationToken.None)
            Dim target = first.CaptureDestination()
            Dim profileFile = Path.Combine(root, "connection.json")
            Dim remembered = File.ReadAllText(profileFile)
            Check(StorageJson.Decode(remembered).ContainsKey("protectedSession"), "Successful sign-in stores a protected refresh session")
            first.SuspendConnection()
            Check(File.ReadAllText(profileFile) = remembered, "Closing the app preserves the saved session")

            Dim restarted As New CloudStorage(root, http)
            Dim logins = server.Requests.Where(Function(r) r.Contains("signInWithPassword")).Count()
            Check(Await restarted.RestoreSessionAsync() = SessionRestoreResult.Connected, "Saved session reconnects without credentials")
            Check(server.Requests.Where(Function(r) r.Contains("signInWithPassword")).Count() = logins, "Automatic restoration uses refresh token, not password sign-in")
            Check(restarted.CaptureDestination().Id = target.Id, "Restoration retains account and database identity")
            Check(File.ReadAllText(profileFile) <> remembered, "Renewed refresh session is saved again with encryption")
            Dim validProfile = File.ReadAllText(profileFile)
            restarted.SuspendConnection()

            server.Offline = True
            Dim offline As New CloudStorage(root, http)
            Check(Await offline.RestoreSessionAsync() = SessionRestoreResult.RetryLater, "Offline startup does not require reconfiguration")
            Check(offline.CaptureDestination().Id = target.Id, "Offline startup retains the destination for new session files")
            Check(File.ReadAllText(profileFile) = validProfile, "A network failure does not erase saved credentials")
            Dim setupFile = Path.Combine(root, "offline.json")
            SampleSetup().Save(setupFile)
            Check(offline.QueueSetup(setupFile), "Files saved while reconnecting are queued for the remembered database")
            server.Offline = False
            Await offline.SynchronizeAsync()
            Check(offline.PendingCount(target) = 0, "Automatic retry sends offline files when the network returns")
            offline.SuspendConnection()

            server.RejectRefresh = True
            Dim revoked As New CloudStorage(root, http)
            Check(Await revoked.RestoreSessionAsync() = SessionRestoreResult.SignInRequired, "Revoked refresh token requests sign-in")
            Check(Not revoked.IsConnected AndAlso revoked.CaptureDestination() Is Nothing, "Invalid saved identity is not activated")
            server.RejectRefresh = False
            Dim saved = StorageJson.Decode(File.ReadAllText(profileFile))
            saved("databaseUrl") = "https://other-default-rtdb.firebaseio.com"
            File.WriteAllText(profileFile, StorageJson.Encode(saved))
            Dim calls = server.Requests.Count
            Dim tampered As New CloudStorage(root, http)
            Check(Await tampered.RestoreSessionAsync() = SessionRestoreResult.SignInRequired, "Encrypted session is bound to its original database and API key")
            Check(server.Requests.Count = calls, "Mismatched profile is rejected before sending a token")
            File.WriteAllText(profileFile, validProfile)

            Dim local As New CloudStorage(root, http)
            Check(Await local.RestoreSessionAsync() = SessionRestoreResult.Connected, "Original encrypted profile still restores")
            local.Disconnect()
            Check(Not StorageJson.Decode(File.ReadAllText(profileFile)).ContainsKey("protectedSession"), "Use local only forgets the saved sign-in")
            Dim nextLaunch As New CloudStorage(root, http)
            calls = server.Requests.Count
            Check(Await nextLaunch.RestoreSessionAsync() = SessionRestoreResult.LocalOnly, "Explicit local-only choice survives restart")
            Check(server.Requests.Count = calls AndAlso nextLaunch.CaptureDestination() Is Nothing, "Local-only startup makes no network requests or cloud destination")
        End Using
    End Function

    Private Sub RunUiTests(root As String)
        Application.EnableVisualStyles()
        Using connection As New FirebaseConnectionDialog(New CloudStorage(Path.Combine(root, "ui")))
            connection.Show()
            Application.DoEvents()
            Dim fields = Descendants(connection).OfType(Of TextBox)().ToArray()
            Check(fields.Length = 4, "Connection dialog contains exactly four fields")
            Check(fields.Count(Function(field) field.UseSystemPasswordChar) = 1, "Password is masked")
            Using bitmap As New Bitmap(connection.Width, connection.Height)
                connection.DrawToBitmap(bitmap, New Rectangle(0, 0, bitmap.Width, bitmap.Height))
                bitmap.Save(Path.Combine(root, "connection-dialog.png"))
            End Using
            connection.Close()
        End Using
        'Show the real entry form, dismiss its startup prompt, and verify local controls remain usable.
        Using setup As New SetUp(), closer As New Windows.Forms.Timer With {.Interval = 50}
            setup.ConnectionStorage = New CloudStorage(Path.Combine(root, "startup-ui"))
            Dim prompts As Integer
            Dim elapsed As Integer
            AddHandler closer.Tick, Sub()
                                       elapsed += 50
                                       For Each form As Form In Application.OpenForms.Cast(Of Form)().ToArray()
                                           If TypeOf form Is FirebaseConnectionDialog Then
                                               prompts += 1
                                               form.Close()
                                           End If
                                       Next
                                       If elapsed > 5000 Then setup.Close()
                                   End Sub
            closer.Start()
            setup.Show()
            Application.DoEvents()
            closer.Stop()
            Check(prompts = 1, "Opening SetUp prompts once; closing the dialog allows startup")
            Check(Not setup.ConnectionStorage.IsConnected, "Closing startup prompt leaves local-only mode")
            Check(Descendants(setup).OfType(Of Button)().Any(Function(button) button.Text = "Configure RTDB…" AndAlso button.Enabled), "Persistent configuration button is available")
            Check(Descendants(setup).OfType(Of Button)().Any(Function(button) button.Text = "Load from RTDB…" AndAlso Not button.Enabled), "Remote load disabled while local-only")
            Check(setup.btnLoad.Enabled AndAlso setup.btnSave.Enabled, "Local load and save stay enabled")
            Using bitmap As New Bitmap(setup.Width, setup.Height)
                setup.DrawToBitmap(bitmap, New Rectangle(0, 0, bitmap.Width, bitmap.Height))
                bitmap.Save(Path.Combine(root, "setup-window.png"))
            End Using
            setup.Close()
        End Using
        Dim rememberedRoot = Path.Combine(root, "remembered-ui")
        Using http As New HttpClient(New FakeFirebase())
            Dim previousRun As New CloudStorage(rememberedRoot, http)
            Task.Run(Async Function()
                         Await previousRun.ConnectAsync(database, key, email, password, CancellationToken.None)
                     End Function).GetAwaiter().GetResult()
            previousRun.SuspendConnection()
            Using setup As New SetUp(), closer As New Windows.Forms.Timer With {.Interval = 25}
                setup.ConnectionStorage = New CloudStorage(rememberedRoot, http)
                Dim prompts As Integer
                AddHandler closer.Tick, Sub()
                                           For Each form In Application.OpenForms.Cast(Of Form)().ToArray()
                                               If TypeOf form Is FirebaseConnectionDialog Then
                                                   prompts += 1
                                                   form.Close()
                                               End If
                                           Next
                                       End Sub
                closer.Start()
                setup.Show()
                Dim watch = Diagnostics.Stopwatch.StartNew()
                Do
                    Application.DoEvents()
                    Thread.Sleep(10)
                Loop Until setup.ConnectionStorage.Status.StartsWith("RTDB connected", StringComparison.Ordinal) OrElse watch.ElapsedMilliseconds > 5000
                Check(prompts = 0, "Saved sign-in skips the startup configuration dialog")
                Check(setup.ConnectionStorage.IsConnected, "Main form reconnects to the remembered RTDB")
                Check(Descendants(setup).OfType(Of Button)().Any(Function(button) button.Text = "Load from RTDB…" AndAlso button.Enabled), "Automatic login enables cloud configuration loading")
                setup.Close()
                Check(Not setup.ConnectionStorage.IsConnected, "Closing main form cancels its active connection")
                Check(setup.ConnectionStorage.SavedConnection().ContainsKey("protectedSession"), "Closing main form keeps the saved login for next time")
            End Using
        End Using
    End Sub

    Private Iterator Function Descendants(control As Control) As IEnumerable(Of Control)
        For Each child As Control In control.Controls
            Yield child
            For Each descendant In Descendants(child)
                Yield descendant
            Next
        Next
    End Function
End Module

'No Firebase account, network, experimental hardware, or real data is used by these tests.
Friend Class FakeFirebase
    Inherits HttpMessageHandler
    Public ReadOnly Requests As New List(Of String)()
    Public ReadOnly Values As New Dictionary(Of String, Object)()
    Public RejectLogin As Boolean
    Public RejectRefresh As Boolean
    Public DenyWrites As Boolean
    Public Offline As Boolean
    Public LoseNextPatchAcknowledgement As Boolean
    Public ExpiresIn As String = "3600"
    Public Refreshes As Integer
    Private uid As String = "researcher"

    Protected Overrides Async Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        cancellationToken.ThrowIfCancellationRequested()
        Await Task.Yield()
        Requests.Add(request.Method.Method & " " & request.RequestUri.AbsolutePath)
        If Offline Then Throw New HttpRequestException("Simulated network outage")
        If request.RequestUri.Host = "identitytoolkit.googleapis.com" Then
            If RejectLogin Then Return Response(HttpStatusCode.BadRequest, "{}")
            Dim auth = StorageJson.Decode(Await request.Content.ReadAsStringAsync())
            uid = If(CStr(auth("email")) = "researcher@example.test", "researcher", "other")
            Return Response(HttpStatusCode.OK, StorageJson.Encode(New With {.idToken = "id-token", .refreshToken = "refresh-token", .expiresIn = ExpiresIn, .localId = uid}))
        End If
        If request.RequestUri.Host = "securetoken.googleapis.com" Then
            Refreshes += 1
            If RejectRefresh Then Return Response(HttpStatusCode.BadRequest, "{}")
            Return Response(HttpStatusCode.OK, StorageJson.Encode(New With {.id_token = "renewed-id-token", .refresh_token = "refresh-token", .expires_in = "3600", .user_id = uid}))
        End If
        Dim path = request.RequestUri.AbsolutePath.Replace("/" & FirebaseClient.Root, "")
        path = path.Substring(0, path.Length - ".json".Length)
        Dim database = "https://" & request.RequestUri.Host
        If request.Method = HttpMethod.Delete Then
            Values.Remove(database & path)
            Return Response(HttpStatusCode.OK, "null")
        End If
        If request.Method = HttpMethod.Get Then
            If path = "/configIndex" Then
                Dim index As New Dictionary(Of String, Object)()
                For Each entry In Values.Where(Function(item) item.Key.StartsWith(database & "/configIndex/", StringComparison.Ordinal))
                    index(entry.Key.Substring((database & "/configIndex/").Length)) = entry.Value
                Next
                Return Response(HttpStatusCode.OK, If(index.Count = 0, "null", StorageJson.Encode(index)))
            End If
            Return Response(HttpStatusCode.OK, If(Values.ContainsKey(database & path), StorageJson.Encode(Values(database & path)), "null"))
        End If
        If DenyWrites Then Return Response(HttpStatusCode.Forbidden, "{}")
        Dim data = StorageJson.Decode(Await request.Content.ReadAsStringAsync())
        If request.Method = HttpMethod.Put Then
            Values(database & path) = data
        ElseIf request.Method.Method = "PATCH" Then
            For Each entry In data
                Values(database & "/" & entry.Key) = entry.Value
            Next
            If LoseNextPatchAcknowledgement Then
                LoseNextPatchAcknowledgement = False
                Throw New HttpRequestException("Simulated lost acknowledgement")
            End If
        Else
            Throw New Exception("Unexpected HTTP method")
        End If
        Return Response(HttpStatusCode.OK, "null")
    End Function

    Private Function Response(code As HttpStatusCode, content As String) As HttpResponseMessage
        Return New HttpResponseMessage(code) With {.Content = New StringContent(content, Encoding.UTF8, "application/json")}
    End Function
End Class
