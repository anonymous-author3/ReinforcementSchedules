$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$a=[Reflection.Assembly]::LoadFrom("$PSScriptRoot\..\bin\LagValidation\ReinforcementSchedules.exe")
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$module=$a.GetTypes() | Where-Object Name -eq Module1
$module.GetField('vCC',$flags).SetValue($null,[byte]1)
$f=[Activator]::CreateInstance(($a.GetTypes() | Where-Object FullName -eq 'ReinforcementSchedules.Component'))
$f.add_Shown({
 $f.Text='Lag feedback QA (isolated)'
 $mode=$f.GetType().GetProperty('cbbScheduleMode',$flags).GetValue($f,$null)
 $mode.Text='Lag Schedules'
})
[void]$f.ShowDialog()
$f.Dispose()
