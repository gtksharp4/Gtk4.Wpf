namespace System.Windows;

/// <summary>Represents the weight of a typeface.</summary>
[System.ComponentModel.TypeConverter(typeof(FontWeightConverter))]
public struct FontWeight : IEquatable<FontWeight>, IFormattable
{
	// 0 means "not set", which WPF treats as Normal (400).
	readonly int _weight;

	/// <summary>Numeric weight value (1..999); a default-constructed value reads as Normal (400).</summary>
	public int Weight => _weight == 0 ? 400 : _weight;

	internal FontWeight(int weight) { _weight = weight; }

	/// <summary>Creates a weight from an OpenType value in 1..999.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Value is outside 1..999.</exception>
	public static FontWeight FromOpenTypeWeight(int weightValue)
	{
		if (weightValue < 1 || weightValue > 999)
			throw new ArgumentOutOfRangeException(nameof(weightValue));
		return new FontWeight(weightValue);
	}

	/// <summary>Compares two weights; negative when <paramref name="left"/> is lighter.</summary>
	public static int Compare(FontWeight left, FontWeight right) => left.Weight.CompareTo(right.Weight);

	/// <inheritdoc/>
	public bool Equals(FontWeight other) => Weight == other.Weight;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is FontWeight fw && Equals(fw);
	/// <inheritdoc/>
	public override int GetHashCode() => Weight;
	/// <inheritdoc/>
	public static bool operator ==(FontWeight left, FontWeight right) => Compare(left, right) == 0;
	/// <inheritdoc/>
	public static bool operator !=(FontWeight left, FontWeight right) => Compare(left, right) != 0;
	/// <summary>Whether <paramref name="left"/> is lighter than <paramref name="right"/>.</summary>
	public static bool operator <(FontWeight left, FontWeight right) => Compare(left, right) < 0;
	/// <summary>Whether <paramref name="left"/> is lighter than or equal to <paramref name="right"/>.</summary>
	public static bool operator <=(FontWeight left, FontWeight right) => Compare(left, right) <= 0;
	/// <summary>Whether <paramref name="left"/> is heavier than <paramref name="right"/>.</summary>
	public static bool operator >(FontWeight left, FontWeight right) => Compare(left, right) > 0;
	/// <summary>Whether <paramref name="left"/> is heavier than or equal to <paramref name="right"/>.</summary>
	public static bool operator >=(FontWeight left, FontWeight right) => Compare(left, right) >= 0;

	/// <summary>Returns the OpenType weight (1..999); a default-constructed value is Normal (400).</summary>
	public int ToOpenTypeWeight() => Weight;

	/// <summary>Returns the WPF name (Thin..ExtraBlack) or the number for other weights.</summary>
	public override string ToString() => ConvertToString(null, null);

	string IFormattable.ToString(string? format, IFormatProvider? provider) => ConvertToString(format, provider);

	string ConvertToString(string? format, IFormatProvider? provider)
	{
		switch (Weight)
		{
			case 100: return "Thin";
			case 200: return "ExtraLight";
			case 300: return "Light";
			case 400: return "Normal";
			case 500: return "Medium";
			case 600: return "SemiBold";
			case 700: return "Bold";
			case 800: return "ExtraBold";
			case 900: return "Black";
			case 950: return "ExtraBlack";
			default: return Weight.ToString(provider);
		}
	}
}

/// <summary>Provides predefined <see cref="FontWeight"/> values.</summary>
public static class FontWeights
{
	/// <summary>Normal weight (400).</summary>
	public static FontWeight Normal { get; } = new FontWeight(400);
	/// <summary>Bold weight (700).</summary>
	public static FontWeight Bold { get; } = new FontWeight(700);
	/// <summary>Thin weight (100).</summary>
	public static FontWeight Thin { get; } = new FontWeight(100);
	/// <summary>Light weight (300).</summary>
	public static FontWeight Light { get; } = new FontWeight(300);
	/// <summary>Medium weight (500).</summary>
	public static FontWeight Medium { get; } = new FontWeight(500);
	/// <summary>SemiBold weight (600).</summary>
	public static FontWeight SemiBold { get; } = new FontWeight(600);
	/// <summary>ExtraBold weight (800).</summary>
	public static FontWeight ExtraBold { get; } = new FontWeight(800);
	/// <summary>Black weight (900).</summary>
	public static FontWeight Black { get; } = new FontWeight(900);
	/// <summary>ExtraLight weight (200).</summary>
	public static FontWeight ExtraLight { get; } = new FontWeight(200);
	/// <summary>UltraLight weight (200).</summary>
	public static FontWeight UltraLight { get; } = new FontWeight(200);
	/// <summary>Regular weight (400).</summary>
	public static FontWeight Regular { get; } = new FontWeight(400);
	/// <summary>DemiBold weight (600).</summary>
	public static FontWeight DemiBold { get; } = new FontWeight(600);
	/// <summary>UltraBold weight (800).</summary>
	public static FontWeight UltraBold { get; } = new FontWeight(800);
	/// <summary>Heavy weight (900).</summary>
	public static FontWeight Heavy { get; } = new FontWeight(900);
	/// <summary>ExtraBlack weight (950).</summary>
	public static FontWeight ExtraBlack { get; } = new FontWeight(950);
	/// <summary>UltraBlack weight (950).</summary>
	public static FontWeight UltraBlack { get; } = new FontWeight(950);
}