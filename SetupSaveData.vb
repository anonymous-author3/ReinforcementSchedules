Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Xml

'A saved editor configuration is separate from a running chamber's state.
<DataContract(Name:="ReinforcementSchedulesSetup", [Namespace]:="urn:reinforcement-schedules:setup")>
Friend Class SetupSaveData
    <DataMember(IsRequired:=True)>
    Public Property FormatVersion As Integer = 3
    <DataMember(IsRequired:=True)>
    Public Property SavedAtUtc As DateTime
    <DataMember(IsRequired:=True)>
    Public Property BoxName As String
    <DataMember(IsRequired:=True)>
    Public Property StartDelay As String
    <DataMember(IsRequired:=True)>
    Public Property PostSession As String
    <DataMember(IsRequired:=True)>
    Public Property ICIDuration As String
    <DataMember(IsRequired:=True)>
    Public Property ICIEnabled As Boolean
    <DataMember(IsRequired:=True)>
    Public Property ICIRetractInputs As Boolean
    <DataMember(IsRequired:=True)>
    Public Property ICIStimulus As String
    <DataMember(IsRequired:=True)>
    Public Property RandomPresentation As Boolean
    <DataMember(IsRequired:=True)>
    Public Property ComponentOrder As String
    <DataMember(Name:="LeverLetters", EmitDefaultValue:=False)>
    Public Property LegacyLeverLetters As String
    <DataMember(EmitDefaultValue:=False)>
    Public Property CommandMap As ArduinoCommandMap
    <DataMember(IsRequired:=True)>
    Public Property ComponentCount As Integer
    <DataMember(IsRequired:=True)>
    Public Property Components As ComponentBlueprint()

    Public Sub Validate()
        'Version 1 files may contain per-box details; those members are ignored on load.
        If FormatVersion < 1 OrElse FormatVersion > 3 Then Throw New InvalidDataException("This setup file uses an unsupported version.")
        For Each field As PropertyInfo In GetType(SetupSaveData).GetProperties()
            If field.Name = NameOf(LegacyLeverLetters) Then Continue For
            If field.PropertyType Is GetType(String) AndAlso field.GetValue(Me) Is Nothing Then
                Throw New InvalidDataException("Missing setup value: " & field.Name & ".")
            End If
        Next
        If FormatVersion < 3 Then CommandMap = ArduinoCommandMap.FromLegacyLevers(LegacyLeverLetters)
        If CommandMap Is Nothing Then Throw New InvalidDataException("Missing Arduino command map.")
        Dim commandError As String = CommandMap.ValidationError()
        If commandError <> "" Then Throw New InvalidDataException(commandError)
        If ComponentCount < 0 OrElse ComponentCount > MAX_COMPONENTS OrElse
            Components Is Nothing OrElse Components.Length <> MAX_COMPONENTS + 1 Then
            Throw New InvalidDataException("Unsupported component count.")
        End If
        For componentIndex As Integer = 1 To ComponentCount
            Dim component As ComponentBlueprint = Components(componentIndex)
            If component.InputCount < 1 OrElse component.InputCount > MAX_INPUTS Then
                Throw New InvalidDataException("Invalid input count in component " & componentIndex & ".")
            End If
            For Each field As FieldInfo In GetType(ComponentBlueprint).GetFields(BindingFlags.Instance Or BindingFlags.Public)
                If Not field.FieldType.IsArray OrElse field.Name = NameOf(ComponentBlueprint.ComponentDuration_measured) Then Continue For
                Dim values As Array = TryCast(field.GetValue(component), Array)
                If values Is Nothing OrElse values.Length <> MAX_INPUTS Then
                    Throw New InvalidDataException("Incomplete " & field.Name & " in component " & componentIndex & ".")
                End If
            Next
        Next
    End Sub

    Public Sub Save(filePath As String)
        Validate()
        Dim destination As String = Path.GetFullPath(filePath)
        Dim temporaryFile As String = destination & "." & Guid.NewGuid().ToString("N") & ".tmp"
        Try
            Dim settings As New XmlWriterSettings With {.Indent = True, .Encoding = New UTF8Encoding(False)}
            Using stream As New FileStream(temporaryFile, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                Using writer As XmlWriter = XmlWriter.Create(stream, settings)
                    Dim serializer As New DataContractSerializer(GetType(SetupSaveData))
                    serializer.WriteObject(writer, Me)
                End Using
                stream.Flush(True)
            End Using
            'Replace only after the complete new file has been written successfully.
            If File.Exists(destination) Then
                File.Replace(temporaryFile, destination, Nothing)
            Else
                File.Move(temporaryFile, destination)
            End If
        Finally
            Try
                If File.Exists(temporaryFile) Then File.Delete(temporaryFile)
            Catch ex As IOException
            Catch ex As UnauthorizedAccessException
            End Try
        End Try
    End Sub

    Public Shared Function Read(filePath As String) As SetupSaveData
        Dim settings As New XmlReaderSettings With {
            .DtdProcessing = DtdProcessing.Prohibit, .XmlResolver = Nothing,
            .MaxCharactersInDocument = 4 * 1024 * 1024
        }
        Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using reader As XmlReader = XmlReader.Create(stream, settings)
                Dim result As SetupSaveData = DirectCast(New DataContractSerializer(GetType(SetupSaveData)).ReadObject(reader), SetupSaveData)
                If result Is Nothing Then Throw New InvalidDataException("Empty setup file.")
                result.Validate()
                Return result
            End Using
        End Using
    End Function
End Class
