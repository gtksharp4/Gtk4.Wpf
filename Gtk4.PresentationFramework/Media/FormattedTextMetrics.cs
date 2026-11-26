namespace System.Windows.Media;

/// <summary>Measures and paints <see cref="FormattedText"/> through Cairo's toy text API.</summary>
static class FormattedTextMetrics
{
	static void Select(Cairo.Context cr, FormattedText t)
	{
		string family = t.Typeface?.FontFamily?.Source ?? "Sans";
		bool italic = t.Typeface != null && t.Typeface.Style == FontStyles.Italic;
		bool bold = t.Typeface != null && t.Typeface.Weight.ToOpenTypeWeight() >= 600;
		cr.SelectFontFace(family, italic ? Cairo.FontSlant.Italic : Cairo.FontSlant.Normal,
			bold ? Cairo.FontWeight.Bold : Cairo.FontWeight.Normal);
		cr.SetFontSize(t.FontSize);
	}

	internal static Size Measure(FormattedText t)
	{
		try
		{
			using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 1, 1);
			using var cr = new Cairo.Context(surface);
			Select(cr, t);
			return new Size(cr.TextExtents(t.Text).XAdvance, cr.FontExtents.Height);
		}
		catch (Exception)
		{
			return new Size(t.Text.Length * t.FontSize * 0.55, t.FontSize * 1.2);
		}
	}

	internal static void Paint(Cairo.Context cr, FormattedText t, Point origin)
	{
		Select(cr, t);
		cr.Save();
		var size = Measure(t);
		t.Foreground?.ApplyToCairo(cr, size.Width, size.Height);
		cr.MoveTo(origin.X, origin.Y + cr.FontExtents.Ascent);
		cr.ShowText(t.Text);
		cr.Restore();
	}
}
