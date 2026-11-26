namespace System.Windows.Media;

/// <summary>Font metrics as ratios of the em size, measured through Pango.</summary>
readonly struct GtkFontMetrics
{
	internal GtkFontMetrics(double baseline, double height, double underlinePosition, double underlineThickness,
		double strikePosition, double strikeThickness, double capsHeight, double xHeight, int resolvedWeight)
	{
		Baseline = baseline;
		Height = height;
		UnderlinePosition = underlinePosition;
		UnderlineThickness = underlineThickness;
		StrikethroughPosition = strikePosition;
		StrikethroughThickness = strikeThickness;
		CapsHeight = capsHeight;
		XHeight = xHeight;
		ResolvedWeight = resolvedWeight;
	}

	internal double Baseline { get; }

	internal double Height { get; }

	internal double UnderlinePosition { get; }

	internal double UnderlineThickness { get; }

	internal double StrikethroughPosition { get; }

	internal double StrikethroughThickness { get; }

	internal double CapsHeight { get; }

	internal double XHeight { get; }

	/// <summary>The OpenType weight of the face Pango picked; 0 when unknown.</summary>
	internal int ResolvedWeight { get; }

	/// <summary>Neutral values used when Pango cannot be reached.</summary>
	internal static GtkFontMetrics Fallback { get; } =
		new GtkFontMetrics(0.9, 1.2, -0.1, 0.05, 0.3, 0.05, 0.7, 0.5, 0);

	const double Scale = 1024.0;
	const double Em = 1000.0;

	/// <summary>Measures the face at a 1000 px em and returns ratios; the ink of "H" and "x" gives the heights.</summary>
	internal static GtkFontMetrics Measure(string family, FontStyle style, FontWeight weight, FontStretch stretch)
	{
		var map = GtkFontCatalog.FontMap;
		if (map == null) return Fallback;
		try
		{
			using (var desc = new Pango.FontDescription())
			{
				desc.Family = family;
				desc.Style = style == FontStyles.Italic ? Pango.Style.Italic
					: style == FontStyles.Oblique ? Pango.Style.Oblique : Pango.Style.Normal;
				desc.Weight = (Pango.Weight)weight.ToOpenTypeWeight();
				desc.Stretch = (Pango.Stretch)(stretch.ToOpenTypeStretch() - 1);
				desc.AbsoluteSize = Em * Scale;
				var ctx = map.CreateContext();
				ctx.FontDescription = desc;
				var m = ctx.GetMetrics(desc, ctx.Language);
				var font = ctx.LoadFont(desc);
				int resolved = font == null ? 0 : (int)font.Describe().Weight;
				double u = Scale * Em;
				double caps = Ink(ctx, "H") / Em;
				double x = Ink(ctx, "x") / Em;
				return new GtkFontMetrics(m.Ascent / u, m.Height / u, m.UnderlinePosition / u, m.UnderlineThickness / u,
					m.StrikethroughPosition / u, m.StrikethroughThickness / u, caps, x, resolved);
			}
		}
		catch (Exception)
		{
			return Fallback;
		}
	}

	static double Ink(Pango.Context ctx, string text)
	{
		var layout = new Pango.Layout(ctx);
		layout.SetText(text);
		layout.GetPixelExtents(out var ink, out _);
		return ink.Height;
	}
}
