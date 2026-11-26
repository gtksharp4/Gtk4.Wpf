using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Paints a <see cref="DocumentPage"/> onto a Cairo context in device-independent units. Only a
/// <see cref="DrawingVisual"/> page has content to paint (shapes and text lines); a page whose visual is a UIElement
/// is left blank, because GTK widgets have no Cairo path here.
/// </summary>
static class DocumentPageRenderer
{
	const double Scale = 1024.0;
	const double DefaultFontSize = 12;

	internal static void Render(DocumentPage page, Cairo.Context cr)
	{
		if (!(page.Visual is DrawingVisual visual) || visual.Drawing == null) return;
		DrawingContextRenderer.Render(visual.Drawing, cr);
		var flow = visual as FlowPageVisual;
		foreach (var cmd in visual.Drawing.Commands)
		{
			if (cmd.Kind != DrawingContext.DrawCommandKind.DrawText) continue;
			if (!(cmd.Arg is ValueTuple<FormattedText, Point> text)) continue;
			DrawText(cr, text.Item1.Text, text.Item2, flow?.FontFamilyName ?? "Sans",
				flow?.FontSize ?? DefaultFontSize);
		}
	}

	static void DrawText(Cairo.Context cr, string text, Point origin, string family, double size)
	{
		using (var desc = new Pango.FontDescription())
		{
			desc.Family = family;
			desc.AbsoluteSize = size * Scale;
			var layout = Pango.CairoHelper.CreateLayout(cr);
			layout.FontDescription = desc;
			layout.SetText(text);
			cr.Save();
			cr.SetSourceRGB(0, 0, 0);
			cr.MoveTo(origin.X, origin.Y);
			Pango.CairoHelper.ShowLayout(cr, layout);
			cr.Restore();
		}
	}
}
