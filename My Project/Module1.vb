Module Module1
    Public Const MAX_INPUTS As Integer = 6
    Public Const MAX_COMPONENTS As Integer = 10
    Public TestsPerformed As Boolean
    Public vFile(3) As String
    Public vCC As Byte 'vCurrentComponent - contador
    Public RandomCPres As Boolean
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
        Dim DeliveryP() As Integer
        Dim PelletP() As Integer
        Dim TDurationD() As Double
        Dim TDurationDelta() As Double
        Dim TProbabilityD() As Integer
        Dim TProbabilityDelta() As Integer
        Dim TCycles() As Integer
        Dim TStartPeriod() As String
        Dim TInterPeriod() As Double
        Dim TStimD() As String
        Dim TStimDelta() As String
        Dim TimeScheduleType As String
        Dim TimeScheduleValue As Double
        Dim TimeReinforcer As String
        Dim TimeMagnitude As Integer
        Dim TimeDeliveryP As Integer
        Dim TimePelletP As Integer
        Dim FeedbackDuration() As Double
        Dim FeedbackType() As String
        Dim DelayDuration() As Double
        Dim DelaySignalDuration() As Double
        Dim DelayType() As String
        Dim DelayRetract() As Boolean
        Dim IterationsLeft As Byte
    End Structure

End Module
