$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$a=[Reflection.Assembly]::LoadFrom((Join-Path $PSScriptRoot '..\bin\IntervalValidation\ReinforcementSchedules.exe'))
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$module=$a.GetTypes() | Where-Object Name -eq Module1
$module.GetField('vCC',$flags).SetValue($null,[byte]1)
function Field($o,$n){$member=$o.GetType().GetField($n,$flags); if($member){return ,$member.GetValue($o)}; return ,$o.GetType().GetProperty($n,$flags).GetValue($o,$null)}
function Call($o,$n){$null=$o.GetType().GetMethod($n,$flags).Invoke($o,@($null,[EventArgs]::Empty))}
function Assert($b,$m){if(-not $b){throw $m}; Write-Output "PASS $m"}
function NewEditor {
 $f=[Activator]::CreateInstance(($a.GetTypes() | Where-Object FullName -eq 'ReinforcementSchedules.Component'))
 $null=$f.Handle
 $f.Show(); [Windows.Forms.Application]::DoEvents(); $f.Hide()
 return $f
}
$f=NewEditor
$mode=Field $f cbbScheduleMode
$rawValidator=$f.GetType().GetMethod('ValidateLagReinforcerText',$flags)
Assert ($rawValidator.Invoke($null,@('abc','100','50','Pellet')) -ne '') 'Reject malformed Lag magnitude'
Assert ($rawValidator.Invoke($null,@('1','abc','50','Pellet')) -ne '') 'Reject malformed Lag delivery probability'
Assert ($rawValidator.Invoke($null,@('1','100','abc','Random')) -ne '') 'Reject malformed Lag pellet probability'
Assert ($rawValidator.Invoke($null,@('1','100','50','Random')) -eq '') 'Accept valid Lag reinforcer values'
Assert ($mode.Items.Contains('Lag Schedules')) 'Third mode exists'
$mode.Text='T Schedules'; Assert ((Field $f grpTSchedule).Visible -eq $false) 'Form hidden during isolated test'
$mode.Text='Lag Schedules'
Assert (-not (Field $f grpTimeSchedule).Enabled -and -not (Field $f txbCOD).Enabled) 'FT/VT/COD disabled'
$inputs=Field $f lagInputs
0..5 | ForEach-Object {$inputs.SetItemChecked($_,$_ -eq 1 -or $_ -eq 5)}
$selector=Field $f lagFeedbackInput
Assert ($selector.Items.Count -eq 2 -and $selector.Items[0] -eq 'Input 2 / Pin 3' -and $selector.Items[1] -eq 'Input 6 / Pin 7') 'Selector contains only active physical inputs'
Assert ($selector.Parent -eq (Field $f grpStimL1)) 'Selector is beside response feedback'
Assert ((Field $f lagPossibleSequences).Text -eq 'Possible sequences: 16 (2^4)') 'Sequence count updates after changing inputs'
(Field $f lagLength).Value=3
Assert ((Field $f lagPossibleSequences).Text -eq 'Possible sequences: 8 (2^3)') 'Sequence count updates after changing length'
0..5 | ForEach-Object {$inputs.SetItemChecked($_,$true)}
(Field $f lagLength).Value=10000
Assert ((Field $f lagPossibleSequences).Text -like '*10^7781 (6^10000)') 'Maximum sequence count does not overflow'
0..5 | ForEach-Object {$inputs.SetItemChecked($_,$false)}
Assert (-not $selector.Enabled -and $selector.Items.Count -eq 0 -and (Field $f lagPossibleSequences).Text -eq 'Possible sequences: 0 (0^10000)') 'No active inputs clears and disables selector'
$mode.Text='Classic schedules'
Assert ((Field $f clbFeedbackStim).Enabled) 'Empty Lag selection does not disable Classic feedback'
$mode.Text='T Schedules'
Assert ((Field $f clbFeedbackStim).Enabled) 'Empty Lag selection does not disable T feedback'
$mode.Text='Lag Schedules'
Assert (-not (Field $f clbFeedbackStim).Enabled) 'Empty Lag selection remains disabled on return'
$inputs.SetItemChecked(5,$true)
Assert ($selector.Items.Count -eq 1 -and $selector.Items[0] -eq 'Input 6 / Pin 7' -and (Field $f lagPossibleSequences).Text -eq 'Possible sequences: 1 (1^10000)') 'One active physical input and long sequence count'
$inputs.SetItemChecked(1,$true)
(Field $f lagLength).Value=4
Assert ((Field $f ComponentToolTip).GetToolTip((Field $f txbStimDurL1)).Contains('0 keeps')) 'Tooltip explains zero duration'

$selector.SelectedIndex=0
$fb=Field $f clbFeedbackStim
$fb.SetItemChecked(0,$true); (Field $f txbStimDurL1).Text='0.2'
$selector.SelectedIndex=1
$fb.SetItemChecked(4,$true); (Field $f txbStimDurL1).Text='0'
$mode.Text='Classic schedules'; $mode.Text='T Schedules'; $mode.Text='Lag Schedules'
Assert ($fb.GetItemChecked(4) -and (Field $f txbStimDurL1).Text -eq '0') 'Feedback survives mode switches'
$selector.SelectedIndex=0
Assert ($fb.GetItemChecked(0) -and -not $fb.GetItemChecked(4) -and (Field $f txbStimDurL1).Text -eq '0.2') 'Per-input feedback preserved'
$reinforcer=Field $f cbbReinforcer1
$reinforcer.Text='Random'
Assert ((Field $f txbPelletProbability1).Enabled) 'Random pellet probability enabled in Lag'
$reinforcer.Text='Pellet'
$delayOutputs=Field $f clbDelayStim
$delayOutputs.SetItemChecked(5,$true)
[Windows.Forms.Application]::DoEvents()
(Field $f txbDelayDurL1).Text='3'
(Field $f txbDelaySignalDurationL1).Text='1'
(Field $f chkRetractL1).Checked=$true
Call $f btnAddInput_Click
Call $f btnAddInput_Click
$list=Field $f AddedInputs
Assert ($list.Count -eq 6) 'Repeated Apply keeps six physical slots'
Assert ($list[1].ScheduleType -eq 'Lag' -and $list[5].ScheduleType -eq 'Lag' -and $list[0].ScheduleType -eq 'N/A') 'Noncontiguous inputs 2 and 6 preserved'
Assert ($list[1].FeedbackType -eq 'Light 1' -and $list[5].FeedbackType -eq 'Tone' -and $list[5].FeedbackDuration -eq 0) 'Distinct feedback and zero duration stored'
$inputs.SetItemChecked(5,$false); $inputs.SetItemChecked(5,$true)
$selector.SelectedIndex=1
Assert ($fb.GetItemChecked(4) -and (Field $f txbStimDurL1).Text -eq '0') 'Deactivating and reactivating input preserves feedback'

$selector.SelectedIndex=1
(Field $f lagFailure).SelectedIndex=1
(Field $f lagTimeout).Value=2.5
Call $f btnAddInput_Click
$f.Show()
[Windows.Forms.Application]::DoEvents()
$b=New-Object Drawing.Bitmap $f.Width,$f.Height
$f.DrawToBitmap($b,(New-Object Drawing.Rectangle 0,0,$f.Width,$f.Height))
$b.Save("$PSScriptRoot\LagEditorTests.png",[Drawing.Imaging.ImageFormat]::Png);$b.Dispose()
(Field $f txbComponentDuration).Text='60'
(Field $f txbComponentIterations).Text='1'
Call $f btnSubmit_Click
$blueprints=$module.GetField('AC',$flags).GetValue($null)
Assert ($blueprints[1].LagEnabled -and $blueprints[1].InputCount -eq 6 -and $blueprints[1].ScheduleType[5] -eq 'Lag') 'Submit saves Lag blueprint in memory'
Assert ($blueprints[1].LagUseTimeout -and $blueprints[1].LagTimeoutSeconds -eq 2.5) 'Failure TO saved'
Assert ($blueprints[1].DelayDuration[1] -eq 3 -and $blueprints[1].DelayDuration[5] -eq 3 -and $blueprints[1].DelaySignalDuration[5] -eq 1 -and $blueprints[1].DelayRetract[5]) 'Shared delay and retraction saved'
$f.Dispose()
$module.GetField('vCC',$flags).SetValue($null,[byte]2)
$f=NewEditor
(Field $f cbbScheduleMode).Text='Lag Schedules'
Call $f btnClose_Click
Assert ($module.GetField('vCC',$flags).GetValue($null) -eq 1) 'Cancel restores component count'
$f.Dispose()
$module.GetField('vCC',$flags).SetValue($null,[byte]2)
$f=NewEditor
Call $f btnAddInput_Click
Assert (-not (Field $f cbbScheduleMode).Items.Contains('Lag Schedules')) 'Classic inputs cannot mix with Lag'
Call $f btnClose_Click
$f.Dispose()
$module.GetField('vCC',$flags).SetValue($null,[byte]2)
$f=NewEditor
(Field $f cbbScheduleMode).Text='Lag Schedules'
$f.Close()
Assert ($module.GetField('vCC',$flags).GetValue($null) -eq 1) 'Window X/Alt+F4 closing restores component count'
$f.Dispose()
Write-Output 'ALL UI TESTS PASSED; no hardware session or file persistence invoked.'