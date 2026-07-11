<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetUp
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SetUp))
        Me.btnComenzar = New System.Windows.Forms.Button()
        Me.txtCOM = New System.Windows.Forms.TextBox()
        Me.txtSession = New System.Windows.Forms.TextBox()
        Me.lblSesion = New System.Windows.Forms.Label()
        Me.txtSubject = New System.Windows.Forms.TextBox()
        Me.lbl = New System.Windows.Forms.Label()
        Me.lblSujeto = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.txbStart = New System.Windows.Forms.TextBox()
        Me.btnAddComponent = New System.Windows.Forms.Button()
        Me.txbICI = New System.Windows.Forms.TextBox()
        Me.lblICI = New System.Windows.Forms.Label()
        Me.txbPostSession = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.txbComponentOrder = New System.Windows.Forms.TextBox()
        Me.chkICIEnabled = New System.Windows.Forms.CheckBox()
        Me.grpICI = New System.Windows.Forms.GroupBox()
        Me.chkICIHouselight = New System.Windows.Forms.CheckBox()
        Me.chkICITone = New System.Windows.Forms.CheckBox()
        Me.chkICILight4 = New System.Windows.Forms.CheckBox()
        Me.chkICILight3 = New System.Windows.Forms.CheckBox()
        Me.chkICILight2 = New System.Windows.Forms.CheckBox()
        Me.chkICILight1 = New System.Windows.Forms.CheckBox()
        Me.chkICIRetractInputs = New System.Windows.Forms.CheckBox()
        Me.lblICIDuration = New System.Windows.Forms.Label()
        Me.btnTests = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtWeight = New System.Windows.Forms.TextBox()
        Me.btnRemoveLast = New System.Windows.Forms.Button()
        Me.btnAuthorInfo = New System.Windows.Forms.Button()
        Me.dgvComponentSummary = New System.Windows.Forms.DataGridView()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.grpICI.SuspendLayout()
        CType(Me.dgvComponentSummary, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnComenzar
        '
        Me.btnComenzar.Location = New System.Drawing.Point(16, 356)
        Me.btnComenzar.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
        Me.btnComenzar.Name = "btnComenzar"
        Me.btnComenzar.Size = New System.Drawing.Size(120, 40)
        Me.btnComenzar.TabIndex = 0
        Me.btnComenzar.Text = "Start"
        Me.btnComenzar.UseVisualStyleBackColor = True
        '
        'txtSubject
        '
        Me.txtSubject.Location = New System.Drawing.Point(68, 7)
        Me.txtSubject.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
        Me.txtSubject.Name = "txtSubject"
        Me.txtSubject.Size = New System.Drawing.Size(62, 20)
        Me.txtSubject.TabIndex = 1
        Me.txtSubject.Text = "S1"
        '
        'lblSujeto
        '
        Me.lblSujeto.AutoSize = True
        Me.lblSujeto.Location = New System.Drawing.Point(12, 10)
        Me.lblSujeto.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.lblSujeto.Name = "lblSujeto"
        Me.lblSujeto.Size = New System.Drawing.Size(46, 13)
        Me.lblSujeto.TabIndex = 2
        Me.lblSujeto.Text = "Subject:"
        '
        'txtSession
        '
        Me.txtSession.Location = New System.Drawing.Point(68, 29)
        Me.txtSession.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
        Me.txtSession.Name = "txtSession"
        Me.txtSession.Size = New System.Drawing.Size(62, 20)
        Me.txtSession.TabIndex = 3
        Me.txtSession.Text = "1"
        '
        'lblSesion
        '
        Me.lblSesion.AutoSize = True
        Me.lblSesion.Location = New System.Drawing.Point(12, 33)
        Me.lblSesion.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.lblSesion.Name = "lblSesion"
        Me.lblSesion.Size = New System.Drawing.Size(47, 13)
        Me.lblSesion.TabIndex = 4
        Me.lblSesion.Text = "Session:"
        '
        'txtWeight
        '
        Me.txtWeight.Location = New System.Drawing.Point(68, 51)
        Me.txtWeight.Name = "txtWeight"
        Me.txtWeight.Size = New System.Drawing.Size(62, 20)
        Me.txtWeight.TabIndex = 5
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 53)
        Me.Label2.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(44, 13)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "Weight:"
        '
        'txtCOM
        '
        Me.txtCOM.Location = New System.Drawing.Point(68, 72)
        Me.txtCOM.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
        Me.txtCOM.Name = "txtCOM"
        Me.txtCOM.Size = New System.Drawing.Size(62, 20)
        Me.txtCOM.TabIndex = 7
        Me.txtCOM.Text = "COM3"
        '
        'lbl
        '
        Me.lbl.AutoSize = True
        Me.lbl.Location = New System.Drawing.Point(12, 75)
        Me.lbl.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.lbl.Name = "lbl"
        Me.lbl.Size = New System.Drawing.Size(29, 13)
        Me.lbl.TabIndex = 8
        Me.lbl.Text = "Port:"
        '
        'txbStart
        '
        Me.txbStart.Location = New System.Drawing.Point(68, 96)
        Me.txbStart.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.txbStart.Name = "txbStart"
        Me.txbStart.Size = New System.Drawing.Size(62, 20)
        Me.txbStart.TabIndex = 9
        Me.txbStart.Text = "5"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(12, 97)
        Me.Label6.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(56, 13)
        Me.Label6.TabIndex = 10
        Me.Label6.Text = "Start after:"
        '
        'txbPostSession
        '
        Me.txbPostSession.Location = New System.Drawing.Point(68, 118)
        Me.txbPostSession.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.txbPostSession.Name = "txbPostSession"
        Me.txbPostSession.Size = New System.Drawing.Size(62, 20)
        Me.txbPostSession.TabIndex = 11
        Me.txbPostSession.Text = "10"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 122)
        Me.Label1.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(53, 13)
        Me.Label1.TabIndex = 12
        Me.Label1.Text = "End after:"
        '
        'chkICIEnabled
        '
        Me.chkICIEnabled.AutoSize = True
        Me.chkICIEnabled.Location = New System.Drawing.Point(70, 143)
        Me.chkICIEnabled.Name = "chkICIEnabled"
        Me.chkICIEnabled.Size = New System.Drawing.Size(15, 14)
        Me.chkICIEnabled.TabIndex = 13
        Me.chkICIEnabled.UseVisualStyleBackColor = True
        '
        'lblICI
        '
        Me.lblICI.AutoSize = True
        Me.lblICI.Location = New System.Drawing.Point(12, 143)
        Me.lblICI.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.lblICI.Name = "lblICI"
        Me.lblICI.Size = New System.Drawing.Size(45, 13)
        Me.lblICI.TabIndex = 14
        Me.lblICI.Text = "Use ICI:"
        '
        'grpICI
        '
        Me.grpICI.Controls.Add(Me.chkICIHouselight)
        Me.grpICI.Controls.Add(Me.chkICITone)
        Me.grpICI.Controls.Add(Me.chkICILight4)
        Me.grpICI.Controls.Add(Me.chkICILight3)
        Me.grpICI.Controls.Add(Me.chkICILight2)
        Me.grpICI.Controls.Add(Me.chkICILight1)
        Me.grpICI.Controls.Add(Me.chkICIRetractInputs)
        Me.grpICI.Controls.Add(Me.lblICIDuration)
        Me.grpICI.Controls.Add(Me.txbICI)
        Me.grpICI.Location = New System.Drawing.Point(142, 143)
        Me.grpICI.Name = "grpICI"
        Me.grpICI.Size = New System.Drawing.Size(140, 157)
        Me.grpICI.TabIndex = 15
        Me.grpICI.TabStop = False
        Me.grpICI.Text = "ICI"
        '
        'lblICIDuration
        '
        Me.lblICIDuration.AutoSize = True
        Me.lblICIDuration.Location = New System.Drawing.Point(11, 23)
        Me.lblICIDuration.Name = "lblICIDuration"
        Me.lblICIDuration.Size = New System.Drawing.Size(50, 13)
        Me.lblICIDuration.TabIndex = 0
        Me.lblICIDuration.Text = "Duration:"
        '
        'txbICI
        '
        Me.txbICI.Location = New System.Drawing.Point(73, 21)
        Me.txbICI.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.txbICI.Name = "txbICI"
        Me.txbICI.Size = New System.Drawing.Size(62, 20)
        Me.txbICI.TabIndex = 1
        Me.txbICI.Text = "5"
        '
        'chkICIRetractInputs
        '
        Me.chkICIRetractInputs.AutoSize = True
        Me.chkICIRetractInputs.Checked = True
        Me.chkICIRetractInputs.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkICIRetractInputs.Location = New System.Drawing.Point(14, 46)
        Me.chkICIRetractInputs.Name = "chkICIRetractInputs"
        Me.chkICIRetractInputs.Size = New System.Drawing.Size(92, 17)
        Me.chkICIRetractInputs.TabIndex = 2
        Me.chkICIRetractInputs.Text = "Retract inputs"
        Me.chkICIRetractInputs.UseVisualStyleBackColor = True
        '
        'chkICILight1
        '
        Me.chkICILight1.AutoSize = True
        Me.chkICILight1.Location = New System.Drawing.Point(14, 69)
        Me.chkICILight1.Name = "chkICILight1"
        Me.chkICILight1.Size = New System.Drawing.Size(59, 17)
        Me.chkICILight1.TabIndex = 3
        Me.chkICILight1.Text = "Light 1"
        Me.chkICILight1.UseVisualStyleBackColor = True
        '
        'chkICILight2
        '
        Me.chkICILight2.AutoSize = True
        Me.chkICILight2.Location = New System.Drawing.Point(75, 69)
        Me.chkICILight2.Name = "chkICILight2"
        Me.chkICILight2.Size = New System.Drawing.Size(59, 17)
        Me.chkICILight2.TabIndex = 4
        Me.chkICILight2.Text = "Light 2"
        Me.chkICILight2.UseVisualStyleBackColor = True
        '
        'chkICILight3
        '
        Me.chkICILight3.AutoSize = True
        Me.chkICILight3.Location = New System.Drawing.Point(14, 92)
        Me.chkICILight3.Name = "chkICILight3"
        Me.chkICILight3.Size = New System.Drawing.Size(59, 17)
        Me.chkICILight3.TabIndex = 5
        Me.chkICILight3.Text = "Light 3"
        Me.chkICILight3.UseVisualStyleBackColor = True
        '
        'chkICILight4
        '
        Me.chkICILight4.AutoSize = True
        Me.chkICILight4.Location = New System.Drawing.Point(75, 92)
        Me.chkICILight4.Name = "chkICILight4"
        Me.chkICILight4.Size = New System.Drawing.Size(59, 17)
        Me.chkICILight4.TabIndex = 6
        Me.chkICILight4.Text = "Light 4"
        Me.chkICILight4.UseVisualStyleBackColor = True
        '
        'chkICITone
        '
        Me.chkICITone.AutoSize = True
        Me.chkICITone.Location = New System.Drawing.Point(14, 115)
        Me.chkICITone.Name = "chkICITone"
        Me.chkICITone.Size = New System.Drawing.Size(51, 17)
        Me.chkICITone.TabIndex = 7
        Me.chkICITone.Text = "Tone"
        Me.chkICITone.UseVisualStyleBackColor = True
        '
        'chkICIHouselight
        '
        Me.chkICIHouselight.AutoSize = True
        Me.chkICIHouselight.Location = New System.Drawing.Point(75, 115)
        Me.chkICIHouselight.Name = "chkICIHouselight"
        Me.chkICIHouselight.Size = New System.Drawing.Size(78, 17)
        Me.chkICIHouselight.TabIndex = 8
        Me.chkICIHouselight.Text = "Houselight"
        Me.chkICIHouselight.UseVisualStyleBackColor = True
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(12, 168)
        Me.Label17.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(97, 13)
        Me.Label17.TabIndex = 16
        Me.Label17.Text = "Rand components:"
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Enabled = False
        Me.CheckBox1.Location = New System.Drawing.Point(110, 167)
        Me.CheckBox1.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(15, 14)
        Me.CheckBox1.TabIndex = 17
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(12, 188)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(88, 13)
        Me.Label18.TabIndex = 18
        Me.Label18.Text = "Component order:"
        '
        'txbComponentOrder
        '
        Me.txbComponentOrder.Location = New System.Drawing.Point(15, 204)
        Me.txbComponentOrder.Name = "txbComponentOrder"
        Me.txbComponentOrder.Size = New System.Drawing.Size(119, 20)
        Me.txbComponentOrder.TabIndex = 19
        '
        'btnAddComponent
        '
        Me.btnAddComponent.Location = New System.Drawing.Point(16, 272)
        Me.btnAddComponent.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
        Me.btnAddComponent.Name = "btnAddComponent"
        Me.btnAddComponent.Size = New System.Drawing.Size(120, 40)
        Me.btnAddComponent.TabIndex = 20
        Me.btnAddComponent.Text = "Add Component"
        Me.btnAddComponent.UseVisualStyleBackColor = True
        '
        'btnRemoveLast
        '
        Me.btnRemoveLast.Location = New System.Drawing.Point(16, 314)
        Me.btnRemoveLast.Name = "btnRemoveLast"
        Me.btnRemoveLast.Size = New System.Drawing.Size(120, 40)
        Me.btnRemoveLast.TabIndex = 21
        Me.btnRemoveLast.Text = "Remove Last"
        Me.btnRemoveLast.UseVisualStyleBackColor = True
        '
        'btnComenzar
        '
        'btnTests
        '
        Me.btnTests.Location = New System.Drawing.Point(16, 398)
        Me.btnTests.Name = "btnTests"
        Me.btnTests.Size = New System.Drawing.Size(120, 40)
        Me.btnTests.TabIndex = 22
        Me.btnTests.Text = "Tests"
        Me.btnTests.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(16, 230)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(58, 36)
        Me.btnSave.TabIndex = 23
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnLoad
        '
        Me.btnLoad.Location = New System.Drawing.Point(78, 230)
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.Size = New System.Drawing.Size(58, 36)
        Me.btnLoad.TabIndex = 24
        Me.btnLoad.Text = "Load"
        Me.btnLoad.UseVisualStyleBackColor = True
        '
        'btnAuthorInfo
        '
        Me.btnAuthorInfo.Location = New System.Drawing.Point(16, 442)
        Me.btnAuthorInfo.Name = "btnAuthorInfo"
        Me.btnAuthorInfo.Size = New System.Drawing.Size(120, 27)
        Me.btnAuthorInfo.TabIndex = 25
        Me.btnAuthorInfo.Text = "Info"
        Me.btnAuthorInfo.UseVisualStyleBackColor = True
        '
        'dgvComponentSummary
        '
        Me.dgvComponentSummary.AllowUserToAddRows = False
        Me.dgvComponentSummary.AllowUserToDeleteRows = False
        Me.dgvComponentSummary.AllowUserToResizeRows = False
        Me.dgvComponentSummary.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells
        Me.dgvComponentSummary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvComponentSummary.Location = New System.Drawing.Point(288, 12)
        Me.dgvComponentSummary.Name = "dgvComponentSummary"
        Me.dgvComponentSummary.ReadOnly = True
        Me.dgvComponentSummary.RowHeadersVisible = False
        Me.dgvComponentSummary.RowHeadersWidth = 62
        Me.dgvComponentSummary.RowTemplate.Height = 28
        Me.dgvComponentSummary.Size = New System.Drawing.Size(630, 457)
        Me.dgvComponentSummary.TabIndex = 26
        '
        'SetUp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(930, 484)
        Me.Controls.Add(Me.Label18)
        Me.Controls.Add(Me.dgvComponentSummary)
        Me.Controls.Add(Me.grpICI)
        Me.Controls.Add(Me.chkICIEnabled)
        Me.Controls.Add(Me.btnAuthorInfo)
        Me.Controls.Add(Me.btnRemoveLast)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtWeight)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnTests)
        Me.Controls.Add(Me.txbComponentOrder)
        Me.Controls.Add(Me.CheckBox1)
        Me.Controls.Add(Me.Label17)
        Me.Controls.Add(Me.txbPostSession)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblICI)
        Me.Controls.Add(Me.btnAddComponent)
        Me.Controls.Add(Me.txbStart)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.lblSujeto)
        Me.Controls.Add(Me.lbl)
        Me.Controls.Add(Me.btnComenzar)
        Me.Controls.Add(Me.txtSubject)
        Me.Controls.Add(Me.lblSesion)
        Me.Controls.Add(Me.txtCOM)
        Me.Controls.Add(Me.txtSession)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(2, 1, 2, 1)
        Me.Name = "SetUp"
        Me.Text = "SetUp"
        Me.grpICI.ResumeLayout(False)
        Me.grpICI.PerformLayout()
        CType(Me.dgvComponentSummary, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnComenzar As Button
    Friend WithEvents txtCOM As TextBox
    Friend WithEvents txtSession As TextBox
    Friend WithEvents lblSesion As Label
    Friend WithEvents txtSubject As TextBox
    Friend WithEvents lbl As Label
    Friend WithEvents lblSujeto As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents txbStart As TextBox
    Friend WithEvents btnAddComponent As Button
    Friend WithEvents txbICI As TextBox
    Friend WithEvents lblICI As Label
    Friend WithEvents txbPostSession As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label17 As Label
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents txbComponentOrder As TextBox
    Friend WithEvents btnTests As Button
    Friend WithEvents btnSave As Button
    Friend WithEvents btnLoad As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents txtWeight As TextBox
    Friend WithEvents btnRemoveLast As Button
    Friend WithEvents btnAuthorInfo As Button
    Friend WithEvents dgvComponentSummary As DataGridView
    Friend WithEvents chkICIEnabled As CheckBox
    Friend WithEvents grpICI As GroupBox
    Friend WithEvents chkICIHouselight As CheckBox
    Friend WithEvents chkICITone As CheckBox
    Friend WithEvents chkICILight4 As CheckBox
    Friend WithEvents chkICILight3 As CheckBox
    Friend WithEvents chkICILight2 As CheckBox
    Friend WithEvents chkICILight1 As CheckBox
    Friend WithEvents chkICIRetractInputs As CheckBox
    Friend WithEvents lblICIDuration As Label
    Friend WithEvents Label18 As Label
End Class
