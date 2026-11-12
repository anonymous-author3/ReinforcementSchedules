Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports ReinforcementSchedules

Module VariableTests
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
            result.VariableValueCount(i) = 10
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
    Private Function CreateSession(first As ComponentBlueprint, Optional second As ComponentBlueprint = Nothing, Optional ici As Integer = 0) As ChamberSession
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

    Private Sub Reject(action As Action, message As String)
        Dim rejected As Boolean
        Try
            action()
        Catch ex As ArgumentException
            rejected = True
        End Try
        Check(rejected, message)
    End Sub


    <STAThread>
    Sub Main()
        Try
            For count As Integer = 1 To 1000
                For Each mean In {1, 2, 3, 5, 10, 30, 100, 10000}
                    Dim ratios = IntervalScheduleTiming.GenerateVariableRatios(mean, count)
                    Check(ratios.Count = count AndAlso ratios.All(Function(x) x >= 1), "positive ratio count")
                    Check(ratios.Sum(Function(x) CLng(x)) = CLng(mean) * count, "exact ratio mean")
                    Check(ratios.SequenceEqual(ratios.OrderBy(Function(x) x)), "ratios monotonic")
                    If IntervalScheduleTiming.ValidationError("Variable Interval", mean, count) = "" Then
                        Dim values = IntervalScheduleTiming.GenerateVariableSeconds(mean, count)
                        Check(Math.Abs(values.Average() - mean) < mean * 0.000000001, "FH mean")
                        For i As Integer = 0 To count - 1
                            Dim a = (count - i - 1) / CDbl(count)
                            Dim b = (count - i) / CDbl(count)
                            Dim oracle = mean * count * (b * (1 - Math.Log(b)) - If(a = 0, 0, a * (1 - Math.Log(If(a = 0, 1, a)))))
                            If Math.Abs(values(i) - oracle) > mean * 0.00000001 Then Throw New Exception("FH oracle")
                        Next
                    End If
                Next
            Next
            For Each count In {0, -1, 1001, Integer.MaxValue}
                Dim n = count
                Reject(Sub() IntervalScheduleTiming.GenerateVariableSeconds(30, n), "bad VI count")
                Reject(Sub() IntervalScheduleTiming.GenerateVariableRatios(30, n), "bad VR count")
            Next
            Check(IntervalScheduleTiming.ValidationError("Variable Interval", 1, 1000) <> "", "sub millisecond caught before start")
            Check(IntervalScheduleTiming.TimeValidationError("Variable Time", Double.NaN, 10) <> "", "NaN time rejected")
            Check(IntervalScheduleTiming.TimeValidationError("Variable Time", 0.001, 10) <> "", "small TV rejected")
            For Each count In {1, 2, 7, 10, 30, 1000}
                Dim bp = Blueprint(False)
                bp.ScheduleType(0) = "Variable Ratio" : bp.ScheduleValue(0) = 30 : bp.VariableValueCount(0) = count
                bp.ScheduleType(1) = "Variable Interval" : bp.ScheduleValue(1) = 30 : bp.VariableValueCount(1) = count
                bp.TimeScheduleType = "Variable Time" : bp.TimeScheduleValue = 30 : bp.TimeVariableValueCount = count
                Using session = CreateSession(bp)
                    Dim goals = DirectCast(Field(session, "RatioGoal"), Integer())
                    Dim timers = DirectCast(Field(session, "ScheduleTimers"), Timer())
                    Dim timeTimer = DirectCast(Field(session, "tmrTimeSchedule"), Timer)
                    For cycle As Integer = 1 To 5
                        Dim r As New List(Of Integer), v As New List(Of Integer), t As New List(Of Integer)
                        For i As Integer = 1 To count
                            r.Add(goals(0)) : v.Add(timers(1).Interval) : t.Add(timeTimer.Interval)
                            CallMethod(session, "VRGen", 0) : CallMethod(session, "VIGen", 1) : CallMethod(session, "ScheduleNextTimeReinforcer")
                        Next
                        Check(r.OrderBy(Function(x) x).SequenceEqual(IntervalScheduleTiming.GenerateVariableRatios(30, count)), "VR without replacement")
                        Dim expected = IntervalScheduleTiming.GenerateVariableSeconds(30, count).Select(Function(x) IntervalScheduleTiming.ToTimerMilliseconds(x)).OrderBy(Function(x) x)
                        Check(v.OrderBy(Function(x) x).SequenceEqual(expected), "VI without replacement")
                        Check(t.OrderBy(Function(x) x).SequenceEqual(expected), "VT without replacement")
                    Next
                End Using
            Next
            Dim c = Blueprint(False)
            c.VariableValueCount(0) = 23 : c.TimeVariableValueCount = 17
            Dim copy = ChamberConfiguration.CloneComponent(c)
            copy.VariableValueCount(0) = 9
            Check(c.VariableValueCount(0) = 23, "clone independence")
            Dim comps(MAX_COMPONENTS) As ComponentBlueprint : comps(1) = c
            Dim data As New SetupSaveData With {.Components = comps, .ComponentCount = 1, .BoxName = "Test", .StartDelay = "0", .PostSession = "0", .ICIDuration = "0", .ICIStimulus = "None", .ComponentOrder = "", .CommandMap = New ArduinoCommandMap()}
            Dim json = data.ToJson()
            Dim loaded = SetupSaveData.ReadJson(json)
            Check(loaded.Components(1).VariableValueCount(0) = 23 AndAlso loaded.Components(1).TimeVariableValueCount = 17, "JSON counts")
            Dim old = Blueprint(False) : old.VariableValueCount = Nothing : old.TimeVariableValueCount = 0
            comps(1) = old : data.FormatVersion = 6 : data.Components = comps
            Dim xml As String
            Using stream As New MemoryStream()
                Dim serializer As New Runtime.Serialization.DataContractSerializer(GetType(SetupSaveData))
                serializer.WriteObject(stream, data)
                xml = Text.Encoding.UTF8.GetString(stream.ToArray())
            End Using
            Dim legacy = SetupSaveData.ReadXml(xml)
            Check(legacy.Components(1).VariableValueCount.All(Function(x) x = 10) AndAlso legacy.Components(1).TimeVariableValueCount = 10, "legacy counts default 10")
            Using setup As New SetUp()
                Module1.vCC = 1 : Module1.AC(1) = c
                Dim lines = DirectCast(CallMethod(setup, "ConfigurationSnapshotLines", True), String())
                File.WriteAllLines("VariableTests/counts.txt", lines.Where(Function(line) Not line.StartsWith("Arduino ")))
                CallMethod(setup, "LoadLegacyConfiguration", Path.GetFullPath("VariableTests/counts.txt"))
                Check(Module1.AC(1).VariableValueCount(0) = 23 AndAlso Module1.AC(1).TimeVariableValueCount = 17, "TXT counts")
            End Using
            Using editor As New Component()
                CallMethod(editor, "Component_Load", editor, EventArgs.Empty)
                Dim vi = DirectCast(Field(editor, "rdoVIL1"), RadioButton)
                vi.Checked = True
                Dim n = DirectCast(Field(editor, "variableValues"), NumericUpDown)
                n.Value = 23
                Dim info = CallMethod(editor, "BuildInputFromEditor", 1)
                Check(CInt(Field(info, "VariableValueCount")) = 23, "editor input captures count")
                Dim vt = DirectCast(Field(editor, "chkVariableTime"), CheckBox) : vt.Checked = True
                Dim tn = DirectCast(Field(editor, "timeVariableValues"), NumericUpDown)
                Check(tn.Parent IsNot Nothing AndAlso tn.Value = 10, "VT UI default")
                CallMethod(editor, "ResetInputEditor")
                Check(n.Value = 10, "input reset")
                editor.Width = 1100 : editor.Height = 850
                editor.Show()
                vi.Checked = True : vt.Checked = True
                Application.DoEvents()
                Using bmp As New Drawing.Bitmap(editor.Width, editor.Height)
                    editor.DrawToBitmap(bmp, New Drawing.Rectangle(0, 0, bmp.Width, bmp.Height))
                    bmp.Save("VariableTests/editor.png")
                End Using
                editor.Close()
            End Using
            Console.WriteLine("PASS " & assertions & " assertions; exhaustive counts 1-1000, 8 means and runtime/storage/UI.")
        Catch ex As Exception
            Console.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
End Module
