using System.Linq;

namespace System.Windows.Media;

/// <summary>A font family combined with a style, weight and stretch.</summary>
public partial class Typeface
{
	/// <summary>Creates a Normal typeface of the named family.</summary>
	public Typeface(string typefaceName)
		: this(new FontFamily(typefaceName ?? throw new ArgumentNullException(nameof(typefaceName))),
			FontStyles.Normal, FontWeights.Normal, FontStretches.Normal)
	{
	}

	/// <summary>Creates a typeface.</summary>
	public Typeface(FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch)
		: this(fontFamily, style, weight, stretch, null)
	{
	}

	/// <summary>Creates a typeface with a fallback family used for characters the family lacks.</summary>
	public Typeface(FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch,
		FontFamily? fallbackFontFamily)
	{
		FontFamily = fontFamily ?? throw new ArgumentNullException(nameof(fontFamily));
		Style = style;
		Weight = weight;
		Stretch = stretch;
		FallbackFontFamily = fallbackFontFamily;
	}

	/// <summary>The font family.</summary>
	public FontFamily FontFamily { get; }

	/// <summary>The style.</summary>
	public FontStyle Style { get; }

	/// <summary>The weight.</summary>
	public FontWeight Weight { get; }

	/// <summary>The stretch.</summary>
	public FontStretch Stretch { get; }

	internal FontFamily? FallbackFontFamily { get; }

	GtkFamilyInfo? Family => GtkFontCatalog.Find(FontFamily.Source);

	internal GtkFaceInfo? Face
	{
		get
		{
			var info = Family;
			if (info == null || info.Faces.Count == 0) return null;
			return info.Faces.OrderBy(Distance).First();
		}
	}

	int Distance(GtkFaceInfo f) =>
		(f.Style == Style ? 0 : 100000)
		+ Math.Abs(f.Weight.ToOpenTypeWeight() - Weight.ToOpenTypeWeight())
		+ 100 * Math.Abs(f.Stretch.ToOpenTypeStretch() - Stretch.ToOpenTypeStretch());

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Typeface t && Equals(t);

	/// <summary>Value equality: family, style, weight, stretch and fallback family.</summary>
	bool Equals(Typeface? other) =>
		other != null && FontFamily.Equals(other.FontFamily) && Style == other.Style && Weight == other.Weight
		&& Stretch == other.Stretch && Equals(FallbackFontFamily, other.FallbackFontFamily);

	/// <inheritdoc/>
	public override int GetHashCode() =>
		FontFamily.GetHashCode() ^ (Style.GetHashCode() << 3) ^ (Weight.GetHashCode() << 7)
		^ (Stretch.GetHashCode() << 13)
		^ (FallbackFontFamily?.GetHashCode() ?? 0);
}
