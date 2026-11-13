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


function AddBP($start) {
    (Field 'cbbScheduleMode').SelectedItem='Breakpoint Schedules'
    (Field 'breakpointStart').Value=$start
    Call 'btnAddInput_Click' @($null,[EventArgs]::Empty)
}
try {
$f.Show();[Windows.Forms.Application]::DoEvents()
AddBP 2;AddBP 3;AddBP 4
$before=(Field 'AddedInputs')[2];$beforeName=$before.Name
Check (Call 'DeleteInputAt' @([int]0)) 'Delete first input succeeds'
$a=Field 'AddedInputs'
Check ($a.Count -eq 3 -and $a[0].ScheduleType -eq 'N/A') 'Deletion keeps reserved physical slot'
Check ([object]::ReferenceEquals($a[2],$before) -and $a[2].Name -eq $beforeName) 'Later input unchanged and not renumbered'
Check ((Field 'lstInputs').SelectedIndex -eq -1) 'Delete clears selection to prevent accidental restore'
Check (-not (Call 'DeleteInputAt' @([int]0))) 'Deleting same tombstone has no effect'
AddBP 7
$a=Field 'AddedInputs'
Check ($a.Count -eq 4 -and $a[0].ScheduleType -eq 'N/A' -and $a[3].Breakpoint.StartRequirement -eq 7) 'Unselected Add appends Input4 rather than reusing Input1'
(Field 'lstInputs').SelectedIndex=0
Check ((Field 'btnAddInput').Enabled) 'Selecting deleted row permits restore'
AddBP 9
$a=Field 'AddedInputs'
Check ($a.Count -eq 4 -and $a[0].Breakpoint.StartRequirement -eq 9 -and $a[3].Breakpoint.StartRequirement -eq 7) 'Explicit selection restores only selected physical slot'
Check (Call 'DeleteInputAt' @([int]3)) 'Delete tail'
Check ((Field 'AddedInputs').Count -eq 4 -and (Field 'AddedInputs')[3].ScheduleType -eq 'N/A') 'Tail tombstone retained'
AddBP 10
Check ((Field 'AddedInputs').Count -eq 5 -and (Field 'AddedInputs')[4].Breakpoint.StartRequirement -eq 10) 'Unselected Add after tail deletion appends Input5'
AddBP 11
Check ((Field 'AddedInputs').Count -eq 6 -and -not (Field 'btnAddInput').Enabled) 'Capacity six disables unselected Add'
Check (Call 'DeleteInputAt' @([int]1)) 'Delete at capacity'
Check (-not (Field 'btnAddInput').Enabled) 'Capacity still needs explicit tombstone selection'
(Field 'lstInputs').SelectedIndex=1
Check ((Field 'btnAddInput').Enabled -and (Field 'GroupBox1').Enabled) 'Selected tombstone restores editing at capacity'
(Field 'cbbScheduleMode').SelectedItem='Breakpoint Schedules';(Field 'breakpointStart').Value=17;(Field 'txbInputName').Text='Custom draft'
(Field 'lstInputs').SelectedIndex=3
Check ((Field 'breakpointStart').Value -eq 17 -and (Field 'txbInputName').Text -eq 'Custom draft') 'Changing selected tombstone preserves draft'
(Field 'lstInputs').SelectedIndex=2
Check (-not (Field 'btnAddInput').Enabled) 'Live row cannot be overwritten at capacity'
(Field 'lstInputs').SelectedIndex=1
AddBP 15
Check ((Field 'AddedInputs')[1].Breakpoint.StartRequirement -eq 15 -and (Field 'AddedInputs')[3].ScheduleType -eq 'N/A') 'Explicit capacity restore changes only selected slot'
for($i=5;$i -ge 0;$i--) {Call 'DeleteInputAt' @([int]$i) | Out-Null}
Check ((Field 'AddedInputs').Count -eq 6 -and @((Field 'AddedInputs') | Where-Object {$_.ScheduleType -ne 'N/A'}).Count -eq 0) 'Deleting all retains tombstones without live inputs'
Check ((Field 'cbbScheduleMode').Items.Contains('Lag Schedules') -and (Field 'cbbScheduleMode').Enabled) 'All deleted permits explicit change to Lag'
(Field 'cbbScheduleMode').SelectedItem='Lag Schedules'
Check ((Field 'AddedInputs').Count -eq 0 -and (Field 'deletedInputSlots').Count -eq 0) 'Entering Lag explicitly clears old regular tombstones'
(Field 'cbbScheduleMode').SelectedItem='Classic schedules'; (Field 'rdoNAL1').Checked=$true
Call 'btnAddInput_Click' @($null,[EventArgs]::Empty)
Check (Call 'DeleteInputAt' @([int]0)) 'Explicitly added N/A remains deletable'
Check (-not (Call 'DeleteInputAt' @([int]-1)) -and -not (Call 'DeleteInputAt' @([int]99))) 'Invalid deletion index safe'
(Field 'lstInputs').SelectedIndex=0
AddBP 2;AddBP 3;AddBP 4
Call 'DeleteInputAt' @([int]1) | Out-Null
Call 'btnSubmit_Click' @($null,[EventArgs]::Empty)
$c=$mod.GetField('AC',$stat).GetValue($null)[1]
Check ($c.InputCount -eq 3 -and $c.ScheduleType[1] -eq 'N/A' -and $c.BreakpointSchedules[1] -eq $null -and $c.BreakpointSchedules[2].StartRequirement -eq 4) 'Submit retains physical mapping with deleted slot'
'DELETE UI QA COMPLETE'
} finally {$f.Dispose()}
& (Join-Path $PSScriptRoot 'LagInputDeletionTests.ps1') -AssemblyPath $AssemblyPath
