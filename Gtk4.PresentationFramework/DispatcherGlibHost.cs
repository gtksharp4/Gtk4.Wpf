using System.Windows.Threading;

namespace System.Windows;

/// <summary>Makes the calling thread's <see cref="Dispatcher"/> the GTK host and gives it nested GLib frames.</summary>
internal static class DispatcherGlibHost
{
	internal static void Attach()
	{
		Dispatcher.AttachHost();
		Dispatcher.HostPushFrame = PushFrame;
	}

	internal static void Detach()
	{
		Dispatcher.DetachHost();
		Dispatcher.HostPushFrame = null;
	}

	static void PushFrame(DispatcherFrame frame)
	{
		var loop = new GLib.MainLoop();
		Action stop = () =>
		{
			if (!frame.Continue)
				loop.Quit();
		};
		frame.ContinueChanged = stop;
		// A quit issued before Run starts would be lost, so re-check once the loop is live.
		GLib.Idle.Add((int)GLib.Priority.Default, () => { stop(); return false; });
		try
		{
			loop.Run();
		}
		finally
		{
			frame.ContinueChanged = null;
		}
	}
}
