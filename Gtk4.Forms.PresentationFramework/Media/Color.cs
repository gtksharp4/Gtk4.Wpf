using System;

namespace System.Windows.Media;

/// <summary>
/// An ARGB colour with byte components. Matches WPF's <c>System.Windows.Media.Color</c>.
/// </summary>
public partial struct Color : IEquatable<Color>
{
	/// <summary>Alpha channel (0 = transparent, 255 = opaque).</summary>
	public byte A { get; set; }
	/// <summary>Red channel.</summary>
	public byte R { get; set; }
	/// <summary>Green channel.</summary>
	public byte G { get; set; }
	/// <summary>Blue channel.</summary>
	public byte B { get; set; }

	// ── Normalized float accessors (WPF compatibility) ───────────────────────
	// NOTE: WPF's Sc* properties use the linear-light sRGB transfer function.
	// These accessors perform simple byte↔[0,1] normalization (A/255 and 255*value),
	// NOT the sRGB⇄linear-light transfer. They are intentionally named Sc* for WPF
	// source compatibility only. A future phase may add the proper transfer function.

	/// <summary>
	/// Alpha as a normalized float in [0,1] (byte / 255).
	/// NOT linear-light scRGB — a future phase will add the sRGB transfer function.
	/// </summary>
	public float ScA { get => A / 255f; set => A = (byte)Math.Round(value * 255f); }

	/// <summary>
	/// Red as a normalized float in [0,1] (byte / 255).
	/// NOT linear-light scRGB — a future phase will add the sRGB transfer function.
	/// </summary>
	public float ScR { get => R / 255f; set => R = (byte)Math.Round(value * 255f); }

	/// <summary>
	/// Green as a normalized float in [0,1] (byte / 255).
	/// NOT linear-light scRGB — a future phase will add the sRGB transfer function.
	/// </summary>
	public float ScG { get => G / 255f; set => G = (byte)Math.Round(value * 255f); }

	/// <summary>
	/// Blue as a normalized float in [0,1] (byte / 255).
	/// NOT linear-light scRGB — a future phase will add the sRGB transfer function.
	/// </summary>
	public float ScB { get => B / 255f; set => B = (byte)Math.Round(value * 255f); }

	// ── Factory methods ──────────────────────────────────────────────────────

	/// <summary>Creates an opaque colour from R, G, B bytes.</summary>
	public static Color FromRgb(byte r, byte g, byte b) =>
		new Color { A = 255, R = r, G = g, B = b };

	/// <summary>Creates a colour from A, R, G, B bytes.</summary>
	public static Color FromArgb(byte a, byte r, byte g, byte b) =>
		new Color { A = a, R = r, G = g, B = b };

	/// <summary>
	/// Creates a colour from normalized float components in [0,1] (each multiplied by 255
	/// and rounded to a byte). Named <c>FromScRgb</c> for WPF source compatibility;
	/// this performs byte normalization only, NOT the sRGB⇄linear-light transfer function.
	/// A future phase will add the proper scRGB conversion.
	/// </summary>
	public static Color FromScRgb(float a, float r, float g, float b) =>
		FromArgb(
			(byte)Math.Round(a * 255f),
			(byte)Math.Round(r * 255f),
			(byte)Math.Round(g * 255f),
			(byte)Math.Round(b * 255f));

	// ── GTK / Cairo mapping ──────────────────────────────────────────────────

	/// <summary>Converts to a <see cref="Gdk.RGBA"/> for GTK CSS / widget colouring.</summary>
	public Gdk.RGBA ToGdkRgba() =>
		new Gdk.RGBA { Red = R / 255f, Green = G / 255f, Blue = B / 255f, Alpha = A / 255f };

	/// <summary>Converts to a <see cref="Cairo.Color"/> for Cairo paint operations.</summary>
	public Cairo.Color ToCairoColor() =>
		new Cairo.Color(R / 255.0, G / 255.0, B / 255.0, A / 255.0);

	// ── Equality ─────────────────────────────────────────────────────────────

	/// <inheritdoc/>
	public bool Equals(Color other) => A == other.A && R == other.R && G == other.G && B == other.B;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Color c && Equals(c);
	/// <inheritdoc/>
	public override int GetHashCode() => HashHelper.Combine(A, R, G, B);
	/// <inheritdoc/>
	public static bool operator ==(Color a, Color b) => a.Equals(b);
	/// <inheritdoc/>
	public static bool operator !=(Color a, Color b) => !a.Equals(b);
	/// <inheritdoc/>
	public override string ToString() => $"#{A:X2}{R:X2}{G:X2}{B:X2}";

	static class HashHelper
	{
		internal static int Combine(byte a, byte r, byte g, byte b)
		{
			int h = a;
			h = (h << 5) + h ^ r;
			h = (h << 5) + h ^ g;
			h = (h << 5) + h ^ b;
			return h;
		}
	}
}