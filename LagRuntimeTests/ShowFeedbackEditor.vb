Imports System.Reflection
Imports System.Windows.Forms
Module ShowFeedbackEditor
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        GetType(ReinforcementSchedules.Component).Assembly.GetType("ReinforcementSchedules.Module1").GetField("vCC", BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static).SetValue(Nothing, CByte(1))
        Using editor As New ReinforcementSchedules.Component
            AddHandler editor.Shown, Sub()
                editor.Text = "Lag feedback QA (isolated)"
                Dim flags = BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance
                DirectCast(editor.GetType().GetProperty("cbbScheduleMode", flags).GetValue(editor, Nothing), ComboBox).Text = "Lag Schedules"
            End Sub
            Application.Run(editor)
        End Using
    End Sub
End Module

