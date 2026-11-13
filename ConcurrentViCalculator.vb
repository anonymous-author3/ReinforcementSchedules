Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Globalization

'Planning only: sums nominal rates, not obtained reinforcers or response ratios.
Friend NotInheritable Class ConcurrentViCalculation
    Public Shared Function Intervals(globalSeconds As Double, weights As Double()) As Double()
        If Double.IsNaN(globalSeconds) OrElse Double.IsInfinity(globalSeconds) OrElse globalSeconds <= 0 Then Throw New ArgumentException("Global VI must be positive and finite.")
        If weights Is Nothing OrElse weights.Length < 2 OrElse weights.Length > 6 Then Throw New ArgumentException("Choose two to six alternatives.")
        If weights.Any(Function(w) Double.IsNaN(w) OrElse Double.IsInfinity(w) OrElse w <= 0) Then Throw New ArgumentException("Every ratio weight must be positive and finite.")
        Dim maximum = weights.Max()
        Dim scaled = weights.Select(Function(w) w / maximum).ToArray()
        Dim total = scaled.Sum()
        Dim result = scaled.Select(Function(w) globalSeconds * (total / w)).ToArray()
        If result.Any(Function(v) Double.IsInfinity(v) OrElse Double.IsNaN(v) OrElse v <= 0) Then Throw New ArgumentException("The requested ratio exceeds the supported numeric range.")
        Return result
    End Function
    Public Shared Function Equivalent(intervals As Double()) As Double
        Return 1.0 / intervals.Sum(Function(v) 1.0 / v)
    End Function
End Class

Friend Class ConcurrentViCalculator
    Inherits Form
    Private ReadOnly globalVi As New NumericUpDown With {.Name = "globalVi", .Minimum = 0.001D, .Maximum = 1000000D, .DecimalPlaces = 3, .Value = 20D, .Width = 100}
    Private ReadOnly alternatives As New NumericUpDown With {.Name = "alternatives", .Minimum = 2, .Maximum = 6, .Value = 2, .Width = 50}
    Private ReadOnly iterations As New NumericUpDown With {.Name = "iterations", .Minimum = 1, .Maximum = 1000, .Value = 10, .Width = 65}
    Private ReadOnly weights As New List(Of NumericUpDown)
    Private ReadOnly output As New DataGridView With {.Name = "results", .Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False, .RowHeadersVisible = False, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, .BackgroundColor = SystemColors.Window, .SelectionMode = DataGridViewSelectionMode.FullRowSelect}
    Private ReadOnly summary As New Label With {.Name = "summary", .Dock = DockStyle.Fill, .AutoSize = True}
    Private ReadOnly ratioPanel As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .WrapContents = True}
    Private ReadOnly ratioLabels As New List(Of Label)
    Public Sub New()
        Text = "Concurrent VI Calculator"
        Name = "ConcurrentViCalculator"
        Font = SystemFonts.MessageBoxFont
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(820, 590)
        MinimumSize = New Size(780, 570)
        ShowInTaskbar = False
        MaximizeBox = False
        Dim layout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(14), .ColumnCount = 1, .RowCount = 6}
        For Each height As Single In {44, 70, 0, 60, 112, 38}
            layout.RowStyles.Add(New RowStyle(If(height = 0, SizeType.Percent, SizeType.Absolute), If(height = 0, 100, height)))
        Next
        Dim settings As New FlowLayoutPanel With {.Dock = DockStyle.Fill}
        settings.Controls.Add(New Label With {.Text = "Global VI (s):", .AutoSize = True, .Margin = New Padding(0, 5, 6, 0)})
        settings.Controls.Add(globalVi)
        settings.Controls.Add(New Label With {.Text = "Alternatives:", .AutoSize = True, .Margin = New Padding(18, 5, 6, 0)})
        settings.Controls.Add(alternatives)
        settings.Controls.Add(New Label With {.Text = "Iterations (for range check):", .AutoSize = True, .Margin = New Padding(18, 5, 6, 0)})
        settings.Controls.Add(iterations)
        layout.Controls.Add(settings, 0, 0)
        For i = 1 To 6
            Dim label As New Label With {.Text = "Alt. " & i & " weight:", .AutoSize = True, .Margin = New Padding(0, 6, 3, 0)}
            Dim weight As New NumericUpDown With {.Name = "weight" & i, .Minimum = 0.001D, .Maximum = 1000000D, .DecimalPlaces = 3, .Value = 1D, .Width = 72, .Margin = New Padding(0, 3, 12, 3)}
            ratioLabels.Add(label) : weights.Add(weight)
            ratioPanel.Controls.Add(label) : ratioPanel.Controls.Add(weight)
            AddHandler weight.ValueChanged, AddressOf RefreshResults
        Next
        layout.Controls.Add(ratioPanel, 0, 1)
        output.Columns.Add("alternative", "Alternative")
        output.Columns.Add("share", "Rate share (%)")
        output.Columns.Add("ideal", "Ideal VI (s)")
        output.Columns.Add("whole", "Whole-second VI (s)")
        output.Columns.Add("check", "Range check")
        layout.Controls.Add(output, 0, 2)
        layout.Controls.Add(summary, 0, 3)
        layout.Controls.Add(New Label With {.Dock = DockStyle.Fill, .Text = "Weights specify relative programmed reinforcement rates, not VI values or response ratios. VI per alternative = global VI x sum of weights / its weight." & Environment.NewLine & "Whole-second means are rounded independently (minimum 1 s); their combined rate and ratio may differ. Nothing is applied to the component automatically." & Environment.NewLine & "Nominal programmed rates only. Obtained rates depend on responding, COD, delivery probability, and session timing. This is not a prediction of choice."}, 0, 4)
        Dim actions As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
        Dim closeButton As New Button With {.Text = "Close", .DialogResult = DialogResult.Cancel, .AutoSize = True}
        actions.Controls.Add(closeButton)
        CancelButton = closeButton
        layout.Controls.Add(actions, 0, 5)
        Controls.Add(layout)
        AddHandler globalVi.ValueChanged, AddressOf RefreshResults
        AddHandler alternatives.ValueChanged, AddressOf RefreshResults
        AddHandler iterations.ValueChanged, AddressOf RefreshResults
        RefreshResults(Nothing, EventArgs.Empty)
    End Sub
    Private Sub RefreshResults(sender As Object, e As EventArgs)
        If weights.Count <> 6 Then Return
        Dim count = CInt(alternatives.Value)
        For i = 0 To 5
            weights(i).Visible = i < count
            ratioLabels(i).Visible = i < count
        Next
        Dim w = weights.Take(count).Select(Function(control) CDbl(control.Value)).ToArray()
        Dim ideal = ConcurrentViCalculation.Intervals(CDbl(globalVi.Value), w)
        Dim rounded = ideal.Select(Function(v) Math.Max(1.0, Math.Round(v, MidpointRounding.AwayFromZero))).ToArray()
        Dim valid = True
        output.Rows.Clear()
        For i = 0 To count - 1
            Dim issue As String
            If rounded(i) > Integer.MaxValue Then
                issue = "Outside timer range"
            Else
                issue = IntervalScheduleTiming.ValidationError("Variable Interval", CInt(rounded(i)), CInt(iterations.Value))
            End If
            valid = valid AndAlso issue = ""
            Dim row = output.Rows.Add("Alt. " & (i + 1), (100 * w(i) / w.Sum()).ToString("0.###"), ideal(i).ToString("0.######"), rounded(i).ToString("0"), If(issue = "", "OK", "Out of range"))
            output.Rows(row).Cells(4).ToolTipText = issue
            If issue <> "" Then output.Rows(row).DefaultCellStyle.ForeColor = Color.Firebrick
        Next
        Dim actual = ConcurrentViCalculation.Equivalent(rounded)
        summary.Text = "Target nominal rate: " & (60.0 / CDbl(globalVi.Value)).ToString("0.######") & " ref/min." & Environment.NewLine &
            "Rounded means: combined VI = " & actual.ToString("0.######") & " s; " & (60.0 / actual).ToString("0.######") & " ref/min" &
            " (" & ((CDbl(globalVi.Value) / actual - 1) * 100).ToString("+0.###;-0.###;0") & "% rate difference)." & If(valid, "", " Not valid for these iterations.")
    End Sub
End Class