using System.Diagnostics;

string PathResolve(params string[] components) {
    return Path.GetFullPath(Path.Combine(components));
}

async Task<string> RunTerminalCommand(string applet, string workdir, string[] args, int[] expectedExitCodes) {
    var startInfo = new ProcessStartInfo { 
        FileName = applet,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        WorkingDirectory = workdir
    };

    Array.ForEach(args, arg => startInfo.ArgumentList.Add(arg));

    using(Process p = Process.Start(startInfo)) {
        string res = null;
        string err = null;

        var tasks = new List<Task>();
        tasks.Add(Task.Run(() => res = p.StandardOutput.ReadToEnd()));
        tasks.Add(Task.Run(() => err = p.StandardError.ReadToEnd()));
        tasks.Add(Task.Run(() => p.WaitForExit()));
        p.StandardInput.Close();
        await Task.WhenAll(tasks);

        if(!expectedExitCodes.Contains(p.ExitCode!)) {
            throw new Exception($"Terminal command '{applet}' exited with code {p.ExitCode}:\n\nargs:{String.Join(' ', args)}\n\n{err}");
        }

        return res;
    }
}