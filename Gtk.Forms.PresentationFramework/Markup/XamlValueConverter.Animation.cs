using System;
using System.Globalization;
using System.Windows.Media.Animation;

namespace System.Windows.Markup;

/// <summary>
/// Converters for <see cref="Duration"/> and <see cref="RepeatBehavior"/>.
/// </summary>
static class AnimationConverters
{
	// ── Duration ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Parses a WPF <c>Duration</c> string.
	/// <list type="bullet">
	/// <item><c>"Forever"</c> → <see cref="Duration.Forever"/></item>
	/// <item><c>"Automatic"</c> → <see cref="Duration.Automatic"/></item>
	/// <item>Any valid <c>TimeSpan</c> string (e.g. <c>"0:0:1"</c>, <c>"00:00:01.5"</c>)
	/// → finite duration.</item>
	/// </list>
	/// </summary>
	internal static Duration ParseDuration(string raw)
	{
		string trimmed = raw.Trim();

		if (string.Equals(trimmed, "Forever", StringComparison.OrdinalIgnoreCase))
			return Duration.Forever;
		if (string.Equals(trimmed, "Automatic", StringComparison.OrdinalIgnoreCase))
			return Duration.Automatic;

		if (TimeSpan.TryParse(trimmed, CultureInfo.InvariantCulture, out TimeSpan ts))
			return new Duration(ts);

		throw new FormatException(
			$"Cannot parse Duration from \"{raw}\". " +
			"Expected \"Forever\", \"Automatic\", or a TimeSpan string such as \"0:0:1\".");
	}

	// ── RepeatBehavior ───────────────────────────────────────────────────────

	/// <summary>
	/// Parses a WPF <c>RepeatBehavior</c> string.
	/// <list type="bullet">
	/// <item><c>"Forever"</c> → <see cref="RepeatBehavior.Forever"/></item>
	/// <item><c>"3x"</c> / <c>"2.5x"</c> → count-based repeat</item>
	/// <item>Any valid <c>TimeSpan</c> string → duration-based repeat</item>
	/// </list>
	/// </summary>
	internal static RepeatBehavior ParseRepeatBehavior(string raw)
	{
		string trimmed = raw.Trim();

		if (string.Equals(trimmed, "Forever", StringComparison.OrdinalIgnoreCase))
			return RepeatBehavior.Forever;

		// Count form: "3x", "2.5x" (case-insensitive x suffix).
		if (trimmed.EndsWith("x", StringComparison.OrdinalIgnoreCase))
		{
			string countPart = trimmed.Substring(0, trimmed.Length - 1);
			if (double.TryParse(countPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double count))
				return new RepeatBehavior(count);
		}

		// Duration form.
		if (TimeSpan.TryParse(trimmed, CultureInfo.InvariantCulture, out TimeSpan ts))
			return new RepeatBehavior(ts);

		throw new FormatException(
			$"Cannot parse RepeatBehavior from \"{raw}\". " +
			"Expected \"Forever\", a count like \"3x\", or a TimeSpan string.");
	}
}