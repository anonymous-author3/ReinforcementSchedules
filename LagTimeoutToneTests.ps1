param([string]$AssemblyPath = 'bin\LagValidation\ReinforcementSchedules.exe')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms,System.Drawing,System.Runtime.Serialization
$a=[Reflection.Assembly]::LoadFrom((Join-Path $PSScriptRoot $AssemblyPath))
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$module=$a.GetType('ReinforcementSchedules.Module1')
$script:assertions=0
$root=Join-Path $PSScriptRoot 'LagRuntimeTests\ToneValidation'
$null=New-Item -ItemType Directory -Path $root -Force
function Field($o,$n){$member=$o.GetType().GetField($n,$flags);if($member){return ,$member.GetValue($o)};return ,$o.GetType().GetProperty($n,$flags).GetValue($o,$null)}
function Invoke($o,$n,[object[]]$values=@()){for($i=0;$i -lt $values.Length;$i++){if($null -ne $values[$i]){$values[$i]=$values[$i].PSObject.BaseObject}};return ,$o.GetType().GetMethod($n,$flags).Invoke($o,$values)}
function Event($o,$n){$null=Invoke $o $n @($null,[EventArgs]::Empty)}
function Check($b,$m){if(-not $b){throw "FAIL: $m"};$script:assertions++;Write-Output "PASS $m"}
function Components{return ,$module.GetField('AC',$flags).GetValue($null)}
function NewEditor {
 $module.GetField('vCC',$flags).SetValue($null,[byte]1)
 $f=[Activator]::CreateInstance($a.GetType('ReinforcementSchedules.Component'))
 $null=$f.Handle
 Event $f Component_Load
 (Field $f cbbScheduleMode).Text='Lag Schedules'
 return $f
}
$f=NewEditor
$tone=Field $f lagTimeoutTone
$failure=Field $f lagFailure
Check ($tone.Text -eq 'Tono' -and -not $tone.Checked -and -not $tone.Enabled) 'Tono starts unchecked and disabled for omission'
$failure.SelectedIndex=1
Check ($tone.Enabled -and (Field $f lagTimeout).Enabled) 'Selecting TO enables duration and Tono'
$tone.Checked=$true
$failure.SelectedIndex=0
Check (-not $tone.Enabled -and -not (Field $f lagTimeout).Enabled) 'Returning to omission disables Tono'
$failure.SelectedIndex=1
Check $tone.Checked 'Re-selecting TO preserves unsaved tone choice'
(Field $f lagTimeout).Value=2.5
Event $f btnAddInput_Click
Event $f btnAddInput_Click
Check ((Field $f lstInputs).Items[1] -like '*2.5* + Tono') 'Repeated Apply previews tone and configured duration'
$panel=Field $f lagPanel
Check ($tone.Right -le $panel.ClientSize.Width -and $tone.Bottom -le (Field $f lagTimeout).Top) 'Checkbox fits within timeout row without overlapping duration'
$b=New-Object Drawing.Bitmap $panel.Width,$panel.Height
$panel.DrawToBitmap($b,(New-Object Drawing.Rectangle 0,0,$panel.Width,$panel.Height)); foreach($control in $panel.Controls){$control.DrawToBitmap($b,$control.Bounds)}
$b.Save((Join-Path $root 'TimeoutToneEditor.png'),[Drawing.Imaging.ImageFormat]::Png);$b.Dispose()
(Field $f txbComponentDuration).Text='60'
(Field $f txbComponentIterations).Text='1'
Event $f btnSubmit_Click
$c=Components
Check ($c[1].LagTimeoutTone -and $c[1].LagUseTimeout -and $c[1].LagTimeoutSeconds -eq 2.5) 'Submit saves tone with TO duration'
$f.Dispose()
$setup=[Activator]::CreateInstance($a.GetType('ReinforcementSchedules.SetUp'))
$null=$setup.Handle
$settings=Invoke $setup CaptureSetup
$path=Join-Path $root 'with-tone.json'
$null=Invoke $settings Save @($path)
$st=$a.GetType('ReinforcementSchedules.SetupSaveData')
$loaded=$st.GetMethod('Read',$flags).Invoke($null,@([string]$path))
Check ($loaded.Components[1].LagTimeoutTone) 'JSON save/load preserves Tono'
$legacyPath=Join-Path $root 'legacy-tone.xml'
$settings.FormatVersion=5
$writer=[Xml.XmlWriter]::Create($legacyPath)
try { ([Runtime.Serialization.DataContractSerializer]::new($st)).WriteObject($writer,$settings) } finally { $writer.Dispose() }
$xml=[IO.File]::ReadAllText($legacyPath)
[xml]$legacyXml=$xml; foreach($node in @($legacyXml.SelectNodes("//*[local-name()='LagTimeoutTone']"))){$null=$node.ParentNode.RemoveChild($node)}; $oldXml=$legacyXml.OuterXml
foreach($version in @(4,5)) {
 $vxml=$oldXml.Replace('<FormatVersion>5</FormatVersion>',"<FormatVersion>$version</FormatVersion>")
 $old=$st.GetMethod('ReadXml',$flags).Invoke($null,@($vxml))
 Check (-not $old.Components[1].LagTimeoutTone -and $old.Components[1].LagUseTimeout) "Older v$version XML defaults Tono off without changing TO"
}
$lines=Invoke $setup ConfigurationSnapshotLines @($true)
Check (@($lines | Where-Object {$_ -eq 'Component 1 Lag Timeout Tone: True'}).Count -eq 1) 'Text snapshot records timeout tone'
# Legacy TXT uses the pre-command-map header; current snapshots are audit records, not saved XML protocols.
$lines=@($lines | Where-Object {$_ -notlike 'Arduino *'})
$textPath=Join-Path $root 'with-tone.txt'
[IO.File]::WriteAllLines($textPath,[string[]]$lines)
$null=Invoke $setup LoadLegacyConfiguration @($textPath)
$c=Components
Check $c[1].LagTimeoutTone 'Legacy text protocol loads Tono'
$oldPath=Join-Path $root 'without-tone.txt'
[IO.File]::WriteAllLines($oldPath,[string[]]@($lines | Where-Object {$_ -notlike '*Lag Timeout Tone:*'}))
$null=Invoke $setup LoadLegacyConfiguration @($oldPath)
$c=Components
Check (-not $c[1].LagTimeoutTone -and $c[1].LagUseTimeout -and $c[1].InputCount -eq 6) 'Old text protocol clears prior tone and preserves input alignment'
$setup.Dispose()
$f=NewEditor
(Field $f lagFailure).SelectedIndex=1
(Field $f lagTimeoutTone).Checked=$true
(Field $f lagFailure).SelectedIndex=0
(Field $f txbComponentDuration).Text='60'
(Field $f txbComponentIterations).Text='1'
Event $f btnSubmit_Click
$c=Components
Check (-not $c[1].LagUseTimeout -and -not $c[1].LagTimeoutTone) 'Submitting omission never stores an active timeout tone'
$f.Dispose()
$f=NewEditor
(Field $f lagFailure).SelectedIndex=1
(Field $f lagTimeoutTone).Checked=$true
Event $f btnClose_Click
Check ($module.GetField('vCC',$flags).GetValue($null) -eq 0) 'Cancel discards unsubmitted tone component'
$f.Dispose()
Write-Output "PASS timeout editor/persistence: $assertions assertions. No visible windows, hardware or network."
