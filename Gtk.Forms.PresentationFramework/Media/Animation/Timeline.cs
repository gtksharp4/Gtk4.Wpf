using System;

namespace System.Windows.Media.Animation
{
	/// <summary>
	/// Base class for all animation timeline types.
	/// Mirrors WPF's <c>System.Windows.Media.Animation.Timeline</c>.
	/// </summary>
	public abstract class Timeline
	{
		/// <summary>Duration of the active period. Defaults to <see cref="Duration.Automatic"/>.</summary>
		public Duration Duration { get; set; } = Duration.Automatic;

		/// <summary>Time to wait before the timeline starts. Null means start immediately.</summary>
		public TimeSpan? BeginTime { get; set; }

		/// <summary>Whether the timeline plays backwards after completing. Default: false.</summary>
		public bool AutoReverse { get; set; }

		/// <summary>Specifies how many times to repeat. Default: once (count=1).</summary>
		public RepeatBehavior RepeatBehavior { get; set; } = new RepeatBehavior(1);

		/// <summary>What happens when the timeline's active period ends. Default: HoldEnd.</summary>
		public FillBehavior FillBehavior { get; set; } = FillBehavior.HoldEnd;
	}

	/// <summary>
	/// Base class for timelines that animate a property value.
	/// Subclasses provide interpolation via <see cref="GetCurrentValue"/>.
	/// </summary>
	public abstract class AnimationTimeline : Timeline
	{
		/// <summary>
		/// Computes the animated value at the given normalised progress in [0,1].
		/// <paramref name="defaultOriginValue"/> is the base (pre-animation) value.
		/// </summary>
		/// <param name="defaultOriginValue">Base value used as the From when From is not set.</param>
		/// <param name="defaultDestinationValue">Destination used as the To when To is not set.</param>
		/// <param name="animationClock">The clock driving this animation (unused in base).</param>
		/// <returns>Interpolated value.</returns>
		public abstract object? GetCurrentValue(
			object? defaultOriginValue,
			object? defaultDestinationValue,
			AnimationClock animationClock);
	}
}
