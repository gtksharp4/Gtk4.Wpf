using System;
using System.Threading;
using System.Windows.Threading;

namespace Gtk4.WindowsBase.Tests;

/// <summary>A background thread running <see cref="Dispatcher.Run"/> on its own dispatcher.</summary>
sealed class DispatcherPump
{
	public Thread Thread { get; }
	public Dispatcher Dispatcher { get; private set; } = null!;
	public Exception? RunException { get; private set; }

	public DispatcherPump()
	{
		var ready = new ManualResetEventSlim();
		Thread = new Thread(() =>
		{
			Dispatcher = Dispatcher.CurrentDispatcher;
			ready.Set();
			try { Dispatcher.Run(); }
			catch (Exception ex) { RunException = ex; }
		}) { IsBackground = true };
		Thread.Start();
		ready.Wait();
		SpinWait.SpinUntil(() => Dispatcher.FrameDepth > 0, 5000);
	}

	public void Stop()
	{
		Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
		Thread.Join(5000);
	}
}
