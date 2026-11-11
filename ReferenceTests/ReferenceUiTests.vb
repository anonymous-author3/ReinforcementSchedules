Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports ReinforcementSchedules

Module ReferenceUiTests
    Private assertions As Integer
    Private Const flags As BindingFlags = BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Instance
    Private Function Field(o As Object, name As String) As Object
        Dim member = o.GetType().GetField(name, flags)
        If member IsNot Nothing Then Return member.GetValue(o)
        Return o.GetType().GetProperty(name, flags).GetValue(o)
    End Function
    Private Sub SetField(o As Object, name As String, value As Object)
        o.GetType().GetField(name, flags).SetValue(o, value)
    End Sub
    Private Function CallMethod(o As Object, name As String, ParamArray args() As Object) As Object
        Return o.GetType().GetMethod(name, flags).Invoke(o, args)
    End Function
    Private Sub Check(value As Boolean, message As String)
        If Not value Then Throw New Exception("FAIL: " & message)
        assertions += 1
    End Sub
    Private Function Blueprint(lag As Boolean, Optional length As Integer = 4, Optional lagValue As Integer = 5) As ComponentBlueprint
        Dim boxed As ValueType = New ComponentBlueprint With {.InputCount = 6, .ComponentDuration = 600, .ComponentIteration = 10, .ComponentName = "Test", .ComponentStimType = "None", .TimeScheduleType = "None", .LagEnabled = lag, .LagSequenceLength = length, .LagValue = lagValue}
        For Each member In GetType(ComponentBlueprint).GetFields(flags)
            If member.FieldType.IsArray Then member.SetValue(boxed, Array.CreateInstance(member.FieldType.GetElementType(), MAX_INPUTS))
        Next
        Dim result = DirectCast(boxed, ComponentBlueprint)
        For i As Integer = 0 To 5
            result.InputName(i) = "Input " & (i + 1)
            result.ScheduleType(i) = If(i < 2, If(lag, "Lag", "Fixed Ratio"), "N/A")
            result.ScheduleValue(i) = 2
            result.Magnitude(i) = If(i < 2, 1, 0)
            result.Reinforcer(i) = If(i < 2, "Pellet", "N/A")
            result.DeliveryP(i) = 100
            result.FeedbackType(i) = "None"
            result.DelayType(i) = "None"
            result.TStimD(i) = "None"
            result.TStimDelta(i) = "None"
        Next
        Return result
    End Function
    Private Function Session(first As ComponentBlueprint, Optional second As ComponentBlueprint = Nothing, Optional ici As Integer = 0) As ChamberSession
        Dim blueprints(MAX_COMPONENTS) As ComponentBlueprint
        blueprints(1) = first
        Dim count As Integer = 1
        If second.InputCount > 0 Then
            blueprints(2) = second
            count = 2
        End If
        Dim config As New ChamberConfiguration With {.BoxName = "Test", .Subject = "Simulation", .SessionName = "No hardware", .ComPort = "COM999", .CommandMap = New ArduinoCommandMap(), .Components = blueprints, .ComponentCount = count, .SnapshotLines = {}, .ICISeconds = ici, .ICIStimulus = "None"}
        Dim result As New ChamberSession(config)
        SetField(result, "SimulationMode", True)
        SetField(result, "simulationCommands", New List(Of String))
        SetField(result, "SessionStarted", True)
        Dim delays(count, MAX_INPUTS - 1) As List(Of Integer)
        For c As Integer = 0 To count
            For i As Integer = 0 To MAX_INPUTS - 1
                delays(c, i) = New List(Of Integer)
            Next
        Next
        SetField(result, "ObtainedDelayDurations", delays)
        SetField(result, "rawWriter", New StreamWriter(New MemoryStream()) With {.AutoFlush = True})
        CallMethod(result, "BeginPrograms")
        Return result
    End Function
    Private Sub Press(session As ChamberSession, sequence As String)
        For Each digit As Char In sequence
            SetField(session, "vTimeNow", CInt(Field(session, "vTimeNow")) + 10)
            CallMethod(session, "Response", Integer.Parse(digit.ToString()) - 1)
        Next
    End Sub
    Private Function Raw(session As ChamberSession) As String
        Dim writer = DirectCast(Field(session, "rawWriter"), StreamWriter)
        writer.Flush()
        Return Text.Encoding.UTF8.GetString(DirectCast(writer.BaseStream, MemoryStream).ToArray())
    End Function
    Private Function Tracker(session As ChamberSession, Optional component As Integer = 1) As LagSequenceTracker
        Return DirectCast(Field(session, "lagTrackers"), LagSequenceTracker())(component)
    End Function
    Private Sub Advance(session As ChamberSession)
        CallMethod(session, "ComponentDuration_Code")
    End Sub

    Private Function Reference(session As ChamberSession) As SessionReferenceData
        Dim header = """Session UID: 11111111111111111111111111111111""" & Environment.NewLine & """ReferenceFormat"",2" & Environment.NewLine
        Return SessionReferenceData.ParseRaw(header & Raw(session) & If(Raw(session).Contains("ReferenceClosed"), "", """ReferenceClosed"",""Completed""" & Environment.NewLine), "", "Simulation only").SelectComponent(1)
    End Function
    Private Sub SetElapsed(session As ChamberSession, milliseconds As Integer)
        SetField(session, "componentStartedTimestamp", Diagnostics.Stopwatch.GetTimestamp() - CLng(milliseconds * Diagnostics.Stopwatch.Frequency / 1000.0))
    End Sub

    <STAThread>
    Sub Main()
        Dim root = Path.Combine(Path.GetTempPath(), "ReferenceUi_" & Guid.NewGuid().ToString("N"))
        Try
            Dim b = Blueprint(True, 2, 2)
            Dim prior As SessionReferenceData
            Using original = Session(b)
                Press(original, "112211")
                prior = Reference(original)
            End Using
            prior.Subject = "Rat source" : prior.SessionName = "Day 1"
            b.LagHistoryReference = prior
            Module1.AC(1) = b
            Module1.vCC = 1
            Using form As New SetUp()
                form.ConnectionStorage = New CloudStorage(root)
                form.ConnectionStorage.Disconnect()
                form.Show()
                Application.DoEvents()
                DirectCast(Field(form, "txtSubject"), TextBox).Text = "Simulation"
                form.Refresh()
                Application.DoEvents()
                Check(form.Controls.Find("btnSessionReferences", True).Length = 1, "reference button exists in setup")
                Using picture As New Drawing.Bitmap(form.Width, form.Height)
                    form.DrawToBitmap(picture, New Drawing.Rectangle(0, 0, picture.Width, picture.Height))
                    picture.Save("ReferenceTests\ReferenceSetup.png")
                End Using
                For run As Integer = 0 To 1
                    Dim accept = run = 1
                    Dim found As Boolean = False
                    Dim failure As Exception = Nothing
                    Using timer As New Timer With {.Interval = 100}
                        AddHandler timer.Tick, Sub()
                            Dim dialog = Application.OpenForms.Cast(Of Form)().FirstOrDefault(Function(f) f.Text.StartsWith("Session references for "))
                            If dialog Is Nothing Then Return
                            timer.Stop()
                            Try
                                found = True
                                Dim status = DirectCast(dialog.Controls.Find("lblReferenceStatus", True).Single(), Label)
                                Check(status.Text.Contains("Prior history:") AndAlso status.Text.Contains(prior.SessionUid), "reference UID visible before apply")
                                If Not accept Then
                                    Using picture As New Drawing.Bitmap(dialog.Width, dialog.Height)
                                        dialog.DrawToBitmap(picture, New Drawing.Rectangle(0, 0, picture.Width, picture.Height))
                                        picture.Save("ReferenceTests\ReferenceDialog.png")
                                    End Using
                                End If
                                DirectCast(dialog.Controls.Cast(Of Control)().Single(Function(c) c.Text = "Clear history"), Button).PerformClick()
                                Check(status.Text.StartsWith("Prior history: none"), "clear updates staged reference only")
                                DirectCast(dialog.Controls.Cast(Of Control)().Single(Function(c) c.Text = If(accept, "Apply references", "Cancel")), Button).PerformClick()
                            Catch ex As Exception
                                failure = ex
                                dialog.Close()
                            End Try
                        End Sub
                        timer.Start()
                        CallMethod(form, "EditSessionReferences", Nothing, EventArgs.Empty)
                    End Using
                    If failure IsNot Nothing Then Throw failure
                    Check(found, "reference editor opened")
                    Check((Module1.AC(1).LagHistoryReference Is Nothing) = accept, If(accept, "Apply commits cleared reference", "Cancel preserves original reference"))
                Next
                CallMethod(form, "DisposeCloudControls", Nothing, New FormClosedEventArgs(CloseReason.None))
            End Using
            Console.WriteLine("PASS reference UI: " & assertions & " assertions; no hardware, no real RTDB, no saved user settings.")
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
End Module
