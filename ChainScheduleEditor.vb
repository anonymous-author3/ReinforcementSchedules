Option Strict On
Imports System.Drawing
Imports System.Windows.Forms

Friend Class ChainScheduleEditor
    Inherits Form
    Private ReadOnly links As New List(Of ClassicChainLink)
    Private ReadOnly list As New ListBox With {.Dock = DockStyle.Fill, .HorizontalScrollbar = True}
    Private ReadOnly kind As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 190}
    Private ReadOnly value As New NumericUpDown With {.Minimum = 0.001D, .Maximum = 2147483647D, .DecimalPlaces = 3, .Value = 1, .Width = 130}
    Private ReadOnly count As New NumericUpDown With {.Minimum = 1, .Maximum = 1000, .Value = 10, .Width = 90}
    Private ReadOnly stimuli As New CheckedListBox With {.CheckOnClick = True, .Height = 110, .Width = 240}
    Private ReadOnly light As New NumericUpDown With {.Minimum = 0, .Maximum = 2147483, .DecimalPlaces = 3, .Width = 130}
    Private ReadOnly tone As New NumericUpDown With {.Minimum = 0, .Maximum = 2147483, .DecimalPlaces = 3, .Width = 130}
    Private ReadOnly retract As New CheckBox With {.Text = "Retract input during this link", .AutoSize = True}
    Private ReadOnly tip As New ToolTip
    Private changing As Boolean
    Private editing As Integer = -1
    Public Property Result As ClassicChainDefinition
    Public Property SimpleSchedule As ClassicChainLink

    Public Sub New(initial As ClassicChainDefinition)
        Text = "Classic schedule chain"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(830, 620)
        MinimumSize = New Size(780, 600)
        Font = New Font("Segoe UI", 9)
        kind.Items.AddRange(ClassicChainConfiguration.Kinds.Cast(Of Object)().ToArray())
        stimuli.Items.AddRange(ClassicChainConfiguration.Outputs.Cast(Of Object)().ToArray())
        If initial IsNot Nothing AndAlso initial.Links IsNot Nothing Then links.AddRange(initial.Copy().Links)
        Dim root As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 3, .Padding = New Padding(12)}
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 48))
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 52))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 45))
        Dim explanation As New Label With {.Text = "Links run in order. Only the last link produces the input's reinforcer, then the chain restarts. FT/VT links advance without a response.", .Dock = DockStyle.Fill}
        root.Controls.Add(explanation, 0, 0)
        root.SetColumnSpan(explanation, 2)
        Dim leftPanel As New TableLayoutPanel With {.Dock = DockStyle.Fill, .RowCount = 2, .ColumnCount = 1}
        leftPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        leftPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 70))
        leftPanel.Controls.Add(list, 0, 0)
        Dim actions As New FlowLayoutPanel With {.Dock = DockStyle.Fill}
        AddButton(actions, "+ Link", Sub() AddLink())
        AddButton(actions, "Remove", Sub() RemoveLink())
        AddButton(actions, "Up", Sub() MoveLink(-1))
        AddButton(actions, "Down", Sub() MoveLink(1))
        leftPanel.Controls.Add(actions, 0, 1)
        root.Controls.Add(leftPanel, 0, 1)
        Dim fields As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 8, .Padding = New Padding(12, 0, 0, 0)}
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 135))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        AddField(fields, 0, "Schedule", kind)
        AddField(fields, 1, "Value (s / responses)", value)
        AddField(fields, 2, "Series values", count)
        AddField(fields, 3, "Link signals", stimuli, 120)
        AddField(fields, 4, "Light on/off (s)", light)
        AddField(fields, 5, "Tone on/off (s)", tone)
        AddField(fields, 6, "FT / VT input", retract)
        Dim info As New Label With {.Text = "0 = continuous signal. Positive values are the duration of each ON and OFF part. Signals stop when the link ends.", .Dock = DockStyle.Fill}
        fields.Controls.Add(info, 0, 7)
        fields.SetColumnSpan(info, 2)
        root.Controls.Add(fields, 1, 1)
        Dim bottom As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
        Dim cancel As New Button With {.Text = "Cancel", .DialogResult = DialogResult.Cancel, .AutoSize = True}
        bottom.Controls.Add(cancel)
        Dim save As New Button With {.Text = "Use chain", .AutoSize = True}
        AddHandler save.Click, Sub() SaveChain()
        bottom.Controls.Add(save)
        Dim simple As New Button With {.Text = "Use simple schedule", .AutoSize = True}
        AddHandler simple.Click, Sub() UseSimpleSchedule()
        tip.SetToolTip(simple, "Keep the first link and remove later links. FT/VT, retraction and link signals remain in a one-link chain so their behavior is preserved.")
        bottom.Controls.Add(simple)
        root.Controls.Add(bottom, 0, 2)
        root.SetColumnSpan(bottom, 2)
        Controls.Add(root)
        CancelButton = cancel
        AcceptButton = save
        AddHandler list.SelectedIndexChanged, Sub() SelectLink()
        AddHandler kind.SelectedIndexChanged, Sub()
                                                  If changing Then Return
                                                  count.Enabled = kind.Text.StartsWith("Variable")
                                                  retract.Enabled = kind.Text = "Fixed Time" OrElse kind.Text = "Variable Time"
                                                  If Not retract.Enabled Then retract.Checked = False
                                              End Sub
        tip.SetToolTip(retract, "FT/VT advance independently of responding. Check to retract this input during this link; unchecked keeps it available.")
        tip.SetToolTip(count, "Fleshler-Hoffman series size, sampled without replacement. Independent for each variable link.")
        tip.SetToolTip(light, "Seconds ON and seconds OFF; zero means continuously on while this link is active. Nonzero periods must be at least 0.05 s; timing is software-based.")
        tip.SetToolTip(tone, "Seconds ON and seconds OFF; zero means continuously on while this link is active. Nonzero periods must be at least 0.05 s; timing is software-based.")
        If links.Count = 0 Then links.Add(New ClassicChainLink)
        RefreshList(0)
    End Sub
    Private Shared Sub AddField(panel As TableLayoutPanel, row As Integer, label As String, control As Control, Optional height As Integer = 37)
        panel.RowStyles.Add(New RowStyle(SizeType.Absolute, height))
        panel.Controls.Add(New Label With {.Text = label, .AutoSize = True, .Margin = New Padding(0, 7, 2, 0)}, 0, row)
        panel.Controls.Add(control, 1, row)
    End Sub
    Private Shared Sub AddButton(panel As FlowLayoutPanel, text As String, action As Action)
        Dim button As New Button With {.Text = text, .AutoSize = True}
        AddHandler button.Click, Sub() action()
        panel.Controls.Add(button)
    End Sub
    Private Function ReadEditor() As ClassicChainLink
        Dim selected = stimuli.CheckedItems.Cast(Of String)().ToArray()
        Return New ClassicChainLink With {.ScheduleType = kind.Text, .Value = CDbl(value.Value), .VariableValueCount = CInt(count.Value), .StimulusType = If(selected.Length = 0, "None", String.Join(" + ", selected)), .LightIntermittency = CDbl(light.Value), .ToneIntermittency = CDbl(tone.Value), .RetractInput = retract.Checked}
    End Function
    Private Function CommitEditor() As Boolean
        If editing < 0 OrElse editing >= links.Count Then Return True
        Dim item = ReadEditor()
        Dim problem = ClassicChainConfiguration.LinkError(item)
        If problem <> "" Then
            MessageBox.Show(Me, problem, "Invalid link", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        links(editing) = item
        changing = True
        If editing < list.Items.Count Then list.Items(editing) = (editing + 1) & ". " & item.ScheduleType & " " & item.Value & " | " & item.StimulusType
        changing = False
        Return True
    End Function
    Private Sub SelectLink()
        If changing Then Return
        Dim nextIndex = list.SelectedIndex
        If Not CommitEditor() Then
            changing = True
            list.SelectedIndex = editing
            changing = False
            Return
        End If
        LoadLink(nextIndex)
    End Sub
    Private Sub LoadLink(index As Integer)
        changing = True
        editing = index
        If index >= 0 AndAlso index < links.Count Then
            Dim item = links(index)
            kind.SelectedItem = item.ScheduleType
            value.Value = CDec(item.Value)
            count.Value = item.VariableValueCount
            light.Value = CDec(item.LightIntermittency)
            tone.Value = CDec(item.ToneIntermittency)
            retract.Checked = item.RetractInput
            retract.Enabled = item.ScheduleType = "Fixed Time" OrElse item.ScheduleType = "Variable Time"
            For i As Integer = 0 To stimuli.Items.Count - 1
                stimuli.SetItemChecked(i, If(item.StimulusType, "None").Split({" + "}, StringSplitOptions.None).Contains(CStr(stimuli.Items(i))))
            Next
            count.Enabled = item.ScheduleType.StartsWith("Variable")
        End If
        changing = False
    End Sub
    Private Sub RefreshList(selected As Integer)
        changing = True
        list.Items.Clear()
        For i As Integer = 0 To links.Count - 1
            list.Items.Add((i + 1) & ". " & links(i).ScheduleType & " " & links(i).Value & " | " & links(i).StimulusType)
        Next
        list.SelectedIndex = selected
        changing = False
        LoadLink(selected)
    End Sub
    Private Sub AddLink()
        If Not CommitEditor() Then Return
        If links.Count >= 100 Then
            MessageBox.Show(Me, "A chain can contain up to 100 links.")
            Return
        End If
        links.Add(New ClassicChainLink)
        RefreshList(links.Count - 1)
    End Sub
    Private Sub RemoveLink()
        If editing < 0 Then Return
        If links.Count = 1 Then
            MessageBox.Show(Me, "Keep at least one link, or choose Use simple schedule.")
            Return
        End If
        Dim index = editing
        links.RemoveAt(index)
        editing = -1
        RefreshList(Math.Min(index, links.Count - 1))
    End Sub
    Private Sub MoveLink(direction As Integer)
        If Not CommitEditor() Then Return
        Dim target = editing + direction
        If target < 0 OrElse target >= links.Count Then Return
        Dim item = links(editing)
        links(editing) = links(target)
        links(target) = item
        RefreshList(target)
    End Sub
    Private Sub UseSimpleSchedule()
        If Not CommitEditor() Then Return
        Dim first = links(0).Copy()
        SimpleSchedule = first.Copy()
        If first.ScheduleType = "Fixed Time" OrElse first.ScheduleType = "Variable Time" OrElse first.StimulusType <> "None" Then
            Result = New ClassicChainDefinition With {.Links = New List(Of ClassicChainLink) From {first}}
        Else
            Result = Nothing
        End If
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub SaveChain()
        If Not CommitEditor() Then Return
        Result = New ClassicChainDefinition With {.Links = links.Select(Function(item) item.Copy()).ToList()}
        DialogResult = DialogResult.OK
        Close()
    End Sub
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then tip.Dispose()
        MyBase.Dispose(disposing)
    End Sub
End Class
