#load "./terminal_helper.csx"

using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

if(!Directory.Exists("original_scripts")) {
    Directory.CreateDirectory("original_scripts");
}

foreach (string originalScriptPath in Directory.EnumerateFiles("ufo50_original_scripts", "*.gml", SearchOption.AllDirectories))
{
    var relativeScriptPath = originalScriptPath.Substring("ufo50_original_scripts/".Length);
    var targetScriptPath = $"original_scripts/{relativeScriptPath}";

    if(!Directory.Exists(targetScriptPath)) {
        Directory.CreateDirectory(Path.GetDirectoryName(targetScriptPath));
    }

    File.Copy(originalScriptPath, targetScriptPath, true);
}


foreach (string moddedScriptPath in Directory.EnumerateFiles("ufo50_modded_scripts", "*.gml", SearchOption.AllDirectories))
{
    var relativeScriptPath = moddedScriptPath.Substring("ufo50_modded_scripts/".Length);
    var originalScriptPath = Path.Join($"original_scripts/{relativeScriptPath}");
    var diffScriptPath = Path.Join($"mod_files/code_diffs/{relativeScriptPath}.diff");

    var diffstring = await RunTerminalCommand(
        "diff", 
        Path.GetFullPath("."), 
        $"-C1 -N {originalScriptPath} {moddedScriptPath}".Split(' '), 
        [0, 1]
    );

    var directoryName = Path.GetDirectoryName(diffScriptPath);
    if (!Directory.Exists(directoryName)) {
        Directory.CreateDirectory(directoryName);
    }

    var skipWrite = false;

    // Don't update the diff if only the dates have changed
    if (File.Exists(diffScriptPath)) {
        skipWrite = true;
        var originalDiffContent = File.ReadAllLines(diffScriptPath).Skip(2).ToList();
        var newDiffContent = diffstring.Split(Environment.NewLine).Skip(2).ToList();

        if (originalDiffContent.Count() != newDiffContent.Count() - 1) {
            skipWrite = false;
        } else {
            foreach (var (first, second) in originalDiffContent.Zip(newDiffContent)){
                if (first != second)
                {
                    skipWrite = false;
                    break;
                }
            }
        }
    }

    if(!skipWrite) {
        File.WriteAllText(diffScriptPath, diffstring);
    }
}