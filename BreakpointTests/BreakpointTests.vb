Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports ReinforcementSchedules

Module BreakpointTests
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



    Private Function WithBreakpoint(Optional start As Integer = 2, Optional stepValue As Decimal = 1D, Optional kind As String = "Arithmetic") As ComponentBlueprint
        Dim result = Blueprint(False)
        result.ScheduleType(0) = "Breakpoint"
        result.ScheduleValue(0) = start
        result.BreakpointSchedules(0) = New BreakpointDefinition With {.Progression = kind, .StartRequirement = start, .StepValue = stepValue}
        Return result
    End Function
    Private Function State(session As ChamberSession, Optional input As Integer = 0, Optional component As Integer = 1) As Object
        Return DirectCast(Field(session, "breakpointStates"), Array).GetValue(component, input)
    End Function
    Private Function Requirement(session As ChamberSession, Optional input As Integer = 0, Optional component As Integer = 1) As Integer
        Return CInt(Field(State(session, input, component), "Requirement"))
    End Function
    Private Sub TickDelay(session As ChamberSession, Optional input As Integer = 0)
        CallMethod(session, "DelayTimer_Tick", DirectCast(Field(session, "DelayTimers"), Timer())(input), EventArgs.Empty)
    End Sub
    Private Function Settings(bp As ComponentBlueprint) As SetupSaveData
        Dim components(MAX_COMPONENTS) As ComponentBlueprint
        components(1) = bp
        Return New SetupSaveData With {.BoxName = "test", .StartDelay = "0", .PostSession = "0", .ICIDuration = "0", .ICIStimulus = "None", .ComponentOrder = "", .ComponentCount = 1, .Components = components, .CommandMap = New ArduinoCommandMap(), .SavedAtUtc = DateTime.UtcNow}
    End Function
    Private Sub Invalid(action As Action, message As String)
        Dim rejected As Boolean
        Try
            action()
        Catch ex As Exception
            rejected = True
        End Try
        Check(rejected, message)
    End Sub
    <STAThread>
    Sub Main()
        Try
            Dim bp = WithBreakpoint(2, 3)
            Using session = CreateSession(bp)
                Press(session, "1")
                Check(session.TotalReinforcers = 0 AndAlso Requirement(session) = 2, "initial FR criterion")
                Press(session, "1")
                Check(session.TotalReinforcers = 1 AndAlso Requirement(session) = 5, "arithmetic progression after obtained reinforcer")
                Press(session, "1111")
                Check(session.TotalReinforcers = 1, "no premature reinforcement")
                Press(session, "1")
                Check(session.TotalReinforcers = 2 AndAlso Requirement(session) = 8, "second arithmetic increment")
                Check(Raw(session).Contains("BreakpointRatioComplete") AndAlso Raw(session).Contains("BreakpointReinforcementResolved"), "raw progression events")
            End Using
            bp = WithBreakpoint(2, 1.5D, "Geometric")
            Using session = CreateSession(bp)
                For Each expected In {2, 3, 5, 8, 12, 18, 27, 41}
                    Check(Requirement(session) = expected, "geometric ceiling from previous integer " & expected)
                    Press(session, New String("1"c, expected))
                Next
                Check(session.TotalReinforcers = 8, "geometric count")
            End Using
            bp = WithBreakpoint(1, 1)
            For i As Integer = 0 To 5
                bp.ScheduleType(i) = "Breakpoint" : bp.ScheduleValue(i) = i + 1
                bp.BreakpointSchedules(i) = New BreakpointDefinition With {.StartRequirement = i + 1, .StepValue = i + 1}
                bp.Magnitude(i) = 1 : bp.Reinforcer(i) = "Pellet"
            Next
            Using session = CreateSession(bp)
                For i As Integer = 0 To 5
                    Press(session, New String(Convert.ToChar(49 + i), i + 1))
                    Check(Requirement(session, i) = 2 * (i + 1), "independent input " & i)
                Next
                Check(session.TotalReinforcers = 6, "all six input outcomes")
            End Using
            Using first = CreateSession(WithBreakpoint()), second = CreateSession(WithBreakpoint())
                Press(first, "11")
                Check(first.TotalReinforcers = 1 AndAlso second.TotalReinforcers = 0 AndAlso Requirement(second) = 2, "independent chamber state")
            End Using
            bp = WithBreakpoint(2, 3)
            Dim other = Blueprint(False)
            Using session = CreateSession(bp, other)
                Press(session, "1111")
                Check(Requirement(session) = 5, "progress before transition")
                Advance(session) : Advance(session)
                Check(Requirement(session) = 2 AndAlso CInt(Field(State(session), "Responses")) = 0, "component return resets full ratio and partial progress")
            End Using
            bp = WithBreakpoint(2, 1)
            bp.DelayDuration(0) = 2 : bp.DelayType(0) = "Unsignaled" : bp.DelayRetract(0) = True
            Using session = CreateSession(bp)
                Press(session, "11")
                Check(session.TotalReinforcers = 0 AndAlso CBool(Field(State(session), "PendingReinforcer")), "delay pending no early progression")
                Press(session, "111")
                Check(CInt(Field(State(session), "Responses")) = 2 AndAlso session.TotalReinforcers = 0, "delay responses do not complete another ratio")
                TickDelay(session)
                Check(session.TotalReinforcers = 1 AndAlso Requirement(session) = 3, "delay delivers then advances")
                TickDelay(session)
                Check(session.TotalReinforcers = 1, "duplicate delay callback cannot deliver twice")
            End Using
            Using session = CreateSession(bp, other)
                Press(session, "11") : Advance(session) : TickDelay(session)
                Check(session.TotalReinforcers = 0, "transition cancels pending reinforcer")
            End Using
            Using session = CreateSession(bp, other)
                Press(session, "11")
                SetField(session, "componentStartedTimestamp", Diagnostics.Stopwatch.GetTimestamp() - CLng(700.0 * Diagnostics.Stopwatch.Frequency))
                TickDelay(session)
                Check(session.TotalReinforcers = 0 AndAlso CInt(Field(session, "vCC")) = 2, "elapsed component beats delayed delivery")
            End Using
            bp = WithBreakpoint(2, 3)
            bp.FeedbackDuration(0) = 1 : bp.FeedbackType(0) = "Time Out"
            Using session = CreateSession(bp)
                Press(session, "1")
                Check(CInt(Field(State(session), "Responses")) = 1, "feedback-triggering response counts")
                Press(session, "1")
                Check(session.TotalReinforcers = 0 AndAlso CInt(Field(State(session), "Responses")) = 1, "responses during TO do not progress")
                CallMethod(session, "FeedbackTimer_Tick", DirectCast(Field(session, "FeedbackTimers"), Timer())(0), EventArgs.Empty)
                Press(session, "1")
                Check(session.TotalReinforcers = 1, "TO ends normally")
            End Using
            bp = WithBreakpoint(2, 3) : bp.Magnitude(0) = 3
            Using session = CreateSession(bp)
                Press(session, "11")
                Check(session.TotalReinforcers = 3 AndAlso Requirement(session) = 5, "magnitude advances once per successful batch")
                CallMethod(session, "Reinforce", 0, True, "Manual")
                Check(Requirement(session) = 5 AndAlso CInt(Field(State(session), "Responses")) = 0, "manual delivery does not progress")
            End Using
            bp = WithBreakpoint(2, 3) : bp.DeliveryP(0) = 0
            Using session = CreateSession(bp)
                Press(session, "1111")
                Check(session.TotalReinforcers = 0 AndAlso Requirement(session) = 2 AndAlso CInt(Field(State(session), "CompletedRatios")) = 2, "omission repeats ratio")
            End Using
            bp = WithBreakpoint(1, Integer.MaxValue) : bp.MaxRefs = 1
            Using session = CreateSession(bp, other)
                Press(session, "1")
                Check(session.TotalReinforcers = 1 AndAlso CInt(Field(session, "vCC")) = 2, "MaxRefs stops without calculating unusable overflowing next ratio")
            End Using
            Using session = CreateSession(WithBreakpoint(), other)
                SetField(session, "componentStartedTimestamp", Diagnostics.Stopwatch.GetTimestamp() - CLng(700.0 * Diagnostics.Stopwatch.Frequency))
                Press(session, "11")
                Check(session.TotalReinforcers = 0 AndAlso CInt(Field(session, "vCC")) = 2, "late response no breakpoint delivery")
            End Using
            bp = WithBreakpoint(1, 1)
            bp.DelayDuration(0) = 2 : bp.DelayType(0) = "Unsignaled" : bp.DelayRetract(0) = True
            Using session = CreateSession(bp, other, 5)
                DirectCast(Field(session, "configuration"), ChamberConfiguration).RetractDuringICI = True
                Press(session, "1")
                SetField(session, "componentStartedTimestamp", Diagnostics.Stopwatch.GetTimestamp() - CLng(700.0 * Diagnostics.Stopwatch.Frequency))
                TickDelay(session)
                Check(DirectCast(Field(session, "tmrICI"), Timer).Enabled AndAlso Not DirectCast(Field(session, "PalIO"), Boolean())(0), "expired delayed reward cannot extend during ICI")
                Check(DirectCast(Field(session, "ObtainedDelayDurations"), List(Of Integer)(,))(1, 0).Count = 0, "cancelled delay not recorded as obtained")
            End Using
            bp.FeedbackDuration(0) = 1 : bp.FeedbackType(0) = "Time Out"
            Using session = CreateSession(bp)
                Press(session, "1")
                CallMethod(session, "FeedbackTimer_Tick", DirectCast(Field(session, "FeedbackTimers"), Timer())(0), EventArgs.Empty)
                Check(DirectCast(Field(session, "DelayTimers"), Timer())(0).Enabled AndAlso Not DirectCast(Field(session, "PalIO"), Boolean())(0), "timeout end preserves delay retraction")
            End Using
            Using session = CreateSession(WithBreakpoint(), other)
                SetField(session, "componentStartedTimestamp", Diagnostics.Stopwatch.GetTimestamp() - CLng(700.0 * Diagnostics.Stopwatch.Frequency))
                Press(session, "1")
                Check(DirectCast(Field(session, "chartResponse"), Integer())(0) = 0, "late response excluded from cumulative plot")
            End Using
            For Each stepValue As Decimal In {1.0001D, 1.1D, 1.5D, 2D, 13D}
                Dim definition As New BreakpointDefinition With {.Progression = "Geometric", .StepValue = stepValue}
                For current As Integer = 1 To 1000
                    Check(BreakpointConfiguration.NextRequirement(current, definition) = CInt(Decimal.Ceiling(current * stepValue)), "exact decimal ceiling")
                Next
            Next
            Invalid(Sub() BreakpointConfiguration.NextRequirement(Integer.MaxValue, New BreakpointDefinition()), "overflow rejects rather than wraps")
            Check(BreakpointConfiguration.ValidateDefinition(New BreakpointDefinition With {.StepValue = 1.5D}) IsNot Nothing, "fractional arithmetic rejects")
            Check(BreakpointConfiguration.ValidateDefinition(New BreakpointDefinition With {.Progression = "Geometric", .StepValue = 1D}) IsNot Nothing, "nonprogressive geometric rejects")
            Check(BreakpointConfiguration.ValidateDefinition(New BreakpointDefinition With {.StartRequirement = 0}) IsNot Nothing, "zero start rejects")
            bp = WithBreakpoint(2, 1.5D, "Geometric")
            Dim data = Settings(bp)
            Dim json = data.ToIndentedJson()
            Dim decoded = SetupSaveData.ReadJson(json)
            Check(decoded.Components(1).BreakpointSchedules(0).StepValue = 1.5D AndAlso decoded.FormatVersion >= 9, "JSON round trip decimal definition")
            Check(json.Contains(Environment.NewLine), "human readable JSON")
            Dim copy = ChamberConfiguration.CloneComponent(bp)
            copy.BreakpointSchedules(0).StepValue = 7
            Check(bp.BreakpointSchedules(0).StepValue = 1.5D, "deep copy settings")
            Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
            Dim tree = serializer.Deserialize(Of Dictionary(Of String, Object))(json)
            Dim comps = DirectCast(tree("Components"), System.Collections.IList)
            Dim rawFields = DirectCast(comps(1), Dictionary(Of String, Object))
            Dim defs = DirectCast(rawFields("BreakpointSchedules"), System.Collections.IList)
            DirectCast(defs(0), Dictionary(Of String, Object)).Remove("StepValue")
            Invalid(Sub() SetupSaveData.ReadJson(serializer.Serialize(tree)), "missing required definition value rejects")
            data = Settings(Blueprint(False))
            tree = serializer.Deserialize(Of Dictionary(Of String, Object))(data.ToJson())
            tree("FormatVersion") = 8
            comps = DirectCast(tree("Components"), System.Collections.IList)
            rawFields = DirectCast(comps(1), Dictionary(Of String, Object))
            rawFields.Remove("BreakpointSchedules")
            decoded = SetupSaveData.ReadJson(serializer.Serialize(tree))
            Check(decoded.Components(1).BreakpointSchedules.Length = 6 AndAlso decoded.Components(1).BreakpointSchedules.All(Function(item) item Is Nothing), "legacy v8 initializes no breakpoint settings")
            bp = WithBreakpoint() : bp.YokedEnabled = True
            Check(BreakpointConfiguration.ValidationError(bp) <> "", "unsupported yoked combination rejects")
            bp = WithBreakpoint() : bp.ScheduleType(0) = "Fixed Ratio"
            Check(BreakpointConfiguration.ValidationError(bp) <> "", "unused definition rejects")
            bp = WithBreakpoint() : bp.ScheduleType(0) = " breakpoint " : bp.BreakpointSchedules(0) = Nothing
            Check(BreakpointConfiguration.ValidationError(bp) <> "", "noncanonical schedule spelling rejected safely")
            bp = WithBreakpoint() : bp.ScheduleValue = New Integer() {}
            Check(BreakpointConfiguration.ValidationError(bp) <> "", "short value array gives validation error")
            Console.WriteLine("Breakpoint checks passed: " & assertions)
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
End Module
