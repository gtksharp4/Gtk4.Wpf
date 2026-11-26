using System;

namespace System.Windows.Threading;

/// <summary>Represents an execution loop pushed onto a <see cref="Dispatcher"/> by <c>PushFrame</c>.</summary>
public class DispatcherFrame : DispatcherObject
{
	readonly bool _exitWhenRequested;
	volatile bool _continue = true;

	/// <summary>Creates a frame that also exits on <see cref="Dispatcher.ExitAllFrames"/> and on shutdown.</summary>
	public DispatcherFrame() : this(true)
	{
	}

	/// <summary>Creates a frame; with <paramref name="exitWhenRequested"/> false only <see cref="Continue"/> ends it.</summary>
	public DispatcherFrame(bool exitWhenRequested)
	{
		_exitWhenRequested = exitWhenRequested;
	}

	// The host bridge hooks this to stop its native loop when the frame ends.
	internal Action? ContinueChanged { get; set; }

	/// <summary>Whether the frame keeps running. Setting it to <c>false</c> ends the frame.</summary>
	public bool Continue
	{
		get => GetContinue();
		set
		{
			_continue = value;
			Dispatcher.WakeFrames();
		}
	}

	/// <summary>Returns whether the frame should keep running.</summary>
	protected virtual bool GetContinue()
	{
		if (_exitWhenRequested && (Dispatcher.ExitRequested || Dispatcher.HasShutdownStarted))
			_continue = false;
		return _continue;
	}

	internal void NotifyContinueChanged() => ContinueChanged?.Invoke();
}
