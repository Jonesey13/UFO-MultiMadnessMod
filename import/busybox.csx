using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

string PathResolve(params string[] components) {
    return Path.GetFullPath(Path.Combine(components));
}

// BusyBox builds and source available here: https://frippery.org/busybox/index.html
async Task<string> BusyBox(string applet, string workdir, string[] args, int expectedExitCode = 0) {
    var startInfo = new ProcessStartInfo { 
        FileName = PathResolve("./import/lib", "busybox.exe"),
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        WorkingDirectory = workdir
    };

    startInfo.ArgumentList.Add(applet);
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

        if(p.ExitCode != expectedExitCode) {
            throw new Exception($"busybox '{applet}' exited with code {p.ExitCode}:\n\nargs:{String.Join(' ', args)}\n\n{err}");
        }

        return res;
    }
}