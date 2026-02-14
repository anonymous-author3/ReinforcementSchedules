Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar

Public Class Component

    Private ComponentStimType As String = ""
    Private ScheduleType1 As String = ""
    Private ScheduleType2 As String = ""
    Private FeedbackType1 As String = ""
    Private FeedbackType2 As String = ""
    Private DelayStimType1 As String = ""
    Private DelayStimType2 As String = ""
    Private HouselightOnOff As Boolean = False
    Private DelayRetract1 As Boolean = False
    Private DelayRetract2 As Boolean = False

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

    Private Sub btnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click

        ' === Validaciones fuertes (parse real) ===
        Dim componentDurationSec As Integer = ToIntSafe(txbComponentDuration.Text, -1)
        If componentDurationSec <= 0 Then
            MsgBox("Please input Component duration (must be > 0).")
            Exit Sub
        End If

        Dim componentIterations As Byte = ToByteSafe(txbComponentIterations.Text, 0)
        If componentIterations = 0 Then
            MsgBox("Please input Component iterations (must be > 0).")
            Exit Sub
        End If

        ' Estimulación del componente: puede ser 0 (always on)
        Dim componentStimSec As Double
        Dim stimTxt As String = Clean(txbComponentStimulation.Text)
        If stimTxt = "" Then
            MsgBox("Please input Component stimulation. Select 0 for always on.")
            Exit Sub
        End If
        componentStimSec = ToDoubleSafe(stimTxt, 0)

        ' === Tipo de estímulo del componente ===
        ComponentStimType = ""
        If rdoComponentStimLight1.Checked Then ComponentStimType = "Light 1"
        If rdoComponentStimLight2.Checked Then ComponentStimType = "Light 2"
        If rdoComponentTone.Checked Then ComponentStimType = "Tone"
        If rdoComponentHouselight.Checked Then ComponentStimType = "Houselight"
        If ComponentStimType = "" Then ComponentStimType = "None"

        ' === Schedules ===
        ScheduleType1 = ""
        If rdoExt1.Checked Then ScheduleType1 = "Extinction"
        If rdoFRL1.Checked Then ScheduleType1 = "Fixed Ratio"
        If rdoVRL1.Checked Then ScheduleType1 = "Variable Ratio"
        If rdoFIL1.Checked Then ScheduleType1 = "Fixed Interval"
        If rdoVIL1.Checked Then ScheduleType1 = "Variable Interval"
        If ScheduleType1 = "" Then ScheduleType1 = "Extinction"

        ScheduleType2 = ""
        If rdoExt2.Checked Then ScheduleType2 = "Extinction"
        If rdoFRL2.Checked Then ScheduleType2 = "Fixed Ratio"
        If rdoVRL2.Checked Then ScheduleType2 = "Variable Ratio"
        If rdoFIL2.Checked Then ScheduleType2 = "Fixed Interval"
        If rdoVIL2.Checked Then ScheduleType2 = "Variable Interval"
        If ScheduleType2 = "" Then ScheduleType2 = "Extinction"

        ' === Feedback ===
        FeedbackType1 = ""
        If rdoLight1L1.Checked Then FeedbackType1 = "Light 1"
        If rdoLight2L1.Checked Then FeedbackType1 = "Light 2"
        If rdoToneL1.Checked Then FeedbackType1 = "Tone"
        If rdoHouselightL1.Checked Then FeedbackType1 = "Houselight"
        If rdoTOL1.Checked Then FeedbackType1 = "Time Out"
        If FeedbackType1 = "" Then FeedbackType1 = "None"

        FeedbackType2 = ""
        If rdoLight1L2.Checked Then FeedbackType2 = "Light 1"
        If rdoLight2L2.Checked Then FeedbackType2 = "Light 2"
        If rdoToneL2.Checked Then FeedbackType2 = "Tone"
        If rdoHouselightL2.Checked Then FeedbackType2 = "Houselight"
        If rdoTOL2.Checked Then FeedbackType2 = "Time Out"
        If FeedbackType2 = "" Then FeedbackType2 = "None"

        ' === Delay ===
        DelayRetract1 = chkRetractL1.Checked
        DelayStimType1 = ""
        If rdoLightDelay1L1.Checked Then DelayStimType1 = "Light 1"
        If rdoLightDelay2L1.Checked Then DelayStimType1 = "Light 2"
        If rdoToneDelayL1.Checked Then DelayStimType1 = "Tone"
        If rdoHouselightDelayL1.Checked Then DelayStimType1 = "Houselight"
        If DelayStimType1 = "" Then DelayStimType1 = "None"

        DelayRetract2 = chkRetractL2.Checked
        DelayStimType2 = ""
        If rdoLightDelay1L2.Checked Then DelayStimType2 = "Light 1"
        If rdoLightDelay2L2.Checked Then DelayStimType2 = "Light 2"
        If rdoToneDelayL2.Checked Then DelayStimType2 = "Tone"
        If rdoHouselightDelayL2.Checked Then DelayStimType2 = "Houselight"
        If DelayStimType2 = "" Then DelayStimType2 = "None"

        ' ===== Asignación a tu blueprint (tipos correctos) =====
        AC(vCC).ComponentDuration = componentDurationSec
        AC(vCC).ComponentIteration = componentIterations
        AC(vCC).ComponentStimDuration = componentStimSec
        AC(vCC).ComponentStimType = ComponentStimType

        ' Arrays (2 levers)
        ReDim AC(vCC).ScheduleType(1)
        ReDim AC(vCC).ScheduleValue(1)
        ReDim AC(vCC).Magnitude(1)
        ReDim AC(vCC).Reinforcer(1)
        ReDim AC(vCC).PelletP(1)
        ReDim AC(vCC).FeedbackDuration(1)
        ReDim AC(vCC).FeedbackType(1)
        ReDim AC(vCC).DelayDuration(1)
        ReDim AC(vCC).DelayType(1)
        ReDim AC(vCC).DelayRetract(1)
        ReDim AC(vCC).DelaySignalDuration(1)

        ' OJO: tu array measured es Integer(), dimensionamos con Iteration (Byte) => se convierte a Integer sin problema
        ReDim AC(vCC).ComponentDuration_measured(CInt(componentIterations))

        ' Lever 1
        AC(vCC).ScheduleType(0) = ScheduleType1
        AC(vCC).ScheduleValue(0) = ToIntSafe(txbValueL1.Text, 0)

        AC(vCC).Reinforcer(0) = ToStrSafe(cbbReinforcer1.Text, "Pellet")
        AC(vCC).Magnitude(0) = ToIntSafe(txbMagL1.Text, 0)
        AC(vCC).PelletP(0) = ToIntSafe(txbPelletProbability1.Text, 0)

        AC(vCC).FeedbackDuration(0) = ToIntSafe(txbStimDurL1.Text, 0)
        AC(vCC).FeedbackType(0) = FeedbackType1

        AC(vCC).DelayDuration(0) = ToIntSafe(txbDelayDurL1.Text, 0)
        AC(vCC).DelayType(0) = DelayStimType1
        AC(vCC).DelayRetract(0) = DelayRetract1
        AC(vCC).DelaySignalDuration(0) = ToIntSafe(txbDelaySignalDurationL1.Text, 0)

        ' Lever 2
        AC(vCC).ScheduleType(1) = ScheduleType2
        AC(vCC).ScheduleValue(1) = ToIntSafe(txbValueL2.Text, 0)

        AC(vCC).Reinforcer(1) = ToStrSafe(cbbReinforcer2.Text, "Pellet")
        AC(vCC).Magnitude(1) = ToIntSafe(txbMagL2.Text, 0)
        AC(vCC).PelletP(1) = ToIntSafe(txbPelletProbability2.Text, 0)

        AC(vCC).FeedbackDuration(1) = ToIntSafe(txbStimDurL2.Text, 0)
        AC(vCC).FeedbackType(1) = FeedbackType2

        AC(vCC).DelayDuration(1) = ToIntSafe(txbDelayDurL2.Text, 0)
        AC(vCC).DelayType(1) = DelayStimType2
        AC(vCC).DelayRetract(1) = DelayRetract2
        AC(vCC).DelaySignalDuration(1) = ToIntSafe(txbDelaySignalDurationL2.Text, 0)

        ' Global
        AC(vCC).HouselightOnOff = HouselightOnOff

        Dim codSec As Integer = ToIntSafe(txbCOD.Text, 0)
        AC(vCC).COD = CDbl(codSec) * 1000.0 ' tu COD es Double (ms)

        AC(vCC).MaxRefs = ToIntSafe(txbMaxRefs.Text, 0)

        ' Preview bookkeeping
        PreviewStartByComponent(vCC) = PreviewCounter

        ' Preview printing (igual que tu versión)
        PrintInfo(SetUp.lblComponent.Location.X, SetUp.lblComponent.Location.Y, "Component " & vCC)
        PrintInfo(SetUp.lblComponentD.Location.X, SetUp.lblComponentD.Location.Y,
                  AC(vCC).ComponentDuration & " seconds / " & AC(vCC).MaxRefs & " refs")
        PrintInfo(SetUp.lblComponentI.Location.X, SetUp.lblComponentI.Location.Y, AC(vCC).ComponentIteration & " times")
        PrintInfo(SetUp.lblComponentS.Location.X, SetUp.lblComponentS.Location.Y,
                  AC(vCC).ComponentStimType & ": " & AC(vCC).ComponentStimDuration & " seconds")
        PrintInfo(SetUp.lblCOD.Location.X, SetUp.lblCOD.Location.Y, (AC(vCC).COD / 1000) & " seconds")

        PrintInfo(SetUp.lblSchedule1.Location.X, SetUp.lblSchedule1.Location.Y, AC(vCC).ScheduleType(0) & " " & AC(vCC).ScheduleValue(0))
        PrintInfo(SetUp.lblMagnitude1.Location.X, SetUp.lblMagnitude1.Location.Y,
                  AC(vCC).Magnitude(0) & " " & AC(vCC).Reinforcer(0) & " " & AC(vCC).PelletP(0))
        PrintInfo(SetUp.lblFeedback1.Location.X, SetUp.lblFeedback1.Location.Y, AC(vCC).FeedbackType(0) & ": " & AC(vCC).FeedbackDuration(0) & " seconds")
        PrintInfo(SetUp.lblDelay1.Location.X, SetUp.lblDelay1.Location.Y,
                  AC(vCC).DelayType(0) & ": " & AC(vCC).DelayDuration(0) & " seconds - Ret: " & AC(vCC).DelayRetract(0) &
                  vbCrLf & "/ signal: " & AC(vCC).DelaySignalDuration(0) & " seconds")

        PrintInfo(SetUp.lblSchedule2.Location.X, SetUp.lblSchedule2.Location.Y, AC(vCC).ScheduleType(1) & " " & AC(vCC).ScheduleValue(1))
        PrintInfo(SetUp.lblMagnitude2.Location.X, SetUp.lblMagnitude2.Location.Y,
                  AC(vCC).Magnitude(1) & " " & AC(vCC).Reinforcer(1) & " " & AC(vCC).PelletP(1))
        PrintInfo(SetUp.lblFeedback2.Location.X, SetUp.lblFeedback2.Location.Y, AC(vCC).FeedbackType(1) & ": " & AC(vCC).FeedbackDuration(1) & " seconds")
        PrintInfo(SetUp.lblDelay2.Location.X, SetUp.lblDelay2.Location.Y,
                  AC(vCC).DelayType(1) & ": " & AC(vCC).DelayDuration(1) & " seconds - Ret: " & AC(vCC).DelayRetract(1) &
                  vbCrLf & " signal: " & AC(vCC).DelaySignalDuration(1) & " seconds")

        For Each ctrl As Control In SetUp.Controls
            Dim lb As Label = TryCast(ctrl, Label)
            If lb IsNot Nothing AndAlso lb.Text IsNot Nothing AndAlso lb.Text.Contains("Component ") Then
                lb.Font = New Font("Microsoft Sans Serif", 11.0!, FontStyle.Bold)
            End If
        Next

        SetUp.Controls.Add(SetUp.LabelPreview(PreviewCounter))
        PreviewCounter += 1
        vPadding += 180

        If vCC >= 2 Then SetUp.CheckBox1.Enabled = True

        Me.Close()
    End Sub

    Private Sub Component_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.ClientSize = New Size(370, 570)
    End Sub

    Private Sub chkHouselightOnOff_CheckedChanged(sender As Object, e As EventArgs) Handles chkHouselightOnOff.CheckedChanged
        HouselightOnOff = chkHouselightOnOff.Checked
    End Sub

    ' Cancel: limpia slot y baja contador (evita componentes a medias)
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        If vCC >= 1 Then
            AC(vCC) = New ComponentBlueprint
            PreviewStartByComponent(vCC) = 0
            vCC -= 1
        End If
        Me.Close()
    End Sub

    Private Sub cbbReinforcer1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbbReinforcer1.SelectedIndexChanged
        If cbbReinforcer1.Text = "Random" Then
            txbPelletProbability1.Text = "50"
            txbPelletProbability1.Enabled = True
        Else
            txbPelletProbability1.Text = "0"
            txbPelletProbability1.Enabled = False
        End If
    End Sub

    Private Sub cbbReinforcer2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbbReinforcer2.SelectedIndexChanged
        If cbbReinforcer2.Text = "Random" Then
            txbPelletProbability2.Text = "50"
            txbPelletProbability2.Enabled = True
        Else
            txbPelletProbability2.Text = "0"
            txbPelletProbability2.Enabled = False
        End If
    End Sub

End Class
