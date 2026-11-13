Option Strict On

'Protocol identity only: usable by reference parsers without editor/runtime dependencies.
Friend NotInheritable Class ReinforcerOutputs
    Public Shared ReadOnly Names As String() = {"Feeder 1", "Pump 1", "Feeder 2", "Pump 2"}
    Private Sub New()
    End Sub
    Public Shared Function CanonicalName(kind As String) As String
        If kind = "Pellet" Then Return "Feeder 1"
        If kind = "Liquid" Then Return "Pump 1"
        Return kind
    End Function
    Public Shared Function IsConcrete(kind As String) As Boolean
        Return Names.Contains(CanonicalName(kind))
    End Function
    Public Shared Function Command(kind As String) As String
        Select Case CanonicalName(kind)
            Case "Feeder 1" : Return "R"
            Case "Pump 1" : Return "W"
            Case "Feeder 2" : Return "U"
            Case "Pump 2" : Return "V"
            Case Else : Throw New ArgumentException("Unknown reinforcer output: " & kind)
        End Select
    End Function
End Class
