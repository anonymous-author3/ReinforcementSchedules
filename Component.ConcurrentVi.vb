Imports System.Drawing
Imports System.Windows.Forms
Partial Public Class Component
    Private concurrentViButton As Button
    Private Sub InitializeConcurrentViCalculator()
        If concurrentViButton IsNot Nothing Then Return
        concurrentViButton = New Button With {.Name = "btnConcurrentViCalculator", .Text = "Concurrent VI Calculator...", .Location = New Point(338, 388), .Size = New Size(215, 30)}
        Controls.Add(concurrentViButton)
        AddHandler concurrentViButton.Click, AddressOf OpenConcurrentViCalculator
        ComponentToolTip.SetToolTip(concurrentViButton, "Calculate VI means for two to six concurrent alternatives from a global rate and ratio of programmed reinforcement rates. Planning only; does not change inputs.")
        UpdateConcurrentViCalculator()
    End Sub
    Private Sub UpdateConcurrentViCalculator()
        If concurrentViButton IsNot Nothing Then concurrentViButton.Visible = Not IsTScheduleMode() AndAlso Not IsLagScheduleMode() AndAlso Not IsBreakpointScheduleMode()
    End Sub
    Private Sub OpenConcurrentViCalculator(sender As Object, e As EventArgs)
        Using calculator As New ConcurrentViCalculator()
            calculator.ShowDialog(Me)
        End Using
    End Sub
End Class