<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LeverCommandSettings
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.lblInstructions = New System.Windows.Forms.Label()
        Me.lblPersistenceWarning = New System.Windows.Forms.Label()
        Me.grpLevers = New System.Windows.Forms.GroupBox()
        Me.lblLeverHeader = New System.Windows.Forms.Label()
        Me.lblInsertHeader = New System.Windows.Forms.Label()
        Me.lblRetractHeader = New System.Windows.Forms.Label()
        Me.lblLeverInput1 = New System.Windows.Forms.Label()
        Me.txtLeverLetter1 = New System.Windows.Forms.TextBox()
        Me.txtLeverRetract1 = New System.Windows.Forms.TextBox()
        Me.lblLeverInput2 = New System.Windows.Forms.Label()
        Me.txtLeverLetter2 = New System.Windows.Forms.TextBox()
        Me.txtLeverRetract2 = New System.Windows.Forms.TextBox()
        Me.lblLeverInput3 = New System.Windows.Forms.Label()
        Me.txtLeverLetter3 = New System.Windows.Forms.TextBox()
        Me.txtLeverRetract3 = New System.Windows.Forms.TextBox()
        Me.lblLeverInput4 = New System.Windows.Forms.Label()
        Me.txtLeverLetter4 = New System.Windows.Forms.TextBox()
        Me.txtLeverRetract4 = New System.Windows.Forms.TextBox()
        Me.lblLeverInput5 = New System.Windows.Forms.Label()
        Me.txtLeverLetter5 = New System.Windows.Forms.TextBox()
        Me.txtLeverRetract5 = New System.Windows.Forms.TextBox()
        Me.lblLeverInput6 = New System.Windows.Forms.Label()
        Me.txtLeverLetter6 = New System.Windows.Forms.TextBox()
        Me.txtLeverRetract6 = New System.Windows.Forms.TextBox()
        Me.lblMapScope = New System.Windows.Forms.Label()
        Me.grpOutputs = New System.Windows.Forms.GroupBox()
        Me.lblOutputHeader = New System.Windows.Forms.Label()
        Me.lblOnHeader = New System.Windows.Forms.Label()
        Me.lblOffHeader = New System.Windows.Forms.Label()
        Me.lblLight1 = New System.Windows.Forms.Label()
        Me.txtLight1On = New System.Windows.Forms.TextBox()
        Me.txtLight1Off = New System.Windows.Forms.TextBox()
        Me.lblLight2 = New System.Windows.Forms.Label()
        Me.txtLight2On = New System.Windows.Forms.TextBox()
        Me.txtLight2Off = New System.Windows.Forms.TextBox()
        Me.lblLight3 = New System.Windows.Forms.Label()
        Me.txtLight3On = New System.Windows.Forms.TextBox()
        Me.txtLight3Off = New System.Windows.Forms.TextBox()
        Me.lblLight4 = New System.Windows.Forms.Label()
        Me.txtLight4On = New System.Windows.Forms.TextBox()
        Me.txtLight4Off = New System.Windows.Forms.TextBox()
        Me.lblHouseLight = New System.Windows.Forms.Label()
        Me.txtHouseLightOn = New System.Windows.Forms.TextBox()
        Me.txtHouseLightOff = New System.Windows.Forms.TextBox()
        Me.lblTone = New System.Windows.Forms.Label()
        Me.txtToneOn = New System.Windows.Forms.TextBox()
        Me.txtToneOff = New System.Windows.Forms.TextBox()
        Me.lblPump = New System.Windows.Forms.Label()
        Me.txtPumpOn = New System.Windows.Forms.TextBox()
        Me.txtPumpOff = New System.Windows.Forms.TextBox()
        Me.lblFeederPulse = New System.Windows.Forms.Label()
        Me.txtFeederPulse = New System.Windows.Forms.TextBox()
        Me.lblPumpPulse = New System.Windows.Forms.Label()
        Me.txtPumpPulse = New System.Windows.Forms.TextBox()
        Me.lblTonePulse = New System.Windows.Forms.Label()
        Me.txtTonePulse = New System.Windows.Forms.TextBox()
        Me.lblAllOff = New System.Windows.Forms.Label()
        Me.txtAllOff = New System.Windows.Forms.TextBox()
        Me.btnDefaults = New System.Windows.Forms.Button()
        Me.btnApply = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.grpLevers.SuspendLayout()
        Me.grpOutputs.SuspendLayout()
        Me.SuspendLayout()
        Me.lblInstructions.Name = "lblInstructions"
        Me.lblInstructions.Location = New System.Drawing.Point(12, 12)
        Me.lblInstructions.Size = New System.Drawing.Size(736, 48)
        Me.lblInstructions.TabIndex = 0
        Me.lblInstructions.Text = "Defaults match the included Arduino sketch (newProgram2026_6inputs_4lights). Enter one letter per command. Uppercase and lowercase are different commands; each action can be remapped independently."
        Me.lblPersistenceWarning.Name = "lblPersistenceWarning"
        Me.lblPersistenceWarning.Location = New System.Drawing.Point(12, 68)
        Me.lblPersistenceWarning.Size = New System.Drawing.Size(736, 108)
        Me.lblPersistenceWarning.TabIndex = 1
        Me.lblPersistenceWarning.Text = "IMPORTANT: Apply changes the current setup only. Use Save in SetUp to store the full command map in a macro. Each time the program starts, it uses the Arduino defaults; use Load to restore a saved map. Closing this window does not reset the map. Remapping does not reprogram Arduino: the letters must match the cases in its sketch."
        Me.lblPersistenceWarning.ForeColor = System.Drawing.Color.Red
        Me.lblPersistenceWarning.BackColor = System.Drawing.Color.MistyRose
        Me.lblPersistenceWarning.Padding = New System.Windows.Forms.Padding(8)
        Me.lblPersistenceWarning.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpLevers.Name = "grpLevers"
        Me.grpLevers.Location = New System.Drawing.Point(12, 188)
        Me.grpLevers.Size = New System.Drawing.Size(344, 264)
        Me.grpLevers.TabIndex = 2
        Me.grpLevers.Text = "Levers"
        Me.lblLeverHeader.Name = "lblLeverHeader"
        Me.lblLeverHeader.Location = New System.Drawing.Point(14, 25)
        Me.lblLeverHeader.Size = New System.Drawing.Size(140, 20)
        Me.lblLeverHeader.TabIndex = 3
        Me.lblLeverHeader.Text = "Action"
        Me.lblInsertHeader.Name = "lblInsertHeader"
        Me.lblInsertHeader.Location = New System.Drawing.Point(178, 25)
        Me.lblInsertHeader.Size = New System.Drawing.Size(70, 20)
        Me.lblInsertHeader.TabIndex = 4
        Me.lblInsertHeader.Text = "Insert"
        Me.lblRetractHeader.Name = "lblRetractHeader"
        Me.lblRetractHeader.Location = New System.Drawing.Point(266, 25)
        Me.lblRetractHeader.Size = New System.Drawing.Size(70, 20)
        Me.lblRetractHeader.TabIndex = 5
        Me.lblRetractHeader.Text = "Retract"
        Me.lblLeverInput1.Name = "lblLeverInput1"
        Me.lblLeverInput1.Location = New System.Drawing.Point(14, 55)
        Me.lblLeverInput1.Size = New System.Drawing.Size(148, 22)
        Me.lblLeverInput1.TabIndex = 6
        Me.lblLeverInput1.Text = "Lever 1"
        Me.txtLeverLetter1.Name = "txtLeverLetter1"
        Me.txtLeverLetter1.Location = New System.Drawing.Point(178, 52)
        Me.txtLeverLetter1.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverLetter1.TabIndex = 7
        Me.txtLeverLetter1.MaxLength = 1
        Me.txtLeverLetter1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverRetract1.Name = "txtLeverRetract1"
        Me.txtLeverRetract1.Location = New System.Drawing.Point(266, 52)
        Me.txtLeverRetract1.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverRetract1.TabIndex = 8
        Me.txtLeverRetract1.MaxLength = 1
        Me.txtLeverRetract1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblLeverInput2.Name = "lblLeverInput2"
        Me.lblLeverInput2.Location = New System.Drawing.Point(14, 87)
        Me.lblLeverInput2.Size = New System.Drawing.Size(148, 22)
        Me.lblLeverInput2.TabIndex = 9
        Me.lblLeverInput2.Text = "Lever 2"
        Me.txtLeverLetter2.Name = "txtLeverLetter2"
        Me.txtLeverLetter2.Location = New System.Drawing.Point(178, 84)
        Me.txtLeverLetter2.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverLetter2.TabIndex = 10
        Me.txtLeverLetter2.MaxLength = 1
        Me.txtLeverLetter2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverRetract2.Name = "txtLeverRetract2"
        Me.txtLeverRetract2.Location = New System.Drawing.Point(266, 84)
        Me.txtLeverRetract2.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverRetract2.TabIndex = 11
        Me.txtLeverRetract2.MaxLength = 1
        Me.txtLeverRetract2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblLeverInput3.Name = "lblLeverInput3"
        Me.lblLeverInput3.Location = New System.Drawing.Point(14, 119)
        Me.lblLeverInput3.Size = New System.Drawing.Size(148, 22)
        Me.lblLeverInput3.TabIndex = 12
        Me.lblLeverInput3.Text = "Lever 3"
        Me.txtLeverLetter3.Name = "txtLeverLetter3"
        Me.txtLeverLetter3.Location = New System.Drawing.Point(178, 116)
        Me.txtLeverLetter3.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverLetter3.TabIndex = 13
        Me.txtLeverLetter3.MaxLength = 1
        Me.txtLeverLetter3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverRetract3.Name = "txtLeverRetract3"
        Me.txtLeverRetract3.Location = New System.Drawing.Point(266, 116)
        Me.txtLeverRetract3.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverRetract3.TabIndex = 14
        Me.txtLeverRetract3.MaxLength = 1
        Me.txtLeverRetract3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblLeverInput4.Name = "lblLeverInput4"
        Me.lblLeverInput4.Location = New System.Drawing.Point(14, 151)
        Me.lblLeverInput4.Size = New System.Drawing.Size(148, 22)
        Me.lblLeverInput4.TabIndex = 15
        Me.lblLeverInput4.Text = "Lever 4"
        Me.txtLeverLetter4.Name = "txtLeverLetter4"
        Me.txtLeverLetter4.Location = New System.Drawing.Point(178, 148)
        Me.txtLeverLetter4.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverLetter4.TabIndex = 16
        Me.txtLeverLetter4.MaxLength = 1
        Me.txtLeverLetter4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverRetract4.Name = "txtLeverRetract4"
        Me.txtLeverRetract4.Location = New System.Drawing.Point(266, 148)
        Me.txtLeverRetract4.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverRetract4.TabIndex = 17
        Me.txtLeverRetract4.MaxLength = 1
        Me.txtLeverRetract4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblLeverInput5.Name = "lblLeverInput5"
        Me.lblLeverInput5.Location = New System.Drawing.Point(14, 183)
        Me.lblLeverInput5.Size = New System.Drawing.Size(148, 22)
        Me.lblLeverInput5.TabIndex = 18
        Me.lblLeverInput5.Text = "Lever 5"
        Me.txtLeverLetter5.Name = "txtLeverLetter5"
        Me.txtLeverLetter5.Location = New System.Drawing.Point(178, 180)
        Me.txtLeverLetter5.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverLetter5.TabIndex = 19
        Me.txtLeverLetter5.MaxLength = 1
        Me.txtLeverLetter5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverRetract5.Name = "txtLeverRetract5"
        Me.txtLeverRetract5.Location = New System.Drawing.Point(266, 180)
        Me.txtLeverRetract5.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverRetract5.TabIndex = 20
        Me.txtLeverRetract5.MaxLength = 1
        Me.txtLeverRetract5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblLeverInput6.Name = "lblLeverInput6"
        Me.lblLeverInput6.Location = New System.Drawing.Point(14, 215)
        Me.lblLeverInput6.Size = New System.Drawing.Size(148, 22)
        Me.lblLeverInput6.TabIndex = 21
        Me.lblLeverInput6.Text = "Lever 6"
        Me.txtLeverLetter6.Name = "txtLeverLetter6"
        Me.txtLeverLetter6.Location = New System.Drawing.Point(178, 212)
        Me.txtLeverLetter6.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverLetter6.TabIndex = 22
        Me.txtLeverLetter6.MaxLength = 1
        Me.txtLeverLetter6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLeverRetract6.Name = "txtLeverRetract6"
        Me.txtLeverRetract6.Location = New System.Drawing.Point(266, 212)
        Me.txtLeverRetract6.Size = New System.Drawing.Size(48, 20)
        Me.txtLeverRetract6.TabIndex = 23
        Me.txtLeverRetract6.MaxLength = 1
        Me.txtLeverRetract6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblMapScope.Name = "lblMapScope"
        Me.lblMapScope.Location = New System.Drawing.Point(12, 466)
        Me.lblMapScope.Size = New System.Drawing.Size(344, 104)
        Me.lblMapScope.TabIndex = 24
        Me.lblMapScope.Text = "Each box keeps the command map it had when you clicked Add box. Changes here apply to boxes added afterward. Older macros restore their lever letters and use the bundled defaults for the other commands."
        Me.grpOutputs.Name = "grpOutputs"
        Me.grpOutputs.Location = New System.Drawing.Point(368, 188)
        Me.grpOutputs.Size = New System.Drawing.Size(380, 384)
        Me.grpOutputs.TabIndex = 25
        Me.grpOutputs.Text = "Lights, sound and reinforcers"
        Me.lblOutputHeader.Name = "lblOutputHeader"
        Me.lblOutputHeader.Location = New System.Drawing.Point(14, 25)
        Me.lblOutputHeader.Size = New System.Drawing.Size(180, 20)
        Me.lblOutputHeader.TabIndex = 26
        Me.lblOutputHeader.Text = "Action"
        Me.lblOnHeader.Name = "lblOnHeader"
        Me.lblOnHeader.Location = New System.Drawing.Point(212, 25)
        Me.lblOnHeader.Size = New System.Drawing.Size(84, 20)
        Me.lblOnHeader.TabIndex = 27
        Me.lblOnHeader.Text = "On / action"
        Me.lblOffHeader.Name = "lblOffHeader"
        Me.lblOffHeader.Location = New System.Drawing.Point(312, 25)
        Me.lblOffHeader.Size = New System.Drawing.Size(54, 20)
        Me.lblOffHeader.TabIndex = 28
        Me.lblOffHeader.Text = "Off"
        Me.lblLight1.Name = "lblLight1"
        Me.lblLight1.Location = New System.Drawing.Point(14, 55)
        Me.lblLight1.Size = New System.Drawing.Size(190, 22)
        Me.lblLight1.TabIndex = 29
        Me.lblLight1.Text = "Light 1"
        Me.txtLight1On.Name = "txtLight1On"
        Me.txtLight1On.Location = New System.Drawing.Point(212, 52)
        Me.txtLight1On.Size = New System.Drawing.Size(48, 20)
        Me.txtLight1On.TabIndex = 30
        Me.txtLight1On.MaxLength = 1
        Me.txtLight1On.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLight1Off.Name = "txtLight1Off"
        Me.txtLight1Off.Location = New System.Drawing.Point(312, 52)
        Me.txtLight1Off.Size = New System.Drawing.Size(48, 20)
        Me.txtLight1Off.TabIndex = 31
        Me.txtLight1Off.MaxLength = 1
        Me.txtLight1Off.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblLight2.Name = "lblLight2"
        Me.lblLight2.Location = New System.Drawing.Point(14, 85)
        Me.lblLight2.Size = New System.Drawing.Size(190, 22)
        Me.lblLight2.TabIndex = 32
        Me.lblLight2.Text = "Light 2"
        Me.txtLight2On.Name = "txtLight2On"
        Me.txtLight2On.Location = New System.Drawing.Point(212, 82)
        Me.txtLight2On.Size = New System.Drawing.Size(48, 20)
        Me.txtLight2On.TabIndex = 33
        Me.txtLight2On.MaxLength = 1
        Me.txtLight2On.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLight2Off.Name = "txtLight2Off"
        Me.txtLight2Off.Location = New System.Drawing.Point(312, 82)
        Me.txtLight2Off.Size = New System.Drawing.Size(48, 20)
        Me.txtLight2Off.TabIndex = 34
        Me.txtLight2Off.MaxLength = 1
        Me.txtLight2Off.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblLight3.Name = "lblLight3"
        Me.lblLight3.Location = New System.Drawing.Point(14, 115)
        Me.lblLight3.Size = New System.Drawing.Size(190, 22)
        Me.lblLight3.TabIndex = 35
        Me.lblLight3.Text = "Light 3"
        Me.txtLight3On.Name = "txtLight3On"
        Me.txtLight3On.Location = New System.Drawing.Point(212, 112)
        Me.txtLight3On.Size = New System.Drawing.Size(48, 20)
        Me.txtLight3On.TabIndex = 36
        Me.txtLight3On.MaxLength = 1
        Me.txtLight3On.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLight3Off.Name = "txtLight3Off"
        Me.txtLight3Off.Location = New System.Drawing.Point(312, 112)
        Me.txtLight3Off.Size = New System.Drawing.Size(48, 20)
        Me.txtLight3Off.TabIndex = 37
        Me.txtLight3Off.MaxLength = 1
        Me.txtLight3Off.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblLight4.Name = "lblLight4"
        Me.lblLight4.Location = New System.Drawing.Point(14, 145)
        Me.lblLight4.Size = New System.Drawing.Size(190, 22)
        Me.lblLight4.TabIndex = 38
        Me.lblLight4.Text = "Light 4"
        Me.txtLight4On.Name = "txtLight4On"
        Me.txtLight4On.Location = New System.Drawing.Point(212, 142)
        Me.txtLight4On.Size = New System.Drawing.Size(48, 20)
        Me.txtLight4On.TabIndex = 39
        Me.txtLight4On.MaxLength = 1
        Me.txtLight4On.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtLight4Off.Name = "txtLight4Off"
        Me.txtLight4Off.Location = New System.Drawing.Point(312, 142)
        Me.txtLight4Off.Size = New System.Drawing.Size(48, 20)
        Me.txtLight4Off.TabIndex = 40
        Me.txtLight4Off.MaxLength = 1
        Me.txtLight4Off.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblHouseLight.Name = "lblHouseLight"
        Me.lblHouseLight.Location = New System.Drawing.Point(14, 175)
        Me.lblHouseLight.Size = New System.Drawing.Size(190, 22)
        Me.lblHouseLight.TabIndex = 41
        Me.lblHouseLight.Text = "House light"
        Me.txtHouseLightOn.Name = "txtHouseLightOn"
        Me.txtHouseLightOn.Location = New System.Drawing.Point(212, 172)
        Me.txtHouseLightOn.Size = New System.Drawing.Size(48, 20)
        Me.txtHouseLightOn.TabIndex = 42
        Me.txtHouseLightOn.MaxLength = 1
        Me.txtHouseLightOn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtHouseLightOff.Name = "txtHouseLightOff"
        Me.txtHouseLightOff.Location = New System.Drawing.Point(312, 172)
        Me.txtHouseLightOff.Size = New System.Drawing.Size(48, 20)
        Me.txtHouseLightOff.TabIndex = 43
        Me.txtHouseLightOff.MaxLength = 1
        Me.txtHouseLightOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblTone.Name = "lblTone"
        Me.lblTone.Location = New System.Drawing.Point(14, 205)
        Me.lblTone.Size = New System.Drawing.Size(190, 22)
        Me.lblTone.TabIndex = 44
        Me.lblTone.Text = "Continuous tone"
        Me.txtToneOn.Name = "txtToneOn"
        Me.txtToneOn.Location = New System.Drawing.Point(212, 202)
        Me.txtToneOn.Size = New System.Drawing.Size(48, 20)
        Me.txtToneOn.TabIndex = 45
        Me.txtToneOn.MaxLength = 1
        Me.txtToneOn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtToneOff.Name = "txtToneOff"
        Me.txtToneOff.Location = New System.Drawing.Point(312, 202)
        Me.txtToneOff.Size = New System.Drawing.Size(48, 20)
        Me.txtToneOff.TabIndex = 46
        Me.txtToneOff.MaxLength = 1
        Me.txtToneOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblPump.Name = "lblPump"
        Me.lblPump.Location = New System.Drawing.Point(14, 235)
        Me.lblPump.Size = New System.Drawing.Size(190, 22)
        Me.lblPump.TabIndex = 47
        Me.lblPump.Text = "Continuous pump"
        Me.txtPumpOn.Name = "txtPumpOn"
        Me.txtPumpOn.Location = New System.Drawing.Point(212, 232)
        Me.txtPumpOn.Size = New System.Drawing.Size(48, 20)
        Me.txtPumpOn.TabIndex = 48
        Me.txtPumpOn.MaxLength = 1
        Me.txtPumpOn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtPumpOff.Name = "txtPumpOff"
        Me.txtPumpOff.Location = New System.Drawing.Point(312, 232)
        Me.txtPumpOff.Size = New System.Drawing.Size(48, 20)
        Me.txtPumpOff.TabIndex = 49
        Me.txtPumpOff.MaxLength = 1
        Me.txtPumpOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblFeederPulse.Name = "lblFeederPulse"
        Me.lblFeederPulse.Location = New System.Drawing.Point(14, 265)
        Me.lblFeederPulse.Size = New System.Drawing.Size(190, 22)
        Me.lblFeederPulse.TabIndex = 50
        Me.lblFeederPulse.Text = "Feeder pulse"
        Me.txtFeederPulse.Name = "txtFeederPulse"
        Me.txtFeederPulse.Location = New System.Drawing.Point(212, 262)
        Me.txtFeederPulse.Size = New System.Drawing.Size(48, 20)
        Me.txtFeederPulse.TabIndex = 51
        Me.txtFeederPulse.MaxLength = 1
        Me.txtFeederPulse.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblPumpPulse.Name = "lblPumpPulse"
        Me.lblPumpPulse.Location = New System.Drawing.Point(14, 295)
        Me.lblPumpPulse.Size = New System.Drawing.Size(190, 22)
        Me.lblPumpPulse.TabIndex = 52
        Me.lblPumpPulse.Text = "Pump pulse"
        Me.txtPumpPulse.Name = "txtPumpPulse"
        Me.txtPumpPulse.Location = New System.Drawing.Point(212, 292)
        Me.txtPumpPulse.Size = New System.Drawing.Size(48, 20)
        Me.txtPumpPulse.TabIndex = 53
        Me.txtPumpPulse.MaxLength = 1
        Me.txtPumpPulse.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblTonePulse.Name = "lblTonePulse"
        Me.lblTonePulse.Location = New System.Drawing.Point(14, 325)
        Me.lblTonePulse.Size = New System.Drawing.Size(190, 22)
        Me.lblTonePulse.TabIndex = 54
        Me.lblTonePulse.Text = "Tone pulse"
        Me.txtTonePulse.Name = "txtTonePulse"
        Me.txtTonePulse.Location = New System.Drawing.Point(212, 322)
        Me.txtTonePulse.Size = New System.Drawing.Size(48, 20)
        Me.txtTonePulse.TabIndex = 55
        Me.txtTonePulse.MaxLength = 1
        Me.txtTonePulse.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.lblAllOff.Name = "lblAllOff"
        Me.lblAllOff.Location = New System.Drawing.Point(14, 355)
        Me.lblAllOff.Size = New System.Drawing.Size(190, 22)
        Me.lblAllOff.TabIndex = 56
        Me.lblAllOff.Text = "All outputs off"
        Me.txtAllOff.Name = "txtAllOff"
        Me.txtAllOff.Location = New System.Drawing.Point(212, 352)
        Me.txtAllOff.Size = New System.Drawing.Size(48, 20)
        Me.txtAllOff.TabIndex = 57
        Me.txtAllOff.MaxLength = 1
        Me.txtAllOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.btnDefaults.Name = "btnDefaults"
        Me.btnDefaults.Location = New System.Drawing.Point(472, 584)
        Me.btnDefaults.Size = New System.Drawing.Size(100, 28)
        Me.btnDefaults.TabIndex = 58
        Me.btnDefaults.Text = "Defaults"
        Me.btnDefaults.UseVisualStyleBackColor = True
        Me.btnApply.Name = "btnApply"
        Me.btnApply.Location = New System.Drawing.Point(578, 584)
        Me.btnApply.Size = New System.Drawing.Size(80, 28)
        Me.btnApply.TabIndex = 59
        Me.btnApply.Text = "Apply"
        Me.btnApply.UseVisualStyleBackColor = True
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Location = New System.Drawing.Point(664, 584)
        Me.btnCancel.Size = New System.Drawing.Size(84, 28)
        Me.btnCancel.TabIndex = 60
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
        Me.ClientSize = New System.Drawing.Size(760, 624)
        Me.Name = "LeverCommandSettings"
        Me.Text = "Arduino commands"
        Me.Controls.Add(Me.lblInstructions)
        Me.Controls.Add(Me.lblPersistenceWarning)
        Me.Controls.Add(Me.grpLevers)
        Me.grpLevers.Controls.Add(Me.lblLeverHeader)
        Me.grpLevers.Controls.Add(Me.lblInsertHeader)
        Me.grpLevers.Controls.Add(Me.lblRetractHeader)
        Me.grpLevers.Controls.Add(Me.lblLeverInput1)
        Me.grpLevers.Controls.Add(Me.txtLeverLetter1)
        Me.grpLevers.Controls.Add(Me.txtLeverRetract1)
        Me.grpLevers.Controls.Add(Me.lblLeverInput2)
        Me.grpLevers.Controls.Add(Me.txtLeverLetter2)
        Me.grpLevers.Controls.Add(Me.txtLeverRetract2)
        Me.grpLevers.Controls.Add(Me.lblLeverInput3)
        Me.grpLevers.Controls.Add(Me.txtLeverLetter3)
        Me.grpLevers.Controls.Add(Me.txtLeverRetract3)
        Me.grpLevers.Controls.Add(Me.lblLeverInput4)
        Me.grpLevers.Controls.Add(Me.txtLeverLetter4)
        Me.grpLevers.Controls.Add(Me.txtLeverRetract4)
        Me.grpLevers.Controls.Add(Me.lblLeverInput5)
        Me.grpLevers.Controls.Add(Me.txtLeverLetter5)
        Me.grpLevers.Controls.Add(Me.txtLeverRetract5)
        Me.grpLevers.Controls.Add(Me.lblLeverInput6)
        Me.grpLevers.Controls.Add(Me.txtLeverLetter6)
        Me.grpLevers.Controls.Add(Me.txtLeverRetract6)
        Me.Controls.Add(Me.lblMapScope)
        Me.Controls.Add(Me.grpOutputs)
        Me.grpOutputs.Controls.Add(Me.lblOutputHeader)
        Me.grpOutputs.Controls.Add(Me.lblOnHeader)
        Me.grpOutputs.Controls.Add(Me.lblOffHeader)
        Me.grpOutputs.Controls.Add(Me.lblLight1)
        Me.grpOutputs.Controls.Add(Me.txtLight1On)
        Me.grpOutputs.Controls.Add(Me.txtLight1Off)
        Me.grpOutputs.Controls.Add(Me.lblLight2)
        Me.grpOutputs.Controls.Add(Me.txtLight2On)
        Me.grpOutputs.Controls.Add(Me.txtLight2Off)
        Me.grpOutputs.Controls.Add(Me.lblLight3)
        Me.grpOutputs.Controls.Add(Me.txtLight3On)
        Me.grpOutputs.Controls.Add(Me.txtLight3Off)
        Me.grpOutputs.Controls.Add(Me.lblLight4)
        Me.grpOutputs.Controls.Add(Me.txtLight4On)
        Me.grpOutputs.Controls.Add(Me.txtLight4Off)
        Me.grpOutputs.Controls.Add(Me.lblHouseLight)
        Me.grpOutputs.Controls.Add(Me.txtHouseLightOn)
        Me.grpOutputs.Controls.Add(Me.txtHouseLightOff)
        Me.grpOutputs.Controls.Add(Me.lblTone)
        Me.grpOutputs.Controls.Add(Me.txtToneOn)
        Me.grpOutputs.Controls.Add(Me.txtToneOff)
        Me.grpOutputs.Controls.Add(Me.lblPump)
        Me.grpOutputs.Controls.Add(Me.txtPumpOn)
        Me.grpOutputs.Controls.Add(Me.txtPumpOff)
        Me.grpOutputs.Controls.Add(Me.lblFeederPulse)
        Me.grpOutputs.Controls.Add(Me.txtFeederPulse)
        Me.grpOutputs.Controls.Add(Me.lblPumpPulse)
        Me.grpOutputs.Controls.Add(Me.txtPumpPulse)
        Me.grpOutputs.Controls.Add(Me.lblTonePulse)
        Me.grpOutputs.Controls.Add(Me.txtTonePulse)
        Me.grpOutputs.Controls.Add(Me.lblAllOff)
        Me.grpOutputs.Controls.Add(Me.txtAllOff)
        Me.Controls.Add(Me.btnDefaults)
        Me.Controls.Add(Me.btnApply)
        Me.Controls.Add(Me.btnCancel)
        Me.grpLevers.ResumeLayout(False)
        Me.grpLevers.PerformLayout()
        Me.grpOutputs.ResumeLayout(False)
        Me.grpOutputs.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblInstructions As System.Windows.Forms.Label
    Friend WithEvents lblPersistenceWarning As System.Windows.Forms.Label
    Friend WithEvents grpLevers As System.Windows.Forms.GroupBox
    Friend WithEvents lblLeverHeader As System.Windows.Forms.Label
    Friend WithEvents lblInsertHeader As System.Windows.Forms.Label
    Friend WithEvents lblRetractHeader As System.Windows.Forms.Label
    Friend WithEvents lblLeverInput1 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter1 As System.Windows.Forms.TextBox
    Friend WithEvents txtLeverRetract1 As System.Windows.Forms.TextBox
    Friend WithEvents lblLeverInput2 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter2 As System.Windows.Forms.TextBox
    Friend WithEvents txtLeverRetract2 As System.Windows.Forms.TextBox
    Friend WithEvents lblLeverInput3 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter3 As System.Windows.Forms.TextBox
    Friend WithEvents txtLeverRetract3 As System.Windows.Forms.TextBox
    Friend WithEvents lblLeverInput4 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter4 As System.Windows.Forms.TextBox
    Friend WithEvents txtLeverRetract4 As System.Windows.Forms.TextBox
    Friend WithEvents lblLeverInput5 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter5 As System.Windows.Forms.TextBox
    Friend WithEvents txtLeverRetract5 As System.Windows.Forms.TextBox
    Friend WithEvents lblLeverInput6 As System.Windows.Forms.Label
    Friend WithEvents txtLeverLetter6 As System.Windows.Forms.TextBox
    Friend WithEvents txtLeverRetract6 As System.Windows.Forms.TextBox
    Friend WithEvents lblMapScope As System.Windows.Forms.Label
    Friend WithEvents grpOutputs As System.Windows.Forms.GroupBox
    Friend WithEvents lblOutputHeader As System.Windows.Forms.Label
    Friend WithEvents lblOnHeader As System.Windows.Forms.Label
    Friend WithEvents lblOffHeader As System.Windows.Forms.Label
    Friend WithEvents lblLight1 As System.Windows.Forms.Label
    Friend WithEvents txtLight1On As System.Windows.Forms.TextBox
    Friend WithEvents txtLight1Off As System.Windows.Forms.TextBox
    Friend WithEvents lblLight2 As System.Windows.Forms.Label
    Friend WithEvents txtLight2On As System.Windows.Forms.TextBox
    Friend WithEvents txtLight2Off As System.Windows.Forms.TextBox
    Friend WithEvents lblLight3 As System.Windows.Forms.Label
    Friend WithEvents txtLight3On As System.Windows.Forms.TextBox
    Friend WithEvents txtLight3Off As System.Windows.Forms.TextBox
    Friend WithEvents lblLight4 As System.Windows.Forms.Label
    Friend WithEvents txtLight4On As System.Windows.Forms.TextBox
    Friend WithEvents txtLight4Off As System.Windows.Forms.TextBox
    Friend WithEvents lblHouseLight As System.Windows.Forms.Label
    Friend WithEvents txtHouseLightOn As System.Windows.Forms.TextBox
    Friend WithEvents txtHouseLightOff As System.Windows.Forms.TextBox
    Friend WithEvents lblTone As System.Windows.Forms.Label
    Friend WithEvents txtToneOn As System.Windows.Forms.TextBox
    Friend WithEvents txtToneOff As System.Windows.Forms.TextBox
    Friend WithEvents lblPump As System.Windows.Forms.Label
    Friend WithEvents txtPumpOn As System.Windows.Forms.TextBox
    Friend WithEvents txtPumpOff As System.Windows.Forms.TextBox
    Friend WithEvents lblFeederPulse As System.Windows.Forms.Label
    Friend WithEvents txtFeederPulse As System.Windows.Forms.TextBox
    Friend WithEvents lblPumpPulse As System.Windows.Forms.Label
    Friend WithEvents txtPumpPulse As System.Windows.Forms.TextBox
    Friend WithEvents lblTonePulse As System.Windows.Forms.Label
    Friend WithEvents txtTonePulse As System.Windows.Forms.TextBox
    Friend WithEvents lblAllOff As System.Windows.Forms.Label
    Friend WithEvents txtAllOff As System.Windows.Forms.TextBox
    Friend WithEvents btnDefaults As System.Windows.Forms.Button
    Friend WithEvents btnApply As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
End Class
