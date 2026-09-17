Imports System.IO
Imports System.Globalization

Partial Public Class ChamberSession
    Private ReadOnly configuration As ChamberConfiguration
    Private serialBuffer As String = ""
    Private rawWriter As StreamWriter
    Private summaryWriter As StreamWriter
    Private chartPath As String
    Private runtimeStarted As Boolean
    Private finalizing As Boolean
    Private currentStatus As String = "Ready"

    Public Sub New(settings As ChamberConfiguration)
        Me.New()
        configuration = settings.Copy()
        AC = configuration.Components
        MAXvCC = CByte(configuration.ComponentCount)
        vCC = 1
        RandomCPres = configuration.RandomPresentation
        For i As Integer = 1 To MAXvCC
            AC(i).IterationsLeft = AC(i).ComponentIteration
            ReDim AC(i).ComponentDuration_measured(AC(i).ComponentIteration)
        Next
        For i As Integer = 0 To MAX_INPUTS - 1
            Actual_Response(i) = "1"
            Previous_Response(i) = "1"
        Next
        lblBoxTitle.Text = configuration.BoxName & " · " & configuration.ComPort & " · " &
            configuration.Subject & " — Cumulative record"
        EnsureInputRuntime()
        RefreshMainTables("Ready")
    End Sub

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return runtimeStarted AndAlso Not SessionClosed
        End Get
    End Property

    Public ReadOnly Property HasEnded As Boolean
        Get
            Return SessionClosed
        End Get
    End Property

    Public ReadOnly Property StatusText As String
        Get
            Return currentStatus
        End Get
    End Property

    Public ReadOnly Property ElapsedSeconds As Integer
        Get
            Return Math.Max(0, SessionTimeSeconds)
        End Get
    End Property

    Public ReadOnly Property CurrentComponentText As String
        Get
            If Not runtimeStarted OrElse tmrStart.Enabled Then Return "—"
            Return vCC & " / " & MAXvCC & " · " & ComponentLabel(vCC)
        End Get
    End Property

    Public Function TotalResponses(inputIndex As Integer) As Integer
        Return chartResponse(inputIndex)
    End Function

    Public ReadOnly Property TotalReinforcers As Integer
        Get
            Dim total As Integer
            For componentIndex As Integer = 1 To MAXvCC
                total += TimeRefCount(componentIndex)
                For inputIndex As Integer = 0 To MAX_INPUTS - 1
                    total += RefCount(componentIndex, inputIndex)
                Next
            Next
            Return total
        End Get
    End Property

    Public Function StartSession() As Boolean
        If runtimeStarted OrElse SessionClosed Then Return False
        Try
            SessionUID = Guid.NewGuid().ToString("N")
            Dim fileStem As String = SafeFilePart(configuration.BoxName) & "_" & SafeFilePart(configuration.Subject) & "_" &
                SafeFilePart(configuration.SessionName) & "_" & Date.Now.ToString("yyyyMMdd_HHmmss") & "_" & SessionUID
            Directory.CreateDirectory("C:\Data\Raw")
            Directory.CreateDirectory("C:\Data\Summary")
            Directory.CreateDirectory("C:\Data\Charts")
            rawWriter = New StreamWriter(Path.Combine("C:\Data\Raw", fileStem & "_Raw.txt")) With {.AutoFlush = True}
            summaryWriter = New StreamWriter(Path.Combine("C:\Data\Summary", fileStem & "_Summary.txt")) With {.AutoFlush = True}
            chartPath = Path.Combine("C:\Data\Charts", fileStem & "_chart.png")
            WriteLine(1, "Session UID: " & SessionUID)
            WriteLine(2, "Session UID: " & SessionUID)
            WriteLine(2, "Box: " & configuration.BoxName)
            WriteLine(2, "Subject: " & configuration.Subject)
            WriteLine(2, "Session: " & configuration.SessionName)
            WriteLine(2, "Weight: " & configuration.Weight)
            For Each line As String In configuration.SnapshotLines
                WriteLine(2, line)
            Next
            WriteLine(2, "Event code legend:")
            For i As Integer = 0 To MAX_INPUTS - 1
                WriteLine(2, "Input " & (i + 1) & " response: " & (i + 1) & "; reinforcer: R" & (i + 1))
            Next
            If SessionClosed Then Return False
            Arduino = New IO.Ports.SerialPort(configuration.ComPort, 9600) With {.WriteTimeout = 100, .ReadTimeout = 100}
            Arduino.Open()
            runtimeStarted = True
            SendArduino("p")
            If SessionClosed Then Return False
            tmrStart.Interval = Math.Max(1, configuration.StartDelaySeconds * 1000)
            Countdown = Environment.TickCount + tmrStart.Interval
            tmrStart.Enabled = True
            acquisitionTimer.Start()
            RefreshMainTables("Starting")
            Return True
        Catch ex As Exception
            FailSession(ex)
            Return False
        End Try
    End Function

    Private Shared Function SafeFilePart(value As String) As String
        Return New String(value.Select(Function(ch) If(Path.GetInvalidFileNameChars().Contains(ch), "_"c, ch)).ToArray())
    End Function

    Private Sub UpdateSessionClock()
        If Not runtimeStarted OrElse SessionClosed Then Return
        vTimeNow = If(tmrStart.Enabled, Math.Max(0, Countdown - Environment.TickCount), Environment.TickCount - vTimeStart)
        SessionTimeSeconds = CInt(Math.Round(vTimeNow / 1000.0))
    End Sub

    Private Sub AcquisitionTick(sender As Object, e As EventArgs) Handles acquisitionTimer.Tick
        If SessionClosed Then Return
        Try
            UpdateSessionClock()
            If Arduino Is Nothing OrElse Not Arduino.IsOpen Then Throw New IOException("The COM connection was closed.")
            serialBuffer &= Arduino.ReadExisting()
            Dim processed As Integer = 0
            Dim newline As Integer = serialBuffer.IndexOf(vbLf, StringComparison.Ordinal)
            While newline >= 0 AndAlso processed < 256 AndAlso Not SessionClosed
                Dim line As String = serialBuffer.Substring(0, newline).Trim()
                serialBuffer = serialBuffer.Substring(newline + 1)
                Dim values() As String = line.Split(","c)
                If values.Length >= ActiveInputCount() AndAlso values.Length <= MAX_INPUTS AndAlso values.All(Function(value) value = "0" OrElse value = "1") Then
                    For inputIndex As Integer = 0 To values.Length - 1
                        Actual_Response(inputIndex) = values(inputIndex)
                        If values(inputIndex) = "0" AndAlso Previous_Response(inputIndex) = "1" AndAlso IsInputAvailable(inputIndex) Then
                            Response(inputIndex)
                        End If
                        Previous_Response(inputIndex) = values(inputIndex)
                    Next
                End If
                processed += 1
                newline = serialBuffer.IndexOf(vbLf, StringComparison.Ordinal)
            End While
            If serialBuffer.Length > 65536 Then Throw New IOException("The Arduino input stream is not providing complete response records.")
            If tmrStart.Enabled AndAlso SessionTimeSeconds <> LastDisplayedSecond Then
                LastDisplayedSecond = SessionTimeSeconds
                RefreshMainSessionTable("Starting in " & SessionTimeSeconds & " s")
            End If
            If SessionEnding AndAlso tmrPostSession.Enabled Then
                Dim remaining As Integer = Math.Max(0, CInt(Math.Ceiling((PostSessionEndTick - Environment.TickCount) / 1000.0)))
                If remaining <> LastPostSessionSecond Then
                    LastPostSessionSecond = remaining
                    RefreshMainSessionTable("Closing in " & remaining & " s")
                End If
            End If
        Catch ex As Exception
            FailSession(ex)
        End Try
    End Sub

    Private Sub WriteLine(fileNumber As Integer, ParamArray values() As Object)
        Dim writer As StreamWriter = If(fileNumber = 1, rawWriter, summaryWriter)
        If writer Is Nothing Then Return
        writer.WriteLine(String.Join(",", values.Select(Function(value) LogValue(value))))
    End Sub

    Private Shared Function LogValue(value As Object) As String
        If TypeOf value Is String Then Return """" & CStr(value).Replace("""", """""") & """"
        Return Convert.ToString(value, CultureInfo.InvariantCulture)
    End Function

    Private Sub FileClose(fileNumber As Integer)
        Dim writer As StreamWriter = If(fileNumber = 1, rawWriter, summaryWriter)
        If fileNumber = 1 Then
            rawWriter = Nothing
        Else
            summaryWriter = Nothing
        End If
        Try
            If writer IsNot Nothing Then writer.Dispose()
        Catch ex As Exception
            currentStatus = "Error closing data: " & ex.Message
        End Try
    End Sub

    Private Sub StopRuntimeTimers()
        acquisitionTimer.Stop()
        For Each timer As Timer In New Timer() {tmrChrt, tmrStart, tmrPostSession, tmrComponentDuration, tmrComponentStim,
            tmrComponentLightStim, tmrTimeSchedule, tmrICI, tmrCOD}
            timer.Stop()
        Next
        For Each group As Timer() In New Timer()() {ScheduleTimers, DelayTimers, DelaySignalTimers,
            DelaySignalStartTimers, FeedbackTimers, TPeriodTimers, TInterPeriodTimers}
            For Each timer As Timer In group
                If timer IsNot Nothing Then timer.Stop()
            Next
        Next
    End Sub

    Private Sub CloseConnection()
        If Arduino Is Nothing Then Return
        Dim connection As IO.Ports.SerialPort = Arduino
        Arduino = Nothing
        Try
            If connection.IsOpen Then connection.WriteLine(configuration.CommandMap.Encode("X"))
        Catch ex As Exception
            currentStatus = "Error: " & ex.Message
        End Try
        Try
            connection.Dispose()
        Catch ex As Exception
            currentStatus = "Error: " & ex.Message
        End Try
    End Sub

    Private Sub FailSession(ex As Exception)
        If SessionClosed AndAlso Not finalizing Then Return
        If runtimeStarted AndAlso tmrComponentDuration.Enabled Then
            AC(vCC).ComponentDuration_measured(AC(vCC).IterationsLeft) =
                Math.Max(0, SessionTimeSeconds - AC(vCC).ComponentDuration_measured(AC(vCC).IterationsLeft))
        End If
        SessionClosed = True
        SessionStarted = False
        StopRuntimeTimers()
        CloseConnection()
        currentStatus = "Error: " & ex.Message
        Try
            WriteLine(2, currentStatus)
            If runtimeStarted Then WriteSessionSummary()
        Catch
        Finally
            FileClose(1)
            FileClose(2)
        End Try
        UpdateManualControls()
    End Sub

    Public Sub RequestFinish()
        Try
            UpdateSessionClock()
            If IsRunning AndAlso Not SessionEnding Then StartPostSession()
        Catch ex As Exception
            FailSession(ex)
        End Try
    End Sub

    Public Sub StopAndSave()
        If Not IsRunning Then Return
        Try
            UpdateSessionClock()
            If Not SessionEnding Then StartPostSession()
            Finish()
        Catch ex As Exception
            FailSession(ex)
        End Try
    End Sub

    Private Sub DisposeRuntime(sender As Object, e As EventArgs) Handles MyBase.Disposed
        If configuration Is Nothing Then Return
        StopRuntimeTimers()
        CloseConnection()
        FileClose(1)
        FileClose(2)
        acquisitionTimer.Dispose()
        tmrChrt.Dispose()
        tmrComponentLightStim.Dispose()
        tmrTimeSchedule.Dispose()
        For Each group As Timer() In New Timer()() {ScheduleTimers, DelayTimers, DelaySignalTimers,
            DelaySignalStartTimers, FeedbackTimers, TPeriodTimers, TInterPeriodTimers}
            For Each timer As Timer In group
                If timer IsNot Nothing Then timer.Dispose()
            Next
        Next
    End Sub
End Class
