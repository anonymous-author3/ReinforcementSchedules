Public Class Main
    Inherits Form

    Private Class ChamberEntry
        Public Configuration As ChamberConfiguration
        Public Session As ChamberSession
        Public Row As DataGridViewRow
        Public RecordPanel As Panel
        Public Expanded As Boolean
    End Class

    Private ReadOnly chambers As New List(Of ChamberEntry)()
    Private ReadOnly overview As New DataGridView()
    Private ReadOnly records As New FlowLayoutPanel()
    Private ReadOnly hubLayout As New TableLayoutPanel()
    Private WithEvents refreshTimer As New Timer With {.Interval = 250}
    Private closingHub As Boolean

    Public Sub New()
        Text = "Main — Session hub"
        ClientSize = New Size(1280, 760)
        MinimumSize = New Size(980, 520)
        StartPosition = FormStartPosition.CenterScreen

        hubLayout.Dock = DockStyle.Fill
        hubLayout.ColumnCount = 1
        hubLayout.RowCount = 3
        hubLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))
        hubLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        hubLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 0))
        Dim toolbar As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(6)}
        Dim setupButton As New Button With {.Text = "Add / configure boxes", .AutoSize = True}
        AddHandler setupButton.Click, Sub()
                                          SetUp.WindowState = FormWindowState.Normal
                                          SetUp.Activate()
                                      End Sub
        toolbar.Controls.Add(setupButton)
        Dim collapseButton As New Button With {.Text = "Collapse all records", .AutoSize = True}
        AddHandler collapseButton.Click, AddressOf CollapseAll
        toolbar.Controls.Add(collapseButton)
        toolbar.Controls.Add(New Label With {
            .Text = "Counts are session totals. Each box starts independently.",
            .AutoSize = True, .Margin = New Padding(14, 7, 0, 0)
        })

        overview.Dock = DockStyle.Fill
        overview.ReadOnly = True
        overview.AllowUserToAddRows = False
        overview.AllowUserToDeleteRows = False
        overview.AllowUserToResizeRows = False
        overview.RowHeadersVisible = False
        overview.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        overview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        overview.RowTemplate.Height = 32
        For Each column As String In New String() {"Box", "COM", "Subject", "Session", "State", "Time (s)", "Component"}
            overview.Columns.Add(column, column)
        Next
        For i As Integer = 1 To MAX_INPUTS
            overview.Columns.Add("R" & i, "R" & i)
            overview.Columns("R" & i).FillWeight = 45
            overview.Columns("R" & i).ToolTipText = "Total responses on input " & i
        Next
        overview.Columns.Add("Reinforcers", "Reinforcers")
        For Each column As String In New String() {"Start", "Finish", "Record", "Remove"}
            overview.Columns.Add(New DataGridViewButtonColumn With {.Name = column, .HeaderText = column, .FillWeight = 70})
        Next
        overview.Columns("State").FillWeight = 140
        overview.Columns("Component").FillWeight = 150
        For Each column As DataGridViewColumn In overview.Columns
            column.SortMode = DataGridViewColumnSortMode.NotSortable
        Next
        AddHandler overview.CellContentClick, AddressOf OverviewClick

        records.Dock = DockStyle.Fill
        records.AutoScroll = True
        records.FlowDirection = FlowDirection.TopDown
        records.WrapContents = False
        records.Visible = False
        AddHandler records.SizeChanged, AddressOf ResizeRecords
        hubLayout.Controls.Add(toolbar, 0, 0)
        hubLayout.Controls.Add(overview, 0, 1)
        hubLayout.Controls.Add(records, 0, 2)
        Controls.Add(hubLayout)
        refreshTimer.Start()
    End Sub

    Public Function PortReserved(comPort As String) As Boolean
        Return chambers.Any(Function(entry) String.Equals(entry.Configuration.ComPort, comPort.Trim(), StringComparison.OrdinalIgnoreCase) AndAlso Not entry.Session.HasEnded)
    End Function

    Public Function AddChamber(configuration As ChamberConfiguration) As Boolean
        If PortReserved(configuration.ComPort) Then
            MessageBox.Show(Me, configuration.ComPort & " is already assigned to a ready or running box.", "Session hub")
            Return False
        End If
        If chambers.Any(Function(existing) String.Equals(existing.Configuration.BoxName, configuration.BoxName, StringComparison.OrdinalIgnoreCase)) Then
            MessageBox.Show(Me, "A box named " & configuration.BoxName & " already exists in the hub.", "Session hub")
            Return False
        End If
        Dim entry As New ChamberEntry With {.Configuration = configuration.Copy()}
        entry.Session = New ChamberSession(entry.Configuration)
        entry.RecordPanel = New Panel With {.Height = 660, .Width = 1200, .Margin = New Padding(6)}
        Dim title As New Label With {
            .Text = configuration.BoxName & " · " & configuration.ComPort & " · " & configuration.Subject & " — Cumulative record",
            .Dock = DockStyle.Top, .Height = 30, .Padding = New Padding(8), .BackColor = Color.AliceBlue
        }
        Dim sessionHost As New Panel With {.Dock = DockStyle.Fill}
        sessionHost.Controls.Add(entry.Session)
        entry.RecordPanel.Controls.Add(sessionHost)
        entry.RecordPanel.Controls.Add(title)
        Dim rowIndex As Integer = overview.Rows.Add()
        entry.Row = overview.Rows(rowIndex)
        entry.Row.Tag = entry
        chambers.Add(entry)
        RefreshEntry(entry)
        UpdateRecordLayout()
        Return True
    End Function

    Private Sub OverviewClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        Dim entry As ChamberEntry = DirectCast(overview.Rows(e.RowIndex).Tag, ChamberEntry)
        Select Case overview.Columns(e.ColumnIndex).Name
            Case "Start"
                If entry.Session.IsRunning Then Return
                If entry.Session.HasEnded Then Return
                entry.Session.StartSession()
            Case "Finish"
                If Not entry.Session.IsRunning Then Return
                If MessageBox.Show(Me, "Finish " & entry.Configuration.BoxName & "?", "Finish box", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                    entry.Session.RequestFinish()
                End If
            Case "Record"
                SetExpanded(entry, Not entry.Expanded)
            Case "Remove"
                If entry.Session.IsRunning Then
                    MessageBox.Show(Me, "Finish this box before removing it.", "Session hub")
                    Return
                End If
                records.Controls.Remove(entry.RecordPanel)
                entry.Session.Dispose()
                entry.RecordPanel.Dispose()
                overview.Rows.Remove(entry.Row)
                chambers.Remove(entry)
                UpdateRecordLayout()
                Return
        End Select
        RefreshEntry(entry)
    End Sub

    Private Sub SetExpanded(entry As ChamberEntry, expanded As Boolean)
        entry.Expanded = expanded
        If expanded Then
            records.Controls.Add(entry.RecordPanel)
            entry.Session.Show()
        Else
            records.Controls.Remove(entry.RecordPanel)
        End If
        RefreshEntry(entry)
        UpdateRecordLayout()
    End Sub

    Private Sub CollapseAll(sender As Object, e As EventArgs)
        For Each entry As ChamberEntry In chambers
            entry.Expanded = False
            records.Controls.Remove(entry.RecordPanel)
            RefreshEntry(entry)
        Next
        UpdateRecordLayout()
    End Sub

    Private Sub UpdateRecordLayout()
        Dim expanded As Boolean = chambers.Any(Function(entry) entry.Expanded)
        records.Visible = expanded
        hubLayout.RowStyles(1).SizeType = If(expanded, SizeType.Absolute, SizeType.Percent)
        hubLayout.RowStyles(1).Height = If(expanded, Math.Min(280, Math.Max(145, 48 + chambers.Count * 32)), 100)
        hubLayout.RowStyles(2).SizeType = If(expanded, SizeType.Percent, SizeType.Absolute)
        hubLayout.RowStyles(2).Height = If(expanded, 100, 0)
        ResizeRecords(Nothing, EventArgs.Empty)
    End Sub

    Private Sub ResizeRecords(sender As Object, e As EventArgs)
        For Each entry As ChamberEntry In chambers
            entry.RecordPanel.Width = Math.Max(850, records.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 18)
        Next
    End Sub

    Private Sub RefreshEntry(entry As ChamberEntry)
        Dim row As DataGridViewRow = entry.Row
        row.Cells("Box").Value = entry.Configuration.BoxName
        row.Cells("COM").Value = entry.Configuration.ComPort
        row.Cells("Subject").Value = entry.Configuration.Subject
        row.Cells("Session").Value = entry.Configuration.SessionName
        row.Cells("State").Value = entry.Session.StatusText
        row.Cells("State").ToolTipText = entry.Session.StatusText
        row.Cells("Time (s)").Value = entry.Session.ElapsedSeconds
        row.Cells("Component").Value = entry.Session.CurrentComponentText
        For i As Integer = 0 To MAX_INPUTS - 1
            row.Cells("R" & (i + 1)).Value = entry.Session.TotalResponses(i)
        Next
        row.Cells("Reinforcers").Value = entry.Session.TotalReinforcers
        row.Cells("Start").Value = If(entry.Session.IsRunning, "Running", If(entry.Session.HasEnded, "Ended", "Start"))
        row.Cells("Finish").Value = If(entry.Session.IsRunning, "Finish", "—")
        row.Cells("Record").Value = If(entry.Expanded, "Collapse", "Expand")
        row.Cells("Remove").Value = If(entry.Session.IsRunning, "—", "Remove")
    End Sub

    Private Sub RefreshOverview(sender As Object, e As EventArgs) Handles refreshTimer.Tick
        For Each entry As ChamberEntry In chambers
            RefreshEntry(entry)
        Next
    End Sub

    Private Sub HubClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If closingHub Then Return
        If chambers.Any(Function(entry) entry.Session.IsRunning) AndAlso
            MessageBox.Show(Me, "Stop all running boxes, save their data, and close the hub?", "Close hub", MessageBoxButtons.YesNo) <> DialogResult.Yes Then
            e.Cancel = True
            Return
        End If
        closingHub = True
        refreshTimer.Stop()
        For Each entry As ChamberEntry In chambers
            entry.Session.StopAndSave()
            entry.Session.Dispose()
            entry.RecordPanel.Dispose()
        Next
        refreshTimer.Dispose()
    End Sub
End Class
