#r "../lib/UndertaleModLib.dll"
#r "../lib/Underanalyzer.dll"
#load "./terminal_helper.csx"

using UndertaleModLib.Util;
using UndertaleModLib.Models;
using UndertaleModLib;
using UndertaleModLib.Decompiler;
using Underanalyzer.Decompiler;
using System.Text.Json.Serialization;
using System.Text.Json;
using Internal;

var originalDatafile = new FileInfo("input/data.win");
UndertaleData originalGameData;

try
{
    using FileStream fs = originalDatafile.OpenRead();
    UndertaleData gmData = UndertaleIO.Read(fs);
    originalGameData = gmData;
}
catch (FileNotFoundException e)
{
    throw new FileNotFoundException($"Data file '{e.FileName}' does not exist");
}

var originalScripts = originalGameData.Code;

var context = new GlobalDecompileContext(originalGameData);

if (context.GlobalFunctions is null)
{
    GlobalDecompileContext.BuildGlobalFunctionCache(originalGameData);
}

if (Directory.Exists("complete_original_scripts"))
{
    Directory.Delete("complete_original_scripts", true);
}


foreach (UndertaleCode originalCode in originalScripts)
{
    if (originalCode != null && originalCode.ParentEntry == null){
        var compileSettings = new DecompileSettings();
        compileSettings.CreateEnumDeclarations = true;
        compileSettings.RemoveSingleLineBlockBraces = true;
        compileSettings.EmptyLineBeforeSwitchCases = true;
        compileSettings.EmptyLineAroundBranchStatements = true;
        var codestring = new DecompileContext(context, originalCode, compileSettings).DecompileToString();
        var outputFilePath = Path.Join($"complete_original_scripts/{originalCode.Name.Content}.gml");

        var directoryName = Path.GetDirectoryName(outputFilePath);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        File.WriteAllText(outputFilePath, codestring);
    } else {
        Console.WriteLine($"Skipping export of {originalCode} as not a top level code file");
    }
}

await RunTerminalCommand(
    "bash", 
    Path.GetFullPath("."), 
    ["import/sort_scripts.sh"], 
    [0]
);