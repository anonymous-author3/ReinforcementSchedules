Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.Web.Script.Serialization

Friend Module StorageJson
    Public Function Encode(value As Object) As String
        Return New JavaScriptSerializer With {.MaxJsonLength = 64 * 1024 * 1024}.Serialize(value)
    End Function

    Public Function Decode(text As String) As Dictionary(Of String, Object)
        Return New JavaScriptSerializer With {.MaxJsonLength = 64 * 1024 * 1024}.Deserialize(Of Dictionary(Of String, Object))(text)
    End Function

    Public Function Hash(value As String) As String
        Using sha = SHA256.Create()
            Return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant()
        End Using
    End Function

    Public Sub WriteAtomically(destination As String, content As String)
        Directory.CreateDirectory(Path.GetDirectoryName(destination))
        Dim temporary = Path.Combine(Path.GetDirectoryName(destination), Guid.NewGuid().ToString("N") & ".tmp")
        Try
            Using stream As New FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                Dim bytes = New UTF8Encoding(False).GetBytes(content)
                stream.Write(bytes, 0, bytes.Length)
                stream.Flush(True)
            End Using
            If File.Exists(destination) Then
                File.Replace(temporary, destination, Nothing)
            Else
                File.Move(temporary, destination)
            End If
        Finally
            If File.Exists(temporary) Then File.Delete(temporary)
        End Try
    End Sub
End Module

Friend Class FirebaseProfile
    Public ReadOnly DatabaseUrl As String
    Public ReadOnly ApiKey As String

    Public Sub New(url As String, key As String)
        Dim address As Uri = Nothing
        If Not Uri.TryCreate(If(url, "").Trim(), UriKind.Absolute, address) OrElse
            address.Scheme <> "https" OrElse Not address.IsDefaultPort OrElse address.UserInfo <> "" OrElse
            address.AbsolutePath <> "/" OrElse address.Query <> "" OrElse address.Fragment <> "" OrElse
            Not (address.Host.EndsWith(".firebaseio.com", StringComparison.OrdinalIgnoreCase) OrElse
                 address.Host.EndsWith(".firebasedatabase.app", StringComparison.OrdinalIgnoreCase)) Then
            Throw New ArgumentException("Enter the HTTPS root URL of your RTDB, without a path or .json.")
        End If
        key = If(key, "").Trim()
        If Not System.Text.RegularExpressions.Regex.IsMatch(key, "^[A-Za-z0-9_-]{20,200}$") Then
            Throw New ArgumentException("Enter the complete Firebase Web API key for the same project.")
        End If
        DatabaseUrl = "https://" & address.Host.ToLowerInvariant()
        ApiKey = key
    End Sub
End Class

Friend Class CloudDestination
    Public ReadOnly DatabaseUrl As String
    Public ReadOnly UserId As String

    Public Sub New(databaseUrl As String, uid As String)
        Me.DatabaseUrl = databaseUrl
        UserId = uid
    End Sub

    Public ReadOnly Property Id As String
        Get
            Return StorageJson.Hash(DatabaseUrl & "|" & UserId)
        End Get
    End Property
End Class

Friend Class FirebaseSignInRequiredException
    Inherits IOException

    Public Sub New()
        MyBase.New("The saved Firebase session is no longer valid. Sign in again using Configure RTDB.")
    End Sub
End Class

'REST keeps the desktop application independent of the Unity Firebase SDK.
Friend Class FirebaseClient
    Public Const Root As String = "reinforcementSchedules"
    Private ReadOnly profile As FirebaseProfile
    Private ReadOnly http As HttpClient
    Private ReadOnly tokenLock As New SemaphoreSlim(1, 1)
    Private idToken As String
    Private refreshToken As String
    Private expiresAt As DateTime
    Private uid As String
    Public Event TokenRenewed As EventHandler

    Public Sub New(settings As FirebaseProfile, transport As HttpClient)
        profile = settings
        http = transport
    End Sub

    Public ReadOnly Property Destination As CloudDestination
        Get
            Return New CloudDestination(profile.DatabaseUrl, uid)
        End Get
    End Property

    Public ReadOnly Property SessionRefreshToken As String
        Get
            Return refreshToken
        End Get
    End Property

    Public Sub RestoreSession(savedToken As String, savedUid As String)
        If String.IsNullOrWhiteSpace(savedToken) OrElse String.IsNullOrWhiteSpace(savedUid) Then
            Throw New FirebaseSignInRequiredException()
        End If
        refreshToken = savedToken
        uid = savedUid
    End Sub

    Public Async Function SignInAsync(email As String, password As String, cancellation As CancellationToken) As Task
        If String.IsNullOrWhiteSpace(email) OrElse Not email.Contains("@") OrElse String.IsNullOrEmpty(password) Then
            Throw New ArgumentException("Enter your researcher email and password.")
        End If
        Dim body = StorageJson.Encode(New With {.email = email.Trim(), .password = password, .returnSecureToken = True})
        Using request As New HttpRequestMessage(HttpMethod.Post,
            "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" & Uri.EscapeDataString(profile.ApiKey))
            request.Content = New StringContent(body, Encoding.UTF8, "application/json")
            ApplyTokens(StorageJson.Decode(Await SendAsync(request, cancellation)), False)
        End Using
    End Function

    Private Sub ApplyTokens(values As Dictionary(Of String, Object), refreshed As Boolean)
        Dim newUid = CStr(values(If(refreshed, "user_id", "localId")))
        If String.IsNullOrWhiteSpace(newUid) OrElse (uid IsNot Nothing AndAlso uid <> newUid) Then
            Throw New FirebaseSignInRequiredException()
        End If
        idToken = CStr(values(If(refreshed, "id_token", "idToken")))
        refreshToken = CStr(values(If(refreshed, "refresh_token", "refreshToken")))
        If String.IsNullOrWhiteSpace(idToken) OrElse String.IsNullOrWhiteSpace(refreshToken) Then
            Throw New IOException("Firebase returned an incomplete authentication response.")
        End If
        uid = newUid
        expiresAt = DateTime.UtcNow.AddSeconds(Convert.ToDouble(values(If(refreshed, "expires_in", "expiresIn")), CultureInfo.InvariantCulture) - 60)
        If refreshed Then RaiseEvent TokenRenewed(Me, EventArgs.Empty)
    End Sub

    Private Async Function EnsureTokenAsync(cancellation As CancellationToken) As Task
        Await tokenLock.WaitAsync(cancellation)
        Try
            If idToken IsNot Nothing AndAlso DateTime.UtcNow < expiresAt Then Return
            If String.IsNullOrEmpty(refreshToken) Then Throw New IOException("Sign in to RTDB again.")
            Using request As New HttpRequestMessage(HttpMethod.Post,
                "https://securetoken.googleapis.com/v1/token?key=" & Uri.EscapeDataString(profile.ApiKey))
                request.Content = New FormUrlEncodedContent(New Dictionary(Of String, String) From {
                    {"grant_type", "refresh_token"}, {"refresh_token", refreshToken}})
                ApplyTokens(StorageJson.Decode(Await SendAsync(request, cancellation)), True)
            End Using
        Finally
            tokenLock.Release()
        End Try
    End Function

    Public Async Function ReadAsync(path As String, cancellation As CancellationToken) As Task(Of Dictionary(Of String, Object))
        Return StorageJson.Decode(Await DatabaseAsync(HttpMethod.Get, path, Nothing, cancellation))
    End Function

    Public Async Function ReadShallowAsync(path As String, cancellation As CancellationToken) As Task(Of Dictionary(Of String, Object))
        If path <> "sessions" Then Throw New ArgumentException("Shallow reads are restricted to the session catalog.")
        Return StorageJson.Decode(Await DatabaseAsync(HttpMethod.Get, path, Nothing, cancellation, True))
    End Function
    Public Async Function ReadSessionMetadataAsync(sessionId As String, cancellation As CancellationToken) As Task(Of Dictionary(Of String, Object))
        Dim parsed As Guid
        If Not Guid.TryParseExact(sessionId, "N", parsed) Then Throw New ArgumentException("Invalid session ID.")
        Dim result As New Dictionary(Of String, Object)
        For Each key In {"sessionUid", "subject", "sessionName", "savedAtUtc"}
            Dim body = Await DatabaseAsync(HttpMethod.Get, "sessions/" & sessionId & "/" & key, Nothing, cancellation)
            Dim value = New JavaScriptSerializer With {.MaxJsonLength = 65536}.DeserializeObject(body)
            If Not TypeOf value Is String Then Throw New InvalidDataException("Incomplete legacy session metadata.")
            result(key) = value
        Next
        Return result
    End Function
    Public Async Function PatchAsync(updates As Dictionary(Of String, Object), cancellation As CancellationToken) As Task
        Await DatabaseAsync(New HttpMethod("PATCH"), "", StorageJson.Encode(updates), cancellation)
    End Function

    Public Async Function CheckAccessAsync(cancellation As CancellationToken) As Task
        Await ReadAsync("configIndex", cancellation)
        Dim marker = "connectionChecks/" & Guid.NewGuid().ToString("N")
        'A dedicated disposable marker verifies write access without creating experimental data.
        Dim writeError As Exception = Nothing
        Try
            Await DatabaseAsync(HttpMethod.Put, marker, StorageJson.Encode(New With {.uid = uid}), cancellation)
        Catch ex As Exception
            writeError = ex
        End Try
        Try
            Await DatabaseAsync(HttpMethod.Delete, marker, Nothing, CancellationToken.None)
        Catch ex As Exception
            If writeError Is Nothing Then Throw New IOException("Could not remove the RTDB connection check. Verify database permissions.")
        End Try
        If writeError IsNot Nothing Then Throw writeError
        cancellation.ThrowIfCancellationRequested()
    End Function

    Private Async Function DatabaseAsync(method As HttpMethod, path As String, body As String, cancellation As CancellationToken, Optional shallow As Boolean = False) As Task(Of String)
        Await EnsureTokenAsync(cancellation)
        Dim url = profile.DatabaseUrl & "/" & Root & If(path = "", "", "/" & path) & ".json?auth=" & Uri.EscapeDataString(idToken) & If(shallow, "&shallow=true", "")
        Using request As New HttpRequestMessage(method, url)
            If body IsNot Nothing Then request.Content = New StringContent(body, Encoding.UTF8, "application/json")
            Return Await SendAsync(request, cancellation)
        End Using
    End Function

    Private Async Function SendAsync(request As HttpRequestMessage, cancellation As CancellationToken) As Task(Of String)
        Try
            Using response = Await http.SendAsync(request, cancellation)
                Dim body = Await response.Content.ReadAsStringAsync()
                If Not response.IsSuccessStatusCode Then
                    If request.RequestUri.Host = "securetoken.googleapis.com" AndAlso
                        (response.StatusCode = HttpStatusCode.BadRequest OrElse response.StatusCode = HttpStatusCode.Unauthorized OrElse
                         response.StatusCode = HttpStatusCode.Forbidden) Then Throw New FirebaseSignInRequiredException()
                    Dim message = "Firebase request failed (HTTP " & CInt(response.StatusCode) & ")."
                    If response.StatusCode = HttpStatusCode.Unauthorized OrElse response.StatusCode = HttpStatusCode.Forbidden Then
                        message = "RTDB access denied. Check the account and database rules for /" & Root & "."
                    ElseIf request.RequestUri.Host = "identitytoolkit.googleapis.com" Then
                        message = "Sign-in failed. Check the API key, email, password, and Firebase Email/Password sign-in settings."
                    ElseIf request.RequestUri.Host = "securetoken.googleapis.com" Then
                        message = "Your Firebase session could not be renewed. Sign in again."
                    End If
                    Throw New IOException(message)
                End If
                Return body
            End Using
        Catch ex As HttpRequestException
            'Do not display request URLs, tokens, credentials, or raw server responses.
            Throw New IOException("Could not reach Firebase. Local files are kept; try again when the connection returns.")
        Catch ex As TaskCanceledException When Not cancellation.IsCancellationRequested
            Throw New IOException("Firebase timed out. Local files are kept; try again later.")
        End Try
    End Function
End Class
