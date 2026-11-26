using System;
using System.Globalization;

namespace System.Windows;

/// <summary>
/// The degree to which a font is stretched relative to normal. Mirrors <c>System.Windows.FontStretch</c>
/// (OpenType usWidthClass 1..9); a default-constructed value is <see cref="FontStretches.Normal"/>.
/// </summary>
public struct FontStretch : IFormattable, IEquatable<FontStretch>
{
	readonly int _stretch;

	internal FontStretch(int openTypeStretch) { _stretch = openTypeStretch; }

	/// <summary>Creates a stretch from an OpenType value in 1..9.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Value is outside 1..9.</exception>
	public static FontStretch FromOpenTypeStretch(int stretchValue)
	{
		if (stretchValue < 1 || stretchValue > 9)
			throw new ArgumentOutOfRangeException(nameof(stretchValue));
		return new FontStretch(stretchValue);
	}

	/// <summary>Compares two stretches by their OpenType value.</summary>
	public static int Compare(FontStretch left, FontStretch right) =>
		left.ToOpenTypeStretch().CompareTo(right.ToOpenTypeStretch());

	/// <inheritdoc/>
	public static bool operator <(FontStretch left, FontStretch right) => Compare(left, right) < 0;
	/// <inheritdoc/>
	public static bool operator <=(FontStretch left, FontStretch right) => Compare(left, right) <= 0;
	/// <inheritdoc/>
	public static bool operator >(FontStretch left, FontStretch right) => Compare(left, right) > 0;
	/// <inheritdoc/>
	public static bool operator >=(FontStretch left, FontStretch right) => Compare(left, right) >= 0;

	/// <summary>Returns the OpenType stretch value in 1..9.</summary>
	public int ToOpenTypeStretch() => _stretch == 0 ? 5 : _stretch;

	/// <inheritdoc/>
	public bool Equals(FontStretch other) => ToOpenTypeStretch() == other.ToOpenTypeStretch();
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is FontStretch fs && Equals(fs);
	/// <inheritdoc/>
	public override int GetHashCode() => ToOpenTypeStretch();
	/// <inheritdoc/>
	public static bool operator ==(FontStretch left, FontStretch right) => left.Equals(right);
	/// <inheritdoc/>
	public static bool operator !=(FontStretch left, FontStretch right) => !left.Equals(right);

	/// <inheritdoc/>
	public override string ToString() => FontStretches.NameOf(ToOpenTypeStretch())
		?? ToOpenTypeStretch().ToString(CultureInfo.InvariantCulture);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) => ToString();
}
