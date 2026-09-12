Module LeverCommands
    Public Const DefaultLeverCommandLetters As String = "LMCDNO"
    Private configuredLetters As String = DefaultLeverCommandLetters

    Public ReadOnly Property LeverCommandLetters As String
        Get
            Return configuredLetters
        End Get
    End Property

    Public Function LeverCommandValidationError(letters As String) As String
        If letters Is Nothing OrElse letters.Length <> MAX_INPUTS Then
            Return "Enter one letter for each of the " & MAX_INPUTS & " inputs."
        End If

        Dim usedLetters As New HashSet(Of Char)()
        For inputIndex As Integer = 0 To MAX_INPUTS - 1
            Dim letter As Char = letters.ToUpperInvariant()(inputIndex)
            If letter < "A"c OrElse letter > "Z"c Then
                Return "Input " & (inputIndex + 1) & " must use a letter from A to Z."
            End If
            If "ABEFHTPRWXZ".IndexOf(letter) >= 0 Then
                Return "The letter " & letter & " is already used by a light, tone, feeder, pump, or all-off command."
            End If
            If usedLetters.Add(letter) = False Then
                Return "Each input must have a different letter. " & letter & " is repeated."
            End If
        Next
        Return ""
    End Function

    Public Sub ConfigureLeverCommands(letters As String)
        Dim validationError As String = LeverCommandValidationError(letters)
        If validationError <> "" Then Throw New ArgumentException(validationError, NameOf(letters))
        configuredLetters = letters.ToUpperInvariant()
    End Sub

    Public Function InputOutputCommand(inputIndex As Integer, turnOn As Boolean) As String
        Return CommandFor(configuredLetters, inputIndex, turnOn)
    End Function

    Public Function CommandFor(letters As String, inputIndex As Integer, turnOn As Boolean) As String
        If inputIndex < 0 OrElse inputIndex >= MAX_INPUTS Then Return ""
        Dim letter As String = letters(inputIndex).ToString()
        Return If(turnOn, letter, letter.ToLowerInvariant())
    End Function

    Public Function AllInputsOffCommand() As String
        Return configuredLetters.ToLowerInvariant()
    End Function
End Module
