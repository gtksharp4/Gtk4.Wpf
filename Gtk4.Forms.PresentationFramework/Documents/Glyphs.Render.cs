using System.Windows.Xps.Packaging;

namespace System.Windows.Documents;

public sealed partial class Glyphs
{
	const double PangoScale = 1024.0;
	Gtk.DrawingArea? _area;
	string? _family;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_area = new Gtk.DrawingArea();
		_area.DrawFunc = (area, cr, w, h) => Draw(cr, w, h);
		SyncSize();
		return _area;
	}

	void Invalidate()
	{
		_family = null;
		if (_area == null)
			return;
		SyncSize();
		_area.QueueDraw();
	}

	string Family => _family ??= XpsFontNames.FamilyOf(Package, FontUri) ?? "Sans";

	// The widget spans from the page origin to the end of the text, so the origin is page-relative.
	void SyncSize()
	{
		double em = FontRenderingEmSize;
		_area!.ContentWidth = (int)Math.Ceiling(OriginX + UnicodeString.Length * em * 0.6 + em);
		_area.ContentHeight = (int)Math.Ceiling(OriginY + em);
	}

	void Draw(Cairo.Context cr, int width, int height)
	{
		if (UnicodeString.Length == 0 || FontRenderingEmSize <= 0)
			return;
		using (var desc = new Pango.FontDescription())
		{
			desc.Family = Family;
			desc.AbsoluteSize = FontRenderingEmSize * PangoScale;
			using var layout = Pango.CairoHelper.CreateLayout(cr);
			layout.FontDescription = desc;
			layout.SetText(UnicodeString);
			cr.Save();
			(Fill ?? Media.Brushes.Black).ApplyToCairo(cr, width, height);
			cr.MoveTo(OriginX, OriginY - layout.Baseline / PangoScale);
			Pango.CairoHelper.ShowLayout(cr, layout);
			cr.Restore();
		}
	}
}
