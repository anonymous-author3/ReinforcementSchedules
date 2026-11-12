Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports ReinforcementSchedules

Module IntervalTests
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

    Private Sub Reject(action As Action, message As String)
        Dim rejected As Boolean
        Try
            action()
        Catch ex As ArgumentException
            rejected = True
        End Try
        Check(rejected, message)
    End Sub

    Private Sub ExhaustiveCalculations()
        For value As Integer = 1 To IntervalScheduleTiming.MaximumFixedSeconds
            If IntervalScheduleTiming.ToTimerMilliseconds(value) <> value * 1000 Then Throw New Exception("FI mismatch " & value)
        Next
        Console.WriteLine("PASS exhaustive FI: 2147483 whole-second requests convert exactly.")
        Dim coefficients(9) As Double
        For i As Integer = 0 To 9
            Dim a As Double = (9 - i) / 10.0
            Dim b As Double = (10 - i) / 10.0
            coefficients(i) = 10 * (b * (1 - Math.Log(b)) - If(a = 0, 0, a * (1 - Math.Log(If(a = 0, 1, a)))))
        Next
        Dim largestError As Double
        For mean As Integer = 1 To IntervalScheduleTiming.MaximumVariableMeanSeconds
            Dim values = IntervalScheduleTiming.GenerateVariableSeconds(mean)
            If Math.Abs(values.Sum() / 10 - mean) > mean * 0.000000000001 Then Throw New Exception("VI mean mismatch " & mean)
            For i As Integer = 0 To 9
                If Math.Abs(values(i) - mean * coefficients(i)) > mean * 0.000000000001 Then Throw New Exception("VI independent integral oracle mismatch " & mean)
                Dim ms = IntervalScheduleTiming.ToTimerMilliseconds(values(i))
                Dim errorMs = Math.Abs(ms - values(i) * 1000)
                If errorMs > 0.500001 OrElse ms < 1 Then Throw New Exception("VI conversion mismatch " & mean)
                largestError = Math.Max(largestError, errorMs)
            Next
        Next
        Console.WriteLine("PASS exhaustive VI: 650243 means, 6502430 intervals; independent exponential-bin integral oracle and mean verified; maximum rounding error = " & largestError.ToString("F6", Globalization.CultureInfo.InvariantCulture) & " ms.")
        For Each value In {0.0, -1.0, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, 0.000999, 2147483.648}
            Dim captured = value
            Reject(Sub() IntervalScheduleTiming.ToTimerMilliseconds(captured), "invalid timer value rejected")
        Next
        Check(IntervalScheduleTiming.ToTimerMilliseconds(0.001) = 1, "1 ms minimum")
        Check(IntervalScheduleTiming.ToTimerMilliseconds(2147483.647) = Integer.MaxValue, "maximum timer")
        Check(IntervalScheduleTiming.ToTimerMilliseconds(1.0005) = 1001, "half milliseconds round away from zero")
        Check(IntervalScheduleTiming.ValidationError("Fixed Interval", 2147483) = "", "FI max accepted")
        Check(IntervalScheduleTiming.ValidationError("Fixed Interval", 2147484) <> "", "FI overflow rejected")
        Check(IntervalScheduleTiming.ValidationError("Variable Interval", 650243) = "", "VI max accepted")
        Check(IntervalScheduleTiming.ValidationError("Variable Interval", 650244) <> "", "VI overflow rejected")
        Check(IntervalScheduleTiming.ValidationError("Variable Interval", 0) <> "", "zero VI rejected")
        Dim sample = IntervalScheduleTiming.GenerateVariableSeconds(30)
        Console.WriteLine("VI 30 seconds, sorted timer ms: " & String.Join(",", sample.Select(Function(v) IntervalScheduleTiming.ToTimerMilliseconds(v))))
        Console.WriteLine("VI 30 programmed-list mean ms: " & sample.Average(Function(v) CDbl(IntervalScheduleTiming.ToTimerMilliseconds(v))).ToString("F3", Globalization.CultureInfo.InvariantCulture))
    End Sub

    Private Sub RuntimeTests()
        Dim fi = Blueprint(False)
        fi.ScheduleType(0) = "Fixed Interval" : fi.ScheduleValue(0) = 30
        Using s = Session(fi)
            Dim timers = DirectCast(Field(s, "ScheduleTimers"), Timer())
            Check(timers(0).Interval = 30000 AndAlso timers(0).Enabled, "FI30 actual timer 30000 ms")
            Press(s, "111")
            Check(s.TotalReinforcers = 0, "FI responses before availability do not reinforce")
            CallMethod(s, "ScheduleTimer_Tick", timers(0), EventArgs.Empty)
            Check(s.TotalReinforcers = 0 AndAlso Not timers(0).Enabled, "FI elapsed timer only makes reinforcement available")
            Press(s, "1")
            Check(s.TotalReinforcers = 1 AndAlso timers(0).Enabled AndAlso timers(0).Interval = 30000, "next response reinforces and rearms exact FI")
            Press(s, "1") : Check(s.TotalReinforcers = 1, "next FI not already available")
        End Using
        Dim vi = Blueprint(False)
        Dim means = {1, 2, 30, 60, 3600, 650243}
        For i As Integer = 0 To 5
            vi.ScheduleType(i) = "Variable Interval" : vi.ScheduleValue(i) = means(i)
            vi.Magnitude(i) = 1 : vi.Reinforcer(i) = "Pellet" : vi.DeliveryP(i) = 100
        Next
        Using s = Session(vi)
            Dim timers = DirectCast(Field(s, "ScheduleTimers"), Timer())
            Dim lists = DirectCast(Field(s, "VIList"), List(Of Double)())
            For i As Integer = 0 To 5
                Check(lists(i).Count = 9, "each independent input starts one sampled interval")
                Dim expected = IntervalScheduleTiming.GenerateVariableSeconds(means(i)).Select(Function(v) IntervalScheduleTiming.ToTimerMilliseconds(v)).OrderBy(Function(v) v).ToArray()
                For cycle As Integer = 1 To 30
                    Dim actual As New List(Of Integer) From {timers(i).Interval}
                    For draw As Integer = 2 To 10
                        CallMethod(s, "VIGen", i) : actual.Add(timers(i).Interval)
                    Next
                    Check(actual.OrderBy(Function(v) v).SequenceEqual(expected), "all ten intervals exactly once in every VI cycle")
                    Check(lists(i).Count = 0, "VI list exhausted after ten draws")
                    CallMethod(s, "VIGen", i)
                    Check(lists(i).Count = 9, "VI list replenishes after exhaustion")
                Next
            Next
            Press(s, "1") : Check(s.TotalReinforcers = 0, "VI requires elapsed availability")
            CallMethod(s, "ScheduleTimer_Tick", timers(0), EventArgs.Empty)
            Press(s, "1") : Check(s.TotalReinforcers = 1, "VI first eligible response reinforced")
        End Using
        Using a = Session(vi), b = Session(vi)
            Dim other = DirectCast(Field(b, "VIList"), List(Of Double)())(0).ToArray()
            CallMethod(a, "VIGen", 0)
            Check(DirectCast(Field(b, "VIList"), List(Of Double)())(0).SequenceEqual(other), "separate chambers do not share list state")
        End Using
        Dim invalid = Blueprint(False) : invalid.ScheduleType(0) = "Variable Interval" : invalid.ScheduleValue(0) = 650244
        Reject(Sub()
                   Using s = Session(invalid)
                   End Using
               End Sub, "runtime rejects unrepresentable VI before start")
        Dim samples(9) As Integer
        vi = Blueprint(False) : vi.ScheduleType(0) = "Variable Interval" : vi.ScheduleValue(0) = 30
        Using s = Session(vi)
            SetField(s, "SessionRandom", New Random(123456))
            Dim lists = DirectCast(Field(s, "VIList"), List(Of Double)())
            Dim timers = DirectCast(Field(s, "ScheduleTimers"), Timer())
            Dim expected = IntervalScheduleTiming.GenerateVariableSeconds(30).Select(Function(v) IntervalScheduleTiming.ToTimerMilliseconds(v)).ToArray()
            For cycle As Integer = 1 To 10000
                lists(0).Clear()
                CallMethod(s, "VIGen", 0)
                samples(Array.IndexOf(expected, timers(0).Interval)) += 1
                For draw As Integer = 2 To 10
                    CallMethod(s, "VIGen", 0)
                Next
            Next
            Check(samples.All(Function(count) count > 800 AndAlso count < 1200), "deterministic first-draw distribution sanity check")
        End Using
        Console.WriteLine("PASS runtime: FI response eligibility/rearm, VI 6 inputs x 30 complete cycles, independent chambers, 10000 shuffle cycles. First draw counts: " & String.Join(",", samples))
    End Sub

    Private Sub CompatibilityTests(root As String)
        Dim components(MAX_COMPONENTS) As ComponentBlueprint
        components(1) = Blueprint(False)
        components(1).ScheduleType(0) = "Fixed Interval" : components(1).ScheduleValue(0) = 30
        components(1).ScheduleType(1) = "Variable Interval" : components(1).ScheduleValue(1) = 60
        Dim setup As New SetupSaveData With {.BoxName = "Interval test", .SavedAtUtc = DateTime.UtcNow, .StartDelay = "0", .PostSession = "0", .ICIDuration = "0", .ICIStimulus = "None", .ComponentOrder = "1", .CommandMap = New ArduinoCommandMap(), .ComponentCount = 1, .Components = components, .LegacyLeverLetters = "A,B"}
        Dim path = IO.Path.Combine(root, "interval-v4.xml")
        setup.FormatVersion = 4
        Using stream = File.Create(path)
            Dim serializer As New System.Runtime.Serialization.DataContractSerializer(GetType(SetupSaveData))
            serializer.WriteObject(stream, setup)
        End Using
        Dim xml = File.ReadAllText(path)
        For version As Integer = 3 To 4
            Dim loaded = SetupSaveData.ReadXml(xml.Replace("<FormatVersion>4</FormatVersion>", "<FormatVersion>" & version & "</FormatVersion>"))
            Check(loaded.Components(1).ScheduleValue(0) = 30 AndAlso loaded.Components(1).ScheduleValue(1) = 60, "XML v" & version & " preserves integer requests")
        Next
        Check(GetType(ComponentBlueprint).GetField("ScheduleValue").FieldType Is GetType(Integer()), "serialized schedule array type unchanged")
        Check(ChamberConfiguration.CloneComponent(components(1)).ScheduleValue.SequenceEqual(components(1).ScheduleValue), "configuration clone preserves requests")
        Dim invalidXml As New System.Xml.XmlDocument()
        invalidXml.LoadXml(xml)
        Dim node = invalidXml.SelectSingleNode("//*[local-name()='ScheduleValue']/*[local-name()='int' and text()='60']")
        Check(node IsNot Nothing, "invalid XML test finds exact VI value")
        node.InnerText = "650244"
        Dim rejected As Boolean
        Try
            SetupSaveData.ReadXml(invalidXml.OuterXml)
        Catch ex As IO.InvalidDataException
            rejected = True
        End Try
        Check(rejected, "XML rejects VI timer overflow before applying setup")
        For Each sampleText As String In {"30.5", "30,5", "NaN", "Infinity", "2147483648", "0", "-1", ""}
            Dim captured = sampleText
            Reject(Sub() IntervalScheduleTiming.ParseWholeSeconds("Fixed Interval", captured), "legacy interval text never silently rounds unsupported values")
        Next
        Check(IntervalScheduleTiming.ParseWholeSeconds("Fixed Interval", "30") = 30, "legacy FI integer accepted")
        Check(IntervalScheduleTiming.ParseWholeSeconds("Variable Interval", "60") = 60, "legacy VI integer accepted")
        Using editor As New Component()
            CallMethod(editor, "Component_Load", Nothing, EventArgs.Empty)
            DirectCast(Field(editor, "rdoFIL1"), RadioButton).Checked = True
            DirectCast(Field(editor, "txbValueL1"), TextBox).Text = "30"
            Dim input = CallMethod(editor, "BuildInputFromEditor", 1)
            Check(CInt(Field(input, "ScheduleValue")) = 30, "FI editor preserves requested whole seconds")
            DirectCast(Field(editor, "rdoVIL1"), RadioButton).Checked = True
            DirectCast(Field(editor, "txbValueL1"), TextBox).Text = "30.5"
            input = CallMethod(editor, "BuildInputFromEditor", 1)
            Check(IntervalScheduleTiming.ValidationError(CStr(Field(input, "ScheduleType")), CInt(Field(input, "ScheduleValue"))) <> "", "VI editor rejects fractional requests instead of rounding")
        End Using
        Console.WriteLine("PASS XML v3/v4 round-trip and unchanged Integer() schema. Fixture: " & path)
    End Sub

    Private Sub ObserveTimer(root As String)
        Dim fi = Blueprint(False) : fi.ScheduleType(0) = "Fixed Interval" : fi.ScheduleValue(0) = 1
        Dim observed As New List(Of Double)
        For trial As Integer = 1 To 5
            Using s = Session(fi)
                Dim timer = DirectCast(Field(s, "ScheduleTimers"), Timer())(0)
                timer.Stop()
                Dim watch = Diagnostics.Stopwatch.StartNew()
                timer.Start()
                While Not DirectCast(Field(s, "refRdy"), Boolean())(0) AndAlso watch.ElapsedMilliseconds < 5000
                    Application.DoEvents()
                    Threading.Thread.Sleep(1)
                End While
                observed.Add(watch.Elapsed.TotalMilliseconds)
                Check(DirectCast(Field(s, "refRdy"), Boolean())(0), "real WinForms message-loop timer makes FI available")
                Check(s.TotalReinforcers = 0, "timer alone never reinforces")
            End Using
        Next
        Dim report = "Requested FI timer: 1000 ms. Five software-only observations (not hardware validation): " & String.Join(", ", observed.Select(Function(v) v.ToString("F3", Globalization.CultureInfo.InvariantCulture))) & " ms."
        Console.WriteLine(report)
        File.WriteAllText(IO.Path.Combine(root, "timer-observations.txt"), report)
    End Sub

    <STAThread>
    Sub Main()
        Try
            Dim root = Path.Combine(Environment.CurrentDirectory, "IntervalTests", "results")
            Directory.CreateDirectory(root)
            ExhaustiveCalculations()
            RuntimeTests()
            CompatibilityTests(root)
            ObserveTimer(root)
            Console.WriteLine("PASS: " & assertions & " integration/boundary assertions, plus exhaustive FI/VI domain. No hardware/network/real session data used.")
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
End Module
