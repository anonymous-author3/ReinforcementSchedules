Imports System.IO.Ports
Imports System.Windows.Forms.DataVisualization.Charting

Public Class Tests
    Public Arduino As SerialPort
    Public blnLeverL
    Public blnLeverR
    Public blnLight
    Public blnPump
    Public blnFeeder
    Private WithEvents tmrChartTests As Timer = New Timer
    Public resp(2)
    Public chrtTime
    Private Sub tmrStart_Tick(sender As Object, e As EventArgs) Handles tmrStart.Tick
        tmrStart.Enabled = False
        Arduino = New SerialPort(SetUp.txtCOM.Text, 9600)
        Arduino.Open()
        Arduino.WriteLine("p")
        tmrChartTests.Interval = 1000
        tmrChartTests.Enabled = True

        Do
            Try
                If Arduino.BytesToRead > 0 Then
                    Actual_Response = Split(Arduino.ReadLine(), ",")
                End If

                If (Actual_Response(0) <> Previous_Response(0) And Actual_Response(0) <> 1) Then
                    TestResponse(0)
                End If
                If (Actual_Response(1) <> Previous_Response(1) And Actual_Response(1) <> 1) Then
                    TestResponse(1)
                End If
                If (Actual_Response(2) <> Previous_Response(2) And Actual_Response(2) <> 1) Then
                    TestResponse(2)
                End If
                If (Actual_Response(3) <> Previous_Response(3) And Actual_Response(3) <> 1) Then

                End If
                If (Actual_Response(4) <> Previous_Response(4) And Actual_Response(4) <> 1) Then

                End If
                Previous_Response(0) = Actual_Response(0)
                Previous_Response(1) = Actual_Response(1)
                Previous_Response(2) = Actual_Response(2)
                Previous_Response(3) = Actual_Response(3)
                Previous_Response(4) = Actual_Response(4)

            Catch ex As Exception
                If tmrChartTests.Enabled = False Then Exit Do
            End Try
            My.Application.DoEvents() 'This will enable the rest of the program to run while executing the code from above.
        Loop
        Me.Close()
    End Sub


    Private Sub TestResponse(x)
        resp(x) += 1
        Me.Text = resp(x)
    End Sub



    Private Sub btnFeeder_Click(sender As Object, e As EventArgs) Handles btnFeeder.Click
        sender.backColor = Color.Green


        If blnFeeder = False Then
            blnFeeder = True
            tmrFeeder.Enabled = True
        ElseIf blnFeeder = True Then
            blnFeeder = False
            tmrFeeder.Enabled = False
        End If


    End Sub
    Private Sub tmrFeeder_Tick(sender As Object, e As EventArgs) Handles tmrFeeder.Tick
        Arduino.WriteLine("R")
    End Sub
    Private Sub btnLeverLeft_Click(sender As Object, e As EventArgs) Handles btnLeverLeft.Click
        sender.backColor = Color.Green
        If blnLeverL = False Then
            blnLeverL = True
            Arduino.WriteLine("L")
        Else
            blnLeverL = False
            Arduino.WriteLine("l")
        End If
    End Sub



    Private Sub btnLeverRight_Click(sender As Object, e As EventArgs) Handles btnLeverRight.Click
        sender.backColor = Color.Green
        If blnLeverR = False Then
            blnLeverR = True
            Arduino.WriteLine("M")
        Else
            blnLeverR = False
            Arduino.WriteLine("m")
        End If
    End Sub

    Private Sub btnPumpOn_Click(sender As Object, e As EventArgs) Handles btnPumpOn.Click
        sender.backColor = Color.Green
        If blnPump = False Then
            blnPump = True
            Arduino.WriteLine("P")
        Else
            blnPump = False
            Arduino.WriteLine("p")
        End If
    End Sub

    Private Sub btnLights_Click(sender As Object, e As EventArgs) Handles btnLights.Click
        sender.backColor = Color.Green
        If blnLight = False Then
            blnLight = True
            Arduino.WriteLine("ABH")
        Else
            blnLight = False
            Arduino.WriteLine("abh")
        End If
    End Sub

    Private Sub btnTone_Click(sender As Object, e As EventArgs) Handles btnTone.Click
        sender.backColor = Color.Green
        Arduino.WriteLine("Z")
    End Sub



    Private Sub tmrChrt_Tick(sender As Object, e As EventArgs) Handles tmrChartTests.Tick

        chrtTime += 1
        Chart1.Series("Lever 1").Points.AddXY(chrtTime, resp(0))
        Chart1.Series("Lever 2").Points.AddXY(chrtTime, resp(1))
        Chart1.Series("Tray").Points.AddXY(chrtTime, resp(2))

    End Sub
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Arduino.WriteLine("abhtlm")
        Arduino.Close()
        tmrFeeder.Enabled = False
        tmrChartTests.Enabled = False

        'Me.Close()

    End Sub

End Class