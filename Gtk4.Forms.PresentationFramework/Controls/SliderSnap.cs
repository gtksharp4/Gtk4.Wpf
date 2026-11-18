using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>
/// Pure, GTK-free helpers for slider tick mathematics.
/// Internal so unit tests can reach them via <c>InternalsVisibleTo</c>.
/// </summary>
internal static class SliderSnap
{
	/// <summary>
	/// Returns all tick positions for the given range and frequency.
	/// Starts at <paramref name="min"/> and steps by <paramref name="frequency"/>;
	/// only positions ≤ <paramref name="max"/> are emitted.
	/// </summary>
	/// <remarks>
	/// Returns an empty sequence when <paramref name="frequency"/> is ≤ 0 or the range is empty.
	/// <paramref name="max"/> is included only when it falls exactly on a tick boundary
	/// (within floating-point tolerance).
	/// </remarks>
	internal static IEnumerable<double> TickPositions(double min, double max, double frequency)
	{
		if (frequency <= 0.0 || max <= min)
			yield break;

		var current = min;
		while (current <= max + (frequency * 1e-10))
		{
			if (current > max) break;
			yield return current;
			current += frequency;
		}
	}

	/// <summary>
	/// Snaps <paramref name="value"/> to the nearest tick at multiples of
	/// <paramref name="frequency"/> relative to <paramref name="min"/>, clamped to
	/// [<paramref name="min"/>, <paramref name="max"/>].
	/// </summary>
	/// <remarks>
	/// Rounding rule: round-half-away-from-zero (0.5 → rounds to 1, −0.5 → rounds to −1).
	/// Returns <paramref name="value"/> unchanged when <paramref name="frequency"/> is ≤ 0
	/// (snap disabled).
	/// </remarks>
	internal static double SnapToTick(double value, double min, double max, double frequency)
	{
		if (frequency <= 0.0)
			return value;

		// Number of whole ticks from min.
		var offset = value - min;
		var steps = offset / frequency;

		// Round-half-away-from-zero (matches the documented rule for the step count,
		// so a negative step of −0.5 rounds to −1 rather than toward +∞).
		var roundedSteps = Math.Round(steps, MidpointRounding.AwayFromZero);
		var snapped = min + roundedSteps * frequency;

		return Math.Max(min, Math.Min(max, snapped));
	}
}
