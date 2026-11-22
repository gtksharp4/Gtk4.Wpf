using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Media.Animation;

/// <summary>
/// A <see cref="Clock"/> that produces animated values for one <see cref="AnimationTimeline"/>
/// and writes them to the properties it is applied to.
/// </summary>
public sealed class AnimationClock : Clock
{
	readonly List<(DependencyObject Target, DependencyProperty Property)> _attachments =
		new List<(DependencyObject, DependencyProperty)>();

	internal AnimationClock(AnimationTimeline timeline) : base(timeline) { }

	/// <summary>Creates a clock applied to <paramref name="target"/>'s <paramref name="property"/>.</summary>
	internal AnimationClock(AnimationTimeline timeline, DependencyObject target, DependencyProperty property)
		: base(timeline)
	{
		InitializeRoot(false);
		AnimationStore.AttachClock(target, property, this);
	}

	/// <summary>The animation timeline this clock runs.</summary>
	public new AnimationTimeline Timeline => (AnimationTimeline)base.Timeline;

	/// <summary>The value taken from a snapshot when this clock replaced others; see <see cref="HasSnapshot"/>.</summary>
	internal object? SnapshotValue { get; private set; }

	internal bool HasSnapshot { get; private set; }

	/// <summary>True once the clock is running (or filling) and so contributes a value.</summary>
	internal bool HasValue => CurrentState != ClockState.Stopped && CurrentTime != null;

	/// <summary>Computes the animated value for this clock's current progress.</summary>
	public object? GetCurrentValue(object? defaultOriginValue, object? defaultDestinationValue) =>
		Timeline.GetCurrentValue(defaultOriginValue, defaultDestinationValue, this);

	internal void SetSnapshot(object? value)
	{
		SnapshotValue = value;
		HasSnapshot = true;
	}

	internal void AddAttachment(DependencyObject target, DependencyProperty dp) => _attachments.Add((target, dp));

	internal void RemoveAttachment(DependencyObject target, DependencyProperty dp) =>
		_attachments.RemoveAll(a => ReferenceEquals(a.Target, target) && a.Property == dp);

	internal override void OnEvaluated(TimeSpan childTime)
	{
		foreach (var (target, property) in _attachments.ToArray())
			AnimationStore.Refresh(target, property);
	}

	internal override void RemoveClock()
	{
		foreach (var (target, property) in _attachments.ToArray())
			AnimationStore.Detach(target, property, this);
		base.RemoveClock();
	}

	/// <summary>Pauses the clock.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Pause() => SetPaused(true);

	/// <summary>Resumes the clock.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Resume() => SetPaused(false);

	/// <summary>Stops the clock and clears its animated values.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Stop() => StopClock();
}
