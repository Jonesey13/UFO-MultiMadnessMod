#load "./busybox.csx"

#r ".\lib\UndertaleModLib.dll"
#r ".\lib\Underanalyzer.dll"

using UndertaleModLib.Util;
using UndertaleModLib.Models;
using UndertaleModLib;
using UndertaleModLib.Decompiler;
using System.Text.Json.Serialization;
using System.Text.Json;
using Internal;

foreach (string scriptDiffPath in Directory.EnumerateFiles("script_diffs", "*.diff", SearchOption.AllDirectories))
{
    var relativeDiffPath = scriptDiffPath.Substring("script_diffs/".Length);
    var relativeScriptPath = Path.GetFileNameWithoutExtension(relativeDiffPath);
    var originalScriptPath = Path.Join($"original_scripts/{relativeScriptPath}");
    var moddedScriptPath = Path.Join($"modded_scripts/{relativeScriptPath}");

    var directoryName = Path.GetDirectoryName(moddedScriptPath);
    if (!Directory.Exists(directoryName)) {
        Directory.CreateDirectory(directoryName);
    }

    var scriptText = File.ReadAllText(originalScriptPath);
    File.WriteAllText(moddedScriptPath, scriptText);

    var moddedCodeString = await BusyBox(
        "patch", 
        Path.GetFullPath("."), 
        $"-R --dry-run -i {scriptDiffPath}".Split(' '), 
        0
    );
}