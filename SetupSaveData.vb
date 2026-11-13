Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Xml
Imports System.Web.Script.Serialization

'A saved editor configuration is separate from a running chamber's state.
<DataContract(Name:="ReinforcementSchedulesSetup", [Namespace]:="urn:reinforcement-schedules:setup")>
Friend Class SetupSaveData
    <DataMember(IsRequired:=True)>
    Public Property FormatVersion As Integer = 10
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
        If FormatVersion < 1 OrElse FormatVersion > 10 Then Throw New InvalidDataException("This setup file uses an unsupported version.")
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
            IntervalScheduleTiming.NormalizeLegacyCounts(component)
            ClassicChainConfiguration.NormalizeLegacy(component)
            BreakpointConfiguration.NormalizeLegacy(component)
            ReinforcerConfiguration.NormalizeLegacy(component)
            Components(componentIndex) = component
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
            Dim reinforcerError = ReinforcerConfiguration.ValidationError(component)
            If reinforcerError <> "" Then Throw New InvalidDataException("Component " & componentIndex & ": " & reinforcerError)
            Dim breakpointError = BreakpointConfiguration.ValidationError(component)
            If breakpointError <> "" Then Throw New InvalidDataException("Component " & componentIndex & ": " & breakpointError)
            Dim chainError = ClassicChainConfiguration.ValidationError(component)
            If chainError <> "" Then Throw New InvalidDataException("Component " & componentIndex & ": " & chainError)
            Dim intervalError As String = IntervalScheduleTiming.ValidationError(component)
            If intervalError <> "" Then Throw New InvalidDataException("Component " & componentIndex & ": " & intervalError)
            Dim referenceError As String = ReferenceConfiguration.ValidationError(component)
            If referenceError <> "" Then Throw New InvalidDataException("Component " & componentIndex & ": " & referenceError)
            Dim lagError As String = LagConfiguration.ValidationError(component)
            If lagError <> "" Then Throw New InvalidDataException("Component " & componentIndex & ": " & lagError)
        Next
    End Sub

    Private Const MaximumBytes As Integer = 4 * 1024 * 1024

    'JSON is the canonical format for new saves. XML remains an import format.
    Public Function ToJson() As String
        Validate()
        Dim json As String = New JavaScriptSerializer With {.MaxJsonLength = MaximumBytes}.Serialize(Me)
        If Encoding.UTF8.GetByteCount(json) > MaximumBytes Then Throw New InvalidDataException("This setup and its reference snapshots exceed the 4 MB setup limit.")
        Return json
    End Function

    Public Sub Save(filePath As String)
        Dim destination As String = Path.GetFullPath(filePath)
        StorageJson.WriteAtomically(destination, ToIndentedJson())
    End Sub

    Public Function ToIndentedJson() As String
        'Whitespace only: preserve number/date tokens and escapes exactly.
        Dim compact As String = ToJson()
        Dim result As New StringBuilder()
        Dim depth As Integer = 0
        Dim inString As Boolean = False
        Dim escaped As Boolean = False
        For index As Integer = 0 To compact.Length - 1
            Dim character = compact(index)
            If inString Then
                result.Append(character)
                If escaped Then
                    escaped = False
                ElseIf character = "\"c Then
                    escaped = True
                ElseIf character = """"c Then
                    inString = False
                End If
                Continue For
            End If
            Select Case character
                Case """"c
                    inString = True
                    result.Append(character)
                Case "{"c, "["c
                    result.Append(character)
                    depth += 1
                    If index + 1 < compact.Length AndAlso compact(index + 1) <> "}"c AndAlso compact(index + 1) <> "]"c Then
                        result.AppendLine().Append(New String(" "c, depth * 2))
                    End If
                Case "}"c, "]"c
                    depth -= 1
                    If index > 0 AndAlso compact(index - 1) <> "{"c AndAlso compact(index - 1) <> "["c Then
                        result.AppendLine().Append(New String(" "c, depth * 2))
                    End If
                    result.Append(character)
                Case ","c
                    result.Append(character).AppendLine().Append(New String(" "c, depth * 2))
                Case ":"c
                    result.Append(": ")
                Case Else
                    If Not Char.IsWhiteSpace(character) Then result.Append(character)
            End Select
        Next
        result.AppendLine()
        Dim json = result.ToString()
        If Encoding.UTF8.GetByteCount(json) > MaximumBytes Then Throw New InvalidDataException("This formatted setup exceeds the 4 MB setup limit. Choose a smaller reference session.")
        Return json
    End Function

    Public Shared Function Read(filePath As String) As SetupSaveData
        If New FileInfo(filePath).Length > MaximumBytes Then Throw New InvalidDataException("Invalid setup size.")
        Return ReadText(File.ReadAllText(filePath, Encoding.UTF8))
    End Function

    Public Shared Function ReadText(content As String) As SetupSaveData
        If content Is Nothing OrElse Encoding.UTF8.GetByteCount(content) > MaximumBytes Then Throw New InvalidDataException("Invalid setup size.")
        Dim trimmed As String = content.TrimStart(ChrW(&HFEFF), " "c, ChrW(9), ChrW(10), ChrW(13))
        If trimmed.StartsWith("{", StringComparison.Ordinal) Then Return ReadJson(trimmed)
        Return ReadXml(trimmed)
    End Function

    Public Shared Function ReadJson(json As String) As SetupSaveData
        If json Is Nothing OrElse Encoding.UTF8.GetByteCount(json) > MaximumBytes Then Throw New InvalidDataException("Invalid setup size.")
        Dim serializer As New JavaScriptSerializer With {.MaxJsonLength = MaximumBytes}
        Dim fields = serializer.Deserialize(Of Dictionary(Of String, Object))(json)
        If fields Is Nothing Then Throw New InvalidDataException("Empty setup file.")
        'Do not silently replace a missing required setting with a default value.
        For Each prop In GetType(SetupSaveData).GetProperties()
            Dim member = DirectCast(Attribute.GetCustomAttribute(prop, GetType(DataMemberAttribute)), DataMemberAttribute)
            If member IsNot Nothing AndAlso member.IsRequired AndAlso Not fields.ContainsKey(prop.Name) Then
                Throw New InvalidDataException("Missing setup value: " & prop.Name & ".")
            End If
        Next
        Dim rawComponents = TryCast(fields("Components"), System.Collections.IList)
        Dim count As Integer = Convert.ToInt32(fields("ComponentCount"))
        If rawComponents Is Nothing OrElse rawComponents.Count <> MAX_COMPONENTS + 1 OrElse count < 0 OrElse count > MAX_COMPONENTS Then Throw New InvalidDataException("Unsupported component count.")
        For index As Integer = 1 To count
            Dim componentFields = TryCast(rawComponents(index), Dictionary(Of String, Object))
            If componentFields Is Nothing Then Throw New InvalidDataException("Missing component " & index & ".")
            If Convert.ToInt32(fields("FormatVersion")) >= 7 AndAlso
                (Not componentFields.ContainsKey("VariableValueCount") OrElse componentFields("VariableValueCount") Is Nothing OrElse
                 Not componentFields.ContainsKey("TimeVariableValueCount") OrElse Convert.ToInt32(componentFields("TimeVariableValueCount")) < 1) Then
                Throw New InvalidDataException("Missing variable series counts in component " & index & ".")
            End If
            If Convert.ToInt32(fields("FormatVersion")) >= 8 AndAlso (Not componentFields.ContainsKey("ClassicChains") OrElse componentFields("ClassicChains") Is Nothing) Then
                Throw New InvalidDataException("Missing classic chain settings in component " & index & ".")
            End If
            If Convert.ToInt32(fields("FormatVersion")) >= 9 AndAlso (Not componentFields.ContainsKey("BreakpointSchedules") OrElse componentFields("BreakpointSchedules") Is Nothing) Then
                Throw New InvalidDataException("Missing breakpoint settings in component " & index & ".")
            End If
            If Convert.ToInt32(fields("FormatVersion")) >= 10 AndAlso (Not componentFields.ContainsKey("ReinforcerSelections") OrElse componentFields("ReinforcerSelections") Is Nothing) Then
                Throw New InvalidDataException("Missing reinforcer selections in component " & index & ".")
            End If
            If componentFields.ContainsKey("ReinforcerSelections") AndAlso componentFields("ReinforcerSelections") IsNot Nothing Then
                Dim selections = TryCast(componentFields("ReinforcerSelections"), System.Collections.IList)
                If selections Is Nothing OrElse selections.Count <> MAX_INPUTS Then Throw New InvalidDataException("Incomplete reinforcer selections.")
                For Each selection In selections
                    ValidateRawReinforcerSelection(selection)
                Next
            End If
            If componentFields.ContainsKey("TimeReinforcerSelection") Then ValidateRawReinforcerSelection(componentFields("TimeReinforcerSelection"))
            If componentFields.ContainsKey("BreakpointSchedules") AndAlso componentFields("BreakpointSchedules") IsNot Nothing Then
                Dim definitions = TryCast(componentFields("BreakpointSchedules"), System.Collections.IList)
                If definitions Is Nothing OrElse definitions.Count <> MAX_INPUTS Then Throw New InvalidDataException("Incomplete breakpoint settings.")
                For Each rawDefinition In definitions
                    If rawDefinition Is Nothing Then Continue For
                    Dim definition = TryCast(rawDefinition, Dictionary(Of String, Object))
                    If definition Is Nothing Then Throw New InvalidDataException("Invalid breakpoint settings.")
                    For Each name In {"Progression", "StartRequirement", "StepValue"}
                        If Not definition.ContainsKey(name) OrElse definition(name) Is Nothing Then Throw New InvalidDataException("Missing breakpoint value: " & name)
                    Next
                Next
            End If
            If componentFields.ContainsKey("ClassicChains") AndAlso componentFields("ClassicChains") IsNot Nothing Then
                Dim chains = TryCast(componentFields("ClassicChains"), System.Collections.IList)
                If chains Is Nothing OrElse chains.Count <> MAX_INPUTS Then Throw New InvalidDataException("Incomplete classic chains in component " & index)
                For Each rawChain In chains
                    If rawChain Is Nothing Then Continue For
                    Dim chain = TryCast(rawChain, Dictionary(Of String, Object))
                    If chain Is Nothing OrElse Not chain.ContainsKey("Links") Then Throw New InvalidDataException("Missing chain links.")
                    Dim links = TryCast(chain("Links"), System.Collections.IList)
                    If links Is Nothing OrElse links.Count > 100 Then Throw New InvalidDataException("Invalid chain links.")
                    For Each rawLink In links
                        Dim link = TryCast(rawLink, Dictionary(Of String, Object))
                        If link Is Nothing Then Throw New InvalidDataException("Missing chain link.")
                        For Each name In {"ScheduleType", "Value", "VariableValueCount", "StimulusType", "LightIntermittency", "ToneIntermittency", "RetractInput"}
                            If Not link.ContainsKey(name) OrElse link(name) Is Nothing Then Throw New InvalidDataException("Missing chain link value: " & name)
                        Next
                    Next
                Next
            End If
            For Each field In GetType(ComponentBlueprint).GetFields(BindingFlags.Instance Or BindingFlags.Public)
                If field.FieldType.IsValueType AndAlso Not Attribute.IsDefined(field, GetType(OptionalFieldAttribute)) AndAlso
                    (Not componentFields.ContainsKey(field.Name) OrElse componentFields(field.Name) Is Nothing) Then
                    Throw New InvalidDataException("Missing component " & index & " value: " & field.Name & ".")
                End If
            Next
        Next
        Dim result = serializer.Deserialize(Of SetupSaveData)(json)
        result.Validate()
        Return result
    End Function

    Private Shared Sub ValidateRawReinforcerSelection(raw As Object)
        If raw Is Nothing Then Return
        Dim fields = TryCast(raw, Dictionary(Of String, Object))
        If fields Is Nothing Then Throw New InvalidDataException("Invalid reinforcer selection.")
        For Each name In {"Feeder1Percent", "Pump1Percent", "Feeder2Percent", "Pump2Percent"}
            If Not fields.ContainsKey(name) OrElse fields(name) Is Nothing Then Throw New InvalidDataException("Missing reinforcer probability: " & name)
            Dim value = Convert.ToDecimal(fields(name), Globalization.CultureInfo.InvariantCulture)
            If value <> Decimal.Truncate(value) OrElse value < 0 OrElse value > 100 Then Throw New InvalidDataException("Reinforcer probabilities must be whole percentages from 0 to 100.")
        Next
    End Sub

    Public Shared Function ReadXml(xml As String) As SetupSaveData
        If xml Is Nothing OrElse Encoding.UTF8.GetByteCount(xml) > MaximumBytes Then Throw New InvalidDataException("Invalid setup size.")
        Using stream As New MemoryStream(Encoding.UTF8.GetBytes(xml))
            Dim settings As New XmlReaderSettings With {
                .DtdProcessing = DtdProcessing.Prohibit, .XmlResolver = Nothing,
                .MaxCharactersInDocument = MaximumBytes
            }
            Using reader As XmlReader = XmlReader.Create(stream, settings)
                Dim result = DirectCast(New DataContractSerializer(GetType(SetupSaveData)).ReadObject(reader), SetupSaveData)
                If result Is Nothing Then Throw New InvalidDataException("Empty setup file.")
                result.Validate()
                Return result
            End Using
        End Using
    End Function
End Class
