Imports System.Math
Imports System.IO.Ports
Imports System.Windows.Forms.DataVisualization.Charting
Public Class Main
    Public Arduino As SerialPort
    Private WithEvents tmrChrt As Timer = New Timer
    Private CompSequence As List(Of Integer)
    Private CompIndexSeq As Integer
    Private NosepokeIn(0) As Boolean 'False = currently OUT, True = currently IN

    Private ObtainedDelayDurations(,) As List(Of Integer)   ' (component 1..MAXvCC, lever 0..1) values in ms
    Private DelayOnset(1) As Integer                        ' onset time (ms) for current active delay per lever; -1 = none
    Private DelayComp(1) As Integer  ' componente en el que comenzó la demora, por palanca



    Function ArduinoVB() As Integer 'This function starts the Arduino-VB communication.
        Arduino = New SerialPort(SetUp.txtCOM.Text, 9600) 'Assigns the Arduino to the selected port at a 9600 baud rate. 
        Arduino.Open() 'Starts the Arduino-VB communication.
        Arduino.WriteLine("p")
        tmrStart.Interval = Max(1, SetUp.txbStart.Text * 1000)
        If SetUp.txbICI.Text <> 0 Then
            tmrICI.Interval = SetUp.txbICI.Text * 1000
        Else
            tmrICI.Interval = 1
        End If
        Countdown = Environment.TickCount + SetUp.txbStart.Text * 1000
        tmrStart.Enabled = True

        Do 'This code will run throughout the session to allow response collection. 
            Try
                If Arduino.BytesToRead > 0 Then 'Checks for activity on the Arduino.
                    Actual_Response = Split(Arduino.ReadLine(), ",") 'Splits data from the arduino into separate responses.
                End If
                'The next following lines compare the state of the data stream of operanda 1 and 2 with previous observations to detect responses.
                If (Actual_Response(0) <> Previous_Response(0) And Actual_Response(0) <> 1) Then
                    Response(0) 'If a response is registered at operanda 1, this code will run.
                End If
                If (Actual_Response(1) <> Previous_Response(1) And Actual_Response(1) <> 1) Then
                    Response(1) 'The same happens for operanda 2.
                End If
                If (Actual_Response(2) <> Previous_Response(2) And Actual_Response(2) <> 1) Then
                    Nosepoke(0) 'The same happens for operanda 3.
                End If
                If (Actual_Response(3) <> Previous_Response(3) And Actual_Response(3) <> 1) Then
                    'Response(3) 'The same happens for operanda 4.
                End If
                If (Actual_Response(4) <> Previous_Response(4) And Actual_Response(4) <> 1) Then
                    'Response(4) 'The same happens for operanda 5.
                End If
                Previous_Response(0) = Actual_Response(0) 'This resets the data stream observation of operanda 1 to detect further responses. 
                Previous_Response(1) = Actual_Response(1) 'The same happens for operanda 2.
                Previous_Response(2) = Actual_Response(2)
                Previous_Response(3) = Actual_Response(3)
                Previous_Response(4) = Actual_Response(4)
                If tmrStart.Enabled = False Then vTimeNow = Environment.TickCount - vTimeStart  'This keeps track of time for the Data output file.
                If tmrStart.Enabled = True Then vTimeNow = (Countdown) - Environment.TickCount
                lblTime.Text = Round(vTimeNow / 1000)



            Catch ex As Exception
            End Try
            My.Application.DoEvents() 'This will enable the rest of the program to run while executing the code from above.
        Loop
        Return 0
    End Function


    Private Sub tmrStart_Tick(sender As Object, e As EventArgs) Handles tmrStart.Tick
        tmrStart.Enabled = False

        WriteLine(1, "Components presented at random: " & CStr(RandomCPres))


        ' =========================================================
        ' Generate randomized component sequence with restriction
        ' (no three identical components in a row)
        ' =========================================================
        If RandomCPres = True Then

            Dim rnd As New Random()
            Dim validSequence As Boolean = False
            Dim maxTries As Integer = 500
            Dim tries As Integer = 0

            Do While Not validSequence AndAlso tries < maxTries
                tries += 1
                validSequence = True
                CompSequence = New List(Of Integer)

                ' Build base component list according to programmed iterations
                For i = 1 To MAXvCC
                    For l = 1 To AC(i).ComponentIteration
                        CompSequence.Add(i)
                    Next
                Next

                ' Shuffle component list
                For i As Integer = CompSequence.Count - 1 To 1 Step -1
                    Dim j As Integer = rnd.Next(0, i + 1)
                    Dim tmp = CompSequence(i)
                    CompSequence(i) = CompSequence(j)
                    CompSequence(j) = tmp
                Next

                ' Check restriction: no three identical components in sequence
                For i As Integer = 2 To CompSequence.Count - 1
                    If CompSequence(i) = CompSequence(i - 1) AndAlso
                   CompSequence(i - 1) = CompSequence(i - 2) Then
                        validSequence = False
                        Exit For
                    End If
                Next
            Loop

            If Not validSequence Then
                WriteLine(1, "WARNING: Could not avoid 3 consecutive components; using last shuffle.")
            Else
                WriteLine(1, "Restricted component sequence: " & String.Join(",", CompSequence))
            End If

            ' Reset sequence index
            CompIndexSeq = 0

        Else
            ' No random presentation: sequence not used
            CompSequence = Nothing
            CompIndexSeq = 0
        End If


        ' =========================================================
        ' Initialize session timing and visual settings
        ' =========================================================
        vTimeStart = Environment.TickCount

        chartResponse(3) += 10
        Chart1.Series("Component 1").Color = Color.FromArgb(100, Color.Blue)
        Chart1.Series("Component 2").Color = Color.FromArgb(100, Color.Red)
        Chart1.Series("Component 3").Color = Color.FromArgb(100, Color.Yellow)
        Chart1.Series("Component 4").Color = Color.FromArgb(100, Color.Green)




        ' =========================================================
        ' Initialize obtained delays containers 
        ' =========================================================
        ReDim ObtainedDelayDurations(MAXvCC, 1)
        For c As Integer = 1 To MAXvCC
            For l As Integer = 0 To 1
                ObtainedDelayDurations(c, l) = New List(Of Integer)
            Next
        Next

        DelayOnset(0) = -1
        DelayOnset(1) = -1
        DelayComp(0) = -1
        DelayComp(1) = -1


        ' Start first component
        BeginPrograms()

    End Sub


    Private Sub BeginPrograms() 'Call this at the start of each component

        ' =========================================================
        ' Select component
        ' =========================================================
        If RandomCPres = True Then
            vCC = CompSequence(CompIndexSeq)
            CompIndexSeq += 1
        End If

        If AC(vCC).IterationsLeft > 0 Then AC(vCC).IterationsLeft -= 1

        ' =========================================================
        ' Update component labels
        ' =========================================================
        lblActiveComponent.Text = vCC
        lblComponentDuration.Text = AC(vCC).ComponentDuration
        lblComponentStim.Text = AC(vCC).ComponentStimType
        lblIterationsLeft.Text = AC(vCC).IterationsLeft

        ' =========================================================
        ' Reset timers and outputs
        ' =========================================================
        tmrComponentStim.Enabled = False
        If AC(vCC).ComponentStimDuration > 0 Then
            tmrComponentStim.Interval = AC(vCC).ComponentStimDuration * 1000
            tmrComponentStim.Enabled = True
        ElseIf AC(vCC).ComponentStimDuration = 0 Then
            tmrComponentStim.Interval = 1
            tmrComponentStim.Enabled = True
        End If

        If AC(vCC).COD > 0 Then tmrCOD.Interval = AC(vCC).COD

        tmrLever1.Enabled = False
        tmrLever2.Enabled = False
        tmrDelay1.Enabled = False
        tmrDelay2.Enabled = False
        tmrStim1.Enabled = False
        tmrStim2.Enabled = False
        tmrNosepoke.Enabled = False
        tmrDelaySignal1.Enabled = False
        tmrDelaySignal2.Enabled = False

        ' Reset current delay onset markers for the new component
        DelayOnset(0) = -1
        DelayOnset(1) = -1
        DelayComp(0) = -1
        DelayComp(1) = -1


        Arduino.WriteLine("a")
        Arduino.WriteLine("b")
        Arduino.WriteLine("t")

        WriteLine(1, vTimeNow, "StartComponent" & vCC)

        tmrComponentDuration.Interval = AC(vCC).ComponentDuration * 1000
        AC(vCC).ComponentDuration_measured(AC(vCC).IterationsLeft) = lblTime.Text
        tmrComponentDuration.Enabled = True

        ' =========================================================
        ' Reset per-component data
        ' =========================================================
        VIList(0) = New List(Of Integer)
        VIList(1) = New List(Of Integer)

        ' -----------------------------
        ' IMPORTANT CHANGE:
        ' Do NOT reset ObtainedDelays here (or you lose session history).
        ' ObtainedDelayDurations(component, lever) already accumulates per component.
        ' -----------------------------
        ' ObtainedDelays(0) = New List(Of Integer)
        ' ObtainedDelays(1) = New List(Of Integer)

        lblSubject.Text = SetUp.txtSubject.Text
        lblSession.Text = SetUp.txtSession.Text
        lblCOM.Text = SetUp.txtCOM.Text

        If AC(vCC).HouselightOnOff = True Then Arduino.WriteLine("H")
        If AC(vCC).HouselightOnOff = False Then Arduino.WriteLine("h")

        If AC(vCC).DelayDuration(0) > 0 Then tmrDelay1.Interval = Math.Max(1, AC(vCC).DelayDuration(0) * 1000)
        If AC(vCC).DelayDuration(1) > 0 Then tmrDelay2.Interval = Math.Max(1, AC(vCC).DelayDuration(1) * 1000)

        If AC(vCC).DelaySignalDuration(0) > 0 Then tmrDelaySignal1.Interval = Math.Max(1, AC(vCC).DelaySignalDuration(0) * 1000)
        If AC(vCC).DelaySignalDuration(1) > 0 Then tmrDelaySignal2.Interval = Math.Max(1, AC(vCC).DelaySignalDuration(1) * 1000)

        If AC(vCC).FeedbackDuration(0) > 0 Then tmrStim1.Interval = Math.Max(1, AC(vCC).FeedbackDuration(0) * 1000)
        If AC(vCC).FeedbackDuration(1) > 0 Then tmrStim2.Interval = Math.Max(1, AC(vCC).FeedbackDuration(1) * 1000)

        ' Lever 1
        If AC(vCC).ScheduleType(0) <> "" AndAlso AC(vCC).ScheduleType(0).ToLower() <> "extinction" Then
            Arduino.WriteLine("L") ' extiende
        Else
            Arduino.WriteLine("l") ' retrae
        End If

        ' Lever 2
        If AC(vCC).ScheduleType(1) <> "" AndAlso AC(vCC).ScheduleType(1).ToLower() <> "extinction" Then
            Arduino.WriteLine("M") ' extiende
        Else
            Arduino.WriteLine("m") ' retrae
        End If

        ' =========================================================
        ' Reset ratio counters & Nosepoke register
        ' =========================================================
        RatioGoal(0) = 0
        RatioGoal(1) = 0
        RatioCount(0) = 0
        RatioCount(1) = 0
        NosepokeIn(0) = False
        tmrNosepoke.Enabled = False

        ' =========================================================
        ' Initialize schedules
        ' =========================================================
        If AC(vCC).ScheduleType(0) = "Fixed Ratio" Then FRGen(0)
        If AC(vCC).ScheduleType(1) = "Fixed Ratio" Then FRGen(1)

        If AC(vCC).ScheduleType(0) = "Variable Ratio" Then VRGen(0)
        If AC(vCC).ScheduleType(1) = "Variable Ratio" Then VRGen(1)

        If AC(vCC).ScheduleType(0) = "Fixed Interval" Then FIGen(0)
        If AC(vCC).ScheduleType(1) = "Fixed Interval" Then FIGen(1)

        If AC(vCC).ScheduleType(0).Contains("Variable Interval") Then VIGen(0)
        If AC(vCC).ScheduleType(1).Contains("Variable Interval") Then VIGen(1)

        ' =========================================================
        ' Update schedule labels and logging
        ' =========================================================
        lblL1.Text = AC(vCC).ScheduleType(0) & " " & AC(vCC).ScheduleValue(0)
        lblL2.Text = AC(vCC).ScheduleType(1) & " " & AC(vCC).ScheduleValue(1)

        WriteLine(1, "Lever 1 Schedule: " & lblL1.Text)
        WriteLine(1, "Lever 2 Schedule: " & lblL2.Text)
        WriteLine(2, "Lever 1 Schedule: " & lblL1.Text)
        WriteLine(2, "Lever 2 Schedule: " & lblL2.Text)

        tmrChrt.Interval = 1000
        tmrChrt.Enabled = True

    End Sub


    Private Sub Response(Lever As Integer)
        ' Registers a response and evaluates whether a reinforcer is available
        ' for ratio- or interval-based schedules.

        If tmrStart.Enabled = False Then

            ' Increment real-time response counter for plotting
            chartResponse(Lever) += 1

            ' ---------------------------------------------------------
            ' Responses during the inter-component interval (ICI)
            ' ---------------------------------------------------------
            If tmrICI.Enabled = True Then
                WriteLine(1, vTimeNow, Lever + 1, "ICIResponse")
                ' Responses during ICI are logged but do not affect schedules

            Else

                ' ---------------------------------------------------------
                ' Changeover Delay (COD) handling
                ' ---------------------------------------------------------
                If tmrCOD.Interval > 0 And tmrCOD.Enabled = False Then
                    tmrCOD.Enabled = True
                    CODL = Lever + 1
                End If

                ' Allow response only if COD has expired or no COD is active
                If Lever + 1 = CODL Or CODL = 0 Then

                    ' Deliver programmed feedback stimulus (if any)
                    If AC(vCC).FeedbackDuration(Lever) > 0 Then Stimulus(Lever)

                    ' ---------------------------------------------------------
                    ' Extinction 
                    ' ---------------------------------------------------------
                    If (AC(vCC).ScheduleType(0) = "Extinction" And Lever = 0) Or
                   (AC(vCC).ScheduleType(1) = "Extinction" And Lever = 1) Then

                        ResponseCount(vCC, Lever) += 1
                        Me.Controls("lblResponses" & (Lever + 1)).Text = ResponseCount(vCC, Lever)
                        WriteLine(1, vTimeNow, "E" & Lever + 1)

                    Else
                        ' ---------------------------------------------------------
                        ' Responses outside reinforcement delay
                        ' ---------------------------------------------------------
                        If tmrDelay1.Enabled = False And tmrDelay2.Enabled = False Then

                            WriteLine(1, vTimeNow, vCC & Lever + 1)
                            ResponseCount(vCC, Lever) += 1
                            Me.Controls("lblResponses" & (Lever + 1)).Text = ResponseCount(vCC, Lever)

                            ' Deliver reinforcer if interval has elapsed
                            If refRdy(Lever) = True Then Reinforce(Lever, False)

                            ' Update ratio counters (FR / VR)
                            Ratio(Lever)

                            ' ---------------------------------------------------------
                            ' Responses during reinforcement delay
                            ' ---------------------------------------------------------
                        ElseIf tmrDelay1.Enabled Then
                            WriteLine(1, vTimeNow, "D1")
                            ResponseCountDel(vCC, Lever) += 1

                        ElseIf tmrDelay2.Enabled Then
                            WriteLine(1, vTimeNow, "D2")
                            ResponseCountDel(vCC, Lever) += 1
                        End If



                    End If

                Else
                    ' Response blocked by active changeover delay
                    WriteLine(1, vTimeNow, Lever + 1, "CODResponse")
                End If

            End If
        End If
    End Sub


    Private Sub Nosepoke(Nose As Integer)

        ' Registers only the ONSET of the nosepoke (entry), not sustained contact.
        ' A new event is allowed only after the subject exits and re-enters.

        If tmrStart.Enabled = False Then

            ' Current raw state coming from Arduino stream:
            ' In your code: value <> 1 means "active" (beam broken / nosepoke present),
            ' value = 1 means "inactive".
            'Dim isActive As Boolean = (Actual_Response(2) <> 1)

            '' 1) ONSET: active now, but previously OUT -> register once
            ' If isActive = True AndAlso NosepokeIn(Nose) = False Then
            'NosepokeIn(Nose) = True

            ' Optional debounce window (keeps your original timer logic)
            ' If tmrNosepoke.Enabled = False Then
            'tmrNosepoke.Enabled = True

            If tmrDelay1.Enabled = True Or tmrDelay2.Enabled = True Then
                WriteLine(1, vTimeNow, "D" & Nose + 3)
                NosepokeCountDel(vCC) += 1
            Else
                NosepokeCount(Nose) += 1
                lblTrayRs.Text = NosepokeCount(Nose)
                chartResponse(2) += 1
                WriteLine(1, vTimeNow, Nose + 3)
            End If
        End If

        '  End If

        ' 2) OFFSET: inactive now -> re-arm for the next entry
        '  If isActive = False Then
        'NosepokeIn(Nose) = False
        'End If

        ' End If

    End Sub


    Private Sub Stimulus(Lever)

        ' Activates feedback stimuli associated with the selected lever
        ' according to the programmed feedback configuration.

        If Lever = 0 Then

            If AC(vCC).FeedbackType(0).Contains("Light 1") = True Then Arduino.WriteLine("A")
            If AC(vCC).FeedbackType(0).Contains("Light 2") = True Then Arduino.WriteLine("B")
            If AC(vCC).FeedbackType(0).Contains("Tone") = True Then Arduino.WriteLine("T")
            If AC(vCC).FeedbackType(0).Contains("Houselight") = True Then Arduino.WriteLine("H")

            ' Time-out feedback suspends component stimulus presentation
            If AC(vCC).FeedbackType(0).Contains("Time Out") = True Then
                tmrComponentStim.Enabled = False
                Arduino.WriteLine("abthlm")
            End If

            tmrStim1.Enabled = True

        ElseIf Lever = 1 Then

            If AC(vCC).FeedbackType(1).Contains("Light 1") = True Then Arduino.WriteLine("A")
            If AC(vCC).FeedbackType(1).Contains("Light 2") = True Then Arduino.WriteLine("B")
            If AC(vCC).FeedbackType(1).Contains("Tone") = True Then Arduino.WriteLine("T")
            If AC(vCC).FeedbackType(1).Contains("Houselight") = True Then Arduino.WriteLine("H")

            ' Time-out feedback suspends component stimulus presentation
            If AC(vCC).FeedbackType(1).Contains("Time Out") = True Then
                tmrComponentStim.Enabled = False
                Arduino.WriteLine("abthlm")
            End If

            tmrStim2.Enabled = True
        End If
    End Sub

    Private Sub Ratio(Lever As Integer)

        ' Counts responses under ratio schedules and sets the reinforcer
        ' availability flag when the ratio requirement is met.

        If RatioGoal(Lever) <> 0 Then
            RatioCount(Lever) += 1

            If RatioCount(Lever) >= RatioGoal(Lever) Then
                refRdy(Lever) = True

                If Lever = 0 Then lblRfR1.Text = refRdy(0)
                If Lever = 1 Then lblRfR2.Text = refRdy(1)

                Reinforce(Lever, False)
                RatioCount(Lever) = 0
            End If
        End If
    End Sub

    Private Sub Reinforce(Lever As Integer, Delay As Boolean)

        ' Lever 1: initiate reinforcement delay
        If Lever = 0 AndAlso AC(vCC).DelayDuration(0) > 0 AndAlso Delay = False Then

            tmrDelay1.Enabled = True
            If AC(vCC).DelayRetract(Lever) = True Then Arduino.WriteLine("l")

            ' Store onset time for this delay (per lever)
            DelayOnset(0) = vTimeNow
            DelayComp(0) = vCC


            ' Activate delay-associated stimuli
            If AC(vCC).DelayType(0) <> "" Then
                If AC(vCC).DelayType(0).Contains("Light 1") = True Then Arduino.WriteLine("A")
                If AC(vCC).DelayType(0).Contains("Light 2") = True Then Arduino.WriteLine("B")
                If AC(vCC).DelayType(0).Contains("Tone") = True Then Arduino.WriteLine("T")
                If AC(vCC).DelayType(0).Contains("Houselight") = True Then Arduino.WriteLine("H")
            End If

            If AC(vCC).DelaySignalDuration(0) > 0 AndAlso AC(vCC).DelaySignalDuration(0) < AC(vCC).DelayDuration(0) Then
                tmrDelaySignal1.Enabled = True
            End If

            ' Lever 2: initiate reinforcement delay
        ElseIf Lever = 1 AndAlso AC(vCC).DelayDuration(1) > 0 AndAlso Delay = False Then

            tmrDelay2.Enabled = True
            If AC(vCC).DelayRetract(Lever) = True Then Arduino.WriteLine("m")

            ' Store onset time for this delay (per lever)
            DelayOnset(1) = vTimeNow
            DelayComp(1) = vCC


            ' Activate delay-associated stimuli
            If AC(vCC).DelayType(1) <> "" Then
                If AC(vCC).DelayType(1).Contains("Light 1") = True Then Arduino.WriteLine("A")
                If AC(vCC).DelayType(1).Contains("Light 2") = True Then Arduino.WriteLine("B")
                If AC(vCC).DelayType(1).Contains("Tone") = True Then Arduino.WriteLine("T")
                If AC(vCC).DelayType(1).Contains("Houselight") = True Then Arduino.WriteLine("H")
            End If

            If AC(vCC).DelaySignalDuration(1) > 0 AndAlso AC(vCC).DelaySignalDuration(1) < AC(vCC).DelayDuration(1) Then
                tmrDelaySignal2.Enabled = True
            End If

            ' External reinforcer trigger (manual)
        ElseIf Lever = 3 Then
            Arduino.WriteLine("R")

            ' Deliver reinforcer now (immediate OR end-of-delay)
        Else
            refRdy(Lever) = False

            RefCount(vCC, Lever) += 1
            RefCount_i(Lever) += 1
            Me.Controls.Item("lblReinforcers" & (Lever + 1)).Text = RefCount(vCC, Lever)

            For i = 1 To AC(vCC).Magnitude(Lever)
                Me.Controls("lblRfR" & (Lever + 1)).Text = refRdy(Lever)
                Chart1.Series("Reinforcers " & Lever + 1).Points.AddXY(chartTime(Lever), chartResponse(Lever) + 3)
                ReinforcerDelivery(Lever)
            Next

            If AC(vCC).Magnitude(Lever) > 0 Then WriteLine(1, vTimeNow, "R" & Lever + 1)

            ' Reinitialize schedules after reinforcement
            If Lever = 0 Then
                If AC(vCC).ScheduleType(0) = "Fixed Ratio" Then FRGen(0)
                If AC(vCC).ScheduleType(0) = "Variable Ratio" Then VRGen(0)
                If AC(vCC).ScheduleType(0) = "Fixed Interval" Then FIGen(0)
                If AC(vCC).ScheduleType(0).Contains("Variable Interval") Then VIGen(0)
            ElseIf Lever = 1 Then
                If AC(vCC).ScheduleType(1) = "Fixed Ratio" Then FRGen(1)
                If AC(vCC).ScheduleType(1) = "Variable Ratio" Then VRGen(1)
                If AC(vCC).ScheduleType(1) = "Fixed Interval" Then FIGen(1)
                If AC(vCC).ScheduleType(1).Contains("Variable Interval") Then VIGen(1)
            End If
        End If

        ' Check component termination based on maximum reinforcers
        If (RefCount_i(0) + RefCount_i(1)) >= AC(vCC).MaxRefs AndAlso AC(vCC).MaxRefs > 0 Then
            ComponentDuration_Code()
        End If

    End Sub


    Private Sub ReinforcerDelivery(Lever)

        ' Sends the appropriate command to the hardware to deliver
        ' the programmed reinforcer type.

        If AC(vCC).Reinforcer(Lever) = "Pellet" Then
            Arduino.WriteLine("R")

        ElseIf AC(vCC).Reinforcer(Lever) = "Liquid" Then
            Arduino.WriteLine("W")

        ElseIf AC(vCC).Reinforcer(Lever) = "Random" Then
            Dim Rand As New Random
            Dim Crit As New Double
            Crit = Rand.Next(1, 101)

            If Crit <= AC(vCC).PelletP(Lever) Then
                Arduino.WriteLine("R")
            Else
                Arduino.WriteLine("W")
            End If
        End If
    End Sub

    Private Sub FRGen(x) 'This initializes Fixed Ratio schedules depending on the selected values / operanda.
        'FR schedules just check current responses against the specified schedule value.
        RatioGoal(x) = AC(vCC).ScheduleValue(x)
    End Sub
    Private Sub VRGen(x) 'This initializes Variable Ratio schedules depending on the selected values / operanda.
        'VR schedules calculate a range between half and 1.5 times the specified schedule value and pick a random value from that range. 
        Randomize()
        Dim Rand As New Random
        RatioGoal(x) = Rand.Next((AC(vCC).ScheduleValue(x) / 2), (AC(vCC).ScheduleValue(x) * 1.5))
    End Sub
    Private Sub FIGen(x) 'This initializes Fixed Interval schedules depending on the selected values / operanda.
        'FI schedules use a timer to check if the specified schedule value has elapsed.
        'Visual Basic manages time in miliseconds, so values are multiplied by 1000.
        If x = 0 Then
            tmrLever1.Interval = (AC(vCC).ScheduleValue(0) + 1) * 1000
            tmrLever1.Enabled = True
        ElseIf x = 1 Then
            tmrLever2.Interval = (AC(vCC).ScheduleValue(1) + 1) * 1000
            tmrLever2.Enabled = True
        End If
    End Sub
    Private Sub VIGen(list As Integer) 'This initializes Variable Interval schedules depending on the selected values / operanda.
        'VI schedules employ Hantula's (1991) method for Fleshler & Hoffman's (1968) iterative equation on Visual Basic.
        'This Subroutine takes 'list' as a value to allocate VI iterations on separate data arrays, allowing for concurrent and independent VI schedules.
        'VI values are selected at random from the lists without replacement. 
        Dim v As Integer
        Dim n = 10 'This value represents the VI iterations. 
        Dim rd(n)
        Dim vi(n)
        Dim order
        Randomize()
        If list = 0 Then v = AC(vCC).ScheduleValue(0)
        If list = 1 Then v = AC(vCC).ScheduleValue(1)
        If VIList(list).Count = 0 Then
            For m As Integer = 1 To n
                If m = n Then vi(m) = v * (1 + Log(n)) : GoTo 1
                vi(m) = v * (1 + (Log(n)) + (n - m) * (Log(n - m)) - (n - m + 1) * Log(n - m + 1))
1:              order = Int((n * Rnd() + 1))
                If rd(order) = 0 Then
                    rd(order) = vi(m)
                Else
                    GoTo 1
                End If
            Next
            For a As Integer = 1 To n
                VIList(list).Add(rd(a))
            Next
        End If
        Dim Rand As New Random
        Dim p As Integer = Rand.Next(VIList(list).Count)
        If list = 0 Then 'The first list saves VI values for schedules on operanda 1.
            tmrLever1.Interval = (VIList(list).Item(p) + 1) * 1000
            tmrLever1.Enabled = True
        End If
        If list = 1 Then 'The second list saves VI values for schedules on operanda 2.
            tmrLever2.Interval = (VIList(list).Item(p) + 1) * 1000
            tmrLever2.Enabled = True
        End If
        VIList(list).RemoveAt(p)
    End Sub
    Private Sub tmrSchedule1_Tick(sender As Object, e As EventArgs) Handles tmrLever1.Tick 'This keeps track of time for interval schedules on operanda 1.
        tmrLever1.Enabled = False
        refRdy(0) = True
        lblRfR1.Text = refRdy(0)
    End Sub
    Private Sub tmrSchedule2_Tick(sender As Object, e As EventArgs) Handles tmrLever2.Tick 'This keeps track of time for interval schedules on operanda 2.
        tmrLever2.Enabled = False
        refRdy(1) = True
        lblRfR2.Text = refRdy(1)
    End Sub
    Private Sub Finish()

        Chart1.SaveImage("C:\Data\Charts\" & SetUp.txtSubject.Text & "_" & SetUp.txtSession.Text & "_chart_" & Format(Date.Now, "hh_mm_ss") & ".png", ChartImageFormat.Png)
        Arduino.WriteLine("nhtabcd") 'Turns off every output on the Arduino.
        Arduino.Close() 'Terminates Arduino-VB communication.

        ' ---------------------------------------------------------
        ' Obtained delays summary (per component, per lever)
        ' ---------------------------------------------------------
        WriteLine(2, "Obtained delays (seconds) by component:")

        For s = 1 To MAXvCC
            If ObtainedDelayDurations(s, 0).Count > 0 Then
                Dim secs1 = ObtainedDelayDurations(s, 0).Select(Function(ms) Math.Round(ms / 1000.0, 3)).ToArray()
                WriteLine(2, "Component " & s & " L1: " & String.Join(", ", secs1))
            Else
                WriteLine(2, "Component " & s & " L1: (none)")
            End If

            If ObtainedDelayDurations(s, 1).Count > 0 Then
                Dim secs2 = ObtainedDelayDurations(s, 1).Select(Function(ms) Math.Round(ms / 1000.0, 3)).ToArray()
                WriteLine(2, "Component " & s & " L2: " & String.Join(", ", secs2))
            Else
                WriteLine(2, "Component " & s & " L2: (none)")
            End If
        Next


        For i = 1 To 2

            ' ---------------------------------------------------------
            ' Compute the *measured* duration for each component (seconds)
            ' and determine which components were actually presented.
            ' ---------------------------------------------------------
            For s = 1 To MAXvCC
                Dim o = 0
                For g = 0 To AC(s).ComponentIteration
                    o += AC(s).ComponentDuration_measured(g)
                Next
                AC(s).ComponentDuration = o
            Next

            ' ---------------------------------------------------------
            ' Responses summary (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Responses:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Lever 1 Component " & s & ": " & ResponseCount(s, 0))
                    WriteLine(i, "Lever 2 Component " & s & ": " & ResponseCount(s, 1))
                End If
            Next

            ' ---------------------------------------------------------
            ' Response rates summary (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Response rates:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Lever 1 Component " & s & ": " & (ResponseCount(s, 0) / (AC(s).ComponentDuration / 60)))
                    WriteLine(i, "Lever 2 Component " & s & ": " & (ResponseCount(s, 1) / (AC(s).ComponentDuration / 60)))
                End If
            Next

            ' ---------------------------------------------------------
            ' Reinforcers summary (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Reinforcers:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Lever 1 Component " & s & ": " & RefCount(s, 0))
                    WriteLine(i, "Lever 2 Component " & s & ": " & RefCount(s, 1))
                End If
            Next

            ' ---------------------------------------------------------
            ' Reinforcer rates summary (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Reinforcer rates:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Lever 1 Component " & s & ": " & (RefCount(s, 0) / (AC(s).ComponentDuration / 60)))
                    WriteLine(i, "Lever 2 Component " & s & ": " & (RefCount(s, 1) / (AC(s).ComponentDuration / 60)))
                End If
            Next

            ' ---------------------------------------------------------
            ' Nosepoke responses during delay (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Nosepoke responses during delay:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Component " & s & ": " & NosepokeCountDel(s))
                End If
            Next

            ' ---------------------------------------------------------
            ' Nosepoke rates during delay (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Nosepoke rates during delay:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Component " & s & ": " & (NosepokeCountDel(s) / (AC(s).ComponentDuration / 60)))
                End If
            Next

            ' ---------------------------------------------------------
            ' Lever responses during delay (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Lever responses during delay:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Component " & s & " L1: " & ResponseCountDel(s, 0))
                End If
            Next
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Component " & s & " L2: " & ResponseCountDel(s, 1))
                End If
            Next

            ' ---------------------------------------------------------
            ' Response rates during delay (only components with duration > 0)
            ' ---------------------------------------------------------
            WriteLine(i, "Response rates during delay:")
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Component " & s & " L1: " & (ResponseCountDel(s, 0) / (AC(s).ComponentDuration / 60)))
                End If
            Next
            For s = 1 To MAXvCC
                If AC(s).ComponentDuration > 0 Then
                    WriteLine(i, "Component " & s & " L2: " & (ResponseCountDel(s, 1) / (AC(s).ComponentDuration / 60)))
                End If
            Next

            ' ---------------------------------------------------------
            ' Session footer
            ' ---------------------------------------------------------
            WriteLine(i, "Total time in minutes: " & lblTime.Text / 60)
            WriteLine(i, Format(Date.Now, "dd-MM-yyyy_hh-mm-ss"))
            WriteLine(i, "END") 'Signals that the session has ended on the data file.
            FileClose(i) 'Closes data file.
        Next

        Application.Exit()

    End Sub


    Private Sub tmrChrt_Tick(sender As Object, e As EventArgs) Handles tmrChrt.Tick

        'This timer tick updates the chart once per second (tmrChrt.Interval = 1000).
        'It advances "chartTime" and plots current response counts plus a visual indicator of the active component.

        For i = 0 To 3
            'Advance the X-axis counters for each plotted stream (lever1, lever2, tray, and component indicator).
            chartTime(i) += 1
        Next

        'Plot cumulative responses for each operandum/tray.
        Chart1.Series("Lever 1").Points.AddXY(chartTime(0), chartResponse(0))
        Chart1.Series("Lever 2").Points.AddXY(chartTime(1), chartResponse(1))
        Chart1.Series("Tray").Points.AddXY(chartTime(2), chartResponse(2))

        'During ICI, no component should be shown as active:
        'all component series are pushed down by CompIndex so they remain visually "off".
        If tmrICI.Enabled = True Then
            Chart1.Series("Component 1").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
            Chart1.Series("Component 2").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
            Chart1.Series("Component 3").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
            Chart1.Series("Component 4").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
        Else
            'When NOT in ICI, exactly one component is shown "on" at chartResponse(3),
            'and the other component series are kept "off" by subtracting CompIndex.
            If vCC = 1 Then
                Chart1.Series("Component 1").Points.AddXY(chartTime(3), chartResponse(3))
                Chart1.Series("Component 2").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
                Chart1.Series("Component 3").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
                Chart1.Series("Component 4").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
            ElseIf vCC = 2 Then
                Chart1.Series("Component 1").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
                Chart1.Series("Component 2").Points.AddXY(chartTime(3), chartResponse(3))
                Chart1.Series("Component 3").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
                Chart1.Series("Component 4").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
            ElseIf vCC = 3 Then
                Chart1.Series("Component 1").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
                Chart1.Series("Component 2").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
                Chart1.Series("Component 3").Points.AddXY(chartTime(3), chartResponse(3))
                Chart1.Series("Component 4").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
            ElseIf vCC = 4 Then
                Chart1.Series("Component 1").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
                Chart1.Series("Component 2").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
                Chart1.Series("Component 3").Points.AddXY(chartTime(3), chartResponse(3) - CompIndex)
                Chart1.Series("Component 4").Points.AddXY(chartTime(3), chartResponse(3))
            End If

        End If

        'If response counts get large, the component indicator line might overlap with the response lines.
        'This block shifts component-series Y-values upward once (by +10) to keep the component indicator readable.
        If (chartResponse(0) > 200 Or chartResponse(1) > 200 Or chartResponse(2) > 200) And chartFlag(0) = False Then
            chartFlag(0) = True

            'Shift only points that are above 0 (avoid moving the "off" baseline).
            For Each pt As DataPoint In Chart1.Series("Component 1").Points
                If pt.YValues(0) > 0 Then pt.YValues(0) += 10
            Next
            For Each pt As DataPoint In Chart1.Series("Component 2").Points
                If pt.YValues(0) > 0 Then pt.YValues(0) += 10
            Next
            For Each pt As DataPoint In Chart1.Series("Component 3").Points
                If pt.YValues(0) > 0 Then pt.YValues(0) += 10
            Next
            For Each pt As DataPoint In Chart1.Series("Component 4").Points
                If pt.YValues(0) > 0 Then pt.YValues(0) += 10
            Next

            'Update the stored offset so future "off" plotting stays aligned with the shifted points.
            CompIndex += 10
            chartResponse(3) += 10

        End If
    End Sub

    Private Sub tmrDelay1_Tick(sender As Object, e As EventArgs) Handles tmrDelay1.Tick

        tmrDelay1.Enabled = False

        If AC(vCC).DelayType(0) <> "" Then
            If AC(vCC).DelayType(0).Contains("Light 1") = True Then Arduino.WriteLine("a")
            If AC(vCC).DelayType(0).Contains("Light 2") = True Then Arduino.WriteLine("b")
            If AC(vCC).DelayType(0).Contains("Tone") = True Then Arduino.WriteLine("t")
            If AC(vCC).DelayType(0).Contains("Houselight") = True Then Arduino.WriteLine("h")
        End If

        ' Deliver reinforcer now
        Reinforce(0, True)

        If AC(vCC).DelayRetract(0) = True Then Arduino.WriteLine("L")

        ' Save obtained delay duration ONLY if the component did NOT change
        If DelayOnset(0) >= 0 AndAlso DelayComp(0) = vCC Then
            Dim dur As Integer = vTimeNow - DelayOnset(0)
            If dur < 0 Then dur = 0
            ObtainedDelayDurations(vCC, 0).Add(dur)
        End If

        DelayOnset(0) = -1
        DelayComp(0) = -1


    End Sub

    Private Sub tmrDelay2_Tick(sender As Object, e As EventArgs) Handles tmrDelay2.Tick

        tmrDelay2.Enabled = False

        If AC(vCC).DelayType(1) <> "" Then
            If AC(vCC).DelayType(1).Contains("Light 1") = True Then Arduino.WriteLine("a")
            If AC(vCC).DelayType(1).Contains("Light 2") = True Then Arduino.WriteLine("b")
            If AC(vCC).DelayType(1).Contains("Tone") = True Then Arduino.WriteLine("t")
            If AC(vCC).DelayType(1).Contains("Houselight") = True Then Arduino.WriteLine("h")
        End If

        ' Deliver reinforcer now
        Reinforce(1, True)

        If AC(vCC).DelayRetract(1) = True Then Arduino.WriteLine("M")

        ' Save obtained delay duration ONLY if the component did NOT change
        If DelayOnset(1) >= 0 AndAlso DelayComp(1) = vCC Then
            Dim dur As Integer = vTimeNow - DelayOnset(1)
            If dur < 0 Then dur = 0
            ObtainedDelayDurations(vCC, 1).Add(dur)
        End If

        DelayOnset(1) = -1
        DelayComp(1) = -1


    End Sub


    Private Sub tmrDelaySignal1_Tick(sender As Object, e As EventArgs) Handles tmrDelaySignal1.Tick
        tmrDelaySignal1.Enabled = False
        If AC(vCC).DelayType(0) <> "" Then
            If AC(vCC).DelayType(0).Contains("Light 1") = True Then Arduino.WriteLine("a")
            If AC(vCC).DelayType(0).Contains("Light 2") = True Then Arduino.WriteLine("b")
            If AC(vCC).DelayType(0).Contains("Tone") = True Then Arduino.WriteLine("t")
            If AC(vCC).DelayType(0).Contains("Houselight") = True Then Arduino.WriteLine("h")
        End If
    End Sub

    Private Sub tmrDelaySignal2_Tick(sender As Object, e As EventArgs) Handles tmrDelaySignal2.Tick
        tmrDelaySignal2.Enabled = False
        If AC(vCC).DelayType(1) <> "" Then
            If AC(vCC).DelayType(1).Contains("Light 1") = True Then Arduino.WriteLine("a")
            If AC(vCC).DelayType(1).Contains("Light 2") = True Then Arduino.WriteLine("b")
            If AC(vCC).DelayType(1).Contains("Tone") = True Then Arduino.WriteLine("t")
            If AC(vCC).DelayType(1).Contains("Houselight") = True Then Arduino.WriteLine("h")
        End If
    End Sub

    Private Sub tmrStim1_Tick(sender As Object, e As EventArgs) Handles tmrStim1.Tick

        'This tick ends the brief response-produced feedback for Lever 1.
        'It turns off the feedback stimuli and, if feedback included a Time Out,
        'it resumes the component stimulation and lever availability after TO ends.

        tmrStim1.Enabled = False

        'Turn off feedback stimuli for Lever 1.
        If AC(vCC).FeedbackType(0).Contains("Light 1") = True Then Arduino.WriteLine("a")
        If AC(vCC).FeedbackType(0).Contains("Light 2") = True Then Arduino.WriteLine("b")
        If AC(vCC).FeedbackType(0).Contains("Tone") = True Then Arduino.WriteLine("t")
        If AC(vCC).FeedbackType(0).Contains("Houselight") = True Then Arduino.WriteLine("h")

        'If this feedback period implemented a Time Out, resume component stimulation afterwards.
        If AC(vCC).FeedbackType(0).Contains("Time Out") = True Then
            tmrComponentStim.Enabled = True

            'Restore lever availability after TO (device-specific: "ML" appears to re-enable both levers).
            Arduino.WriteLine("ML")

            'Resume the programmed component stimulus.
            If AC(vCC).ComponentStimType.Contains("Light 1") = True Then Arduino.WriteLine("A") 'Resumes component stimulation
            If AC(vCC).ComponentStimType.Contains("Light 2") = True Then Arduino.WriteLine("B")
            If AC(vCC).ComponentStimType.Contains("Tone") = True Then Arduino.WriteLine("T")
            If AC(vCC).ComponentStimType.Contains("Houselight") = True Then Arduino.WriteLine("H")

            'If the houselight is set to be on during the component, enforce that state here as well.
            If AC(vCC).HouselightOnOff = True Then Arduino.WriteLine("H")
        End If
    End Sub

    Private Sub tmrStim2_Tick(sender As Object, e As EventArgs) Handles tmrStim2.Tick

        'Same logic as tmrStim1_Tick, but for Lever 2.

        tmrStim2.Enabled = False

        'Turn off feedback stimuli for Lever 2.
        If AC(vCC).FeedbackType(1).Contains("Light 1") = True Then Arduino.WriteLine("a")
        If AC(vCC).FeedbackType(1).Contains("Light 2") = True Then Arduino.WriteLine("b")
        If AC(vCC).FeedbackType(1).Contains("Tone") = True Then Arduino.WriteLine("t")
        If AC(vCC).FeedbackType(1).Contains("Houselight") = True Then Arduino.WriteLine("h")

        'If this feedback period implemented a Time Out, resume component stimulation afterwards.
        If AC(vCC).FeedbackType(1).Contains("Time Out") = True Then
            tmrComponentStim.Enabled = True

            'Restore lever availability after TO.
            Arduino.WriteLine("ML")

            'Resume the programmed component stimulus.
            If AC(vCC).ComponentStimType.Contains("Light 1") = True Then Arduino.WriteLine("A")
            If AC(vCC).ComponentStimType.Contains("Light 2") = True Then Arduino.WriteLine("B")
            If AC(vCC).ComponentStimType.Contains("Tone") = True Then Arduino.WriteLine("T")
            If AC(vCC).ComponentStimType.Contains("Houselight") = True Then Arduino.WriteLine("H")

            'If the houselight is set to be on during the component, enforce that state here as well.
            If AC(vCC).HouselightOnOff = True Then Arduino.WriteLine("H")
        End If
    End Sub

    Private Sub btnFinish_Click(sender As Object, e As EventArgs) Handles btnFinish.Click
        'This handler is triggered when the user manually ends the session by clicking the Finish button.
        'It immediately stops all component-related activity, disables manual controls,
        'and schedules a short post-session period before final cleanup and data saving.

        lblActiveComponent.Text = "Session End"     'Visually indicate that the session has ended.
        tmrComponentDuration.Enabled = False        'Stop the component duration timer.
        tmrComponentStim.Enabled = False            'Stop any component-related stimulation timer.

        'Disable all manual interaction controls to prevent further responses.
        btnFinish.Enabled = False
        btnLever1.Enabled = False
        btnLever2.Enabled = False
        btnL1IO.Enabled = False
        btnL2IO.Enabled = False
        btnReinforce.Enabled = False

        'Start the post-session timer (e.g., to allow animals to consume the last reinforcer).
        tmrPostSession.Interval = Math.Max(1, SetUp.txbPostSession.Text * 1000)
        tmrPostSession.Enabled = True
    End Sub

    Private Sub btnL1IO_Click(sender As Object, e As EventArgs) Handles btnL1IO.Click
        'Manually toggles the input/output state of Lever 1.
        'This is typically used for testing or manual intervention during setup.

        If PalIO(0) = True Then
            PalIO(0) = False
            Arduino.WriteLine("L")   'Deactivate / retract lever 1.
        ElseIf PalIO(0) = False Then
            PalIO(0) = True
            Arduino.WriteLine("l")   'Activate / extend lever 1.
        End If
    End Sub

    Private Sub btnL2IO_Click(sender As Object, e As EventArgs) Handles btnL2IO.Click
        'Manually toggles the input/output state of Lever 2.

        If PalIO(1) = True Then
            PalIO(1) = False
            Arduino.WriteLine("M")   'Deactivate / retract lever 2.
        ElseIf PalIO(1) = False Then
            PalIO(1) = True
            Arduino.WriteLine("m")   'Activate / extend lever 2.
        End If
    End Sub

    Private Sub btnLever1_Click(sender As Object, e As EventArgs) Handles btnLever1.Click
        'Registers a manual response on Lever 1 (useful for testing without Arduino input).
        Response(0)
    End Sub

    Private Sub btnLever2_Click(sender As Object, e As EventArgs) Handles btnLever2.Click
        'Registers a manual response on Lever 2.
        Response(1)
    End Sub

    Private Sub btnReinforce_Click(sender As Object, e As EventArgs) Handles btnReinforce.Click
        'Manually delivers a reinforcer independently of the programmed schedules.
        'Lever = 3 is used here as a special case to force delivery.
        Reinforce(3, False)
    End Sub

    Private Sub tmrPostSession_Tick(sender As Object, e As EventArgs) Handles tmrPostSession.Tick
        'This timer fires after the post-session interval has elapsed.
        'It performs final cleanup, writes summaries, closes files, and exits the program.

        tmrPostSession.Enabled = False
        Finish()
    End Sub

    Private Sub tmrNosepoke_Tick(sender As Object, e As EventArgs) Handles tmrNosepoke.Tick
        'This timer prevents multiple nosepoke registrations from a single sustained poke.
        'Once elapsed, it allows nosepoke detection again.
        tmrNosepoke.Enabled = False
    End Sub

    Private Sub tmrComponentDuration_Tick(sender As Object, e As EventArgs) Handles tmrComponentDuration.Tick
        'This timer marks the programmed end of the current component.
        'Control is passed to ComponentDuration_Code to handle the transition.
        ComponentDuration_Code()
    End Sub

    Private Sub ComponentDuration_Code()
        'This routine handles the formal termination of a component.
        'It logs the component end, computes the actual component duration,
        'clears component-related UI elements, and initiates the inter-component interval (ICI).

        tmrComponentDuration.Enabled = False         'Stop the component duration timer.
        RefCount_i(0) = 0                            'Reset within-component reinforcer counters.
        RefCount_i(1) = 0

        'Log the end of the component in the data file.
        WriteLine(1, vTimeNow, "EndComponent" & vCC)

        'Compute actual component duration by subtracting start time from current time.
        AC(vCC).ComponentDuration_measured(AC(vCC).IterationsLeft) =
            lblTime.Text - AC(vCC).ComponentDuration_measured(AC(vCC).IterationsLeft)

        'Update UI to reflect that the system is now in the ICI.
        lblActiveComponent.Text = "ICI"
        lblComponentDuration.Text = SetUp.txbICI.Text
        lblComponentStim.Text = ""
        lblIterationsLeft.Text = ""
        lblL1.Text = ""
        lblL2.Text = ""
        lblRfR1.Text = ""
        lblRfR2.Text = ""

        'Turn off component stimulation.
        tmrComponentStim.Enabled = False

        'Deactivate outputs during ICI; include levers only if ICI duration > 0.
        If SetUp.txbICI.Text <> 0 Then
            Arduino.WriteLine("abhtlm")
        Else
            Arduino.WriteLine("abht")
        End If


        'If a component ends while a delay is active, invalidate delay annotation
        DelayOnset(0) = -1 : DelayComp(0) = -1
        DelayOnset(1) = -1 : DelayComp(1) = -1


        'Start the inter-component interval timer.
        tmrICI.Enabled = True
    End Sub

    Private Sub tmrComponentStim_Tick(sender As Object, e As EventArgs) Handles tmrComponentStim.Tick
        'This timer controls continuous or intermittent component-level stimulation
        '(e.g., blinking lights or pulsed tones during a component).

        If AC(vCC).ComponentStimDuration = 0 Then
            'If the stimulus duration is 0, stimulation is continuous and only needs to be turned on once.
            tmrComponentStim.Enabled = False

            If AC(vCC).ComponentStimType.Contains("Light 1") = True Then Arduino.WriteLine("A")
            If AC(vCC).ComponentStimType.Contains("Light 2") = True Then Arduino.WriteLine("B")
            If AC(vCC).ComponentStimType.Contains("Tone") = True Then Arduino.WriteLine("T")
            If AC(vCC).ComponentStimType.Contains("Houselight") = True Then Arduino.WriteLine("H")
        Else
            'If the stimulus is intermittent, toggle it on and off each tick.
            If StimInt = False Then
                StimInt = True
                If AC(vCC).ComponentStimType.Contains("Light 1") = True Then Arduino.WriteLine("A")
                If AC(vCC).ComponentStimType.Contains("Light 2") = True Then Arduino.WriteLine("B")
                If AC(vCC).ComponentStimType.Contains("Tone") = True Then Arduino.WriteLine("T")
                If AC(vCC).ComponentStimType.Contains("Houselight") = True Then Arduino.WriteLine("H")
            ElseIf StimInt = True Then
                StimInt = False
                If AC(vCC).ComponentStimType.Contains("Light 1") = True Then Arduino.WriteLine("a")
                If AC(vCC).ComponentStimType.Contains("Light 2") = True Then Arduino.WriteLine("b")
                If AC(vCC).ComponentStimType.Contains("Tone") = True Then Arduino.WriteLine("t")
                If AC(vCC).ComponentStimType.Contains("Houselight") = True Then Arduino.WriteLine("h")
            End If
        End If
    End Sub

    Private Sub tmrICI_Tick(sender As Object, e As EventArgs) Handles tmrICI.Tick
        'This timer fires at the end of the inter-component interval.
        'It determines whether all components have been exhausted or selects the next component.

        Dim allDepleted As Boolean = True

        'Check whether any component still has iterations remaining.
        For i = 1 To MAXvCC
            If AC(i).IterationsLeft > 0 Then allDepleted = False
        Next

        If allDepleted = True Then ComponentsDepleted = True

        'If all components are depleted, end the session.
        If ComponentsDepleted = True Then
            btnFinish.PerformClick()
        Else
            'Otherwise, select the next component and start it.
            tmrICI.Enabled = False

            If RandomCPres = False Then
                'Sequential component presentation.
                If vCC = MAXvCC Then
                    If AC(1).IterationsLeft > 0 Then
                        vCC = 1
                    ElseIf AC(2).IterationsLeft > 0 Then
                        vCC = 2
                    ElseIf AC(3).IterationsLeft > 0 Then
                        vCC = 3
                    ElseIf AC(4).IterationsLeft > 0 Then
                        vCC = 4
                    End If
                Else
                    vCC += 1
                End If

                BeginPrograms()
            ElseIf RandomCPres = True Then
                'Random (restricted) component presentation.
                BeginPrograms()
            End If
        End If
    End Sub

    Private Sub tmrCOD_Tick(sender As Object, e As EventArgs) Handles tmrCOD.Tick
        'This timer enforces the changeover delay (COD).
        'Once it elapses, responses on either lever are again eligible to produce consequences.

        tmrCOD.Enabled = False
        CODL = 0
    End Sub


End Class