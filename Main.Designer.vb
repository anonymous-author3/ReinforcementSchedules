<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Main
    Inherits System.Windows.Forms.Form

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
        Me.hubLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.toolbar = New System.Windows.Forms.Panel()
        Me.setupButton = New System.Windows.Forms.Button()
        Me.collapseButton = New System.Windows.Forms.Button()
        Me.lblHubHelp = New System.Windows.Forms.Label()
        Me.overview = New System.Windows.Forms.DataGridView()
        Me.overviewColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn7 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn8 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn9 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn10 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn11 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn12 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn13 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn14 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.overviewColumn15 = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.overviewColumn16 = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.overviewColumn17 = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.overviewColumn18 = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.records = New System.Windows.Forms.FlowLayoutPanel()
        Me.recordPreview = New Global.ReinforcementSchedules.ChamberSession()
        Me.refreshTimer = New System.Windows.Forms.Timer(Me.components)
        Me.hubLayout.SuspendLayout()
        Me.toolbar.SuspendLayout()
        Me.records.SuspendLayout()
        CType(Me.overview, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        Me.hubLayout.Name = "hubLayout"
        Me.hubLayout.TabIndex = 0
        Me.hubLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.hubLayout.ColumnCount = 1
        Me.hubLayout.RowCount = 3
        Me.hubLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.hubLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        Me.hubLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 220.0!))
        Me.hubLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.toolbar.Name = "toolbar"
        Me.toolbar.TabIndex = 1
        Me.toolbar.Dock = System.Windows.Forms.DockStyle.Fill
        Me.setupButton.Name = "setupButton"
        Me.setupButton.Location = New System.Drawing.Point(6, 3)
        Me.setupButton.Size = New System.Drawing.Size(152, 28)
        Me.setupButton.TabIndex = 2
        Me.setupButton.Text = "Add / configure boxes"
        Me.setupButton.UseVisualStyleBackColor = True
        Me.collapseButton.Name = "collapseButton"
        Me.collapseButton.Location = New System.Drawing.Point(164, 3)
        Me.collapseButton.Size = New System.Drawing.Size(142, 28)
        Me.collapseButton.TabIndex = 3
        Me.collapseButton.Text = "Collapse all records"
        Me.collapseButton.UseVisualStyleBackColor = True
        Me.lblHubHelp.Name = "lblHubHelp"
        Me.lblHubHelp.Location = New System.Drawing.Point(320, 10)
        Me.lblHubHelp.Size = New System.Drawing.Size(480, 18)
        Me.lblHubHelp.TabIndex = 4
        Me.lblHubHelp.Text = "Counts are session totals. Each box starts independently."
        Me.overview.Name = "overview"
        Me.overview.Location = New System.Drawing.Point(0, 0)
        Me.overview.Size = New System.Drawing.Size(1274, 214)
        Me.overview.TabIndex = 5
        Me.overview.ReadOnly = True
        Me.overview.AllowUserToAddRows = False
        Me.overview.AllowUserToDeleteRows = False
        Me.overview.AllowUserToResizeRows = False
        Me.overview.RowHeadersVisible = False
        Me.overview.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.overview.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.overview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.overviewColumn1.Name = "Box"
        Me.overviewColumn1.HeaderText = "Box"
        Me.overviewColumn1.ReadOnly = True
        Me.overviewColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn1.FillWeight = 100.0!
        Me.overviewColumn2.Name = "COM"
        Me.overviewColumn2.HeaderText = "COM"
        Me.overviewColumn2.ReadOnly = True
        Me.overviewColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn2.FillWeight = 100.0!
        Me.overviewColumn3.Name = "Subject"
        Me.overviewColumn3.HeaderText = "Subject"
        Me.overviewColumn3.ReadOnly = True
        Me.overviewColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn3.FillWeight = 100.0!
        Me.overviewColumn4.Name = "Session"
        Me.overviewColumn4.HeaderText = "Session"
        Me.overviewColumn4.ReadOnly = True
        Me.overviewColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn4.FillWeight = 100.0!
        Me.overviewColumn5.Name = "State"
        Me.overviewColumn5.HeaderText = "State"
        Me.overviewColumn5.ReadOnly = True
        Me.overviewColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn5.FillWeight = 140.0!
        Me.overviewColumn6.Name = "TimeSeconds"
        Me.overviewColumn6.HeaderText = "Time (s)"
        Me.overviewColumn6.ReadOnly = True
        Me.overviewColumn6.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn6.FillWeight = 100.0!
        Me.overviewColumn7.Name = "Component"
        Me.overviewColumn7.HeaderText = "Component"
        Me.overviewColumn7.ReadOnly = True
        Me.overviewColumn7.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn7.FillWeight = 150.0!
        Me.overviewColumn8.Name = "R1"
        Me.overviewColumn8.HeaderText = "R1"
        Me.overviewColumn8.ReadOnly = True
        Me.overviewColumn8.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn8.FillWeight = 45.0!
        Me.overviewColumn9.Name = "R2"
        Me.overviewColumn9.HeaderText = "R2"
        Me.overviewColumn9.ReadOnly = True
        Me.overviewColumn9.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn9.FillWeight = 45.0!
        Me.overviewColumn10.Name = "R3"
        Me.overviewColumn10.HeaderText = "R3"
        Me.overviewColumn10.ReadOnly = True
        Me.overviewColumn10.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn10.FillWeight = 45.0!
        Me.overviewColumn11.Name = "R4"
        Me.overviewColumn11.HeaderText = "R4"
        Me.overviewColumn11.ReadOnly = True
        Me.overviewColumn11.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn11.FillWeight = 45.0!
        Me.overviewColumn12.Name = "R5"
        Me.overviewColumn12.HeaderText = "R5"
        Me.overviewColumn12.ReadOnly = True
        Me.overviewColumn12.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn12.FillWeight = 45.0!
        Me.overviewColumn13.Name = "R6"
        Me.overviewColumn13.HeaderText = "R6"
        Me.overviewColumn13.ReadOnly = True
        Me.overviewColumn13.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn13.FillWeight = 45.0!
        Me.overviewColumn14.Name = "Reinforcers"
        Me.overviewColumn14.HeaderText = "Reinforcers"
        Me.overviewColumn14.ReadOnly = True
        Me.overviewColumn14.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn14.FillWeight = 100.0!
        Me.overviewColumn15.Name = "Start"
        Me.overviewColumn15.HeaderText = "Start"
        Me.overviewColumn15.ReadOnly = True
        Me.overviewColumn15.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn15.FillWeight = 70.0!
        Me.overviewColumn16.Name = "Finish"
        Me.overviewColumn16.HeaderText = "Finish"
        Me.overviewColumn16.ReadOnly = True
        Me.overviewColumn16.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn16.FillWeight = 70.0!
        Me.overviewColumn17.Name = "Record"
        Me.overviewColumn17.HeaderText = "Record"
        Me.overviewColumn17.ReadOnly = True
        Me.overviewColumn17.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn17.FillWeight = 70.0!
        Me.overviewColumn18.Name = "Remove"
        Me.overviewColumn18.HeaderText = "Remove"
        Me.overviewColumn18.ReadOnly = True
        Me.overviewColumn18.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.overviewColumn18.FillWeight = 70.0!
        Me.overview.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.overviewColumn1, Me.overviewColumn2, Me.overviewColumn3, Me.overviewColumn4, Me.overviewColumn5, Me.overviewColumn6, Me.overviewColumn7, Me.overviewColumn8, Me.overviewColumn9, Me.overviewColumn10, Me.overviewColumn11, Me.overviewColumn12, Me.overviewColumn13, Me.overviewColumn14, Me.overviewColumn15, Me.overviewColumn16, Me.overviewColumn17, Me.overviewColumn18})
        Me.overview.Dock = System.Windows.Forms.DockStyle.Fill
        Me.overview.RowTemplate.Height = 32
        Me.overviewColumn8.ToolTipText = "Total responses on input 1"
        Me.overviewColumn9.ToolTipText = "Total responses on input 2"
        Me.overviewColumn10.ToolTipText = "Total responses on input 3"
        Me.overviewColumn11.ToolTipText = "Total responses on input 4"
        Me.overviewColumn12.ToolTipText = "Total responses on input 5"
        Me.overviewColumn13.ToolTipText = "Total responses on input 6"
        Me.records.Name = "records"
        Me.records.TabIndex = 24
        Me.records.Dock = System.Windows.Forms.DockStyle.Fill
        Me.records.AutoScroll = True
        Me.records.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.records.WrapContents = False
        Me.recordPreview.Name = "recordPreview"
        Me.recordPreview.Size = New System.Drawing.Size(1200, 660)
        Me.recordPreview.TabIndex = 25
        Me.refreshTimer.Interval = 250
        Me.MinimumSize = New System.Drawing.Size(980, 520)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1280, 760)
        Me.Name = "Main"
        Me.Text = "Main — Session hub"
        Me.Controls.Add(Me.hubLayout)
        Me.toolbar.Controls.Add(Me.setupButton)
        Me.toolbar.Controls.Add(Me.collapseButton)
        Me.toolbar.Controls.Add(Me.lblHubHelp)
        Me.records.Controls.Add(Me.recordPreview)
        Me.hubLayout.Controls.Add(Me.toolbar, 0, 0)
        Me.hubLayout.Controls.Add(Me.overview, 0, 1)
        Me.hubLayout.Controls.Add(Me.records, 0, 2)
        Me.records.ResumeLayout(False)
        Me.records.PerformLayout()
        Me.toolbar.ResumeLayout(False)
        Me.toolbar.PerformLayout()
        Me.hubLayout.ResumeLayout(False)
        Me.hubLayout.PerformLayout()
        CType(Me.overview, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents hubLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents toolbar As System.Windows.Forms.Panel
    Friend WithEvents setupButton As System.Windows.Forms.Button
    Friend WithEvents collapseButton As System.Windows.Forms.Button
    Friend WithEvents lblHubHelp As System.Windows.Forms.Label
    Friend WithEvents overview As System.Windows.Forms.DataGridView
    Friend WithEvents overviewColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn4 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn5 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn6 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn7 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn8 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn9 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn10 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn11 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn12 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn13 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn14 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents overviewColumn15 As System.Windows.Forms.DataGridViewButtonColumn
    Friend WithEvents overviewColumn16 As System.Windows.Forms.DataGridViewButtonColumn
    Friend WithEvents overviewColumn17 As System.Windows.Forms.DataGridViewButtonColumn
    Friend WithEvents overviewColumn18 As System.Windows.Forms.DataGridViewButtonColumn
    Friend WithEvents records As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents recordPreview As Global.ReinforcementSchedules.ChamberSession
    Friend WithEvents refreshTimer As System.Windows.Forms.Timer
End Class
