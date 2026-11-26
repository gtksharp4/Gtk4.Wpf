using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.VisualLayer;

/// <summary>DrawText paints through one path only: the shared renderer is opt-in, the page renderer owns pages.</summary>
public class DrawTextOptInTests : WpfGtkTestBase
{
	const int W = 300, H = 400;

	static byte[] Ink(System.Action<Cairo.Context> paint)
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, W, H);
		using (var cr = new Cairo.Context(surface)) paint(cr);
		surface.Flush();
		return (byte[])surface.Data.Clone();
	}

	static bool RowInked(byte[] px, int y)
	{
		for (int x = 0; x < W; x++)
			if (px[(y * W + x) * 4 + 3] != 0) return true;
		return false;
	}

	/// <summary>Horizontal ink bands: runs of inked rows separated by blank rows.</summary>
	static List<(int Top, int Bottom)> Bands(byte[] px)
	{
		var bands = new List<(int, int)>();
		int start = -1;
		for (int y = 0; y < H; y++)
		{
			bool inked = RowInked(px, y);
			if (inked && start < 0) start = y;
			if (!inked && start >= 0) { bands.Add((start, y - 1)); start = -1; }
		}
		if (start >= 0) bands.Add((start, H - 1));
		return bands;
	}

	static DocumentPage Page(params string[] paragraphs)
	{
		var doc = new FlowDocument();
		foreach (var p in paragraphs) doc.Blocks.Add(new Paragraph(new Run(p)));
		var paginator = (DynamicDocumentPaginator)((IDocumentPaginatorSource)doc).DocumentPaginator;
		paginator.PageSize = new Size(W, H);
		return paginator.GetPage(0);
	}

	static List<(string Text, Point Origin)> Texts(DocumentPage page) =>
		((DrawingVisual)page.Visual!).Drawing!.Commands
			.Where(c => c.Kind == DrawingContext.DrawCommandKind.DrawText)
			.Select(c => ((System.ValueTuple<FormattedText, Point>)c.Arg!))
			.Select(t => (t.Item1.Text, t.Item2)).ToList();
	/// <summary>One Pango pass over the page's text commands: what a correct page paints.</summary>
	static byte[] SinglePangoPass(DocumentPage page, List<(string Text, Point Origin)> texts)
	{
		var flow = (FlowPageVisual)page.Visual!;
		return Ink(cr =>
		{
			foreach (var (text, origin) in texts)
			{
				using var desc = new Pango.FontDescription { Family = flow.FontFamilyName };
				desc.AbsoluteSize = flow.FontSize * 1024.0;
				var layout = Pango.CairoHelper.CreateLayout(cr);
				layout.FontDescription = desc;
				layout.SetText(text);
				cr.SetSourceRGB(0, 0, 0);
				cr.MoveTo(origin.X, origin.Y);
				Pango.CairoHelper.ShowLayout(cr, layout);
			}
		});
	}

	static int InkCount(byte[] px)
	{
		int n = 0;
		for (int i = 3; i < px.Length; i += 4) if (px[i] != 0) n++;
		return n;
	}

	void AssertSingleText(params string[] paragraphs)
	{
		var page = Page(paragraphs);
		var texts = Texts(page);
		Assert.True(texts.Sum(t => t.Text.Split((char)10).Length) >= 2, "need a multi-line page");
		var actual = Ink(cr => DocumentPageRenderer.Render(page, cr));
		var expected = SinglePangoPass(page, texts);
		Assert.Equal(Bands(expected).Count, Bands(actual).Count);
		Assert.Equal(Bands(expected), Bands(actual));
		Assert.Equal(InkCount(expected), InkCount(actual));
	}

	const string Long = "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt "
		+ "ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation";

	[Fact]
	public void A_multi_line_paragraph_page_paints_each_line_once() => Run(() => AssertSingleText(Long));

	[Fact]
	public void Two_paragraphs_paint_each_line_once() => Run(() => AssertSingleText(Long, Long));

	static DrawingContext TextOnly()
	{
		var dv = new DrawingVisual();
		using (var dc = dv.RenderOpen())
			dc.DrawText(new FormattedText("Hello text", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
				new Typeface("Sans"), 20, Brushes.Black, 1.0), new Point(10, 10));
		return dv.Drawing!;
	}

	static bool AnyInk(byte[] px)
	{
		for (int i = 3; i < px.Length; i += 4) if (px[i] != 0) return true;
		return false;
	}

	[Fact]
	public void The_shared_renderer_skips_text_by_default() => Run(() =>
		Assert.False(AnyInk(Ink(cr => DrawingContextRenderer.Render(TextOnly(), cr)))));

	[Fact]
	public void The_shared_renderer_paints_text_when_asked() => Run(() =>
		Assert.True(AnyInk(Ink(cr => DrawingContextRenderer.Render(TextOnly(), cr, paintText: true)))));

	[Fact]
	public void A_DrawingVisual_paints_its_text_through_the_visual_renderer() => Run(() =>
	{
		var dv = new DrawingVisual();
		using (var dc = dv.RenderOpen())
			dc.DrawText(new FormattedText("Hello text", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
				new Typeface("Sans"), 20, Brushes.Black, 1.0), new Point(10, 10));
		var host = new ContainerVisual();
		host.Children.Add(dv);
		Assert.True(AnyInk(Ink(cr => VisualRenderer.PaintChildren(host, cr))));
	});
}
