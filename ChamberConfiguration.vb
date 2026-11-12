Public Class ChamberConfiguration
    Public Property BoxName As String
    Public Property ComPort As String
    Public Property Subject As String
    Public Property SessionName As String
    Public Property Weight As String
    Public Property StartDelaySeconds As Integer
    Public Property PostSessionSeconds As Integer
    Public Property ICISeconds As Integer
    Public Property RetractDuringICI As Boolean
    Public Property ICIStimulus As String
    Public Property RandomPresentation As Boolean
    Public Property ManualSequence As List(Of Integer)
    Public Property CommandMap As ArduinoCommandMap
    Friend Property Components As ComponentBlueprint()
    Public Property ComponentCount As Integer
    Public Property SnapshotLines As String()

    Public Function Copy() As ChamberConfiguration
        Dim result As ChamberConfiguration = DirectCast(MemberwiseClone(), ChamberConfiguration)
        result.CommandMap = CommandMap.Copy()
        result.Components = Components.Select(Function(component) CloneComponent(component)).ToArray()
        result.ManualSequence = If(ManualSequence Is Nothing, Nothing, New List(Of Integer)(ManualSequence))
        result.SnapshotLines = DirectCast(SnapshotLines.Clone(), String())
        Return result
    End Function
    Friend Shared Function CloneComponent(source As ComponentBlueprint) As ComponentBlueprint
        Dim copy As ComponentBlueprint = source
        If source.ClassicChains IsNot Nothing Then copy.ClassicChains = source.ClassicChains.Select(Function(chain) If(chain Is Nothing, Nothing, chain.Copy())).ToArray()
        If source.YokedReference IsNot Nothing Then copy.YokedReference = source.YokedReference.Copy()
        If source.LagHistoryReference IsNot Nothing Then copy.LagHistoryReference = source.LagHistoryReference.Copy()
        If source.ComponentDuration_measured IsNot Nothing Then copy.ComponentDuration_measured = DirectCast(source.ComponentDuration_measured.Clone(), Integer())
        If source.InputName IsNot Nothing Then copy.InputName = DirectCast(source.InputName.Clone(), String())
        If source.ScheduleType IsNot Nothing Then copy.ScheduleType = DirectCast(source.ScheduleType.Clone(), String())
        If source.VariableValueCount IsNot Nothing Then copy.VariableValueCount = DirectCast(source.VariableValueCount.Clone(), Integer())
        If source.ScheduleValue IsNot Nothing Then copy.ScheduleValue = DirectCast(source.ScheduleValue.Clone(), Integer())
        If source.Magnitude IsNot Nothing Then copy.Magnitude = DirectCast(source.Magnitude.Clone(), Integer())
        If source.Reinforcer IsNot Nothing Then copy.Reinforcer = DirectCast(source.Reinforcer.Clone(), String())
        If source.DeliveryP IsNot Nothing Then copy.DeliveryP = DirectCast(source.DeliveryP.Clone(), Integer())
        If source.PelletP IsNot Nothing Then copy.PelletP = DirectCast(source.PelletP.Clone(), Integer())
        If source.TDurationD IsNot Nothing Then copy.TDurationD = DirectCast(source.TDurationD.Clone(), Double())
        If source.TDurationDelta IsNot Nothing Then copy.TDurationDelta = DirectCast(source.TDurationDelta.Clone(), Double())
        If source.TProbabilityD IsNot Nothing Then copy.TProbabilityD = DirectCast(source.TProbabilityD.Clone(), Integer())
        If source.TProbabilityDelta IsNot Nothing Then copy.TProbabilityDelta = DirectCast(source.TProbabilityDelta.Clone(), Integer())
        If source.TCycles IsNot Nothing Then copy.TCycles = DirectCast(source.TCycles.Clone(), Integer())
        If source.TStartPeriod IsNot Nothing Then copy.TStartPeriod = DirectCast(source.TStartPeriod.Clone(), String())
        If source.TInterPeriod IsNot Nothing Then copy.TInterPeriod = DirectCast(source.TInterPeriod.Clone(), Double())
        If source.TStimD IsNot Nothing Then copy.TStimD = DirectCast(source.TStimD.Clone(), String())
        If source.TStimDelta IsNot Nothing Then copy.TStimDelta = DirectCast(source.TStimDelta.Clone(), String())
        If source.FeedbackDuration IsNot Nothing Then copy.FeedbackDuration = DirectCast(source.FeedbackDuration.Clone(), Double())
        If source.FeedbackType IsNot Nothing Then copy.FeedbackType = DirectCast(source.FeedbackType.Clone(), String())
        If source.DelayDuration IsNot Nothing Then copy.DelayDuration = DirectCast(source.DelayDuration.Clone(), Double())
        If source.DelaySignalDuration IsNot Nothing Then copy.DelaySignalDuration = DirectCast(source.DelaySignalDuration.Clone(), Double())
        If source.DelayType IsNot Nothing Then copy.DelayType = DirectCast(source.DelayType.Clone(), String())
        If source.DelayRetract IsNot Nothing Then copy.DelayRetract = DirectCast(source.DelayRetract.Clone(), Boolean())
        Return copy
    End Function
End Class
