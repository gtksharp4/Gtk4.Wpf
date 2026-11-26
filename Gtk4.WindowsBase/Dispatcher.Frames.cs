using System;
using System.Collections.Generic;
using System.Threading;

namespace System.Windows.Threading;

public partial class Dispatcher
{
	readonly List<DispatcherFrame> _frames = new List<DispatcherFrame>();
	int _frameDepth;
	volatile bool _exitAllFrames;

	/// <summary>
	/// Runs a native nested loop on the GTK thread until the frame ends. Set by the presentation layer;
	/// it must call <see cref="DispatcherFrame.ContinueChanged"/> consumers' stop path.
	/// </summary>
	internal static Action<DispatcherFrame>? HostPushFrame { get; set; }

	// Number of frames running on this dispatcher.
	internal int FrameDepth => _frameDepth;

	internal bool ExitRequested => _exitAllFrames;

	/// <summary>Pumps the calling thread's dispatcher until <see cref="ExitAllFrames"/> or shutdown.</summary>
	public static void Run() => PushFrame(new DispatcherFrame());

	/// <summary>
	/// Enters an execution loop on the calling thread's dispatcher that ends when <paramref name="frame"/> stops.
	/// On the GTK thread this is a nested GLib loop; on any other thread it pumps that thread's own queue.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="frame"/> is <c>null</c>.</exception>
	/// <exception cref="InvalidOperationException">The dispatcher already shut down.</exception>
	public static void PushFrame(DispatcherFrame frame)
	{
		if (frame == null) throw new ArgumentNullException(nameof(frame));
		CurrentDispatcher.PushFrameCore(frame);
	}

	/// <summary>Asks every frame running on the calling thread's dispatcher to exit.</summary>
	public static void ExitAllFrames()
	{
		var dispatcher = CurrentDispatcher;
		if (dispatcher._frameDepth == 0)
			return;
		dispatcher._exitAllFrames = true;
		dispatcher.WakeFrames();
	}

	void PushFrameCore(DispatcherFrame frame)
	{
		if (HasShutdownFinished)
			throw new InvalidOperationException("Cannot perform requested operation because the Dispatcher shut down.");
		if (!IsHost)
			_everRan = true;
		lock (_queueLock)
		{
			_frames.Add(frame);
			_frameDepth++;
		}
		try
		{
			var native = HostPushFrame;
			if (IsHost && native != null) native(frame);
			else RunQueue(frame);
		}
		finally
		{
			bool outermost;
			lock (_queueLock)
			{
				_frames.Remove(frame);
				outermost = --_frameDepth == 0;
			}
			if (outermost)
			{
				_exitAllFrames = false;
				if (HasShutdownStarted)
					FinishShutdown();
			}
		}
	}

	// Pumps a nested frame until the operation finishes or the timeout elapses.
	internal void WaitInFrame(DispatcherOperation operation, TimeSpan timeout)
	{
		var frame = new OperationFrame(operation);
		operation.Completed += frame.Stop;
		operation.Aborted += frame.Stop;
		Timer? timer = null;
		if (timeout != Timeout.InfiniteTimeSpan)
			timer = new Timer(_ => frame.Continue = false, null, timeout, Timeout.InfiniteTimeSpan);
		try
		{
			if (operation.Status == DispatcherOperationStatus.Pending)
				PushFrameCore(frame);
		}
		finally
		{
			timer?.Dispose();
			operation.Completed -= frame.Stop;
			operation.Aborted -= frame.Stop;
		}
	}

	sealed class OperationFrame : DispatcherFrame
	{
		public OperationFrame(DispatcherOperation operation) : base(false)
		{
		}

		public void Stop(object? sender, EventArgs e) => Continue = false;
	}
}
