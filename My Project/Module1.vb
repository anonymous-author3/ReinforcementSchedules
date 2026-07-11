Module Module1
    Public Const MAX_INPUTS As Integer = 6
    Public Const MAX_COMPONENTS As Integer = 10
    Public TestsPerformed As Boolean
    Public CODL As Integer
    Public CompIndex As Byte = 10
    Public chartFlag(1) As Boolean
    Public ICIcounter As Integer
    Public vFile(3) As String
    Public rep(10) As Integer
    Public Lever1 As String = ""
    Public Lever2 As String = ""
    Public vTimeStart As Integer
    Public vTimeNow As Integer
    Public DelayIndex(1) As Integer
    'Public DelayIndex2 As Integer
    Public Countdown As Integer
    Public Actual_Response(MAX_INPUTS - 1) As String
    Public Previous_Response(MAX_INPUTS - 1) As String
    Public RefCount(MAX_COMPONENTS, MAX_INPUTS - 1) As Integer
    Public RefCount_i(MAX_INPUTS - 1) As Integer
    Public refRdy(MAX_INPUTS - 1) As Boolean
    Public VIList(MAX_INPUTS - 1) As List(Of Integer)
    Public ObtainedDelays(MAX_INPUTS - 1) As List(Of Integer)
    Public RatioCount(MAX_INPUTS - 1) As Integer
    Public RatioGoal(MAX_INPUTS - 1) As Integer
    Public ResponseCount(MAX_COMPONENTS, MAX_INPUTS - 1) As Integer
    Public ResponseCountDel(MAX_COMPONENTS, MAX_INPUTS - 1) As Integer
    Public chartTime(MAX_INPUTS) As Integer
    Public chartResponse(MAX_INPUTS) As Integer
    Public StimInt As Boolean
    Public vCC As Byte 'vCurrentComponent - contador
    Public MAXvCC As Byte
    Public RandomCPres As Boolean
    Public PreviousComp(1) As Byte
    Public ComponentsDepleted As Boolean
    Public PalIO(MAX_INPUTS - 1) As Boolean
    Public CompList As List(Of Integer)
    Public AC(MAX_COMPONENTS) As ComponentBlueprint ' ActualComponent

    Public Structure ComponentBlueprint
        Dim HouselightOnOff As Boolean
        Dim COD As Double
        Dim MaxRefs As Integer
        Dim ComponentName As String
        Dim ComponentDuration As Double
        Dim ComponentDuration_measured() As Integer
        Dim ComponentIteration As Byte
        Dim ComponentLightIntermittency As Double
        Dim ComponentStimDuration As Double
        Dim ComponentStimType As String
        Dim InputCount As Integer
        Dim InputName() As String
        Dim ScheduleType() As String
        Dim ScheduleValue() As Integer
        Dim Magnitude() As Integer
        Dim Reinforcer() As String
        Dim PelletP() As Integer
        Dim FeedbackDuration() As Double
        Dim FeedbackType() As String
        Dim DelayDuration() As Double
        Dim DelaySignalDuration() As Double
        Dim DelayType() As String
        Dim DelayRetract() As Boolean
        Dim IterationsLeft As Byte
    End Structure

End Module
