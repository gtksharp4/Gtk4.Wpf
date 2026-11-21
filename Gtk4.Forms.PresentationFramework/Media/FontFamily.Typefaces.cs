using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace System.Windows.Media;

public partial class FontFamily
{
	FamilyTypefaceCollection? _familyTypefaces;
	FontFamilyMapCollection? _familyMaps;

	/// <summary>Creates a family from a name or fallback list, remembering the base URI.</summary>
	public FontFamily(Uri? baseUri, string familyName) : this(familyName) => BaseUri = baseUri;

	/// <summary>The base URI the family name resolves against; null for a plain name.</summary>
	public Uri? BaseUri { get; }

	GtkFamilyInfo? SystemFamily => GtkFontCatalog.Find(Source);

	/// <summary>The family name keyed by language. Pango reports one name, stored under en-US.</summary>
	public LanguageSpecificStringDictionary FamilyNames
	{
		get
		{
			var info = SystemFamily;
			var first = Source.Split(',')[0].Trim();
			int hash = first.LastIndexOf('#');
			var name = info?.Name ?? (hash >= 0 ? first.Substring(hash + 1) : first);
			return LanguageSpecificStringDictionary.Single("en-US", name);
		}
	}

	/// <summary>The faces of the family as <see cref="FamilyTypeface"/> descriptions (empty for a non-system family).</summary>
	public FamilyTypefaceCollection FamilyTypefaces
	{
		get
		{
			if (_familyTypefaces != null) return _familyTypefaces;
			var c = new FamilyTypefaceCollection();
			var info = SystemFamily;
			if (info != null)
				foreach (var f in info.Faces)
				{
					var m = GtkFontMetrics.Measure(info.Name, f.Style, f.Weight, f.Stretch);
					var ft = new FamilyTypeface
					{
						Style = f.Style, Weight = f.Weight, Stretch = f.Stretch,
						UnderlinePosition = m.UnderlinePosition, UnderlineThickness = m.UnderlineThickness,
						StrikethroughPosition = m.StrikethroughPosition, StrikethroughThickness = m.StrikethroughThickness,
						CapsHeight = m.CapsHeight, XHeight = m.XHeight,
					};
					ft.AdjustedFaceNames.Add(XmlLanguage.GetLanguage("en-US"), f.Name);
					c.Add(ft);
				}
			return _familyTypefaces = c;
		}
	}

	/// <summary>Unicode-range family mappings. Stored only: GTK does no font composition.</summary>
	public FontFamilyMapCollection FamilyMaps => _familyMaps ?? (_familyMaps = new FontFamilyMapCollection());

	/// <summary>The faces of the family. A family GTK does not know yields one Normal typeface.</summary>
	public ICollection<Typeface> GetTypefaces()
	{
		var info = SystemFamily;
		if (info == null || info.Faces.Count == 0)
			return new List<Typeface> { new Typeface(this, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal) };
		return info.Faces.Select(f => new Typeface(this, f.Style, f.Weight, f.Stretch)).ToList();
	}

	/// <summary>Distance from the top of the line to the baseline, as an em ratio.</summary>
	public double Baseline => Metrics.Baseline;

	/// <summary>Line height as an em ratio.</summary>
	public double LineSpacing => Metrics.Height;

	GtkFontMetrics Metrics =>
		GtkFontMetrics.Measure(SystemFamily?.Name ?? Source, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
}
