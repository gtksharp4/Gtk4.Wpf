using System;
using System.Collections.Concurrent;

namespace Xamarin.Forms.Platform.GTK.UnitTests
{
	/// <summary>
	/// Lets a thread-0 runner (see <c>Program.RunOnThreadZero</c>, used on macOS) own the thread GTK
	/// runs on.
	/// </summary>
	/// <remarks>
	/// macOS's only GTK 4 GDK backend is Quartz, which requires GTK - NSWindow creation and main-loop
	/// polling alike - to run on thread 0, the process's first thread. Under the normal path
	/// <see cref="GtkFixture"/> runs GTK on a background thread, which macOS aborts the moment a
	/// toplevel is realised. When <see cref="Queue"/> is set, <see cref="GtkFixture"/> marshals every
	/// test body onto it instead of spawning its own thread, and the runner drains it on thread 0
	/// through <see cref="RunLoop"/>. On Windows and Linux the queue stays null and the fixture keeps
	/// its background-thread behaviour unchanged.
	/// </remarks>
	static class GtkThreadBridge
	{
		/// <summary>Set by the thread-0 runner; null under <c>dotnet test</c> and direct console runs.</summary>
		public static BlockingCollection<Action> Queue;

		/// <summary>
		/// Consumes queued GTK work on the calling thread (thread 0) until the queue is completed.
		/// </summary>
		public static void RunLoop()
		{
			foreach (var work in Queue.GetConsumingEnumerable())
			{
				// Each item already captures its own body's exceptions (GtkFixture.Invoke) and releases
				// its waiter in a finally, so a throw out here is a stray one - a native callback from
				// an async op a skipped test left pending, firing during a later pump. Swallow it so one
				// does not abort the whole run.
				try
				{
					work();
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("MacRunner: ignored a stray GTK-thread exception: " + e.Message);
				}
			}
		}
	}
}
