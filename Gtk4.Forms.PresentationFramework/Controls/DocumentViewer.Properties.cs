using System.Windows.Input;

namespace System.Windows.Controls;

public partial class DocumentViewer
{
	const int ThumbnailsAcross = 4;
	const int MaxPagesAcrossLimit = 32;

	/// <summary>The command that shows the pages as thumbnails.</summary>
	public static RoutedUICommand ViewThumbnailsCommand { get; } =
		new RoutedUICommand("View Thumbnails", nameof(ViewThumbnailsCommand), typeof(DocumentViewer));

	/// <summary>The command that fits the page width to the viewer.</summary>
	public static RoutedUICommand FitToWidthCommand { get; } =
		new RoutedUICommand("Fit To Width", nameof(FitToWidthCommand), typeof(DocumentViewer));

	/// <summary>The command that fits the page height to the viewer.</summary>
	public static RoutedUICommand FitToHeightCommand { get; } =
		new RoutedUICommand("Fit To Height", nameof(FitToHeightCommand), typeof(DocumentViewer));

	/// <summary>The command that fits a number of pages across the viewer; the parameter is the count.</summary>
	public static RoutedUICommand FitToMaxPagesAcrossCommand { get; } =
		new RoutedUICommand("Fit To Max Pages Across", nameof(FitToMaxPagesAcrossCommand), typeof(DocumentViewer));

	/// <summary>Identifies the Zoom property (default 100, kept between 5 and 5000).</summary>
	public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
		nameof(Zoom), typeof(double), typeof(DocumentViewer),
		new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.None, null,
			(d, v) => Math.Max(5.0, Math.Min(5000.0, (double)v!))),
		v => v is double z && !double.IsNaN(z) && !double.IsInfinity(z) && z > 0);

	/// <summary>Identifies the ShowPageBorders property (default true).</summary>
	public static readonly DependencyProperty ShowPageBordersProperty = DependencyProperty.Register(
		nameof(ShowPageBorders), typeof(bool), typeof(DocumentViewer),
		new FrameworkPropertyMetadata(true, Relayout));

	/// <summary>Identifies the VerticalPageSpacing property (default 10).</summary>
	public static readonly DependencyProperty VerticalPageSpacingProperty = Spacing(nameof(VerticalPageSpacing));

	/// <summary>Identifies the HorizontalPageSpacing property (default 10).</summary>
	public static readonly DependencyProperty HorizontalPageSpacingProperty = Spacing(nameof(HorizontalPageSpacing));

	/// <summary>Identifies the MaxPagesAcross property (default 1).</summary>
	public static readonly DependencyProperty MaxPagesAcrossProperty = DependencyProperty.Register(
		nameof(MaxPagesAcross), typeof(int), typeof(DocumentViewer), new FrameworkPropertyMetadata(1, Relayout),
		v => v is int n && n >= 1 && n <= MaxPagesAcrossLimit);

	static void Relayout(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DocumentViewer)d).Rebuild();

	static DependencyProperty Spacing(string name) => DependencyProperty.Register(
		name, typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(10.0, Relayout),
		v => v is double s && !double.IsNaN(s) && !double.IsInfinity(s) && s >= 0);

	/// <summary>Gets or sets the zoom percentage (stored; the pages are not scaled).</summary>
	public double Zoom
	{
		get => (double)GetValue(ZoomProperty)!;
		set => SetValue(ZoomProperty, value);
	}

	/// <summary>Gets or sets whether each page has a border.</summary>
	public bool ShowPageBorders
	{
		get => (bool)GetValue(ShowPageBordersProperty)!;
		set => SetValue(ShowPageBordersProperty, value);
	}

	/// <summary>Gets or sets the gap between page rows.</summary>
	public double VerticalPageSpacing
	{
		get => (double)GetValue(VerticalPageSpacingProperty)!;
		set => SetValue(VerticalPageSpacingProperty, value);
	}

	/// <summary>Gets or sets the gap between pages in a row.</summary>
	public double HorizontalPageSpacing
	{
		get => (double)GetValue(HorizontalPageSpacingProperty)!;
		set => SetValue(HorizontalPageSpacingProperty, value);
	}

	/// <summary>Gets or sets how many pages share a row.</summary>
	public int MaxPagesAcross
	{
		get => (int)GetValue(MaxPagesAcrossProperty)!;
		set => SetValue(MaxPagesAcrossProperty, value);
	}

	/// <summary>Sets <see cref="Zoom"/> to fit the page width; unchanged before the viewer has a size.</summary>
	public void FitToWidth() => Fit(ActualWidth, Document?.DocumentPaginator.PageSize.Width ?? 0);

	/// <summary>Sets <see cref="Zoom"/> to fit the page height; unchanged before the viewer has a size.</summary>
	public void FitToHeight() => Fit(ActualHeight, Document?.DocumentPaginator.PageSize.Height ?? 0);

	/// <summary>Fits the current <see cref="MaxPagesAcross"/> pages to the width, leaving it unchanged.</summary>
	public void FitToMaxPagesAcross() => FitToMaxPagesAcross(MaxPagesAcross);

	/// <summary>Shows <paramref name="pagesAcross"/> pages per row and fits them to the width.</summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="pagesAcross"/> is outside 1 to 32.</exception>
	public void FitToMaxPagesAcross(int pagesAcross)
	{
		if (pagesAcross < 1 || pagesAcross > MaxPagesAcrossLimit)
			throw new ArgumentOutOfRangeException(nameof(pagesAcross));
		MaxPagesAcross = pagesAcross;
		double page = Document?.DocumentPaginator.PageSize.Width ?? 0;
		Fit(ActualWidth, page * pagesAcross + HorizontalPageSpacing * (pagesAcross - 1));
	}

	/// <summary>Shows the pages as thumbnails: four to a row.</summary>
	public void ViewThumbnails() => FitToMaxPagesAcross(ThumbnailsAcross);

	void Fit(double available, double needed)
	{
		if (available > 0 && needed > 0 && !double.IsInfinity(needed)) Zoom = 100 * available / needed;
	}

	static DocumentViewer()
	{
		Bind(FitToWidthCommand, v => v.FitToWidth());
		Bind(FitToHeightCommand, v => v.FitToHeight());
		Bind(ViewThumbnailsCommand, v => v.ViewThumbnails());
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewer), new CommandBinding(
			FitToMaxPagesAcrossCommand,
			(s, e) => ((DocumentViewer)s).FitToMaxPagesAcross(e.Parameter is int n ? n : 1),
			(s, e) => e.CanExecute = ((DocumentViewer)s).Document != null));
	}

	static void Bind(RoutedCommand command, Action<DocumentViewer> run) =>
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewer), new CommandBinding(command,
			(s, _) => run((DocumentViewer)s),
			(s, e) => e.CanExecute = ((DocumentViewer)s).Document != null));
}
