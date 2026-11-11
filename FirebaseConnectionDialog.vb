Imports System.Threading

Friend Class FirebaseConnectionDialog
    Inherits Form

    Private ReadOnly storage As CloudStorage
    Private ReadOnly urlBox As New TextBox With {.Dock = DockStyle.Fill}
    Private ReadOnly keyBox As New TextBox With {.Dock = DockStyle.Fill}
    Private ReadOnly emailBox As New TextBox With {.Dock = DockStyle.Fill}
    Private ReadOnly passwordBox As New TextBox With {.Dock = DockStyle.Fill, .UseSystemPasswordChar = True}
    Private ReadOnly connectButton As New Button With {.Text = "Connect", .AutoSize = True}
    Private ReadOnly localButton As New Button With {.Text = "Use local only", .AutoSize = True}
    Private ReadOnly statusLabel As New Label With {.Dock = DockStyle.Fill, .AutoSize = True}
    Private ReadOnly cancellation As New CancellationTokenSource()

    Public Sub New(service As CloudStorage)
        storage = service
        Text = "Connect to Firebase RTDB (optional)"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(570, 345)
        Dim layout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(16), .ColumnCount = 2, .RowCount = 7}
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 125))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 64))
        For i = 1 To 4
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
        Next
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
        Dim help As New Label With {.Text = "Connect once; this Windows account will remember your RTDB session. Your password is not saved. Use local only to sign out and disable automatic connection.",
            .Dock = DockStyle.Fill}
        layout.Controls.Add(help, 0, 0)
        layout.SetColumnSpan(help, 2)
        Dim labels = New String() {"RTDB URL", "Web API key", "Email", "Password"}
        Dim fields = New TextBox() {urlBox, keyBox, emailBox, passwordBox}
        For i = 0 To fields.Length - 1
            layout.Controls.Add(New Label With {.Text = labels(i), .AutoSize = True, .Anchor = AnchorStyles.Left}, 0, i + 1)
            fields(i).TabIndex = i
            layout.Controls.Add(fields(i), 1, i + 1)
        Next
        layout.Controls.Add(statusLabel, 0, 5)
        layout.SetColumnSpan(statusLabel, 2)
        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
        buttons.Controls.Add(connectButton)
        buttons.Controls.Add(localButton)
        layout.Controls.Add(buttons, 0, 6)
        layout.SetColumnSpan(buttons, 2)
        Controls.Add(layout)
        AcceptButton = connectButton
        CancelButton = localButton
        localButton.DialogResult = DialogResult.None
        Dim saved = storage.SavedConnection()
        If saved.ContainsKey("databaseUrl") Then urlBox.Text = CStr(saved("databaseUrl"))
        If saved.ContainsKey("apiKey") Then keyBox.Text = CStr(saved("apiKey"))
        If saved.ContainsKey("email") Then emailBox.Text = CStr(saved("email"))
        statusLabel.Text = storage.Status
        AddHandler connectButton.Click, AddressOf ConnectClick
        AddHandler localButton.Click, AddressOf LocalOnlyClick
        AddHandler FormClosed, Sub()
                                   cancellation.Cancel()
                                   passwordBox.Clear()
                               End Sub
    End Sub

    Private Sub LocalOnlyClick(sender As Object, e As EventArgs)
        Try
            cancellation.Cancel()
            storage.Disconnect()
            DialogResult = DialogResult.Cancel
            Close()
        Catch ex As Exception
            statusLabel.Text = "Local mode is active, but the saved sign-in could not be cleared. " & ex.Message
        End Try
    End Sub

    Private Async Sub ConnectClick(sender As Object, e As EventArgs)
        connectButton.Enabled = False
        For Each field In New TextBox() {urlBox, keyBox, emailBox, passwordBox}
            field.ReadOnly = True
        Next
        statusLabel.Text = "Signing in and checking RTDB access…"
        Try
            Await storage.ConnectAsync(urlBox.Text, keyBox.Text, emailBox.Text, passwordBox.Text, cancellation.Token)
            If IsDisposed OrElse cancellation.IsCancellationRequested Then Return
            DialogResult = DialogResult.OK
            Close()
        Catch ex As OperationCanceledException
        Catch ex As Exception
            If Not IsDisposed Then statusLabel.Text = ex.Message
        Finally
            If Not IsDisposed Then
                passwordBox.Clear()
                connectButton.Enabled = True
                For Each field In New TextBox() {urlBox, keyBox, emailBox, passwordBox}
                    field.ReadOnly = False
                Next
                passwordBox.Focus()
            End If
        End Try
    End Sub
End Class
