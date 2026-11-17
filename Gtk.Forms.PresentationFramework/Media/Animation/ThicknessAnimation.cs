using System;

namespace System.Windows.Media.Animation;

/// <summary>
/// Animates a <see cref="Thickness"/> property between <see cref="From"/> and <see cref="To"/>
/// using per-side linear interpolation.
/// </summary>
public class ThicknessAnimation : AnimationTimeline
{
	/// <summary>Starting thickness. Null uses the current property value.</summary>
	public Thickness? From { get; set; }

	/// <summary>Ending thickness. Null holds the starting thickness.</summary>
	public Thickness? To { get; set; }

	/// <inheritdoc/>
	public override object? GetCurrentValue(
		object? defaultOriginValue,
		object? defaultDestinationValue,
		AnimationClock animationClock)
	{
		var from = From ?? (defaultOriginValue is Thickness th ? th : default(Thickness));
		var to = To ?? from;

		double t = animationClock.CurrentProgress;

		return new Thickness(
			Lerp(from.Left, to.Left, t),
			Lerp(from.Top, to.Top, t),
			Lerp(from.Right, to.Right, t),
			Lerp(from.Bottom, to.Bottom, t));
	}

	static double Lerp(double a, double b, double t) => a + (b - a) * t;
}