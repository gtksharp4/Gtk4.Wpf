using System;
using System.Windows.Threading;

namespace System.Windows;

public partial class Application
{
	/// <summary>
	/// Maps a <see cref="DispatcherPriority"/> to a GLib source priority (lower runs first).
	/// <code>
	/// Send            -40   above events: runs before everything else
	/// Normal          -30
	/// DataBind        -20
	/// Render          -10
	/// Loaded           -5   Loaded and above are always at or above G_PRIORITY_DEFAULT
	/// Input             0   G_PRIORITY_DEFAULT (GDK events)
	/// Background      130   after GDK_PRIORITY_REDRAW (GTK layout/paint)
	/// ContextIdle     150
	/// ApplicationIdle 200   G_PRIORITY_DEFAULT_IDLE
	/// SystemIdle      300   G_PRIORITY_LOW
	/// </code>
	/// Idle priorities are reserved for Background and below: on macOS the Quartz event source and
	/// frame clock starve idle sources, so Loaded and above must never be mapped there.
	/// </summary>
	const int GdkPriorityRedraw = (int)GLib.Priority.HighIdle + 20;

	internal static int ToGLibPriority(DispatcherPriority priority) => priority switch
	{
		DispatcherPriority.Send => -40,
		DispatcherPriority.Normal => -30,
		DispatcherPriority.DataBind => -20,
		DispatcherPriority.Render => -10,
		DispatcherPriority.Loaded => -5,
		DispatcherPriority.Input => (int)GLib.Priority.Default,
		DispatcherPriority.Background => GdkPriorityRedraw + 10,
		DispatcherPriority.ContextIdle => 150,
		DispatcherPriority.ApplicationIdle => (int)GLib.Priority.DefaultIdle,
		DispatcherPriority.SystemIdle => (int)GLib.Priority.Low,
		_ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Priority is not processable."),
	};

	/// <summary>Posts dispatcher work to the GLib main loop at the mapped GLib priority.</summary>
	internal static void PostToMainLoop(Action action, DispatcherPriority priority) =>
		GLib.Idle.Add(ToGLibPriority(priority), () => { action(); return false; });
}
