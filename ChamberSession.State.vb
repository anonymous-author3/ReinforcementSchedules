Partial Public Class ChamberSession
    Private CODL As Integer
    Private CompIndex As Byte = 10
    Private chartFlag(1) As Boolean
    Private SessionUID As String = ""
    Private vTimeStart As Integer
    Private vTimeNow As Integer
    Private Countdown As Integer
    Private Actual_Response(MAX_INPUTS - 1) As String
    Private Previous_Response(MAX_INPUTS - 1) As String
    Private RefCount(MAX_COMPONENTS, MAX_INPUTS - 1) As Integer
    Private RefCount_i(MAX_INPUTS - 1) As Integer
    Private TimeRefCount(MAX_COMPONENTS) As Integer
    Private TimeRefCount_i As Integer
    Private refRdy(MAX_INPUTS - 1) As Boolean
    Private VIList(MAX_INPUTS - 1) As List(Of Double)
    Private ObtainedDelays(MAX_INPUTS - 1) As List(Of Integer)
    Private RatioCount(MAX_INPUTS - 1) As Integer
    Private RatioGoal(MAX_INPUTS - 1) As Integer
    Private ResponseCount(MAX_COMPONENTS, MAX_INPUTS - 1) As Integer
    Private ResponseCountDel(MAX_COMPONENTS, MAX_INPUTS - 1) As Integer
    Private chartTime(MAX_INPUTS) As Integer
    Private chartResponse(MAX_INPUTS) As Integer
    Private StimInt As Boolean
    Private vCC As Byte 'vCurrentComponent - contador
    Private MAXvCC As Byte
    Private RandomCPres As Boolean
    Private ComponentsDepleted As Boolean
    Private PalIO(MAX_INPUTS - 1) As Boolean
    Private AC(MAX_COMPONENTS) As ComponentBlueprint ' ActualComponent

End Class
