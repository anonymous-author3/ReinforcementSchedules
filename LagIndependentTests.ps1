param([string]$AssemblyPath = 'bin\LagValidation\ReinforcementSchedules.exe')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Runtime.Serialization
$a = [Reflection.Assembly]::Load([IO.File]::ReadAllBytes((Join-Path (Get-Location) $AssemblyPath)))
$t = $a.GetType('ReinforcementSchedules.LagSequenceTracker', $true)
$ct = $a.GetType('ReinforcementSchedules.Module1+ComponentBlueprint', $true)
$st = $a.GetType('ReinforcementSchedules.SetupSaveData', $true)
$script:assertions = 0
function Check($condition, [string]$message) { if (-not $condition) { throw "FAIL: $message" }; $script:assertions++ }
function GetProp($o,[string]$n) { $o.GetType().GetProperty($n).GetValue($o,$null) }
function SetProp($o,[string]$n,$v) { $o.GetType().GetProperty($n).SetValue($o,$v,$null) }
function SetField($o,[string]$n,$v) { $o.GetType().GetField($n).SetValue($o,$v) }
function Field($o,[string]$n) { return ,($o.GetType().GetField($n).GetValue($o)) }
function NewTracker([int]$n,[int]$lag) { [Activator]::CreateInstance($t,[object[]]@($n,$lag)) }
function Push($tracker,[string]$sequence) { $r=$null; foreach($ch in $sequence.ToCharArray()) { $r=$t.GetMethod('AddResponse').Invoke($tracker,[object[]]@([int]::Parse([string]$ch))) }; $r }
function Reject([scriptblock]$action,[string]$message) { $caught=$false; try { &$action | Out-Null } catch { $caught=$true }; Check $caught $message }
Add-Type -TypeDefinition @"
using System;
using System.Reflection;
public static class LagFixture {
  public static void F(object o,string n,object v){o.GetType().GetField(n).SetValue(o,v);}
  public static void P(object o,string n,object v){o.GetType().GetProperty(n).SetValue(o,v,null);}
  public static object Sample(Assembly a,bool lag){
    var ct=a.GetType("ReinforcementSchedules.Module1+ComponentBlueprint");
    object c=Activator.CreateInstance(ct);
    foreach(var f in ct.GetFields()) if(f.FieldType.IsArray) f.SetValue(c,Array.CreateInstance(f.FieldType.GetElementType(),6));
    F(c,"InputCount",6);F(c,"ComponentDuration",60d);F(c,"ComponentIteration",(byte)2);F(c,"ComponentName","Independent test");F(c,"TimeScheduleType","None");F(c,"ComponentStimType","None");
    F(c,"LagEnabled",lag);F(c,"LagSequenceLength",4);F(c,"LagValue",2);F(c,"LagUseTimeout",true);F(c,"LagTimeoutSeconds",1.25d);
    var names=(string[])ct.GetField("InputName").GetValue(c);var sch=(string[])ct.GetField("ScheduleType").GetValue(c);var rf=(string[])ct.GetField("Reinforcer").GetValue(c);var mag=(int[])ct.GetField("Magnitude").GetValue(c);var p=(int[])ct.GetField("DeliveryP").GetValue(c);var ft=(string[])ct.GetField("FeedbackType").GetValue(c);var dt=(string[])ct.GetField("DelayType").GetValue(c);
    for(int i=0;i<6;i++){names[i]="Pin "+(i+2);sch[i]="N/A";rf[i]="N/A";ft[i]="None";dt[i]="None";}
    foreach(int i in (lag?new int[]{0,2,5}:new int[]{0})){sch[i]=lag?"Lag":"Fixed Ratio";rf[i]="Pellet";mag[i]=1;p[i]=100;}
    ((int[])ct.GetField("ScheduleValue").GetValue(c))[0]=5;
    var st=a.GetType("ReinforcementSchedules.SetupSaveData");object s=Activator.CreateInstance(st);
    foreach(var prop in st.GetProperties())if(prop.PropertyType==typeof(string))prop.SetValue(s,"",null);
    P(s,"BoxName","Independent");P(s,"StartDelay","0");P(s,"PostSession","0");P(s,"ICIDuration","0");P(s,"ICIStimulus","None");P(s,"ComponentOrder","1,1");P(s,"SavedAtUtc",DateTime.UtcNow);P(s,"CommandMap",Activator.CreateInstance(a.GetType("ReinforcementSchedules.ArduinoCommandMap")));P(s,"ComponentCount",1);
    var ca=Array.CreateInstance(ct,11);ca.SetValue(c,1);P(s,"Components",ca);return s;
  }
  public static void Change(object s,string n,object value){var ca=(Array)s.GetType().GetProperty("Components").GetValue(s,null);var c=ca.GetValue(1);F(c,n,value);ca.SetValue(c,1);}
}
"@
function Sample([bool]$lag) { [LagFixture]::Sample($a,$lag) }
function Xml($s){$ser=New-Object System.Runtime.Serialization.DataContractSerializer($st);$m=New-Object IO.MemoryStream;try{$ser.WriteObject($m,$s);[Text.Encoding]::UTF8.GetString($m.ToArray())}finally{$m.Dispose()}}
function ReadXml([string]$xml){$st.GetMethod('ReadXml').Invoke($null,[object[]]@($xml))}
# Boundary and window behavior.
$x=NewTracker 4 0; foreach($seq in @('1236','1236','6666')) { $r=Push $x $seq; Check (GetProp $r 'MeetsCriterion') 'Lag 0 accepts every complete sequence'; Check ((GetProp $r 'ComparedCount') -eq 0) 'Lag 0 compares none' }
Check ((GetProp $x 'HistoryCount') -eq 0) 'Lag 0 retains no history'
$x=NewTracker 2 1; foreach($case in @(@('11',$true),@('11',$false),@('22',$true),@('11',$true))) {$r=Push $x $case[0];Check ((GetProp $r 'MeetsCriterion') -eq $case[1]) ('Lag1 '+$case[0])}
$x=NewTracker 2 2; foreach($case in @(@('11',$true),@('22',$true),@('11',$false),@('22',$false),@('33',$true),@('11',$true))) {$r=Push $x $case[0];Check ((GetProp $r 'MeetsCriterion') -eq $case[1]) ('Lag2 '+$case[0])}
$x=NewTracker 1 2; foreach($case in @(@('1',$true),@('1',$false),@('1',$false),@('2',$true),@('1',$false))) {$r=Push $x $case[0];Check ((GetProp $r 'MeetsCriterion') -eq $case[1]) 'Repeated entries maintain multiplicity'}
$x=NewTracker 4 5; Check ($null -eq (Push $x '136')) 'Partial is not evaluated';Check ((GetProp $x 'PartialCount') -eq 3) 'Partial count';$discard=$t.GetMethod('DiscardPartial').Invoke($x,$null);Check ($discard -eq '136') 'Discard preserves physical identities';Check ((GetProp $x 'HistoryCount') -eq 0) 'Partial does not enter history';$r=Push $x '1636';Check ((GetProp $r 'Sequence') -eq '1636') 'Non-contiguous physical IDs preserved';Check ((GetProp $r 'ComparedCount') -eq 0) 'First complete compares zero';$t.GetMethod('DiscardPartial').Invoke($x,$null)|Out-Null;$r=Push $x '1636';Check (-not (GetProp $r 'MeetsCriterion')) 'History survives partial reset';$y=NewTracker 4 5;Check (GetProp (Push $y '1636') 'MeetsCriterion') 'Fresh session has independent history'
Reject {NewTracker 0 1} 'Reject length 0';Reject {NewTracker 1 -1} 'Reject negative Lag';Reject {$t.GetMethod('AddResponse').Invoke($x,[object[]]@([int]0))} 'Reject input 0';Reject {$t.GetMethod('AddResponse').Invoke($x,[object[]]@([int]7))} 'Reject input 7'
# Exhaustive deterministic reference comparison, including duplicate history.
$rng=New-Object Random 20261110
foreach($n in @(1,2,4,12)){foreach($lag in @(0,1,2,5,31)){$x=NewTracker $n $lag;$history=New-Object 'Collections.Generic.List[string]';for($j=0;$j -lt 250;$j++){$chars=for($k=0;$k -lt $n;$k++){[string]$rng.Next(1,7)};$seq=$chars -join '';if($j%3 -eq 0 -and $history.Count -gt 0){$seq=$history[$history.Count-1]};$start=[Math]::Max(0,$history.Count-$lag);$expected=$true;for($k=$start;$k -lt $history.Count;$k++){if($history[$k] -eq $seq){$expected=$false}};$r=Push $x $seq;Check ((GetProp $r 'MeetsCriterion') -eq $expected) 'Oracle criterion';Check ((GetProp $r 'ComparedCount') -eq [Math]::Min($lag,$history.Count)) 'Oracle window';Check ((GetProp $r 'SequenceNumber') -eq $j+1) 'Sequence ordinal';$history.Add($seq)}}}
# Serialization compatibility and invalid combinations.
$s=Sample $true;$xml=Xml $s;$loaded=ReadXml $xml;$c=(GetProp $loaded 'Components').GetValue(1);Check ((GetProp $loaded 'FormatVersion') -eq 5) 'Format 5 round-trip';Check (Field $c 'LagEnabled') 'Lag enabled round-trip';Check ((Field $c 'LagValue') -eq 2) 'Lag value round-trip';Check ((Field $c 'LagSequenceLength') -eq 4) 'Length round-trip';Check ((Field $c 'LagTimeoutSeconds') -eq 1.25) 'Fractional timeout round-trip';Check (((Field $c 'ScheduleType') -join ',') -eq 'Lag,N/A,Lag,N/A,N/A,Lag') 'Physical subset round-trip'
$old=Sample $false;[xml]$oldXml=Xml $old;foreach($node in @($oldXml.SelectNodes("//*[starts-with(local-name(),'Lag')]"))){$node.ParentNode.RemoveChild($node)|Out-Null};$oldXml.SelectSingleNode("//*[local-name()='FormatVersion']").InnerText='3';$oldLoaded=ReadXml $oldXml.OuterXml;$c=(GetProp $oldLoaded 'Components').GetValue(1);Check (-not (Field $c 'LagEnabled')) 'Old XML without Lag fields loads disabled';Check ((Field $c 'LagSequenceLength') -eq 0) 'Old XML defaults missing fields';Check ((Field $c 'ScheduleValue')[0] -eq 5) 'Old Classic configuration retained'
[xml]$futureXml=$xml;$futureXml.SelectSingleNode("//*[local-name()='FormatVersion']").InnerText='6';Reject {ReadXml $futureXml.OuterXml} 'Future format rejected'
foreach($case in @(@('LagValue',[int]-1),@('LagSequenceLength',[int]0),@('LagTimeoutSeconds',[double]::NaN),@('COD',[double]10),@('TimeScheduleType','Fixed Time'))){$s=Sample $true;$ca=GetProp $s 'Components';$c=$ca.GetValue(1);[LagFixture]::Change($s,$case[0],$case[1]);Reject {$st.GetMethod('Validate').Invoke($s,$null)} ('Reject invalid '+$case[0])}
Write-Output ('PASS: '+$script:assertions+' independent assertions; pure engine + deterministic oracle + XML compatibility. No hardware, network, or experimental data used.')