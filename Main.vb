Imports System.Math
Imports System.IO.Ports
Imports System.Security.Cryptography
Imports System.Windows.Forms.DataVisualization.Charting
Public Class Main
    Public Arduino As SerialPort
    Private SimulationMode As Boolean = False
    Private WithEvents tmrChrt As Timer = New Timer
    Private WithEvents tmrComponentLightStim As Timer = New Timer
    Private WithEvents tmrTimeSchedule As Timer = New Timer
    Private CompSequence As List(Of Integer)
    Private CompIndexSeq As Integer
    Private TimeScheduleList As New List(Of Integer)
    Private ObtainedDelayDurations(,) As List(Of Integer)
    Private DelayOnset(MAX_INPUTS - 1) As Integer
    Private DelayComp(MAX_INPUTS - 1) As Integer
    Private ScheduleTimers(MAX_INPUTS - 1) As Timer
    Private DelayTimers(MAX_INPUTS - 1) As Timer
    Private DelaySignalTimers(MAX_INPUTS - 1) As Timer
    Private DelaySignalStartTimers(MAX_INPUTS - 1) As Timer
    Private FeedbackTimers(MAX_INPUTS - 1) As Timer
    Private TPeriodTimers(MAX_INPUTS - 1) As Timer
    Private TInterPeriodTimers(MAX_INPUTS - 1) As Timer
    Private TCurrentPeriodIsD(MAX_INPUTS - 1) As Boolean
    Private TPeriodsRemaining(MAX_INPUTS - 1) As Integer
    Private TInInterPeriod(MAX_INPUTS - 1) As Boolean
    Private ComponentLightStimOn As Boolean
    Private ComponentToneStimOn As Boolean
    Private ChartSeriesConfigured As Boolean = False
    Private SessionTimeSeconds As Integer = 0
    Private LastDisplayedSecond As Integer = Integer.MinValue
    Private SessionStarted As Boolean = False
    Private SessionEnding As Boolean = False
    Private SessionClosed As Boolean = False
    Private PostSessionEndTick As Integer = 0
    Private LastPostSessionSecond As Integer = Integer.MinValue
    Private ReadOnly SessionRandom As Random = New Random(CreateRandomSeed())
    Private dgvMainSession As DataGridView
    Private dgvMainInputs As DataGridView
    Private pnlManualControls As FlowLayoutPanel

    Private Shared Function CreateRandomSeed() As Integer
        Dim bytes(3) As Byte
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(bytes)
        End Using
        Return BitConverter.ToInt32(bytes, 0)
    End Function

    Private Sub EnsureInputRuntime()
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            If ScheduleTimers(inputIndex) Is Nothing Then
                ScheduleTimers(inputIndex) = New Timer()
                AddHandler ScheduleTimers(inputIndex).Tick, AddressOf ScheduleTimer_Tick
            End If

            If DelayTimers(inputIndex) Is Nothing Then
                DelayTimers(inputIndex) = New Timer()
                DelayTimers(inputIndex).Interval = 1000
                AddHandler DelayTimers(inputIndex).Tick, AddressOf DelayTimer_Tick
            End If

            If DelaySignalTimers(inputIndex) Is Nothing Then
                DelaySignalTimers(inputIndex) = New Timer()
                AddHandler DelaySignalTimers(inputIndex).Tick, AddressOf DelaySignalTimer_Tick
            End If

            If DelaySignalStartTimers(inputIndex) Is Nothing Then
                DelaySignalStartTimers(inputIndex) = New Timer()
                AddHandler DelaySignalStartTimers(inputIndex).Tick, AddressOf DelaySignalStartTimer_Tick
            End If

            If FeedbackTimers(inputIndex) Is Nothing Then
                FeedbackTimers(inputIndex) = New Timer()
                FeedbackTimers(inputIndex).Interval = 1000
                AddHandler FeedbackTimers(inputIndex).Tick, AddressOf FeedbackTimer_Tick
            End If

            If TPeriodTimers(inputIndex) Is Nothing Then
                TPeriodTimers(inputIndex) = New Timer()
                AddHandler TPeriodTimers(inputIndex).Tick, AddressOf TPeriodTimer_Tick
            End If

            If TInterPeriodTimers(inputIndex) Is Nothing Then
                TInterPeriodTimers(inputIndex) = New Timer()
                AddHandler TInterPeriodTimers(inputIndex).Tick, AddressOf TInterPeriodTimer_Tick
            End If
        Next

        ConfigureMainLayout()
        EnsureChartSeries()
        EnsureInputStatusControls()
    End Sub

    Private Sub EnsureInputStatusControls()
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            EnsureSimulationButton(inputIndex)
            EnsureInputOutputButton(inputIndex)
            EnsureInputReinforcerButton(inputIndex)
        Next
    End Sub

    Private Sub EnsureSimulationButton(inputIndex As Integer)
        Dim controlName As String = "btnSimResponse" & (inputIndex + 1)
        If pnlManualControls.Controls.ContainsKey(controlName) Then Exit Sub

        Dim responseButton As New Button With {
            .Name = controlName,
            .Size = New Size(138, 28),
            .Tag = inputIndex,
            .Text = "Response " & (inputIndex + 1),
            .UseVisualStyleBackColor = True
        }

        AddHandler responseButton.Click, AddressOf SimResponseButton_Click
        pnlManualControls.Controls.Add(responseButton)
        responseButton.Visible = False
    End Sub

    Private Sub SimResponseButton_Click(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = CInt(DirectCast(sender, Button).Tag)
        If IsInputAvailable(inputIndex) Then Response(inputIndex)
    End Sub

    Private Sub InputOutputButton_Click(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = CInt(DirectCast(sender, Button).Tag)
        If IsInputAvailable(inputIndex) = False Then Exit Sub

        PalIO(inputIndex) = Not PalIO(inputIndex)
        SetInputOutput(inputIndex, PalIO(inputIndex))
        UpdateManualControls()
    End Sub

    Private Sub InputReinforcerButton_Click(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = CInt(DirectCast(sender, Button).Tag)
        If IsInputAvailable(inputIndex) Then Reinforce(inputIndex, True)
    End Sub

    Private Sub SetSimulationButton(inputIndex As Integer, enabled As Boolean)
        Dim controlName As String = "btnSimResponse" & (inputIndex + 1)
        If pnlManualControls Is Nothing OrElse pnlManualControls.Controls.ContainsKey(controlName) = False Then Exit Sub

        Dim responseButton As Button = DirectCast(pnlManualControls.Controls(controlName), Button)
        responseButton.Enabled = enabled
        responseButton.Visible = enabled
        responseButton.Text = If(enabled, "Response: " & InputLabel(inputIndex), "Response " & (inputIndex + 1))
    End Sub

    Private Sub SetManualButton(controlName As String, inputIndex As Integer, enabled As Boolean, textValue As String)
        If pnlManualControls Is Nothing OrElse pnlManualControls.Controls.ContainsKey(controlName) = False Then Exit Sub

        Dim manualButton As Button = DirectCast(pnlManualControls.Controls(controlName), Button)
        manualButton.Enabled = enabled
        manualButton.Visible = enabled
        manualButton.Text = textValue
    End Sub

    Private Sub EnsureInputOutputButton(inputIndex As Integer)
        Dim controlName As String = "btnInputOutput" & (inputIndex + 1)
        If pnlManualControls.Controls.ContainsKey(controlName) Then Exit Sub

        Dim ioButton As New Button With {
            .Name = controlName,
            .Size = New Size(138, 28),
            .Tag = inputIndex,
            .Text = "In/Out " & (inputIndex + 1),
            .UseVisualStyleBackColor = True
        }

        AddHandler ioButton.Click, AddressOf InputOutputButton_Click
        pnlManualControls.Controls.Add(ioButton)
        ioButton.Visible = False
    End Sub

    Private Sub EnsureInputReinforcerButton(inputIndex As Integer)
        Dim controlName As String = "btnInputReinforcer" & (inputIndex + 1)
        If pnlManualControls.Controls.ContainsKey(controlName) Then Exit Sub

        Dim reinforcerButton As New Button With {
            .Name = controlName,
            .Size = New Size(138, 28),
            .Tag = inputIndex,
            .Text = "Rf " & (inputIndex + 1),
            .UseVisualStyleBackColor = True
        }

        AddHandler reinforcerButton.Click, AddressOf InputReinforcerButton_Click
        pnlManualControls.Controls.Add(reinforcerButton)
        reinforcerButton.Visible = False
    End Sub

    Private Sub ConfigureMainLayout()
        If Me.ClientSize.Width < 1490 OrElse Me.ClientSize.Height < 820 Then
            Me.ClientSize = New Size(Math.Max(Me.ClientSize.Width, 1490), Math.Max(Me.ClientSize.Height, 820))
        End If

        Dim margin As Integer = 12
        Dim gap As Integer = 12
        Dim tableWidth As Integer = CInt((Me.ClientSize.Width - (margin * 2) - gap) / 2)
        Dim tableHeight As Integer = 220
        Dim chartTop As Integer = margin + tableHeight + gap
        Dim chartHeight As Integer = 430
        Dim controlsTop As Integer = chartTop + chartHeight + gap

        If dgvMainSession Is Nothing Then
            dgvMainSession = New DataGridView With {
                .Name = "dgvMainSession",
                .Location = New Point(margin, margin),
                .Size = New Size(tableWidth, tableHeight),
                .AllowUserToAddRows = False,
                .AllowUserToDeleteRows = False,
                .AllowUserToResizeRows = False,
                .ReadOnly = True,
                .RowHeadersVisible = False,
                .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            }
            dgvMainSession.Columns.Add("Property", "Property")
            dgvMainSession.Columns.Add("Value", "Value")
            Me.Controls.Add(dgvMainSession)
        End If
        dgvMainSession.Location = New Point(margin, margin)
        dgvMainSession.Size = New Size(tableWidth, tableHeight)

        If dgvMainInputs Is Nothing Then
            dgvMainInputs = New DataGridView With {
                .Name = "dgvMainInputs",
                .Location = New Point(margin + tableWidth + gap, margin),
                .Size = New Size(tableWidth, tableHeight),
                .AllowUserToAddRows = False,
                .AllowUserToDeleteRows = False,
                .AllowUserToResizeRows = False,
                .ReadOnly = True,
                .RowHeadersVisible = False,
                .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            }
            dgvMainInputs.Columns.Add("Input", "Input")
            dgvMainInputs.Columns.Add("Schedule", "Schedule")
            dgvMainInputs.Columns.Add("RfReady", "Rf ready")
            dgvMainInputs.Columns.Add("Responses", "Resp")
            dgvMainInputs.Columns.Add("CODResponses", "COD")
            dgvMainInputs.Columns.Add("Reinforcers", "Rf")
            Me.Controls.Add(dgvMainInputs)
        End If
        dgvMainInputs.Location = New Point(margin + tableWidth + gap, margin)
        dgvMainInputs.Size = New Size(tableWidth, tableHeight)

        If pnlManualControls Is Nothing Then
            pnlManualControls = New FlowLayoutPanel With {
                .Name = "pnlManualControls",
                .Location = New Point(margin, controlsTop),
                .Size = New Size(Me.ClientSize.Width - (margin * 2), 112),
                .AutoScroll = True,
                .WrapContents = True,
                .Visible = False
            }
            Me.Controls.Add(pnlManualControls)
        End If
        pnlManualControls.Location = New Point(margin, controlsTop)
        pnlManualControls.Size = New Size(Me.ClientSize.Width - (margin * 2), 112)

        Chart1.Location = New Point(margin, chartTop)
        Chart1.Size = New Size(Me.ClientSize.Width - (margin * 2), chartHeight)

        If btnFinish.Parent IsNot pnlManualControls Then
            Me.Controls.Remove(btnFinish)
            pnlManualControls.Controls.Add(btnFinish)
        End If
        btnFinish.Size = New Size(138, 28)
        btnFinish.Margin = New Padding(3)
        btnFinish.Font = New Font("Microsoft Sans Serif", 8.25!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
        btnFinish.BackColor = Color.Firebrick
        btnFinish.ForeColor = Color.White
        btnFinish.FlatStyle = FlatStyle.Flat
        btnFinish.UseVisualStyleBackColor = False
        btnFinish.Visible = SessionStarted AndAlso SessionEnding = False
    End Sub

    Private Sub EnsureChartSeries()
        If ChartSeriesConfigured Then Exit Sub

        Chart1.Series.Clear()
        If Chart1.ChartAreas.Count = 0 Then Chart1.ChartAreas.Add("ChartArea1")
        Dim chartArea As ChartArea = Chart1.ChartAreas(0)
        chartArea.AxisX.MajorGrid.Enabled = False
        chartArea.AxisX.MinorGrid.Enabled = False
        chartArea.AxisY.MajorGrid.Enabled = False
        chartArea.AxisY.MinorGrid.Enabled = False
        chartArea.AxisX.Title = "Time"
        chartArea.AxisY.Title = "Responses"
        chartArea.AxisX.StripLines.Clear()
        If Chart1.Legends.Count = 0 Then Chart1.Legends.Add("Legend1")
        Chart1.Legends(0).Docking = Docking.Right

        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Dim inputSeries As String = ResponseSeriesName(inputIndex)
            If Chart1.Series.IndexOf(inputSeries) < 0 Then
                Dim series As New Series(inputSeries)
                series.ChartArea = "ChartArea1"
                series.ChartType = SeriesChartType.StepLine
                series.Legend = "Legend1"
                series.BorderWidth = 2
                series.IsVisibleInLegend = False
                Chart1.Series.Add(series)
            End If

            Dim reinforcerSeries As String = ReinforcerSeriesName(inputIndex)
            If Chart1.Series.IndexOf(reinforcerSeries) < 0 Then
                Dim series As New Series(reinforcerSeries)
                series.ChartArea = "ChartArea1"
                series.ChartType = SeriesChartType.Point
                series.Legend = "Legend1"
                series.MarkerStyle = MarkerStyle.Cross
                series.MarkerSize = 8
                series.IsVisibleInLegend = False
                Chart1.Series.Add(series)
            End If
        Next

        Dim componentColors() As Color = {
            Color.FromArgb(45, Color.Blue),
            Color.FromArgb(45, Color.Red),
            Color.FromArgb(55, Color.Goldenrod),
            Color.FromArgb(45, Color.Green),
            Color.FromArgb(45, Color.DarkViolet),
            Color.FromArgb(45, Color.Teal),
            Color.FromArgb(45, Color.OrangeRed),
            Color.FromArgb(45, Color.SlateGray),
            Color.FromArgb(45, Color.DeepPink),
            Color.FromArgb(45, Color.DarkCyan)
        }

        For componentIndex As Integer = 1 To MAX_COMPONENTS
            Dim componentSeries As String = ComponentSeriesName(componentIndex)
            Dim series As New Series(componentSeries)
            series.ChartArea = "ChartArea1"
            series.ChartType = SeriesChartType.Area
            series.Legend = "Legend1"
            series.BorderWidth = 1
            series.Color = componentColors((componentIndex - 1) Mod componentColors.Length)
            series.IsVisibleInLegend = False
            Chart1.Series.Add(series)
        Next

        ChartSeriesConfigured = True
    End Sub

    Private Function ResponseSeriesName(inputIndex As Integer) As String
        Return "ResponseInput" & (inputIndex + 1)
    End Function

    Private Function ReinforcerSeriesName(inputIndex As Integer) As String
        Return "ReinforcerInput" & (inputIndex + 1)
    End Function

    Private Function ComponentSeriesName(componentIndex As Integer) As String
        Return "ComponentSeries" & componentIndex
    End Function

    Private Sub ClearComponentEndMarkers()
        If Chart1.ChartAreas.Count = 0 Then Exit Sub
        Chart1.ChartAreas(0).AxisX.StripLines.Clear()
    End Sub

    Private Sub AddComponentEndMarker()
        If Chart1.ChartAreas.Count = 0 Then Exit Sub

        Dim marker As New StripLine()
        marker.IntervalOffset = chartTime(MAX_INPUTS)
        marker.StripWidth = 0
        marker.BorderColor = Color.DimGray
        marker.BorderWidth = 1
        marker.BorderDashStyle = ChartDashStyle.Dash
        marker.Text = ""

        Chart1.ChartAreas(0).AxisX.StripLines.Add(marker)
    End Sub

    Private Function ActiveInputCount(Optional componentIndex As Integer = -1) As Integer
        If componentIndex < 0 Then componentIndex = vCC
        If AC(componentIndex).InputCount <= 0 Then Return 2
        Return Math.Min(AC(componentIndex).InputCount, MAX_INPUTS)
    End Function

    Private Function InputLabel(inputIndex As Integer, Optional componentIndex As Integer = -1) As String
        If componentIndex < 0 Then componentIndex = vCC
        If AC(componentIndex).InputName IsNot Nothing AndAlso
           inputIndex <= AC(componentIndex).InputName.Length - 1 AndAlso
           AC(componentIndex).InputName(inputIndex) <> "" Then
            Return AC(componentIndex).InputName(inputIndex)
        End If

        Return "Input " & (inputIndex + 1)
    End Function

    Private Function ComponentLabel(componentIndex As Integer) As String
        If AC(componentIndex).ComponentName <> "" Then Return AC(componentIndex).ComponentName
        Return "Component " & componentIndex
    End Function

    Private Function ScheduleText(inputIndex As Integer) As String
        If AC(vCC).ScheduleType(inputIndex) = "N/A" Then Return "N/A"
        If AC(vCC).ScheduleType(inputIndex) = "Extinction" Then Return "Extinction"
        If AC(vCC).ScheduleType(inputIndex) = "T Schedule" Then
            Return "T Schedule TD " & AC(vCC).TDurationD(inputIndex) & " s / TΔ " &
                AC(vCC).TDurationDelta(inputIndex) & " s / P " &
                AC(vCC).TProbabilityD(inputIndex) & "%|" & AC(vCC).TProbabilityDelta(inputIndex) & "%"
        End If
        Return AC(vCC).ScheduleType(inputIndex) & " " & AC(vCC).ScheduleValue(inputIndex)
    End Function

    Private Function NormalizedScheduleType(inputIndex As Integer) As String
        If AC(vCC).ScheduleType Is Nothing OrElse inputIndex < 0 OrElse inputIndex > AC(vCC).ScheduleType.Length - 1 Then Return ""
        Return If(AC(vCC).ScheduleType(inputIndex), "").Trim().ToLowerInvariant()
    End Function

    Private Function IsRatioSchedule(inputIndex As Integer) As Boolean
        Dim scheduleType As String = NormalizedScheduleType(inputIndex)
        Return scheduleType = "fixed ratio" OrElse scheduleType = "variable ratio"
    End Function

    Private Function IsIntervalSchedule(inputIndex As Integer) As Boolean
        Dim scheduleType As String = NormalizedScheduleType(inputIndex)
        Return scheduleType = "fixed interval" OrElse scheduleType.Contains("variable interval")
    End Function

    Private Function IsTSchedule(inputIndex As Integer) As Boolean
        Return NormalizedScheduleType(inputIndex) = "t schedule"
    End Function

    Private Function IsExtinctionSchedule(inputIndex As Integer) As Boolean
        Return NormalizedScheduleType(inputIndex) = "extinction"
    End Function

    Private Function IsUnavailableSchedule(inputIndex As Integer) As Boolean
        Return NormalizedScheduleType(inputIndex) = "n/a"
    End Function

    Private Function IsInputAvailable(inputIndex As Integer) As Boolean
        Return inputIndex < ActiveInputCount() AndAlso IsUnavailableSchedule(inputIndex) = False
    End Function

    Private Sub WriteRawEvent(eventCode As String)
        WriteLine(1, vTimeNow, vCC, eventCode)
    End Sub

    Private Function HasAnyObtainedDelay() As Boolean
        If ObtainedDelayDurations Is Nothing Then Return False

        For componentIndex As Integer = 1 To MAXvCC
            For inputIndex As Integer = 0 To ActiveInputCount(componentIndex) - 1
                If ObtainedDelayDurations(componentIndex, inputIndex) IsNot Nothing AndAlso
                   ObtainedDelayDurations(componentIndex, inputIndex).Count > 0 Then
                    Return True
                End If
            Next
        Next

        Return False
    End Function

    Private Function ProbabilityHit(probability As Integer) As Boolean
        If probability <= 0 Then Return False
        If probability >= 100 Then Return True
        Return SessionRandom.Next(1, 101) <= probability
    End Function

    Private Function ComponentRefTotal() As Integer
        Dim componentRefs As Integer = TimeRefCount_i
        For inputIndex As Integer = 0 To ActiveInputCount() - 1
            componentRefs += RefCount_i(inputIndex)
        Next
        Return componentRefs
    End Function

    Private Function ComponentStimText() As String
        If AC(vCC).ComponentStimType Is Nothing OrElse AC(vCC).ComponentStimType = "" OrElse AC(vCC).ComponentStimType = "None" Then Return "None"

        Dim details As New List(Of String)
        details.Add(AC(vCC).ComponentStimType)
        If AC(vCC).ComponentStimType.Contains("Light") Then details.Add("Light int: " & AC(vCC).ComponentLightIntermittency & " s")
        If AC(vCC).ComponentStimType.Contains("Tone") Then details.Add("Tone int: " & AC(vCC).ComponentStimDuration & " s")
        Return String.Join(" / ", details)
    End Function

    Private Sub AddMainSessionRow(label As String, value As String)
        Dim rowIndex As Integer = dgvMainSession.Rows.Add()
        dgvMainSession.Rows(rowIndex).Cells(0).Value = label
        dgvMainSession.Rows(rowIndex).Cells(1).Value = value
    End Sub

    Private Sub RefreshMainSessionTable(Optional statusText As String = "")
        If dgvMainSession Is Nothing Then Exit Sub

        dgvMainSession.Rows.Clear()
        AddMainSessionRow("Subject", SetUp.txtSubject.Text)
        AddMainSessionRow("Session", SetUp.txtSession.Text)
        AddMainSessionRow("COM / mode", SetUp.txtCOM.Text & If(SimulationMode, " / Simulation", ""))
        AddMainSessionRow("Time (s)", CStr(SessionTimeSeconds))
        AddMainSessionRow("Current component", If(statusText <> "", statusText, ComponentLabel(vCC)))
        AddMainSessionRow("Duration (s)", CStr(AC(vCC).ComponentDuration))
        AddMainSessionRow("Iterations left", CStr(AC(vCC).IterationsLeft))
        AddMainSessionRow("Stimulus", ComponentStimText())
    End Sub

    Private Sub RefreshInputStatusTable()
        If dgvMainInputs Is Nothing Then Exit Sub

        dgvMainInputs.Rows.Clear()
        For inputIndex As Integer = 0 To ActiveInputCount() - 1
            dgvMainInputs.Rows.Add(
                InputLabel(inputIndex),
                ScheduleText(inputIndex),
                ScheduleStatusText(inputIndex),
                CStr(ResponseCount(vCC, inputIndex)),
                CStr(ResponseCountDel(vCC, inputIndex)),
                CStr(RefCount(vCC, inputIndex)))
        Next
    End Sub

    Private Function ScheduleStatusText(inputIndex As Integer) As String
        If IsRatioSchedule(inputIndex) Then Return "N/A"
        If IsTSchedule(inputIndex) Then
            If TInInterPeriod(inputIndex) Then Return "Between periods"
            If TPeriodTimers(inputIndex) IsNot Nothing AndAlso TPeriodTimers(inputIndex).Enabled Then Return If(TCurrentPeriodIsD(inputIndex), "TD", "TΔ")
            Return "Off"
        End If

        Return CStr(refRdy(inputIndex))
    End Function

    Private Sub RefreshMainTables(Optional statusText As String = "")
        RefreshMainSessionTable(statusText)
        RefreshInputStatusTable()
        UpdateManualControls()
        RefreshChartLegend()
    End Sub

    Private Sub RefreshChartLegend()
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Dim active As Boolean = IsInputAvailable(inputIndex)
            If Chart1.Series.IndexOf(ResponseSeriesName(inputIndex)) >= 0 Then
                Chart1.Series(ResponseSeriesName(inputIndex)).LegendText = If(active, InputLabel(inputIndex) & " responses", "")
                Chart1.Series(ResponseSeriesName(inputIndex)).IsVisibleInLegend = active
            End If

            If Chart1.Series.IndexOf(ReinforcerSeriesName(inputIndex)) >= 0 Then
                Chart1.Series(ReinforcerSeriesName(inputIndex)).LegendText = If(active, InputLabel(inputIndex) & " reinforcers", "")
                Chart1.Series(ReinforcerSeriesName(inputIndex)).IsVisibleInLegend = active AndAlso AC(vCC).Magnitude(inputIndex) > 0
            End If
        Next

        For componentIndex As Integer = 1 To MAX_COMPONENTS
            If Chart1.Series.IndexOf(ComponentSeriesName(componentIndex)) >= 0 Then
                Chart1.Series(ComponentSeriesName(componentIndex)).LegendText = ComponentLabel(componentIndex)
                Chart1.Series(ComponentSeriesName(componentIndex)).IsVisibleInLegend = componentIndex <= MAXvCC
            End If
        Next
    End Sub

    Private Sub UpdateManualControls()
        If pnlManualControls IsNot Nothing Then pnlManualControls.Visible = SessionStarted AndAlso SessionEnding = False
        btnFinish.Visible = SessionStarted AndAlso SessionEnding = False

        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Dim active As Boolean = inputIndex < ActiveInputCount()
            Dim controlsEnabled As Boolean = SessionStarted AndAlso SessionEnding = False AndAlso active
            SetSimulationButton(inputIndex, controlsEnabled)
            SetManualButton("btnInputOutput" & (inputIndex + 1), inputIndex, controlsEnabled, If(active, If(PalIO(inputIndex), "Retract: ", "Insert: ") & InputLabel(inputIndex), "In/Out " & (inputIndex + 1)))
            SetManualButton("btnInputReinforcer" & (inputIndex + 1), inputIndex, controlsEnabled AndAlso AC(vCC).Magnitude(inputIndex) > 0, If(active, "Rf: " & InputLabel(inputIndex), "Rf " & (inputIndex + 1)))
        Next
    End Sub

    Private Function InputTimerIndex(timers() As Timer, sender As Object) As Integer
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            If Object.ReferenceEquals(timers(inputIndex), sender) Then Return inputIndex
        Next
        Return -1
    End Function

    Private Function AnyDelayActive() As Boolean
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            If DelayTimers(inputIndex) IsNot Nothing AndAlso DelayTimers(inputIndex).Enabled Then Return True
        Next
        Return False
    End Function

    Private Function FirstActiveDelayIndex() As Integer
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            If DelayTimers(inputIndex) IsNot Nothing AndAlso DelayTimers(inputIndex).Enabled Then Return inputIndex
        Next
        Return -1
    End Function

    Private Sub SendArduino(command As String)
        If SimulationMode Then Exit Sub
        If Arduino Is Nothing OrElse Arduino.IsOpen = False Then Exit Sub
        Arduino.WriteLine(command)
    End Sub

    Private Function OpenArduinoOrSimulation() As Boolean
        Try
            Arduino = New SerialPort(SetUp.txtCOM.Text, 9600)
            Arduino.Open()
            SimulationMode = False
            Return True
        Catch ex As Exception
            Dim result As DialogResult = MessageBox.Show(
                "No Arduino was found on " & SetUp.txtCOM.Text & "." & Environment.NewLine &
                "If you continue, the session will run in simulation mode without hardware input/output." & Environment.NewLine &
                "Choose Cancel to close this session window.",
                "Arduino not found",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning)

            If result = DialogResult.OK Then
                SimulationMode = True
                Arduino = Nothing
                Return True
            End If

            Return False
        End Try
    End Function

    Private Sub SetInputOutput(inputIndex As Integer, available As Boolean)
        Dim onCommands() As String = {"L", "M", "C", "D", "N", "O"}
        Dim offCommands() As String = {"l", "m", "c", "d", "n", "o"}

        If inputIndex < 0 OrElse inputIndex > onCommands.Length - 1 Then Exit Sub
        SendArduino(If(available, onCommands(inputIndex), offCommands(inputIndex)))
    End Sub

    Private Sub TurnOffAllInputs()
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            SetInputOutput(inputIndex, False)
            PalIO(inputIndex) = False
        Next
    End Sub

    Private Sub RestoreRetractedInput(inputIndex As Integer)
        If AC(vCC).DelayRetract(inputIndex) AndAlso IsInputAvailable(inputIndex) Then SetInputOutput(inputIndex, True)
    End Sub

    Private Sub StartComponentStimulus()
        tmrComponentStim.Enabled = False
        tmrComponentLightStim.Enabled = False
        ComponentLightStimOn = False
        ComponentToneStimOn = False
        StimInt = False

        If AC(vCC).ComponentStimType IsNot Nothing AndAlso AC(vCC).ComponentStimType.Contains("Light") Then
            If AC(vCC).ComponentLightIntermittency > 0 Then
                ComponentLightStimOn = True
                ActivateStimulus(AC(vCC).ComponentStimType, True, False)
                tmrComponentLightStim.Interval = Math.Max(1, CInt(AC(vCC).ComponentLightIntermittency * 1000))
                tmrComponentLightStim.Enabled = True
            Else
                ActivateStimulus(AC(vCC).ComponentStimType, True, False)
            End If
        End If

        If AC(vCC).ComponentStimType IsNot Nothing AndAlso AC(vCC).ComponentStimType.Contains("Tone") Then
            If AC(vCC).ComponentStimDuration > 0 Then
                ComponentToneStimOn = True
                ActivateStimulus("Tone", True)
                tmrComponentStim.Interval = Math.Max(1, CInt(AC(vCC).ComponentStimDuration * 1000))
                tmrComponentStim.Enabled = True
            Else
                ActivateStimulus("Tone", True)
            End If
        End If
    End Sub

    Private Sub StopComponentStimulus()
        tmrComponentStim.Enabled = False
        tmrComponentLightStim.Enabled = False
        ComponentLightStimOn = False
        ComponentToneStimOn = False
        ActivateStimulus(AC(vCC).ComponentStimType, False)
    End Sub


    Function ArduinoVB() As Boolean 'This function starts the Arduino-VB communication.
        EnsureInputRuntime()
        If OpenArduinoOrSimulation() = False Then
            Me.Close()
            Return False
        End If

        SendArduino("p")
        SessionStarted = False
        SessionEnding = False
        SessionClosed = False
        LastDisplayedSecond = Integer.MinValue
        LastPostSessionSecond = Integer.MinValue
        RefreshMainTables("Starting")
        tmrStart.Interval = Max(1, SetUp.txbStart.Text * 1000)
        If SetUp.ICIDurationSeconds() > 0 Then tmrICI.Interval = SetUp.ICIDurationSeconds() * 1000
        Countdown = Environment.TickCount + SetUp.txbStart.Text * 1000
        tmrStart.Enabled = True

        Do While SessionClosed = False 'This code will run throughout the session to allow response collection.
            Try
                If SimulationMode = False AndAlso Arduino IsNot Nothing AndAlso Arduino.IsOpen AndAlso Arduino.BytesToRead > 0 Then 'Checks for activity on the Arduino.
                    Dim readValues() As String = Split(Arduino.ReadLine(), ",")
                    For inputIndex As Integer = 0 To MAX_INPUTS - 1
                        If inputIndex <= readValues.Length - 1 Then
                            Actual_Response(inputIndex) = readValues(inputIndex)
                        End If
                    Next
                End If

                For inputIndex As Integer = 0 To MAX_INPUTS - 1
                    If Actual_Response(inputIndex) <> Previous_Response(inputIndex) AndAlso Actual_Response(inputIndex) <> "1" Then
                        If IsInputAvailable(inputIndex) Then
                            Response(inputIndex)
                        End If
                    End If
                    Previous_Response(inputIndex) = Actual_Response(inputIndex)
                Next
                If tmrStart.Enabled = False Then vTimeNow = Environment.TickCount - vTimeStart  'This keeps track of time for the Data output file.
                If tmrStart.Enabled = True Then vTimeNow = (Countdown) - Environment.TickCount
                SessionTimeSeconds = CInt(Round(vTimeNow / 1000))
                If SessionTimeSeconds < 0 AndAlso tmrStart.Enabled Then SessionTimeSeconds = 0

                If tmrStart.Enabled AndAlso SessionTimeSeconds <> LastDisplayedSecond Then
                    LastDisplayedSecond = SessionTimeSeconds
                    RefreshMainSessionTable("Starting in " & SessionTimeSeconds & " s")
                End If

                If SessionEnding AndAlso tmrPostSession.Enabled Then
                    Dim remainingPostSeconds As Integer = CInt(Math.Ceiling((PostSessionEndTick - Environment.TickCount) / 1000.0))
                    If remainingPostSeconds < 0 Then remainingPostSeconds = 0

                    If remainingPostSeconds <> LastPostSessionSecond Then
                        LastPostSessionSecond = remainingPostSeconds
                        RefreshMainSessionTable("Closing in " & remainingPostSeconds & " s")
                    End If
                End If



            Catch ex As Exception
            End Try
            My.Application.DoEvents() 'This will enable the rest of the program to run while executing the code from above.
        Loop
        Return True
    End Function


    Private Sub tmrStart_Tick(sender As Object, e As EventArgs) Handles tmrStart.Tick
        tmrStart.Enabled = False
        SessionStarted = True
        SessionEnding = False
        LastDisplayedSecond = Integer.MinValue

        WriteLine(2, "Components presented at random: " & CStr(RandomCPres))


        ' =========================================================
        ' Generate component sequence when using manual or random order
        ' =========================================================
        Dim manualSequence As List(Of Integer) = SetUp.ManualComponentSequence()
        If manualSequence IsNot Nothing Then
            CompSequence = manualSequence
            CompIndexSeq = 0
            WriteLine(2, "Manual component sequence: " & String.Join(",", CompSequence))

        ElseIf RandomCPres = True Then

            Dim validSequence As Boolean = False
            Dim maxTries As Integer = 500
            Dim tries As Integer = 0

            Do While Not validSequence AndAlso tries < maxTries
                tries += 1
                validSequence = True
                CompSequence = New List(Of Integer)

                ' Build base component list according to programmed iterations
                For i = 1 To MAXvCC
                    For l = 1 To AC(i).ComponentIteration
                        CompSequence.Add(i)
                    Next
                Next

                ' Shuffle component list
                For i As Integer = CompSequence.Count - 1 To 1 Step -1
                    Dim j As Integer = SessionRandom.Next(0, i + 1)
                    Dim tmp = CompSequence(i)
                    CompSequence(i) = CompSequence(j)
                    CompSequence(j) = tmp
                Next

                ' Check restriction: no three identical components in sequence
                For i As Integer = 2 To CompSequence.Count - 1
                    If CompSequence(i) = CompSequence(i - 1) AndAlso
                   CompSequence(i - 1) = CompSequence(i - 2) Then
                        validSequence = False
                        Exit For
                    End If
                Next
            Loop

            If Not validSequence Then
                WriteLine(2, "WARNING: Could not avoid 3 consecutive components; using last shuffle.")
            Else
                WriteLine(2, "Restricted component sequence: " & String.Join(",", CompSequence))
            End If

            ' Reset sequence index
            CompIndexSeq = 0

        Else
            ' No random presentation: sequence not used
            CompSequence = Nothing
            CompIndexSeq = 0
        End If


        ' =========================================================
        ' Initialize session timing and visual settings
        ' =========================================================
        vTimeStart = Environment.TickCount
        ClearComponentEndMarkers()

        chartResponse(MAX_INPUTS) += 10
        RefreshChartLegend()




        ' =========================================================
        ' Initialize obtained delays containers 
        ' =========================================================
        ReDim ObtainedDelayDurations(MAXvCC, MAX_INPUTS - 1)
        For c As Integer = 1 To MAXvCC
            For l As Integer = 0 To MAX_INPUTS - 1
                ObtainedDelayDurations(c, l) = New List(Of Integer)
            Next
        Next

        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            DelayOnset(inputIndex) = -1
            DelayComp(inputIndex) = -1
        Next


        ' Start first component
        BeginPrograms()

    End Sub


    Private Sub BeginPrograms() 'Call this at the start of each component

        ' =========================================================
        ' Select component
        ' =========================================================
        If CompSequence IsNot Nothing Then
            vCC = CompSequence(CompIndexSeq)
            CompIndexSeq += 1
        End If

        If AC(vCC).IterationsLeft > 0 Then AC(vCC).IterationsLeft -= 1

        ' =========================================================
        ' Reset timers and outputs
        ' =========================================================
        tmrCOD.Enabled = False
        tmrTimeSchedule.Enabled = False
        CODL = 0
        SendArduino("abeft")
        TurnOffAllInputs()
        StartComponentStimulus()

        If AC(vCC).COD > 0 Then tmrCOD.Interval = AC(vCC).COD

        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            ScheduleTimers(inputIndex).Enabled = False
            DelayTimers(inputIndex).Enabled = False
            FeedbackTimers(inputIndex).Enabled = False
            DelaySignalTimers(inputIndex).Enabled = False
            DelaySignalStartTimers(inputIndex).Enabled = False
            TPeriodTimers(inputIndex).Enabled = False
            TInterPeriodTimers(inputIndex).Enabled = False
            TInInterPeriod(inputIndex) = False
            TPeriodsRemaining(inputIndex) = 0
        Next

        ' Reset current delay onset markers for the new component
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            DelayOnset(inputIndex) = -1
            DelayComp(inputIndex) = -1
        Next

        WriteRawEvent("StartComponent" & vCC)

        tmrComponentDuration.Interval = Math.Max(1, CInt(AC(vCC).ComponentDuration * 1000))
        AC(vCC).ComponentDuration_measured(AC(vCC).IterationsLeft) = SessionTimeSeconds
        tmrComponentDuration.Enabled = True

        ' =========================================================
        ' Reset per-component data
        ' =========================================================
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            VIList(inputIndex) = New List(Of Integer)
        Next
        TimeScheduleList = New List(Of Integer)

        ' -----------------------------
        ' IMPORTANT CHANGE:
        ' Do NOT reset ObtainedDelays here (or you lose session history).
        ' ObtainedDelayDurations(component, lever) already accumulates per component.
        ' -----------------------------
        ' ObtainedDelays(0) = New List(Of Integer)
        ' ObtainedDelays(1) = New List(Of Integer)

        If AC(vCC).HouselightOnOff = True Then SendArduino("H")
        If AC(vCC).HouselightOnOff = False Then SendArduino("h")

        For inputIndex As Integer = 0 To ActiveInputCount() - 1
            If AC(vCC).DelayDuration(inputIndex) > 0 Then DelayTimers(inputIndex).Interval = Math.Max(1, CInt(AC(vCC).DelayDuration(inputIndex) * 1000))
            If AC(vCC).DelaySignalDuration(inputIndex) > 0 Then DelaySignalTimers(inputIndex).Interval = Math.Max(1, CInt(AC(vCC).DelaySignalDuration(inputIndex) * 1000))
            If AC(vCC).FeedbackDuration(inputIndex) > 0 Then FeedbackTimers(inputIndex).Interval = Math.Max(1, CInt(AC(vCC).FeedbackDuration(inputIndex) * 1000))

            If IsExtinctionSchedule(inputIndex) = False AndAlso IsUnavailableSchedule(inputIndex) = False AndAlso AC(vCC).Reinforcer(inputIndex) <> "N/A" AndAlso AC(vCC).Magnitude(inputIndex) <= 0 Then
                AC(vCC).Magnitude(inputIndex) = 1
            End If

            SetInputOutput(inputIndex, NormalizedScheduleType(inputIndex) <> "" AndAlso IsExtinctionSchedule(inputIndex) = False AndAlso IsUnavailableSchedule(inputIndex) = False)
            PalIO(inputIndex) = NormalizedScheduleType(inputIndex) <> "" AndAlso IsExtinctionSchedule(inputIndex) = False AndAlso IsUnavailableSchedule(inputIndex) = False
        Next

        ' =========================================================
        ' Reset ratio counters
        ' =========================================================
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            RatioGoal(inputIndex) = 0
            RatioCount(inputIndex) = 0
            refRdy(inputIndex) = False
        Next
        TimeRefCount_i = 0
        ' =========================================================
        ' Initialize schedules
        ' =========================================================
        For inputIndex As Integer = 0 To ActiveInputCount() - 1
            InitializeSchedule(inputIndex)
            If IsTSchedule(inputIndex) Then InitializeTSchedule(inputIndex)
        Next
        InitializeTimeSchedule()

        ' =========================================================
        ' Update schedule logging
        ' =========================================================
        For inputIndex As Integer = 0 To ActiveInputCount() - 1
            SetSimulationButton(inputIndex, IsInputAvailable(inputIndex))

            Dim scheduleLogText As String = InputLabel(inputIndex) & " Schedule: " & ScheduleText(inputIndex)
            WriteLine(2, scheduleLogText)
        Next

        For inputIndex As Integer = ActiveInputCount() To MAX_INPUTS - 1
            SetSimulationButton(inputIndex, False)
        Next

        RefreshMainTables()

        tmrChrt.Interval = 1000
        tmrChrt.Enabled = True

    End Sub


    Private Sub Response(Lever As Integer)
        ' Registers a response and evaluates whether a reinforcer is available
        ' for ratio- or interval-based schedules.

        If SessionStarted AndAlso SessionEnding = False AndAlso tmrStart.Enabled = False AndAlso IsInputAvailable(Lever) Then

            ' Increment real-time response counter for plotting
            chartResponse(Lever) += 1

            ' ---------------------------------------------------------
            ' Responses during the inter-component interval (ICI)
            ' ---------------------------------------------------------
            If tmrICI.Enabled = True Then
                WriteRawEvent("ICIResponse" & (Lever + 1))
                ' Responses during ICI are logged but do not affect schedules

            Else
                If tmrCOD.Enabled Then
                    ResponseCount(vCC, Lever) += 1
                    RefreshInputStatusTable()
                    WriteRawEvent("CODResponse" & (Lever + 1))
                    Exit Sub
                End If

                ' Deliver programmed feedback stimulus (if any)
                If AC(vCC).FeedbackDuration(Lever) > 0 Then Stimulus(Lever)

                ' ---------------------------------------------------------
                ' Extinction 
                ' ---------------------------------------------------------
                If IsExtinctionSchedule(Lever) Then

                    ResponseCount(vCC, Lever) += 1
                    RefreshInputStatusTable()
                    WriteRawEvent("E" & (Lever + 1))

                Else
                    ' ---------------------------------------------------------
                    ' Responses outside reinforcement delay
                    ' ---------------------------------------------------------
                    If AnyDelayActive() = False Then

                        WriteRawEvent(CStr(Lever + 1))
                        ResponseCount(vCC, Lever) += 1
                        RefreshInputStatusTable()

                        If IsRatioSchedule(Lever) Then
                            Ratio(Lever)
                        ElseIf IsIntervalSchedule(Lever) AndAlso refRdy(Lever) = True Then
                            Reinforce(Lever, False)
                        ElseIf IsTSchedule(Lever) Then
                            TScheduleResponse(Lever)
                        End If

                        ' ---------------------------------------------------------
                        ' Responses during reinforcement delay
                        ' ---------------------------------------------------------
                    Else
                        Dim activeDelay As Integer = FirstActiveDelayIndex()
                        WriteRawEvent("D" & (activeDelay + 1))
                        ResponseCountDel(vCC, Lever) += 1
                        RefreshInputStatusTable()
                    End If
                End If

                If AC(vCC).COD > 0 Then
                    tmrCOD.Interval = Math.Max(1, CInt(AC(vCC).COD))
                    tmrCOD.Enabled = True
                End If
            End If
        End If
    End Sub


    Private Sub Stimulus(Lever)
        ActivateStimulus(AC(vCC).FeedbackType(Lever), True)

        If AC(vCC).FeedbackType(Lever).Contains("Time Out") = True Then
            tmrComponentStim.Enabled = False
            tmrComponentLightStim.Enabled = False
            SendArduino("abefhtlmcdno")
        End If

        FeedbackTimers(Lever).Enabled = True
    End Sub

    Private Sub ActivateStimulus(stimulusType As String, turnOn As Boolean, Optional includeTone As Boolean = True)
        If stimulusType Is Nothing Then Exit Sub

        If stimulusType.Contains("Light 1") = True Then SendArduino(If(turnOn, "A", "a"))
        If stimulusType.Contains("Light 2") = True Then SendArduino(If(turnOn, "B", "b"))
        If stimulusType.Contains("Light 3") = True Then SendArduino(If(turnOn, "E", "e"))
        If stimulusType.Contains("Light 4") = True Then SendArduino(If(turnOn, "F", "f"))
        If includeTone AndAlso stimulusType.Contains("Tone") = True Then SendArduino(If(turnOn, "T", "t"))
        If stimulusType.Contains("Houselight") = True Then SendArduino(If(turnOn, "H", "h"))
    End Sub

    Private Sub Ratio(Lever As Integer)

        ' Counts responses under ratio schedules and delivers the reinforcer
        ' when the fixed or variable response requirement is met.

        If RatioGoal(Lever) <> 0 Then
            RatioCount(Lever) += 1
            If RatioCount(Lever) >= RatioGoal(Lever) Then
                RatioCount(Lever) = 0
                Reinforce(Lever, False)
                RefreshInputStatusTable()
            End If
        End If
    End Sub

    Private Sub Reinforce(Lever As Integer, Delay As Boolean)

        If Lever >= 0 AndAlso Lever < MAX_INPUTS AndAlso AC(vCC).DelayDuration(Lever) > 0 AndAlso Delay = False Then
            DelayTimers(Lever).Enabled = True
            If AC(vCC).DelayRetract(Lever) = True Then SetInputOutput(Lever, False)

            DelayOnset(Lever) = vTimeNow
            DelayComp(Lever) = vCC

            DelaySignalStartTimers(Lever).Enabled = False
            DelaySignalTimers(Lever).Enabled = False

            If AC(vCC).DelayType(Lever) <> "Unsignaled" AndAlso AC(vCC).DelayType(Lever) <> "None" Then
                If AC(vCC).DelaySignalDuration(Lever) > 0 AndAlso AC(vCC).DelaySignalDuration(Lever) < AC(vCC).DelayDuration(Lever) Then
                    If DelaySignalAtEnd(Lever) Then
                        DelaySignalStartTimers(Lever).Interval = Math.Max(1, CInt((AC(vCC).DelayDuration(Lever) - AC(vCC).DelaySignalDuration(Lever)) * 1000))
                        DelaySignalStartTimers(Lever).Enabled = True
                    Else
                        ActivateStimulus(AC(vCC).DelayType(Lever), True)
                        DelaySignalTimers(Lever).Enabled = True
                    End If
                Else
                    ActivateStimulus(AC(vCC).DelayType(Lever), True)
                End If
            End If

        ElseIf Lever = -1 Then
            SendArduino("R")

            ' Deliver reinforcer now (immediate OR end-of-delay)
        Else
            refRdy(Lever) = False

            Dim deliveredCount As Integer = 0
            For i = 1 To AC(vCC).Magnitude(Lever)
                If ReinforcerDelivery(AC(vCC).Reinforcer(Lever), If(AC(vCC).DeliveryP Is Nothing, 100, AC(vCC).DeliveryP(Lever)), AC(vCC).PelletP(Lever)) Then
                    deliveredCount += 1
                    Dim seriesName As String = ReinforcerSeriesName(Lever)
                    If Chart1.Series.IndexOf(seriesName) >= 0 Then Chart1.Series(seriesName).Points.AddXY(chartTime(Lever), chartResponse(Lever))
                End If
            Next
            RefCount(vCC, Lever) += deliveredCount
            RefCount_i(Lever) += deliveredCount
            RefreshInputStatusTable()

            If deliveredCount > 0 Then WriteRawEvent("R" & (Lever + 1))

            InitializeSchedule(Lever)
        End If

        ' Check component termination based on maximum reinforcers
        If ComponentRefTotal() >= AC(vCC).MaxRefs AndAlso AC(vCC).MaxRefs > 0 Then
            ComponentDuration_Code()
        End If

    End Sub

    Private Function DelaySignalAtEnd(inputIndex As Integer) As Boolean
        If AC(vCC).DelayType Is Nothing Then Return False
        If inputIndex < 0 OrElse inputIndex > AC(vCC).DelayType.Length - 1 Then Return False
        Return If(AC(vCC).DelayType(inputIndex), "").IndexOf("Signal End", StringComparison.OrdinalIgnoreCase) >= 0
    End Function


    Private Function ReinforcerDelivery(reinforcerType As String, deliveryProbability As Integer, pelletProbability As Integer) As Boolean
        If ProbabilityHit(deliveryProbability) = False Then Return False

        If reinforcerType = "Pellet" Then
            SendArduino("R")
            Return True

        ElseIf reinforcerType = "Liquid" Then
            SendArduino("W")
            Return True

        ElseIf reinforcerType = "Random" Then
            If ProbabilityHit(pelletProbability) Then
                SendArduino("R")
            Else
                SendArduino("W")
            End If
            Return True
        End If

        Return False
    End Function

    Private Sub FRGen(x) 'This initializes Fixed Ratio schedules depending on the selected values / operanda.
        'FR schedules just check current responses against the specified schedule value.
        RatioGoal(x) = Math.Max(1, AC(vCC).ScheduleValue(x))
    End Sub
    Private Sub VRGen(x) 'This initializes Variable Ratio schedules depending on the selected values / operanda.
        'VR schedules calculate a range between half and 1.5 times the specified schedule value and pick a random value from that range. 
        Dim minGoal As Integer = Math.Max(1, CInt(Math.Floor(AC(vCC).ScheduleValue(x) / 2.0)))
        Dim maxGoal As Integer = Math.Max(minGoal + 1, CInt(Math.Ceiling(AC(vCC).ScheduleValue(x) * 1.5)))
        RatioGoal(x) = SessionRandom.Next(minGoal, maxGoal + 1)
    End Sub
    Private Sub FIGen(x) 'This initializes Fixed Interval schedules depending on the selected values / operanda.
        'FI schedules use a timer to check if the specified schedule value has elapsed.
        'Visual Basic manages time in miliseconds, so values are multiplied by 1000.
        ScheduleTimers(x).Interval = Math.Max(1, (AC(vCC).ScheduleValue(x) + 1) * 1000)
        ScheduleTimers(x).Enabled = True
    End Sub
    Private Sub VIGen(list As Integer) 'This initializes Variable Interval schedules depending on the selected values / operanda.
        'VI schedules employ Hantula's (1991) method for Fleshler & Hoffman's (1968) iterative equation on Visual Basic.
        'This Subroutine takes 'list' as a value to allocate VI iterations on separate data arrays, allowing for concurrent and independent VI schedules.
        'VI values are selected at random from the lists without replacement. 
        Dim v As Integer
        Dim n = 10 'This value represents the VI iterations. 
        Dim rd(n)
        Dim vi(n)
        Dim order
        v = AC(vCC).ScheduleValue(list)
        If VIList(list).Count = 0 Then
            For m As Integer = 1 To n
                If m = n Then vi(m) = v * (1 + Log(n)) : GoTo 1
                vi(m) = v * (1 + (Log(n)) + (n - m) * (Log(n - m)) - (n - m + 1) * Log(n - m + 1))
1:              order = SessionRandom.Next(1, n + 1)
                If rd(order) = 0 Then
                    rd(order) = vi(m)
                Else
                    GoTo 1
                End If
            Next
            For a As Integer = 1 To n
                VIList(list).Add(rd(a))
            Next
        End If
        Dim p As Integer = SessionRandom.Next(VIList(list).Count)
        ScheduleTimers(list).Interval = Math.Max(1, (VIList(list).Item(p) + 1) * 1000)
        ScheduleTimers(list).Enabled = True
        VIList(list).RemoveAt(p)
    End Sub

    Private Function TStimulus(inputIndex As Integer) As String
        If TCurrentPeriodIsD(inputIndex) Then
            Return If(AC(vCC).TStimD Is Nothing, "None", AC(vCC).TStimD(inputIndex))
        End If

        Return If(AC(vCC).TStimDelta Is Nothing, "None", AC(vCC).TStimDelta(inputIndex))
    End Function

    Private Function TPeriodDuration(inputIndex As Integer) As Double
        If TCurrentPeriodIsD(inputIndex) Then
            Return If(AC(vCC).TDurationD Is Nothing, 0, AC(vCC).TDurationD(inputIndex))
        End If

        Return If(AC(vCC).TDurationDelta Is Nothing, 0, AC(vCC).TDurationDelta(inputIndex))
    End Function

    Private Sub InitializeTSchedule(inputIndex As Integer)
        If AC(vCC).TCycles Is Nothing OrElse AC(vCC).TCycles(inputIndex) <= 0 Then Exit Sub
        If AC(vCC).TDurationD(inputIndex) <= 0 OrElse AC(vCC).TDurationDelta(inputIndex) <= 0 Then Exit Sub

        TPeriodsRemaining(inputIndex) = AC(vCC).TCycles(inputIndex) * 2
        TCurrentPeriodIsD(inputIndex) = If(AC(vCC).TStartPeriod Is Nothing, True, AC(vCC).TStartPeriod(inputIndex) <> "TDelta")
        StartTPeriod(inputIndex)
    End Sub

    Private Sub StartTPeriod(inputIndex As Integer)
        If TPeriodsRemaining(inputIndex) <= 0 OrElse SessionStarted = False OrElse SessionEnding Then Exit Sub

        TInInterPeriod(inputIndex) = False
        ActivateStimulus(TStimulus(inputIndex), True)
        TPeriodTimers(inputIndex).Interval = Math.Max(1, CInt(TPeriodDuration(inputIndex) * 1000))
        TPeriodTimers(inputIndex).Enabled = True
        TPeriodsRemaining(inputIndex) -= 1
        RefreshInputStatusTable()
    End Sub

    Private Sub StopTPeriod(inputIndex As Integer)
        TPeriodTimers(inputIndex).Enabled = False
        ActivateStimulus(TStimulus(inputIndex), False)
    End Sub

    Private Sub AdvanceTPeriod(inputIndex As Integer)
        If TPeriodsRemaining(inputIndex) <= 0 Then
            TInInterPeriod(inputIndex) = False
            RefreshInputStatusTable()
            Exit Sub
        End If

        TCurrentPeriodIsD(inputIndex) = Not TCurrentPeriodIsD(inputIndex)
        StartTPeriod(inputIndex)
    End Sub

    Private Sub TPeriodTimer_Tick(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = InputTimerIndex(TPeriodTimers, sender)
        If inputIndex < 0 Then Exit Sub

        StopTPeriod(inputIndex)
        If AC(vCC).TInterPeriod IsNot Nothing AndAlso AC(vCC).TInterPeriod(inputIndex) > 0 AndAlso TPeriodsRemaining(inputIndex) > 0 Then
            TInInterPeriod(inputIndex) = True
            TInterPeriodTimers(inputIndex).Interval = Math.Max(1, CInt(AC(vCC).TInterPeriod(inputIndex) * 1000))
            TInterPeriodTimers(inputIndex).Enabled = True
            RefreshInputStatusTable()
        Else
            AdvanceTPeriod(inputIndex)
        End If
    End Sub

    Private Sub TInterPeriodTimer_Tick(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = InputTimerIndex(TInterPeriodTimers, sender)
        If inputIndex < 0 Then Exit Sub

        TInterPeriodTimers(inputIndex).Enabled = False
        TInInterPeriod(inputIndex) = False
        AdvanceTPeriod(inputIndex)
    End Sub

    Private Sub TScheduleResponse(inputIndex As Integer)
        If TInInterPeriod(inputIndex) Then Exit Sub
        If TPeriodTimers(inputIndex).Enabled = False Then Exit Sub

        Dim probability As Integer = If(TCurrentPeriodIsD(inputIndex), AC(vCC).TProbabilityD(inputIndex), AC(vCC).TProbabilityDelta(inputIndex))
        If ProbabilityHit(probability) Then Reinforce(inputIndex, False)
    End Sub

    Private Function NormalizedTimeScheduleType() As String
        Return If(AC(vCC).TimeScheduleType, "").Trim().ToLowerInvariant()
    End Function

    Private Sub InitializeTimeSchedule()
        tmrTimeSchedule.Enabled = False
        Dim scheduleType As String = NormalizedTimeScheduleType()
        If scheduleType <> "fixed time" AndAlso scheduleType <> "variable time" Then Exit Sub
        If AC(vCC).TimeScheduleValue <= 0 Then Exit Sub
        ScheduleNextTimeReinforcer()
    End Sub

    Private Sub ScheduleNextTimeReinforcer()
        Dim scheduleType As String = NormalizedTimeScheduleType()
        Dim intervalSeconds As Double = AC(vCC).TimeScheduleValue

        If scheduleType = "variable time" Then
            If TimeScheduleList.Count = 0 Then
                Dim v As Double = AC(vCC).TimeScheduleValue
                Dim n As Integer = 10
                Dim rd(n) As Integer
                Dim vi(n) As Integer
                For m As Integer = 1 To n
                    If m = n Then
                        vi(m) = CInt(Math.Max(1, v * (1 + Log(n))))
                    Else
                        vi(m) = CInt(Math.Max(1, v * (1 + (Log(n)) + (n - m) * (Log(n - m)) - (n - m + 1) * Log(n - m + 1))))
                    End If
RetryOrder:
                    Dim order As Integer = SessionRandom.Next(1, n + 1)
                    If rd(order) = 0 Then
                        rd(order) = vi(m)
                    Else
                        GoTo RetryOrder
                    End If
                Next
                For i As Integer = 1 To n
                    TimeScheduleList.Add(rd(i))
                Next
            End If

            Dim p As Integer = SessionRandom.Next(TimeScheduleList.Count)
            intervalSeconds = TimeScheduleList(p)
            TimeScheduleList.RemoveAt(p)
        End If

        tmrTimeSchedule.Interval = Math.Max(1, CInt(intervalSeconds * 1000))
        tmrTimeSchedule.Enabled = True
    End Sub

    Private Sub tmrTimeSchedule_Tick(sender As Object, e As EventArgs) Handles tmrTimeSchedule.Tick
        tmrTimeSchedule.Enabled = False
        If SessionStarted = False OrElse SessionEnding Then Exit Sub
        If NormalizedTimeScheduleType() <> "fixed time" AndAlso NormalizedTimeScheduleType() <> "variable time" Then Exit Sub

        Dim deliveredCount As Integer = 0
        For i As Integer = 1 To Math.Max(1, AC(vCC).TimeMagnitude)
            If ReinforcerDelivery(AC(vCC).TimeReinforcer, AC(vCC).TimeDeliveryP, AC(vCC).TimePelletP) Then deliveredCount += 1
        Next

        If deliveredCount > 0 Then
            TimeRefCount(vCC) += deliveredCount
            TimeRefCount_i += deliveredCount
            WriteRawEvent("RT")
            RefreshInputStatusTable()
        End If

        If ComponentRefTotal() >= AC(vCC).MaxRefs AndAlso AC(vCC).MaxRefs > 0 Then
            ComponentDuration_Code()
        Else
            ScheduleNextTimeReinforcer()
        End If
    End Sub

    Private Sub InitializeSchedule(inputIndex As Integer)
        Dim scheduleType As String = NormalizedScheduleType(inputIndex)
        If scheduleType = "fixed ratio" Then FRGen(inputIndex)
        If scheduleType = "variable ratio" Then VRGen(inputIndex)
        If scheduleType = "fixed interval" Then FIGen(inputIndex)
        If scheduleType.Contains("variable interval") Then VIGen(inputIndex)
    End Sub

    Private Sub ScheduleTimer_Tick(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = InputTimerIndex(ScheduleTimers, sender)
        If inputIndex < 0 Then Exit Sub

        ScheduleTimers(inputIndex).Enabled = False
        refRdy(inputIndex) = True
        RefreshInputStatusTable()
    End Sub
    Private Sub Finish()
        SessionClosed = True
        SessionStarted = False
        SessionEnding = False
        tmrChrt.Enabled = False

        Chart1.SaveImage("C:\Data\Charts\" & SetUp.txtSubject.Text & "_" & SetUp.txtSession.Text & "_chart_" & Format(Date.Now, "hh_mm_ss") & ".png", ChartImageFormat.Png)
        SendArduino("htabeflmcdnop") 'Turns off every output on the Arduino.
        If Arduino IsNot Nothing AndAlso Arduino.IsOpen Then Arduino.Close() 'Terminates Arduino-VB communication.

        FileClose(1)

        If HasAnyObtainedDelay() Then
            ' ---------------------------------------------------------
            ' Obtained delays summary (per component, per input)
            ' ---------------------------------------------------------
            WriteLine(2, "Obtained delays (seconds) by component:")

            For s = 1 To MAXvCC
                For inputIndex As Integer = 0 To ActiveInputCount(s) - 1
                    If ObtainedDelayDurations(s, inputIndex).Count > 0 Then
                        Dim secs = ObtainedDelayDurations(s, inputIndex).Select(Function(ms) Math.Round(ms / 1000.0, 3)).ToArray()
                        WriteLine(2, "Component " & s & " " & InputLabel(inputIndex, s) & ": " & String.Join(", ", secs))
                    End If
                Next
            Next
        End If


        For i = 2 To 2

            ' ---------------------------------------------------------
            ' Compute the *measured* duration for each component (seconds)
            ' and determine which components were actually presented.
            ' ---------------------------------------------------------
            For s = 1 To MAXvCC
                Dim o = 0
                For g = 0 To AC(s).ComponentIteration
                    o += AC(s).ComponentDuration_measured(g)
                Next
                AC(s).ComponentDuration = o
            Next

            ' ---------------------------------------------------------
            ' Responses summary (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Responses:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    For inputIndex As Integer = 0 To ActiveInputCount(s) - 1
                        WriteLine(i, InputLabel(inputIndex, s) & " Component " & s & ": " & ResponseCount(s, inputIndex))
                    Next
                End If
            Next

            ' ---------------------------------------------------------
            ' Response rates summary (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Response rates:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    For inputIndex As Integer = 0 To ActiveInputCount(s) - 1
                        WriteLine(i, InputLabel(inputIndex, s) & " Component " & s & ": " & (ResponseCount(s, inputIndex) / (AC(s).ComponentDuration / 60)))
                    Next
                End If
            Next

            ' ---------------------------------------------------------
            ' Reinforcers summary (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Reinforcers:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    For inputIndex As Integer = 0 To ActiveInputCount(s) - 1
                        WriteLine(i, InputLabel(inputIndex, s) & " Component " & s & ": " & RefCount(s, inputIndex))
                    Next
                    If AC(s).TimeScheduleType <> "" AndAlso AC(s).TimeScheduleType <> "None" Then
                        WriteLine(i, "Time schedule Component " & s & ": " & TimeRefCount(s))
                    End If
                End If
            Next

            ' ---------------------------------------------------------
            ' Reinforcer rates summary (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Reinforcer rates:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    For inputIndex As Integer = 0 To ActiveInputCount(s) - 1
                        WriteLine(i, InputLabel(inputIndex, s) & " Component " & s & ": " & (RefCount(s, inputIndex) / (AC(s).ComponentDuration / 60)))
                    Next
                    If AC(s).TimeScheduleType <> "" AndAlso AC(s).TimeScheduleType <> "None" Then
                        WriteLine(i, "Time schedule Component " & s & ": " & (TimeRefCount(s) / (AC(s).ComponentDuration / 60)))
                    End If
                End If
            Next

            ' ---------------------------------------------------------
            ' Lever responses during delay (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Lever responses during delay:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    For inputIndex As Integer = 0 To ActiveInputCount(s) - 1
                        WriteLine(i, "Component " & s & " " & InputLabel(inputIndex, s) & ": " & ResponseCountDel(s, inputIndex))
                    Next
                End If
            Next

            ' ---------------------------------------------------------
            ' Response rates during delay (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Response rates during delay:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    For inputIndex As Integer = 0 To ActiveInputCount(s) - 1
                        WriteLine(i, "Component " & s & " " & InputLabel(inputIndex, s) & ": " & (ResponseCountDel(s, inputIndex) / (AC(s).ComponentDuration / 60)))
                    Next
                End If
            Next

            ' ---------------------------------------------------------
            ' Session footer
            ' ---------------------------------------------------------
            WriteLine(i, "Total time in minutes: " & SessionTimeSeconds / 60)
            WriteLine(i, Format(Date.Now, "dd-MM-yyyy_hh-mm-ss"))
            WriteLine(i, "END") 'Signals that the session has ended on the data file.
            FileClose(i) 'Closes data file.
        Next

        Application.Exit()

    End Sub

    Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        SessionClosed = True
        tmrChrt.Enabled = False
        tmrStart.Enabled = False
        tmrPostSession.Enabled = False
        tmrComponentDuration.Enabled = False
        tmrComponentStim.Enabled = False
        tmrComponentLightStim.Enabled = False
        tmrTimeSchedule.Enabled = False
        tmrICI.Enabled = False
        tmrCOD.Enabled = False
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            If TPeriodTimers(inputIndex) IsNot Nothing Then TPeriodTimers(inputIndex).Enabled = False
            If TInterPeriodTimers(inputIndex) IsNot Nothing Then TInterPeriodTimers(inputIndex).Enabled = False
            If DelaySignalStartTimers(inputIndex) IsNot Nothing Then DelaySignalStartTimers(inputIndex).Enabled = False
        Next
        If Arduino IsNot Nothing AndAlso Arduino.IsOpen Then Arduino.Close()
    End Sub


    Private Sub tmrChrt_Tick(sender As Object, e As EventArgs) Handles tmrChrt.Tick

        'This timer tick updates the chart once per second (tmrChrt.Interval = 1000).
        'It advances "chartTime" and plots current response counts plus a visual indicator of the active component.

        For i = 0 To MAX_INPUTS
            'Advance the X-axis counters for each plotted stream (lever1, lever2, tray, and component indicator).
            chartTime(i) += 1
        Next

        For inputIndex As Integer = 0 To ActiveInputCount() - 1
            Dim seriesName As String = ResponseSeriesName(inputIndex)
            If Chart1.Series.IndexOf(seriesName) >= 0 Then
                Chart1.Series(seriesName).Points.AddXY(chartTime(inputIndex), chartResponse(inputIndex))
            End If
        Next

        For componentIndex As Integer = 1 To MAXvCC
            Dim seriesName As String = ComponentSeriesName(componentIndex)
            If Chart1.Series.IndexOf(seriesName) >= 0 Then
                Dim yValue As Integer = chartResponse(MAX_INPUTS) - CompIndex
                If tmrICI.Enabled = False AndAlso componentIndex = vCC Then yValue = chartResponse(MAX_INPUTS)
                Chart1.Series(seriesName).Points.AddXY(chartTime(MAX_INPUTS), yValue)
            End If
        Next

        'If response counts get large, the component indicator line might overlap with the response lines.
        'This block shifts component-series Y-values upward once (by +10) to keep the component indicator readable.
        Dim maxInputResponses As Integer = 0
        For inputIndex As Integer = 0 To ActiveInputCount() - 1
            If chartResponse(inputIndex) > maxInputResponses Then maxInputResponses = chartResponse(inputIndex)
        Next

        If maxInputResponses > 200 And chartFlag(0) = False Then
            chartFlag(0) = True

            'Shift only points that are above 0 (avoid moving the "off" baseline).
            For componentIndex As Integer = 1 To MAXvCC
                Dim seriesName As String = ComponentSeriesName(componentIndex)
                If Chart1.Series.IndexOf(seriesName) >= 0 Then
                    For Each pt As DataPoint In Chart1.Series(seriesName).Points
                        If pt.YValues(0) > 0 Then pt.YValues(0) += 10
                    Next
                End If
            Next

            'Update the stored offset so future "off" plotting stays aligned with the shifted points.
            CompIndex += 10
            chartResponse(MAX_INPUTS) += 10

        End If

        RefreshMainSessionTable(If(tmrICI.Enabled, "ICI", ""))
    End Sub

    Private Sub DelayTimer_Tick(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = InputTimerIndex(DelayTimers, sender)
        If inputIndex < 0 Then Exit Sub

        DelayTimers(inputIndex).Enabled = False
        DelaySignalStartTimers(inputIndex).Enabled = False
        DelaySignalTimers(inputIndex).Enabled = False
        ActivateStimulus(AC(vCC).DelayType(inputIndex), False)

        Reinforce(inputIndex, True)
        RestoreRetractedInput(inputIndex)

        If DelayOnset(inputIndex) >= 0 AndAlso DelayComp(inputIndex) = vCC Then
            Dim dur As Integer = vTimeNow - DelayOnset(inputIndex)
            If dur < 0 Then dur = 0
            ObtainedDelayDurations(vCC, inputIndex).Add(dur)
        End If

        DelayOnset(inputIndex) = -1
        DelayComp(inputIndex) = -1
    End Sub

    Private Sub DelaySignalTimer_Tick(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = InputTimerIndex(DelaySignalTimers, sender)
        If inputIndex < 0 Then Exit Sub

        DelaySignalTimers(inputIndex).Enabled = False
        ActivateStimulus(AC(vCC).DelayType(inputIndex), False)
    End Sub

    Private Sub DelaySignalStartTimer_Tick(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = InputTimerIndex(DelaySignalStartTimers, sender)
        If inputIndex < 0 Then Exit Sub

        DelaySignalStartTimers(inputIndex).Enabled = False
        If DelayTimers(inputIndex).Enabled = False Then Exit Sub

        ActivateStimulus(AC(vCC).DelayType(inputIndex), True)
        DelaySignalTimers(inputIndex).Interval = Math.Max(1, CInt(AC(vCC).DelaySignalDuration(inputIndex) * 1000))
        DelaySignalTimers(inputIndex).Enabled = True
    End Sub

    Private Sub FeedbackTimer_Tick(sender As Object, e As EventArgs)
        Dim inputIndex As Integer = InputTimerIndex(FeedbackTimers, sender)
        If inputIndex < 0 Then Exit Sub

        FeedbackTimers(inputIndex).Enabled = False
        ActivateStimulus(AC(vCC).FeedbackType(inputIndex), False)

        If AC(vCC).FeedbackType(inputIndex).Contains("Time Out") = True Then
            For activeInput As Integer = 0 To ActiveInputCount() - 1
                SetInputOutput(activeInput, IsInputAvailable(activeInput) AndAlso IsExtinctionSchedule(activeInput) = False)
            Next
            StartComponentStimulus()
            If AC(vCC).HouselightOnOff = True Then SendArduino("H")
        End If
    End Sub

    Private Sub StartPostSession()
        If SessionEnding Then Exit Sub

        SessionEnding = True
        SessionStarted = False
        tmrComponentDuration.Enabled = False        'Stop the component duration timer.
        tmrComponentStim.Enabled = False            'Stop any component-related stimulation timer.
        tmrComponentLightStim.Enabled = False
        tmrTimeSchedule.Enabled = False
        tmrChrt.Enabled = False
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            If TPeriodTimers(inputIndex) IsNot Nothing Then TPeriodTimers(inputIndex).Enabled = False
            If TInterPeriodTimers(inputIndex) IsNot Nothing Then TInterPeriodTimers(inputIndex).Enabled = False
            If DelaySignalStartTimers(inputIndex) IsNot Nothing Then DelaySignalStartTimers(inputIndex).Enabled = False
            ActivateStimulus(If(AC(vCC).TStimD Is Nothing, "None", AC(vCC).TStimD(inputIndex)), False)
            ActivateStimulus(If(AC(vCC).TStimDelta Is Nothing, "None", AC(vCC).TStimDelta(inputIndex)), False)
        Next

        RefreshMainSessionTable("Post-session")
        RefreshInputStatusTable()
        UpdateManualControls()

        'Disable all manual interaction controls to prevent further responses.
        btnFinish.Enabled = False
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            SetSimulationButton(inputIndex, False)
            SetManualButton("btnInputOutput" & (inputIndex + 1), inputIndex, False, "In/Out " & (inputIndex + 1))
            SetManualButton("btnInputReinforcer" & (inputIndex + 1), inputIndex, False, "Rf " & (inputIndex + 1))
        Next

        'Start the post-session timer (e.g., to allow animals to consume the last reinforcer).
        tmrPostSession.Interval = Math.Max(1, SetUp.txbPostSession.Text * 1000)
        PostSessionEndTick = Environment.TickCount + tmrPostSession.Interval
        LastPostSessionSecond = Integer.MinValue
        tmrPostSession.Enabled = True
    End Sub

    Private Sub btnFinish_Click(sender As Object, e As EventArgs) Handles btnFinish.Click
        'This handler is triggered when the user manually ends the session by clicking the Finish button.
        Dim result As DialogResult = MessageBox.Show(
            "Are you sure you want to finish the session?",
            "Finish session",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning)

        If result <> DialogResult.Yes Then Exit Sub
        StartPostSession()
    End Sub

    Private Sub tmrPostSession_Tick(sender As Object, e As EventArgs) Handles tmrPostSession.Tick
        'This timer fires after the post-session interval has elapsed.
        'It performs final cleanup, writes summaries, closes files, and exits the program.

        tmrPostSession.Enabled = False
        Finish()
    End Sub

    Private Sub tmrComponentDuration_Tick(sender As Object, e As EventArgs) Handles tmrComponentDuration.Tick
        'This timer marks the programmed end of the current component.
        'Control is passed to ComponentDuration_Code to handle the transition.
        ComponentDuration_Code()
    End Sub

    Private Sub ComponentDuration_Code()
        'This routine handles the formal termination of a component.
        'It logs the component end, computes the actual component duration,
        'clears component-related UI elements, and initiates the inter-component interval (ICI).

        tmrComponentDuration.Enabled = False         'Stop the component duration timer.
        tmrTimeSchedule.Enabled = False
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            RefCount_i(inputIndex) = 0
        Next

        'Log the end of the component in the data file.
        WriteRawEvent("EndComponent" & vCC)
        AddComponentEndMarker()

        'Compute actual component duration by subtracting start time from current time.
        AC(vCC).ComponentDuration_measured(AC(vCC).IterationsLeft) =
            SessionTimeSeconds - AC(vCC).ComponentDuration_measured(AC(vCC).IterationsLeft)

        'Clear component-specific UI before either entering ICI or advancing immediately.
        RefreshMainTables("Transition")

        'Turn off component stimulation.
        StopComponentStimulus()

        SendArduino("abefht")


        'If a component ends while a delay is active, invalidate delay annotation
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            DelayOnset(inputIndex) = -1
            DelayComp(inputIndex) = -1
            DelayTimers(inputIndex).Enabled = False
            DelaySignalTimers(inputIndex).Enabled = False
            DelaySignalStartTimers(inputIndex).Enabled = False
            FeedbackTimers(inputIndex).Enabled = False
            ScheduleTimers(inputIndex).Enabled = False
            TPeriodTimers(inputIndex).Enabled = False
            TInterPeriodTimers(inputIndex).Enabled = False
            ActivateStimulus(If(AC(vCC).TStimD Is Nothing, "None", AC(vCC).TStimD(inputIndex)), False)
            ActivateStimulus(If(AC(vCC).TStimDelta Is Nothing, "None", AC(vCC).TStimDelta(inputIndex)), False)
        Next


        tmrCOD.Enabled = False
        CODL = 0

        If SetUp.ICIDurationSeconds() > 0 Then
            StartInterComponentInterval()
        Else
            AdvanceAfterInterComponentInterval()
        End If
    End Sub

    Private Sub StartInterComponentInterval()
        If SetUp.ICIRetractInputs() Then TurnOffAllInputs()
        ActivateStimulus(SetUp.ICIStimulusType(), True)
        RefreshMainTables("ICI")

        tmrICI.Interval = Math.Max(1, SetUp.ICIDurationSeconds() * 1000)
        tmrICI.Enabled = True
    End Sub

    Private Sub tmrComponentStim_Tick(sender As Object, e As EventArgs) Handles tmrComponentStim.Tick
        ComponentToneStimOn = Not ComponentToneStimOn
        ActivateStimulus("Tone", ComponentToneStimOn)
    End Sub

    Private Sub tmrComponentLightStim_Tick(sender As Object, e As EventArgs) Handles tmrComponentLightStim.Tick
        ComponentLightStimOn = Not ComponentLightStimOn
        ActivateStimulus(AC(vCC).ComponentStimType, ComponentLightStimOn, False)
    End Sub

    Private Sub AdvanceAfterInterComponentInterval()
        'This timer fires at the end of the inter-component interval.
        'It determines whether all components have been exhausted or selects the next component.

        tmrICI.Enabled = False
        ActivateStimulus(SetUp.ICIStimulusType(), False)

        Dim allDepleted As Boolean = True

        'Check whether any component still has iterations remaining.
        For i = 1 To MAXvCC
            If AC(i).IterationsLeft > 0 Then allDepleted = False
        Next

        If allDepleted = True Then ComponentsDepleted = True

        'If all components are depleted, end the session.
        If ComponentsDepleted = True Then
            StartPostSession()
        Else
            'Otherwise, select the next component and start it.
            If CompSequence Is Nothing Then
                'Sequential component presentation.
                Dim nextComponent As Integer = vCC
                For offset As Integer = 1 To MAXvCC
                    Dim candidate As Integer = ((vCC - 1 + offset) Mod MAXvCC) + 1
                    If AC(candidate).IterationsLeft > 0 Then
                        nextComponent = candidate
                        Exit For
                    End If
                Next
                vCC = CByte(nextComponent)

                BeginPrograms()
            Else
                'Manual or random sequence presentation.
                BeginPrograms()
            End If
        End If
    End Sub

    Private Sub tmrICI_Tick(sender As Object, e As EventArgs) Handles tmrICI.Tick
        AdvanceAfterInterComponentInterval()
    End Sub

    Private Sub tmrCOD_Tick(sender As Object, e As EventArgs) Handles tmrCOD.Tick
        'This timer enforces the changeover delay (COD).
        'Once it elapses, responses on either lever are again eligible to produce consequences.

        tmrCOD.Enabled = False
        CODL = 0
    End Sub


End Class
