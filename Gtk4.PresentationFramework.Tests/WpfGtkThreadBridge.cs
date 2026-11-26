using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Lets a thread-0 runner (macOS) own the GTK thread. Mirrors GtkThreadBridge.
/// </summary>
static class WpfGtkThreadBridge
{
	/// <summary>Set by thread-0 runner; null on Windows/Linux where a background thread is used.</summary>
	public static BlockingCollection<Action>? Queue;

	static long _lastProgress = Environment.TickCount64;

	/// <summary>Milliseconds since a work item last started or finished on thread 0.</summary>
	public static long IdleMilliseconds => Environment.TickCount64 - Interlocked.Read(ref _lastProgress);

	/// <summary>Drains the queue on the calling thread (thread 0) until it is completed.</summary>
	public static void RunLoop()
	{
		foreach (var work in Queue!.GetConsumingEnumerable())
		{
			Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
			try { work(); }
			catch (Exception e)
			{
				Console.Error.WriteLine(
					"MacRunner: ignored a stray GTK-thread exception: " + e.Message);
			}
			finally { Interlocked.Exchange(ref _lastProgress, Environment.TickCount64); }
		}
	}
}