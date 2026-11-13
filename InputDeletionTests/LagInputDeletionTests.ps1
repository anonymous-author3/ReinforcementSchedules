#Requires -PSEdition Desktop
# Run with Windows PowerShell: powershell.exe -NoProfile -STA -File .\InputDeletionTests\InputDeletionTests.ps1
# No serial ports, sessions, RTDB, or saved files are changed. UI stays offscreen.
param([string]$AssemblyPath = (Join-Path $PSScriptRoot '..\bin\Release\ReinforcementSchedules.exe'))
if ([Threading.Thread]::CurrentThread.ApartmentState -ne 'STA') { throw 'Run this test with Windows PowerShell -STA.' }
New-Item -ItemType Directory -Force -Path (Join-Path $env:TEMP 'soft-breakpoint-ui-qa') | Out-Null
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$asm=[Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
$flags=[Reflection.BindingFlags]'Instance,Public,NonPublic'
$stat=[Reflection.BindingFlags]'Static,Public,NonPublic'
$type=$asm.GetType('ReinforcementSchedules.Component')
$mod=$asm.GetType('ReinforcementSchedules.Module1')
$mod.GetField('vCC',$stat).SetValue($null,[byte]1)
$f=[Activator]::CreateInstance($type)
function Field($name) { $fi=$type.GetField($name,$flags); if($fi){return ,$fi.GetValue($f)}; return ,$type.GetProperty($name,$flags).GetValue($f,$null) }
function Call($name,$args1) { $type.GetMethod($name,$flags).Invoke($f,$args1) }
function Check($v,$msg) { if(-not $v) {throw "FAILED: $msg"}; "PASS: $msg" }
$f.StartPosition='Manual'; $f.Location=New-Object Drawing.Point(-20000,-20000);$f.ShowInTaskbar=$false


try {
$f.Show();[Windows.Forms.Application]::DoEvents()
(Field 'cbbScheduleMode').SelectedItem='Breakpoint Schedules'; Call 'btnAddInput_Click' @($null,[EventArgs]::Empty)
(Field 'cbbScheduleMode').SelectedItem='Breakpoint Schedules';(Field 'breakpointStart').Value=42;(Field 'txbInputName').Text='Custom unfinished input'
Call 'DeleteInputAt' @([int]0)|Out-Null
Check ((Field 'cbbScheduleMode').Text -eq 'Breakpoint Schedules' -and (Field 'breakpointStart').Value -eq 42 -and (Field 'txbInputName').Text -eq 'Custom unfinished input') 'Nonfull delete preserves unfinished custom draft'
} finally {$f.Dispose()}
$mod.GetField('vCC',$stat).SetValue($null,[byte]1)
$f=[Activator]::CreateInstance($type);$f.StartPosition='Manual';$f.Location=New-Object Drawing.Point(-20000,-20000);$f.ShowInTaskbar=$false
try {
$f.Show();[Windows.Forms.Application]::DoEvents()
(Field 'cbbScheduleMode').SelectedItem='Lag Schedules'
(Field 'lagInputs').SetItemChecked(2,$true)
$types=Field 'lagFeedbackTypes';$durations=Field 'lagFeedbackDurations'
$types[0]='Tone';$types[1]='Light 1';$types[2]='Light 2';$durations[0]='1';$durations[1]='2';$durations[2]='3'
Call 'RefreshLagFeedbackInputs' @([int]-1,$false,$false)
Check (Call 'ApplyLagInputs' @()) 'Three-input Lag applied'
$l=Field 'lstInputs'; $l.SelectedIndex=0
Check (-not (Field 'deleteInputButton').Enabled -and (Call 'SelectedInputSlot' @()) -eq -1) 'Lag heading not deletable'
$l.SelectedIndex=1
Check (-not (Field 'deleteInputButton').Enabled) 'Lag failure heading not deletable'
$l.SelectedIndex=3;(Field 'lagFeedbackInput').SelectedIndex=1
Check ((Field 'deleteInputButton').Enabled) 'Active Lag input enables Delete'
Check (Call 'DeleteInputAt' @([int]1)) 'Middle Lag input deleted'
[Windows.Forms.Application]::DoEvents()
$a=Field 'AddedInputs';$types=Field 'lagFeedbackTypes';$durations=Field 'lagFeedbackDurations'
Check (-not (Field 'lagInputs').GetItemChecked(1) -and $a[1].ScheduleType -eq 'N/A') 'Deleted Lag input deselected and N/A'
Check ($types[1] -eq 'None' -and $durations[1] -eq '1') 'Deleted Lag feedback cleared'
Check ($types[0] -eq 'Tone' -and $types[2] -eq 'Light 2' -and $durations[2] -eq '3') 'Other Lag feedback retained'
Check ($a[2].ScheduleType -eq 'Lag' -and $a[2].Name -eq 'Arduino Pin 4') 'Later Lag physical port unchanged'
$l.SelectedIndex=3
Check (-not (Field 'deleteInputButton').Enabled) 'Deleted Lag row not deletable'
Check (Call 'ApplyLagInputs' @()) 'Remaining two Lag inputs can be applied'
Check ((Field 'AddedInputs')[2].FeedbackType -eq 'Light 2') 'Remaining Lag feedback persists after reapply'
Check (Call 'DeleteInputAt' @([int]0)) 'Lag can temporarily have one input'
Check ((Field 'lagInputs').CheckedItems.Count -eq 1 -and -not (Field 'cbbScheduleMode').Enabled) 'One Lag input retains locked mode'
Check (Call 'DeleteInputAt' @([int]2)) 'Delete last Lag input'
Check ((Field 'AddedInputs').Count -eq 0 -and (Field 'cbbScheduleMode').Enabled -and (Field 'lagInputs').CheckedItems.Count -eq 0) 'Empty Lag unlocks mode without selected inputs'
(Field 'cbbScheduleMode').SelectedItem='Breakpoint Schedules'
Call 'btnAddInput_Click' @($null,[EventArgs]::Empty)
Check ((Field 'AddedInputs').Count -eq 1 -and (Field 'AddedInputs')[0].ScheduleType -eq 'Breakpoint') 'New regular input possible after deleting all Lag'
[Windows.Forms.Application]::DoEvents()
$bmp=New-Object Drawing.Bitmap($f.Width,$f.Height);$f.DrawToBitmap($bmp,(New-Object Drawing.Rectangle(0,0,$f.Width,$f.Height)));$bmp.Save((Join-Path $env:TEMP 'soft-breakpoint-ui-qa\delete-input-ui.png'));$bmp.Dispose()
'LAG DELETE UI QA COMPLETE'
} finally {$f.Dispose()}
