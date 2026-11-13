Imports System.Drawing
Imports System.Windows.Forms

Partial Public Class Component
    Private inputRandomSelection As New ReinforcerSelectionDefinition With {.Feeder1Percent = 50, .Pump1Percent = 50}
    Private timeRandomSelection As New ReinforcerSelectionDefinition With {.Feeder1Percent = 50, .Pump1Percent = 50}
    Private inputDistributionButton As Button
    Private timeDistributionButton As Button

    Private Sub InitializeReinforcerEditors()
        If inputDistributionButton IsNot Nothing Then Return
        cbbReinforcer1.Items.Clear()
        cbbReinforcer1.Items.AddRange(New Object() {"Feeder 1", "Feeder 2", "Pump 1", "Pump 2", "Random", "N/A"})
        cbbTimeReinforcer.Items.Clear()
        cbbTimeReinforcer.Items.AddRange(New Object() {"Feeder 1", "Feeder 2", "Pump 1", "Pump 2", "Random"})
        cbbTimeReinforcer.SelectedItem = "Feeder 1"
        Label7.Visible = False
        txbPelletProbability1.Visible = False
        lblTimePelletProbability.Visible = False
        txbTimePelletProbability.Visible = False
        inputDistributionButton = New Button With {.Name = "btnInputDistribution", .Text = "Distribution...", .Location = New Point(283, 25), .Size = New Size(132, 26), .Visible = False}
        timeDistributionButton = New Button With {.Name = "btnTimeDistribution", .Text = "Distribution...", .Location = New Point(15, 174), .Size = New Size(165, 26), .Visible = False}
        grpMagnitude.Controls.Add(inputDistributionButton)
        grpTimeSchedule.Controls.Add(timeDistributionButton)
        AddHandler inputDistributionButton.Click, Sub() EditReinforcerDistribution(False)
        AddHandler timeDistributionButton.Click, Sub() EditReinforcerDistribution(True)
        AddHandler cbbReinforcer1.SelectionChangeCommitted, Sub()
            If cbbReinforcer1.Text = "Random" Then EditReinforcerDistribution(False)
        End Sub
        AddHandler cbbTimeReinforcer.SelectionChangeCommitted, Sub()
            If cbbTimeReinforcer.Text = "Random" Then EditReinforcerDistribution(True)
        End Sub
        AddHandler cbbReinforcer1.SelectedIndexChanged, Sub() UpdateReinforcerDistributionButtons()
        AddHandler cbbTimeReinforcer.SelectedIndexChanged, Sub() UpdateReinforcerDistributionButtons()
        ComponentToolTip.SetToolTip(cbbReinforcer1, "Choose one feeder/pump output, or Random to select outputs and their delivery percentages.")
        ComponentToolTip.SetToolTip(cbbTimeReinforcer, "Choose one feeder/pump output, or Random to select outputs and their delivery percentages.")
        UpdateReinforcerDistributionButtons()
    End Sub

    Private Sub UpdateReinforcerDistributionButtons()
        If inputDistributionButton Is Nothing Then Return
        inputDistributionButton.Visible = cbbReinforcer1.Text = "Random"
        timeDistributionButton.Visible = cbbTimeReinforcer.Text = "Random"
        timeDistributionButton.Enabled = chkFixedTime.Checked OrElse chkVariableTime.Checked
        ComponentToolTip.SetToolTip(inputDistributionButton, ReinforcerConfiguration.Summary(inputRandomSelection))
        ComponentToolTip.SetToolTip(timeDistributionButton, ReinforcerConfiguration.Summary(timeRandomSelection))
    End Sub

    Private Sub EditReinforcerDistribution(forTime As Boolean)
        Using editor As New ReinforcerDistributionEditor(If(forTime, timeRandomSelection, inputRandomSelection))
            If editor.ShowDialog(Me) <> DialogResult.OK Then Return
            If forTime Then
                timeRandomSelection = editor.Selection.Copy()
            Else
                inputRandomSelection = editor.Selection.Copy()
            End If
        End Using
        UpdateReinforcerDistributionButtons()
    End Sub

    Private Function BuildReinforcerSelection(kind As String, forTime As Boolean) As ReinforcerSelectionDefinition
        If kind = "N/A" OrElse String.IsNullOrEmpty(kind) Then Return Nothing
        If kind = "Random" Then Return If(forTime, timeRandomSelection, inputRandomSelection).Copy()
        Return Nothing
    End Function

    Private Sub ResetInputReinforcerSelection()
        If cbbReinforcer1.Items.Contains("Feeder 1") Then cbbReinforcer1.SelectedItem = "Feeder 1"
        inputRandomSelection = New ReinforcerSelectionDefinition With {.Feeder1Percent = 50, .Pump1Percent = 50}
        UpdateReinforcerDistributionButtons()
    End Sub

    Private Class ReinforcerDistributionEditor
        Inherits Form
        Private ReadOnly choices(3) As CheckBox
        Private ReadOnly percentages(3) As NumericUpDown
        Private ReadOnly totalLabel As New Label
        Private ReadOnly acceptButtonControl As New Button
        Public Property Selection As ReinforcerSelectionDefinition

        Public Sub New(initial As ReinforcerSelectionDefinition)
            Text = "Random reinforcer distribution"
            ClientSize = New Size(390, 265)
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            StartPosition = FormStartPosition.CenterParent
            ShowInTaskbar = False
            Controls.Add(New Label With {.Text = "Select the outputs and their percentages (total = 100%).", .AutoSize = False, .Location = New Point(15, 14), .Size = New Size(360, 32)})
            Dim names = {"Feeder 1", "Feeder 2", "Pump 1", "Pump 2"}
            Dim values = {initial.Feeder1Percent, initial.Feeder2Percent, initial.Pump1Percent, initial.Pump2Percent}
            For i = 0 To 3
                Dim index = i
                choices(i) = New CheckBox With {.Text = names(i), .Checked = values(i) > 0, .Location = New Point(20, 55 + i * 32), .Size = New Size(140, 24)}
                percentages(i) = New NumericUpDown With {.Minimum = 0, .Maximum = 100, .Value = values(i), .Enabled = values(i) > 0, .Location = New Point(190, 55 + i * 32), .Width = 90}
                Controls.Add(choices(i))
                Controls.Add(percentages(i))
                Controls.Add(New Label With {.Text = "%", .AutoSize = True, .Location = New Point(288, 59 + i * 32)})
                AddHandler choices(i).CheckedChanged, Sub()
                    percentages(index).Enabled = choices(index).Checked
                    If Not choices(index).Checked Then percentages(index).Value = 0
                    UpdateTotal()
                End Sub
                AddHandler percentages(i).ValueChanged, Sub() UpdateTotal()
            Next
            totalLabel.Location = New Point(20, 189)
            totalLabel.Size = New Size(350, 24)
            Controls.Add(totalLabel)
            acceptButtonControl.Text = "OK"
            acceptButtonControl.Location = New Point(205, 224)
            acceptButtonControl.Size = New Size(75, 26)
            Dim cancelButtonControl As New Button With {.Text = "Cancel", .Location = New Point(290, 224), .Size = New Size(75, 26), .DialogResult = DialogResult.Cancel}
            Controls.Add(acceptButtonControl)
            Controls.Add(cancelButtonControl)
            AcceptButton = acceptButtonControl
            CancelButton = cancelButtonControl
            AddHandler acceptButtonControl.Click, Sub()
                Dim result = CurrentSelection()
                Dim problem = ReinforcerConfiguration.ValidateSelection(result)
                If Not String.IsNullOrEmpty(problem) Then
                    MessageBox.Show(Me, problem, "Invalid distribution", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
                Selection = result
                DialogResult = DialogResult.OK
            End Sub
            UpdateTotal()
        End Sub

        Private Function CurrentSelection() As ReinforcerSelectionDefinition
            Return New ReinforcerSelectionDefinition With {.Feeder1Percent = CInt(percentages(0).Value), .Feeder2Percent = CInt(percentages(1).Value), .Pump1Percent = CInt(percentages(2).Value), .Pump2Percent = CInt(percentages(3).Value)}
        End Function

        Private Sub UpdateTotal()
            If percentages.Any(Function(item) item Is Nothing) Then Return
            Dim total = percentages.Sum(Function(item) item.Value)
            totalLabel.Text = "Total: " & total & "%" & If(total = 100, "", " — must equal 100%")
            totalLabel.ForeColor = If(total = 100, Color.DarkGreen, Color.Firebrick)
            acceptButtonControl.Enabled = total = 100
        End Sub
    End Class
End Class
