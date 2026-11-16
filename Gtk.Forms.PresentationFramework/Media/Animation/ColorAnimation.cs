using System;
using System.Windows.Media;

namespace System.Windows.Media.Animation
{
	/// <summary>
	/// Animates a <see cref="Color"/> property between <see cref="From"/> and <see cref="To"/>
	/// using per-channel (A, R, G, B) linear interpolation.
	/// </summary>
	public class ColorAnimation : AnimationTimeline
	{
		/// <summary>Starting colour. Null uses the current property value.</summary>
		public Color? From { get; set; }

		/// <summary>Ending colour. Null holds the starting colour.</summary>
		public Color? To { get; set; }

		/// <inheritdoc/>
		public override object? GetCurrentValue(
			object? defaultOriginValue,
			object? defaultDestinationValue,
			AnimationClock animationClock)
		{
			var from = From ?? (defaultOriginValue is Color c ? c : default(Color));
			var to = To ?? from;

			double t = animationClock.CurrentProgress;

			return Color.FromArgb(
				LerpByte(from.A, to.A, t),
				LerpByte(from.R, to.R, t),
				LerpByte(from.G, to.G, t),
				LerpByte(from.B, to.B, t));
		}

		static byte LerpByte(byte a, byte b, double t) =>
			(byte)Math.Round(a + (b - a) * t);
	}
}
