Imports System.Reflection
Imports System.Drawing
Imports System.Windows.Forms.DataVisualization.Charting

Module Constants
    Public Const MAX_COMPONENTS As Integer = 10
End Module
Partial Public Class ChamberSession
    Inherits UserControl
    Public vCC As Integer = 1
    Public Lag As Boolean = True
    Public Structure Blueprint
        Public ScheduleType() As String
    End Structure
    Public AC(10) As Blueprint
    Public Chart1 As New Chart()
    Public Sub New()
        For i As Integer = 0 To 10
            AC(i).ScheduleType = {"Lag", "Lag", "Lag", "Lag", "Lag", "Lag"}
        Next
        Size = New Size(1000, 440)
        Chart1.Bounds = New Rectangle(0, 0, 1000, 440)
        Chart1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Chart1.ChartAreas.Add(New ChartArea("Classic"))
        Chart1.Series.Add(New Series("Classic"))
        Chart1.Series(0).Points.AddXY(1, 1)
        Controls.Add(Chart1)
    End Sub
    Private Function IsLagComponent() As Boolean
        Return Lag
    End Function
End Class
Module LagChartTests
    Private count As Integer
    Private Const flags As BindingFlags = BindingFlags.Instance Or BindingFlags.NonPublic
    Private Sub Check(value As Boolean, message As String)
        If Not value Then Throw New Exception(message)
        count += 1
    End Sub
    Private Function F(o As Object, name As String) As Object
        Return o.GetType().GetField(name, flags).GetValue(o)
    End Function
    Private Sub CallM(o As Object, name As String, ParamArray values() As Object)
        o.GetType().GetMethod(name, flags).Invoke(o, values)
    End Sub
    <STAThread> Public Sub Main()
        Try
            Application.EnableVisualStyles()
            Using a As New ChamberSession(), b As New ChamberSession()
                CallM(a, "RefreshScheduleChartVisibility")
                CallM(a, "BeginLagChartPresentation")
                For Each digit As Char In "112222111122"
                    CallM(a, "RecordLagChartResponse", Integer.Parse(digit.ToString()) - 1)
                Next
                CallM(a, "RecordLagChartSequence", "1122")
                CallM(a, "RecordLagChartSequence", "2211")
                CallM(a, "RecordLagChartSequence", "1122")
                CallM(a, "RenderLagFrequencyChart")
                Dim chart = DirectCast(F(a, "lagFrequencyChart"), Chart)
                Check(chart.Series(0).Points.Count = 2, "one bar per distinct sequence")
                Check(chart.Series(0).Points(0).YValues(0) = 2, "repeat raises same bar")
                Check(chart.Series(0).Points(1).YValues(0) = 1, "second sequence counted")
                Check(Not a.Chart1.Visible, "cumulative hidden during Lag")
                Dim initialStates = DirectCast(F(a, "lagFrequencyStates"), Array)
                Dim initial = initialStates.GetValue(1)
                Dim matrix = DirectCast(initial.GetType().GetField("Transitions").GetValue(initial), Long(,))
                Dim outgoing = DirectCast(initial.GetType().GetField("Departures").GetValue(initial), Long())
                Check(matrix(1, 1) = 3 AndAlso matrix(1, 2) = 2 AndAlso matrix(2, 1) = 1 AndAlso matrix(2, 2) = 3, "within-sequence transitions exact, including self loops")
                Check(outgoing(1) = 5 AndAlso outgoing(2) = 4, "conditional denominators exclude final response of each sequence")
                Dim allMatrix = DirectCast(initial.GetType().GetField("AllTransitions").GetValue(initial), Long(,))
                Dim allOutgoing = DirectCast(initial.GetType().GetField("AllDepartures").GetValue(initial), Long())
                Check(allMatrix(1, 1) = 4 AndAlso allMatrix(2, 2) = 4, "all-response stream includes between-trial links")
                Check(allOutgoing.Sum() = 11, "all-response stream counts every adjacent eligible response")
                Dim modeSelector = DirectCast(F(a, "lagTransitionMode"), CheckBox)
                modeSelector.Checked = True
                Check(Object.ReferenceEquals(F(F(a, "lagTransitionDiagram"), "transitions"), allMatrix), "toggle selects accumulated all-response matrix")
                modeSelector.Checked = False
                Check(Object.ReferenceEquals(F(F(a, "lagTransitionDiagram"), "transitions"), matrix), "toggle restores within-complete matrix without recounting")
                Check(chart.Series(0).Points.Count = 2 AndAlso chart.Series(0).Points(0).YValues(0) = 2, "toggle leaves frequency bars unchanged")
                a.vCC = 2
                CallM(a, "RefreshScheduleChartVisibility")
                Check(chart.Series(0).Points.Count = 0, "new component independent")
                CallM(a, "RecordLagChartSequence", "1122")
                CallM(a, "RenderLagFrequencyChart")
                Check(chart.Series(0).Points(0).YValues(0) = 1, "component totals independent")
                a.vCC = 1
                CallM(a, "RefreshScheduleChartVisibility")
                Check(chart.Series(0).Points(0).YValues(0) = 2, "return retains component frequencies")
                CallM(b, "RefreshScheduleChartVisibility")
                Check(DirectCast(F(b, "lagFrequencyChart"), Chart).Series(0).Points.Count = 0, "other chamber independent")
                CallM(b, "RecordLagChartSequence", "1")
                CallM(b, "RenderLagFrequencyChart")
                Dim bState = DirectCast(F(b, "lagFrequencyStates"), Array).GetValue(1)
                Check(DirectCast(bState.GetType().GetField("Departures").GetValue(bState), Long()).Sum() = 0, "length-one sequence contributes no transition")
                Check(CLng(bState.GetType().GetField("ResponseTotal").GetValue(bState)) = 1, "length-one contributes marginal input probability")
                CallM(b, "BeginLagChartPresentation")
                CallM(b, "RecordLagChartResponse", 0)
                CallM(b, "BeginLagChartPresentation")
                CallM(b, "RecordLagChartResponse", 1)
                b.AC(1).ScheduleType(5) = "N/A"
                CallM(b, "RecordLagChartResponse", 5)
                Check(DirectCast(bState.GetType().GetField("AllDepartures").GetValue(bState), Long()).Sum() = 0, "presentations never linked")
                Check(CLng(bState.GetType().GetField("AllResponseTotal").GetValue(bState)) = 2, "inactive input excluded from all-response mode")
                a.Lag = False
                CallM(a, "RefreshScheduleChartVisibility")
                Check(a.Chart1.Visible AndAlso a.Chart1.Series(0).Points.Count = 1, "Classic cumulative restored intact")
                a.Lag = True
                CallM(a, "RefreshScheduleChartVisibility")
                For i As Integer = 0 To 49999
                    Dim n As Integer = i
                    Dim sequence As String = ""
                    Do
                        sequence = CStr((n Mod 6) + 1) & sequence
                        n = n \ 6
                    Loop While n > 0
                    CallM(a, "RecordLagChartSequence", sequence.PadLeft(7, "1"c))
                Next
                CallM(a, "RenderLagFrequencyChart")
                Check(chart.Series(0).Points.Count = 40, "render window stays bounded for 50000 distinct sequences")
                Dim states = DirectCast(F(a, "lagFrequencyStates"), Array)
                Dim state = states.GetValue(1)
                Dim fieldFlags = BindingFlags.Instance Or BindingFlags.Public
                Check(CLng(state.GetType().GetField("Total", fieldFlags).GetValue(state)) = 50003, "all completed counts retained beyond viewport")
                Check(DirectCast(state.GetType().GetField("Counts", fieldFlags).GetValue(state), Dictionary(Of String, Long)).Count = 50002, "no frequency truncation")
                CallM(a, "SaveScheduleChart", IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LagFrequencyPreview.png"))
                Check(IO.File.Exists(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LagFrequencyPreview_LagTransitions.png")), "transition diagram exported")
                Check(IO.File.Exists(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LagFrequencyPreview.png")), "PNG exports active Lag chart")
                DirectCast(F(a, "lagFrequencyTabs"), TabControl).SelectedIndex = 1
                Dim mainPath = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SelectedTransitions.png")
                CallM(a, "SaveScheduleChart", mainPath)
                Check(IO.File.ReadAllBytes(mainPath).SequenceEqual(IO.File.ReadAllBytes(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SelectedTransitions_LagTransitions.png"))), "main PNG follows selected transition tab")
                Dim diagramControl = DirectCast(F(a, "lagTransitionDiagram"), LagTransitionDiagram)
                Using bitmap = diagramControl.RenderImage(1184, 285)
                    bitmap.Save(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TransitionsUiHeight.png"), Imaging.ImageFormat.Png)
                End Using
                Dim panel = DirectCast(F(a, "lagFrequencyPanel"), Panel)
                panel.CreateControl()
                modeSelector.Checked = False
                Using bitmap As New Bitmap(panel.Width, panel.Height)
                    panel.DrawToBitmap(bitmap, New Rectangle(0, 0, panel.Width, panel.Height))
                    bitmap.Save(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TransitionsToggleWithin.png"), Imaging.ImageFormat.Png)
                End Using
                modeSelector.Checked = True
                Using bitmap As New Bitmap(panel.Width, panel.Height)
                    panel.DrawToBitmap(bitmap, New Rectangle(0, 0, panel.Width, panel.Height))
                    bitmap.Save(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TransitionsToggleAll.png"), Imaging.ImageFormat.Png)
                End Using
                Dim timer = DirectCast(F(a, "lagFrequencyTimer"), Timer)
                a.Dispose()
                Check(Not timer.Enabled, "render timer stops on disposal")
            End Using
            Console.WriteLine("PASS Lag chart: " & count & " assertions; no hardware or network.")
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
End Module
