'Compatibility for macros that stored only the six lever letters.
Module LeverCommands
    Public ReadOnly Property DefaultLeverCommandLetters As String
        Get
            Return String.Concat(Enumerable.Range(0, MAX_INPUTS).Select(Function(index) ArduinoCommandMap.LeverAction(index, True)))
        End Get
    End Property

    Public Function LeverCommandValidationError(letters As String) As String
        Try
            ArduinoCommandMap.FromLegacyLevers(letters)
            Return ""
        Catch ex As ArgumentException
            Return ex.Message
        End Try
    End Function

    Public Sub ConfigureLeverCommands(letters As String)
        ArduinoCommands.Configure(ArduinoCommandMap.FromLegacyLevers(letters))
    End Sub
End Module
