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


foreach (string moddedScriptPath in Directory.EnumerateFiles("modded_scripts", "*.gml", SearchOption.AllDirectories))
{
    var relativeScriptPath = moddedScriptPath.Substring("modded_scripts/".Length);
    var originalScriptPath = Path.Join($"original_scripts/{relativeScriptPath}");
    var diffScriptPath = Path.Join($"script_diffs/{relativeScriptPath}.diff");

    var diffstring = await BusyBox(
        "diff", 
        Path.GetFullPath("."), 
        $"-a -b -B -d -N -w -U 0 -r {originalScriptPath} {moddedScriptPath}".Split(' '), 
        1
    );

    var directoryName = Path.GetDirectoryName(diffScriptPath);
    if (!Directory.Exists(directoryName)) {
        Directory.CreateDirectory(directoryName);
    }
    File.WriteAllText(diffScriptPath, diffstring);
}