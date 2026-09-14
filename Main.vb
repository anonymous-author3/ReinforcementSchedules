Public Class Main

    Private Class ChamberEntry
        Public Configuration As ChamberConfiguration
        Public Session As ChamberSession
        Public Row As DataGridViewRow
        Public Expanded As Boolean
    End Class

    Private ReadOnly chambers As New List(Of ChamberEntry)()
    Private ReadOnly expandedOverviewStyle As RowStyle
    Private ReadOnly expandedRecordsStyle As RowStyle
    Private closingHub As Boolean

    Public Sub New()
        InitializeComponent()
        expandedOverviewStyle = New RowStyle(hubLayout.RowStyles(1).SizeType, hubLayout.RowStyles(1).Height)
        expandedRecordsStyle = New RowStyle(hubLayout.RowStyles(2).SizeType, hubLayout.RowStyles(2).Height)
    End Sub

    Private Sub HubLoad(sender As Object, e As EventArgs) Handles MyBase.Load
        If DesignMode OrElse System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then Return
        'The designer shows the reusable card; real sessions are added from SetUp.
        records.Controls.Remove(recordPreview)
        recordPreview.Dispose()
        UpdateRecordLayout()
        refreshTimer.Start()
    End Sub

    Private Sub OpenSetup(sender As Object, e As EventArgs) Handles setupButton.Click
        SetUp.WindowState = FormWindowState.Normal
        SetUp.Activate()
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
        Dim rowIndex As Integer = overview.Rows.Add()
        entry.Row = overview.Rows(rowIndex)
        entry.Row.Tag = entry
        chambers.Add(entry)
        RefreshEntry(entry)
        UpdateRecordLayout()
        Return True
    End Function

    Private Sub OverviewClick(sender As Object, e As DataGridViewCellEventArgs) Handles overview.CellContentClick
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
                records.Controls.Remove(entry.Session)
                entry.Session.Dispose()
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
            records.Controls.Add(entry.Session)
            entry.Session.Show()
        Else
            records.Controls.Remove(entry.Session)
        End If
        RefreshEntry(entry)
        UpdateRecordLayout()
    End Sub

    Private Sub CollapseAll(sender As Object, e As EventArgs) Handles collapseButton.Click
        For Each entry As ChamberEntry In chambers
            entry.Expanded = False
            records.Controls.Remove(entry.Session)
            RefreshEntry(entry)
        Next
        UpdateRecordLayout()
    End Sub

    Private Sub UpdateRecordLayout()
        Dim expanded As Boolean = chambers.Any(Function(entry) entry.Expanded)
        hubLayout.SuspendLayout()
        records.Visible = expanded
        'Restore the split chosen in Design when a record is expanded.
        hubLayout.RowStyles(1).SizeType = If(expanded, expandedOverviewStyle.SizeType, SizeType.Percent)
        hubLayout.RowStyles(1).Height = If(expanded, expandedOverviewStyle.Height, 100)
        hubLayout.RowStyles(2).SizeType = If(expanded, expandedRecordsStyle.SizeType, SizeType.Absolute)
        hubLayout.RowStyles(2).Height = If(expanded, expandedRecordsStyle.Height, 0)
        hubLayout.ResumeLayout(True)
        ResizeRecords(Nothing, EventArgs.Empty)
    End Sub

    Private Sub ResizeRecords(sender As Object, e As EventArgs) Handles records.SizeChanged
        For Each entry As ChamberEntry In chambers
            entry.Session.Width = Math.Max(entry.Session.MinimumSize.Width,
                records.ClientSize.Width - records.Padding.Horizontal -
                SystemInformation.VerticalScrollBarWidth - entry.Session.Margin.Horizontal)
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
        Next
        refreshTimer.Dispose()
    End Sub
End Class
