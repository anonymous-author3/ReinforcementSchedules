Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports ReinforcementSchedules

Module LagRuntimeTests
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
    Private Function ReportRows(text As String) As List(Of String())
        Dim rows As New List(Of String())
        Using parser As New Microsoft.VisualBasic.FileIO.TextFieldParser(New StringReader(text))
            parser.SetDelimiters(",")
            parser.HasFieldsEnclosedInQuotes = True
            parser.TrimWhiteSpace = False
            While Not parser.EndOfData
                Dim row = parser.ReadFields()
                Check(row.Length = 20, "report CSV keeps all 20 columns")
                rows.Add(row)
            End While
        End Using
        Return rows
    End Function
    Private Function ExportReport(session As ChamberSession) As List(Of String())
        Dim writer As New StringWriter()
        LagSequenceReport.Write(New StringReader(Raw(session)), writer, DirectCast(Field(session, "configuration"), ChamberConfiguration), "test-session")
        Return ReportRows(writer.ToString())
    End Function
    Private Sub PrepareReportFiles(session As ChamberSession, directory As String)
        IO.Directory.CreateDirectory(directory)
        Dim rawText As String = Raw(session)
        DirectCast(Field(session, "rawWriter"), StreamWriter).Dispose()
        Dim rawFile As String = Path.Combine(directory, "Simulation_Raw.txt")
        File.WriteAllText(rawFile, rawText)
        SetField(session, "rawPath", rawFile)
        SetField(session, "rawWriter", New StreamWriter(rawFile, True) With {.AutoFlush = True})
        SetField(session, "summaryPath", Path.Combine(directory, "Simulation_Summary.txt"))
        SetField(session, "summaryWriter", New StreamWriter(Path.Combine(directory, "Simulation_Summary.txt")) With {.AutoFlush = True})
        SetField(session, "chartPath", Path.Combine(directory, "Simulation_chart.png"))
        SetField(session, "lagReportPath", Path.Combine(directory, "Simulation_LagSequences.csv"))
    End Sub
    Private Sub RunReportTests()
        Dim first = Blueprint(True, 4, 1)
        first.ComponentName = "Lag, ""first""" & Microsoft.VisualBasic.Constants.vbLf & "second line"
        Dim second = Blueprint(True, 2, 0)
        second.ComponentName = "Second Lag"
        Using s = Session(first, second)
            Press(s, "1122112212")
            Advance(s)
            Press(s, "211212")
            Advance(s)
            Press(s, "22111122")
            Dim rows = ExportReport(s)
            Dim sequences = rows.Where(Function(row) row(0) = "Sequence").ToArray()
            Check(sequences.Select(Function(row) row(7)).SequenceEqual({"1122", "1122", "21", "12", "12", "2211", "1122"}), "report includes every passing and failing complete sequence in occurrence order")
            Check(sequences.Select(Function(row) row(3)).SequenceEqual({"1", "1", "2", "2", "2", "1", "1"}), "report preserves mixed Lag component identities")
            Check(sequences.Select(Function(row) row(1)).SequenceEqual({"1", "2", "4", "5", "6", "7", "8"}), "order includes explicitly marked partial rows")
            Check(sequences(1)(8) = "Fail" AndAlso sequences(1)(9) = "1" AndAlso sequences(1)(10) = "1", "report retains criterion result and comparison settings")
            Check(sequences(5)(5) = "2" AndAlso sequences(5)(6) = "3", "returning component preserves presentation and sequence number")
            Check(sequences.All(Function(row) row(16) = "Test" AndAlso row(17) = "Simulation" AndAlso row(18) = "No hardware"), "every sequence identifies chamber, subject and session")
            Check(sequences(0)(4) = first.ComponentName, "CSV escapes comma, quotation mark and newline in component label")
            Dim partialRow = rows.Single(Function(row) row(0) = "Partial")
            Check(partialRow(7) = "12" AndAlso partialRow(8) = "Incomplete" AndAlso partialRow(14) = "ComponentEnd", "partial exported distinctly with reason")
            Dim frequencies = rows.Where(Function(row) row(0) = "FrequencyAll").ToArray()
            Check(frequencies.Select(Function(row) row(7)).SequenceEqual({"1122", "12", "21", "2211"}), "global frequencies descend, with deterministic ordinal ties")
            Check(frequencies.Select(Function(row) row(11)).SequenceEqual({"3", "2", "1", "1"}), "all complete occurrences counted, partial excluded despite matching another component's sequence")
            Check(frequencies(0)(12) = "2" AndAlso frequencies(0)(13) = "1", "frequency separates passing and failing counts")
            Dim perComponent = rows.Where(Function(row) row(0) = "FrequencyComponent").ToArray()
            Check(perComponent.Count(Function(row) row(3) = "1") = 2 AndAlso perComponent.Count(Function(row) row(3) = "2") = 2, "frequency sections retain distinct components")
            Check(rows.Single(Function(row) row(0) = "Totals")(11) = "7", "complete total agrees with ordered records")
        End Using
        Using s = Session(Blueprint(True), Blueprint(False))
            Press(s, "11221122") : Advance(s) : Press(s, "1111")
            Check(ExportReport(s).Where(Function(row) row(0) = "Sequence").Count() = 2, "Classic responses never enter Lag report")
        End Using
        Using s = Session(Blueprint(True))
            Dim rows = ExportReport(s)
            Check(Not rows.Any(Function(row) row(0).StartsWith("Frequency")) AndAlso rows.Single(Function(row) row(0) = "Totals")(11) = "0", "zero-sequence sessions produce valid empty report")
        End Using
        Dim sparse = Blueprint(True, 2, 0)
        sparse.ScheduleType(0) = "N/A" : sparse.ScheduleType(5) = "Lag" : sparse.Reinforcer(5) = "Pellet" : sparse.Magnitude(5) = 1
        sparse.DeliveryP(1) = 0 : sparse.DeliveryP(5) = 0
        Using s = Session(sparse)
            Press(s, "2662")
            Dim rows = ExportReport(s)
            Check(rows.Where(Function(row) row(0) = "Sequence").Select(Function(row) row(7)).SequenceEqual({"26", "62"}), "report retains sparse physical input numbers")
            Check(s.TotalReinforcers = 0 AndAlso rows.Where(Function(row) row(0) = "Sequence").All(Function(row) row(8) = "Pass"), "criterion pass is not mislabeled as delivered reinforcement")
        End Using
        Dim root = Path.Combine(Environment.CurrentDirectory, "obj", "LagReportTests", Guid.NewGuid().ToString("N"))
        For Each closeMode In {"Finish", "FailSession", "Dispose"}
            Dim folder = Path.Combine(root, closeMode)
            Using s = Session(Blueprint(True))
                PrepareReportFiles(s, folder)
                Press(s, "112211221")
                If closeMode = "FailSession" Then
                    CallMethod(s, closeMode, New IOException("Simulated connection failure"))
                ElseIf closeMode = "Dispose" Then
                    s.Dispose()
                Else
                    CallMethod(s, "StartPostSession")
                    CallMethod(s, "Finish")
                End If
                DirectCast(Field(s, "lagReportThread"), System.Threading.Thread).Join()
                Dim reportFile = Path.Combine(folder, "Simulation_LagSequences.csv")
                Check(File.Exists(reportFile), closeMode & " writes complementary report")
                Dim text = File.ReadAllText(reportFile)
                Dim rows = ReportRows(text)
                Check(rows.Where(Function(row) row(0) = "Sequence").Count() = 2 AndAlso rows.Where(Function(row) row(0) = "Partial").Count() = 1, closeMode & " retains complete and partial sequences")
                CallMethod(s, "FileClose", 1)
                Check(File.ReadAllText(reportFile) = text, "repeated file close cannot duplicate or overwrite report")
                Check(File.ReadAllText(Path.Combine(folder, "Simulation_Raw.txt")).Contains("LagPartialDiscarded"), "raw source preserved")
                If closeMode = "Finish" Then Check(File.ReadAllText(Path.Combine(folder, "Simulation_Summary.txt")).Contains("criterion failed=1"), "traditional summary still written")
            End Using
        Next
        Using s = Session(Blueprint(True))
            PrepareReportFiles(s, Path.Combine(root, "ExportFailure"))
            Press(s, "1122")
            SetField(s, "lagReportPath", Path.Combine(root, "MissingDirectory", "report.csv"))
            CallMethod(s, "StartPostSession") : CallMethod(s, "Finish")
            DirectCast(Field(s, "lagReportThread"), System.Threading.Thread).Join()
            Check(s.StatusText.StartsWith("Error saving Lag sequence report"), "export failure remains visible in session status")
            Check(File.ReadAllText(CStr(Field(s, "rawPath"))).Contains("LagSequence"), "export failure does not lose raw records")
            Check(File.ReadAllText(CStr(Field(s, "summaryPath"))).Contains("criterion passed=1"), "export failure does not prevent traditional summary")
        End Using
        Using s = Session(Blueprint(True, 10000, 0))
            Dim settings = DirectCast(Field(s, "configuration"), ChamberConfiguration)
            settings.Subject = "=1+1"
            Dim sequence As String = New String("1"c, 9999) & "2"
            Dim writer As New StringWriter()
            LagSequenceReport.Write(New StringReader("1,1,""LagSequence"",1,""" & sequence & """,""Pass"",0,0"), writer, settings, "long-test")
            Dim row = ReportRows(writer.ToString()).Single(Function(value) value(0) = "Sequence")
            Check(row(7) = sequence AndAlso row(19) = "Inputs: " & sequence, "10,000-response sequence retains every digit and spreadsheet-safe text representation")
            Check(row(17) = "'=1+1", "spreadsheet-like metadata is escaped as text")
        End Using
        Using s = Session(Blueprint(True)), other = Session(Blueprint(True))
            PrepareReportFiles(s, Path.Combine(root, "LargeAsync"))
            Dim rawWriter = DirectCast(Field(s, "rawWriter"), StreamWriter)
            rawWriter.AutoFlush = False
            For n As Integer = 1 To 50000
                rawWriter.WriteLine(n & ",1,""LagSequence""," & n & ",""1122"",""" & If(n = 1, "Pass", "Fail") & """,1,5")
            Next
            rawWriter.Flush()
            CallMethod(s, "FileClose", 1)
            Dim worker = DirectCast(Field(s, "lagReportThread"), System.Threading.Thread)
            Check(worker.IsAlive AndAlso Not worker.IsBackground, "large report runs independently and survives normal application shutdown")
            Check(s.StatusText.Contains("saving Lag report"), "pending report exposed in hub status")
            Press(other, "1122")
            Check(other.TotalReinforcers = 1 AndAlso worker.IsAlive, "another chamber processes a sequence while first report exports")
            worker.Join()
            Dim rows = ReportRows(File.ReadAllText(CStr(Field(s, "lagReportPath"))))
            Check(rows.Where(Function(row) row(0) = "Sequence").Count() = 50000, "more than 32,767 sequences exported without truncation")
            Check(rows.Single(Function(row) row(0) = "FrequencyAll")(11) = "50000", "large frequency retains total across full session")
            Check(Not s.StatusText.Contains("saving Lag report"), "completed worker clears pending report status")
        End Using
        Dim invalidFolder = Path.Combine(root, "InvalidRaw")
        Directory.CreateDirectory(invalidFolder)
        Dim badRaw = Path.Combine(invalidFolder, "raw.txt")
        File.WriteAllText(badRaw, "1,1,""LagSequence"",1,""1122"",""INVALID"",0,5")
        Using s = Session(Blueprint(True))
            Dim rejected As Boolean
            Try
                LagSequenceReport.Save(badRaw, Path.Combine(invalidFolder, "report.csv"), DirectCast(Field(s, "configuration"), ChamberConfiguration), "test")
            Catch ex As InvalidDataException
                rejected = True
            End Try
            Check(rejected AndAlso Not File.Exists(Path.Combine(invalidFolder, "report.csv")), "malformed source cannot publish a misleading completed report")
            Check(File.Exists(badRaw), "malformed source remains available for recovery")
        End Using
        Console.WriteLine("Report fixtures: " & root)
    End Sub
    <STAThread>
    Sub Main()
        Try
            Dim lag = Blueprint(True)
            Using s = Session(lag, Blueprint(False))
                Press(s, "1122")
                Check(s.TotalReinforcers = 1, "first full sequence reinforces")
                Press(s, "1122")
                Check(s.TotalReinforcers = 1 AndAlso Tracker(s).CompletedCount = 2, "repeat fails but enters history")
                Check(Raw(s).Contains("""1122"",""Fail"",1,5"), "full failure logged with previous comparison size")
                Press(s, "1")
                Advance(s)
                Check(Tracker(s).PartialCount = 0 AndAlso Raw(s).Contains("LagPartialDiscarded"), "partial discarded and logged")
                Advance(s)
                Check(CByte(Field(s, "vCC")) = 1 AndAlso Tracker(s).CompletedCount = 2, "same component retains history")
                Press(s, "1122")
                Check(s.TotalReinforcers = 1 AndAlso Tracker(s).CompletedCount = 3, "returning component still rejects previous sequence")
                Press(s, "6")
                Check(Raw(s).Contains("LagInactiveResponse6"), "inactive physical input logged without sequence")
                Check(Tracker(s).PartialCount = 0, "inactive input does not enter sequence")
            End Using
            lag.LagUseTimeout = True : lag.LagTimeoutSeconds = 30
            Using s = Session(lag, Blueprint(False), 5)
                Press(s, "11221122")
                Check(CBool(Field(s, "lagTimeoutActive")), "failure starts TO")
                Press(s, "2")
                Check(Tracker(s).PartialCount = 0 AndAlso Raw(s).Contains("LagTOResponse2"), "TO responses logged, not sequenced")
                Advance(s)
                Check(Not CBool(Field(s, "lagTimeoutActive")) AndAlso Not DirectCast(Field(s, "lagTimeoutTimer"), Timer).Enabled, "component end interrupts timeout")
                CallMethod(s, "LagTimeoutTick", Nothing, EventArgs.Empty)
                Check(DirectCast(Field(s, "tmrICI"), Timer).Enabled AndAlso Raw(s).Contains("LagTOInterrupted:ComponentEnd"), "stale TO tick cannot escape ICI")
            End Using
            Using s = Session(lag)
                Press(s, "11221122")
                SetField(s, "lagTimeoutDeadline", 0L)
                CallMethod(s, "LagTimeoutTick", Nothing, EventArgs.Empty)
                Check(Not CBool(Field(s, "lagTimeoutActive")) AndAlso Raw(s).Contains("LagTOEnd"), "TO expiry restores component")
                Press(s, "2211")
                Check(s.TotalReinforcers = 2, "new sequence after timeout reinforces")
            End Using
            lag.LagUseTimeout = False
            lag.MaxRefs = 1
            For i As Integer = 0 To 1
                lag.DelayDuration(i) = 1
                lag.DelayType(i) = "Unsignaled"
                lag.DelayRetract(i) = True
            Next
            Using s = Session(lag, Blueprint(False))
                Press(s, "1122")
                Check(s.TotalReinforcers = 0 AndAlso DirectCast(Field(s, "DelayTimers"), Timer())(1).Enabled, "success starts shared delay")
                Press(s, "1")
                Check(Raw(s).Contains("LagDelayResponse1") AndAlso Tracker(s).PartialCount = 0, "delay preserves actual input, excludes sequence")
                CallMethod(s, "DelayTimer_Tick", DirectCast(Field(s, "DelayTimers"), Timer())(1), EventArgs.Empty)
                Check(s.TotalReinforcers = 1 AndAlso CByte(Field(s, "vCC")) = 2, "delayed MaxRefs advances synchronously")
                Check(DirectCast(Field(s, "ObtainedDelayDurations"), List(Of Integer)(,))(1, 1).Count = 1, "obtained delay attributed to old component")
                Check(DirectCast(Field(s, "lagTrackers"), LagSequenceTracker())(2) Is Nothing, "next Classic untouched by old lag delay")
                CallMethod(s, "DelayTimer_Tick", DirectCast(Field(s, "DelayTimers"), Timer())(1), EventArgs.Empty)
                Check(s.TotalReinforcers = 1, "cancelled delay callback cannot reinforce next Classic component")
                Press(s, "11")
                Check(s.TotalReinforcers = 2, "FR still works after Lag")
            End Using
            Using s = Session(Blueprint(True), Blueprint(False))
                Press(s, "112")
                SetField(s, "lagComponentDeadline", 0L)
                Press(s, "2")
                Check(Tracker(s).CompletedCount = 0 AndAlso CByte(Field(s, "vCC")) = 2, "deadline wins over fourth response")
                Check(Raw(s).Contains("LagBoundaryResponse2"), "boundary response logged")
            End Using
            Dim t = Blueprint(False)
            t.ScheduleType(0) = "T Schedule"
            t.TDurationD(0) = 10 : t.TDurationDelta(0) = 10 : t.TCycles(0) = 1 : t.TProbabilityD(0) = 100
            Using s = Session(t)
                Press(s, "1")
                Check(s.TotalReinforcers = 1, "T schedule TD regression")
                CallMethod(s, "TPeriodTimer_Tick", DirectCast(Field(s, "TPeriodTimers"), Timer())(0), EventArgs.Empty)
                Press(s, "1")
                Check(s.TotalReinforcers = 1, "T delta probability zero regression")
            End Using
            Using a = Session(Blueprint(True)), b = Session(Blueprint(True))
                Press(a, "11221122") : Press(b, "1122")
                Check(a.TotalReinforcers = 1 AndAlso b.TotalReinforcers = 1 AndAlso Tracker(b).CompletedCount = 1, "boxes have independent histories")
            End Using
            Using s = Session(Blueprint(True))
                For repeat As Integer = 1 To 10
                    For Each sequence In {"1111", "1112", "1121", "1122", "1211", "1212"}
                        Press(s, sequence)
                    Next
                Next
                Check(s.TotalReinforcers = 60 AndAlso Tracker(s).CompletedCount = 60, "six distinct sequences cycle indefinitely under Lag 5")
                Press(s, "112")
                CallMethod(s, "StartPostSession")
                Check(Tracker(s).PartialCount = 0 AndAlso Raw(s).Contains("LagPartialDiscarded"), "manual finish discards and logs partial")
            End Using
            Dim timeoutLag = Blueprint(True)
            timeoutLag.LagUseTimeout = True : timeoutLag.LagTimeoutSeconds = 30
            Using s = Session(timeoutLag, Blueprint(False))
                Press(s, "11221122")
                SetField(s, "lagComponentDeadline", 0L)
                SetField(s, "lagTimeoutDeadline", 0L)
                CallMethod(s, "LagTimeoutTick", Nothing, EventArgs.Empty)
                Check(CByte(Field(s, "vCC")) = 2 AndAlso Not CBool(Field(s, "lagTimeoutActive")), "component deadline wins simultaneous TO expiry")
                Check(Not Raw(s).Contains("LagTOEnd"), "expired component interrupts instead of resuming timeout outputs")
            End Using
            Using s = Session(timeoutLag)
                Press(s, "11221122")
                CallMethod(s, "StartPostSession")
                Check(Not CBool(Field(s, "lagTimeoutActive")) AndAlso Not DirectCast(Field(s, "lagTimeoutTimer"), Timer).Enabled, "manual finish stops failure timeout")
            End Using
            Dim feedbackLag = Blueprint(True)
            feedbackLag.FeedbackType(0) = "Light 1 + Tone"
            feedbackLag.FeedbackDuration(0) = 0
            feedbackLag.FeedbackType(1) = "Tone"
            feedbackLag.FeedbackDuration(1) = 1
            Using s = Session(feedbackLag, Blueprint(False), 5)
                Dim commands = DirectCast(Field(s, "simulationCommands"), List(Of String))
                Dim persistent = DirectCast(Field(s, "lagPersistentFeedback"), Boolean())
                Dim timers = DirectCast(Field(s, "FeedbackTimers"), Timer())
                commands.Clear()
                Press(s, "1")
                Check(persistent(0) AndAlso Not timers(0).Enabled AndAlso commands.Contains("A") AndAlso commands.Contains("T"), "zero duration turns on outputs without timer")
                Press(s, "2")
                commands.Clear()
                CallMethod(s, "FeedbackTimer_Tick", timers(1), EventArgs.Empty)
                Check(Not commands.Contains("t") AndAlso persistent(0), "timed pulse expiry cannot interrupt shared persistent tone")
                commands.Clear()
                Press(s, "11")
                Check(Not persistent.Any(Function(active) active) AndAlso commands.Contains("a") AndAlso commands.Contains("t"), "sequence end releases zero-duration outputs")
                Press(s, "12")
                commands.Clear()
                Press(s, "11")
                Check(commands.Contains("a") AndAlso Not commands.Contains("t") AndAlso timers(1).Enabled, "sequence end preserves overlapping positive pulse")
                commands.Clear()
                CallMethod(s, "FeedbackTimer_Tick", timers(1), EventArgs.Empty)
                Check(commands.Contains("t"), "last shared owner expiry turns output off")
                Press(s, "1")
                Advance(s)
                Check(Not persistent.Any(Function(active) active) AndAlso timers.All(Function(timer) Not timer.Enabled), "component end clears feedback before ICI")
                commands.Clear()
                CallMethod(s, "FeedbackTimer_Tick", timers(1), EventArgs.Empty)
                Check(commands.Count = 0, "stale feedback callback cannot affect ICI")
            End Using
            feedbackLag.ComponentStimType = "Tone + Light 1"
            feedbackLag.HouselightOnOff = True
            feedbackLag.FeedbackType(0) = "Tone + Light 1 + Houselight"
            Using s = Session(feedbackLag)
                Dim commands = DirectCast(Field(s, "simulationCommands"), List(Of String))
                Press(s, "1")
                commands.Clear()
                Press(s, "111")
                Check(Not commands.Any(Function(command) command = "t" OrElse command = "a" OrElse command = "h"), "sequence release never interrupts shared component outputs")
            End Using
            feedbackLag.ComponentStimDuration = 1
            feedbackLag.ComponentLightIntermittency = 1
            Using s = Session(feedbackLag)
                Dim commands = DirectCast(Field(s, "simulationCommands"), List(Of String))
                Press(s, "1")
                commands.Clear()
                CallMethod(s, "tmrComponentStim_Tick", Nothing, EventArgs.Empty)
                CallMethod(s, "tmrComponentLightStim_Tick", Nothing, EventArgs.Empty)
                Check(Not commands.Contains("t") AndAlso Not commands.Contains("a"), "component intermittency cannot interrupt persistent feedback")
                commands.Clear()
                Press(s, "111")
                Check(commands.Contains("a") AndAlso commands.Contains("t") AndAlso Not commands.Contains("h"), "release honors current component off phase and houselight owner")
                Press(s, "1")
                CallMethod(s, "StartPostSession")
                Check(Not DirectCast(Field(s, "lagPersistentFeedback"), Boolean()).Any(Function(active) active), "manual session stop clears persistent feedback")
            End Using
            Dim allInputsLag = Blueprint(True, 6, 0)
            For i As Integer = 0 To 5
                allInputsLag.ScheduleType(i) = "Lag"
                allInputsLag.Reinforcer(i) = "Pellet" : allInputsLag.Magnitude(i) = 1
                allInputsLag.FeedbackType(i) = New String() {"Light 1", "Light 2", "Light 3", "Light 4", "Tone", "Houselight"}(i)
                allInputsLag.FeedbackDuration(i) = 0
            Next
            Using s = Session(allInputsLag)
                Dim commands = DirectCast(Field(s, "simulationCommands"), List(Of String))
                commands.Clear()
                Press(s, "123456")
                Check(New String() {"A", "B", "E", "F", "T", "H", "a", "b", "e", "f", "t", "h"}.All(Function(command) commands.Contains(command)), "all six active inputs turn their own outputs on and release at sequence end")
                Check(Not DirectCast(Field(s, "lagPersistentFeedback"), Boolean()).Any(Function(active) active), "six input sequence leaves no persistent outputs")
            End Using
            Dim oneResponse = Blueprint(True, 1, 0)
            oneResponse.FeedbackType(0) = "Tone" : oneResponse.FeedbackDuration(0) = 0
            oneResponse.MaxRefs = 1
            Using s = Session(oneResponse, Blueprint(False))
                Press(s, "1")
                Check(CByte(Field(s, "vCC")) = 2 AndAlso Not DirectCast(Field(s, "lagPersistentFeedback"), Boolean()).Any(Function(active) active), "length one and synchronous MaxRefs do not leak feedback into next component")
            End Using
            RunReportTests()
            Console.WriteLine("PASS runtime: " & assertions & " assertions; no hardware, network, or real data used.")
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
End Module

