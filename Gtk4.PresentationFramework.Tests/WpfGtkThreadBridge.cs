using System;
using System.Collections.Concurrent;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Lets a thread-0 runner (macOS) own the GTK thread. Mirrors GtkThreadBridge.
/// </summary>
static class WpfGtkThreadBridge
{
	/// <summary>Set by thread-0 runner; null on Windows/Linux where a background thread is used.</summary>
	public static BlockingCollection<Action>? Queue;

	/// <summary>Drains the queue on the calling thread (thread 0) until it is completed.</summary>
	public static void RunLoop()
	{
		foreach (var work in Queue!.GetConsumingEnumerable())
		{
			try { work(); }
			catch (Exception e)
			{
				Console.Error.WriteLine(
					"MacRunner: ignored a stray GTK-thread exception: " + e.Message);
			}
		}
	}
}