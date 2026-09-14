<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Tests
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
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series2 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series3 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series4 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series5 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series6 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Me.btnLights = New System.Windows.Forms.Button()
        Me.btnTone = New System.Windows.Forms.Button()
        Me.btnFeeder = New System.Windows.Forms.Button()
        Me.btnPumpOn = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.lblInputName1 = New System.Windows.Forms.Label()
        Me.lblInputValue1 = New System.Windows.Forms.Label()
        Me.lblInputCount1 = New System.Windows.Forms.Label()
        Me.btnInputOutput1 = New System.Windows.Forms.Button()
        Me.lblInputName2 = New System.Windows.Forms.Label()
        Me.lblInputValue2 = New System.Windows.Forms.Label()
        Me.lblInputCount2 = New System.Windows.Forms.Label()
        Me.btnInputOutput2 = New System.Windows.Forms.Button()
        Me.lblInputName3 = New System.Windows.Forms.Label()
        Me.lblInputValue3 = New System.Windows.Forms.Label()
        Me.lblInputCount3 = New System.Windows.Forms.Label()
        Me.btnInputOutput3 = New System.Windows.Forms.Button()
        Me.lblInputName4 = New System.Windows.Forms.Label()
        Me.lblInputValue4 = New System.Windows.Forms.Label()
        Me.lblInputCount4 = New System.Windows.Forms.Label()
        Me.btnInputOutput4 = New System.Windows.Forms.Button()
        Me.lblInputName5 = New System.Windows.Forms.Label()
        Me.lblInputValue5 = New System.Windows.Forms.Label()
        Me.lblInputCount5 = New System.Windows.Forms.Label()
        Me.btnInputOutput5 = New System.Windows.Forms.Button()
        Me.lblInputName6 = New System.Windows.Forms.Label()
        Me.lblInputValue6 = New System.Windows.Forms.Label()
        Me.lblInputCount6 = New System.Windows.Forms.Label()
        Me.btnInputOutput6 = New System.Windows.Forms.Button()
        Me.Chart1 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.tmrStart = New System.Windows.Forms.Timer(Me.components)
        Me.tmrFeeder = New System.Windows.Forms.Timer(Me.components)
        Me.tmrChartTests = New System.Windows.Forms.Timer(Me.components)
        Me.acquisitionTimer = New System.Windows.Forms.Timer(Me.components)
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        Me.btnLights.Name = "btnLights"
        Me.btnLights.Location = New System.Drawing.Point(263, 18)
        Me.btnLights.Size = New System.Drawing.Size(153, 52)
        Me.btnLights.TabIndex = 0
        Me.btnLights.Text = "Lights"
        Me.btnLights.UseVisualStyleBackColor = True
        Me.btnTone.Name = "btnTone"
        Me.btnTone.Location = New System.Drawing.Point(263, 108)
        Me.btnTone.Size = New System.Drawing.Size(153, 52)
        Me.btnTone.TabIndex = 1
        Me.btnTone.Text = "Tone"
        Me.btnTone.UseVisualStyleBackColor = True
        Me.btnFeeder.Name = "btnFeeder"
        Me.btnFeeder.Location = New System.Drawing.Point(263, 198)
        Me.btnFeeder.Size = New System.Drawing.Size(153, 52)
        Me.btnFeeder.TabIndex = 2
        Me.btnFeeder.Text = "Feeder"
        Me.btnFeeder.UseVisualStyleBackColor = True
        Me.btnPumpOn.Name = "btnPumpOn"
        Me.btnPumpOn.Location = New System.Drawing.Point(263, 288)
        Me.btnPumpOn.Size = New System.Drawing.Size(153, 52)
        Me.btnPumpOn.TabIndex = 3
        Me.btnPumpOn.Text = "Pump"
        Me.btnPumpOn.UseVisualStyleBackColor = True
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Location = New System.Drawing.Point(263, 558)
        Me.btnClose.Size = New System.Drawing.Size(153, 52)
        Me.btnClose.TabIndex = 4
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = True
        Me.lblInputName1.Name = "lblInputName1"
        Me.lblInputName1.Location = New System.Drawing.Point(13, 18)
        Me.lblInputName1.Size = New System.Drawing.Size(110, 22)
        Me.lblInputName1.TabIndex = 5
        Me.lblInputName1.Text = "Input 1:"
        Me.lblInputValue1.Name = "lblInputValue1"
        Me.lblInputValue1.Location = New System.Drawing.Point(128, 18)
        Me.lblInputValue1.Size = New System.Drawing.Size(45, 22)
        Me.lblInputValue1.TabIndex = 6
        Me.lblInputValue1.Text = "-"
        Me.lblInputValue1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputValue1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblInputCount1.Name = "lblInputCount1"
        Me.lblInputCount1.Location = New System.Drawing.Point(178, 18)
        Me.lblInputCount1.Size = New System.Drawing.Size(55, 22)
        Me.lblInputCount1.TabIndex = 7
        Me.lblInputCount1.Text = "0"
        Me.lblInputCount1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputCount1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.btnInputOutput1.Name = "btnInputOutput1"
        Me.btnInputOutput1.Location = New System.Drawing.Point(13, 250)
        Me.btnInputOutput1.Size = New System.Drawing.Size(230, 30)
        Me.btnInputOutput1.TabIndex = 8
        Me.btnInputOutput1.Text = "Input 1 output OFF"
        Me.btnInputOutput1.UseVisualStyleBackColor = True
        Me.btnInputOutput1.Tag = 0
        Me.lblInputName2.Name = "lblInputName2"
        Me.lblInputName2.Location = New System.Drawing.Point(13, 54)
        Me.lblInputName2.Size = New System.Drawing.Size(110, 22)
        Me.lblInputName2.TabIndex = 9
        Me.lblInputName2.Text = "Input 2:"
        Me.lblInputValue2.Name = "lblInputValue2"
        Me.lblInputValue2.Location = New System.Drawing.Point(128, 54)
        Me.lblInputValue2.Size = New System.Drawing.Size(45, 22)
        Me.lblInputValue2.TabIndex = 10
        Me.lblInputValue2.Text = "-"
        Me.lblInputValue2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputValue2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblInputCount2.Name = "lblInputCount2"
        Me.lblInputCount2.Location = New System.Drawing.Point(178, 54)
        Me.lblInputCount2.Size = New System.Drawing.Size(55, 22)
        Me.lblInputCount2.TabIndex = 11
        Me.lblInputCount2.Text = "0"
        Me.lblInputCount2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputCount2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.btnInputOutput2.Name = "btnInputOutput2"
        Me.btnInputOutput2.Location = New System.Drawing.Point(13, 286)
        Me.btnInputOutput2.Size = New System.Drawing.Size(230, 30)
        Me.btnInputOutput2.TabIndex = 12
        Me.btnInputOutput2.Text = "Input 2 output OFF"
        Me.btnInputOutput2.UseVisualStyleBackColor = True
        Me.btnInputOutput2.Tag = 1
        Me.lblInputName3.Name = "lblInputName3"
        Me.lblInputName3.Location = New System.Drawing.Point(13, 90)
        Me.lblInputName3.Size = New System.Drawing.Size(110, 22)
        Me.lblInputName3.TabIndex = 13
        Me.lblInputName3.Text = "Input 3:"
        Me.lblInputValue3.Name = "lblInputValue3"
        Me.lblInputValue3.Location = New System.Drawing.Point(128, 90)
        Me.lblInputValue3.Size = New System.Drawing.Size(45, 22)
        Me.lblInputValue3.TabIndex = 14
        Me.lblInputValue3.Text = "-"
        Me.lblInputValue3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputValue3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblInputCount3.Name = "lblInputCount3"
        Me.lblInputCount3.Location = New System.Drawing.Point(178, 90)
        Me.lblInputCount3.Size = New System.Drawing.Size(55, 22)
        Me.lblInputCount3.TabIndex = 15
        Me.lblInputCount3.Text = "0"
        Me.lblInputCount3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputCount3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.btnInputOutput3.Name = "btnInputOutput3"
        Me.btnInputOutput3.Location = New System.Drawing.Point(13, 322)
        Me.btnInputOutput3.Size = New System.Drawing.Size(230, 30)
        Me.btnInputOutput3.TabIndex = 16
        Me.btnInputOutput3.Text = "Input 3 output OFF"
        Me.btnInputOutput3.UseVisualStyleBackColor = True
        Me.btnInputOutput3.Tag = 2
        Me.lblInputName4.Name = "lblInputName4"
        Me.lblInputName4.Location = New System.Drawing.Point(13, 126)
        Me.lblInputName4.Size = New System.Drawing.Size(110, 22)
        Me.lblInputName4.TabIndex = 17
        Me.lblInputName4.Text = "Input 4:"
        Me.lblInputValue4.Name = "lblInputValue4"
        Me.lblInputValue4.Location = New System.Drawing.Point(128, 126)
        Me.lblInputValue4.Size = New System.Drawing.Size(45, 22)
        Me.lblInputValue4.TabIndex = 18
        Me.lblInputValue4.Text = "-"
        Me.lblInputValue4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputValue4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblInputCount4.Name = "lblInputCount4"
        Me.lblInputCount4.Location = New System.Drawing.Point(178, 126)
        Me.lblInputCount4.Size = New System.Drawing.Size(55, 22)
        Me.lblInputCount4.TabIndex = 19
        Me.lblInputCount4.Text = "0"
        Me.lblInputCount4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputCount4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.btnInputOutput4.Name = "btnInputOutput4"
        Me.btnInputOutput4.Location = New System.Drawing.Point(13, 358)
        Me.btnInputOutput4.Size = New System.Drawing.Size(230, 30)
        Me.btnInputOutput4.TabIndex = 20
        Me.btnInputOutput4.Text = "Input 4 output OFF"
        Me.btnInputOutput4.UseVisualStyleBackColor = True
        Me.btnInputOutput4.Tag = 3
        Me.lblInputName5.Name = "lblInputName5"
        Me.lblInputName5.Location = New System.Drawing.Point(13, 162)
        Me.lblInputName5.Size = New System.Drawing.Size(110, 22)
        Me.lblInputName5.TabIndex = 21
        Me.lblInputName5.Text = "Input 5:"
        Me.lblInputValue5.Name = "lblInputValue5"
        Me.lblInputValue5.Location = New System.Drawing.Point(128, 162)
        Me.lblInputValue5.Size = New System.Drawing.Size(45, 22)
        Me.lblInputValue5.TabIndex = 22
        Me.lblInputValue5.Text = "-"
        Me.lblInputValue5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputValue5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblInputCount5.Name = "lblInputCount5"
        Me.lblInputCount5.Location = New System.Drawing.Point(178, 162)
        Me.lblInputCount5.Size = New System.Drawing.Size(55, 22)
        Me.lblInputCount5.TabIndex = 23
        Me.lblInputCount5.Text = "0"
        Me.lblInputCount5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputCount5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.btnInputOutput5.Name = "btnInputOutput5"
        Me.btnInputOutput5.Location = New System.Drawing.Point(13, 394)
        Me.btnInputOutput5.Size = New System.Drawing.Size(230, 30)
        Me.btnInputOutput5.TabIndex = 24
        Me.btnInputOutput5.Text = "Input 5 output OFF"
        Me.btnInputOutput5.UseVisualStyleBackColor = True
        Me.btnInputOutput5.Tag = 4
        Me.lblInputName6.Name = "lblInputName6"
        Me.lblInputName6.Location = New System.Drawing.Point(13, 198)
        Me.lblInputName6.Size = New System.Drawing.Size(110, 22)
        Me.lblInputName6.TabIndex = 25
        Me.lblInputName6.Text = "Input 6:"
        Me.lblInputValue6.Name = "lblInputValue6"
        Me.lblInputValue6.Location = New System.Drawing.Point(128, 198)
        Me.lblInputValue6.Size = New System.Drawing.Size(45, 22)
        Me.lblInputValue6.TabIndex = 26
        Me.lblInputValue6.Text = "-"
        Me.lblInputValue6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputValue6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblInputCount6.Name = "lblInputCount6"
        Me.lblInputCount6.Location = New System.Drawing.Point(178, 198)
        Me.lblInputCount6.Size = New System.Drawing.Size(55, 22)
        Me.lblInputCount6.TabIndex = 27
        Me.lblInputCount6.Text = "0"
        Me.lblInputCount6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblInputCount6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.btnInputOutput6.Name = "btnInputOutput6"
        Me.btnInputOutput6.Location = New System.Drawing.Point(13, 430)
        Me.btnInputOutput6.Size = New System.Drawing.Size(230, 30)
        Me.btnInputOutput6.TabIndex = 28
        Me.btnInputOutput6.Text = "Input 6 output OFF"
        Me.btnInputOutput6.UseVisualStyleBackColor = True
        Me.btnInputOutput6.Tag = 5
        Me.Chart1.Name = "Chart1"
        Me.Chart1.Location = New System.Drawing.Point(520, 18)
        Me.Chart1.Size = New System.Drawing.Size(575, 620)
        Me.Chart1.TabIndex = 29
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
        Series1.Name = "Input 1"
        Series1.ChartArea = "ChartArea1"
        Series1.Legend = "Legend1"
        Series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Me.Chart1.Series.Add(Series1)
        Series2.Name = "Input 2"
        Series2.ChartArea = "ChartArea1"
        Series2.Legend = "Legend1"
        Series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Me.Chart1.Series.Add(Series2)
        Series3.Name = "Input 3"
        Series3.ChartArea = "ChartArea1"
        Series3.Legend = "Legend1"
        Series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Me.Chart1.Series.Add(Series3)
        Series4.Name = "Input 4"
        Series4.ChartArea = "ChartArea1"
        Series4.Legend = "Legend1"
        Series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Me.Chart1.Series.Add(Series4)
        Series5.Name = "Input 5"
        Series5.ChartArea = "ChartArea1"
        Series5.Legend = "Legend1"
        Series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Me.Chart1.Series.Add(Series5)
        Series6.Name = "Input 6"
        Series6.ChartArea = "ChartArea1"
        Series6.Legend = "Legend1"
        Series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine
        Me.Chart1.Series.Add(Series6)
        Legend1.BackColor = System.Drawing.Color.Transparent
        Legend1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.125!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Legend1.IsTextAutoFit = False
        Legend1.TitleFont = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Chart1.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.tmrStart.Interval = 100
        Me.tmrFeeder.Interval = 1000
        Me.tmrChartTests.Interval = 100
        Me.acquisitionTimer.Interval = 10
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1107, 655)
        Me.Name = "Tests"
        Me.Text = "Tests"
        Me.Controls.Add(Me.btnLights)
        Me.Controls.Add(Me.btnTone)
        Me.Controls.Add(Me.btnFeeder)
        Me.Controls.Add(Me.btnPumpOn)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.lblInputName1)
        Me.Controls.Add(Me.lblInputValue1)
        Me.Controls.Add(Me.lblInputCount1)
        Me.Controls.Add(Me.btnInputOutput1)
        Me.Controls.Add(Me.lblInputName2)
        Me.Controls.Add(Me.lblInputValue2)
        Me.Controls.Add(Me.lblInputCount2)
        Me.Controls.Add(Me.btnInputOutput2)
        Me.Controls.Add(Me.lblInputName3)
        Me.Controls.Add(Me.lblInputValue3)
        Me.Controls.Add(Me.lblInputCount3)
        Me.Controls.Add(Me.btnInputOutput3)
        Me.Controls.Add(Me.lblInputName4)
        Me.Controls.Add(Me.lblInputValue4)
        Me.Controls.Add(Me.lblInputCount4)
        Me.Controls.Add(Me.btnInputOutput4)
        Me.Controls.Add(Me.lblInputName5)
        Me.Controls.Add(Me.lblInputValue5)
        Me.Controls.Add(Me.lblInputCount5)
        Me.Controls.Add(Me.btnInputOutput5)
        Me.Controls.Add(Me.lblInputName6)
        Me.Controls.Add(Me.lblInputValue6)
        Me.Controls.Add(Me.lblInputCount6)
        Me.Controls.Add(Me.btnInputOutput6)
        Me.Controls.Add(Me.Chart1)
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents btnLights As System.Windows.Forms.Button
    Friend WithEvents btnTone As System.Windows.Forms.Button
    Friend WithEvents btnFeeder As System.Windows.Forms.Button
    Friend WithEvents btnPumpOn As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents lblInputName1 As System.Windows.Forms.Label
    Friend WithEvents lblInputValue1 As System.Windows.Forms.Label
    Friend WithEvents lblInputCount1 As System.Windows.Forms.Label
    Friend WithEvents btnInputOutput1 As System.Windows.Forms.Button
    Friend WithEvents lblInputName2 As System.Windows.Forms.Label
    Friend WithEvents lblInputValue2 As System.Windows.Forms.Label
    Friend WithEvents lblInputCount2 As System.Windows.Forms.Label
    Friend WithEvents btnInputOutput2 As System.Windows.Forms.Button
    Friend WithEvents lblInputName3 As System.Windows.Forms.Label
    Friend WithEvents lblInputValue3 As System.Windows.Forms.Label
    Friend WithEvents lblInputCount3 As System.Windows.Forms.Label
    Friend WithEvents btnInputOutput3 As System.Windows.Forms.Button
    Friend WithEvents lblInputName4 As System.Windows.Forms.Label
    Friend WithEvents lblInputValue4 As System.Windows.Forms.Label
    Friend WithEvents lblInputCount4 As System.Windows.Forms.Label
    Friend WithEvents btnInputOutput4 As System.Windows.Forms.Button
    Friend WithEvents lblInputName5 As System.Windows.Forms.Label
    Friend WithEvents lblInputValue5 As System.Windows.Forms.Label
    Friend WithEvents lblInputCount5 As System.Windows.Forms.Label
    Friend WithEvents btnInputOutput5 As System.Windows.Forms.Button
    Friend WithEvents lblInputName6 As System.Windows.Forms.Label
    Friend WithEvents lblInputValue6 As System.Windows.Forms.Label
    Friend WithEvents lblInputCount6 As System.Windows.Forms.Label
    Friend WithEvents btnInputOutput6 As System.Windows.Forms.Button
    Friend WithEvents Chart1 As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents tmrStart As System.Windows.Forms.Timer
    Friend WithEvents tmrFeeder As System.Windows.Forms.Timer
    Friend WithEvents tmrChartTests As System.Windows.Forms.Timer
    Friend WithEvents acquisitionTimer As System.Windows.Forms.Timer
End Class
