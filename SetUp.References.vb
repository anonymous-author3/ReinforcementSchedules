Option Strict On

Partial Public Class SetUp
    Private Sub InitializeReferenceControls(sender As Object, e As EventArgs) Handles MyBase.Load
        If DesignMode Then Return
        Dim button As New Button With {.Name = "btnSessionReferences", .Text = "Session references: continue Lag / Yoked...", .Dock = DockStyle.Bottom, .Height = 32}
        ClientSize = New Size(ClientSize.Width, ClientSize.Height + button.Height)
        Controls.Add(button)
        AddHandler button.Click, AddressOf EditSessionReferences
    End Sub

    Private Sub EditSessionReferences(sender As Object, e As EventArgs)
        If vCC = 0 Then
            MessageBox.Show(Me, "Add or load at least one component first.", "Session references")
            Return
        End If
        Dim working = SnapshotComponents()
        Using dialog As New Form With {.Text = "Session references for " & txtSubject.Text, .StartPosition = FormStartPosition.CenterParent, .ClientSize = New Size(850, 350), .MinimizeBox = False, .MaximizeBox = False, .ShowInTaskbar = False}
            Dim component As New ComboBox With {.Name = "cbbReferenceComponent", .DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(16, 14), .Width = 500}
            For i As Integer = 1 To vCC
                component.Items.Add("Component " & i & ": " & ComponentDisplayName(i))
            Next
            Dim status As New Label With {.Name = "lblReferenceStatus", .Location = New Point(16, 58), .Size = New Size(815, 140)}
            Dim history As New Button With {.Text = "Load prior Lag history...", .Location = New Point(16, 210), .Size = New Size(190, 32)}
            Dim clearHistory As New Button With {.Text = "Clear history", .Location = New Point(215, 210), .Size = New Size(130, 32)}
            Dim yoked As New Button With {.Text = "Load Yoked reference...", .Location = New Point(415, 210), .Size = New Size(195, 32)}
            Dim clearYoked As New Button With {.Text = "Disable Yoked", .Location = New Point(620, 210), .Size = New Size(160, 32)}
            Dim apply As New Button With {.Text = "Apply references", .Location = New Point(530, 292), .Size = New Size(145, 34), .DialogResult = DialogResult.OK}
            Dim cancel As New Button With {.Text = "Cancel", .Location = New Point(690, 292), .Size = New Size(110, 34), .DialogResult = DialogResult.Cancel}
            Dim refresh As Action = Sub()
                Dim index As Integer = component.SelectedIndex + 1
                If index < 1 Then Return
                history.Enabled = working(index).LagEnabled AndAlso Not working(index).YokedEnabled
                clearHistory.Enabled = working(index).LagHistoryReference IsNot Nothing
                clearYoked.Enabled = working(index).YokedEnabled
                Dim prior = working(index).LagHistoryReference
                Dim source = working(index).YokedReference
                status.Text = "Prior history: " & If(prior Is Nothing, "none", prior.Subject & " / " & prior.SessionName & " / component " & prior.SelectedComponent & " / UID " & prior.SessionUid) & Environment.NewLine &
                    "Yoked: " & If(Not working(index).YokedEnabled OrElse source Is Nothing, "off", source.Subject & " / " & source.SessionName & " / component " & source.SelectedComponent & " / UID " & source.SessionUid) & Environment.NewLine & Environment.NewLine &
                    If(working(index).LagEnabled, "Lag Yoked copies each trial's exact feeder/pump output commands, independent of sequence. The last reference trial ends the session. Current probability, magnitude, delay, timeout and MaxRefs are bypassed; time remains a safety limit.", "Classic/T Yoked makes reference batches available at their source component times. One response on the same input collects the next available batch. Independent FT/VT references are not supported.")
            End Sub
            Dim busy As Action(Of Boolean) = Sub(value)
                component.Enabled = Not value
                apply.Enabled = Not value
                history.Enabled = Not value
                yoked.Enabled = Not value
                clearHistory.Enabled = Not value
                clearYoked.Enabled = Not value
                If Not value Then refresh()
            End Sub
            Dim selectReference As Func(Of Boolean, Task) = Async Function(useYoked As Boolean)
                Dim index As Integer = component.SelectedIndex + 1
                busy(True)
                Try
                    Dim reference = Await SessionReferencePicker.ChooseAsync(dialog, ConnectionStorage, txtSubject.Text)
                    If dialog.IsDisposed OrElse reference Is Nothing Then Return
                    Dim problem As String = If(useYoked, ReferenceConfiguration.ValidateSelection(working(index), reference), reference.ValidateHistory(working(index).LagSequenceLength, Enumerable.Range(1, MAX_INPUTS).Where(Function(n) working(index).ScheduleType(n - 1) = "Lag")))
                    If problem <> "" Then
                        MessageBox.Show(dialog, problem, "Incompatible reference", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If
                    If reference.Subject <> "" AndAlso txtSubject.Text <> "" AndAlso reference.Subject <> txtSubject.Text Then
                        If MessageBox.Show(dialog, "Reference subject is " & reference.Subject & ", while this box is " & txtSubject.Text & ". Use the selected reference?", "Confirm reference subject", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
                    End If
                    If useYoked Then
                        working(index).YokedReference = reference.Copy()
                        working(index).YokedEnabled = True
                        working(index).LagHistoryReference = Nothing
                    Else
                        working(index).LagHistoryReference = reference.Copy()
                    End If
                Catch ex As Exception
                    If Not dialog.IsDisposed Then MessageBox.Show(dialog, ex.Message, "Reference session", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Finally
                    If Not dialog.IsDisposed Then busy(False)
                End Try
            End Function
            AddHandler history.Click, Async Sub() Await selectReference(False)
            AddHandler yoked.Click, Async Sub() Await selectReference(True)
            AddHandler clearHistory.Click, Sub()
                working(component.SelectedIndex + 1).LagHistoryReference = Nothing
                refresh()
            End Sub
            AddHandler clearYoked.Click, Sub()
                working(component.SelectedIndex + 1).YokedEnabled = False
                working(component.SelectedIndex + 1).YokedReference = Nothing
                refresh()
            End Sub
            AddHandler component.SelectedIndexChanged, Sub() refresh()
            dialog.Controls.AddRange(New Control() {component, status, history, clearHistory, yoked, clearYoked, apply, cancel})
            dialog.AcceptButton = apply
            dialog.CancelButton = cancel
            component.SelectedIndex = 0
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            RestoreComponents(working)
            RefreshComponentSummaryTable()
        End Using
    End Sub
End Class
