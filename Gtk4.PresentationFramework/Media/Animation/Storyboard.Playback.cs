using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Media.Animation;

public partial class Storyboard
{
	sealed class Run
	{
		internal ClockGroup Root = null!;
		internal bool Controllable;
		internal readonly List<AnimationClock> Leaves = new List<AnimationClock>();
	}

	readonly Dictionary<FrameworkElement, Run> _runs = new Dictionary<FrameworkElement, Run>();

	/// <summary>Starts the storyboard on <paramref name="containingObject"/> (not controllable).</summary>
	public void Begin(FrameworkElement containingObject) =>
		Begin(containingObject, HandoffBehavior.SnapshotAndReplace, false);

	/// <summary>Starts the storyboard; <paramref name="isControllable"/> allows later Stop/Pause/Seek.</summary>
	public void Begin(FrameworkElement containingObject, bool isControllable) =>
		Begin(containingObject, HandoffBehavior.SnapshotAndReplace, isControllable);

	/// <summary>Starts the storyboard with the given hand-off behaviour.</summary>
	public void Begin(FrameworkElement containingObject, HandoffBehavior handoffBehavior) =>
		Begin(containingObject, handoffBehavior, false);

	/// <summary>Starts the storyboard with the given hand-off behaviour and control.</summary>
	public void Begin(FrameworkElement containingObject, HandoffBehavior handoffBehavior, bool isControllable)
	{
		if (containingObject == null) throw new ArgumentNullException(nameof(containingObject));
		if (handoffBehavior != HandoffBehavior.SnapshotAndReplace && handoffBehavior != HandoffBehavior.Compose)
			throw new ArgumentException("Invalid HandoffBehavior value.", nameof(handoffBehavior));
		if (_runs.TryGetValue(containingObject, out var previous))
		{
			previous.Root.RemoveClock();
			_runs.Remove(containingObject);
		}
		AdoptContext(containingObject);
		var run = new Run { Root = (ClockGroup)CreateClock(isControllable), Controllable = isControllable };
		BindClocks(run.Root, containingObject, (string?)GetValue(TargetNameProperty),
			(DependencyObject?)GetValue(TargetProperty), (PropertyPath?)GetValue(TargetPropertyProperty),
			handoffBehavior, run.Leaves);
		_runs[containingObject] = run;
		AnimationScheduler.Register(run.Root);
	}

	ClockController? Controller(FrameworkElement element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (!_runs.TryGetValue(element, out var run)) return null;
		if (run.Controllable) return run.Root.Controller;
		TraceNotControllable("control");
		return null;
	}

	Clock? ClockOf(FrameworkElement element) => Controller(element)?.Clock;

	/// <summary>Stops the storyboard started on <paramref name="containingObject"/>.</summary>
	public void Stop(FrameworkElement containingObject) => Controller(containingObject)?.Stop();

	/// <summary>Pauses the storyboard started on <paramref name="containingObject"/>.</summary>
	public void Pause(FrameworkElement containingObject) => Controller(containingObject)?.Pause();

	/// <summary>Resumes the storyboard started on <paramref name="containingObject"/>.</summary>
	public void Resume(FrameworkElement containingObject) => Controller(containingObject)?.Resume();

	/// <summary>Removes the storyboard started on <paramref name="containingObject"/>.</summary>
	public void Remove(FrameworkElement containingObject)
	{
		if (Controller(containingObject) is { } c)
		{
			c.Remove();
			_runs.Remove(containingObject);
		}
	}

	/// <summary>Moves the storyboard to its fill period.</summary>
	public void SkipToFill(FrameworkElement containingObject) => Controller(containingObject)?.SkipToFill();

	/// <summary>Seeks the storyboard.</summary>
	public void Seek(FrameworkElement containingObject, TimeSpan offset, TimeSeekOrigin origin) =>
		Controller(containingObject)?.Seek(offset, origin);

	/// <summary>Seeks the storyboard on the last tick.</summary>
	public void SeekAlignedToLastTick(FrameworkElement containingObject, TimeSpan offset, TimeSeekOrigin origin) =>
		Controller(containingObject)?.SeekAlignedToLastTick(offset, origin);

	/// <summary>Sets the storyboard speed.</summary>
	public void SetSpeedRatio(FrameworkElement containingObject, double speedRatio)
	{
		if (Controller(containingObject) is { } c) c.SpeedRatio = speedRatio;
	}

	/// <summary>The current time of the controllable storyboard, or null.</summary>
	public TimeSpan? GetCurrentTime(FrameworkElement containingObject) => ClockOf(containingObject)?.CurrentTime;

	/// <summary>The current state of the controllable storyboard, or null.</summary>
	public ClockState? GetCurrentState(FrameworkElement containingObject) => ClockOf(containingObject)?.CurrentState;

	/// <summary>The current progress of the controllable storyboard, or null.</summary>
	public double? GetCurrentProgress(FrameworkElement containingObject) =>
		ClockOf(containingObject)?.CurrentProgress;

	/// <summary>The current iteration of the controllable storyboard, or null.</summary>
	public int? GetCurrentIteration(FrameworkElement containingObject) =>
		ClockOf(containingObject)?.CurrentIteration;

	/// <summary>Whether the controllable storyboard is paused; false when not begun.</summary>
	public bool GetIsPaused(FrameworkElement containingObject) => ClockOf(containingObject)?.IsPaused ?? false;

	// Parameterless forms predate the WPF API: they act on every run, controllable or not.

	/// <summary>Stops and removes every run of this storyboard.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Stop()
	{
		foreach (var run in _runs.Values) run.Root.RemoveClock();
		_runs.Clear();
	}

	/// <summary>Pauses every run.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Pause()
	{
		foreach (var run in _runs.Values) run.Root.SetPaused(true);
	}

	/// <summary>Resumes every run.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Resume()
	{
		foreach (var run in _runs.Values) run.Root.SetPaused(false);
	}

	/// <summary>The animation clocks of the runs, so tests can tick them without GLib.</summary>
	internal IReadOnlyList<AnimationClock> ActiveClocks
	{
		get
		{
			var all = new List<AnimationClock>();
			foreach (var run in _runs.Values) all.AddRange(run.Leaves);
			return all;
		}
	}
}
