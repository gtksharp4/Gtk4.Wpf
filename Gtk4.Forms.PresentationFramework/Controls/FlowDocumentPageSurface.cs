using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// The GTK page view of <see cref="FlowDocumentPageViewer"/>: a drawing area that paints one page of the
/// <see cref="FlowDocumentPageMeter"/> the paginator also counts pages with, so the page count and the drawing
/// agree. The page is centred in the viewer; the document Background fills the page and Foreground colours the text.
/// </summary>
sealed class FlowDocumentPageSurface
{
	readonly Gtk.DrawingArea _area;
	bool _drawing;

	internal FlowDocumentPageSurface(Action<Size> onViewport)
	{
		_area = new Gtk.DrawingArea { Hexpand = true, Vexpand = true };
		_area.DrawFunc = (area, cr, w, h) =>
		{
			if (_drawing) return;
			_drawing = true;
			try
			{
				onViewport(new Size(w, h));
				Draw(cr, w, h);
			}
			finally { _drawing = false; }
		};
	}

	internal Gtk.DrawingArea Area => _area;

	internal FlowDocument? Document { get; set; }

	/// <summary>The zero-based page to draw.</summary>
	internal int PageIndex { get; set; }

	/// <summary>The size the last draw saw; the page size wherever PageWidth/PageHeight are NaN.</summary>
	internal Size? Viewport { get; private set; }

	internal void Redraw() => _area.QueueDraw();

	void Draw(Cairo.Context cr, int width, int height)
	{
		Viewport = new Size(width, height);
		var doc = Document;
		if (doc == null || !(((IDocumentPaginatorSource)doc).DocumentPaginator is FlowDocumentPaginator paginator))
			return;
		var meter = paginator.Meter;
		if (PageIndex < 0 || PageIndex >= meter.Pages.Count) return;
		var g = meter.Geometry;
		cr.Save();
		cr.Rectangle(0, 0, width, height);
		cr.Clip();
		cr.Translate(Math.Floor((width - g.Page.Width) / 2), Math.Floor((height - g.Page.Height) / 2));
		if (doc.Background != null)
		{
			doc.Background.ApplyToCairo(cr, g.Page.Width, g.Page.Height);
			cr.Rectangle(0, 0, g.Page.Width, g.Page.Height);
			cr.Fill();
		}
		cr.Rectangle(0, 0, g.Page.Width, g.Page.Height);
		cr.Clip();
		var page = meter.Pages[PageIndex];
		for (int c = 0; c < page.Columns.Count; c++)
			foreach (var slice in page.Columns[c].Slices)
				DrawSlice(cr, doc, g, c, slice);
		DrawRules(cr, doc, g);
		cr.Restore();
	}

	static void DrawSlice(Cairo.Context cr, FlowDocument doc, FlowPageGeometry g, int column, FlowSlice slice)
	{
		var box = slice.Box;
		double x = g.ColumnX(column) + box.Margin.Left + box.Padding.Left;
		double top = g.Padding.Top + slice.Y;
		cr.Save();
		cr.Rectangle(x, top, box.TextWidth, slice.Height);
		cr.Clip();
		using (var layout = Pango.CairoHelper.CreateLayout(cr))
		{
			layout.SetMarkup(box.Markup);
			FlowBoxText.Configure(layout, box, doc, box.Pitch);
			doc.Foreground.ApplyToCairo(cr, box.TextWidth, slice.Height);
			cr.MoveTo(x, top - slice.First * box.Pitch);
			Pango.CairoHelper.ShowLayout(cr, layout);
		}
		cr.Restore();
	}

	// A rule is centred in each gap between two columns and spans the content height.
	static void DrawRules(Cairo.Context cr, FlowDocument doc, FlowPageGeometry g)
	{
		double ruleWidth = doc.ColumnRuleWidth;
		if (g.Columns < 2 || ruleWidth <= 0 || doc.ColumnRuleBrush == null) return;
		for (int c = 0; c < g.Columns - 1; c++)
		{
			double x = g.ColumnX(c) + g.ColumnWidth + (g.ColumnGap - ruleWidth) / 2;
			doc.ColumnRuleBrush.ApplyToCairo(cr, ruleWidth, g.ContentHeight);
			cr.Rectangle(x, g.Padding.Top, ruleWidth, g.ContentHeight);
			cr.Fill();
		}
	}
}
