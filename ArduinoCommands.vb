Imports System.Runtime.Serialization
Imports System.Text

<DataContract(Name:="ArduinoCommandMap", [Namespace]:="urn:reinforcement-schedules:setup")>
Public Class ArduinoCommandMap
    'Order: six insert/retract pairs, four lights, house light, tone, pump,
    'then feeder pulse, pump pulse, tone pulse and all-off. Matches the bundled sketch.
    Public Const LegacyCodes As String = "LlMmCcDdNnOoAaBbEeFfHhTtPpRWZX"
    Public Const DefaultCodes As String = LegacyCodes & "UV"

    <DataMember(IsRequired:=True)>
    Public Property Codes As String = DefaultCodes

    Public Function Copy() As ArduinoCommandMap
        Dim result As New ArduinoCommandMap With {.Codes = Codes}
        result.NormalizeLegacy()
        Return result
    End Function

    Public Shared Function CommandName(index As Integer) As String
        If index < 12 Then Return "Lever " & (index \ 2 + 1) & If(index Mod 2 = 0, " insert", " retract")
        If index < 20 Then Return "Light " & ((index - 12) \ 2 + 1) & If(index Mod 2 = 0, " on", " off")
        Select Case index
            Case 20 : Return "House light on"
            Case 21 : Return "House light off"
            Case 22 : Return "Tone on"
            Case 23 : Return "Tone off"
            Case 24 : Return "Pump 1 on"
            Case 25 : Return "Pump 1 off"
            Case 26 : Return "Feeder 1 pulse"
            Case 27 : Return "Pump 1 pulse"
            Case 28 : Return "Tone pulse"
            Case 29 : Return "All off"
            Case 30 : Return "Feeder 2 pulse"
            Case 31 : Return "Pump 2 pulse"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(index))
        End Select
    End Function

    Public Sub NormalizeLegacy()
        If Codes Is Nothing OrElse Codes.Length <> LegacyCodes.Length Then Return
        'Never overwrite a historical mapping. Append two unused letters only.
        For Each preferred As Char In "UV"
            Dim available As Char = preferred
            If Codes.Contains(available) Then available = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".First(Function(letter) Not Codes.Contains(letter))
            Codes &= available
        Next
    End Sub

    Public Function ValidationError() As String
        NormalizeLegacy()
        If Codes Is Nothing OrElse Codes.Length <> DefaultCodes.Length Then Return "Enter one letter for each Arduino command."
        Dim used As New Dictionary(Of Char, Integer)()
        For index As Integer = 0 To Codes.Length - 1
            Dim letter As Char = Codes(index)
            If Not ((letter >= "A"c AndAlso letter <= "Z"c) OrElse (letter >= "a"c AndAlso letter <= "z"c)) Then
                Return CommandName(index) & " must use one letter from A-Z or a-z. Uppercase and lowercase are different commands."
            End If
            If used.ContainsKey(letter) Then
                Return "The letter '" & letter & "' is assigned to both " & CommandName(used(letter)) & " and " & CommandName(index) & ". Choose a different letter."
            End If
            used.Add(letter, index)
        Next
        Return ""
    End Function

    'Call exactly once, at the serial boundary. The engine uses the default codes
    'as logical action identifiers, even when a macro remaps the transmitted letters.
    Public Function Encode(logicalCommands As String) As String
        NormalizeLegacy()
        Dim result As New StringBuilder()
        For Each logicalCommand As Char In logicalCommands
            Dim index As Integer = DefaultCodes.IndexOf(logicalCommand)
            If index < 0 Then Throw New ArgumentException("Unknown Arduino action: " & logicalCommand)
            result.Append(Codes(index))
        Next
        Return result.ToString()
    End Function

    Public Shared Function LeverAction(inputIndex As Integer, insert As Boolean) As String
        If inputIndex < 0 OrElse inputIndex >= MAX_INPUTS Then Return ""
        Return DefaultCodes(inputIndex * 2 + If(insert, 0, 1)).ToString()
    End Function

    Public Shared Function RetractAllActions() As String
        Return String.Concat(Enumerable.Range(0, MAX_INPUTS).Select(Function(index) LeverAction(index, False)))
    End Function

    Public Shared Function FromLegacyLevers(letters As String) As ArduinoCommandMap
        If letters Is Nothing OrElse letters.Length <> MAX_INPUTS Then Throw New ArgumentException("Enter one letter for each lever.")
        Dim codes As Char() = LegacyCodes.ToCharArray()
        For index As Integer = 0 To MAX_INPUTS - 1
            codes(index * 2) = Char.ToUpperInvariant(letters(index))
            codes(index * 2 + 1) = Char.ToLowerInvariant(letters(index))
        Next
        Dim result As New ArduinoCommandMap With {.Codes = New String(codes)}
        Dim message As String = result.ValidationError()
        If message <> "" Then Throw New ArgumentException(message)
        Return result
    End Function
End Class

Module ArduinoCommands
    'Only the current SetUp map. Added chambers receive an independent copy.
    Private configured As New ArduinoCommandMap()

    Public Function Snapshot() As ArduinoCommandMap
        Return configured.Copy()
    End Function

    Public Sub Configure(settings As ArduinoCommandMap)
        If settings Is Nothing Then Throw New ArgumentNullException(NameOf(settings))
        Dim message As String = settings.ValidationError()
        If message <> "" Then Throw New ArgumentException(message)
        configured = settings.Copy()
    End Sub
End Module
