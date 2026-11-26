namespace System.Windows.Controls;

public partial class FlowDocumentReader
{
	static readonly DependencyPropertyKey PageCountPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(PageCount), typeof(int), typeof(FlowDocumentReader), new PropertyMetadata(0));

	static readonly DependencyPropertyKey PageNumberPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(PageNumber), typeof(int), typeof(FlowDocumentReader), new PropertyMetadata(0));

	/// <summary>Identifies the ViewingMode property (default Page).</summary>
	public static readonly DependencyProperty ViewingModeProperty = DependencyProperty.Register(
		nameof(ViewingMode), typeof(FlowDocumentReaderViewingMode), typeof(FlowDocumentReader),
		new PropertyMetadata(FlowDocumentReaderViewingMode.Page),
		v => v is FlowDocumentReaderViewingMode m && m >= FlowDocumentReaderViewingMode.Page
			&& m <= FlowDocumentReaderViewingMode.Scroll);

	/// <summary>Identifies the IsPageViewEnabled property (default true).</summary>
	public static readonly DependencyProperty IsPageViewEnabledProperty = Flag(nameof(IsPageViewEnabled));

	/// <summary>Identifies the IsTwoPageViewEnabled property (default true).</summary>
	public static readonly DependencyProperty IsTwoPageViewEnabledProperty = Flag(nameof(IsTwoPageViewEnabled));

	/// <summary>Identifies the IsScrollViewEnabled property (default true).</summary>
	public static readonly DependencyProperty IsScrollViewEnabledProperty = Flag(nameof(IsScrollViewEnabled));

	/// <summary>Identifies the IsFindEnabled property (default true).</summary>
	public static readonly DependencyProperty IsFindEnabledProperty = Flag(nameof(IsFindEnabled));

	/// <summary>Identifies the IsPrintEnabled property (default true).</summary>
	public static readonly DependencyProperty IsPrintEnabledProperty = Flag(nameof(IsPrintEnabled));

	/// <summary>Identifies the Zoom property (default 100).</summary>
	public static readonly DependencyProperty ZoomProperty = Zoomer(nameof(Zoom), 100);

	/// <summary>Identifies the MinZoom property (default 80).</summary>
	public static readonly DependencyProperty MinZoomProperty = Zoomer(nameof(MinZoom), 80);

	/// <summary>Identifies the MaxZoom property (default 200).</summary>
	public static readonly DependencyProperty MaxZoomProperty = Zoomer(nameof(MaxZoom), 200);

	/// <summary>Identifies the ZoomIncrement property (default 10).</summary>
	public static readonly DependencyProperty ZoomIncrementProperty = Zoomer(nameof(ZoomIncrement), 10);

	/// <summary>Identifies the PageCount property.</summary>
	public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;

	/// <summary>Identifies the PageNumber property.</summary>
	public static readonly DependencyProperty PageNumberProperty = PageNumberPropertyKey.DependencyProperty;

	static DependencyProperty Flag(string name) => DependencyProperty.Register(
		name, typeof(bool), typeof(FlowDocumentReader), new PropertyMetadata(true));

	static DependencyProperty Zoomer(string name, double value) => DependencyProperty.Register(
		name, typeof(double), typeof(FlowDocumentReader), new PropertyMetadata(value),
		v => v is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d > 0);

	/// <summary>Gets or sets the viewing mode (Page and TwoPage render as Scroll).</summary>
	public FlowDocumentReaderViewingMode ViewingMode
	{
		get => (FlowDocumentReaderViewingMode)GetValue(ViewingModeProperty)!;
		set => SetValue(ViewingModeProperty, value);
	}

	/// <summary>Gets or sets whether the page view can be chosen.</summary>
	public bool IsPageViewEnabled
	{
		get => (bool)GetValue(IsPageViewEnabledProperty)!;
		set => SetValue(IsPageViewEnabledProperty, value);
	}

	/// <summary>Gets or sets whether the two-page view can be chosen.</summary>
	public bool IsTwoPageViewEnabled
	{
		get => (bool)GetValue(IsTwoPageViewEnabledProperty)!;
		set => SetValue(IsTwoPageViewEnabledProperty, value);
	}

	/// <summary>Gets or sets whether the scroll view can be chosen.</summary>
	public bool IsScrollViewEnabled
	{
		get => (bool)GetValue(IsScrollViewEnabledProperty)!;
		set => SetValue(IsScrollViewEnabledProperty, value);
	}

	/// <summary>Gets or sets whether find is enabled (stored).</summary>
	public bool IsFindEnabled
	{
		get => (bool)GetValue(IsFindEnabledProperty)!;
		set => SetValue(IsFindEnabledProperty, value);
	}

	/// <summary>Gets or sets whether print is enabled (stored).</summary>
	public bool IsPrintEnabled
	{
		get => (bool)GetValue(IsPrintEnabledProperty)!;
		set => SetValue(IsPrintEnabledProperty, value);
	}

	/// <summary>Gets or sets the zoom percentage (stored).</summary>
	public double Zoom
	{
		get => (double)GetValue(ZoomProperty)!;
		set => SetValue(ZoomProperty, value);
	}

	/// <summary>Gets or sets the smallest zoom percentage (stored).</summary>
	public double MinZoom
	{
		get => (double)GetValue(MinZoomProperty)!;
		set => SetValue(MinZoomProperty, value);
	}

	/// <summary>Gets or sets the largest zoom percentage (stored).</summary>
	public double MaxZoom
	{
		get => (double)GetValue(MaxZoomProperty)!;
		set => SetValue(MaxZoomProperty, value);
	}

	/// <summary>Gets or sets the zoom step (stored).</summary>
	public double ZoomIncrement
	{
		get => (double)GetValue(ZoomIncrementProperty)!;
		set => SetValue(ZoomIncrementProperty, value);
	}

	/// <summary>Gets the number of pages: 1 with a document, otherwise 0 (the label is not paginated).</summary>
	public int PageCount => (int)GetValue(PageCountProperty)!;

	/// <summary>Gets the current page number: 1 with a document, otherwise 0.</summary>
	public int PageNumber => (int)GetValue(PageNumberProperty)!;

	/// <summary>Returns whether <paramref name="pageNumber"/> exists (only page 1, and only with a document).</summary>
	public bool CanGoToPage(int pageNumber) => pageNumber >= 1 && pageNumber <= PageCount;

	void UpdatePages()
	{
		int pages = Document == null ? 0 : 1;
		SetValue(PageCountPropertyKey, pages);
		SetValue(PageNumberPropertyKey, pages);
	}
}
