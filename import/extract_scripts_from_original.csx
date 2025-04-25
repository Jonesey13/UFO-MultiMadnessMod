#r ".\lib\UndertaleModLib.dll"
#r ".\lib\Underanalyzer.dll"

using UndertaleModLib.Util;
using UndertaleModLib.Models;
using UndertaleModLib;
using UndertaleModLib.Decompiler;
using System.Text.Json.Serialization;
using System.Text.Json;

var originalDatafile = new FileInfo("data.win");
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

foreach (string scriptPath in Directory.EnumerateFiles("modded_scripts", "*.gml", SearchOption.AllDirectories))
{
    var scriptText = File.ReadAllText(scriptPath);

    string scriptName = Path.GetFileNameWithoutExtension(scriptPath);

    var originalCode = originalScripts.Single( script => script.Name.Content == scriptName);
    var codestring = new Underanalyzer.Decompiler.DecompileContext(context, originalCode).DecompileToString();
    var scriptOutputPath = scriptPath.Substring("modded_scripts/".Length);
    var outputFilePath = Path.Join($"original_scripts/{scriptOutputPath}");
    
    var directoryName = Path.GetDirectoryName(outputFilePath);
    if (!Directory.Exists(directoryName)) {
        Directory.CreateDirectory(directoryName);
    }
    File.WriteAllText(outputFilePath, codestring);
}

