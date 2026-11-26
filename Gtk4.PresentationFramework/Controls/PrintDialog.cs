using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// Chooses a printer and prints visuals and paginated documents through GTK 4's <c>Gtk.PrintOperation</c>.
/// Gaps: <c>PrintQueue</c> and <c>PrintTicket</c> (System.Printing) are not provided; the print path paints only
/// <see cref="DrawingVisual"/> pages (UIElement visuals print blank); CurrentPage and SelectedPages print all pages.
/// </summary>
public partial class PrintDialog
{
	uint _minPage = 1;
	uint _maxPage = 9999;
	PageRange _pageRange = new PageRange(1, 1);
	double _areaWidth = 816;
	double _areaHeight = 1056;
	Gtk.PageSetup? _pageSetup;
	Gtk.PrintSettings? _settings;

	/// <summary>Gets or sets which pages print (default AllPages).</summary>
	public PageRangeSelection PageRangeSelection { get; set; } = PageRangeSelection.AllPages;

	/// <summary>Gets or sets the range used when <see cref="PageRangeSelection"/> is UserPages.</summary>
	public PageRange PageRange
	{
		get => _pageRange;
		set => _pageRange = value;
	}

	/// <summary>Gets or sets whether the user may enter a page range.</summary>
	public bool UserPageRangeEnabled { get; set; }

	/// <summary>Gets or sets whether the dialog offers the selected pages.</summary>
	public bool SelectedPagesEnabled { get; set; }

	/// <summary>Gets or sets whether the dialog offers the current page.</summary>
	public bool CurrentPageEnabled { get; set; }

	/// <summary>Gets or sets the lowest page number the user may pick (default 1).</summary>
	/// <exception cref="ArgumentException">The value is 0.</exception>
	public uint MinPage
	{
		get => _minPage;
		set
		{
			if (value == 0) throw new ArgumentException("The page number must be at least 1.", nameof(value));
			_minPage = value;
		}
	}

	/// <summary>Gets or sets the highest page number the user may pick (default 9999).</summary>
	/// <exception cref="ArgumentException">The value is 0.</exception>
	public uint MaxPage
	{
		get => _maxPage;
		set
		{
			if (value == 0) throw new ArgumentException("The page number must be at least 1.", nameof(value));
			_maxPage = value;
		}
	}

	/// <summary>Gets the printable width in device-independent units (letter size until a page is chosen).</summary>
	public double PrintableAreaWidth => _areaWidth;

	/// <summary>Gets the printable height in device-independent units (letter size until a page is chosen).</summary>
	public double PrintableAreaHeight => _areaHeight;

	/// <summary>A PDF file to print to instead of a printer; set by tests.</summary>
	internal string? ExportFilename { get; set; }

	/// <summary>Shows the GTK print dialog; true when the user accepted, false when cancelled.</summary>
	public bool? ShowDialog()
	{
		var setup = Presenter(OwnerWindow());
		if (setup == null) return false;
		_pageSetup = setup.Value.PageSetup;
		_settings = setup.Value.Settings;
		ReadPrintableArea(_pageSetup);
		return true;
	}

	/// <summary>Prints one visual as a single page.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="visual"/> is null.</exception>
	public void PrintVisual(Visual visual, string description)
	{
		if (visual == null) throw new ArgumentNullException(nameof(visual));
		var page = new DocumentPage(visual, new Size(_areaWidth, _areaHeight),
			new Rect(0, 0, _areaWidth, _areaHeight), new Rect(0, 0, _areaWidth, _areaHeight));
		Print(new SinglePagePaginator(page), description);
	}

	/// <summary>Prints the pages of <paramref name="documentPaginator"/> in <see cref="PageRangeSelection"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="documentPaginator"/> is null.</exception>
	public void PrintDocument(DocumentPaginator documentPaginator, string description)
	{
		if (documentPaginator == null) throw new ArgumentNullException(nameof(documentPaginator));
		Print(documentPaginator, description);
	}

	/// <summary>The one-based pages the job prints, in order.</summary>
	internal int[] SelectPages(int pageCount)
	{
		int from = 1;
		int to = pageCount;
		if (PageRangeSelection == PageRangeSelection.UserPages)
		{
			from = Math.Max(1, Math.Min(_pageRange.PageFrom, _pageRange.PageTo));
			to = Math.Min(pageCount, Math.Max(_pageRange.PageFrom, _pageRange.PageTo));
		}
		var pages = new int[Math.Max(0, to - from + 1)];
		for (int i = 0; i < pages.Length; i++) pages[i] = from + i;
		return pages;
	}
}
