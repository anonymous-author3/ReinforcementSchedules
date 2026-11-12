Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports ReinforcementSchedules

Module ChainTests
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



    Private Function Link(kind As String, value As Double, Optional stimulus As String = "None", Optional retract As Boolean = False) As ClassicChainLink
        Return New ClassicChainLink With {.ScheduleType = kind, .Value = value, .StimulusType = stimulus, .RetractInput = retract}
    End Function
    Private Function WithChain(ParamArray links() As ClassicChainLink) As ComponentBlueprint
        Dim bp = Blueprint(False)
        bp.ClassicChains = New ClassicChainDefinition(MAX_INPUTS - 1) {}
        bp.ClassicChains(0) = New ClassicChainDefinition With {.Links = links.ToList()}
        bp.ScheduleType(0) = links(0).ScheduleType
        bp.ScheduleValue(0) = Math.Max(1, CInt(links(0).Value))
        Return bp
    End Function
    Private Function State(session As ChamberSession, Optional input As Integer = 0) As Object
        Return DirectCast(Field(session, "chainStates"), Array).GetValue(input)
    End Function
    Private Sub Elapse(session As ChamberSession, Optional input As Integer = 0)
        SetField(State(session, input), "Deadline", Diagnostics.Stopwatch.GetTimestamp() - 1)
        CallMethod(session, "ScheduleTimer_Tick", DirectCast(Field(session, "ScheduleTimers"), Timer())(input), EventArgs.Empty)
    End Sub
    <STAThread>
    Sub Main()
        Try
            Dim bp = WithChain(Link("Fixed Interval", 30), Link("Fixed Ratio", 5))
            Using session = CreateSession(bp)
                Press(session, "11111")
                Check(CInt(Field(State(session), "LinkIndex")) = 0 AndAlso session.TotalReinforcers = 0, "FI cannot complete early")
                Elapse(session)
                Check(CInt(Field(State(session), "LinkIndex")) = 0, "FI expiry still requires response")
                Press(session, "1")
                Check(CInt(Field(State(session), "LinkIndex")) = 1 AndAlso CInt(Field(State(session), "RatioCount")) = 0, "terminal response not reused")
                Press(session, "1111")
                Check(session.TotalReinforcers = 0, "FR five not four")
                Press(session, "1")
                Check(session.TotalReinforcers = 1 AndAlso CInt(Field(State(session), "LinkIndex")) = 0, "one terminal reward and restart")
                Check(Raw(session).Contains("ChainLinkComplete") AndAlso Raw(session).Contains("ChainComplete"), "events")
                CallMethod(session, "ScheduleTimer_Tick", DirectCast(Field(session, "ScheduleTimers"), Timer())(0), EventArgs.Empty)
                Check(Not CBool(Field(State(session), "Ready")), "stale callback cannot ready new FI")
            End Using
            bp = WithChain(Link("Fixed Time", 10, "Light 1", True), Link("Fixed Ratio", 2, "Tone"))
            Using session = CreateSession(bp)
                Dim commands = DirectCast(Field(session, "simulationCommands"), List(Of String))
                Check(commands.Contains("l"), "FT retract")
                Press(session, "111")
                Check(CInt(Field(State(session), "LinkIndex")) = 0, "FT ignores response criterion")
                Elapse(session)
                Check(CInt(Field(State(session), "LinkIndex")) = 1 AndAlso commands.Contains("L"), "FT advances automatically and extends next link")
                Press(session, "11")
                Check(session.TotalReinforcers = 1 AndAlso CInt(Field(State(session), "LinkIndex")) = 0, "FT-FR restarts")
            End Using
            For Each kind In {"Variable Interval", "Variable Ratio", "Variable Time"}
                bp = WithChain(Link(kind, 30))
                bp.ClassicChains(0).Links(0).VariableValueCount = 7
                Using session = CreateSession(bp)
                    Dim values As New List(Of Double)
                    For i As Integer = 1 To 7
                        values.Add(CDbl(Field(State(session), "Criterion")))
                        If kind.Contains("Ratio") Then
                            Press(session, New String("1"c, CInt(Field(State(session), "Criterion"))))
                        Else
                            Elapse(session)
                            If kind.Contains("Interval") Then Press(session, "1")
                        End If
                    Next
                    Check(values.Count = 7 AndAlso Math.Abs(values.Sum() - 210) < 0.00001, kind & " exact mean without replacement")
                    Check(session.TotalReinforcers = 7, kind & " one reward per chain")
                End Using
            Next
            bp = WithChain(Link("Fixed Ratio", 1), Link("Fixed Ratio", 1))
            bp.MaxRefs = 1
            Dim nextBp = Blueprint(False)
            Using session = CreateSession(bp, nextBp)
                Press(session, "11")
                Check(CInt(Field(session, "vCC")) = 2 AndAlso State(session) Is Nothing, "MaxRefs cuts chain before restart")
            End Using
            bp = WithChain(Link("Fixed Time", 1))
            Using session = CreateSession(bp, nextBp)
                SetField(session, "componentStartedTimestamp", Diagnostics.Stopwatch.GetTimestamp() - CLng(700.0 * Diagnostics.Stopwatch.Frequency))
                Elapse(session)
                Check(session.TotalReinforcers = 0 AndAlso CInt(Field(session, "vCC")) = 2, "component deadline beats due time reward")
            End Using
            bp = WithChain(Link("Fixed Ratio", 1), Link("Fixed Time", 1))
            bp.DelayDuration(0) = 2 : bp.DelayType(0) = "Unsignaled" : bp.DelayRetract(0) = True
            Using session = CreateSession(bp)
                Press(session, "1") : Elapse(session)
                Check(CBool(Field(State(session), "PendingReinforcer")) AndAlso session.TotalReinforcers = 0, "legacy terminal delay retained")
                CallMethod(session, "DelayTimer_Tick", DirectCast(Field(session, "DelayTimers"), Timer())(0), EventArgs.Empty)
                Check(session.TotalReinforcers = 1 AndAlso Not CBool(Field(State(session), "PendingReinforcer")), "delay delivers then restarts")
            End Using
            bp = WithChain(Link("Fixed Ratio", 1, "Light 1"), Link("Fixed Ratio", 1, "Tone"))
            bp.ComponentStimType = "Light 1"
            Using session = CreateSession(bp, nextBp, 1)
                Press(session, "1")
                Dim states = DirectCast(Field(session, "chainOutputState"), Dictionary(Of String, Boolean))
                Check(states("Light 1") AndAlso states("Tone"), "link transition preserves component output")
                CallMethod(session, "ComponentDuration_Code")
                Check(Not CBool(CallMethod(session, "AnyClassicChainActive")), "component end clears claims")
            End Using
            bp = WithChain(Link("Fixed Ratio", 2, "Tone"))
            bp.FeedbackType(0) = "Time Out" : bp.FeedbackDuration(0) = 1
            Using session = CreateSession(bp)
                Press(session, "1")
                Check(CInt(Field(State(session), "RatioCount")) = 1, "TO-trigger response retains simple schedule semantics")
                Dim states = DirectCast(Field(session, "chainOutputState"), Dictionary(Of String, Boolean))
                Check(states.Values.All(Function(v) Not v), "TO suppresses chain cues")
                bp.FeedbackType(0) = "None"
            End Using
            bp = WithChain(Link("Fixed Ratio", 2), Link("Variable Time", 10, "Tone", True))
            Dim copy = ChamberConfiguration.CloneComponent(bp)
            copy.ClassicChains(0).Links(1).Value = 99
            Check(bp.ClassicChains(0).Links(1).Value = 10, "deep copy links")
            Dim comps(MAX_COMPONENTS) As ComponentBlueprint : comps(1) = bp
            Dim data As New SetupSaveData With {.Components = comps, .ComponentCount = 1, .BoxName = "Test", .StartDelay = "0", .PostSession = "0", .ICIDuration = "0", .ICIStimulus = "None", .ComponentOrder = "", .CommandMap = New ArduinoCommandMap()}
            Dim json = data.ToIndentedJson()
            Dim loaded = SetupSaveData.ReadJson(json)
            Check(loaded.Components(1).ClassicChains(0).Links.Count = 2 AndAlso loaded.Components(1).ClassicChains(0).Links(1).RetractInput, "JSON chains roundtrip")
            Dim old = Blueprint(False) : old.ClassicChains = Nothing : data.FormatVersion = 7 : comps(1) = old : data.Components = comps
            Dim legacy = SetupSaveData.ReadJson(data.ToJson())
            Check(legacy.Components(1).ClassicChains.Length = 6 AndAlso legacy.Components(1).ClassicChains.All(Function(c) c Is Nothing), "legacy simple normalized")
            Check(ClassicChainConfiguration.LinkError(Link("Fixed Ratio", 1, "None", True)) <> "", "non-time retraction invalid")
            Check(ClassicChainConfiguration.LinkError(Link("Fixed Time", 0)) <> "", "zero time invalid")

            bp = WithChain(Link("Fixed Ratio", 1, "Light 1"))
            bp.FeedbackType(0) = "Time Out" : bp.FeedbackDuration(0) = 1
            Using session = CreateSession(bp)
                Dim commands = DirectCast(Field(session, "simulationCommands"), List(Of String))
                commands.Clear()
                Press(session, "1")
                Check(session.TotalReinforcers = 1, "single FR1 chain matches simple with TO-trigger")
                Check(Not commands.Contains("L"), "TO-trigger cannot reextend next cycle")
                CallMethod(session, "FeedbackTimer_Tick", DirectCast(Field(session, "FeedbackTimers"), Timer())(0), EventArgs.Empty)
                Check(commands.Contains("L"), "TO end restores current available link")
            End Using
            bp = WithChain(Link("Fixed Time", 1, "Light 1", True), Link("Fixed Ratio", 1))
            bp.DelayDuration(0) = 1 : bp.DelayType(0) = "Unsignaled" : bp.DelayRetract(0) = True
            Using session = CreateSession(bp)
                Dim commands = DirectCast(Field(session, "simulationCommands"), List(Of String))
                Check(Not commands.Contains("L"), "first FT retract has no transient extension")
                Elapse(session) : Press(session, "1")
                commands.Clear()
                CallMethod(session, "DelayTimer_Tick", DirectCast(Field(session, "DelayTimers"), Timer())(0), EventArgs.Empty)
                Check(Not commands.Contains("L"), "delay completion cannot override next FT retraction")
            End Using
            bp = WithChain(Link("Fixed Ratio", 2, "Light 1"))
            bp.FeedbackType(0) = "Light 1" : bp.FeedbackDuration(0) = 1
            Using session = CreateSession(bp)
                Press(session, "1")
                Dim commands = DirectCast(Field(session, "simulationCommands"), List(Of String))
                commands.Clear()
                CallMethod(session, "FeedbackTimer_Tick", DirectCast(Field(session, "FeedbackTimers"), Timer())(0), EventArgs.Empty)
                Check(Not commands.Contains("a"), "feedback off does not pulse shared chain output")
                CallMethod(session, "StartPostSession")
                Check(commands.Any(Function(command) command.Contains("a")), "post-session turns off chain outputs")
            End Using
            bp = WithChain(Link("Fixed Ratio", 1))
            comps(1) = bp : data.Components = comps : data.FormatVersion = 8
            json = data.ToJson()
            For Each fieldName In {"Value", "ScheduleType", "RetractInput", "VariableValueCount", "StimulusType", "LightIntermittency", "ToneIntermittency"}
                Dim obj = StorageJson.Decode(json)
                Dim items = DirectCast(obj("Components"), System.Collections.IList)
                Dim comp = DirectCast(items(1), Dictionary(Of String, Object))
                Dim chains = DirectCast(comp("ClassicChains"), System.Collections.IList)
                Dim chain = DirectCast(chains(0), Dictionary(Of String, Object))
                Dim links = DirectCast(chain("Links"), System.Collections.IList)
                DirectCast(links(0), Dictionary(Of String, Object)).Remove(fieldName)
                Dim rejected As Boolean
                Try
                    SetupSaveData.ReadJson(StorageJson.Encode(obj))
                Catch ex As IO.InvalidDataException
                    rejected = True
                End Try
                Check(rejected, "missing JSON link " & fieldName & " rejected")
            Next
            Check(ClassicChainConfiguration.LinkError(New ClassicChainLink With {.LightIntermittency = 0.001}) <> "", "sub-resolution signal rejected")
            Using session = CreateSession(WithChain(Link("Fixed Ratio", 5, "Tone")))
                DirectCast(Field(session, "rawWriter"), StreamWriter).BaseStream.Close()
                CallMethod(session, "StopRuntimeTimers")
                Check(Not CBool(CallMethod(session, "AnyClassicChainActive")), "failed data stream cannot interrupt chain cleanup")
                SetField(session, "rawWriter", Nothing)
            End Using
            Console.WriteLine("PASS: " & assertions & " chain assertions")
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
End Module
