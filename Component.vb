Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar

Public Class Component

    Private ComponentStimType As String = ""
    Private HouselightOnOff As Boolean = False
    Private ReadOnly AddedInputs As New List(Of ComponentInputConfiguration)
    Private ReadOnly ComponentToolTip As New ToolTip()

    Private Class ComponentInputConfiguration
        Public Property Name As String
        Public Property ScheduleType As String
        Public Property ScheduleValue As Integer
        Public Property Reinforcer As String
        Public Property Magnitude As Integer
        Public Property PelletP As Integer
        Public Property FeedbackDuration As Double
        Public Property FeedbackType As String
        Public Property DelayDuration As Double
        Public Property DelayType As String
        Public Property DelayRetract As Boolean
        Public Property DelaySignalDuration As Double
    End Class

    ' ---------- Helpers (sin casts implícitos) ----------
    Private Function Clean(s As String) As String
        Return If(s, "") _
            .Replace("""", "") _
            .Replace(vbCr, " ") _
            .Replace(vbLf, " ") _
            .Trim()
    End Function

    Private Function ToIntSafe(txt As String, Optional defaultValue As Integer = 0) As Integer
        Dim s As String = Clean(txt)
        Dim n As Integer
        If Integer.TryParse(s, n) Then Return n
        Return defaultValue
    End Function

    Private Function ToDoubleSafe(txt As String, Optional defaultValue As Double = 0) As Double
        Dim s As String = Clean(txt)
        Dim n As Double
        If Double.TryParse(s, n) Then Return n
        If Double.TryParse(s.Replace("."c, ","c), n) Then Return n
        If Double.TryParse(s.Replace(","c, "."c), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, n) Then Return n
        Return defaultValue
    End Function

    Private Function ToByteSafe(txt As String, Optional defaultValue As Byte = 0) As Byte
        Dim s As String = Clean(txt)
        Dim n As Integer
        If Integer.TryParse(s, n) Then
            If n < 0 Then n = 0
            If n > 255 Then n = 255
            Return CByte(n)
        End If
        Return defaultValue
    End Function

    Private Function ToStrSafe(txt As String, Optional defaultValue As String = "") As String
        Dim s As String = Clean(txt)
        If s = "" Then Return defaultValue
        Return s
    End Function

    Private Function GetSelectedScheduleType() As String
        If rdoFRL1.Checked Then Return "Fixed Ratio"
        If rdoVRL1.Checked Then Return "Variable Ratio"
        If rdoFIL1.Checked Then Return "Fixed Interval"
        If rdoVIL1.Checked Then Return "Variable Interval"
        Return "Extinction"
    End Function

    Private Function GetSelectedFeedbackType() As String
        Dim selected As New List(Of String)
        If rdoLight1L1.Checked Then selected.Add("Light 1")
        If rdoLight2L1.Checked Then selected.Add("Light 2")
        If rdoLight3L1.Checked Then selected.Add("Light 3")
        If rdoLight4L1.Checked Then selected.Add("Light 4")
        If rdoToneL1.Checked Then selected.Add("Tone")
        If rdoHouselightL1.Checked Then selected.Add("Houselight")
        If rdoTOL1.Checked Then selected.Add("Time Out")
        If selected.Count = 0 Then Return "None"
        Return String.Join(" + ", selected)
    End Function

    Private Function GetSelectedDelayType() As String
        If chkUnsignaledDelay.Checked Then Return "Unsignaled"

        Dim selected As New List(Of String)
        If rdoLightDelay1L1.Checked Then selected.Add("Light 1")
        If rdoLightDelay2L1.Checked Then selected.Add("Light 2")
        If rdoLightDelay3L1.Checked Then selected.Add("Light 3")
        If rdoLightDelay4L1.Checked Then selected.Add("Light 4")
        If rdoToneDelayL1.Checked Then selected.Add("Tone")
        If rdoHouselightDelayL1.Checked Then selected.Add("Houselight")
        If selected.Count = 0 Then Return "None"
        Return String.Join(" + ", selected)
    End Function

    Private Function GetSelectedComponentStimType() As String
        Dim selected As New List(Of String)
        If rdoComponentStimLight1.Checked Then selected.Add("Light 1")
        If rdoComponentStimLight2.Checked Then selected.Add("Light 2")
        If rdoComponentStimLight3.Checked Then selected.Add("Light 3")
        If rdoComponentStimLight4.Checked Then selected.Add("Light 4")
        If rdoComponentTone.Checked Then selected.Add("Tone")
        If selected.Count = 0 Then Return "None"
        Return String.Join(" + ", selected)
    End Function

    Private Function HasFeedbackSelection() As Boolean
        Return rdoLight1L1.Checked OrElse rdoLight2L1.Checked OrElse rdoLight3L1.Checked OrElse rdoLight4L1.Checked OrElse rdoToneL1.Checked OrElse rdoHouselightL1.Checked OrElse rdoTOL1.Checked
    End Function

    Private Function HasDelaySelection() As Boolean
        Return chkUnsignaledDelay.Checked OrElse rdoLightDelay1L1.Checked OrElse rdoLightDelay2L1.Checked OrElse rdoLightDelay3L1.Checked OrElse rdoLightDelay4L1.Checked OrElse rdoToneDelayL1.Checked OrElse rdoHouselightDelayL1.Checked
    End Function

    Private Function HasComponentLightSelection() As Boolean
        Return rdoComponentStimLight1.Checked OrElse rdoComponentStimLight2.Checked OrElse rdoComponentStimLight3.Checked OrElse rdoComponentStimLight4.Checked
    End Function

    Private Sub UpdateFeedbackState()
        Dim enabled As Boolean = HasFeedbackSelection()
        txbStimDurL1.Enabled = enabled
        txbStimDurL1.Visible = enabled
        Label12.Visible = enabled
    End Sub

    Private Sub UpdateDelayState()
        Dim enabled As Boolean = HasDelaySelection()
        Dim signaled As Boolean = enabled AndAlso chkUnsignaledDelay.Checked = False

        rdoLightDelay1L1.Enabled = chkUnsignaledDelay.Checked = False
        rdoLightDelay2L1.Enabled = chkUnsignaledDelay.Checked = False
        rdoLightDelay3L1.Enabled = chkUnsignaledDelay.Checked = False
        rdoLightDelay4L1.Enabled = chkUnsignaledDelay.Checked = False
        rdoToneDelayL1.Enabled = chkUnsignaledDelay.Checked = False
        rdoHouselightDelayL1.Enabled = chkUnsignaledDelay.Checked = False

        If chkUnsignaledDelay.Checked Then
            rdoLightDelay1L1.Checked = False
            rdoLightDelay2L1.Checked = False
            rdoLightDelay3L1.Checked = False
            rdoLightDelay4L1.Checked = False
            rdoToneDelayL1.Checked = False
            rdoHouselightDelayL1.Checked = False
        End If

        txbDelayDurL1.Enabled = enabled
        txbDelaySignalDurationL1.Enabled = signaled
        txbDelayDurL1.Visible = enabled
        txbDelaySignalDurationL1.Visible = signaled
        Label9.Visible = enabled
        Label16.Visible = signaled
        chkRetractL1.Visible = enabled
        If enabled = False Then
            chkRetractL1.Checked = False
        End If
        If signaled = False Then txbDelaySignalDurationL1.Text = "0"
    End Sub

    Private Sub UpdateComponentIntermittencyState()
        txbLightIntermittency.Enabled = HasComponentLightSelection()
        txbToneIntermittency.Enabled = rdoComponentTone.Checked

        If HasComponentLightSelection() = False Then txbLightIntermittency.Text = "0"
        If rdoComponentTone.Checked = False Then txbToneIntermittency.Text = "0"
    End Sub

    Private Function BuildInputFromEditor(inputNumber As Integer) As ComponentInputConfiguration
        Dim inputName As String = ToStrSafe(txbInputName.Text, "Input " & inputNumber)
        Dim scheduleType As String = GetSelectedScheduleType()

        Return New ComponentInputConfiguration With {
            .Name = inputName,
            .ScheduleType = scheduleType,
            .ScheduleValue = If(scheduleType = "Extinction", 0, ToIntSafe(txbValueL1.Text, 0)),
            .Reinforcer = If(scheduleType = "Extinction", "N/A", ToStrSafe(cbbReinforcer1.Text, "Pellet")),
            .Magnitude = If(scheduleType = "Extinction", 0, ToIntSafe(txbMagL1.Text, 0)),
            .PelletP = If(scheduleType = "Extinction", 0, ToIntSafe(txbPelletProbability1.Text, 0)),
            .FeedbackDuration = If(HasFeedbackSelection(), ToDoubleSafe(txbStimDurL1.Text, 1), 0),
            .FeedbackType = GetSelectedFeedbackType(),
            .DelayDuration = If(HasDelaySelection(), ToDoubleSafe(txbDelayDurL1.Text, 1), 0),
            .DelayType = GetSelectedDelayType(),
            .DelayRetract = chkRetractL1.Checked,
            .DelaySignalDuration = If(chkUnsignaledDelay.Checked, 0, ToDoubleSafe(txbDelaySignalDurationL1.Text, 0))
        }
    End Function

    Private Function BuildDefaultInput(inputNumber As Integer) As ComponentInputConfiguration
        Return New ComponentInputConfiguration With {
            .Name = "Input " & inputNumber,
            .ScheduleType = "Extinction",
            .ScheduleValue = 0,
            .Reinforcer = "N/A",
            .Magnitude = 1,
            .PelletP = 0,
            .FeedbackDuration = 0,
            .FeedbackType = "None",
            .DelayDuration = 1,
            .DelayType = "None",
            .DelayRetract = False,
            .DelaySignalDuration = 1
        }
    End Function

    Private Function InputSummary(inputInfo As ComponentInputConfiguration) As String
        If inputInfo.ScheduleType = "Extinction" Then
            Return inputInfo.Name & " - Extinction / N/A"
        End If

        Return inputInfo.Name & " - " & inputInfo.ScheduleType & " " & inputInfo.ScheduleValue &
            " / " & inputInfo.Magnitude & " " & inputInfo.Reinforcer
    End Function

    Private Sub UpdateExtinctionState()
        Dim isExtinction As Boolean = rdoExt1.Checked

        txbValueL1.Enabled = Not isExtinction
        grpMagnitude.Enabled = Not isExtinction

        If isExtinction Then
            txbValueL1.Text = ""
            cbbReinforcer1.Text = ""
            txbMagL1.Text = ""
            txbPelletProbability1.Text = ""
            txbPelletProbability1.Enabled = False
        Else
            If txbValueL1.Text = "" Then txbValueL1.Text = "0"
            If cbbReinforcer1.Text = "" Then cbbReinforcer1.Text = "Pellet"
            If txbMagL1.Text = "" Then txbMagL1.Text = "1"
            If txbPelletProbability1.Text = "" Then txbPelletProbability1.Text = "0"
            txbPelletProbability1.Enabled = (cbbReinforcer1.Text = "Random")
        End If
    End Sub

    Private Sub ResetInputEditor()
        txbInputName.Text = "Arduino Pin " & (AddedInputs.Count + 2)
        rdoExt1.Checked = True
        UpdateExtinctionState()
        txbStimDurL1.Text = "1"
        rdoLight1L1.Checked = False
        rdoLight2L1.Checked = False
        rdoLight3L1.Checked = False
        rdoLight4L1.Checked = False
        rdoToneL1.Checked = False
        rdoHouselightL1.Checked = False
        rdoTOL1.Checked = False
        txbDelayDurL1.Text = "1"
        chkRetractL1.Checked = False
        chkUnsignaledDelay.Checked = False
        rdoLightDelay1L1.Checked = False
        rdoLightDelay2L1.Checked = False
        rdoLightDelay3L1.Checked = False
        rdoLightDelay4L1.Checked = False
        rdoToneDelayL1.Checked = False
        rdoHouselightDelayL1.Checked = False
        txbDelaySignalDurationL1.Text = "0"
        UpdateFeedbackState()
        UpdateDelayState()
    End Sub

    Private Sub btnAddInput_Click(sender As Object, e As EventArgs) Handles btnAddInput.Click
        If AddedInputs.Count >= MAX_INPUTS Then
            MsgBox("This version supports up to " & MAX_INPUTS & " inputs.")
            Exit Sub
        End If

        Dim inputInfo As ComponentInputConfiguration = BuildInputFromEditor(AddedInputs.Count + 1)
        If inputInfo.ScheduleType <> "Extinction" Then
            If inputInfo.ScheduleValue <= 0 Then
                MsgBox("Please enter a schedule value greater than 0 for this input.")
                Exit Sub
            End If

            If inputInfo.Reinforcer = "" OrElse inputInfo.Reinforcer = "N/A" Then
                MsgBox("Please select a reinforcer for this input.")
                Exit Sub
            End If

            If inputInfo.Magnitude <= 0 Then
                MsgBox("Please enter a reinforcer magnitude greater than 0 for this input.")
                Exit Sub
            End If
        End If

        AddedInputs.Add(inputInfo)
        lstInputs.Items.Add(InputSummary(inputInfo))

        If AddedInputs.Count >= MAX_INPUTS Then
            btnAddInput.Enabled = False
            txbInputName.Enabled = False
            GroupBox1.Enabled = False
        Else
            ResetInputEditor()
        End If
    End Sub

    Private Sub btnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click

        ' === Validaciones fuertes (parse real) ===
        Dim componentDurationSec As Double = ToDoubleSafe(txbComponentDuration.Text, -1)
        If componentDurationSec <= 0 Then
            MsgBox("Please input Component duration (must be > 0).")
            Exit Sub
        End If

        Dim componentIterations As Byte = ToByteSafe(txbComponentIterations.Text, 0)
        If componentIterations = 0 Then
            MsgBox("Please input Component iterations (must be > 0).")
            Exit Sub
        End If

        ComponentStimType = GetSelectedComponentStimType()
        Dim componentLightIntermittencySec As Double = 0
        Dim componentToneIntermittencySec As Double = 0
        If HasComponentLightSelection() Then componentLightIntermittencySec = ToDoubleSafe(txbLightIntermittency.Text, 0)
        If rdoComponentTone.Checked Then componentToneIntermittencySec = ToDoubleSafe(txbToneIntermittency.Text, 0)

        If AddedInputs.Count = 0 Then
            MsgBox("Please add at least one input before submitting the component.")
            Exit Sub
        End If

        ' ===== Asignación a tu blueprint (tipos correctos) =====
        AC(vCC).ComponentName = ToStrSafe(txbComponentName.Text, "Component " & vCC)
        AC(vCC).ComponentDuration = componentDurationSec
        AC(vCC).ComponentIteration = componentIterations
        AC(vCC).ComponentLightIntermittency = componentLightIntermittencySec
        AC(vCC).ComponentStimDuration = componentToneIntermittencySec
        AC(vCC).ComponentStimType = ComponentStimType

        AC(vCC).InputCount = Math.Min(AddedInputs.Count, MAX_INPUTS)

        ReDim AC(vCC).InputName(MAX_INPUTS - 1)
        ReDim AC(vCC).ScheduleType(MAX_INPUTS - 1)
        ReDim AC(vCC).ScheduleValue(MAX_INPUTS - 1)
        ReDim AC(vCC).Magnitude(MAX_INPUTS - 1)
        ReDim AC(vCC).Reinforcer(MAX_INPUTS - 1)
        ReDim AC(vCC).PelletP(MAX_INPUTS - 1)
        ReDim AC(vCC).FeedbackDuration(MAX_INPUTS - 1)
        ReDim AC(vCC).FeedbackType(MAX_INPUTS - 1)
        ReDim AC(vCC).DelayDuration(MAX_INPUTS - 1)
        ReDim AC(vCC).DelayType(MAX_INPUTS - 1)
        ReDim AC(vCC).DelayRetract(MAX_INPUTS - 1)
        ReDim AC(vCC).DelaySignalDuration(MAX_INPUTS - 1)

        ' OJO: tu array measured es Integer(), dimensionamos con Iteration (Byte) => se convierte a Integer sin problema
        ReDim AC(vCC).ComponentDuration_measured(CInt(componentIterations))

        Dim storedInputs(MAX_INPUTS - 1) As ComponentInputConfiguration
        For i As Integer = 0 To MAX_INPUTS - 1
            storedInputs(i) = BuildDefaultInput(i + 1)
        Next

        For i As Integer = 0 To AC(vCC).InputCount - 1
            storedInputs(i) = AddedInputs(i)
        Next

        For i As Integer = 0 To MAX_INPUTS - 1
            AC(vCC).InputName(i) = storedInputs(i).Name
            AC(vCC).ScheduleType(i) = storedInputs(i).ScheduleType
            AC(vCC).ScheduleValue(i) = storedInputs(i).ScheduleValue
            AC(vCC).Reinforcer(i) = storedInputs(i).Reinforcer
            AC(vCC).Magnitude(i) = storedInputs(i).Magnitude
            AC(vCC).PelletP(i) = storedInputs(i).PelletP
            AC(vCC).FeedbackDuration(i) = storedInputs(i).FeedbackDuration
            AC(vCC).FeedbackType(i) = storedInputs(i).FeedbackType
            AC(vCC).DelayDuration(i) = storedInputs(i).DelayDuration
            AC(vCC).DelayType(i) = storedInputs(i).DelayType
            AC(vCC).DelayRetract(i) = storedInputs(i).DelayRetract
            AC(vCC).DelaySignalDuration(i) = storedInputs(i).DelaySignalDuration
        Next

        ' Global
        AC(vCC).HouselightOnOff = HouselightOnOff

        Dim codSec As Double = ToDoubleSafe(txbCOD.Text, 0)
        AC(vCC).COD = CDbl(codSec) * 1000.0 ' tu COD es Double (ms)

        AC(vCC).MaxRefs = ToIntSafe(txbMaxRefs.Text, 0)

        SetUp.RefreshComponentSummaryTable()

        If vCC >= 2 Then SetUp.CheckBox1.Enabled = True

        Me.Close()
    End Sub

    Private Sub Component_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoScroll = False
        Me.StartPosition = FormStartPosition.CenterParent
        GroupBox1.Text = "Input"
        txbComponentName.Text = "Component " & vCC
        ComponentToolTip.InitialDelay = 300
        ComponentToolTip.ReshowDelay = 100
        ComponentToolTip.AutoPopDelay = 7000
        ComponentToolTip.SetToolTip(txbComponentDuration, "Component duration in seconds. Decimal values such as 1.5 are allowed.")
        ComponentToolTip.SetToolTip(txbMaxRefs, "Maximum reinforcers for this component iteration. Reaching this number ends the current iteration; later iterations can deliver up to this maximum again.")
        ComponentToolTip.SetToolTip(txbCOD, "Changeover delay in seconds. Decimal values such as 1.5 are allowed.")
        ComponentToolTip.SetToolTip(cbbReinforcer1, "Select Random to use pellet probability P for this input's reinforcer.")
        ComponentToolTip.SetToolTip(txbPelletProbability1, "Pellet probability P when Random is selected.")
        ComponentToolTip.SetToolTip(txbStimDurL1, "Response feedback duration in seconds. Decimal values such as 1.5 are allowed.")
        ComponentToolTip.SetToolTip(rdoTOL1, "Time Out stops component stimulation, turns off chamber stimuli, and retracts all active inputs for the feedback duration.")
        ComponentToolTip.SetToolTip(txbDelayDurL1, "Delay duration in seconds. Decimal values such as 1.5 are allowed.")
        ComponentToolTip.SetToolTip(txbDelaySignalDurationL1, "Delay signal duration in seconds. It may be shorter or longer than the programmed delay; equal/greater values keep the signal on for the whole delay.")
        ComponentToolTip.SetToolTip(txbLightIntermittency, "0 keeps selected lights continuously on. Values greater than 0 set the on/off intermittency in seconds.")
        ComponentToolTip.SetToolTip(txbToneIntermittency, "0 keeps the tone continuously on. Values greater than 0 set the on/off intermittency in seconds.")
        ResetInputEditor()
        UpdateComponentIntermittencyState()
    End Sub

    Private Sub chkHouselightOnOff_CheckedChanged(sender As Object, e As EventArgs) Handles chkHouselightOnOff.CheckedChanged
        HouselightOnOff = chkHouselightOnOff.Checked
    End Sub

    Private Sub Schedule_CheckedChanged(sender As Object, e As EventArgs) Handles rdoExt1.CheckedChanged, rdoFRL1.CheckedChanged, rdoVRL1.CheckedChanged, rdoFIL1.CheckedChanged, rdoVIL1.CheckedChanged
        UpdateExtinctionState()
    End Sub

    ' Cancel: limpia slot y baja contador (evita componentes a medias)
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        If vCC >= 1 Then
            AC(vCC) = New ComponentBlueprint
            vCC -= 1
        End If
        Me.Close()
    End Sub

    Private Sub cbbReinforcer1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbbReinforcer1.SelectedIndexChanged
        If rdoExt1.Checked Then
            txbPelletProbability1.Text = ""
            txbPelletProbability1.Enabled = False
            Exit Sub
        End If

        If cbbReinforcer1.Text = "Random" Then
            txbPelletProbability1.Text = "50"
            txbPelletProbability1.Enabled = True
        Else
            txbPelletProbability1.Text = "0"
            txbPelletProbability1.Enabled = False
        End If
    End Sub

    Private Sub Feedback_CheckedChanged(sender As Object, e As EventArgs) Handles rdoLight1L1.CheckedChanged, rdoLight2L1.CheckedChanged, rdoLight3L1.CheckedChanged, rdoLight4L1.CheckedChanged, rdoToneL1.CheckedChanged, rdoHouselightL1.CheckedChanged, rdoTOL1.CheckedChanged
        UpdateFeedbackState()
    End Sub

    Private Sub Delay_CheckedChanged(sender As Object, e As EventArgs) Handles chkUnsignaledDelay.CheckedChanged, rdoLightDelay1L1.CheckedChanged, rdoLightDelay2L1.CheckedChanged, rdoLightDelay3L1.CheckedChanged, rdoLightDelay4L1.CheckedChanged, rdoToneDelayL1.CheckedChanged, rdoHouselightDelayL1.CheckedChanged
        UpdateDelayState()
    End Sub

    Private Sub ComponentStim_CheckedChanged(sender As Object, e As EventArgs) Handles rdoComponentTone.CheckedChanged, rdoComponentStimLight1.CheckedChanged, rdoComponentStimLight2.CheckedChanged, rdoComponentStimLight3.CheckedChanged, rdoComponentStimLight4.CheckedChanged
        UpdateComponentIntermittencyState()
    End Sub


End Class
