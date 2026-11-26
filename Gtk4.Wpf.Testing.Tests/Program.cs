using System;
using System.Linq;
using Gtk4.Wpf.Testing;

namespace Gtk4.Wpf.Testing.Tests;

/// <summary>
/// Entry point. Windows and Linux run the standard runner and <see cref="GtkThread"/> makes its own thread.
/// macOS (or XF_GTK_MAC_RUNNER=1) needs GTK on thread 0, so the runner moves to a worker.
/// </summary>
static class Program
{
	[STAThread]
	static int Main(string[] args)
	{
		if (args.Any(a => a == "--server" || a == "--internal-msbuild-node"))
			return global::Xunit.MicrosoftTestingPlatform.TestPlatformTestFramework
				.RunAsync(args, SelfRegisteredExtensions.AddSelfRegisteredExtensions)
				.GetAwaiter().GetResult();

		bool threadZero = OperatingSystem.IsMacOS()
			|| Environment.GetEnvironmentVariable("XF_GTK_MAC_RUNNER") == "1";
		if (!threadZero)
			return global::Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args).GetAwaiter().GetResult();

		MacActivity.Begin();
		return GtkThread.RunOnCurrentThread(() =>
			global::Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args).GetAwaiter().GetResult());
	}
}
