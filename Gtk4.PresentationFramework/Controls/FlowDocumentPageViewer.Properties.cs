using System.Windows.Media;

namespace System.Windows.Controls;

public partial class FlowDocumentPageViewer
{
	static readonly DependencyPropertyKey CanIncreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(CanIncreaseZoom), typeof(bool), typeof(FlowDocumentPageViewer), new PropertyMetadata(true));

	static readonly DependencyPropertyKey CanDecreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(CanDecreaseZoom), typeof(bool), typeof(FlowDocumentPageViewer), new PropertyMetadata(true));

	static readonly DependencyPropertyKey IsSelectionActivePropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(IsSelectionActive), typeof(bool), typeof(FlowDocumentPageViewer), new PropertyMetadata(false));

	/// <summary>Identifies the Zoom property (default 100, kept between MinZoom and MaxZoom).</summary>
	public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
		nameof(Zoom), typeof(double), typeof(FlowDocumentPageViewer),
		new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.None, OnZoomChanged, CoerceZoom),
		IsPositive);

	/// <summary>Identifies the MaxZoom property (default 200, never below MinZoom).</summary>
	public static readonly DependencyProperty MaxZoomProperty = DependencyProperty.Register(
		nameof(MaxZoom), typeof(double), typeof(FlowDocumentPageViewer),
		new FrameworkPropertyMetadata(200.0, FrameworkPropertyMetadataOptions.None, OnLimitChanged, CoerceMaxZoom),
		IsPositive);

	/// <summary>Identifies the MinZoom property (default 80, never above MaxZoom).</summary>
	public static readonly DependencyProperty MinZoomProperty = DependencyProperty.Register(
		nameof(MinZoom), typeof(double), typeof(FlowDocumentPageViewer),
		new FrameworkPropertyMetadata(80.0, FrameworkPropertyMetadataOptions.None, OnLimitChanged, CoerceMinZoom),
		IsPositive);

	/// <summary>Identifies the ZoomIncrement property (default 10).</summary>
	public static readonly DependencyProperty ZoomIncrementProperty = DependencyProperty.Register(
		nameof(ZoomIncrement), typeof(double), typeof(FlowDocumentPageViewer),
		new FrameworkPropertyMetadata(10.0), IsPositive);

	/// <summary>Identifies the CanIncreaseZoom property.</summary>
	public static readonly DependencyProperty CanIncreaseZoomProperty = CanIncreaseZoomPropertyKey.DependencyProperty;

	/// <summary>Identifies the CanDecreaseZoom property.</summary>
	public static readonly DependencyProperty CanDecreaseZoomProperty = CanDecreaseZoomPropertyKey.DependencyProperty;

	/// <summary>Identifies the SelectionBrush property (default the system highlight brush).</summary>
	public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(
		nameof(SelectionBrush), typeof(Brush), typeof(FlowDocumentPageViewer),
		new FrameworkPropertyMetadata(SystemColors.HighlightBrush));

	/// <summary>Identifies the SelectionOpacity property (default 0.4).</summary>
	public static readonly DependencyProperty SelectionOpacityProperty = DependencyProperty.Register(
		nameof(SelectionOpacity), typeof(double), typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(0.4));

	/// <summary>Identifies the IsSelectionActive property.</summary>
	public static readonly DependencyProperty IsSelectionActiveProperty =
		IsSelectionActivePropertyKey.DependencyProperty;

	/// <summary>Identifies the IsInactiveSelectionHighlightEnabled property (default false).</summary>
	public static readonly DependencyProperty IsInactiveSelectionHighlightEnabledProperty =
		DependencyProperty.Register(nameof(IsInactiveSelectionHighlightEnabled), typeof(bool),
			typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(false));

	static bool IsPositive(object? v) => v is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d > 0;

	static object CoerceZoom(DependencyObject d, object? value)
	{
		var v = (FlowDocumentPageViewer)d;
		return Math.Max(v.MinZoom, Math.Min(v.MaxZoom, (double)value!));
	}

	static object CoerceMaxZoom(DependencyObject d, object? value) =>
		Math.Max(((FlowDocumentPageViewer)d).MinZoom, (double)value!);

	static object CoerceMinZoom(DependencyObject d, object? value) =>
		Math.Min(((FlowDocumentPageViewer)d).MaxZoom, (double)value!);

	static void OnLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(ZoomProperty);
		((FlowDocumentPageViewer)d).UpdateZoomFlags();
	}

	static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((FlowDocumentPageViewer)d).UpdateZoomFlags();

	void UpdateZoomFlags()
	{
		SetValue(CanIncreaseZoomPropertyKey, Zoom < MaxZoom);
		SetValue(CanDecreaseZoomPropertyKey, Zoom > MinZoom);
	}

	/// <summary>Gets or sets the zoom percentage (stored; the label is not scaled).</summary>
	public double Zoom
	{
		get => (double)GetValue(ZoomProperty)!;
		set => SetValue(ZoomProperty, value);
	}

	/// <summary>Gets or sets the largest zoom percentage.</summary>
	public double MaxZoom
	{
		get => (double)GetValue(MaxZoomProperty)!;
		set => SetValue(MaxZoomProperty, value);
	}

	/// <summary>Gets or sets the smallest zoom percentage.</summary>
	public double MinZoom
	{
		get => (double)GetValue(MinZoomProperty)!;
		set => SetValue(MinZoomProperty, value);
	}

	/// <summary>Gets or sets the step of <see cref="IncreaseZoom"/> and <see cref="DecreaseZoom"/>.</summary>
	public double ZoomIncrement
	{
		get => (double)GetValue(ZoomIncrementProperty)!;
		set => SetValue(ZoomIncrementProperty, value);
	}

	/// <summary>Gets whether <see cref="Zoom"/> is below <see cref="MaxZoom"/>.</summary>
	public bool CanIncreaseZoom => (bool)GetValue(CanIncreaseZoomProperty)!;

	/// <summary>Gets whether <see cref="Zoom"/> is above <see cref="MinZoom"/>.</summary>
	public bool CanDecreaseZoom => (bool)GetValue(CanDecreaseZoomProperty)!;

	/// <summary>Gets or sets the selection brush (stored).</summary>
	public Brush? SelectionBrush
	{
		get => (Brush?)GetValue(SelectionBrushProperty);
		set => SetValue(SelectionBrushProperty, value);
	}

	/// <summary>Gets or sets the selection opacity (stored).</summary>
	public double SelectionOpacity
	{
		get => (double)GetValue(SelectionOpacityProperty)!;
		set => SetValue(SelectionOpacityProperty, value);
	}

	/// <summary>Gets whether the selection is active; always false here.</summary>
	public bool IsSelectionActive => (bool)GetValue(IsSelectionActiveProperty)!;

	/// <summary>Gets or sets whether an inactive selection stays highlighted (stored).</summary>
	public bool IsInactiveSelectionHighlightEnabled
	{
		get => (bool)GetValue(IsInactiveSelectionHighlightEnabledProperty)!;
		set => SetValue(IsInactiveSelectionHighlightEnabledProperty, value);
	}

	/// <summary>Raises <see cref="Zoom"/> by <see cref="ZoomIncrement"/>, up to <see cref="MaxZoom"/>.</summary>
	public void IncreaseZoom() => Zoom += ZoomIncrement;

	/// <summary>Lowers <see cref="Zoom"/> by <see cref="ZoomIncrement"/>, down to <see cref="MinZoom"/>.</summary>
	public void DecreaseZoom() => Zoom -= ZoomIncrement;
}
