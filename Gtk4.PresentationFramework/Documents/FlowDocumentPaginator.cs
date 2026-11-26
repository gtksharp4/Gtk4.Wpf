using System.Windows.Media;
using System.Windows.Threading;

namespace System.Windows.Documents;

/// <summary>A drawing visual for one flow page; carries the font the print path draws its text lines in.</summary>
sealed class FlowPageVisual : DrawingVisual
{
	internal FlowPageVisual(string fontFamily, double fontSize)
	{
		FontFamilyName = fontFamily;
		FontSize = fontSize;
	}

	internal string FontFamilyName { get; }

	internal double FontSize { get; }
}

/// <summary>Position of a page start in a <see cref="FlowDocument"/>: the zero-based page number.</summary>
sealed class FlowContentPosition : ContentPosition
{
	internal FlowContentPosition(int page) => Page = page;

	internal int Page { get; }
}

/// <summary>
/// The paginator behind <see cref="IDocumentPaginatorSource.DocumentPaginator"/> of a <see cref="FlowDocument"/>.
/// Page counts come from <see cref="FlowDocumentPageMeter"/>, the block-aware layout the page viewer draws.
/// </summary>
sealed class FlowDocumentPaginator : DynamicDocumentPaginator
{
	readonly FlowDocument _document;
	Size? _pageSize;
	bool _valid;
	bool _dirty;
	bool _scheduled;
	bool _background = true;
	int _count = 1;
	FlowDocumentPageMeter? _meter;

	internal FlowDocumentPaginator(FlowDocument document)
	{
		_document = document;
		document.ContentChanged += Invalidate;
	}

	public override IDocumentPaginatorSource Source => _document;

	public override bool IsBackgroundPaginationEnabled
	{
		get => _background;
		set => _background = value;
	}

	public override bool IsPageCountValid
	{
		get
		{
			FlushPending();
			return _valid;
		}
	}

	public override Size PageSize
	{
		get => _pageSize ?? DocumentPageSize;
		set
		{
			if (_pageSize == value) return;
			_pageSize = value;
			Invalidate();
		}
	}

	Size DocumentPageSize => FlowPageGeometry.PageSize(_document,
		_viewport ?? new Size(FlowDocumentPageMeter.DefaultWidth, FlowDocumentPageMeter.DefaultHeight));

	Size? _viewport;

	/// <summary>The viewer's content area: the page size wherever PageWidth/PageHeight are NaN.</summary>
	internal Size? Viewport
	{
		get => _viewport;
		set
		{
			if (_viewport == value) return;
			_viewport = value;
			Invalidate();
		}
	}

	public override int PageCount
	{
		get
		{
			FlushPending();
			if (!_valid) Recompute();
			return _count;
		}
	}

	// Edits only mark the pagination dirty; one Background callback per dispatcher turn then raises PagesChanged
	// and finishes, so building N blocks costs one batch. A synchronous reader flushes the pending change itself.
	void Invalidate()
	{
		_dirty = true;
		if (_scheduled) return;
		_scheduled = true;
		Dispatcher.CurrentDispatcher.BeginInvoke(Finish, DispatcherPriority.Background);
	}

	void FlushPending()
	{
		if (!_dirty) return;
		_dirty = false;
		_valid = false;
		_meter = null;
		OnPagesChanged(new PagesChangedEventArgs(0, _count));
	}

	void Finish()
	{
		_scheduled = false;
		FlushPending();
		if (!_background) return;
		if (!_valid) Recompute();
		OnPaginationProgress(new PaginationProgressEventArgs(0, _count));
		OnPaginationCompleted(EventArgs.Empty);
	}

	void Recompute()
	{
		_meter = FlowDocumentPageMeter.Measure(_document, PageSize);
		_count = _meter.PageCount;
		_valid = true;
	}

	internal FlowDocumentPageMeter Meter
	{
		get
		{
			FlushPending();
			if (_meter == null) Recompute();
			return _meter!;
		}
	}

	public override DocumentPage GetPage(int pageNumber)
	{
		if (pageNumber < 0) throw new ArgumentOutOfRangeException(nameof(pageNumber));
		var meter = Meter;
		if (pageNumber >= _count) return DocumentPage.Missing;
		var size = PageSize;
		var visual = new FlowPageVisual(
			((FontFamily)_document.GetValue(FlowDocument.FontFamilyProperty)!).Source,
			(double)_document.GetValue(FlowDocument.FontSizeProperty)!);
		using (var scope = visual.RenderOpen())
			DrawColumns(scope.Context, meter, pageNumber);
		var pad = meter.Geometry.Padding;
		var content = new Rect(pad.Left, pad.Top, meter.Geometry.ContentWidth, meter.Geometry.ContentHeight);
		return new DocumentPage(visual, size, new Rect(0, 0, size.Width, size.Height), content);
	}

	static void DrawColumns(DrawingContext dc, FlowDocumentPageMeter meter, int pageNumber)
	{
		var g = meter.Geometry;
		var page = meter.Pages[pageNumber];
		for (int c = 0; c < page.Columns.Count; c++)
		{
			foreach (var slice in page.Columns[c].Slices)
			{
				var lines = slice.Box.Lines.GetRange(slice.First, slice.Count);
				double x = g.ColumnX(c) + slice.Box.Margin.Left + slice.Box.Padding.Left;
				dc.DrawText(new FormattedText(string.Join("\n", lines)), new Point(x, g.Padding.Top + slice.Y));
			}
		}
	}

	public override int GetPageNumber(ContentPosition contentPosition)
	{
		if (contentPosition == null) throw new ArgumentNullException(nameof(contentPosition));
		return contentPosition is FlowContentPosition p ? Math.Min(p.Page, PageCount - 1) : 0;
	}

	public override ContentPosition GetPagePosition(DocumentPage page)
	{
		if (page == null) throw new ArgumentNullException(nameof(page));
		return ContentPosition.Missing;
	}

	public override ContentPosition GetObjectPosition(object element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (!(element is TextElement te) || !ReferenceEquals(te.OwningDocument, _document))
			return ContentPosition.Missing;
		TextTree.PlainChars(_document, te, out int before, out _);
		return new FlowContentPosition(Meter.PageOfChar(before));
	}
}
