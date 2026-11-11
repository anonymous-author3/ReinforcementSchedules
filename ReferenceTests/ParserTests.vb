Imports System
Imports System.IO
Imports System.Linq
Imports System.Collections.Generic
Module ParserTests
    Private checks As Integer
    Sub Check(v As Boolean, label As String)
        If Not v Then Throw New Exception(label)
        checks += 1
    End Sub
    Sub Reject(f As Action, label As String)
        Try
            f()
        Catch ex As InvalidDataException
            checks += 1
            Return
        End Try
        Throw New Exception("Accepted invalid data: " & label)
    End Sub
    Sub Main()
        Dim raw = String.Join(vbLf, {"""Session UID: 0123456789abcdef0123456789abcdef""", """ReferenceFormat"",2", "0,1,""StartComponent1""", "10,1,""LagSequence"",1,""12"",""Pass"",0,5", "15,1,""DeliveredReinforcer"",1,2,""Pellet"",1,""Scheduled""", "15,1,""LagOutcome"",1,1,""Delivered""", "20,1,""LagSequence"",2,""12"",""Fail"",1,5", "20,1,""LagOutcome"",2,0,""Omitted""", "30,2,""StartComponent2""", "40,2,""DeliveredReinforcer"",2,0,""Liquid"",0,""Time""", "50,1,""StartComponent1""", "60,1,""LagSequence"",3,""21"",""Pass"",2,5", "61,1,""LagOutcome"",3,0,""Cancelled:ComponentEnd"""})
        raw &= vbLf & """ReferenceClosed"",""Completed"""
        Dim result = SessionReferenceData.ParseRaw(raw, """Session UID: 0123456789abcdef0123456789abcdef""" & vbLf & """Subject: Rat 1""" & vbLf & """Session: Test""")
        Check(result.HasExactDeliveries AndAlso result.HasExactOutcomes, "exact schema")
        Check(result.Subject = "Rat 1" AndAlso result.SessionName = "Test", "metadata")
        Check(result.Deliveries(1).ComponentTimeMs = 10 AndAlso result.Deliveries(1).Source = "Time", "component time/source")
        Dim selected = result.SelectComponent(1)
        Check(selected.Sequences.Count = 3 AndAlso selected.Deliveries.Count = 1, "component filtering")
        Check(String.Join(",", selected.LastSequences(2)) = "12,21", "history ordered including failed")
        Check(selected.ValidateHistory(2, {1, 2}) = "", "valid history")
        Check(selected.ValidateHistory(3, {1, 2}) <> "", "length mismatch")
        Check(selected.ValidateHistory(2, {1, 6}) <> "", "input mismatch")
        Check(selected.Sequences(2).DeliveredCount.GetValueOrDefault(-1) = 0 AndAlso selected.Sequences(2).Outcome.StartsWith("Cancelled:"), "cancelled trial")
        selected.Sequences(0).Value = "66"
        Check(result.Sequences(0).Value = "12", "deep copy")
        Check(result.ComponentNumbers().SequenceEqual({1, 2}), "component list")
        Dim old = SessionReferenceData.ParseRaw(raw.Replace("""ReferenceFormat"",2", ""), "")
        Check(Not old.HasExactDeliveries AndAlso Not old.HasExactOutcomes, "old never exact")
        Reject(Sub() SessionReferenceData.ParseRaw(raw, """Session UID: other"""), "uid mismatch")
        Reject(Sub() SessionReferenceData.ParseRaw(raw.Replace("1,1,""Delivered""", "1,2,""Delivered"""), ""), "count mismatch")
        Reject(Sub() SessionReferenceData.ParseRaw(raw.Replace("1,""Scheduled""", "1,""Manual"""), ""), "manual trial")
        Reject(Sub() SessionReferenceData.ParseRaw(raw.Replace("60,1,""LagSequence""", "4,1,""LagSequence"""), ""), "time backwards")
        Reject(Sub() SessionReferenceData.ParseRaw(raw.Replace("""12""", """19"""), ""), "bad input")
        Reject(Sub() SessionReferenceData.ParseRaw(raw & vbLf & "70,1,""LagOutcome"",3,0,""Omitted""", ""), "duplicate outcome")
        Dim incomplete = SessionReferenceData.ParseRaw(raw.Replace("61,1,""LagOutcome"",3,0,""Cancelled:ComponentEnd""", ""), "")
        Check(Not incomplete.HasExactOutcomes, "no missing-outcome inference")
        Dim header = "RecordType,Order,TimeMs,Component,ComponentName,Presentation,SequenceNumber,Sequence,Criterion,ComparedCount,Lag,Frequency,Passed,Failed,Reason,SessionUID,Box,Subject,SessionName,SequenceText"
        Dim csv = header & vbLf & "Sequence,1,10,1,A,1,1,12,Pass,0,5,,,,,0123456789abcdef0123456789abcdef,Box,Rat 1,Test,Inputs: 12" & vbLf & "Partial,2,11,1,A,1,,1,Incomplete,,,,,,End,0123456789abcdef0123456789abcdef,Box,Rat 1,Test,Inputs: 1"
        Dim c = SessionReferenceData.ParseLagCsv(csv)
        Check(c.Sequences.Count = 1 AndAlso Not c.HasExactOutcomes, "CSV complete only, no delivery claims")
        Dim batched = raw.Replace("""Scheduled""", """Scheduled"",1").Replace("""Time""", """Time"",2")
        Dim b = SessionReferenceData.ParseRaw(batched, "").SelectComponent(1)
        Check(b.ValidateYoked(True) = "" AndAlso b.ValidateYoked(False) <> "", "Lag exact; Classic repeated presentation rejected")
        Check(SessionReferenceData.ParseRaw(raw, "").SelectComponent(1).ValidateYoked(False) <> "", "old batches rejected for Classic")
        Check(SessionReferenceData.ParseRaw(batched, "").SelectComponent(2).ValidateYoked(False) <> "", "FT VT no operando")
        Dim cancelled = SessionReferenceData.ParseRaw(batched.Replace("1,1,""Delivered""", "1,1,""Cancelled:SessionStop"""), "").SelectComponent(1)
        Check(cancelled.ValidateHistory(2, {1, 2}) = "" AndAlso cancelled.ValidateYoked(True) <> "", "cancelled partial delivery history only")
        b.Deliveries(0).Count = 2
        Check(b.ValidateIntegrity() <> "", "tampered count")
        b.Deliveries = Nothing
        Check(b.ValidateIntegrity() <> "", "null list")
        Dim classicRaw = String.Join(vbLf, {"""Session UID: 0123456789abcdef0123456789abcdef""", """ReferenceFormat"",2", "0,1,""StartComponent1""", "10,1,""DeliveredReinforcer"",1,1,""Pellet"",0,""Scheduled"",1", "20,1,""EndComponent1"""})
        classicRaw &= vbLf & """ReferenceClosed"",""Completed"""
        Dim classic = SessionReferenceData.ParseRaw(classicRaw, "").SelectComponent(1)
        Check(classic.ValidateYoked(False) = "" AndAlso classic.Presentations.Count = 1 AndAlso classic.Presentations(0).EndTimeMs.GetValueOrDefault() = 20, "single Classic presentation with end")
        Dim withEmpty = SessionReferenceData.ParseRaw(classicRaw & vbLf & "30,1,""StartComponent1""" & vbLf & "40,1,""EndComponent1""" & vbLf & "50,3,""StartComponent3""" & vbLf & "60,3,""EndComponent3""", "")
        Check(withEmpty.SelectComponent(1).Presentations.Count = 2 AndAlso withEmpty.SelectComponent(1).ValidateYoked(False) <> "", "empty repeated presentation retained and rejected")
        Check(withEmpty.ComponentNumbers().SequenceEqual({1, 3}), "empty component listed")
        Check(withEmpty.SelectComponent(3).Presentations.Count = 1 AndAlso withEmpty.SelectComponent(3).Deliveries.Count = 0 AndAlso withEmpty.SelectComponent(3).ValidateYoked(False) <> "", "empty component not yoked")
        Dim cloned = classic.Copy()
        cloned.Presentations(0).StartTimeMs = 11
        Check(classic.Presentations(0).StartTimeMs = 0 AndAlso cloned.ValidateIntegrity() <> "", "presentation deep copy and bound check")
        Reject(Sub() SessionReferenceData.ParseRaw(classicRaw & vbLf & "30,1,""EndComponent1""", ""), "duplicate presentation end")
        Check(c.SelectComponent(1).ValidateHistory(2, {1, 2}) = "", "legacy CSV history without boundary metadata")
        Dim openRaw = classicRaw.Replace("""ReferenceClosed"",""Completed""", "")
        Check(SessionReferenceData.ParseRaw(openRaw, "").SelectComponent(1).ValidateYoked(False) <> "", "unclosed reference rejected")
        Check(SessionReferenceData.ParseRaw(classicRaw.Replace("""Completed""", """Error"""), "").SelectComponent(1).ValidateYoked(False) <> "", "error reference rejected")
        Check(SessionReferenceData.ParseRaw(classicRaw.Replace("""Completed""", """Disposed"""), "").SelectComponent(1).ValidateYoked(False) <> "", "disposed reference rejected")
        Reject(Sub() SessionReferenceData.ParseRaw(classicRaw & vbLf & """ReferenceClosed"",""Error""", ""), "conflicting closure")
        Check(SessionReferenceData.ParseRaw(raw.Replace("""Completed""", """Error"""), "").SelectComponent(1).ValidateHistory(2, {1, 2}) = "", "complete sequences in failed session still valid history")
        Console.WriteLine("PASS " & checks & " parser assertions")
    End Sub
End Module




