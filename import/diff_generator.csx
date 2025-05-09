#load "./terminal_helper.csx"

if(Directory.Exists("mod_files/code_diffs")) {
    Directory.Delete("mod_files/code_diffs", true);
}

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
    File.WriteAllText(diffScriptPath, diffstring);
}