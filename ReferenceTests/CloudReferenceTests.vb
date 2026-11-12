Imports System
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Generic
Friend Class SetupSaveData
    Public Property SavedAtUtc As DateTime
    Public Shared Function Read(s As String) As SetupSaveData
        Return New SetupSaveData
    End Function
    Public Shared Function ReadJson(s As String) As SetupSaveData
        Return New SetupSaveData
    End Function
    Public Function ToJson() As String
        Return "{}"
    End Function
    Public Shared Function ReadXml(s As String) As SetupSaveData
        Return New SetupSaveData
    End Function
End Class
Friend Class FakeHandler
    Inherits HttpMessageHandler
    Public ReadOnly Requests As New List(Of String)
    Public Property SessionRaw As String
    Public Property SessionSummary As String
    Public Const FirstId As String = "0123456789abcdef0123456789abcdef"
    Public Const SecondId As String = "abcdef0123456789abcdef0123456789"
    Public Shared Function Meta(id As String, subject As String, dateText As String) As Dictionary(Of String, Object)
        Return New Dictionary(Of String,Object) From {{"sessionUid",id},{"subject",subject},{"sessionName","Test"},{"savedAtUtc",dateText},{"schemaVersion",1}}
    End Function
    Protected Overrides Function SendAsync(request As HttpRequestMessage, token As CancellationToken) As Task(Of HttpResponseMessage)
        Requests.Add(request.Method.Method & " " & request.RequestUri.PathAndQuery)
        If request.Method <> HttpMethod.Get Then Throw New Exception("Unexpected write from reference reader")
        Dim p = request.RequestUri.AbsolutePath
        Dim obj As Object = Nothing
        If p.EndsWith("/sessionIndex.json") Then
            obj = New Dictionary(Of String,Object) From {{FirstId, Meta(FirstId,"Rat A","2026-11-09T00:00:00Z")}}
        ElseIf p.EndsWith("/sessions.json") AndAlso request.RequestUri.Query.Contains("shallow=true") Then
            obj = New Dictionary(Of String,Object) From {{FirstId,True},{SecondId,True}}
        ElseIf p.Contains("/sessions/" & SecondId & "/") Then
            Dim key = Path.GetFileNameWithoutExtension(p)
            obj = Meta(SecondId,"Rat B","2026-11-10T00:00:00Z")(key)
        ElseIf p.EndsWith("/sessions/" & FirstId & ".json") Then
            Dim data = Meta(FirstId,"Rat A","2026-11-09T00:00:00Z")
            data("files") = New Dictionary(Of String,Object) From {{"raw",New With {.encoding="utf-8", .content=SessionRaw}}, {"summary",New With {.encoding="utf-8",.content=SessionSummary}}}
            obj = data
        Else
            Throw New Exception("Unexpected URL " & p)
        End If
        Return Task.FromResult(New HttpResponseMessage(HttpStatusCode.OK) With {.Content=New StringContent(StorageJson.Encode(obj))})
    End Function
End Class
Module CloudReferenceTests
    Dim checks As Integer
    Sub Check(v As Boolean, label As String)
        If Not v Then Throw New Exception(label)
        checks += 1
    End Sub
    Sub SetField(o As Object, name As String, value As Object)
        o.GetType().GetField(name, BindingFlags.Instance Or BindingFlags.NonPublic).SetValue(o,value)
    End Sub
    Sub Main()
        Run().GetAwaiter().GetResult()
    End Sub
    Async Function Run() As Task
        Dim handler As New FakeHandler With {.SessionRaw="""Session UID: " & FakeHandler.FirstId & """" & vbLf & "0,1,""StartComponent1""" & vbLf & "10,1,""LagSequence"",1,""12"",""Pass"",0,5", .SessionSummary="""Session UID: " & FakeHandler.FirstId & """" & vbLf & """Subject: Rat A"""}
        Dim http As New HttpClient(handler)
        Dim client As New FirebaseClient(New FirebaseProfile("https://test-default-rtdb.firebaseio.com", New String("k"c,32)),http)
        SetField(client,"idToken","synthetic-token")
        SetField(client,"uid","synthetic-user")
        SetField(client,"expiresAt",DateTime.UtcNow.AddHours(1))
        Dim storage As New CloudStorage(Path.Combine(Environment.CurrentDirectory,"obj","ReferenceParserValidation","cloud"),http)
        SetField(storage,"active",client)
        SetField(storage,"activeCancellation", New CancellationTokenSource)
        Dim list = Await storage.ListSessionsAsync("Rat A")
        Check(list.Count=2 AndAlso list(0).Id=FakeHandler.FirstId AndAlso list(0).Suggested,"exact subject suggestion")
        Check(handler.Requests.Count=6,"index+shallow+four legacy metadata reads")
        Check(Not handler.Requests.Any(Function(r) r.Contains("/files") OrElse r.Contains("chart")),"no catalog blobs")
        Dim reference=Await storage.ReadSessionReferenceAsync(list(0))
        Check(reference.Subject="Rat A" AndAlso reference.Sequences.Count=1,"selected read")
        list(0).DestinationId="different"
        Try
            Await storage.ReadSessionReferenceAsync(list(0))
            Throw New Exception("Wrong destination accepted")
        Catch ex As IOException
            checks += 1
        End Try
        Check(handler.Requests.All(Function(r) r.StartsWith("GET ")),"no writes")
        Dim root=Path.Combine(Environment.CurrentDirectory,"obj","ReferenceParserValidation","cloud")
        Directory.CreateDirectory(root)
        Dim raw=Path.Combine(root,"Raw.txt"), summary=Path.Combine(root,"Summary.txt")
        File.WriteAllText(raw,handler.SessionRaw): File.WriteAllText(summary,handler.SessionSummary)
        storage.QueueSession(client.Destination,FakeHandler.FirstId,"Box","Rat A","Test","Completed",raw,summary,Path.Combine(root,"missing.png"))
        Dim pending=Directory.GetFiles(root,"*.json",SearchOption.AllDirectories).Where(Function(p) p.Contains("pending")).Last()
        Dim updates=DirectCast(GetType(CloudStorage).GetMethod("PendingUpdates",BindingFlags.Instance Or BindingFlags.NonPublic).Invoke(storage,{pending,client.Destination}),Dictionary(Of String,Object))
        Check(updates.ContainsKey("sessions/" & FakeHandler.FirstId) AndAlso updates.ContainsKey("sessionIndex/" & FakeHandler.FirstId),"atomic session+index patch")
        Check(Not DirectCast(updates("sessionIndex/" & FakeHandler.FirstId),Dictionary(Of String,Object)).ContainsKey("files"),"index excludes blobs")
        Console.WriteLine("PASS " & checks & " cloud reference assertions; fake HTTP only")
    End Function
End Module
