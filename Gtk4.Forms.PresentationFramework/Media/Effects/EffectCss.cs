using System.Globalization;

namespace System.Windows.Media.Effects;

/// <summary>
/// Converts a <see cref="Effect"/> to a CSS <c>filter</c> value string.
/// All methods are pure and have no GTK dependency — fully unit-testable headless.
/// <para>
/// <b>Direction convention:</b> WPF measures <c>Direction</c> in degrees,
/// counter-clockwise from the positive X-axis (0° = right, 90° = up).
/// Screen space has Y increasing downward, so dy = −depth·sin(dir).
/// </para>
/// </summary>
internal static class EffectCss
{
	static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

	static string F(double v) => v.ToString("G6", Inv);

	/// <summary>
	/// Builds the CSS <c>filter</c> value for <paramref name="effect"/>.
	/// Returns <c>"none"</c> for a <c>null</c> effect.
	/// </summary>
	/// <param name="effect">The effect to convert, or <c>null</c>.</param>
	/// <returns>
	/// A CSS filter function string such as <c>blur(5px)</c> or
	/// <c>drop-shadow(3px -3px 5px rgba(0,0,0,1))</c>, or <c>"none"</c>.
	/// </returns>
	internal static string BuildFilter(Effect? effect)
	{
		return effect switch
		{
			BlurEffect blur       => BuildBlur(blur),
			DropShadowEffect drop => BuildDropShadow(drop),
			null                  => "none",
			_                     => "none",
		};
	}

	/// <summary>
	/// Builds a CSS snippet <c>selector { filter: VALUE; }</c> for the given widget selector.
	/// </summary>
	internal static string BuildFilterSnippet(string selector, Effect? effect)
	{
		string value = BuildFilter(effect);
		return $"{selector} {{ filter: {value}; }}";
	}

	// ── Helpers ──────────────────────────────────────────────────────────────

	static string BuildBlur(BlurEffect blur) =>
		$"blur({F(blur.Radius)}px)";

	static string BuildDropShadow(DropShadowEffect drop)
	{
		double rad = drop.Direction * Math.PI / 180.0;
		// WPF: 0° = right (+X), CCW positive. Screen Y increases down → negate sin.
		double dx = drop.ShadowDepth * Math.Cos(rad);
		double dy = -drop.ShadowDepth * Math.Sin(rad);

		// Compose effective alpha: shadow colour's A channel × Opacity.
		double alpha = (drop.Color.A / 255.0) * drop.Opacity;

		string rgba = $"rgba({drop.Color.R},{drop.Color.G},{drop.Color.B},{F(alpha)})";
		return $"drop-shadow({F(dx)}px {F(dy)}px {F(drop.BlurRadius)}px {rgba})";
	}
}
