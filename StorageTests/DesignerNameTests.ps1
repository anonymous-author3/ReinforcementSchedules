param([string]$AssemblyPath = '')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms, System.Design, Microsoft.VisualBasic
$source = @"
using System;
using System.CodeDom;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic;

// Model the VB designer's component-name validation, including CodeDom deserialization.
public sealed class DesignerVbNameService : INameCreationService {
    private readonly VBCodeProvider provider = new VBCodeProvider();
    public string CreateName(IContainer container, Type type) { return type.Name + "1"; }
    public bool IsValidName(string name) { return provider.IsValidIdentifier(name); }
    public void ValidateName(string name) {
        if (!IsValidName(name)) throw new ArgumentException("Invalid VB component name: " + name);
    }
}
public sealed class DesignerNameSerializer : CodeDomSerializer {
    private void Assign(IDesignerSerializationManager manager, string variable, string name) {
        DeserializeStatement(manager, new CodeAssignStatement(
            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(variable), "Name"),
            new CodePrimitiveExpression(name)));
    }
    private static int CheckName(string variable, string name, bool expectError) {
        using (var surface = new DesignSurface()) {
            ((IServiceContainer)surface.GetService(typeof(IServiceContainer))).AddService(
                typeof(INameCreationService), new DesignerVbNameService());
            surface.BeginLoad(typeof(Form));
            var host = (IDesignerHost)surface.GetService(typeof(IDesignerHost));
            var column = (DataGridViewTextBoxColumn)host.CreateComponent(typeof(DataGridViewTextBoxColumn), variable);
            var manager = new DesignerSerializationManager(host);
            using (manager.CreateSession()) {
                ((IDesignerSerializationManager)manager).SetName(column, variable);
                new DesignerNameSerializer().Assign(manager, variable, name);
                if (expectError) {
                    if (manager.Errors.Count != 1) throw new Exception("Expected the former invalid name to fail.");
                    string error = manager.Errors[0].ToString();
                    if (!error.Contains("ReflectPropertyDescriptor.SetValue") || !error.Contains("DeserializePropertyAssignStatement"))
                        throw new Exception("Failure did not exercise the reported CodeDom path: " + error);
                    Console.WriteLine("PASS: former name reproduces the reported CodeDom call stack.");
                } else {
                    if (manager.Errors.Count != 0) throw new Exception("Designer rejected " + name + ": " + manager.Errors[0]);
                    if (column.Name != name) throw new Exception("Name assignment was not applied: " + name);
                }
                return 1;
            }
        }
    }
    public static void Run(string designerPath) {
        int count = CheckName("overviewColumn6", "Time (s)", true);
        string source = File.ReadAllText(designerPath);
        var matches = Regex.Matches(source, "Me\\.(overviewColumn[0-9]+)\\.Name = " + '"' + "([^" + '"' + "]+)" + '"');
        if (matches.Count != 18) throw new Exception("Expected all 18 Main overview column names.");
        foreach (Match match in matches) count += CheckName(match.Groups[1].Value, match.Groups[2].Value, false);
        Console.WriteLine("PASS: all 18 current overview column names deserialize without errors (" + count + " cases).");
    }
}
"@
Add-Type -TypeDefinition $source -ReferencedAssemblies System.Design,System.Windows.Forms,Microsoft.VisualBasic
$repo = Split-Path $PSScriptRoot -Parent
[DesignerNameSerializer]::Run((Join-Path $repo 'Main.Designer.vb'))
if ($AssemblyPath) {
    $assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
    $main = [Activator]::CreateInstance($assembly.GetType('ReinforcementSchedules.Main', $true))
    try {
        $flags = [Reflection.BindingFlags]'Instance,Public,NonPublic'
        $property = $main.GetType().GetProperty('overview', $flags)
        if ($property) {
            $grid = $property.GetValue($main, $null)
        } else {
            $field = $main.GetType().GetField('overview', $flags)
            if (-not $field) { throw 'Cannot find the Main overview control.' }
            $grid = $field.GetValue($main)
        }
        if ($grid.Columns.Count -ne 18) { throw 'Main must retain 18 overview columns.' }
        if (-not $grid.Columns.Contains('TimeSeconds')) { throw 'TimeSeconds column lookup failed.' }
        if ($grid.Columns['TimeSeconds'].HeaderText -ne 'Time (s)') { throw 'Visible time header changed.' }
        $row = $grid.Rows[$grid.Rows.Add()]
        $row.Cells['TimeSeconds'].Value = 123
        if ($row.Cells['TimeSeconds'].Value -ne 123) { throw 'Time cell update failed.' }
        $main.Size = New-Object System.Drawing.Size(1024, 720)
        Write-Output 'PASS: Main constructs, resizes, and updates the time cell while preserving the visible header.'
    } finally { $main.Dispose() }
}