Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports ReinforcementSchedules

Module ReinforcerTests
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
            Dim expected = New Dictionary(Of String, String) From {{"Pellet", "R"}, {"Liquid", "W"}, {"Feeder 1", "R"}, {"Pump 1", "W"}, {"Feeder 2", "U"}, {"Pump 2", "V"}}
            For Each pair In expected
                Check(ReinforcerConfiguration.Command(pair.Key) = pair.Value, "output mapping " & pair.Key)
            Next
            Dim selection As New ReinforcerSelectionDefinition With {.Feeder1Percent = 10, .Pump1Percent = 20, .Feeder2Percent = 30, .Pump2Percent = 40}
            Dim counts = Enumerable.Range(0, 100).Select(Function(draw) ReinforcerConfiguration.Choose(selection, draw)).GroupBy(Function(name) name).ToDictionary(Function(g) g.Key, Function(g) g.Count())
            Check(counts("Feeder 1") = 10 AndAlso counts("Pump 1") = 20 AndAlso counts("Feeder 2") = 30 AndAlso counts("Pump 2") = 40, "exact cumulative probabilities")
            Reject(Sub() ReinforcerConfiguration.Choose(selection, 100), "out of range draw")
            selection.Feeder1Percent = -1
            Check(ReinforcerConfiguration.ValidateSelection(selection) <> "", "negative probability")
            selection.Feeder1Percent = 11
            Check(ReinforcerConfiguration.ValidateSelection(selection) <> "", "sum other than100")
            selection.Feeder1Percent = 10
            Dim legacyMap As New ArduinoCommandMap With {.Codes = ArduinoCommandMap.LegacyCodes}
            Check(legacyMap.ValidationError() = "" AndAlso legacyMap.Codes = ArduinoCommandMap.DefaultCodes, "legacy defaults expanded")
            Dim custom = ArduinoCommandMap.LegacyCodes.Replace("L", "U").Replace("l", "u")
            Dim mapped As New ArduinoCommandMap With {.Codes = custom}
            Check(mapped.ValidationError() = "" AndAlso mapped.Codes.Substring(0, 30) = custom AndAlso mapped.Codes.Distinct().Count() = 32, "custom legacy map kept without new collisions")
            Check(mapped.Encode("L") = "U" AndAlso mapped.Encode("U") <> "U", "logical second feeder cannot collide with legacy lever")
            Check(ArduinoCommandMap.FromLegacyLevers("UMCDNO").ValidationError() = "", "six legacy lever codes expanded safely")
            Dim collision As New ArduinoCommandMap With {.Codes = ArduinoCommandMap.DefaultCodes.Substring(0, 31) & "U"}
            Check(collision.ValidationError() <> "", "new command collision rejected")
            For Each pair In expected
                Dim bp = Blueprint(False)
                bp.ScheduleValue(0) = 1
                bp.Reinforcer(0) = pair.Key
                Using session = CreateSession(bp)
                    Dim commands = DirectCast(Field(session, "simulationCommands"), List(Of String))
                    commands.Clear()
                    Press(session, "1")
                    Check(session.TotalReinforcers = 1 AndAlso commands.SequenceEqual({pair.Value}), "scheduled correct output " & pair.Key)
                    Check(Raw(session).Contains("""" & pair.Key & """"), "raw preserves output identity " & pair.Key)
                    Dim reference = SessionReferenceData.ParseRaw("""ReferenceFormat"",2" & Environment.NewLine & Raw(session), "")
                    Check(reference.Deliveries.Count = 1 AndAlso reference.Deliveries(0).ReinforcerType = pair.Key, "reference parser identity " & pair.Key)
                    commands.Clear()
                    CallMethod(session, "ReinforcerDelivery", pair.Key, 100, 0, 1, "Yoked")
                    Check(commands.SequenceEqual({pair.Value}), "yoked delivery output " & pair.Key)
                End Using
            Next
            For index As Integer = 0 To 3
                Dim currentIndex = index
                Dim bp = Blueprint(False) : bp.ScheduleValue(0) = 1 : bp.Reinforcer(0) = "Random"
                Dim allOne As New ReinforcerSelectionDefinition With {.Feeder1Percent = If(index = 0, 100, 0), .Pump1Percent = If(index = 1, 100, 0), .Feeder2Percent = If(index = 2, 100, 0), .Pump2Percent = If(index = 3, 100, 0)}
                bp.ReinforcerSelections(0) = allOne
                bp.TimeReinforcer = "Random" : bp.TimeReinforcerSelection = allOne.Copy()
                Using session = CreateSession(bp)
                    Dim commands = DirectCast(Field(session, "simulationCommands"), List(Of String)) : commands.Clear()
                    Press(session, New String("1"c, 25))
                    Check(commands.Count = 25 AndAlso commands.All(Function(command) command = ReinforcerConfiguration.Command(ReinforcerConfiguration.Names(currentIndex))), "random only eligible selected output " & index)
                    commands.Clear()
                    CallMethod(session, "ReinforcerDelivery", "Random", 100, 0, 0, "Time")
                    Check(commands.Single() = ReinforcerConfiguration.Command(ReinforcerConfiguration.Names(index)), "time own random selection " & index)
                End Using
            Next
            Dim first = Blueprint(False) : first.ScheduleValue(0) = 1 : first.Reinforcer(0) = "Random" : first.PelletP(0) = 100
            Using session = CreateSession(first)
                Dim commands = DirectCast(Field(session, "simulationCommands"), List(Of String)) : commands.Clear()
                Press(session, "1")
                Check(commands.SequenceEqual({"R"}), "old random100 pellet still feeder1")
            End Using
            first.PelletP(0) = 0
            Using session = CreateSession(first)
                Dim commands = DirectCast(Field(session, "simulationCommands"), List(Of String)) : commands.Clear()
                Press(session, "1")
                Check(commands.SequenceEqual({"W"}), "old random0 pellet still pump1")
            End Using
            first.ReinforcerSelections(0) = selection.Copy()
            first.TimeReinforcer = "Random" : first.TimeReinforcerSelection = selection.Copy()
            Dim copied = ChamberConfiguration.CloneComponent(first)
            copied.ReinforcerSelections(0).Feeder1Percent = 99 : copied.TimeReinforcerSelection.Pump2Percent = 99
            Check(first.ReinforcerSelections(0).Feeder1Percent = 10 AndAlso first.TimeReinforcerSelection.Pump2Percent = 40, "independent chamber copies")
            Dim comps(MAX_COMPONENTS) As ComponentBlueprint : comps(1) = first
            Dim data As New SetupSaveData With {.Components = comps, .ComponentCount = 1, .BoxName = "Test", .StartDelay = "0", .PostSession = "0", .ICIDuration = "0", .ICIStimulus = "None", .ComponentOrder = "", .CommandMap = New ArduinoCommandMap()}
            Dim json = data.ToIndentedJson() : Dim loaded = SetupSaveData.ReadJson(json)
            Check(loaded.FormatVersion = 10 AndAlso loaded.Components(1).ReinforcerSelections(0).Feeder2Percent = 30 AndAlso loaded.Components(1).TimeReinforcerSelection.Pump2Percent = 40, "v10 JSON selections roundtrip")
            For Each name In {"Feeder1Percent", "Pump1Percent", "Feeder2Percent", "Pump2Percent"}
                Dim obj = StorageJson.Decode(json)
                Dim fields = DirectCast(DirectCast(obj("Components"), System.Collections.IList)(1), Dictionary(Of String, Object))
                Dim selections = DirectCast(fields("ReinforcerSelections"), System.Collections.IList)
                DirectCast(selections(0), Dictionary(Of String, Object)).Remove(name)
                Dim rejected As Boolean
                Try
                    SetupSaveData.ReadJson(StorageJson.Encode(obj))
                Catch ex As IO.InvalidDataException
                    rejected = True
                End Try
                Check(rejected, "missing percentage rejected " & name)
            Next
            Dim oldObj = StorageJson.Decode(json) : oldObj("FormatVersion") = 8
            Dim oldFields = DirectCast(DirectCast(oldObj("Components"), System.Collections.IList)(1), Dictionary(Of String, Object))
            oldFields.Remove("ReinforcerSelections") : oldFields.Remove("TimeReinforcerSelection") : oldFields.Remove("BreakpointSchedules")
            Dim old = SetupSaveData.ReadJson(StorageJson.Encode(oldObj))
            Check(old.Components(1).ReinforcerSelections.Length = 6 AndAlso old.Components(1).ReinforcerSelections.All(Function(value) value Is Nothing), "v8 absence normalized without changing old random")
            Dim sparse = StorageJson.Decode(json)
            Dim shapes As New List(Of Object)
            Dim sharedFlags = BindingFlags.NonPublic Or BindingFlags.Static
            GetType(CloudStorage).GetMethod("CaptureArrayShapes", sharedFlags).Invoke(Nothing, New Object() {sparse, "", shapes})
            Dim sparseFields = DirectCast(DirectCast(sparse("Components"), System.Collections.IList)(1), Dictionary(Of String, Object))
            Dim oldSelections = DirectCast(sparseFields("ReinforcerSelections"), System.Collections.IList)
            sparseFields("ReinforcerSelections") = New Dictionary(Of String, Object) From {{"0", oldSelections(0)}}
            Dim shapeWrapper = StorageJson.Decode(StorageJson.Encode(New Dictionary(Of String, Object) From {{"shapes", shapes}}))
            GetType(CloudStorage).GetMethod("RestoreArrayShapes", sharedFlags).Invoke(Nothing, New Object() {sparse, shapeWrapper("shapes")})
            Dim restored = SetupSaveData.ReadJson(StorageJson.Encode(sparse))
            Check(restored.Components(1).ReinforcerSelections.Length = 6 AndAlso restored.Components(1).ReinforcerSelections(0).Feeder2Percent = 30 AndAlso restored.Components(1).ReinforcerSelections(5) Is Nothing, "RTDB sparse positions restore")
            Dim fractional = StorageJson.Decode(json)
            Dim fractionalFields = DirectCast(DirectCast(fractional("Components"), System.Collections.IList)(1), Dictionary(Of String, Object))
            DirectCast(DirectCast(fractionalFields("ReinforcerSelections"), System.Collections.IList)(0), Dictionary(Of String, Object))("Feeder1Percent") = 10.5D
            Dim fractionalRejected As Boolean
            Try
                SetupSaveData.ReadJson(StorageJson.Encode(fractional))
            Catch ex As IO.InvalidDataException
                fractionalRejected = True
            End Try
            Check(fractionalRejected, "fractional probability not silently rounded")
            Console.WriteLine("PASS: " & assertions & " reinforcer assertions; simulation only, no hardware.")
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
End Module
