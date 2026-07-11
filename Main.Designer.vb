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

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Main))
        Me.Chart1 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.btnFinish = New System.Windows.Forms.Button()
        Me.tmrStart = New System.Windows.Forms.Timer(Me.components)
        Me.tmrPostSession = New System.Windows.Forms.Timer(Me.components)
        Me.tmrComponentDuration = New System.Windows.Forms.Timer(Me.components)
        Me.tmrComponentStim = New System.Windows.Forms.Timer(Me.components)
        Me.tmrICI = New System.Windows.Forms.Timer(Me.components)
        Me.tmrCOD = New System.Windows.Forms.Timer(Me.components)
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Chart1
        '
        Me.Chart1.BackColor = System.Drawing.Color.Transparent
        ChartArea1.AxisX.MajorGrid.Enabled = False
        ChartArea1.AxisX.MaximumAutoSize = 100.0!
        ChartArea1.AxisX.MinorGrid.Enabled = False
        ChartArea1.AxisX.Title = "Time"
        ChartArea1.AxisY.MajorGrid.Enabled = False
        ChartArea1.AxisY.MinorGrid.Enabled = False
        ChartArea1.AxisY.Title = "Responses"
        ChartArea1.Name = "ChartArea1"
        Me.Chart1.ChartAreas.Add(ChartArea1)
        Legend1.Name = "Legend1"
        Me.Chart1.Legends.Add(Legend1)
        Me.Chart1.Location = New System.Drawing.Point(12, 244)
        Me.Chart1.Margin = New System.Windows.Forms.Padding(2)
        Me.Chart1.Name = "Chart1"
        Me.Chart1.Size = New System.Drawing.Size(1466, 430)
        Me.Chart1.TabIndex = 0
        '
        'btnFinish
        '
        Me.btnFinish.Location = New System.Drawing.Point(12, 686)
        Me.btnFinish.Margin = New System.Windows.Forms.Padding(3)
        Me.btnFinish.Name = "btnFinish"
        Me.btnFinish.Size = New System.Drawing.Size(138, 28)
        Me.btnFinish.TabIndex = 1
        Me.btnFinish.Text = "Finish"
        Me.btnFinish.BackColor = System.Drawing.Color.Firebrick
        Me.btnFinish.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnFinish.ForeColor = System.Drawing.Color.White
        Me.btnFinish.UseVisualStyleBackColor = False
        Me.btnFinish.Visible = False
        '
        'tmrComponentStim
        '
        Me.tmrComponentStim.Interval = 1000
        '
        'tmrCOD
        '
        Me.tmrCOD.Interval = 1000
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1490, 820)
        Me.Controls.Add(Me.btnFinish)
        Me.Controls.Add(Me.Chart1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
        Me.Name = "Main"
        Me.Text = "Main"
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Chart1 As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents btnFinish As System.Windows.Forms.Button
    Friend WithEvents tmrStart As System.Windows.Forms.Timer
    Friend WithEvents tmrPostSession As System.Windows.Forms.Timer
    Friend WithEvents tmrComponentDuration As System.Windows.Forms.Timer
    Friend WithEvents tmrComponentStim As System.Windows.Forms.Timer
    Friend WithEvents tmrICI As System.Windows.Forms.Timer
    Friend WithEvents tmrCOD As System.Windows.Forms.Timer
End Class
