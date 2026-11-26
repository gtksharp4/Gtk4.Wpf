using System;
using System.Diagnostics;
using System.Text;

namespace Gtk4.Integration.Tests;

/// <summary>Runs the <c>dotnet</c> CLI and captures its combined output.</summary>
static class DotnetCli
{
	public readonly record struct Result(int ExitCode, string Output);

	public static Result Run(string arguments, string workingDir, TimeSpan timeout)
	{
		var psi = new ProcessStartInfo("dotnet", arguments)
		{
			WorkingDirectory = workingDir,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
		};

		var output = new StringBuilder();
		using var p = new Process { StartInfo = psi };
		p.OutputDataReceived += (_, e) => { if (e.Data != null) lock (output) output.AppendLine(e.Data); };
		p.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (output) output.AppendLine(e.Data); };

		p.Start();
		p.BeginOutputReadLine();
		p.BeginErrorReadLine();

		if (!p.WaitForExit((int)timeout.TotalMilliseconds))
		{
			try { p.Kill(entireProcessTree: true); } catch { /* already gone */ }
			return new Result(-1, output + Environment.NewLine + "[timed out]");
		}

		p.WaitForExit(); // ensure the async output readers have flushed
		return new Result(p.ExitCode, output.ToString());
	}
}