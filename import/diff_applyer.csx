#load "./terminal_helper.csx"

if(Directory.Exists("original_scripts")) {
    Directory.Delete("original_scripts", true);
}
Directory.CreateDirectory("original_scripts");

foreach (string scriptDiffPath in Directory.EnumerateFiles("mod_files/code_diffs", "*.diff", SearchOption.AllDirectories))
{
    var relativeDiffPath = scriptDiffPath.Substring("mod_files/code_diffs/".Length);
    var relativeScriptPath = relativeDiffPath.Substring(0, relativeDiffPath.Length - 5); // Remove .diff
    var originalScriptPath = $"original_scripts/{relativeScriptPath}";
    var originalScriptDirPath = Path.GetDirectoryName(originalScriptPath);

    if (!Directory.Exists(originalScriptDirPath)) {
        Directory.CreateDirectory(originalScriptDirPath);
    }

    var sourceScriptPath = $"ufo50_original_scripts/{relativeScriptPath}";
    if (File.Exists(sourceScriptPath)) {
        File.Copy(sourceScriptPath, originalScriptPath, true);
    }

    if (!Path.Exists(originalScriptPath)) {
        File.Create(originalScriptPath);
    }

    var moddedCodeString = await RunTerminalCommand(
        "patch", 
        Path.GetFullPath("."), 
        $"-p0 -F6 -i {scriptDiffPath}".Split(' '), 
        [0]
    );
}

foreach (string moddedScriptPath in Directory.EnumerateFiles("original_scripts", "*.gml", SearchOption.AllDirectories))
{
    var relativeScriptPath = moddedScriptPath.Substring("original_scripts/".Length);
    var targetScriptPath = $"ufo50_modded_scripts/{relativeScriptPath}";

    if(!Directory.Exists(targetScriptPath)) {
        Directory.CreateDirectory(Path.GetDirectoryName(targetScriptPath));
    }

    File.Copy(moddedScriptPath, targetScriptPath, true);
}

Directory.Delete("original_scripts", true);