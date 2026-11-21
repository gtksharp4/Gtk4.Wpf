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
/// Page counts come from <see cref="FlowDocumentPageMeter"/>, an approximation of WPF's layout.
/// </summary>
sealed class FlowDocumentPaginator : DynamicDocumentPaginator
{
	readonly FlowDocument _document;
	Size? _pageSize;
	bool _valid;
	bool _background = true;
	int _count = 1;
	FlowDocumentPageMeter? _meter;

	internal FlowDocumentPaginator(FlowDocument document) => _document = document;

	public override IDocumentPaginatorSource Source => _document;

	public override bool IsBackgroundPaginationEnabled
	{
		get => _background;
		set => _background = value;
	}

	public override bool IsPageCountValid => _valid;

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

	Size DocumentPageSize => new Size(
		double.IsNaN(_document.PageWidth) ? FlowDocumentPageMeter.DefaultWidth : _document.PageWidth,
		double.IsNaN(_document.PageHeight) ? FlowDocumentPageMeter.DefaultHeight : _document.PageHeight);

	public override int PageCount
	{
		get
		{
			if (!_valid) Recompute();
			return _count;
		}
	}

	void Invalidate()
	{
		_valid = false;
		_meter = null;
		OnPagesChanged(new PagesChangedEventArgs(0, _count));
		if (_background) Dispatcher.CurrentDispatcher.BeginInvoke(Finish, DispatcherPriority.Background);
	}

	void Finish()
	{
		if (_valid) return;
		Recompute();
		OnPaginationProgress(new PaginationProgressEventArgs(0, _count));
		OnPaginationCompleted(EventArgs.Empty);
	}

	void Recompute()
	{
		_meter = FlowDocumentPageMeter.Measure(_document, PageSize);
		_count = _meter.PageCount;
		_valid = true;
	}

	FlowDocumentPageMeter Meter
	{
		get
		{
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
		var content = new Rect(meter.Padding.Left, meter.Padding.Top,
			Math.Max(0, size.Width - meter.Padding.Left - meter.Padding.Right),
			Math.Max(0, size.Height - meter.Padding.Top - meter.Padding.Bottom));
		return new DocumentPage(visual, size, new Rect(0, 0, size.Width, size.Height), content);
	}

	static void DrawColumns(DrawingContext dc, FlowDocumentPageMeter meter, int pageNumber)
	{
		int first = pageNumber * meter.LinesPerPage;
		for (int c = 0; c < meter.Columns; c++)
		{
			int start = first + c * meter.LinesPerColumn;
			if (start >= meter.Lines.Count) break;
			int n = Math.Min(meter.LinesPerColumn, meter.Lines.Count - start);
			var text = string.Join("\n", meter.Lines.GetRange(start, n));
			double x = meter.Padding.Left + c * (meter.ColumnWidth + meter.ColumnGap);
			dc.DrawText(new FormattedText(text), new Point(x, meter.Padding.Top));
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
		return ContentPosition.Missing;
	}
}
