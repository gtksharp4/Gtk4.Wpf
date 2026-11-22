namespace System.Windows;

/// <summary>Represents the weight of a typeface.</summary>
[System.ComponentModel.TypeConverter(typeof(FontWeightConverter))]
public struct FontWeight : IEquatable<FontWeight>
{
	/// <summary>Numeric weight value (100–900, matching CSS font-weight).</summary>
	public int Weight { get; }

	internal FontWeight(int weight) { Weight = weight; }

	/// <summary>Creates a weight from an OpenType value in 1..999.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Value is outside 1..999.</exception>
	public static FontWeight FromOpenTypeWeight(int weightValue)
	{
		if (weightValue < 1 || weightValue > 999)
			throw new ArgumentOutOfRangeException(nameof(weightValue));
		return new FontWeight(weightValue);
	}

	/// <inheritdoc/>
	public bool Equals(FontWeight other) => Weight == other.Weight;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is FontWeight fw && Equals(fw);
	/// <inheritdoc/>
	public override int GetHashCode() => Weight;
	/// <inheritdoc/>
	public static bool operator ==(FontWeight a, FontWeight b) => a.Weight == b.Weight;
	/// <inheritdoc/>
	public static bool operator !=(FontWeight a, FontWeight b) => a.Weight != b.Weight;
	/// <summary>Returns the OpenType weight (1..999); a default-constructed value is Normal (400).</summary>
	public int ToOpenTypeWeight() => Weight == 0 ? 400 : Weight;

	/// <inheritdoc/>
	public override string ToString() => Weight.ToString();
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