Partial Public Class SetUp
    Friend Property ConnectionStorage As CloudStorage = CloudStorage.Current
    Private cloudStatus As Label
    Private cloudLoadButton As Button
    Private cloudSyncButton As Button
    Private cloudLoading As Boolean
    Private WithEvents cloudTimer As New Timer With {.Interval = 60000}

    Private Sub ConfigureCloudControls()
        If DesignMode Then Return
        Dim footer As New TableLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 62, .Padding = New Padding(10, 4, 10, 4), .ColumnCount = 3, .RowCount = 2}
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 135))
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 135))
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        footer.RowStyles.Add(New RowStyle(SizeType.Absolute, 30))
        footer.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        Dim configure As New Button With {.Text = "Configure RTDB…", .Dock = DockStyle.Fill}
        cloudLoadButton = New Button With {.Text = "Load from RTDB…", .Dock = DockStyle.Fill}
        cloudSyncButton = New Button With {.Text = "Sync pending files", .Width = 150, .Dock = DockStyle.Left}
        cloudStatus = New Label With {.Text = "Local only", .Dock = DockStyle.Fill, .AutoEllipsis = True}
        footer.Controls.Add(configure, 0, 0)
        footer.Controls.Add(cloudLoadButton, 1, 0)
        footer.Controls.Add(cloudSyncButton, 2, 0)
        footer.Controls.Add(cloudStatus, 0, 1)
        footer.SetColumnSpan(cloudStatus, 3)
        ClientSize = New Size(ClientSize.Width, ClientSize.Height + footer.Height)
        Controls.Add(footer)
        AddHandler configure.Click, AddressOf ConfigureRtdbClick
        AddHandler cloudLoadButton.Click, AddressOf LoadRtdbClick
        AddHandler cloudSyncButton.Click, AddressOf SyncRtdbClick
        AddHandler ConnectionStorage.StatusChanged, AddressOf CloudStatusChanged
        RefreshCloudStatus()
        cloudTimer.Start()
    End Sub

    Private Async Sub SetupShown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If DesignMode Then Return
        Dim restored = Await ConnectionStorage.RestoreSessionAsync()
        If IsDisposed OrElse Disposing Then Return
        If restored = SessionRestoreResult.NoSession OrElse restored = SessionRestoreResult.SignInRequired Then
            ConfigureRtdbClick(Me, EventArgs.Empty)
        ElseIf restored = SessionRestoreResult.Connected Then
            Await ConnectionStorage.SynchronizeAsync()
        End If
    End Sub

    Private Async Sub ConfigureRtdbClick(sender As Object, e As EventArgs)
        Using dialog As New FirebaseConnectionDialog(ConnectionStorage)
            If dialog.ShowDialog(Me) = DialogResult.OK Then Await ConnectionStorage.SynchronizeAsync()
        End Using
    End Sub

    Private Sub RefreshCloudStatus()
        If cloudStatus Is Nothing OrElse IsDisposed Then Return
        cloudStatus.Text = ConnectionStorage.Status
        SetUpToolTip.SetToolTip(cloudStatus, cloudStatus.Text)
        cloudLoadButton.Enabled = ConnectionStorage.IsConnected AndAlso Not cloudLoading
        cloudSyncButton.Enabled = ConnectionStorage.IsConnected
    End Sub

    Private Sub CloudStatusChanged(sender As Object, e As EventArgs)
        If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
        Try
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf RefreshCloudStatus))
            Else
                RefreshCloudStatus()
            End If
        Catch ex As InvalidOperationException
            'The form may close while an upload completes.
        End Try
    End Sub

    Private Async Sub SyncRtdbClick(sender As Object, e As EventArgs) Handles cloudTimer.Tick
        Await ConnectionStorage.SynchronizeAsync()
    End Sub

    Private Async Sub LoadRtdbClick(sender As Object, e As EventArgs)
        cloudLoading = True
        cloudLoadButton.Enabled = False
        Try
            Dim entries = Await ConnectionStorage.ListSetupsAsync()
            If IsDisposed Then Return
            If entries.Count = 0 Then
                MessageBox.Show(Me, "There are no setups in this RTDB yet. Save a local setup while connected to upload it.", "Load from RTDB")
                Return
            End If
            Using picker As New Form With {.Text = "Load setup from RTDB", .StartPosition = FormStartPosition.CenterParent,
                .ClientSize = New Size(660, 360), .MinimizeBox = False, .MaximizeBox = False, .ShowInTaskbar = False}
                Dim list As New ListBox With {.Dock = DockStyle.Fill, .HorizontalScrollbar = True}
                For Each entry In entries
                    list.Items.Add(entry)
                Next
                list.SelectedIndex = 0
                Dim load As New Button With {.Text = "Load selected setup", .Dock = DockStyle.Bottom, .Height = 38, .DialogResult = DialogResult.OK}
                picker.Controls.Add(list)
                picker.Controls.Add(load)
                picker.AcceptButton = load
                If picker.ShowDialog(Me) <> DialogResult.OK OrElse list.SelectedItem Is Nothing Then Return
                Dim file = Await ConnectionStorage.DownloadSetupAsync(DirectCast(list.SelectedItem, CloudSetupInfo))
                If IsDisposed Then Return
                LoadSetupFile(file)
            End Using
        Catch ex As Exception
            If Not IsDisposed Then MessageBox.Show(Me, "Could not load from RTDB. You can still use Load for local files." & Environment.NewLine & ex.Message,
                "Load from RTDB", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            cloudLoading = False
            RefreshCloudStatus()
        End Try
    End Sub

    Private Sub DisposeCloudControls(sender As Object, e As EventArgs) Handles MyBase.FormClosed
        cloudTimer.Dispose()
        ConnectionStorage.SuspendConnection()
        RemoveHandler ConnectionStorage.StatusChanged, AddressOf CloudStatusChanged
    End Sub
End Class
