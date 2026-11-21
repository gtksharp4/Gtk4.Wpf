namespace System.Windows.Controls;

/// <summary>
/// Scroll-bar, selection, tool-bar and zoom DPs. The scroll-bar visibilities map to the ScrolledWindow policy and
/// <see cref="IsSelectionEnabled"/> to the label's selectable flag. Gap: <see cref="IsToolBarVisible"/> and the
/// zoom values are stored (no tool bar is built and the label is not scaled).
/// </summary>
public partial class FlowDocumentScrollViewer
{
	/// <summary>Identifies the VerticalScrollBarVisibility property: <see cref="ScrollViewer"/>'s property, with
	/// this type's default of Visible.</summary>
	public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
		ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(FlowDocumentScrollViewer),
			new FrameworkPropertyMetadata(ScrollBarVisibility.Visible, OnScrollBarChanged));

	/// <summary>Identifies the HorizontalScrollBarVisibility property: <see cref="ScrollViewer"/>'s property, with
	/// this type's default of Auto.</summary>
	public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
		ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(FlowDocumentScrollViewer),
			new FrameworkPropertyMetadata(ScrollBarVisibility.Auto, OnScrollBarChanged));

	/// <summary>Identifies the IsToolBarVisible property (default false).</summary>
	public static readonly DependencyProperty IsToolBarVisibleProperty = DependencyProperty.Register(
		nameof(IsToolBarVisible), typeof(bool), typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the IsSelectionEnabled property (default true).</summary>
	public static readonly DependencyProperty IsSelectionEnabledProperty = DependencyProperty.Register(
		nameof(IsSelectionEnabled), typeof(bool), typeof(FlowDocumentScrollViewer),
		new FrameworkPropertyMetadata(true, OnSelectionChanged));

	/// <summary>Identifies the Zoom property (default 100).</summary>
	public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
		nameof(Zoom), typeof(double), typeof(FlowDocumentScrollViewer),
		new FrameworkPropertyMetadata(100.0, null, CoerceZoom), IsValidZoom);

	/// <summary>Identifies the MinZoom property (default 80).</summary>
	public static readonly DependencyProperty MinZoomProperty = DependencyProperty.Register(
		nameof(MinZoom), typeof(double), typeof(FlowDocumentScrollViewer),
		new FrameworkPropertyMetadata(80.0, OnZoomRangeChanged), IsValidZoom);

	/// <summary>Identifies the MaxZoom property (default 200).</summary>
	public static readonly DependencyProperty MaxZoomProperty = DependencyProperty.Register(
		nameof(MaxZoom), typeof(double), typeof(FlowDocumentScrollViewer),
		new FrameworkPropertyMetadata(200.0, OnZoomRangeChanged), IsValidZoom);

	/// <summary>Gets or sets the vertical scroll-bar visibility.</summary>
	public ScrollBarVisibility VerticalScrollBarVisibility
	{
		get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty)!;
		set => SetValue(VerticalScrollBarVisibilityProperty, value);
	}

	/// <summary>Gets or sets the horizontal scroll-bar visibility.</summary>
	public ScrollBarVisibility HorizontalScrollBarVisibility
	{
		get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty)!;
		set => SetValue(HorizontalScrollBarVisibilityProperty, value);
	}

	/// <summary>Gets or sets whether the tool bar is shown (stored).</summary>
	public bool IsToolBarVisible
	{
		get => (bool)GetValue(IsToolBarVisibleProperty)!;
		set => SetValue(IsToolBarVisibleProperty, value);
	}

	/// <summary>Gets or sets whether the text can be selected.</summary>
	public bool IsSelectionEnabled
	{
		get => (bool)GetValue(IsSelectionEnabledProperty)!;
		set => SetValue(IsSelectionEnabledProperty, value);
	}

	/// <summary>Gets or sets the zoom percentage, kept within MinZoom and MaxZoom (stored).</summary>
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

	static bool IsValidZoom(object? v) => v is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d > 0;

	static void OnScrollBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var viewer = (FlowDocumentScrollViewer)d;
		viewer._presenter?.SetScrollBars(viewer.HorizontalScrollBarVisibility, viewer.VerticalScrollBarVisibility);
	}

	static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var viewer = (FlowDocumentScrollViewer)d;
		if (viewer._presenter != null) viewer._presenter.Label.Selectable = viewer.IsSelectionEnabled;
	}

	static void OnZoomRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		d.CoerceValue(ZoomProperty);

	static object? CoerceZoom(DependencyObject d, object? value)
	{
		var viewer = (FlowDocumentScrollViewer)d;
		double zoom = (double)value!;
		double min = viewer.MinZoom, max = viewer.MaxZoom;
		return zoom < min ? min : zoom > max ? Math.Max(min, max) : zoom;
	}
}
