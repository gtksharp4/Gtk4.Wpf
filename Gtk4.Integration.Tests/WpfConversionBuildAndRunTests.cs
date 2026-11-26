using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;

namespace Gtk4.Integration.Tests;

/// <summary>
/// End-to-end: take the embedded WPF sample solution, add the <c>Gtk4.Wpf</c> package
/// reference, build the solution (the package converts the project on the first build), run the
/// produced executable, and kill it one second after its MainWindow appears.
/// </summary>
public class WpfConversionBuildAndRunTests
{
	[Fact]
	public void Wpf_sample_builds_and_runs_on_Gtk_Forms()
	{
		if (!OperatingSystem.IsWindows())
			Assert.Skip("Window detection is implemented for Windows only.");

		string? packagesDir = PackageFeed.FindPackagesDirectory();
		if (packagesDir == null)
			Assert.Skip("Repository Packages folder not found.");

		string? version = PackageFeed.LatestVersion(packagesDir, "Gtk4.Wpf");
		if (version == null)
			Assert.Skip("Gtk4.Wpf package not in the Packages folder; build the solution first.");

		if (!GtkRuntime.IsAvailable())
			Assert.Skip("Native GTK 4 runtime not found on PATH.");

		using var ws = SampleWorkspace.Create();
		ws.UseGtkFormsWpf(packagesDir, version);

		BuildSolution(ws);
		RunAndKillAfterWindow(FindExecutable(ws.Root));
	}

	/// <summary>
	/// The Gtk4.Wpf converter rewrites the project on the first build and stops it with a
	/// "re-run the build" error; the second build compiles the converted project. Try twice.
	/// </summary>
	static void BuildSolution(SampleWorkspace ws)
	{
		DotnetCli.Result build = default;
		for (int attempt = 0; attempt < 2; attempt++)
		{
			build = DotnetCli.Run($"build \"{ws.SolutionPath}\" -c Debug", ws.Root, TimeSpan.FromMinutes(5));
			if (build.ExitCode == 0)
				return;
		}
		Assert.Fail($"Build did not succeed after conversion:{Environment.NewLine}{build.Output}");
	}

	static void RunAndKillAfterWindow(string exePath)
	{
		using var app = Process.Start(new ProcessStartInfo(exePath)
		{
			WorkingDirectory = Path.GetDirectoryName(exePath)!,
			UseShellExecute = false,
		})!;

		try
		{
			// The IsWindows() guard both satisfies the platform analyzer and mirrors the skip
			// at the top of the test (window detection is Windows-only).
			bool shown = OperatingSystem.IsWindows()
			             && NativeWindowProbe.WaitForWindow(app, "MainWindow", TimeSpan.FromSeconds(30));
			Assert.True(shown, "The MainWindow was not shown within the timeout.");

			Thread.Sleep(1000); // kill one second after the window appears
		}
		finally
		{
			try { if (!app.HasExited) app.Kill(entireProcessTree: true); } catch { /* already gone */ }
		}
	}

	static string FindExecutable(string root)
	{
		string binDir = Path.Combine(root, "WpfAppToConvert", "bin");
		string[] hits = Directory.Exists(binDir)
			? Directory.GetFiles(binDir, "WpfAppToConvert.exe", SearchOption.AllDirectories)
			: Array.Empty<string>();

		Assert.True(hits.Length > 0, "Built WpfAppToConvert.exe was not found under bin/.");
		return hits[0];
	}
}