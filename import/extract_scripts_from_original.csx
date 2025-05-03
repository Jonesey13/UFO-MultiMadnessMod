#r "../lib/UndertaleModLib.dll"
#r "../lib/Underanalyzer.dll"

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

if (Directory.Exists("original_scripts"))
{
    Directory.Delete("original_scripts", true);
}

foreach (string scriptPath in Directory.EnumerateFiles("ufo50_modded_scripts", "*.gml", SearchOption.AllDirectories))
{
    var scriptText = File.ReadAllText(scriptPath);

    string scriptName = Path.GetFileNameWithoutExtension(scriptPath);

    var originalCode = originalScripts.Single(script => script.Name.Content == scriptName);
    var compileSettings = new DecompileSettings();
    compileSettings.CreateEnumDeclarations = true;
    compileSettings.RemoveSingleLineBlockBraces = true;
    compileSettings.EmptyLineBeforeSwitchCases = true;
    compileSettings.EmptyLineAroundBranchStatements = true;
    var codestring = new DecompileContext(context, originalCode, compileSettings).DecompileToString();
    var scriptOutputPath = scriptPath.Substring("ufo50_modded_scripts/".Length);
    var outputFilePath = Path.Join($"original_scripts/{scriptOutputPath}");

    var directoryName = Path.GetDirectoryName(outputFilePath);
    if (!Directory.Exists(directoryName))
    {
        Directory.CreateDirectory(directoryName);
    }
    File.WriteAllText(outputFilePath, codestring);
}

