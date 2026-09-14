<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ChamberSession
    Inherits System.Windows.Forms.UserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    'Layout and control properties are maintained by the Visual Studio designer.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series2 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series3 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series4 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series5 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series6 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series7 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series8 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series9 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series10 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series11 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series12 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series13 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series14 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series15 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series16 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series17 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series18 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series19 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series20 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series21 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series22 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Me.lblBoxTitle = New System.Windows.Forms.Label()
        Me.dgvMainSession = New System.Windows.Forms.DataGridView()
        Me.dgvMainSessionColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvMainSessionColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvMainInputs = New System.Windows.Forms.DataGridView()
        Me.dgvMainInputsColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvMainInputsColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvMainInputsColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvMainInputsColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvMainInputsColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvMainInputsColumn6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Chart1 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.pnlManualControls = New System.Windows.Forms.Panel()
        Me.btnSimResponse1 = New System.Windows.Forms.Button()
        Me.btnInputOutput1 = New System.Windows.Forms.Button()
        Me.btnInputReinforcer1 = New System.Windows.Forms.Button()
        Me.btnSimResponse2 = New System.Windows.Forms.Button()
        Me.btnInputOutput2 = New System.Windows.Forms.Button()
        Me.btnInputReinforcer2 = New System.Windows.Forms.Button()
        Me.btnSimResponse3 = New System.Windows.Forms.Button()
        Me.btnInputOutput3 = New System.Windows.Forms.Button()
        Me.btnInputReinforcer3 = New System.Windows.Forms.Button()
        Me.btnSimResponse4 = New System.Windows.Forms.Button()
        Me.btnInputOutput4 = New System.Windows.Forms.Button()
        Me.btnInputReinforcer4 = New System.Windows.Forms.Button()
        Me.btnSimResponse5 = New System.Windows.Forms.Button()
        Me.btnInputOutput5 = New System.Windows.Forms.Button()
        Me.btnInputReinforcer5 = New System.Windows.Forms.Button()
        Me.btnSimResponse6 = New System.Windows.Forms.Button()
        Me.btnInputOutput6 = New System.Windows.Forms.Button()
        Me.btnInputReinforcer6 = New System.Windows.Forms.Button()
        Me.btnFinish = New System.Windows.Forms.Button()
        Me.tmrStart = New System.Windows.Forms.Timer(Me.components)
        Me.tmrPostSession = New System.Windows.Forms.Timer(Me.components)
        Me.tmrComponentDuration = New System.Windows.Forms.Timer(Me.components)
        Me.tmrComponentStim = New System.Windows.Forms.Timer(Me.components)
        Me.tmrICI = New System.Windows.Forms.Timer(Me.components)
        Me.tmrCOD = New System.Windows.Forms.Timer(Me.components)
        Me.tmrChrt = New System.Windows.Forms.Timer(Me.components)
        Me.tmrComponentLightStim = New System.Windows.Forms.Timer(Me.components)
        Me.tmrTimeSchedule = New System.Windows.Forms.Timer(Me.components)
        Me.acquisitionTimer = New System.Windows.Forms.Timer(Me.components)
        Me.pnlManualControls.SuspendLayout()
        CType(Me.dgvMainSession, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvMainInputs, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        Me.lblBoxTitle.Name = "lblBoxTitle"
        Me.lblBoxTitle.Location = New System.Drawing.Point(8, 8)
        Me.lblBoxTitle.Size = New System.Drawing.Size(1184, 22)
        Me.lblBoxTitle.TabIndex = 0
        Me.lblBoxTitle.Text = "Box · COM · Subject — Cumulative record"
        Me.lblBoxTitle.BackColor = System.Drawing.Color.AliceBlue
        Me.lblBoxTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBoxTitle.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.dgvMainSession.Name = "dgvMainSession"
        Me.dgvMainSession.Location = New System.Drawing.Point(8, 38)
        Me.dgvMainSession.Size = New System.Drawing.Size(420, 180)
        Me.dgvMainSession.TabIndex = 1
        Me.dgvMainSession.ReadOnly = True
        Me.dgvMainSession.AllowUserToAddRows = False
        Me.dgvMainSession.AllowUserToDeleteRows = False
        Me.dgvMainSession.AllowUserToResizeRows = False
        Me.dgvMainSession.RowHeadersVisible = False
        Me.dgvMainSession.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvMainSession.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvMainSession.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvMainSessionColumn1.Name = "Property"
        Me.dgvMainSessionColumn1.HeaderText = "Property"
        Me.dgvMainSessionColumn1.ReadOnly = True
        Me.dgvMainSessionColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.dgvMainSessionColumn1.FillWeight = 100.0!
        Me.dgvMainSessionColumn2.Name = "Value"
        Me.dgvMainSessionColumn2.HeaderText = "Value"
        Me.dgvMainSessionColumn2.ReadOnly = True
        Me.dgvMainSessionColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.dgvMainSessionColumn2.FillWeight = 100.0!
        Me.dgvMainSession.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.dgvMainSessionColumn1, Me.dgvMainSessionColumn2})
        Me.dgvMainInputs.Name = "dgvMainInputs"
        Me.dgvMainInputs.Location = New System.Drawing.Point(436, 38)
        Me.dgvMainInputs.Size = New System.Drawing.Size(756, 180)
        Me.dgvMainInputs.TabIndex = 4
        Me.dgvMainInputs.ReadOnly = True
        Me.dgvMainInputs.AllowUserToAddRows = False
        Me.dgvMainInputs.AllowUserToDeleteRows = False
        Me.dgvMainInputs.AllowUserToResizeRows = False
        Me.dgvMainInputs.RowHeadersVisible = False
        Me.dgvMainInputs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvMainInputs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvMainInputs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvMainInputsColumn1.Name = "Input"
        Me.dgvMainInputsColumn1.HeaderText = "Input"
        Me.dgvMainInputsColumn1.ReadOnly = True
        Me.dgvMainInputsColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.dgvMainInputsColumn1.FillWeight = 100.0!
        Me.dgvMainInputsColumn2.Name = "Schedule"
        Me.dgvMainInputsColumn2.HeaderText = "Schedule"
        Me.dgvMainInputsColumn2.ReadOnly = True
        Me.dgvMainInputsColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.dgvMainInputsColumn2.FillWeight = 100.0!
        Me.dgvMainInputsColumn3.Name = "RfReady"
        Me.dgvMainInputsColumn3.HeaderText = "Rf ready"
        Me.dgvMainInputsColumn3.ReadOnly = True
        Me.dgvMainInputsColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.dgvMainInputsColumn3.FillWeight = 100.0!
        Me.dgvMainInputsColumn4.Name = "Responses"
        Me.dgvMainInputsColumn4.HeaderText = "Resp"
        Me.dgvMainInputsColumn4.ReadOnly = True
        Me.dgvMainInputsColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.dgvMainInputsColumn4.FillWeight = 100.0!
        Me.dgvMainInputsColumn5.Name = "CODResponses"
        Me.dgvMainInputsColumn5.HeaderText = "COD"
        Me.dgvMainInputsColumn5.ReadOnly = True
        Me.dgvMainInputsColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.dgvMainInputsColumn5.FillWeight = 100.0!
        Me.dgvMainInputsColumn6.Name = "Reinforcers"
        Me.dgvMainInputsColumn6.HeaderText = "Rf"
        Me.dgvMainInputsColumn6.ReadOnly = True
        Me.dgvMainInputsColumn6.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.dgvMainInputsColumn6.FillWeight = 100.0!
        Me.dgvMainInputs.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.dgvMainInputsColumn1, Me.dgvMainInputsColumn2, Me.dgvMainInputsColumn3, Me.dgvMainInputsColumn4, Me.dgvMainInputsColumn5, Me.dgvMainInputsColumn6})
        Me.dgvMainInputs.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.Chart1.Name = "Chart1"
        Me.Chart1.Location = New System.Drawing.Point(8, 226)
        Me.Chart1.Size = New System.Drawing.Size(1184, 310)
        Me.Chart1.TabIndex = 11
        Me.Chart1.BackColor = System.Drawing.Color.Transparent
        ChartArea1.Name = "ChartArea1"
        ChartArea1.AxisX.Title = "Time"
        ChartArea1.AxisY.Title = "Responses"
        ChartArea1.AxisX.MaximumAutoSize = 100.0!
        ChartArea1.AxisX.MajorGrid.Enabled = False
        ChartArea1.AxisX.MinorGrid.Enabled = False
        ChartArea1.AxisY.MajorGrid.Enabled = False
        ChartArea1.AxisY.MinorGrid.Enabled = False
        Me.Chart1.ChartAreas.Add(ChartArea1)
        Legend1.Name = "Legend1"
        Legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Right
        Me.Chart1.Legends.Add(Legend1)
        Series1.Name = "ResponseInput1"
        Series1.ChartArea = "ChartArea1"
        Series1.Legend = "Legend1"
        Series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Series1.BorderWidth = 2
        Series1.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series1)
        Series2.Name = "ReinforcerInput1"
        Series2.ChartArea = "ChartArea1"
        Series2.Legend = "Legend1"
        Series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point
        Series2.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross
        Series2.MarkerSize = 8
        Series2.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series2)
        Series3.Name = "ResponseInput2"
        Series3.ChartArea = "ChartArea1"
        Series3.Legend = "Legend1"
        Series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Series3.BorderWidth = 2
        Series3.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series3)
        Series4.Name = "ReinforcerInput2"
        Series4.ChartArea = "ChartArea1"
        Series4.Legend = "Legend1"
        Series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point
        Series4.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross
        Series4.MarkerSize = 8
        Series4.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series4)
        Series5.Name = "ResponseInput3"
        Series5.ChartArea = "ChartArea1"
        Series5.Legend = "Legend1"
        Series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Series5.BorderWidth = 2
        Series5.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series5)
        Series6.Name = "ReinforcerInput3"
        Series6.ChartArea = "ChartArea1"
        Series6.Legend = "Legend1"
        Series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point
        Series6.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross
        Series6.MarkerSize = 8
        Series6.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series6)
        Series7.Name = "ResponseInput4"
        Series7.ChartArea = "ChartArea1"
        Series7.Legend = "Legend1"
        Series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Series7.BorderWidth = 2
        Series7.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series7)
        Series8.Name = "ReinforcerInput4"
        Series8.ChartArea = "ChartArea1"
        Series8.Legend = "Legend1"
        Series8.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point
        Series8.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross
        Series8.MarkerSize = 8
        Series8.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series8)
        Series9.Name = "ResponseInput5"
        Series9.ChartArea = "ChartArea1"
        Series9.Legend = "Legend1"
        Series9.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Series9.BorderWidth = 2
        Series9.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series9)
        Series10.Name = "ReinforcerInput5"
        Series10.ChartArea = "ChartArea1"
        Series10.Legend = "Legend1"
        Series10.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point
        Series10.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross
        Series10.MarkerSize = 8
        Series10.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series10)
        Series11.Name = "ResponseInput6"
        Series11.ChartArea = "ChartArea1"
        Series11.Legend = "Legend1"
        Series11.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Series11.BorderWidth = 2
        Series11.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series11)
        Series12.Name = "ReinforcerInput6"
        Series12.ChartArea = "ChartArea1"
        Series12.Legend = "Legend1"
        Series12.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point
        Series12.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross
        Series12.MarkerSize = 8
        Series12.IsVisibleInLegend = False
        Me.Chart1.Series.Add(Series12)
        Series13.Name = "ComponentSeries1"
        Series13.ChartArea = "ChartArea1"
        Series13.Legend = "Legend1"
        Series13.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series13.BorderWidth = 1
        Series13.IsVisibleInLegend = False
        Series13.Color = System.Drawing.Color.FromArgb(45, System.Drawing.Color.Blue)
        Me.Chart1.Series.Add(Series13)
        Series14.Name = "ComponentSeries2"
        Series14.ChartArea = "ChartArea1"
        Series14.Legend = "Legend1"
        Series14.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series14.BorderWidth = 1
        Series14.IsVisibleInLegend = False
        Series14.Color = System.Drawing.Color.FromArgb(45, System.Drawing.Color.Red)
        Me.Chart1.Series.Add(Series14)
        Series15.Name = "ComponentSeries3"
        Series15.ChartArea = "ChartArea1"
        Series15.Legend = "Legend1"
        Series15.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series15.BorderWidth = 1
        Series15.IsVisibleInLegend = False
        Series15.Color = System.Drawing.Color.FromArgb(55, System.Drawing.Color.Goldenrod)
        Me.Chart1.Series.Add(Series15)
        Series16.Name = "ComponentSeries4"
        Series16.ChartArea = "ChartArea1"
        Series16.Legend = "Legend1"
        Series16.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series16.BorderWidth = 1
        Series16.IsVisibleInLegend = False
        Series16.Color = System.Drawing.Color.FromArgb(45, System.Drawing.Color.Green)
        Me.Chart1.Series.Add(Series16)
        Series17.Name = "ComponentSeries5"
        Series17.ChartArea = "ChartArea1"
        Series17.Legend = "Legend1"
        Series17.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series17.BorderWidth = 1
        Series17.IsVisibleInLegend = False
        Series17.Color = System.Drawing.Color.FromArgb(45, System.Drawing.Color.DarkViolet)
        Me.Chart1.Series.Add(Series17)
        Series18.Name = "ComponentSeries6"
        Series18.ChartArea = "ChartArea1"
        Series18.Legend = "Legend1"
        Series18.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series18.BorderWidth = 1
        Series18.IsVisibleInLegend = False
        Series18.Color = System.Drawing.Color.FromArgb(45, System.Drawing.Color.Teal)
        Me.Chart1.Series.Add(Series18)
        Series19.Name = "ComponentSeries7"
        Series19.ChartArea = "ChartArea1"
        Series19.Legend = "Legend1"
        Series19.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series19.BorderWidth = 1
        Series19.IsVisibleInLegend = False
        Series19.Color = System.Drawing.Color.FromArgb(45, System.Drawing.Color.OrangeRed)
        Me.Chart1.Series.Add(Series19)
        Series20.Name = "ComponentSeries8"
        Series20.ChartArea = "ChartArea1"
        Series20.Legend = "Legend1"
        Series20.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series20.BorderWidth = 1
        Series20.IsVisibleInLegend = False
        Series20.Color = System.Drawing.Color.FromArgb(45, System.Drawing.Color.SlateGray)
        Me.Chart1.Series.Add(Series20)
        Series21.Name = "ComponentSeries9"
        Series21.ChartArea = "ChartArea1"
        Series21.Legend = "Legend1"
        Series21.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series21.BorderWidth = 1
        Series21.IsVisibleInLegend = False
        Series21.Color = System.Drawing.Color.FromArgb(45, System.Drawing.Color.DeepPink)
        Me.Chart1.Series.Add(Series21)
        Series22.Name = "ComponentSeries10"
        Series22.ChartArea = "ChartArea1"
        Series22.Legend = "Legend1"
        Series22.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area
        Series22.BorderWidth = 1
        Series22.IsVisibleInLegend = False
        Series22.Color = System.Drawing.Color.FromArgb(45, System.Drawing.Color.DarkCyan)
        Me.Chart1.Series.Add(Series22)
        Me.Chart1.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.pnlManualControls.Name = "pnlManualControls"
        Me.pnlManualControls.Location = New System.Drawing.Point(8, 544)
        Me.pnlManualControls.Size = New System.Drawing.Size(1184, 108)
        Me.pnlManualControls.TabIndex = 12
        Me.pnlManualControls.AutoScroll = True
        Me.pnlManualControls.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnSimResponse1.Name = "btnSimResponse1"
        Me.btnSimResponse1.Location = New System.Drawing.Point(0, 0)
        Me.btnSimResponse1.Size = New System.Drawing.Size(138, 28)
        Me.btnSimResponse1.TabIndex = 13
        Me.btnSimResponse1.Text = "Response 1"
        Me.btnSimResponse1.UseVisualStyleBackColor = True
        Me.btnSimResponse1.Tag = 0
        Me.btnInputOutput1.Name = "btnInputOutput1"
        Me.btnInputOutput1.Location = New System.Drawing.Point(0, 32)
        Me.btnInputOutput1.Size = New System.Drawing.Size(138, 28)
        Me.btnInputOutput1.TabIndex = 14
        Me.btnInputOutput1.Text = "In/Out 1"
        Me.btnInputOutput1.UseVisualStyleBackColor = True
        Me.btnInputOutput1.Tag = 0
        Me.btnInputReinforcer1.Name = "btnInputReinforcer1"
        Me.btnInputReinforcer1.Location = New System.Drawing.Point(0, 64)
        Me.btnInputReinforcer1.Size = New System.Drawing.Size(138, 28)
        Me.btnInputReinforcer1.TabIndex = 15
        Me.btnInputReinforcer1.Text = "Rf 1"
        Me.btnInputReinforcer1.UseVisualStyleBackColor = True
        Me.btnInputReinforcer1.Tag = 0
        Me.btnSimResponse2.Name = "btnSimResponse2"
        Me.btnSimResponse2.Location = New System.Drawing.Point(144, 0)
        Me.btnSimResponse2.Size = New System.Drawing.Size(138, 28)
        Me.btnSimResponse2.TabIndex = 16
        Me.btnSimResponse2.Text = "Response 2"
        Me.btnSimResponse2.UseVisualStyleBackColor = True
        Me.btnSimResponse2.Tag = 1
        Me.btnInputOutput2.Name = "btnInputOutput2"
        Me.btnInputOutput2.Location = New System.Drawing.Point(144, 32)
        Me.btnInputOutput2.Size = New System.Drawing.Size(138, 28)
        Me.btnInputOutput2.TabIndex = 17
        Me.btnInputOutput2.Text = "In/Out 2"
        Me.btnInputOutput2.UseVisualStyleBackColor = True
        Me.btnInputOutput2.Tag = 1
        Me.btnInputReinforcer2.Name = "btnInputReinforcer2"
        Me.btnInputReinforcer2.Location = New System.Drawing.Point(144, 64)
        Me.btnInputReinforcer2.Size = New System.Drawing.Size(138, 28)
        Me.btnInputReinforcer2.TabIndex = 18
        Me.btnInputReinforcer2.Text = "Rf 2"
        Me.btnInputReinforcer2.UseVisualStyleBackColor = True
        Me.btnInputReinforcer2.Tag = 1
        Me.btnSimResponse3.Name = "btnSimResponse3"
        Me.btnSimResponse3.Location = New System.Drawing.Point(288, 0)
        Me.btnSimResponse3.Size = New System.Drawing.Size(138, 28)
        Me.btnSimResponse3.TabIndex = 19
        Me.btnSimResponse3.Text = "Response 3"
        Me.btnSimResponse3.UseVisualStyleBackColor = True
        Me.btnSimResponse3.Tag = 2
        Me.btnInputOutput3.Name = "btnInputOutput3"
        Me.btnInputOutput3.Location = New System.Drawing.Point(288, 32)
        Me.btnInputOutput3.Size = New System.Drawing.Size(138, 28)
        Me.btnInputOutput3.TabIndex = 20
        Me.btnInputOutput3.Text = "In/Out 3"
        Me.btnInputOutput3.UseVisualStyleBackColor = True
        Me.btnInputOutput3.Tag = 2
        Me.btnInputReinforcer3.Name = "btnInputReinforcer3"
        Me.btnInputReinforcer3.Location = New System.Drawing.Point(288, 64)
        Me.btnInputReinforcer3.Size = New System.Drawing.Size(138, 28)
        Me.btnInputReinforcer3.TabIndex = 21
        Me.btnInputReinforcer3.Text = "Rf 3"
        Me.btnInputReinforcer3.UseVisualStyleBackColor = True
        Me.btnInputReinforcer3.Tag = 2
        Me.btnSimResponse4.Name = "btnSimResponse4"
        Me.btnSimResponse4.Location = New System.Drawing.Point(432, 0)
        Me.btnSimResponse4.Size = New System.Drawing.Size(138, 28)
        Me.btnSimResponse4.TabIndex = 22
        Me.btnSimResponse4.Text = "Response 4"
        Me.btnSimResponse4.UseVisualStyleBackColor = True
        Me.btnSimResponse4.Tag = 3
        Me.btnInputOutput4.Name = "btnInputOutput4"
        Me.btnInputOutput4.Location = New System.Drawing.Point(432, 32)
        Me.btnInputOutput4.Size = New System.Drawing.Size(138, 28)
        Me.btnInputOutput4.TabIndex = 23
        Me.btnInputOutput4.Text = "In/Out 4"
        Me.btnInputOutput4.UseVisualStyleBackColor = True
        Me.btnInputOutput4.Tag = 3
        Me.btnInputReinforcer4.Name = "btnInputReinforcer4"
        Me.btnInputReinforcer4.Location = New System.Drawing.Point(432, 64)
        Me.btnInputReinforcer4.Size = New System.Drawing.Size(138, 28)
        Me.btnInputReinforcer4.TabIndex = 24
        Me.btnInputReinforcer4.Text = "Rf 4"
        Me.btnInputReinforcer4.UseVisualStyleBackColor = True
        Me.btnInputReinforcer4.Tag = 3
        Me.btnSimResponse5.Name = "btnSimResponse5"
        Me.btnSimResponse5.Location = New System.Drawing.Point(576, 0)
        Me.btnSimResponse5.Size = New System.Drawing.Size(138, 28)
        Me.btnSimResponse5.TabIndex = 25
        Me.btnSimResponse5.Text = "Response 5"
        Me.btnSimResponse5.UseVisualStyleBackColor = True
        Me.btnSimResponse5.Tag = 4
        Me.btnInputOutput5.Name = "btnInputOutput5"
        Me.btnInputOutput5.Location = New System.Drawing.Point(576, 32)
        Me.btnInputOutput5.Size = New System.Drawing.Size(138, 28)
        Me.btnInputOutput5.TabIndex = 26
        Me.btnInputOutput5.Text = "In/Out 5"
        Me.btnInputOutput5.UseVisualStyleBackColor = True
        Me.btnInputOutput5.Tag = 4
        Me.btnInputReinforcer5.Name = "btnInputReinforcer5"
        Me.btnInputReinforcer5.Location = New System.Drawing.Point(576, 64)
        Me.btnInputReinforcer5.Size = New System.Drawing.Size(138, 28)
        Me.btnInputReinforcer5.TabIndex = 27
        Me.btnInputReinforcer5.Text = "Rf 5"
        Me.btnInputReinforcer5.UseVisualStyleBackColor = True
        Me.btnInputReinforcer5.Tag = 4
        Me.btnSimResponse6.Name = "btnSimResponse6"
        Me.btnSimResponse6.Location = New System.Drawing.Point(720, 0)
        Me.btnSimResponse6.Size = New System.Drawing.Size(138, 28)
        Me.btnSimResponse6.TabIndex = 28
        Me.btnSimResponse6.Text = "Response 6"
        Me.btnSimResponse6.UseVisualStyleBackColor = True
        Me.btnSimResponse6.Tag = 5
        Me.btnInputOutput6.Name = "btnInputOutput6"
        Me.btnInputOutput6.Location = New System.Drawing.Point(720, 32)
        Me.btnInputOutput6.Size = New System.Drawing.Size(138, 28)
        Me.btnInputOutput6.TabIndex = 29
        Me.btnInputOutput6.Text = "In/Out 6"
        Me.btnInputOutput6.UseVisualStyleBackColor = True
        Me.btnInputOutput6.Tag = 5
        Me.btnInputReinforcer6.Name = "btnInputReinforcer6"
        Me.btnInputReinforcer6.Location = New System.Drawing.Point(720, 64)
        Me.btnInputReinforcer6.Size = New System.Drawing.Size(138, 28)
        Me.btnInputReinforcer6.TabIndex = 30
        Me.btnInputReinforcer6.Text = "Rf 6"
        Me.btnInputReinforcer6.UseVisualStyleBackColor = True
        Me.btnInputReinforcer6.Tag = 5
        Me.btnFinish.Name = "btnFinish"
        Me.btnFinish.Location = New System.Drawing.Point(864, 64)
        Me.btnFinish.Size = New System.Drawing.Size(138, 28)
        Me.btnFinish.TabIndex = 31
        Me.btnFinish.Text = "Finish"
        Me.btnFinish.UseVisualStyleBackColor = False
        Me.btnFinish.BackColor = System.Drawing.Color.Firebrick
        Me.btnFinish.ForeColor = System.Drawing.Color.White
        Me.btnFinish.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.tmrStart.Interval = 100
        Me.tmrPostSession.Interval = 100
        Me.tmrComponentDuration.Interval = 100
        Me.tmrComponentStim.Interval = 1000
        Me.tmrICI.Interval = 100
        Me.tmrCOD.Interval = 1000
        Me.tmrChrt.Interval = 100
        Me.tmrComponentLightStim.Interval = 100
        Me.tmrTimeSchedule.Interval = 100
        Me.acquisitionTimer.Interval = 10
        Me.MinimumSize = New System.Drawing.Size(850, 400)
        Me.Margin = New System.Windows.Forms.Padding(6)
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Size = New System.Drawing.Size(1200, 660)
        Me.Name = "ChamberSession"
        Me.Controls.Add(Me.lblBoxTitle)
        Me.Controls.Add(Me.dgvMainSession)
        Me.Controls.Add(Me.dgvMainInputs)
        Me.Controls.Add(Me.Chart1)
        Me.Controls.Add(Me.pnlManualControls)
        Me.pnlManualControls.Controls.Add(Me.btnSimResponse1)
        Me.pnlManualControls.Controls.Add(Me.btnInputOutput1)
        Me.pnlManualControls.Controls.Add(Me.btnInputReinforcer1)
        Me.pnlManualControls.Controls.Add(Me.btnSimResponse2)
        Me.pnlManualControls.Controls.Add(Me.btnInputOutput2)
        Me.pnlManualControls.Controls.Add(Me.btnInputReinforcer2)
        Me.pnlManualControls.Controls.Add(Me.btnSimResponse3)
        Me.pnlManualControls.Controls.Add(Me.btnInputOutput3)
        Me.pnlManualControls.Controls.Add(Me.btnInputReinforcer3)
        Me.pnlManualControls.Controls.Add(Me.btnSimResponse4)
        Me.pnlManualControls.Controls.Add(Me.btnInputOutput4)
        Me.pnlManualControls.Controls.Add(Me.btnInputReinforcer4)
        Me.pnlManualControls.Controls.Add(Me.btnSimResponse5)
        Me.pnlManualControls.Controls.Add(Me.btnInputOutput5)
        Me.pnlManualControls.Controls.Add(Me.btnInputReinforcer5)
        Me.pnlManualControls.Controls.Add(Me.btnSimResponse6)
        Me.pnlManualControls.Controls.Add(Me.btnInputOutput6)
        Me.pnlManualControls.Controls.Add(Me.btnInputReinforcer6)
        Me.pnlManualControls.Controls.Add(Me.btnFinish)
        Me.pnlManualControls.ResumeLayout(False)
        Me.pnlManualControls.PerformLayout()
        CType(Me.dgvMainSession, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvMainInputs, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblBoxTitle As System.Windows.Forms.Label
    Friend WithEvents dgvMainSession As System.Windows.Forms.DataGridView
    Friend WithEvents dgvMainSessionColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvMainSessionColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvMainInputs As System.Windows.Forms.DataGridView
    Friend WithEvents dgvMainInputsColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvMainInputsColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvMainInputsColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvMainInputsColumn4 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvMainInputsColumn5 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvMainInputsColumn6 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Chart1 As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents pnlManualControls As System.Windows.Forms.Panel
    Friend WithEvents btnSimResponse1 As System.Windows.Forms.Button
    Friend WithEvents btnInputOutput1 As System.Windows.Forms.Button
    Friend WithEvents btnInputReinforcer1 As System.Windows.Forms.Button
    Friend WithEvents btnSimResponse2 As System.Windows.Forms.Button
    Friend WithEvents btnInputOutput2 As System.Windows.Forms.Button
    Friend WithEvents btnInputReinforcer2 As System.Windows.Forms.Button
    Friend WithEvents btnSimResponse3 As System.Windows.Forms.Button
    Friend WithEvents btnInputOutput3 As System.Windows.Forms.Button
    Friend WithEvents btnInputReinforcer3 As System.Windows.Forms.Button
    Friend WithEvents btnSimResponse4 As System.Windows.Forms.Button
    Friend WithEvents btnInputOutput4 As System.Windows.Forms.Button
    Friend WithEvents btnInputReinforcer4 As System.Windows.Forms.Button
    Friend WithEvents btnSimResponse5 As System.Windows.Forms.Button
    Friend WithEvents btnInputOutput5 As System.Windows.Forms.Button
    Friend WithEvents btnInputReinforcer5 As System.Windows.Forms.Button
    Friend WithEvents btnSimResponse6 As System.Windows.Forms.Button
    Friend WithEvents btnInputOutput6 As System.Windows.Forms.Button
    Friend WithEvents btnInputReinforcer6 As System.Windows.Forms.Button
    Friend WithEvents btnFinish As System.Windows.Forms.Button
    Friend WithEvents tmrStart As System.Windows.Forms.Timer
    Friend WithEvents tmrPostSession As System.Windows.Forms.Timer
    Friend WithEvents tmrComponentDuration As System.Windows.Forms.Timer
    Friend WithEvents tmrComponentStim As System.Windows.Forms.Timer
    Friend WithEvents tmrICI As System.Windows.Forms.Timer
    Friend WithEvents tmrCOD As System.Windows.Forms.Timer
    Friend WithEvents tmrChrt As System.Windows.Forms.Timer
    Friend WithEvents tmrComponentLightStim As System.Windows.Forms.Timer
    Friend WithEvents tmrTimeSchedule As System.Windows.Forms.Timer
    Friend WithEvents acquisitionTimer As System.Windows.Forms.Timer
End Class
