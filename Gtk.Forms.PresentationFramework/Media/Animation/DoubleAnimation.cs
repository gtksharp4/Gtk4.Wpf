using System;

namespace System.Windows.Media.Animation
{
	/// <summary>
	/// Animates a <see cref="double"/> property between <see cref="From"/> and <see cref="To"/>
	/// over <see cref="Timeline.Duration"/> using linear interpolation.
	/// When <see cref="From"/> is unset the current/base value is used as the origin.
	/// When <see cref="To"/> is unset <see cref="By"/> is added to the origin; if neither
	/// is set the value holds at <see cref="From"/> (or the base value).
	/// </summary>
	public class DoubleAnimation : AnimationTimeline
	{
		/// <summary>Starting value. Null means use the current property value.</summary>
		public double? From { get; set; }

		/// <summary>Ending value. Null means use the base value + <see cref="By"/>.</summary>
		public double? To { get; set; }

		/// <summary>
		/// Offset added to <see cref="From"/> (or the base value) when <see cref="To"/> is null.
		/// </summary>
		public double? By { get; set; }

		/// <summary>Optional easing function applied to the linear progress.</summary>
		public IEasingFunction? EasingFunction { get; set; }

		/// <inheritdoc/>
		public override object? GetCurrentValue(
			object? defaultOriginValue,
			object? defaultDestinationValue,
			AnimationClock animationClock)
		{
			double from = From ?? (defaultOriginValue is double d ? d : 0.0);
			double to;
			if (To.HasValue)
				to = To.Value;
			else if (By.HasValue)
				to = from + By.Value;
			else
				to = from;

			double t = animationClock.CurrentProgress;
			if (EasingFunction != null)
				t = EasingFunction.Ease(t);

			return from + (to - from) * t;
		}
	}
}
