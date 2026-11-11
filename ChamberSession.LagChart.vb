Option Strict On
Imports Microsoft.VisualBasic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms.DataVisualization.Charting

Partial Public Class ChamberSession
    'The full count is independent of the bounded, scrollable rendering window.
    Private NotInheritable Class LagFrequencyState
        Public ReadOnly Counts As New Dictionary(Of String, Long)(StringComparer.Ordinal)
        Public ReadOnly Order As New List(Of String)
        Public ReadOnly Transitions(6, 6) As Long
        Public ReadOnly Departures(6) As Long
        Public ReadOnly Responses(6) As Long
        Public ReadOnly AllTransitions(6, 6) As Long
        Public ReadOnly AllDepartures(6) As Long
        Public ReadOnly AllResponses(6) As Long
        Public AllResponseTotal As Long
        Public LastResponseInput As Integer
        Public ResponseTotal As Long
        Public Total As Long
        Public WindowStart As Integer
        Public UserScrolled As Boolean
    End Class
    Private Const LagChartWindowSize As Integer = 40
    Private ReadOnly lagFrequencyStates(MAX_COMPONENTS) As LagFrequencyState
    Private lagFrequencyPanel As Panel
    Private lagFrequencyTabs As TabControl
    Private lagTransitionDiagram As LagTransitionDiagram
    Private lagTransitionMode As CheckBox
    Private lagFrequencyChart As Chart
    Private lagFrequencyScroll As HScrollBar
    Private lagFrequencyTimer As Timer
    Private lagFrequencyDirty As Boolean
    Private lagFrequencyComponent As Integer
    Private lagFrequencyUpdatingScroll As Boolean

    Private Sub EnsureLagFrequencyChart()
        If lagFrequencyPanel IsNot Nothing Then Return
        lagFrequencyPanel = New Panel With {.Name = "LagFrequencyPanel", .Bounds = Chart1.Bounds, .Anchor = Chart1.Anchor, .Visible = False}
        lagFrequencyChart = New Chart With {.Name = "LagFrequencyChart", .Dock = DockStyle.Fill, .BackColor = Color.White}
        Dim area As New ChartArea("Sequences")
        area.AxisX.Title = "Completed response sequence (first-observed order)"
        area.AxisX.Interval = 1
        area.AxisX.MajorGrid.Enabled = False
        area.AxisX.LabelStyle.Angle = -45
        area.AxisY.Title = "Occurrences"
        area.AxisY.Minimum = 0
        area.AxisY.LabelStyle.Format = "0"
        area.AxisY.MajorGrid.LineColor = Color.Gainsboro
        lagFrequencyChart.ChartAreas.Add(area)
        lagFrequencyChart.Series.Add(New Series("Frequency") With {.ChartType = SeriesChartType.Column, .Color = Color.SteelBlue, .IsValueShownAsLabel = True, .IsXValueIndexed = True, .ChartArea = "Sequences"})
        lagFrequencyChart.Titles.Add("Completed sequences")
        lagFrequencyScroll = New HScrollBar With {.Dock = DockStyle.Bottom, .Minimum = 0, .SmallChange = 1, .LargeChange = LagChartWindowSize, .Enabled = False}
        AddHandler lagFrequencyScroll.Scroll, AddressOf LagFrequencyScrollChanged
        lagFrequencyTabs = New TabControl With {.Dock = DockStyle.Fill}
        Dim bars As New TabPage("Sequence frequencies")
        Dim transitions As New TabPage("Response transitions")
        bars.Controls.Add(lagFrequencyChart)
        bars.Controls.Add(lagFrequencyScroll)
        lagTransitionDiagram = New LagTransitionDiagram With {.Dock = DockStyle.Fill, .BackColor = Color.White}
        lagTransitionMode = New CheckBox With {.Name = "LagTransitionMode", .Dock = DockStyle.Top, .Height = 26, .Text = "All responses (partial / between-trial / delay / timeout). Off: within completed sequences only.", .Checked = False, .AutoSize = False}
        AddHandler lagTransitionMode.CheckedChanged,
            Sub()
                lagFrequencyDirty = True
                RenderLagFrequencyChart()
            End Sub
        transitions.Controls.Add(lagTransitionDiagram)
        transitions.Controls.Add(lagTransitionMode)
        lagFrequencyTabs.TabPages.Add(bars)
        lagFrequencyTabs.TabPages.Add(transitions)
        lagFrequencyPanel.Controls.Add(lagFrequencyTabs)
        Controls.Add(lagFrequencyPanel)
        lagFrequencyTimer = New Timer With {.Interval = 150}
        AddHandler lagFrequencyTimer.Tick, AddressOf LagFrequencyTick
        lagFrequencyTimer.Start()
    End Sub

    Private Function CurrentLagFrequencyState() As LagFrequencyState
        If lagFrequencyStates(vCC) Is Nothing Then lagFrequencyStates(vCC) = New LagFrequencyState()
        Return lagFrequencyStates(vCC)
    End Function

    Private Sub RefreshScheduleChartVisibility()
        If IsLagComponent() Then
            EnsureLagFrequencyChart()
            lagFrequencyComponent = vCC
            Chart1.Visible = False
            lagFrequencyPanel.Visible = True
            lagFrequencyPanel.BringToFront()
            lagFrequencyDirty = True
            RenderLagFrequencyChart()
        Else
            If lagFrequencyPanel IsNot Nothing Then lagFrequencyPanel.Visible = False
            Chart1.Visible = True
            Chart1.BringToFront()
        End If
    End Sub

    Private Sub BeginLagChartPresentation()
        If Not IsLagComponent() Then Return
        CurrentLagFrequencyState().LastResponseInput = 0
    End Sub

    'Called only after the active-presentation deadline check, before delay/timeout gates.
    Private Sub RecordLagChartResponse(inputIndex As Integer)
        If Not IsLagComponent() OrElse inputIndex < 0 OrElse inputIndex >= 6 Then Return
        If AC(vCC).ScheduleType(inputIndex) <> "Lag" Then Return
        Dim state As LagFrequencyState = CurrentLagFrequencyState()
        Dim physicalInput As Integer = inputIndex + 1
        state.AllResponses(physicalInput) += 1
        state.AllResponseTotal += 1
        If state.LastResponseInput > 0 Then
            state.AllTransitions(state.LastResponseInput, physicalInput) += 1
            state.AllDepartures(state.LastResponseInput) += 1
        End If
        state.LastResponseInput = physicalInput
        lagFrequencyDirty = True
    End Sub

    Private Sub RecordLagChartSequence(sequence As String)
        If Not IsLagComponent() Then Return
        If String.IsNullOrEmpty(sequence) OrElse sequence.Any(Function(c) c < "1"c OrElse c > "6"c) Then Throw New ArgumentException("A completed physical-input sequence is required.")
        EnsureLagFrequencyChart()
        Dim state As LagFrequencyState = CurrentLagFrequencyState()
        If Not state.Counts.ContainsKey(sequence) Then
            state.Counts.Add(sequence, 0)
            state.Order.Add(sequence)
            If Not state.UserScrolled Then state.WindowStart = Math.Max(0, state.Order.Count - LagChartWindowSize)
        End If
        state.Counts(sequence) += 1
        state.Total += 1
        For i As Integer = 0 To sequence.Length - 1
            Dim current As Integer = AscW(sequence(i)) - AscW("0"c)
            state.Responses(current) += 1
            state.ResponseTotal += 1
            If i > 0 Then
                Dim previous As Integer = AscW(sequence(i - 1)) - AscW("0"c)
                state.Transitions(previous, current) += 1
                state.Departures(previous) += 1
            End If
        Next
        lagFrequencyDirty = True
    End Sub

    Private Sub LagFrequencyTick(sender As Object, e As EventArgs)
        If IsDisposed OrElse Not lagFrequencyDirty OrElse Not IsLagComponent() Then Return
        RenderLagFrequencyChart()
    End Sub

    Private Sub LagFrequencyScrollChanged(sender As Object, e As ScrollEventArgs)
        If lagFrequencyUpdatingScroll OrElse Not IsLagComponent() Then Return
        Dim state As LagFrequencyState = CurrentLagFrequencyState()
        state.WindowStart = e.NewValue
        state.UserScrolled = state.WindowStart < Math.Max(0, state.Order.Count - LagChartWindowSize)
        lagFrequencyDirty = True
        RenderLagFrequencyChart()
    End Sub

    Private Sub RenderLagFrequencyChart()
        If lagFrequencyChart Is Nothing OrElse Not IsLagComponent() Then Return
        Dim state As LagFrequencyState = CurrentLagFrequencyState()
        Dim maxStart As Integer = Math.Max(0, state.Order.Count - LagChartWindowSize)
        state.WindowStart = Math.Max(0, Math.Min(state.WindowStart, maxStart))
        lagFrequencyUpdatingScroll = True
        Try
            lagFrequencyScroll.Maximum = Math.Max(LagChartWindowSize - 1, state.Order.Count - 1)
            lagFrequencyScroll.Value = state.WindowStart
            lagFrequencyScroll.Enabled = maxStart > 0
        Finally
            lagFrequencyUpdatingScroll = False
        End Try
        Dim series As Series = lagFrequencyChart.Series("Frequency")
        series.Points.Clear()
        Dim endIndex As Integer = Math.Min(state.Order.Count, state.WindowStart + LagChartWindowSize)
        Dim highest As Long = 1
        For index As Integer = state.WindowStart To endIndex - 1
            Dim sequence As String = state.Order(index)
            Dim count As Long = state.Counts(sequence)
            highest = Math.Max(highest, count)
            Dim label As String = If(sequence.Length > 16, sequence.Substring(0, 12) & "... #" & (index + 1), sequence)
            Dim pointIndex As Integer = series.Points.AddXY(index + 1, count)
            series.Points(pointIndex).AxisLabel = label
            series.Points(pointIndex).ToolTip = "Sequence " & (index + 1) & ": " & sequence & Environment.NewLine & "Occurrences: " & count
        Next
        lagFrequencyChart.ChartAreas(0).AxisY.Interval = If(highest <= 10, 1, 0)
        lagFrequencyChart.ChartAreas(0).AxisY.Maximum = If(highest <= 1, 1, Double.NaN)
        lagFrequencyChart.Titles(0).Text = "Lag component " & vCC & " | " & state.Total & " completed | " & state.Order.Count & " different sequences" &
            If(state.Order.Count > LagChartWindowSize, " | showing " & (state.WindowStart + 1) & "-" & endIndex & " (scroll below)", "")
        lagFrequencyChart.AccessibleName = lagFrequencyChart.Titles(0).Text
        Dim inputs As Integer() = Enumerable.Range(1, 6).Where(Function(input) AC(vCC).ScheduleType(input - 1) = "Lag").ToArray()
        If lagTransitionMode.Checked Then
            lagTransitionDiagram.SetData(inputs, state.AllTransitions, state.AllDepartures, state.AllResponses, state.AllResponseTotal, vCC, True)
        Else
            lagTransitionDiagram.SetData(inputs, state.Transitions, state.Departures, state.Responses, state.ResponseTotal, vCC, False)
        End If
        lagFrequencyComponent = vCC
        lagFrequencyDirty = False
    End Sub

    Private Sub SaveScheduleChart(path As String)
        If IsLagComponent() Then
            EnsureLagFrequencyChart()
            RenderLagFrequencyChart()
            Dim folder As String = IO.Path.GetDirectoryName(path)
            Dim stem As String = IO.Path.GetFileNameWithoutExtension(path)
            lagFrequencyChart.SaveImage(IO.Path.Combine(folder, stem & "_LagBars.png"), ChartImageFormat.Png)
            Using diagram As Bitmap = lagTransitionDiagram.RenderImage(Math.Max(900, lagFrequencyChart.Width), Math.Max(500, lagFrequencyChart.Height))
                diagram.Save(IO.Path.Combine(folder, stem & "_LagTransitions.png"), Imaging.ImageFormat.Png)
                If lagFrequencyTabs.SelectedIndex = 1 Then
                    diagram.Save(path, Imaging.ImageFormat.Png)
                Else
                    lagFrequencyChart.SaveImage(path, ChartImageFormat.Png)
                End If
            End Using
        Else
            Chart1.SaveImage(path, ChartImageFormat.Png)
        End If
    End Sub

    Private Sub DisposeLagFrequencyChart(sender As Object, e As EventArgs) Handles MyBase.Disposed
        If lagFrequencyTimer IsNot Nothing Then
            lagFrequencyTimer.Stop()
            lagFrequencyTimer.Dispose()
        End If
    End Sub
End Class

'Empirical first-order transition view, never a predictive or stationarity claim.
Friend NotInheritable Class LagTransitionDiagram
    Inherits Control
    Private inputs() As Integer = {}
    Private transitions(6, 6) As Long
    Private departures(6) As Long
    Private responses(6) As Long
    Private totalResponses As Long
    Private allResponsesMode As Boolean
    Private component As Integer
    Private selected As Integer
    Private positions As New Dictionary(Of Integer, PointF)
    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.ResizeRedraw, True)
    End Sub
    Public Sub SetData(active() As Integer, matrix(,) As Long, outgoing() As Long, frequencies() As Long, total As Long, componentNumber As Integer, Optional includeAllResponses As Boolean = False)
        inputs = active
        transitions = matrix
        departures = outgoing
        responses = frequencies
        totalResponses = total
        allResponsesMode = includeAllResponses
        component = componentNumber
        If Not inputs.Contains(selected) Then selected = If(inputs.Length = 0, 0, inputs(0))
        Invalidate()
    End Sub
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        DrawDiagram(e.Graphics, ClientRectangle)
    End Sub
    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        For Each item In positions
            If Math.Pow(e.X - item.Value.X, 2) + Math.Pow(e.Y - item.Value.Y, 2) <= 35 * 35 Then
                selected = item.Key
                Invalidate()
                Exit For
            End If
        Next
    End Sub
    Public Function RenderImage(width As Integer, height As Integer) As Bitmap
        Dim result As New Bitmap(width, height)
        Using graphics As Graphics = Graphics.FromImage(result)
            DrawDiagram(graphics, New Rectangle(0, 0, width, height))
        End Using
        Return result
    End Function
    Private Sub DrawDiagram(g As Graphics, bounds As Rectangle)
        g.Clear(Color.White)
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim nodeRadius As Single = Math.Min(34, Math.Max(20, (bounds.Height - 190) / 4.0F))
        Using titleFont As New Font("Segoe UI", 10, FontStyle.Bold), smallFont As New Font("Segoe UI", 8), nodeFont As New Font("Segoe UI", If(nodeRadius < 30, 7, 8), FontStyle.Bold)
            g.DrawString("Lag component " & component & " | empirical response transitions | " & If(allResponsesMode, "ALL RESPONSES", "WITHIN COMPLETE SEQUENCES"), titleFont, Brushes.Black, 10, 7)
            g.DrawString(If(allResponsesMode, "Includes partial, between-trial, delay and timeout responses; excludes ICI, inactive inputs, other components and between-presentation links.", "Within completed sequences only; no between-trial, partial, delay, timeout or prior-session transitions."), smallFont, Brushes.DimGray, 10, 29)
            g.DrawString("Click a node: blue arrows show P(next input | Input " & selected & "), labelled count / outgoing total. Gray = other sources.", smallFont, Brushes.DimGray, 10, 45)
            positions.Clear()
            Dim cx As Single = bounds.Width / 2.0F
            Dim cy As Single = (bounds.Height + 12) / 2.0F
            Dim rx As Single = Math.Max(45, bounds.Width * 0.31F)
            Dim ry As Single = Math.Max(nodeRadius * 2.1F, (bounds.Height - 200) / 2.0F)
            For index As Integer = 0 To inputs.Length - 1
                Dim angle As Double = -Math.PI / 2 + 2 * Math.PI * index / Math.Max(1, inputs.Length)
                positions(inputs(index)) = New PointF(cx + CSng(Math.Cos(angle) * rx), cy + CSng(Math.Sin(angle) * ry))
            Next
            'Draw nonselected sources first, so the inspected distribution stays legible.
            For Each source As Integer In inputs.OrderBy(Function(value) If(value = selected, 1, 0))
                If departures(source) = 0 Then Continue For
                For Each target As Integer In inputs
                    Dim count As Long = transitions(source, target)
                    If count = 0 Then Continue For
                    Dim probability As Double = count / CDbl(departures(source))
                    Dim highlighted As Boolean = source = selected
                    Dim fromPoint As PointF = positions(source)
                    Dim toPoint As PointF = positions(target)
                    Dim labelPoint As PointF
                    Using path As New GraphicsPath(), pen As New Pen(If(highlighted, Color.SteelBlue, Color.FromArgb(85, 145, 145, 145)), If(highlighted, 1.4F + CSng(probability * 3), 1.0F)), cap As New AdjustableArrowCap(4, 5, True)
                        pen.CustomEndCap = cap
                        If source = target Then
                            path.AddBezier(fromPoint.X + 25, fromPoint.Y - 20, fromPoint.X + 100, fromPoint.Y - 60, fromPoint.X + 100, fromPoint.Y + 60, fromPoint.X + 25, fromPoint.Y + 20)
                            labelPoint = New PointF(fromPoint.X + 44, fromPoint.Y - 7)
                        Else
                            Dim dx As Single = toPoint.X - fromPoint.X
                            Dim dy As Single = toPoint.Y - fromPoint.Y
                            Dim length As Single = CSng(Math.Sqrt(dx * dx + dy * dy))
                            Dim nx As Single = -dy / length
                            Dim ny As Single = dx / length
                            Dim start As New PointF(fromPoint.X + dx / length * 35, fromPoint.Y + dy / length * 35)
                            Dim finish As New PointF(toPoint.X - dx / length * 35, toPoint.Y - dy / length * 35)
                            Dim bend As Single = 24
                            path.AddBezier(start, New PointF(start.X + dx / 3 + nx * bend, start.Y + dy / 3 + ny * bend), New PointF(finish.X - dx / 3 + nx * bend, finish.Y - dy / 3 + ny * bend), finish)
                            labelPoint = New PointF((start.X + finish.X) / 2 + nx * bend - 40, (start.Y + finish.Y) / 2 + ny * bend - 7)
                        End If
                        g.DrawPath(pen, path)
                    End Using
                    If highlighted Then
                        Dim label As String = probability.ToString("0.000", Globalization.CultureInfo.InvariantCulture) & " (" & count & "/" & departures(source) & ")"
                        Dim size As SizeF = g.MeasureString(label, smallFont)
                        g.FillRectangle(Brushes.White, labelPoint.X, labelPoint.Y, size.Width, size.Height)
                        g.DrawString(label, smallFont, Brushes.DarkBlue, labelPoint)
                    End If
                Next
            Next
            Using centered As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                For Each input As Integer In inputs
                    Dim point As PointF = positions(input)
                    Dim circle As New RectangleF(point.X - 34, point.Y - nodeRadius, 68, nodeRadius * 2)
                    g.FillEllipse(If(input = selected, Brushes.LightSteelBlue, Brushes.WhiteSmoke), circle)
                    g.DrawEllipse(Pens.SteelBlue, circle)
                    Dim marginal As String = If(totalResponses = 0, "n/a", (responses(input) / CDbl(Math.Max(1, totalResponses))).ToString("0.000", Globalization.CultureInfo.InvariantCulture))
                    g.DrawString("Input " & input & Environment.NewLine & "p=" & marginal & Environment.NewLine & "n=" & responses(input), nodeFont, Brushes.Black, circle, centered)
                Next
            End Using
            g.DrawString("Node p = input responses / all responses in the selected mode. No outgoing observations = conditional probability undefined (no arrow).", smallFont, Brushes.DimGray, 10, bounds.Height - 30)
            If selected > 0 AndAlso departures(selected) = 0 Then g.DrawString("Input " & selected & ": no outgoing observations yet.", smallFont, Brushes.DarkBlue, 10, bounds.Height - 49)
        End Using
    End Sub
End Class
