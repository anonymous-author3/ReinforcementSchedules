Partial Public Class ChamberSession
    Private cloudDestination As CloudDestination
    Private rawPath As String
    Private summaryPath As String
    Private cloudQueued As Boolean

    Private Sub QueueFinishedSession()
        If cloudQueued OrElse cloudDestination Is Nothing Then Return
        Try
            CloudStorage.Current.QueueSession(cloudDestination, SessionUID, configuration.BoxName, configuration.Subject,
                configuration.SessionName, currentStatus, rawPath, summaryPath, chartPath)
            cloudQueued = True
            UploadFinishedSession()
        Catch ex As Exception
            currentStatus &= " · RTDB not queued: " & ex.Message
        End Try
    End Sub

    Private Async Sub UploadFinishedSession()
        Await CloudStorage.Current.SynchronizeAsync()
    End Sub
End Class
