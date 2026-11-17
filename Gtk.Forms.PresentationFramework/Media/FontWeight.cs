namespace System.Windows;

/// <summary>Represents the weight of a typeface.</summary>
public struct FontWeight : IEquatable<FontWeight>
{
	/// <summary>Numeric weight value (100–900, matching CSS font-weight).</summary>
	public int Weight { get; }

	internal FontWeight(int weight) { Weight = weight; }

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
}