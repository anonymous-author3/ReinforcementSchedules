Public Class SetUp
    Private ReadOnly SetUpToolTip As New ToolTip()

    Private Sub WriteConfigLine(fileNumber As Integer, label As String, value As Object)
        WriteLine(fileNumber, label & ": " & CStr(value))
    End Sub

    Private Sub WriteConfigurationSnapshot(fileNumber As Integer, includeSavedAt As Boolean)
        If includeSavedAt Then WriteConfigLine(fileNumber, "Saved At", Format(Date.Now, "dd-MM-yyyy_hh-mm-ss"))
        WriteConfigLine(fileNumber, "Component Count", vCC)
        WriteConfigLine(fileNumber, "COM Port", txtCOM.Text)
        WriteConfigLine(fileNumber, "Start Delay Seconds", txbStart.Text)
        WriteConfigLine(fileNumber, "Post Session Seconds", txbPostSession.Text)
        WriteConfigLine(fileNumber, "Inter Component Interval Seconds", txbICI.Text)
        WriteConfigLine(fileNumber, "Inter Component Interval Enabled", chkICIEnabled.Checked)
        WriteConfigLine(fileNumber, "Inter Component Interval Retract Inputs", chkICIRetractInputs.Checked)
        WriteConfigLine(fileNumber, "Inter Component Interval Stimulus Type", ICIStimulusType())
        WriteConfigLine(fileNumber, "Random Component Presentation", CheckBox1.Checked)
        WriteConfigLine(fileNumber, "Component Presentation Order", txbComponentOrder.Text)

        For i = 1 To vCC
            WriteConfigLine(fileNumber, "Component " & i & " Houselight On Off", AC(i).HouselightOnOff)
            WriteConfigLine(fileNumber, "Component " & i & " Changeover Delay Ms", AC(i).COD)
            WriteConfigLine(fileNumber, "Component " & i & " Max Reinforcers", AC(i).MaxRefs)
            WriteConfigLine(fileNumber, "Component " & i & " Name", ComponentDisplayName(i))
            WriteConfigLine(fileNumber, "Component " & i & " Duration Seconds", AC(i).ComponentDuration)
            WriteConfigLine(fileNumber, "Component " & i & " Iterations", AC(i).ComponentIteration)
            WriteConfigLine(fileNumber, "Component " & i & " Light Intermittency Seconds", AC(i).ComponentLightIntermittency)
            WriteConfigLine(fileNumber, "Component " & i & " Tone Intermittency Seconds", AC(i).ComponentStimDuration)
            WriteConfigLine(fileNumber, "Component " & i & " Stimulus Type", AC(i).ComponentStimType)
            WriteConfigLine(fileNumber, "Component " & i & " Time Schedule Type", If(AC(i).TimeScheduleType = "", "None", AC(i).TimeScheduleType))
            WriteConfigLine(fileNumber, "Component " & i & " Time Schedule Value Seconds", AC(i).TimeScheduleValue)
            WriteConfigLine(fileNumber, "Component " & i & " Time Reinforcer Magnitude", AC(i).TimeMagnitude)
            WriteConfigLine(fileNumber, "Component " & i & " Time Reinforcer Type", If(AC(i).TimeReinforcer = "", "N/A", AC(i).TimeReinforcer))
            WriteConfigLine(fileNumber, "Component " & i & " Time Reinforcer Delivery Probability", AC(i).TimeDeliveryP)
            WriteConfigLine(fileNumber, "Component " & i & " Time Reinforcer Pellet Probability", AC(i).TimePelletP)
            WriteConfigLine(fileNumber, "Component " & i & " Input Count", AC(i).InputCount)

            For inputIndex As Integer = 0 To AC(i).InputCount - 1
                Dim noInputReinforcer As Boolean = HasNoInputReinforcer(i, inputIndex)
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Name", AC(i).InputName(inputIndex))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Schedule Type", AC(i).ScheduleType(inputIndex))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Schedule Value", If(noInputReinforcer, "N/A", CStr(AC(i).ScheduleValue(inputIndex))))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Reinforcer Magnitude", If(noInputReinforcer, "N/A", CStr(AC(i).Magnitude(inputIndex))))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Reinforcer Type", If(noInputReinforcer, "N/A", AC(i).Reinforcer(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Reinforcer Delivery Probability", If(noInputReinforcer, "N/A", CStr(AC(i).DeliveryP(inputIndex))))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Pellet Probability", If(noInputReinforcer, "N/A", CStr(AC(i).PelletP(inputIndex))))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " T TD Seconds", If(AC(i).TDurationD Is Nothing, 0, AC(i).TDurationD(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " T TDelta Seconds", If(AC(i).TDurationDelta Is Nothing, 0, AC(i).TDurationDelta(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " T Probability TD", If(AC(i).TProbabilityD Is Nothing, 0, AC(i).TProbabilityD(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " T Probability TDelta", If(AC(i).TProbabilityDelta Is Nothing, 0, AC(i).TProbabilityDelta(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " T Cycles", If(AC(i).TCycles Is Nothing, 0, AC(i).TCycles(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " T Start Period", If(AC(i).TStartPeriod Is Nothing, "", AC(i).TStartPeriod(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " T Inter Period Seconds", If(AC(i).TInterPeriod Is Nothing, 0, AC(i).TInterPeriod(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " T TD Stimulus Type", If(AC(i).TStimD Is Nothing, "None", AC(i).TStimD(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " T TDelta Stimulus Type", If(AC(i).TStimDelta Is Nothing, "None", AC(i).TStimDelta(inputIndex)))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Feedback Duration Seconds", AC(i).FeedbackDuration(inputIndex))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Feedback Type", AC(i).FeedbackType(inputIndex))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Delay Duration Seconds", AC(i).DelayDuration(inputIndex))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Delay Type", AC(i).DelayType(inputIndex))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Delay Retract", AC(i).DelayRetract(inputIndex))
                WriteConfigLine(fileNumber, "Component " & i & " Input " & (inputIndex + 1) & " Delay Signal Duration Seconds", AC(i).DelaySignalDuration(inputIndex))
            Next
        Next
    End Sub

    Private Function ReadConfigValue(reader As System.IO.TextReader) As String
        Dim line As String = reader.ReadLine()
        If line Is Nothing Then Throw New System.IO.InvalidDataException("Unexpected end of configuration file.")

        Dim separatorIndex As Integer = line.IndexOf(":"c)
        If separatorIndex >= 0 Then Return line.Substring(separatorIndex + 1).Replace("""", "").Replace("#", "").Trim()

        Return line.Replace("""", "").Replace("#", "").Trim()
    End Function

    Private Function ComponentBlueprintClone(source As ComponentBlueprint) As ComponentBlueprint
        Dim copy As ComponentBlueprint = source
        If source.ComponentDuration_measured IsNot Nothing Then copy.ComponentDuration_measured = DirectCast(source.ComponentDuration_measured.Clone(), Integer())
        If source.InputName IsNot Nothing Then copy.InputName = DirectCast(source.InputName.Clone(), String())
        If source.ScheduleType IsNot Nothing Then copy.ScheduleType = DirectCast(source.ScheduleType.Clone(), String())
        If source.ScheduleValue IsNot Nothing Then copy.ScheduleValue = DirectCast(source.ScheduleValue.Clone(), Integer())
        If source.Magnitude IsNot Nothing Then copy.Magnitude = DirectCast(source.Magnitude.Clone(), Integer())
        If source.Reinforcer IsNot Nothing Then copy.Reinforcer = DirectCast(source.Reinforcer.Clone(), String())
        If source.DeliveryP IsNot Nothing Then copy.DeliveryP = DirectCast(source.DeliveryP.Clone(), Integer())
        If source.PelletP IsNot Nothing Then copy.PelletP = DirectCast(source.PelletP.Clone(), Integer())
        If source.TDurationD IsNot Nothing Then copy.TDurationD = DirectCast(source.TDurationD.Clone(), Double())
        If source.TDurationDelta IsNot Nothing Then copy.TDurationDelta = DirectCast(source.TDurationDelta.Clone(), Double())
        If source.TProbabilityD IsNot Nothing Then copy.TProbabilityD = DirectCast(source.TProbabilityD.Clone(), Integer())
        If source.TProbabilityDelta IsNot Nothing Then copy.TProbabilityDelta = DirectCast(source.TProbabilityDelta.Clone(), Integer())
        If source.TCycles IsNot Nothing Then copy.TCycles = DirectCast(source.TCycles.Clone(), Integer())
        If source.TStartPeriod IsNot Nothing Then copy.TStartPeriod = DirectCast(source.TStartPeriod.Clone(), String())
        If source.TInterPeriod IsNot Nothing Then copy.TInterPeriod = DirectCast(source.TInterPeriod.Clone(), Double())
        If source.TStimD IsNot Nothing Then copy.TStimD = DirectCast(source.TStimD.Clone(), String())
        If source.TStimDelta IsNot Nothing Then copy.TStimDelta = DirectCast(source.TStimDelta.Clone(), String())
        If source.FeedbackDuration IsNot Nothing Then copy.FeedbackDuration = DirectCast(source.FeedbackDuration.Clone(), Double())
        If source.FeedbackType IsNot Nothing Then copy.FeedbackType = DirectCast(source.FeedbackType.Clone(), String())
        If source.DelayDuration IsNot Nothing Then copy.DelayDuration = DirectCast(source.DelayDuration.Clone(), Double())
        If source.DelaySignalDuration IsNot Nothing Then copy.DelaySignalDuration = DirectCast(source.DelaySignalDuration.Clone(), Double())
        If source.DelayType IsNot Nothing Then copy.DelayType = DirectCast(source.DelayType.Clone(), String())
        If source.DelayRetract IsNot Nothing Then copy.DelayRetract = DirectCast(source.DelayRetract.Clone(), Boolean())
        Return copy
    End Function

    Private Function SnapshotComponents() As ComponentBlueprint()
        Dim snapshot(AC.Length - 1) As ComponentBlueprint
        For i As Integer = 0 To AC.Length - 1
            snapshot(i) = ComponentBlueprintClone(AC(i))
        Next
        Return snapshot
    End Function

    Private Sub RestoreComponents(snapshot() As ComponentBlueprint)
        For i As Integer = 0 To Math.Min(AC.Length, snapshot.Length) - 1
            AC(i) = ComponentBlueprintClone(snapshot(i))
        Next
    End Sub

    Private Function ConfigValueFromLine(line As String) As String
        If line Is Nothing Then Return ""

        Dim separatorIndex As Integer = line.IndexOf(":"c)
        If separatorIndex >= 0 Then Return line.Substring(separatorIndex + 1).Replace("""", "").Replace("#", "").Trim()

        Return line.Replace("""", "").Replace("#", "").Trim()
    End Function

    Private Function ConfigLineStartsWith(line As String, label As String) As Boolean
        If line Is Nothing Then Return False

        Return line.Trim().Trim(""""c).StartsWith(label, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function BoolFromConfig(value As String) As Boolean
        Return value.Trim().Replace("#", "").Equals("True", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function IntFromText(value As String, defaultValue As Integer) As Integer
        Dim parsed As Integer
        If Integer.TryParse(value, parsed) Then Return parsed
        Return defaultValue
    End Function

    Public Function ICIDurationSeconds() As Integer
        If chkICIEnabled.Checked = False Then Return 0
        Return Math.Max(0, IntFromText(txbICI.Text, 0))
    End Function

    Public Function ICIRetractInputs() As Boolean
        Return chkICIEnabled.Checked AndAlso chkICIRetractInputs.Checked
    End Function

    Public Function ICIStimulusType() As String
        Dim selected As New List(Of String)

        If chkICILight1.Checked Then selected.Add("Light 1")
        If chkICILight2.Checked Then selected.Add("Light 2")
        If chkICILight3.Checked Then selected.Add("Light 3")
        If chkICILight4.Checked Then selected.Add("Light 4")
        If chkICITone.Checked Then selected.Add("Tone")
        If chkICIHouselight.Checked Then selected.Add("Houselight")

        If selected.Count = 0 Then Return "None"
        Return String.Join(" + ", selected)
    End Function

    Private Sub ApplyICIStimulusType(stimulusType As String)
        If stimulusType Is Nothing Then stimulusType = ""

        chkICILight1.Checked = stimulusType.Contains("Light 1")
        chkICILight2.Checked = stimulusType.Contains("Light 2")
        chkICILight3.Checked = stimulusType.Contains("Light 3")
        chkICILight4.Checked = stimulusType.Contains("Light 4")
        chkICITone.Checked = stimulusType.Contains("Tone")
        chkICIHouselight.Checked = stimulusType.Contains("Houselight")
    End Sub

    Private Sub UpdateICIState()
        grpICI.Visible = chkICIEnabled.Checked
    End Sub

    Private Sub UpdateComponentOrderState()
        txbComponentOrder.Enabled = CheckBox1.Checked = False
        If CheckBox1.Checked Then txbComponentOrder.Text = ""
    End Sub

    Public Function ManualComponentSequence() As List(Of Integer)
        Dim sequenceText As String = If(txbComponentOrder.Text, "").Trim()
        If sequenceText = "" Then Return Nothing

        Dim sequence As New List(Of Integer)
        Dim parts() As String = sequenceText.Split(New Char() {","c, ";"c, " "c, vbTab}, StringSplitOptions.RemoveEmptyEntries)
        Dim componentLimit As Integer = If(MAXvCC > 0, MAXvCC, vCC)

        For Each part As String In parts
            Dim componentIndex As Integer
            If Integer.TryParse(part.Trim(), componentIndex) = False OrElse componentIndex < 1 OrElse componentIndex > componentLimit Then
                Return Nothing
            End If
            sequence.Add(componentIndex)
        Next

        If sequence.Count = 0 Then Return Nothing
        Return sequence
    End Function

    Private Function ManualComponentSequenceIsValid() As Boolean
        Return ManualComponentSequenceValidationError() = ""
    End Function

    Private Function ManualComponentSequenceValidationError() As String
        If If(txbComponentOrder.Text, "").Trim() = "" Then Return ""

        Dim sequence As List(Of Integer) = ManualComponentSequence()
        If sequence Is Nothing Then
            Return "The component order list is invalid. Use component numbers between 1 and " & vCC & ", separated by commas or spaces."
        End If

        Dim expectedCounts(vCC) As Integer
        Dim actualCounts(vCC) As Integer
        Dim expectedTotal As Integer = 0

        For componentIndex As Integer = 1 To vCC
            expectedCounts(componentIndex) = Math.Max(0, CInt(AC(componentIndex).ComponentIteration))
            expectedTotal += expectedCounts(componentIndex)
        Next

        If sequence.Count <> expectedTotal Then
            Return "The component order list has " & sequence.Count & " presentations, but the programmed component iterations require " & expectedTotal & ". Please list every programmed presentation."
        End If

        For Each componentIndex As Integer In sequence
            actualCounts(componentIndex) += 1
        Next

        For componentIndex As Integer = 1 To vCC
            If actualCounts(componentIndex) <> expectedCounts(componentIndex) Then
                Return "Component " & componentIndex & " appears " & actualCounts(componentIndex) & " time(s) in the order list, but it is programmed for " & expectedCounts(componentIndex) & " iteration(s)."
            End If
        Next

        Return ""
    End Function

    Private Sub ApplyManualComponentSequence(sequence As List(Of Integer))
        If sequence Is Nothing Then Exit Sub

        Dim counts(MAXvCC) As Integer
        For Each componentIndex As Integer In sequence
            counts(componentIndex) += 1
        Next

        For i = 1 To MAXvCC
            AC(i).ComponentIteration = CByte(Math.Min(255, counts(i)))
            AC(i).IterationsLeft = AC(i).ComponentIteration
            ReDim AC(i).ComponentDuration_measured(AC(i).ComponentIteration)
        Next
    End Sub

    Private Function ComponentDisplayName(componentIndex As Integer) As String
        If AC(componentIndex).ComponentName <> "" Then Return AC(componentIndex).ComponentName
        Return "Component " & componentIndex
    End Function

    Private Function IsExtinction(componentIndex As Integer, inputIndex As Integer) As Boolean
        Return AC(componentIndex).ScheduleType(inputIndex) = "Extinction"
    End Function

    Private Function IsInputUnavailable(componentIndex As Integer, inputIndex As Integer) As Boolean
        Return AC(componentIndex).ScheduleType(inputIndex) = "N/A"
    End Function

    Private Function HasNoInputReinforcer(componentIndex As Integer, inputIndex As Integer) As Boolean
        Return IsExtinction(componentIndex, inputIndex) OrElse IsInputUnavailable(componentIndex, inputIndex)
    End Function

    Private Function ScheduleDisplay(componentIndex As Integer, inputIndex As Integer) As String
        If IsInputUnavailable(componentIndex, inputIndex) Then Return "N/A"
        If IsExtinction(componentIndex, inputIndex) Then Return "Extinction"
        If AC(componentIndex).ScheduleType(inputIndex) = "T Schedule" Then
            Dim td As Double = If(AC(componentIndex).TDurationD Is Nothing, 0, AC(componentIndex).TDurationD(inputIndex))
            Dim tDelta As Double = If(AC(componentIndex).TDurationDelta Is Nothing, 0, AC(componentIndex).TDurationDelta(inputIndex))
            Dim pTd As Integer = If(AC(componentIndex).TProbabilityD Is Nothing, 0, AC(componentIndex).TProbabilityD(inputIndex))
            Dim pDelta As Integer = If(AC(componentIndex).TProbabilityDelta Is Nothing, 0, AC(componentIndex).TProbabilityDelta(inputIndex))
            Dim cycles As Integer = If(AC(componentIndex).TCycles Is Nothing, 0, AC(componentIndex).TCycles(inputIndex))
            Return "T Schedule TD " & td & " s / TΔ " & tDelta & " s / P " & pTd & "%|" & pDelta & "% / " & cycles & " cycles"
        End If
        Return AC(componentIndex).ScheduleType(inputIndex) & " " & AC(componentIndex).ScheduleValue(inputIndex)
    End Function

    Private Function ReinforcerDisplay(componentIndex As Integer, inputIndex As Integer) As String
        If HasNoInputReinforcer(componentIndex, inputIndex) Then Return "N/A"

        Dim reinforcer As String = AC(componentIndex).Reinforcer(inputIndex)
        Dim displayText As String = AC(componentIndex).Magnitude(inputIndex) & " " & reinforcer
        If AC(componentIndex).DeliveryP IsNot Nothing AndAlso inputIndex <= AC(componentIndex).DeliveryP.Length - 1 Then
            displayText &= " / Deliver " & AC(componentIndex).DeliveryP(inputIndex) & "%"
        End If

        If reinforcer = "Random" Then
            displayText &= " (" & AC(componentIndex).PelletP(inputIndex) & "% pellet)"
        End If

        Return displayText
    End Function

    Private Function TimeScheduleDisplay(componentIndex As Integer) As String
        If AC(componentIndex).TimeScheduleType = "" OrElse AC(componentIndex).TimeScheduleType = "None" Then Return "None"

        Dim displayText As String = AC(componentIndex).TimeScheduleType & " " & AC(componentIndex).TimeScheduleValue & " s / " &
            AC(componentIndex).TimeMagnitude & " " & AC(componentIndex).TimeReinforcer & " / Deliver " & AC(componentIndex).TimeDeliveryP & "%"

        If AC(componentIndex).TimeReinforcer = "Random" Then
            displayText &= " (" & AC(componentIndex).TimePelletP & "% pellet)"
        End If

        Return displayText
    End Function

    Private Function FeedbackDisplay(componentIndex As Integer, inputIndex As Integer) As String
        If AC(componentIndex).FeedbackType(inputIndex) = "" OrElse AC(componentIndex).FeedbackType(inputIndex) = "None" Then Return ""
        If AC(componentIndex).FeedbackDuration(inputIndex) <= 0 Then Return ""
        Return AC(componentIndex).FeedbackType(inputIndex) & " / " & AC(componentIndex).FeedbackDuration(inputIndex) & " s"
    End Function

    Private Function DelayDisplay(componentIndex As Integer, inputIndex As Integer) As String
        Dim hasDelayType As Boolean = AC(componentIndex).DelayType(inputIndex) <> "" AndAlso AC(componentIndex).DelayType(inputIndex) <> "None"
        Dim hasDelayTiming As Boolean = AC(componentIndex).DelayDuration(inputIndex) > 0 OrElse AC(componentIndex).DelaySignalDuration(inputIndex) > 0

        If hasDelayType = False AndAlso hasDelayTiming = False AndAlso AC(componentIndex).DelayRetract(inputIndex) = False Then Return ""

        If AC(componentIndex).DelayType(inputIndex) = "Unsignaled" Then
            Return "Unsignaled / " & AC(componentIndex).DelayDuration(inputIndex) & " s / Ret: " & AC(componentIndex).DelayRetract(inputIndex)
        End If

        Return AC(componentIndex).DelayType(inputIndex) & " / " & AC(componentIndex).DelayDuration(inputIndex) & " s / Ret: " & AC(componentIndex).DelayRetract(inputIndex) & " / Signal: " & AC(componentIndex).DelaySignalDuration(inputIndex) & " s"
    End Function

    Private Sub AddSummaryRow(label As String, ParamArray values() As String)
        Dim rowIndex As Integer = dgvComponentSummary.Rows.Add()
        dgvComponentSummary.Rows(rowIndex).Cells(0).Value = label

        For componentIndex As Integer = 1 To values.Length
            dgvComponentSummary.Rows(rowIndex).Cells(componentIndex).Value = values(componentIndex - 1)
        Next
    End Sub

    Public Sub RefreshComponentSummaryTable()
        dgvComponentSummary.Columns.Clear()
        dgvComponentSummary.Rows.Clear()
        dgvComponentSummary.Columns.Add("Property", "Property")

        For componentIndex As Integer = 1 To vCC
            dgvComponentSummary.Columns.Add("Component" & componentIndex, "Component " & componentIndex)
        Next

        If vCC <= 0 Then Exit Sub

        AddSummaryRow("Component name", Enumerable.Range(1, vCC).Select(Function(i) ComponentDisplayName(i)).ToArray())
        AddSummaryRow("Duration (s)", Enumerable.Range(1, vCC).Select(Function(i) CStr(AC(i).ComponentDuration)).ToArray())
        AddSummaryRow("Iterations", Enumerable.Range(1, vCC).Select(Function(i) CStr(AC(i).ComponentIteration)).ToArray())
        AddSummaryRow("Stimulus", Enumerable.Range(1, vCC).Select(Function(i) AC(i).ComponentStimType).ToArray())
        AddSummaryRow("Light intermittency (s)", Enumerable.Range(1, vCC).Select(Function(i) CStr(AC(i).ComponentLightIntermittency)).ToArray())
        AddSummaryRow("Tone intermittency (s)", Enumerable.Range(1, vCC).Select(Function(i) CStr(AC(i).ComponentStimDuration)).ToArray())
        AddSummaryRow("Time schedule", Enumerable.Range(1, vCC).Select(Function(i) TimeScheduleDisplay(i)).ToArray())
        AddSummaryRow("COD (s)", Enumerable.Range(1, vCC).Select(Function(i) CStr(AC(i).COD / 1000)).ToArray())
        AddSummaryRow("Max reinforcers", Enumerable.Range(1, vCC).Select(Function(i) CStr(AC(i).MaxRefs)).ToArray())
        AddSummaryRow("Input count", Enumerable.Range(1, vCC).Select(Function(i) CStr(AC(i).InputCount)).ToArray())

        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Dim currentInput As Integer = inputIndex
            Dim hasAnyInput As Boolean = False
            For componentIndex As Integer = 1 To vCC
                If currentInput < AC(componentIndex).InputCount Then hasAnyInput = True
            Next
            If hasAnyInput = False Then Continue For

            AddSummaryRow("Input " & (currentInput + 1) & " name",
                          Enumerable.Range(1, vCC).Select(Function(i) If(currentInput < AC(i).InputCount, AC(i).InputName(currentInput), "")).ToArray())
            AddSummaryRow("Input " & (currentInput + 1) & " schedule",
                          Enumerable.Range(1, vCC).Select(Function(i) If(currentInput < AC(i).InputCount, ScheduleDisplay(i, currentInput), "")).ToArray())
            AddSummaryRow("Input " & (currentInput + 1) & " reinforcer",
                          Enumerable.Range(1, vCC).Select(Function(i) If(currentInput < AC(i).InputCount, ReinforcerDisplay(i, currentInput), "")).ToArray())

            Dim feedbackValues() As String = Enumerable.Range(1, vCC).Select(Function(i) If(currentInput < AC(i).InputCount, FeedbackDisplay(i, currentInput), "")).ToArray()
            If feedbackValues.Any(Function(value) value <> "") Then AddSummaryRow("Input " & (currentInput + 1) & " feedback", feedbackValues)

            Dim delayValues() As String = Enumerable.Range(1, vCC).Select(Function(i) If(currentInput < AC(i).InputCount, DelayDisplay(i, currentInput), "")).ToArray()
            If delayValues.Any(Function(value) value <> "") Then AddSummaryRow("Input " & (currentInput + 1) & " delay", delayValues)
        Next
    End Sub

    Private Sub SetUp_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetUpToolTip.InitialDelay = 300
        SetUpToolTip.ReshowDelay = 100
        SetUpToolTip.AutoPopDelay = 7000
        SetUpToolTip.SetToolTip(chkICIEnabled, "Use an inter-component interval between components. If unchecked, the session moves directly to the next component or ends.")
        SetUpToolTip.SetToolTip(CheckBox1, "Randomizes component presentation using the programmed component iterations.")
        SetUpToolTip.SetToolTip(txbStart, "Pre-session period in seconds before the first component starts, useful for habituation.")
        SetUpToolTip.SetToolTip(txbPostSession, "Post-session period in seconds after the session ends, useful as a resting period.")
        SetUpToolTip.SetToolTip(txbComponentOrder, "Optional explicit component order, e.g., 1,2,1,3. Leave blank to use the order components were added.")
        UpdateICIState()
        UpdateComponentOrderState()
        RefreshComponentSummaryTable()
    End Sub

    Private Sub btnComenzar_Click(sender As Object, e As EventArgs) Handles btnComenzar.Click
        Dim ask As MsgBoxResult = MsgBox("Did you test everything?", MsgBoxStyle.YesNo)
        If ask = MsgBoxResult.Yes Then

            'Do not start if no components were configured
            If vCC <= 0 Then
                MessageBox.Show("No components have been added. Please add at least one component before starting.")
                Exit Sub
            End If

            For i = 1 To vCC
                If AC(i).ComponentDuration <= 0 Then
                    MessageBox.Show("Component " & i & " has a duration of 0. Please set a valid duration before starting.")
                    Exit Sub
                End If
            Next

            Dim componentOrderError As String = ManualComponentSequenceValidationError()
            If componentOrderError <> "" Then
                MessageBox.Show(componentOrderError)
                Exit Sub
            End If

            If txbPostSession.Text = "" Then txbPostSession.Text = 0
            If txbStart.Text = "" Then txbStart.Text = 0

            'This checks for errors or missing data in the set up and prompts the user for corrections. If no problem is found the selected programs are initiated.
            If txtSubject.Text = "" Or txtSession.Text = "" Or txtCOM.Text = "" Then
                MessageBox.Show("Some session data is missing.")
            Else

                vFile(0) = "C:\Data\Raw\" & txtSubject.Text & "_" & txtSession.Text & "_Raw.txt"
                vFile(1) = "C:\Data\Summary\" & txtSubject.Text & "_" & txtSession.Text & "Summary.txt"
                FileClose(1)
                FileClose(2)
                FileOpen(1, vFile(0), OpenMode.Append)
                FileOpen(2, vFile(1), OpenMode.Append)
                SessionUID = Guid.NewGuid().ToString("N")
                WriteLine(1, "Session UID: " & SessionUID)
                WriteLine(2, Format(Date.Now, "dd-MM-yyyy_hh-mm-ss"))
                WriteLine(2, "Session UID: " & SessionUID)
                WriteLine(2, "Subject: " & txtSubject.Text)
                WriteLine(2, "Session: " & txtSession.Text)
                WriteLine(2, "Weight: " & txtWeight.Text)
                WriteLine(2, "COM Port: " & txtCOM.Text)
                WriteLine(2, "")
                WriteLine(2, "Configuration used:")
                WriteConfigurationSnapshot(2, False)
                WriteLine(2, "")
                WriteLine(2, "Event code legend:")
                MAXvCC = vCC
                For inputIndex As Integer = 0 To MAX_INPUTS - 1
                    WriteLine(2, "Input " & (inputIndex + 1) & " response: " & (inputIndex + 1))
                    WriteLine(2, "Input " & (inputIndex + 1) & " reinforcer: R" & (inputIndex + 1))
                    WriteLine(2, "Input " & (inputIndex + 1) & " r on Delay: D" & (inputIndex + 1))
                Next
                vCC = 1
                For i = 1 To MAXvCC
                    AC(i).IterationsLeft = AC(i).ComponentIteration
                    ReDim AC(i).ComponentDuration_measured(AC(i).ComponentIteration)
                Next
                ApplyManualComponentSequence(ManualComponentSequence())


                Dim x As New Main
                x.Show()
                If x.ArduinoVB() = False Then
                    FileClose(1)
                    FileClose(2)
                    Exit Sub
                End If

            End If
        End If
    End Sub

    Private Sub btnAddComponent_Click(sender As Object, e As EventArgs) Handles btnAddComponent.Click
        If vCC >= MAX_COMPONENTS Then
            MessageBox.Show("This version supports up to " & MAX_COMPONENTS & " components.")
            Exit Sub
        End If

        Dim f As New Component
        vCC += 1
        f.Text = "Component " & vCC
        f.ShowDialog()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then RandomCPres = True
        If CheckBox1.Checked = False Then RandomCPres = False
        UpdateComponentOrderState()
    End Sub

    Private Sub chkICIEnabled_CheckedChanged(sender As Object, e As EventArgs) Handles chkICIEnabled.CheckedChanged
        UpdateICIState()
    End Sub

    Private Sub btnTests_Click(sender As Object, e As EventArgs) Handles btnTests.Click
        Dim f As New Tests
        f.ShowDialog()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If vCC > 0 Then
            vFile(2) = "C:\SavedPrograms\NewProgram(Rename me please)_" & Strings.Right(CStr(Environment.TickCount), 5) & ".txt"
            FileOpen(3, vFile(2), OpenMode.Append)
            WriteConfigLine(3, "Saved At", Format(Date.Now, "dd-MM-yyyy_hh-mm-ss"))
            WriteConfigLine(3, "Component Count", vCC)
            WriteConfigLine(3, "COM Port", txtCOM.Text)
            WriteConfigLine(3, "Start Delay Seconds", txbStart.Text)
            WriteConfigLine(3, "Post Session Seconds", txbPostSession.Text)
            WriteConfigLine(3, "Inter Component Interval Seconds", txbICI.Text)
            WriteConfigLine(3, "Inter Component Interval Enabled", chkICIEnabled.Checked)
            WriteConfigLine(3, "Inter Component Interval Retract Inputs", chkICIRetractInputs.Checked)
            WriteConfigLine(3, "Inter Component Interval Stimulus Type", ICIStimulusType())
            WriteConfigLine(3, "Random Component Presentation", CheckBox1.Checked)
            WriteConfigLine(3, "Component Presentation Order", txbComponentOrder.Text)
            For i = 1 To vCC
                WriteConfigLine(3, "Component " & i & " Houselight On Off", AC(i).HouselightOnOff)
                WriteConfigLine(3, "Component " & i & " Changeover Delay Ms", AC(i).COD)
                WriteConfigLine(3, "Component " & i & " Max Reinforcers", AC(i).MaxRefs)
                WriteConfigLine(3, "Component " & i & " Name", ComponentDisplayName(i))
                WriteConfigLine(3, "Component " & i & " Duration Seconds", AC(i).ComponentDuration)
                WriteConfigLine(3, "Component " & i & " Iterations", AC(i).ComponentIteration)
                WriteConfigLine(3, "Component " & i & " Light Intermittency Seconds", AC(i).ComponentLightIntermittency)
                WriteConfigLine(3, "Component " & i & " Tone Intermittency Seconds", AC(i).ComponentStimDuration)
                WriteConfigLine(3, "Component " & i & " Stimulus Type", AC(i).ComponentStimType)
                WriteConfigLine(3, "Component " & i & " Time Schedule Type", If(AC(i).TimeScheduleType = "", "None", AC(i).TimeScheduleType))
                WriteConfigLine(3, "Component " & i & " Time Schedule Value Seconds", AC(i).TimeScheduleValue)
                WriteConfigLine(3, "Component " & i & " Time Reinforcer Magnitude", AC(i).TimeMagnitude)
                WriteConfigLine(3, "Component " & i & " Time Reinforcer Type", If(AC(i).TimeReinforcer = "", "N/A", AC(i).TimeReinforcer))
                WriteConfigLine(3, "Component " & i & " Time Reinforcer Delivery Probability", AC(i).TimeDeliveryP)
                WriteConfigLine(3, "Component " & i & " Time Reinforcer Pellet Probability", AC(i).TimePelletP)
                WriteConfigLine(3, "Component " & i & " Input Count", AC(i).InputCount)
                For inputIndex As Integer = 0 To AC(i).InputCount - 1
                    Dim noInputReinforcer As Boolean = HasNoInputReinforcer(i, inputIndex)
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Name", AC(i).InputName(inputIndex))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Schedule Type", AC(i).ScheduleType(inputIndex))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Schedule Value", If(noInputReinforcer, "N/A", CStr(AC(i).ScheduleValue(inputIndex))))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Reinforcer Magnitude", If(noInputReinforcer, "N/A", CStr(AC(i).Magnitude(inputIndex))))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Reinforcer Type", If(noInputReinforcer, "N/A", AC(i).Reinforcer(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Reinforcer Delivery Probability", If(noInputReinforcer, "N/A", CStr(AC(i).DeliveryP(inputIndex))))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Pellet Probability", If(noInputReinforcer, "N/A", CStr(AC(i).PelletP(inputIndex))))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " T TD Seconds", If(AC(i).TDurationD Is Nothing, 0, AC(i).TDurationD(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " T TDelta Seconds", If(AC(i).TDurationDelta Is Nothing, 0, AC(i).TDurationDelta(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " T Probability TD", If(AC(i).TProbabilityD Is Nothing, 0, AC(i).TProbabilityD(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " T Probability TDelta", If(AC(i).TProbabilityDelta Is Nothing, 0, AC(i).TProbabilityDelta(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " T Cycles", If(AC(i).TCycles Is Nothing, 0, AC(i).TCycles(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " T Start Period", If(AC(i).TStartPeriod Is Nothing, "", AC(i).TStartPeriod(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " T Inter Period Seconds", If(AC(i).TInterPeriod Is Nothing, 0, AC(i).TInterPeriod(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " T TD Stimulus Type", If(AC(i).TStimD Is Nothing, "None", AC(i).TStimD(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " T TDelta Stimulus Type", If(AC(i).TStimDelta Is Nothing, "None", AC(i).TStimDelta(inputIndex)))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Feedback Duration Seconds", AC(i).FeedbackDuration(inputIndex))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Feedback Type", AC(i).FeedbackType(inputIndex))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Delay Duration Seconds", AC(i).DelayDuration(inputIndex))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Delay Type", AC(i).DelayType(inputIndex))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Delay Retract", AC(i).DelayRetract(inputIndex))
                    WriteConfigLine(3, "Component " & i & " Input " & (inputIndex + 1) & " Delay Signal Duration Seconds", AC(i).DelaySignalDuration(inputIndex))
                Next
            Next
            FileClose(3)
        Else
            MessageBox.Show("Nothing to save.")
        End If
    End Sub

    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        Dim fd As OpenFileDialog = New OpenFileDialog()
        fd.Title = "Select Macro"
        fd.InitialDirectory = "C:\SavedPrograms\"
        fd.Filter = "Text files (*.txt)|*.txt"
        fd.FilterIndex = 1
        fd.RestoreDirectory = False

        If fd.ShowDialog() = DialogResult.OK Then
            Dim previousComponents() As ComponentBlueprint = SnapshotComponents()
            Dim previousVCC As Byte = vCC
            Dim previousStart As String = txbStart.Text
            Dim previousPostSession As String = txbPostSession.Text
            Dim previousICI As String = txbICI.Text
            Dim previousICIEnabled As Boolean = chkICIEnabled.Checked
            Dim previousICIRetract As Boolean = chkICIRetractInputs.Checked
            Dim previousRandom As Boolean = CheckBox1.Checked
            Dim previousOrder As String = txbComponentOrder.Text
            Dim previousCOM As String = txtCOM.Text
            Dim fileReader As System.IO.TextReader = Nothing

            Try
                vFile(3) = fd.FileName
                fileReader = My.Computer.FileSystem.OpenTextFileReader(vFile(3))
                Dim stringReader = ReadConfigValue(fileReader)

                Dim componentCountLine As String = fileReader.ReadLine()
                If ConfigLineStartsWith(componentCountLine, "Subject") Then
                    ReadConfigValue(fileReader)
                    ReadConfigValue(fileReader)
                    ReadConfigValue(fileReader)
                    componentCountLine = fileReader.ReadLine()
                End If

                vCC = ConfigValueFromLine(componentCountLine)
                If vCC <= 0 OrElse vCC > MAX_COMPONENTS Then Throw New System.IO.InvalidDataException("Unsupported component count.")

                Dim startDelayLine As String = fileReader.ReadLine()
                If ConfigLineStartsWith(startDelayLine, "COM Port") Then
                    txtCOM.Text = ConfigValueFromLine(startDelayLine).Replace("""", "")
                    startDelayLine = fileReader.ReadLine()
                End If

                txbStart.Text = ConfigValueFromLine(startDelayLine).Replace("""", "")
                txbPostSession.Text = ReadConfigValue(fileReader).Replace("""", "")
                txbICI.Text = ReadConfigValue(fileReader).Replace("""", "")

                Dim nextConfigLine As String = fileReader.ReadLine()
                If ConfigLineStartsWith(nextConfigLine, "Inter Component Interval Enabled") Then
                    chkICIEnabled.Checked = BoolFromConfig(ConfigValueFromLine(nextConfigLine))
                    chkICIRetractInputs.Checked = BoolFromConfig(ReadConfigValue(fileReader))
                    ApplyICIStimulusType(ReadConfigValue(fileReader).Replace("""", ""))
                    CheckBox1.Checked = BoolFromConfig(ReadConfigValue(fileReader))
                Else
                    chkICIEnabled.Checked = IntFromText(txbICI.Text, 0) > 0
                    chkICIRetractInputs.Checked = True
                    ApplyICIStimulusType("None")
                    CheckBox1.Checked = BoolFromConfig(ConfigValueFromLine(nextConfigLine))
                End If

                Dim firstComponentLine As String = fileReader.ReadLine()
                If ConfigLineStartsWith(firstComponentLine, "Component Presentation Order") Then
                    txbComponentOrder.Text = ConfigValueFromLine(firstComponentLine).Replace("""", "")
                    firstComponentLine = Nothing
                Else
                    txbComponentOrder.Text = ""
                End If

                UpdateICIState()
                UpdateComponentOrderState()
                For i = 1 To vCC
                    ReDim AC(i).InputName(MAX_INPUTS - 1)
                    ReDim AC(i).ScheduleType(MAX_INPUTS - 1)
                    ReDim AC(i).ScheduleValue(MAX_INPUTS - 1)
                    ReDim AC(i).Magnitude(MAX_INPUTS - 1)
                    ReDim AC(i).Reinforcer(MAX_INPUTS - 1)
                    ReDim AC(i).DeliveryP(MAX_INPUTS - 1)
                    ReDim AC(i).PelletP(MAX_INPUTS - 1)
                    ReDim AC(i).TDurationD(MAX_INPUTS - 1)
                    ReDim AC(i).TDurationDelta(MAX_INPUTS - 1)
                    ReDim AC(i).TProbabilityD(MAX_INPUTS - 1)
                    ReDim AC(i).TProbabilityDelta(MAX_INPUTS - 1)
                    ReDim AC(i).TCycles(MAX_INPUTS - 1)
                    ReDim AC(i).TStartPeriod(MAX_INPUTS - 1)
                    ReDim AC(i).TInterPeriod(MAX_INPUTS - 1)
                    ReDim AC(i).TStimD(MAX_INPUTS - 1)
                    ReDim AC(i).TStimDelta(MAX_INPUTS - 1)
                    ReDim AC(i).FeedbackDuration(MAX_INPUTS - 1)
                    ReDim AC(i).FeedbackType(MAX_INPUTS - 1)
                    ReDim AC(i).DelayDuration(MAX_INPUTS - 1)
                    ReDim AC(i).DelayType(MAX_INPUTS - 1)
                    ReDim AC(i).DelayRetract(MAX_INPUTS - 1)
                    ReDim AC(i).DelaySignalDuration(MAX_INPUTS - 1)
                    '
                    If i = 1 AndAlso firstComponentLine IsNot Nothing Then
                        AC(i).HouselightOnOff = ConfigValueFromLine(firstComponentLine).Replace("#", "")
                    Else
                        AC(i).HouselightOnOff = ReadConfigValue(fileReader).Replace("#", "")
                    End If
                    AC(i).COD = ReadConfigValue(fileReader)
                    AC(i).MaxRefs = ReadConfigValue(fileReader)
                    AC(i).ComponentName = ReadConfigValue(fileReader).Replace("""", "")
                    AC(i).ComponentDuration = ReadConfigValue(fileReader)
                    AC(i).ComponentIteration = ReadConfigValue(fileReader)
                    Dim componentStimLine As String = fileReader.ReadLine()
                    If ConfigLineStartsWith(componentStimLine, "Component " & i & " Light Intermittency Seconds") Then
                        AC(i).ComponentLightIntermittency = ConfigValueFromLine(componentStimLine)
                        AC(i).ComponentStimDuration = ReadConfigValue(fileReader)
                        AC(i).ComponentStimType = ReadConfigValue(fileReader).Replace("""", "")
                    Else
                        AC(i).ComponentLightIntermittency = 0
                        AC(i).ComponentStimDuration = ConfigValueFromLine(componentStimLine)
                        AC(i).ComponentStimType = ReadConfigValue(fileReader).Replace("""", "")
                    End If
                    Dim inputCountLine As String = fileReader.ReadLine()
                    If ConfigLineStartsWith(inputCountLine, "Component " & i & " Time Schedule Type") Then
                        AC(i).TimeScheduleType = ConfigValueFromLine(inputCountLine).Replace("""", "")
                        AC(i).TimeScheduleValue = ReadConfigValue(fileReader)
                        AC(i).TimeMagnitude = ReadConfigValue(fileReader)
                        AC(i).TimeReinforcer = ReadConfigValue(fileReader).Replace("""", "")
                        AC(i).TimeDeliveryP = ReadConfigValue(fileReader)
                        AC(i).TimePelletP = ReadConfigValue(fileReader)
                        inputCountLine = fileReader.ReadLine()
                    Else
                        AC(i).TimeScheduleType = "None"
                        AC(i).TimeScheduleValue = 0
                        AC(i).TimeMagnitude = 0
                        AC(i).TimeReinforcer = "N/A"
                        AC(i).TimeDeliveryP = 0
                        AC(i).TimePelletP = 0
                    End If

                    AC(i).InputCount = ConfigValueFromLine(inputCountLine)
                    If AC(i).InputCount <= 0 Then AC(i).InputCount = 2
                    If AC(i).InputCount > MAX_INPUTS Then AC(i).InputCount = MAX_INPUTS

                    For inputIndex As Integer = 0 To AC(i).InputCount - 1
                        AC(i).InputName(inputIndex) = ReadConfigValue(fileReader).Replace("""", "")
                        AC(i).ScheduleType(inputIndex) = ReadConfigValue(fileReader).Replace("""", "")
                        Dim scheduleValueText As String = ReadConfigValue(fileReader)
                        Dim magnitudeText As String = ReadConfigValue(fileReader)
                        Dim reinforcerText As String = ReadConfigValue(fileReader).Replace("""", "")
                        Dim deliveryOrPelletLine As String = fileReader.ReadLine()
                        Dim deliveryPText As String = "100"
                        Dim pelletPText As String
                        If ConfigLineStartsWith(deliveryOrPelletLine, "Component " & i & " Input " & (inputIndex + 1) & " Reinforcer Delivery Probability") Then
                            deliveryPText = ConfigValueFromLine(deliveryOrPelletLine)
                            pelletPText = ReadConfigValue(fileReader)
                        Else
                            pelletPText = ConfigValueFromLine(deliveryOrPelletLine)
                        End If

                        If AC(i).ScheduleType(inputIndex) = "Extinction" OrElse AC(i).ScheduleType(inputIndex) = "N/A" Then
                            AC(i).ScheduleValue(inputIndex) = 0
                            AC(i).Magnitude(inputIndex) = 0
                            AC(i).Reinforcer(inputIndex) = "N/A"
                            AC(i).DeliveryP(inputIndex) = 0
                            AC(i).PelletP(inputIndex) = 0
                        Else
                            AC(i).ScheduleValue(inputIndex) = scheduleValueText
                            AC(i).Magnitude(inputIndex) = magnitudeText
                            AC(i).Reinforcer(inputIndex) = reinforcerText
                            AC(i).DeliveryP(inputIndex) = deliveryPText
                            AC(i).PelletP(inputIndex) = pelletPText
                        End If

                        Dim feedbackDurationLine As String = fileReader.ReadLine()
                        If ConfigLineStartsWith(feedbackDurationLine, "Component " & i & " Input " & (inputIndex + 1) & " T TD Seconds") Then
                            AC(i).TDurationD(inputIndex) = ConfigValueFromLine(feedbackDurationLine)
                            AC(i).TDurationDelta(inputIndex) = ReadConfigValue(fileReader)
                            AC(i).TProbabilityD(inputIndex) = ReadConfigValue(fileReader)
                            AC(i).TProbabilityDelta(inputIndex) = ReadConfigValue(fileReader)
                            AC(i).TCycles(inputIndex) = ReadConfigValue(fileReader)
                            AC(i).TStartPeriod(inputIndex) = ReadConfigValue(fileReader).Replace("""", "")
                            AC(i).TInterPeriod(inputIndex) = ReadConfigValue(fileReader)
                            AC(i).TStimD(inputIndex) = ReadConfigValue(fileReader).Replace("""", "")
                            AC(i).TStimDelta(inputIndex) = ReadConfigValue(fileReader).Replace("""", "")
                            AC(i).FeedbackDuration(inputIndex) = ReadConfigValue(fileReader)
                        Else
                            AC(i).TDurationD(inputIndex) = 0
                            AC(i).TDurationDelta(inputIndex) = 0
                            AC(i).TProbabilityD(inputIndex) = 100
                            AC(i).TProbabilityDelta(inputIndex) = 0
                            AC(i).TCycles(inputIndex) = 0
                            AC(i).TStartPeriod(inputIndex) = "TD"
                            AC(i).TInterPeriod(inputIndex) = 0
                            AC(i).TStimD(inputIndex) = "None"
                            AC(i).TStimDelta(inputIndex) = "None"
                            AC(i).FeedbackDuration(inputIndex) = ConfigValueFromLine(feedbackDurationLine)
                        End If
                        AC(i).FeedbackType(inputIndex) = ReadConfigValue(fileReader).Replace("""", "")
                        AC(i).DelayDuration(inputIndex) = ReadConfigValue(fileReader)
                        AC(i).DelayType(inputIndex) = ReadConfigValue(fileReader).Replace("""", "")
                        AC(i).DelayRetract(inputIndex) = ReadConfigValue(fileReader).Replace("#", "")
                        AC(i).DelaySignalDuration(inputIndex) = ReadConfigValue(fileReader)
                    Next

                    For inputIndex As Integer = AC(i).InputCount To MAX_INPUTS - 1
                        AC(i).InputName(inputIndex) = ""
                        AC(i).ScheduleType(inputIndex) = "Extinction"
                        AC(i).ScheduleValue(inputIndex) = 0
                        AC(i).Magnitude(inputIndex) = 0
                        AC(i).Reinforcer(inputIndex) = "Pellet"
                        AC(i).DeliveryP(inputIndex) = 0
                        AC(i).PelletP(inputIndex) = 0
                        AC(i).TDurationD(inputIndex) = 0
                        AC(i).TDurationDelta(inputIndex) = 0
                        AC(i).TProbabilityD(inputIndex) = 100
                        AC(i).TProbabilityDelta(inputIndex) = 0
                        AC(i).TCycles(inputIndex) = 0
                        AC(i).TStartPeriod(inputIndex) = "TD"
                        AC(i).TInterPeriod(inputIndex) = 0
                        AC(i).TStimD(inputIndex) = "None"
                        AC(i).TStimDelta(inputIndex) = "None"
                        AC(i).FeedbackDuration(inputIndex) = 0
                        AC(i).FeedbackType(inputIndex) = "None"
                        AC(i).DelayDuration(inputIndex) = 0
                        AC(i).DelayType(inputIndex) = "None"
                        AC(i).DelayRetract(inputIndex) = False
                        AC(i).DelaySignalDuration(inputIndex) = 0
                    Next

                Next
                RefreshComponentSummaryTable()

                If vCC >= 2 Then
                    CheckBox1.Enabled = True
                    CheckBox1.Checked = True
                End If

            Catch ex As Exception
                RestoreComponents(previousComponents)
                vCC = previousVCC
                txbStart.Text = previousStart
                txbPostSession.Text = previousPostSession
                txbICI.Text = previousICI
                chkICIEnabled.Checked = previousICIEnabled
                chkICIRetractInputs.Checked = previousICIRetract
                CheckBox1.Checked = previousRandom
                txbComponentOrder.Text = previousOrder
                txtCOM.Text = previousCOM
                UpdateICIState()
                UpdateComponentOrderState()
                RefreshComponentSummaryTable()
                MessageBox.Show("This configuration file is not compatible with this version of the program.")
            Finally
                If fileReader IsNot Nothing Then fileReader.Close()
            End Try

        End If
    End Sub

    Private Sub btnRemoveLast_Click(sender As Object, e As EventArgs) Handles btnRemoveLast.Click

        ' Nothing to remove
        If vCC <= 0 Then Exit Sub

        ' Reset the removed component slot (Structure)
        AC(vCC) = New ComponentBlueprint
        AC(vCC).ComponentDuration_measured = Nothing
        AC(vCC).InputName = Nothing
        AC(vCC).ScheduleType = Nothing
        AC(vCC).ScheduleValue = Nothing
        AC(vCC).Magnitude = Nothing
        AC(vCC).Reinforcer = Nothing
        AC(vCC).DeliveryP = Nothing
        AC(vCC).PelletP = Nothing
        AC(vCC).FeedbackDuration = Nothing
        AC(vCC).FeedbackType = Nothing
        AC(vCC).DelayDuration = Nothing
        AC(vCC).DelayType = Nothing
        AC(vCC).DelayRetract = Nothing
        AC(vCC).DelaySignalDuration = Nothing

        ' Update component count & layout
        vCC -= 1
        RefreshComponentSummaryTable()

        ' Randomization only makes sense with 2+ components
        If vCC < 2 Then
            CheckBox1.Checked = False
            CheckBox1.Enabled = False
            RandomCPres = False
        End If

    End Sub

    Private Sub btnAuthorInfo_Click(sender As Object, e As EventArgs) Handles btnAuthorInfo.Click
        ' Displays a dialog with author information

        MsgBox("Made by XXXXX" & vbCrLf & vbCrLf &
           "• Intentionally left blank during peer-review.",
           MsgBoxStyle.Information)
    End Sub
End Class
