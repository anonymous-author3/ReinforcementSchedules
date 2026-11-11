Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.FileIO

'Build the extra report from the authoritative raw log, without retaining every sequence in memory.
Friend NotInheritable Class LagSequenceReport
    Private Sub New()
    End Sub

    Private NotInheritable Class Frequency
        Public Count As Long
        Public Passed As Long
        Public Failed As Long
    End Class

    Public Shared Sub Save(rawPath As String, reportPath As String, settings As ChamberConfiguration, sessionId As String)
        Dim temporaryPath As String = reportPath & "." & Guid.NewGuid().ToString("N") & ".tmp"
        Try
            Using source As New StreamReader(rawPath), destination As New StreamWriter(temporaryPath, False, New UTF8Encoding(True))
                Write(source, destination, settings, sessionId)
            End Using
            'Publish only a complete report. Never overwrite an existing session report.
            File.Move(temporaryPath, reportPath)
        Catch
            Try
                If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
            Catch
            End Try
            Throw
        End Try
    End Sub

    Public Shared Sub Write(source As TextReader, destination As TextWriter, settings As ChamberConfiguration, sessionId As String)
        Dim total As New Dictionary(Of String, Frequency)(StringComparer.Ordinal)
        Dim perComponent As New Dictionary(Of Integer, Dictionary(Of String, Frequency))
        Dim presentations As New Dictionary(Of Integer, Integer)
        Dim order As Long
        Dim completed As Long
        Dim partials As Long
        WriteCsv(destination, {"RecordType", "Order", "TimeMs", "Component", "ComponentName", "Presentation", "SequenceNumber", "Sequence", "Criterion", "ComparedCount", "Lag", "Frequency", "Passed", "Failed", "Reason", "SessionUID", "Box", "Subject", "SessionName", "SequenceText"})
        WriteRow(destination, settings, sessionId, "Note", Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing,
            "Sequence rows are complete sequences in raw-log order; Partial rows are incomplete and excluded from frequencies. Pass/Fail describes the Lag criterion, not reinforcer delivery. Frequency ties use ascending sequence order. Import Sequence as Text in spreadsheets, or use SequenceText to preserve long sequences. Text resembling a spreadsheet formula is prefixed with an apostrophe; raw data is unchanged.")
        For component As Integer = 1 To settings.ComponentCount
            If Not settings.Components(component).LagEnabled Then Continue For
            perComponent.Add(component, New Dictionary(Of String, Frequency)(StringComparer.Ordinal))
            presentations.Add(component, 0)
            WriteRow(destination, settings, sessionId, "Component", Nothing, Nothing, component, settings.Components(component).ComponentName, Nothing, Nothing, Nothing, Nothing, Nothing,
                settings.Components(component).LagValue, Nothing, Nothing, Nothing, "Responses per sequence: " & settings.Components(component).LagSequenceLength.ToString(CultureInfo.InvariantCulture))
        Next
        Using parser As New TextFieldParser(source)
            parser.TextFieldType = FieldType.Delimited
            parser.SetDelimiters(",")
            parser.HasFieldsEnclosedInQuotes = True
            parser.TrimWhiteSpace = False
            While Not parser.EndOfData
                Dim fields() As String = parser.ReadFields()
                If fields Is Nothing OrElse fields.Length < 3 Then Continue While
                Dim component As Integer
                If Not Integer.TryParse(fields(1), NumberStyles.Integer, CultureInfo.InvariantCulture, component) OrElse Not perComponent.ContainsKey(component) Then Continue While
                Dim code As String = fields(2)
                If code = "StartComponent" & component.ToString(CultureInfo.InvariantCulture) Then
                    presentations(component) += 1
                    Continue While
                End If
                If code <> "LagSequence" AndAlso code <> "LagPartialDiscarded" Then Continue While
                Dim time As Long
                If Not Long.TryParse(fields(0), NumberStyles.Integer, CultureInfo.InvariantCulture, time) Then Throw New InvalidDataException("Invalid Lag event time in raw data.")
                Dim componentName As String = settings.Components(component).ComponentName
                order += 1
                If code = "LagSequence" Then
                    Dim sequenceNumber As Long
                    Dim compared As Integer
                    Dim lag As Integer
                    If fields.Length <> 8 OrElse Not Long.TryParse(fields(3), NumberStyles.Integer, CultureInfo.InvariantCulture, sequenceNumber) OrElse
                        Not Integer.TryParse(fields(6), NumberStyles.Integer, CultureInfo.InvariantCulture, compared) OrElse
                        Not Integer.TryParse(fields(7), NumberStyles.Integer, CultureInfo.InvariantCulture, lag) OrElse
                        (fields(5) <> "Pass" AndAlso fields(5) <> "Fail") Then Throw New InvalidDataException("Invalid completed Lag sequence in raw data.")
                    ValidateSequence(fields(4), settings.Components(component).LagSequenceLength, True)
                    completed += 1
                    WriteRow(destination, settings, sessionId, "Sequence", order, time, component, componentName, presentations(component), sequenceNumber, fields(4), fields(5), compared, lag)
                    Count(total, fields(4), fields(5) = "Pass")
                    Count(perComponent(component), fields(4), fields(5) = "Pass")
                Else
                    If fields.Length <> 5 Then Throw New InvalidDataException("Invalid partial Lag sequence in raw data.")
                    ValidateSequence(fields(3), settings.Components(component).LagSequenceLength, False)
                    partials += 1
                    WriteRow(destination, settings, sessionId, "Partial", order, time, component, componentName, presentations(component), Nothing, fields(3), "Incomplete", Nothing,
                        settings.Components(component).LagValue, Nothing, Nothing, Nothing, fields(4))
                End If
            End While
        End Using
        WriteFrequencies(destination, settings, sessionId, "FrequencyAll", Nothing, total)
        For Each component As Integer In perComponent.Keys.OrderBy(Function(value) value)
            WriteFrequencies(destination, settings, sessionId, "FrequencyComponent", component, perComponent(component))
        Next
        WriteRow(destination, settings, sessionId, "Totals", Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, completed,
            total.Values.Sum(Function(value) value.Passed), total.Values.Sum(Function(value) value.Failed), "Incomplete sequences excluded: " & partials.ToString(CultureInfo.InvariantCulture))
    End Sub

    Private Shared Sub ValidateSequence(sequence As String, length As Integer, complete As Boolean)
        If String.IsNullOrEmpty(sequence) OrElse sequence.Any(Function(value) value < "1"c OrElse value > "6"c) OrElse
            (complete AndAlso sequence.Length <> length) OrElse (Not complete AndAlso sequence.Length >= length) Then
            Throw New InvalidDataException("Invalid physical-input sequence in raw data.")
        End If
    End Sub

    Private Shared Sub Count(frequencies As Dictionary(Of String, Frequency), sequence As String, passed As Boolean)
        Dim item As Frequency = Nothing
        If Not frequencies.TryGetValue(sequence, item) Then
            item = New Frequency()
            frequencies.Add(sequence, item)
        End If
        item.Count += 1
        If passed Then
            item.Passed += 1
        Else
            item.Failed += 1
        End If
    End Sub

    Private Shared Sub WriteFrequencies(destination As TextWriter, settings As ChamberConfiguration, sessionId As String, kind As String,
                                        component As Integer?, frequencies As Dictionary(Of String, Frequency))
        For Each item In frequencies.OrderByDescending(Function(value) value.Value.Count).ThenBy(Function(value) value.Key, StringComparer.Ordinal)
            WriteRow(destination, settings, sessionId, kind, Nothing, Nothing, component,
                If(component.HasValue, settings.Components(component.GetValueOrDefault()).ComponentName, Nothing), Nothing, Nothing, item.Key, Nothing, Nothing,
                If(component.HasValue, CType(settings.Components(component.GetValueOrDefault()).LagValue, Object), Nothing), item.Value.Count, item.Value.Passed, item.Value.Failed)
        Next
    End Sub

    Private Shared Sub WriteRow(destination As TextWriter, settings As ChamberConfiguration, sessionId As String, kind As String, ParamArray details() As Object)
        Dim row(19) As Object
        row(0) = kind
        Array.Copy(details, 0, row, 1, details.Length)
        row(15) = sessionId
        row(16) = settings.BoxName
        row(17) = settings.Subject
        row(18) = settings.SessionName
        If row(7) IsNot Nothing Then row(19) = "Inputs: " & CStr(row(7))
        WriteCsv(destination, row)
    End Sub

    Private Shared Sub WriteCsv(destination As TextWriter, values() As Object)
        destination.WriteLine(String.Join(",", values.Select(Function(value) CsvValue(value))))
    End Sub

    Private Shared Function CsvValue(value As Object) As String
        If value Is Nothing Then Return ""
        Dim text As String = Convert.ToString(value, CultureInfo.InvariantCulture)
        Dim trimmed As String = text.TrimStart()
        If trimmed.Length > 0 AndAlso "=+-@".Contains(trimmed.Substring(0, 1)) Then text = "'" & text
        Return """" & text.Replace("""", """""") & """"
    End Function
End Class