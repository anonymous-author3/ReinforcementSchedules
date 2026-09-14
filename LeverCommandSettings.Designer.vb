<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LeverCommandSettings
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
        Me.lblInstructions = New System.Windows.Forms.Label()
        Me.lblInputHeader = New System.Windows.Forms.Label()
        Me.lblLetterHeader = New System.Windows.Forms.Label()
        Me.lblPreviewHeader = New System.Windows.Forms.Label()
        Me.lblLeverInput1 = New System.Windows.Forms.Label()
        Me.txtLeverLetter1 = New System.Windows.Forms.TextBox()
        Me.lblCommandPreview1 = New System.Windows.Forms.Label()
        Me.lblLeverInput2 = New System.Windows.Forms.Label()
        Me.txtLeverLetter2 = New System.Windows.Forms.TextBox()
        Me.lblCommandPreview2 = New System.Windows.Forms.Label()
        Me.lblLeverInput3 = New System.Windows.Forms.Label()
        Me.txtLeverLetter3 = New System.Windows.Forms.TextBox()
        Me.lblCommandPreview3 = New System.Windows.Forms.Label()
        Me.lblLeverInput4 = New System.Windows.Forms.Label()
        Me.txtLeverLetter4 = New System.Windows.Forms.TextBox()
        Me.lblCommandPreview4 = New System.Windows.Forms.Label()
        Me.lblLeverInput5 = New System.Windows.Forms.Label()
        Me.txtLeverLetter5 = New System.Windows.Forms.TextBox()
        Me.lblCommandPreview5 = New System.Windows.Forms.Label()
        Me.lblLeverInput6 = New System.Windows.Forms.Label()
        Me.txtLeverLetter6 = New System.Windows.Forms.TextBox()
        Me.lblCommandPreview6 = New System.Windows.Forms.Label()
        Me.btnDefaults = New System.Windows.Forms.Button()
        Me.btnApply = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.lblInstructions.Name = "lblInstructions"
        Me.lblInstructions.Location = New System.Drawing.Point(15, 15)
        Me.lblInstructions.Size = New System.Drawing.Size(365, 60)
        Me.lblInstructions.TabIndex = 0
        Me.lblInstructions.Text = "Choose the letter each Arduino input already uses. Uppercase inserts the lever; lowercase retracts it. These letters are included when you save the program."
        Me.lblInputHeader.Name = "lblInputHeader"
        Me.lblInputHeader.Location = New System.Drawing.Point(15, 85)
        Me.lblInputHeader.Size = New System.Drawing.Size(105, 20)
        Me.lblInputHeader.TabIndex = 1
        Me.lblInputHeader.Text = "Input / lever"
        Me.lblLetterHeader.Name = "lblLetterHeader"
        Me.lblLetterHeader.Location = New System.Drawing.Point(130, 85)
        Me.lblLetterHeader.Size = New System.Drawing.Size(65, 20)
        Me.lblLetterHeader.TabIndex = 2
        Me.lblLetterHeader.Text = "Letter"
        Me.lblPreviewHeader.Name = "lblPreviewHeader"
        Me.lblPreviewHeader.Location = New System.Drawing.Point(220, 85)
        Me.lblPreviewHeader.Size = New System.Drawing.Size(150, 20)
        Me.lblPreviewHeader.TabIndex = 3
        Me.lblPreviewHeader.Text = "Insert / Retract"
        Me.lblLeverInput1.Name = "lblLeverInput1"
        Me.lblLeverInput1.Location = New System.Drawing.Point(15, 111)
        Me.lblLeverInput1.Size = New System.Drawing.Size(105, 22)
        Me.lblLeverInput1.TabIndex = 4
        Me.lblLeverInput1.Text = "Input 1"
        Me.txtLeverLetter1.Name = "txtLeverLetter1"
        Me.txtLeverLetter1.Location = New System.Drawing.Point(130, 111)
        Me.txtLeverLetter1.Size = New System.Drawing.Size(45, 20)
        Me.txtLeverLetter1.TabIndex = 5
        Me.txtLeverLetter1.MaxLength = 1
        Me.txtLeverLetter1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtLeverLetter1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverLetter1.Tag = 0
        Me.lblCommandPreview1.Name = "lblCommandPreview1"
        Me.lblCommandPreview1.Location = New System.Drawing.Point(220, 111)
        Me.lblCommandPreview1.Size = New System.Drawing.Size(150, 22)
        Me.lblCommandPreview1.TabIndex = 6
        Me.lblCommandPreview1.Text = "— / —"
        Me.lblLeverInput2.Name = "lblLeverInput2"
        Me.lblLeverInput2.Location = New System.Drawing.Point(15, 143)
        Me.lblLeverInput2.Size = New System.Drawing.Size(105, 22)
        Me.lblLeverInput2.TabIndex = 7
        Me.lblLeverInput2.Text = "Input 2"
        Me.txtLeverLetter2.Name = "txtLeverLetter2"
        Me.txtLeverLetter2.Location = New System.Drawing.Point(130, 143)
        Me.txtLeverLetter2.Size = New System.Drawing.Size(45, 20)
        Me.txtLeverLetter2.TabIndex = 8
        Me.txtLeverLetter2.MaxLength = 1
        Me.txtLeverLetter2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtLeverLetter2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverLetter2.Tag = 1
        Me.lblCommandPreview2.Name = "lblCommandPreview2"
        Me.lblCommandPreview2.Location = New System.Drawing.Point(220, 143)
        Me.lblCommandPreview2.Size = New System.Drawing.Size(150, 22)
        Me.lblCommandPreview2.TabIndex = 9
        Me.lblCommandPreview2.Text = "— / —"
        Me.lblLeverInput3.Name = "lblLeverInput3"
        Me.lblLeverInput3.Location = New System.Drawing.Point(15, 175)
        Me.lblLeverInput3.Size = New System.Drawing.Size(105, 22)
        Me.lblLeverInput3.TabIndex = 10
        Me.lblLeverInput3.Text = "Input 3"
        Me.txtLeverLetter3.Name = "txtLeverLetter3"
        Me.txtLeverLetter3.Location = New System.Drawing.Point(130, 175)
        Me.txtLeverLetter3.Size = New System.Drawing.Size(45, 20)
        Me.txtLeverLetter3.TabIndex = 11
        Me.txtLeverLetter3.MaxLength = 1
        Me.txtLeverLetter3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtLeverLetter3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverLetter3.Tag = 2
        Me.lblCommandPreview3.Name = "lblCommandPreview3"
        Me.lblCommandPreview3.Location = New System.Drawing.Point(220, 175)
        Me.lblCommandPreview3.Size = New System.Drawing.Size(150, 22)
        Me.lblCommandPreview3.TabIndex = 12
        Me.lblCommandPreview3.Text = "— / —"
        Me.lblLeverInput4.Name = "lblLeverInput4"
        Me.lblLeverInput4.Location = New System.Drawing.Point(15, 207)
        Me.lblLeverInput4.Size = New System.Drawing.Size(105, 22)
        Me.lblLeverInput4.TabIndex = 13
        Me.lblLeverInput4.Text = "Input 4"
        Me.txtLeverLetter4.Name = "txtLeverLetter4"
        Me.txtLeverLetter4.Location = New System.Drawing.Point(130, 207)
        Me.txtLeverLetter4.Size = New System.Drawing.Size(45, 20)
        Me.txtLeverLetter4.TabIndex = 14
        Me.txtLeverLetter4.MaxLength = 1
        Me.txtLeverLetter4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtLeverLetter4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverLetter4.Tag = 3
        Me.lblCommandPreview4.Name = "lblCommandPreview4"
        Me.lblCommandPreview4.Location = New System.Drawing.Point(220, 207)
        Me.lblCommandPreview4.Size = New System.Drawing.Size(150, 22)
        Me.lblCommandPreview4.TabIndex = 15
        Me.lblCommandPreview4.Text = "— / —"
        Me.lblLeverInput5.Name = "lblLeverInput5"
        Me.lblLeverInput5.Location = New System.Drawing.Point(15, 239)
        Me.lblLeverInput5.Size = New System.Drawing.Size(105, 22)
        Me.lblLeverInput5.TabIndex = 16
        Me.lblLeverInput5.Text = "Input 5"
        Me.txtLeverLetter5.Name = "txtLeverLetter5"
        Me.txtLeverLetter5.Location = New System.Drawing.Point(130, 239)
        Me.txtLeverLetter5.Size = New System.Drawing.Size(45, 20)
        Me.txtLeverLetter5.TabIndex = 17
        Me.txtLeverLetter5.MaxLength = 1
        Me.txtLeverLetter5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtLeverLetter5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverLetter5.Tag = 4
        Me.lblCommandPreview5.Name = "lblCommandPreview5"
        Me.lblCommandPreview5.Location = New System.Drawing.Point(220, 239)
        Me.lblCommandPreview5.Size = New System.Drawing.Size(150, 22)
        Me.lblCommandPreview5.TabIndex = 18
        Me.lblCommandPreview5.Text = "— / —"
        Me.lblLeverInput6.Name = "lblLeverInput6"
        Me.lblLeverInput6.Location = New System.Drawing.Point(15, 271)
        Me.lblLeverInput6.Size = New System.Drawing.Size(105, 22)
        Me.lblLeverInput6.TabIndex = 19
        Me.lblLeverInput6.Text = "Input 6"
        Me.txtLeverLetter6.Name = "txtLeverLetter6"
        Me.txtLeverLetter6.Location = New System.Drawing.Point(130, 271)
        Me.txtLeverLetter6.Size = New System.Drawing.Size(45, 20)
        Me.txtLeverLetter6.TabIndex = 20
        Me.txtLeverLetter6.MaxLength = 1
        Me.txtLeverLetter6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtLeverLetter6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverLetter6.Tag = 5
        Me.lblCommandPreview6.Name = "lblCommandPreview6"
        Me.lblCommandPreview6.Location = New System.Drawing.Point(220, 271)
        Me.lblCommandPreview6.Size = New System.Drawing.Size(150, 22)
        Me.lblCommandPreview6.TabIndex = 21
        Me.lblCommandPreview6.Text = "— / —"
        Me.btnDefaults.Name = "btnDefaults"
        Me.btnDefaults.Location = New System.Drawing.Point(115, 312)
        Me.btnDefaults.Size = New System.Drawing.Size(80, 28)
        Me.btnDefaults.TabIndex = 22
        Me.btnDefaults.Text = "Defaults"
        Me.btnDefaults.UseVisualStyleBackColor = True
        Me.btnApply.Name = "btnApply"
        Me.btnApply.Location = New System.Drawing.Point(201, 312)
        Me.btnApply.Size = New System.Drawing.Size(80, 28)
        Me.btnApply.TabIndex = 23
        Me.btnApply.Text = "Apply"
        Me.btnApply.UseVisualStyleBackColor = True
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Location = New System.Drawing.Point(287, 312)
        Me.btnCancel.Size = New System.Drawing.Size(80, 28)
        Me.btnCancel.TabIndex = 24
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.AcceptButton = Me.btnApply
        Me.CancelButton = Me.btnCancel
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(395, 355)
        Me.Name = "LeverCommandSettings"
        Me.Text = "Lever commands"
        Me.Controls.Add(Me.lblInstructions)
        Me.Controls.Add(Me.lblInputHeader)
        Me.Controls.Add(Me.lblLetterHeader)
        Me.Controls.Add(Me.lblPreviewHeader)
        Me.Controls.Add(Me.lblLeverInput1)
        Me.Controls.Add(Me.txtLeverLetter1)
        Me.Controls.Add(Me.lblCommandPreview1)
        Me.Controls.Add(Me.lblLeverInput2)
        Me.Controls.Add(Me.txtLeverLetter2)
        Me.Controls.Add(Me.lblCommandPreview2)
        Me.Controls.Add(Me.lblLeverInput3)
        Me.Controls.Add(Me.txtLeverLetter3)
        Me.Controls.Add(Me.lblCommandPreview3)
        Me.Controls.Add(Me.lblLeverInput4)
        Me.Controls.Add(Me.txtLeverLetter4)
        Me.Controls.Add(Me.lblCommandPreview4)
        Me.Controls.Add(Me.lblLeverInput5)
        Me.Controls.Add(Me.txtLeverLetter5)
        Me.Controls.Add(Me.lblCommandPreview5)
        Me.Controls.Add(Me.lblLeverInput6)
        Me.Controls.Add(Me.txtLeverLetter6)
        Me.Controls.Add(Me.lblCommandPreview6)
        Me.Controls.Add(Me.btnDefaults)
        Me.Controls.Add(Me.btnApply)
        Me.Controls.Add(Me.btnCancel)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblInstructions As System.Windows.Forms.Label
    Friend WithEvents lblInputHeader As System.Windows.Forms.Label
    Friend WithEvents lblLetterHeader As System.Windows.Forms.Label
    Friend WithEvents lblPreviewHeader As System.Windows.Forms.Label
    Friend WithEvents lblLeverInput1 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter1 As System.Windows.Forms.TextBox
    Friend WithEvents lblCommandPreview1 As System.Windows.Forms.Label
    Friend WithEvents lblLeverInput2 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter2 As System.Windows.Forms.TextBox
    Friend WithEvents lblCommandPreview2 As System.Windows.Forms.Label
    Friend WithEvents lblLeverInput3 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter3 As System.Windows.Forms.TextBox
    Friend WithEvents lblCommandPreview3 As System.Windows.Forms.Label
    Friend WithEvents lblLeverInput4 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter4 As System.Windows.Forms.TextBox
    Friend WithEvents lblCommandPreview4 As System.Windows.Forms.Label
    Friend WithEvents lblLeverInput5 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter5 As System.Windows.Forms.TextBox
    Friend WithEvents lblCommandPreview5 As System.Windows.Forms.Label
    Friend WithEvents lblLeverInput6 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter6 As System.Windows.Forms.TextBox
    Friend WithEvents lblCommandPreview6 As System.Windows.Forms.Label
    Friend WithEvents btnDefaults As System.Windows.Forms.Button
    Friend WithEvents btnApply As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
End Class
