using System.Collections.Generic;
using System.Windows;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Resolved by <see cref="Application.StartupUri"/> in the startup tests.</summary>
public class StartupProbeWindow : Window
{
}

/// <summary>Application that records startup order and Exit events for lifecycle tests.</summary>
class RecordingApp : Application
{
	public List<string> Log { get; } = new List<string>();
	public bool SkipBase { get; set; }
	public int ExitRaised { get; private set; }
	public int ExitCodeSeen { get; private set; }

	public RecordingApp()
	{
		Startup += (_, e) => Log.Add("event:" + string.Join(",", e.Args));
		Exit += (_, e) => { ExitRaised++; ExitCodeSeen = e.ApplicationExitCode; };
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		Log.Add("override");
		if (!SkipBase)
			base.OnStartup(e);
	}
}
