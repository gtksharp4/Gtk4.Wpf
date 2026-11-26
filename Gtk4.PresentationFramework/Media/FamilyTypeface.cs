using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Media;

/// <summary>Describes one face of a <see cref="FontFamily"/>: its style, weight, stretch and metrics.</summary>
public sealed class FamilyTypeface : IEquatable<FamilyTypeface>
{
	/// <summary>Creates a Normal/Normal/Normal face description.</summary>
	public FamilyTypeface()
	{
	}

	/// <summary>The face style.</summary>
	public FontStyle Style { get; set; } = FontStyles.Normal;

	/// <summary>The face weight.</summary>
	public FontWeight Weight { get; set; } = FontWeights.Normal;

	/// <summary>The face stretch.</summary>
	public FontStretch Stretch { get; set; } = FontStretches.Normal;

	/// <summary>Underline position relative to the baseline, as an em ratio.</summary>
	public double UnderlinePosition { get; set; }

	/// <summary>Underline thickness as an em ratio.</summary>
	public double UnderlineThickness { get; set; }

	/// <summary>Strikethrough position relative to the baseline, as an em ratio.</summary>
	public double StrikethroughPosition { get; set; }

	/// <summary>Strikethrough thickness as an em ratio.</summary>
	public double StrikethroughThickness { get; set; }

	/// <summary>Capital letter height as an em ratio.</summary>
	public double CapsHeight { get; set; }

	/// <summary>Lowercase x height as an em ratio.</summary>
	public double XHeight { get; set; }

	/// <summary>The device font name; stored only (there are no device fonts under GTK).</summary>
	public string? DeviceFontName { get; set; }

	/// <summary>Face names that override the font's own, keyed by language.</summary>
	public IDictionary<XmlLanguage, string> AdjustedFaceNames { get; } = new LanguageSpecificStringDictionary();

	/// <inheritdoc/>
	public bool Equals(FamilyTypeface? other) =>
		other != null && Style == other.Style && Weight == other.Weight && Stretch == other.Stretch;

	/// <inheritdoc/>
	public override bool Equals(object? obj) => Equals(obj as FamilyTypeface);

	/// <inheritdoc/>
	public override int GetHashCode() =>
		Style.GetHashCode() ^ (Weight.GetHashCode() << 4) ^ (Stretch.GetHashCode() << 12);
}
