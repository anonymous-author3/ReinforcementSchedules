param([string]$AssemblyPath = (Join-Path $PSScriptRoot 'bin\ConcurrentValidation\ReinforcementSchedules.exe'))
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$asm=[Reflection.Assembly]::LoadFrom((Resolve-Path $AssemblyPath).Path)
$flags=[Reflection.BindingFlags]'Instance,Public,NonPublic'
$stat=[Reflection.BindingFlags]'Static,Public,NonPublic'
$calc=$asm.GetType('ReinforcementSchedules.ConcurrentViCalculation')
$n=0
function Check($test,$name) {if(-not $test){throw "FAIL: $name"};$script:n++;"PASS: $name"}
function Values($g,$w) {return ,$calc.GetMethod('Intervals',$stat).Invoke($null,@([double]$g,[double[]]$w))}
foreach($case in @(@(1,1),@(1,3),@(1,27),@(1,1,1),@(1,2,3,4,5,6))) {
 $v=Values 20 $case
 $sum=0;foreach($x in $v){$sum+=1/$x}
 Check ([math]::Abs($sum-.05) -lt 1e-12) "Sum rates: $case"
 for($i=1;$i -lt $v.Length;$i++){Check ([math]::Abs($v[0]/$v[$i]-$case[$i]/$case[0]) -lt 1e-10) 'Rates preserve ratio'}
}
Check ((Values 20 @(1,1))[0] -eq 40) '1:1 => 40 s'
Check ((Values 20 @(1,3))[0] -eq 80) '1:3 => 80 s'
Check ((Values 20 @(1,27))[0] -eq 560) '1:27 => 560 s'
foreach($bad in @(@(0,1),@(-1,1),@(1),@(1,1,1,1,1,1,1),@([double]::NaN,1),@([double]::PositiveInfinity,1))) {
 $rejected=$false;try{Values 20 $bad | Out-Null}catch{$rejected=$true};Check $rejected 'Invalid ratio rejected'
}
foreach($g in @(0,-1,[double]::NaN,[double]::PositiveInfinity)){$rejected=$false;try{Values $g @(1,1)|Out-Null}catch{$rejected=$true};Check $rejected 'Invalid global VI rejected'}
$type=$asm.GetType('ReinforcementSchedules.ConcurrentViCalculator')
$f=[Activator]::CreateInstance($type,$true)
function Field($name){return ,$type.GetField($name,$flags).GetValue($f)}
try {
 $f.StartPosition='Manual';$f.Location=New-Object Drawing.Point(-20000,-20000);$f.ShowInTaskbar=$false;$f.Show();[Windows.Forms.Application]::DoEvents()
 $grid=Field 'output'
 Check ($grid.Rows.Count -eq 2 -and $grid.Rows[0].Cells[2].Value -eq '40') 'Initial UI 1:1'
 $w=Field 'weights';$w[1].Value=3
 Check ($grid.Rows[0].Cells[2].Value -eq '80' -and $grid.Rows[1].Cells[3].Value -eq '27') 'Live ratio refresh and rounding'
 (Field 'alternatives').Value=6
 Check ($grid.Rows.Count -eq 6 -and $w[5].Visible) 'Six alternatives'
 (Field 'alternatives').Value=2
 Check ($grid.Rows.Count -eq 2 -and -not $w[5].Visible) 'Hide unused weights'
 (Field 'globalVi').Value=.001
 Check ($grid.Rows[0].Cells[3].Value -eq '1') 'Positive minimum rounded mean'
 (Field 'globalVi').Value=1000000;$w[0].Value=.001;$w[1].Value=1000000
 Check ($grid.Rows[0].Cells[4].Value -eq 'Out of range') 'Unsupported range reported without overflow'
 (Field 'globalVi').Value=20;$w[0].Value=1;$w[1].Value=27
 $img=New-Object Drawing.Bitmap($f.Width,$f.Height);$f.DrawToBitmap($img,(New-Object Drawing.Rectangle(0,0,$f.Width,$f.Height)));$img.Save((Join-Path $env:TEMP 'soft-concurrent-calculator.png'));$img.Dispose()
 Check ($f.CancelButton.Text -eq 'Close') 'Close/Escape action'
}finally{$f.Dispose()}
$compType=$asm.GetType('ReinforcementSchedules.Component');$mod=$asm.GetType('ReinforcementSchedules.Module1');$mod.GetField('vCC',$stat).SetValue($null,[byte]1)
$c=[Activator]::CreateInstance($compType)
function CField($name){$fi=$compType.GetField($name,$flags);if($fi){return ,$fi.GetValue($c)};return ,$compType.GetProperty($name,$flags).GetValue($c,$null)}
try {
 $c.StartPosition='Manual';$c.Location=New-Object Drawing.Point(-20000,-20000);$c.ShowInTaskbar=$false;$c.Show();[Windows.Forms.Application]::DoEvents()
 $button=CField 'concurrentViButton'
 Check $button.Visible 'Classic button visible'
 foreach($mode in @('T Schedules','Lag Schedules','Breakpoint Schedules')){(CField 'cbbScheduleMode').SelectedItem=$mode;Check (-not $button.Visible) "Hidden in $mode"}
 (CField 'cbbScheduleMode').SelectedItem='Classic schedules'
 Check $button.Visible 'Returns in Classic'
 $img=New-Object Drawing.Bitmap($c.Width,$c.Height);$c.DrawToBitmap($img,(New-Object Drawing.Rectangle(0,0,$c.Width,$c.Height)));$img.Save((Join-Path $env:TEMP 'soft-concurrent-component.png'));$img.Dispose()
 $before=(CField 'AddedInputs').Count
 $timer=New-Object Windows.Forms.Timer;$timer.Interval=200;$timer.Add_Tick({foreach($open in @([Windows.Forms.Application]::OpenForms)){if($open.Name -eq 'ConcurrentViCalculator'){$open.Close()}}})
 $timer.Start();$button.PerformClick();$button.PerformClick();$timer.Stop();$timer.Dispose()
 Check ((CField 'AddedInputs').Count -eq $before) 'Open/close/reopen never changes component inputs'
}finally{$c.Dispose()}
"PASS: $n checks"