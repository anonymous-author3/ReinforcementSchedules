Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports ReinforcementSchedules

Module ReferenceRuntimeTests
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
        Try
            Dim source = Blueprint(True, 2, 1)
            Dim prior As SessionReferenceData
            Using original = Session(source)
                Press(original, "11221122")
                prior = Reference(original)
                Check(prior.Sequences.Count = 4 AndAlso prior.HasExactOutcomes, "new logs round-trip explicit outcomes")
                Check(prior.Deliveries.Count = 4 AndAlso prior.Deliveries.All(Function(d) d.BatchNumber > 0), "every delivered unit and batch logged")
            End Using
            Dim continued = Blueprint(True, 2, 2)
            continued.LagHistoryReference = prior
            Using s = Session(continued)
                Check(Tracker(s).HistoryCount = 2 AndAlso Tracker(s).CompletedCount = 0, "only last N source sequences seed zero current trials")
                Press(s, "11")
                Check(s.TotalReinforcers = 0 AndAlso Tracker(s).CompletedCount = 1, "seed history controls first current trial")
            End Using
            Check(prior.Sequences.Count = 4, "source snapshot not mutated")
            Dim cap = Blueprint(True, 2, 5)
            cap.LagMaxSequences = 3
            cap.MaxRefs = 1
            Using s = Session(cap, Blueprint(False))
                Press(s, "11")
                Check(CByte(Field(s, "vCC")) = 1, "sequence limit supersedes MaxRefs")
                Press(s, "1111")
                Check(CByte(Field(s, "vCC")) = 2 AndAlso Tracker(s).CompletedCount = 3 AndAlso s.TotalReinforcers = 1, "three trials ends presentation regardless reinforcement")
            End Using
            cap.LagMaxSequences = 0
            Using s = Session(cap, Blueprint(False))
                Press(s, "11")
                Check(CByte(Field(s, "vCC")) = 2, "zero sequence cap retains MaxRefs")
            End Using
            cap.LagMaxSequences = 1
            cap.DelayDuration(0) = 1 : cap.DelayDuration(1) = 1
            Using s = Session(cap, Blueprint(False))
                Press(s, "11")
                Check(CByte(Field(s, "vCC")) = 1 AndAlso s.TotalReinforcers = 0, "final trial waits for its configured delay")
                CallMethod(s, "CompleteLagDelay", 0)
                Check(CByte(Field(s, "vCC")) = 2 AndAlso s.TotalReinforcers = 1, "final delayed delivery occurs before sequence cap transition")
            End Using
            Using s = Session(cap, Blueprint(False))
                Press(s, "11")
                SetField(s, "lagComponentDeadline", 0L)
                CallMethod(s, "CompleteLagDelay", 0)
                Check(CByte(Field(s, "vCC")) = 2 AndAlso s.TotalReinforcers = 0 AndAlso Raw(s).Contains("Cancelled:ComponentEnd"), "time wins even over final delayed trial")
            End Using
            Dim fifty = Blueprint(True, 1, 1)
            fifty.LagMaxSequences = 50 : fifty.MaxRefs = 1
            Using s = Session(fifty, Blueprint(False))
                Press(s, New String("1"c, 49))
                Check(CByte(Field(s, "vCC")) = 1 AndAlso Tracker(s).CompletedCount = 49 AndAlso s.TotalReinforcers = 1, "49 sequences do not end a 50-trial presentation even after MaxRefs")
                Press(s, "1")
                Check(CByte(Field(s, "vCC")) = 2 AndAlso Tracker(s).CompletedCount = 50, "exactly 50 complete sequences ends presentation")
            End Using
            Dim timeout = Blueprint(True, 1, 1)
            timeout.LagMaxSequences = 2 : timeout.LagUseTimeout = True : timeout.LagTimeoutSeconds = 2
            Using s = Session(timeout, Blueprint(False))
                Press(s, "11")
                Check(CByte(Field(s, "vCC")) = 1 AndAlso CBool(Field(s, "lagTimeoutActive")), "last failed trial retains its timeout")
                SetField(s, "lagTimeoutDeadline", 0L)
                CallMethod(s, "LagTimeoutTick", Nothing, EventArgs.Empty)
                Check(CByte(Field(s, "vCC")) = 2, "last timeout completes before sequence-cap transition")
            End Using
            Dim pattern = Blueprint(True, 2, 0)
            pattern.Magnitude(0) = 2 : pattern.Magnitude(1) = 2
            Dim yokedReference As SessionReferenceData
            Using s = Session(pattern)
                Press(s, "11")
                Dim live = DirectCast(Field(s, "AC"), ComponentBlueprint())
                live(1).DeliveryP(0) = 0 : live(1).DeliveryP(1) = 0
                Press(s, "22")
                live(1).DeliveryP(0) = 100 : live(1).DeliveryP(1) = 100
                live(1).Magnitude(0) = 1 : live(1).Magnitude(1) = 1
                live(1).Reinforcer(0) = "Liquid" : live(1).Reinforcer(1) = "Liquid"
                Press(s, "12")
                yokedReference = Reference(s)
                Check(yokedReference.Sequences.Select(Function(t) t.DeliveredCount.Value).SequenceEqual({2, 0, 1}), "actual delivery not criterion pass defines outcomes")
                Check(yokedReference.ValidateYoked(True) = "", "complete exact reference validates")
            End Using
            Dim yoked = Blueprint(True, 2, 100)
            yoked.YokedEnabled = True : yoked.YokedReference = yokedReference
            yoked.MaxRefs = 1 : yoked.LagMaxSequences = 1
            yoked.DeliveryP(0) = 0 : yoked.DeliveryP(1) = 0
            yoked.DelayDuration(0) = 10 : yoked.DelayDuration(1) = 10
            yoked.LagUseTimeout = True : yoked.LagTimeoutSeconds = 1
            Using s = Session(yoked, Blueprint(False))
                Dim commands = DirectCast(Field(s, "simulationCommands"), List(Of String))
                commands.Clear()
                Press(s, "11")
                Check(s.TotalReinforcers = 2 AndAlso Not CBool(Field(s, "SessionEnding")), "yoked copies quantity independent of MaxRefs/current probability/delay")
                Press(s, "11")
                Check(s.TotalReinforcers = 2 AndAlso Not CBool(Field(s, "lagTimeoutActive")), "omission follows reference, not repeated sequence timeout")
                Press(s, "11")
                Check(s.TotalReinforcers = 3 AndAlso CBool(Field(s, "SessionEnding")) AndAlso CByte(Field(s, "vCC")) = 1, "last source trial ends entire session before other component")
                Check(commands.Where(Function(c) c = "R" OrElse c = "W").SequenceEqual({"R", "R", "W"}), "Pellet/Liquid command order exactly copied")
                Press(s, "11")
                Check(s.TotalReinforcers = 3, "exhausted reference never wraps")
                Dim roundtrip = Reference(s)
                Check(roundtrip.Sequences.Select(Function(t) t.DeliveredCount.Value).SequenceEqual({2, 0, 1}), "yoked logs remain reusable exact references")
            End Using
            Dim classic = Blueprint(False)
            classic.ScheduleValue(0) = 1 : classic.ScheduleValue(1) = 1
            classic.Magnitude(0) = 2
            classic.Reinforcer(1) = "Liquid"
            Dim classicReference As SessionReferenceData
            Using s = Session(classic)
                Press(s, "12")
                classicReference = Reference(s)
                Check(classicReference.ValidateYoked(False) = "", "classic batches support yoking")
                Check(classicReference.Deliveries.Count = 3, "classic magnitude logs three units in two batches")
            End Using
            classic.YokedEnabled = True : classic.YokedReference = classicReference
            classic.MaxRefs = 1
            classic.DeliveryP(0) = 0 : classic.DeliveryP(1) = 0
            Using s = Session(classic, Blueprint(False))
                SetElapsed(s, 0)
                Press(s, "1")
                Check(s.TotalReinforcers = 0, "classic yoke waits until availability")
                SetElapsed(s, 100)
                Press(s, "1")
                Check(s.TotalReinforcers = 2 AndAlso Not CBool(Field(s, "SessionEnding")), "one response collects exact first batch, not all due batches")
                Press(s, "1")
                Check(s.TotalReinforcers = 2, "wrong input cannot collect next batch")
                Press(s, "2")
                Check(s.TotalReinforcers = 3 AndAlso CBool(Field(s, "SessionEnding")), "correct input collects last batch and ends reference")
                Check(DirectCast(Field(s, "ScheduleTimers"), Timer()).All(Function(t) Not t.Enabled), "ordinary schedules disabled while yoked")
            End Using
            Dim invalid = classicReference.Copy()
            invalid.Deliveries(0).Input = 0
            Check(ReferenceConfiguration.ValidateSelection(classic, invalid) <> "", "FT/VT without input rejected")
            invalid = yokedReference.Copy() : invalid.Sequences(0).Value = "333"
            Check(ReferenceConfiguration.ValidateSelection(yoked, invalid) <> "", "incompatible sequence reference rejected")
            Dim visual = Blueprint(True, 2, 1)
            visual.LagUseTimeout = True : visual.LagTimeoutSeconds = 2
            Using s = Session(visual, Blueprint(False), 1)
                Press(s, "11112")
                Dim state = DirectCast(Field(s, "lagFrequencyStates"), Array).GetValue(1)
                Check(CLng(Field(state, "Total")) = 2 AndAlso CLng(Field(state, "AllResponseTotal")) = 5, "integrated chart counts timeout responses only in all mode")
                Check(DirectCast(Field(state, "Transitions"), Long(,))(1, 1) = 2 AndAlso DirectCast(Field(state, "AllTransitions"), Long(,))(1, 1) = 3, "all mode includes trial crossings while within mode excludes them")
                Advance(s)
                Press(s, "1")
                Check(CLng(Field(state, "AllResponseTotal")) = 5, "ICI excluded from both graphs")
                CallMethod(s, "tmrICI_Tick", Nothing, EventArgs.Empty)
                Press(s, "1")
                Check(CLng(Field(state, "AllResponseTotal")) = 5, "Classic responses do not contaminate Lag graph")
                Advance(s)
                CallMethod(s, "tmrICI_Tick", Nothing, EventArgs.Empty)
                Dim before = DirectCast(Field(state, "AllDepartures"), Long()).Sum()
                Press(s, "2")
                Check(DirectCast(Field(state, "AllDepartures"), Long()).Sum() = before, "new presentation starts no cross-boundary transition")
                Dim toggle = DirectCast(Field(s, "lagTransitionMode"), CheckBox)
                toggle.Checked = True : toggle.Checked = False
                Check(CLng(Field(state, "Total")) = 2 AndAlso CLng(Field(state, "AllResponseTotal")) = 6, "switching integrated toggle preserves both datasets and bars")
            End Using
            Dim copied = ChamberConfiguration.CloneComponent(yoked)
            copied.YokedReference.Sequences(0).Value = "22"
            Check(yoked.YokedReference.Sequences(0).Value = "11", "reference snapshots deep cloned per chamber")
            Dim settings As New SetupSaveData With {.SavedAtUtc = Date.UtcNow, .BoxName = "Test", .StartDelay = "0", .PostSession = "0", .ICIDuration = "0", .ICIStimulus = "None", .ComponentOrder = "", .CommandMap = New ArduinoCommandMap(), .ComponentCount = 1}
            Dim comps(MAX_COMPONENTS) As ComponentBlueprint : comps(1) = yoked : settings.Components = comps
            Dim file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") & ".json")
            Try
                settings.Save(file)
                Dim loaded = SetupSaveData.Read(file)
                Check(loaded.FormatVersion = 8 AndAlso loaded.Components(1).YokedEnabled AndAlso loaded.Components(1).YokedReference.Deliveries.Count = 3, "v7 exact references round-trip JSON")
            Finally
                If IO.File.Exists(file) Then IO.File.Delete(file)
            End Try
            Console.WriteLine("PASS reference runtime: " & assertions & " assertions, simulation only.")
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
End Module
