Option Strict On
Imports System.IO
Imports System.Windows.Forms

Friend NotInheritable Class SessionReferencePicker
    Private Sub New()
    End Sub

    Public Shared Async Function ChooseAsync(owner As IWin32Window, storage As CloudStorage, subject As String) As Task(Of SessionReferenceData)
        Dim reference As SessionReferenceData = Nothing
        If storage.IsConnected Then
            Using picker As New Form With {.Text = "Choose reference session", .StartPosition = FormStartPosition.CenterParent, .ClientSize = New Size(860, 410), .MinimizeBox = False, .MaximizeBox = False, .ShowInTaskbar = False}
                Dim list As New ListBox With {.Dock = DockStyle.Fill, .HorizontalScrollbar = True}
                Dim status As New Label With {.Dock = DockStyle.Top, .Height = 42, .Text = "Loading RTDB sessions. Suggestions match the subject exactly; select explicitly. Local files are also available."}
                Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 44}
                Dim choose As New Button With {.Text = "Use selected session", .Width = 160, .Enabled = False}
                Dim local As New Button With {.Text = "Choose local file...", .Width = 145}
                Dim cancel As New Button With {.Text = "Cancel", .DialogResult = DialogResult.Cancel}
                buttons.Controls.AddRange(New Control() {choose, local, cancel})
                picker.Controls.Add(list)
                picker.Controls.Add(status)
                picker.Controls.Add(buttons)
                picker.CancelButton = cancel
                AddHandler list.SelectedIndexChanged, Sub() choose.Enabled = list.SelectedItem IsNot Nothing
                AddHandler picker.Shown, Async Sub()
                    Try
                        Dim entries = Await storage.ListSessionsAsync(subject)
                        If picker.IsDisposed Then Return
                        For Each entry In entries
                            list.Items.Add(entry)
                        Next
                        status.Text = If(entries.Count = 0, "No RTDB sessions found. Choose a local file.", "Select a session. Suggested means the newest exact subject match, not verified compatibility.")
                    Catch ex As Exception
                        If Not picker.IsDisposed Then status.Text = "RTDB unavailable: " & ex.Message & " Choose a local file or cancel."
                    End Try
                End Sub
                AddHandler choose.Click, Async Sub()
                    If list.SelectedItem Is Nothing Then Return
                    Dim info = DirectCast(list.SelectedItem, CloudSessionInfo)
                    choose.Enabled = False
                    local.Enabled = False
                    list.Enabled = False
                    Try
                        Dim loaded = Await storage.ReadSessionReferenceAsync(info)
                        If picker.IsDisposed Then Return
                        reference = loaded
                        picker.DialogResult = DialogResult.OK
                        picker.Close()
                    Catch ex As Exception
                        If Not picker.IsDisposed Then status.Text = ex.Message
                    Finally
                        If Not picker.IsDisposed Then
                            choose.Enabled = list.SelectedItem IsNot Nothing
                            local.Enabled = True
                            list.Enabled = True
                        End If
                    End Try
                End Sub
                AddHandler local.Click, Async Sub()
                    choose.Enabled = False
                    local.Enabled = False
                    list.Enabled = False
                    Try
                        Dim loaded = Await ChooseLocalAsync(picker)
                        If picker.IsDisposed OrElse loaded Is Nothing Then Return
                        reference = loaded
                        picker.DialogResult = DialogResult.OK
                        picker.Close()
                    Catch ex As Exception
                        If Not picker.IsDisposed Then status.Text = ex.Message
                    Finally
                        If Not picker.IsDisposed Then
                            choose.Enabled = list.SelectedItem IsNot Nothing
                            local.Enabled = True
                            list.Enabled = True
                        End If
                    End Try
                End Sub
                If picker.ShowDialog(owner) <> DialogResult.OK Then Return Nothing
            End Using
        Else
            reference = Await ChooseLocalAsync(owner)
        End If
        If reference Is Nothing Then Return Nothing
        Dim components = reference.ComponentNumbers().ToArray()
        If components.Length = 0 Then Throw New InvalidDataException("The session contains no completed sequences or reinforcement commands.")
        Using picker As New Form With {.Text = "Choose reference component", .StartPosition = FormStartPosition.CenterParent, .ClientSize = New Size(730, 290), .MinimizeBox = False, .MaximizeBox = False, .ShowInTaskbar = False}
            Dim info As New Label With {.Dock = DockStyle.Top, .Height = 74, .Text = "Subject: " & reference.Subject & " | Session: " & reference.SessionName & Environment.NewLine & "UID: " & reference.SessionUid & Environment.NewLine & reference.SourceDescription, .AutoEllipsis = True}
            Dim list As New ListBox With {.Dock = DockStyle.Fill}
            For Each number As Integer In components
                list.Items.Add("Component " & number & ": " & reference.Sequences.Where(Function(s) s.Component = number).Count() & " completed sequences; " & reference.Deliveries.Where(Function(d) d.Component = number).Count() & " reinforcement commands")
            Next
            Dim choose As New Button With {.Text = "Use selected component", .Dock = DockStyle.Bottom, .Height = 36, .DialogResult = DialogResult.OK, .Enabled = False}
            AddHandler list.SelectedIndexChanged, Sub() choose.Enabled = list.SelectedIndex >= 0
            picker.Controls.Add(list)
            picker.Controls.Add(info)
            picker.Controls.Add(choose)
            picker.AcceptButton = choose
            If picker.ShowDialog(owner) <> DialogResult.OK OrElse list.SelectedIndex < 0 Then Return Nothing
            Return reference.SelectComponent(components(list.SelectedIndex))
        End Using
    End Function

    Private Shared Async Function ChooseLocalAsync(owner As IWin32Window) As Task(Of SessionReferenceData)
        Using dialog As New OpenFileDialog With {.Title = "Choose previous session raw, summary, or Lag sequence CSV", .Filter = "Session files (*.txt;*.csv)|*.txt;*.csv|All files (*.*)|*.*", .CheckFileExists = True, .Multiselect = False, .RestoreDirectory = True, .InitialDirectory = If(Directory.Exists("C:\Data\Summary"), "C:\Data\Summary", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))}
            If dialog.ShowDialog(owner) <> DialogResult.OK Then Return Nothing
            Dim file As String = dialog.FileName
            Try
                Return Await Task.Run(Function() SessionReferenceData.ReadLocal(file))
            Catch ex As Exception
                Dim control As Control = TryCast(owner, Control)
                If control Is Nothing OrElse (Not control.IsDisposed AndAlso Not control.Disposing) Then
                    MessageBox.Show(owner, "Could not use this session: " & ex.Message, "Reference session", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
                Return Nothing
            End Try
        End Using
    End Function
End Class
