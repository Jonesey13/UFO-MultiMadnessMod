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

List<string> listOfDiffsTargets = [];

if (Directory.Exists("mod_files/code_diffs")) {
    listOfDiffsTargets = Directory.EnumerateFiles("mod_files/code_diffs", "*.gml.diff", SearchOption.AllDirectories)
        .Select(diffFile => diffFile[..^5]["mod_files/code_diffs/".Length..]).ToList(); // Remove .diff and diff folder prefix
}

List<string> listOfModdedScriptTargets = [];

if (Directory.Exists("ufo50_modded_scripts")) {
    listOfModdedScriptTargets = Directory.EnumerateFiles("ufo50_modded_scripts", "*.gml", SearchOption.AllDirectories)
        .Select(diffFile => diffFile["ufo50_modded_scripts/".Length..]).ToList(); // Remove ufo50_modded_scripts folder prefix
}

var combinedListOfTargets = listOfDiffsTargets.Concat(listOfModdedScriptTargets).Distinct().ToList();

foreach (string scriptPath in combinedListOfTargets)
{
    string scriptName = Path.GetFileNameWithoutExtension(scriptPath);

    var originalCode = originalScripts.SingleOrDefault(script => script.Name.Content == scriptName);

    if (originalCode != null){
        var compileSettings = new DecompileSettings();
        compileSettings.CreateEnumDeclarations = true;
        compileSettings.RemoveSingleLineBlockBraces = true;
        compileSettings.EmptyLineBeforeSwitchCases = true;
        compileSettings.EmptyLineAroundBranchStatements = true;
        var codestring = new DecompileContext(context, originalCode, compileSettings).DecompileToString();
        var outputFilePath = Path.Join($"original_scripts/{scriptPath}");

        var directoryName = Path.GetDirectoryName(outputFilePath);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        File.WriteAllText(outputFilePath, codestring);
    } else {
        Console.WriteLine($"Skipping export of {scriptName} as not found in orginal file");
    }
}

foreach (string originalScriptPath in Directory.EnumerateFiles("original_scripts", "*.gml", SearchOption.AllDirectories))
{
    var relativeScriptPath = originalScriptPath.Substring("original_scripts/".Length);
    var targetScriptPath = $"ufo50_original_scripts/{relativeScriptPath}";

    if(!Directory.Exists(targetScriptPath)) {
        Directory.CreateDirectory(Path.GetDirectoryName(targetScriptPath));
    }

    File.Copy(originalScriptPath, targetScriptPath, true);
}

Directory.Delete("original_scripts", true);
