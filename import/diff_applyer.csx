#load "./terminal_helper.csx"

if(Directory.Exists("modded_scripts")) {
    Directory.Delete("modded_scripts", true);
}
Directory.CreateDirectory("original_scripts");

foreach (string originalScriptPath in Directory.EnumerateFiles("ufo50_original_scripts", "*.gml", SearchOption.AllDirectories))
{
    var relativeScriptPath = originalScriptPath.Substring("original_scripts/".Length);
    var targetScriptPath = $"original_scripts/{relativeScriptPath}";

    if(!Directory.Exists(targetScriptPath)) {
        Directory.CreateDirectory(Path.GetDirectoryName(targetScriptPath));
    }

    File.Copy(originalScriptPath, targetScriptPath, true);
}

try {
    foreach (string scriptDiffPath in Directory.EnumerateFiles("mod_files/code_diffs", "*.diff", SearchOption.AllDirectories))
    {
        var relativeDiffPath = scriptDiffPath.Substring("mod_files/code_diffs/".Length);
        var relativeScriptPath = relativeDiffPath.Substring(0, relativeDiffPath.Length - 5); // Remove .diff
        var originalScriptPath = $"original_scripts/{relativeScriptPath}";

        var moddedCodeString = await RunTerminalCommand(
            "patch", 
            Path.GetFullPath("."), 
            $"-p0 -i {scriptDiffPath}".Split(' '), 
            [0]
        );
    }
    Directory.Move("original_scripts", "modded_scripts");
} catch {
    if(Directory.Exists("original_scripts")) {
        Directory.Move("original_scripts", "modded_scripts");
    }
    throw;
}

foreach (string moddedScriptPath in Directory.EnumerateFiles("modded_scripts", "*.gml", SearchOption.AllDirectories))
{
    var relativeScriptPath = moddedScriptPath.Substring("modded_scripts/".Length);
    var targetScriptPath = $"ufo50_modded_scripts/{relativeScriptPath}";

    if(!Directory.Exists(targetScriptPath)) {
        Directory.CreateDirectory(Path.GetDirectoryName(targetScriptPath));
    }

    File.Copy(moddedScriptPath, targetScriptPath, true);
}

Directory.Delete("modded_scripts", true);