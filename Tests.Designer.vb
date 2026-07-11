<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Tests
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.btnFeeder = New System.Windows.Forms.Button()
        Me.btnPumpOn = New System.Windows.Forms.Button()
        Me.btnLights = New System.Windows.Forms.Button()
        Me.btnTone = New System.Windows.Forms.Button()
        Me.tmrStart = New System.Windows.Forms.Timer(Me.components)
        Me.Chart1 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.tmrFeeder = New System.Windows.Forms.Timer(Me.components)
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(175, 363)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(153, 52)
        Me.btnClose.TabIndex = 0
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'btnFeeder
        '
        Me.btnFeeder.Location = New System.Drawing.Point(175, 129)
        Me.btnFeeder.Name = "btnFeeder"
        Me.btnFeeder.Size = New System.Drawing.Size(153, 52)
        Me.btnFeeder.TabIndex = 1
        Me.btnFeeder.Text = "Feeder"
        Me.btnFeeder.UseVisualStyleBackColor = True
        '
        'btnPumpOn
        '
        Me.btnPumpOn.Location = New System.Drawing.Point(175, 187)
        Me.btnPumpOn.Name = "btnPumpOn"
        Me.btnPumpOn.Size = New System.Drawing.Size(153, 52)
        Me.btnPumpOn.TabIndex = 4
        Me.btnPumpOn.Text = "Pump"
        Me.btnPumpOn.UseVisualStyleBackColor = True
        '
        'btnLights
        '
        Me.btnLights.Location = New System.Drawing.Point(175, 12)
        Me.btnLights.Name = "btnLights"
        Me.btnLights.Size = New System.Drawing.Size(153, 52)
        Me.btnLights.TabIndex = 7
        Me.btnLights.Text = "Lights"
        Me.btnLights.UseVisualStyleBackColor = True
        '
        'btnTone
        '
        Me.btnTone.Location = New System.Drawing.Point(175, 70)
        Me.btnTone.Name = "btnTone"
        Me.btnTone.Size = New System.Drawing.Size(153, 52)
        Me.btnTone.TabIndex = 8
        Me.btnTone.Text = "Tone"
        Me.btnTone.UseVisualStyleBackColor = True
        '
        'tmrStart
        '
        Me.tmrStart.Enabled = True
        '
        'Chart1
        '
        Me.Chart1.BackColor = System.Drawing.Color.Transparent
        ChartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.Transparent
        ChartArea1.AxisX2.TitleForeColor = System.Drawing.Color.Bisque
        ChartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.Transparent
        ChartArea1.BackSecondaryColor = System.Drawing.Color.White
        ChartArea1.Name = "ChartArea1"
        Me.Chart1.ChartAreas.Add(ChartArea1)
        Legend1.BackColor = System.Drawing.Color.Transparent
        Legend1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.125!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Legend1.IsTextAutoFit = False
        Legend1.Name = "Legend1"
        Legend1.TitleFont = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Chart1.Legends.Add(Legend1)
        Me.Chart1.Location = New System.Drawing.Point(347, 12)
        Me.Chart1.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.Chart1.Name = "Chart1"
        Me.Chart1.Size = New System.Drawing.Size(383, 403)
        Me.Chart1.TabIndex = 33
        '
        'tmrFeeder
        '
        Me.tmrFeeder.Interval = 1000
        '
        'Tests
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(738, 426)
        Me.Controls.Add(Me.Chart1)
        Me.Controls.Add(Me.btnTone)
        Me.Controls.Add(Me.btnLights)
        Me.Controls.Add(Me.btnPumpOn)
        Me.Controls.Add(Me.btnFeeder)
        Me.Controls.Add(Me.btnClose)
        Me.Name = "Tests"
        Me.Text = "Tests"
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnClose As Button
    Friend WithEvents btnFeeder As Button
    Friend WithEvents btnPumpOn As Button
    Friend WithEvents btnLights As Button
    Friend WithEvents btnTone As Button
    Friend WithEvents tmrStart As Timer
    Friend WithEvents Chart1 As DataVisualization.Charting.Chart
    Friend WithEvents tmrFeeder As Timer
End Class
