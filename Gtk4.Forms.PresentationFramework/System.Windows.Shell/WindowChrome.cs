namespace System.Windows.Shell;

/// <summary>
/// Describes the custom non-client area of a <see cref="Window"/>. On GTK a positive <see cref="CaptionHeight"/>
/// makes the window undecorated and turns the top strip into a drag area; the other properties are stored only
/// (GTK has no resize-border, glass or caption-button hooks).
/// </summary>
public partial class WindowChrome : Freezable
{
	/// <summary>The thickness that extends the glass frame over the whole window.</summary>
	public static Thickness GlassFrameCompleteThickness => new Thickness(-1);

	/// <summary>Identifies <see cref="CaptionHeight"/>. Default 0.</summary>
	public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register(
		nameof(CaptionHeight), typeof(double), typeof(WindowChrome), new PropertyMetadata(0.0), IsValidLength);

	/// <summary>
	/// Identifies <see cref="ResizeBorderThickness"/>. Metadata default 0; a new instance starts at 4.
	/// </summary>
	public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register(
		nameof(ResizeBorderThickness), typeof(Thickness), typeof(WindowChrome),
		new PropertyMetadata(default(Thickness)), IsValidThickness);

	/// <summary>Identifies <see cref="GlassFrameThickness"/>. Default 0.</summary>
	public static readonly DependencyProperty GlassFrameThicknessProperty = DependencyProperty.Register(
		nameof(GlassFrameThickness), typeof(Thickness), typeof(WindowChrome),
		new PropertyMetadata(new Thickness(0)), IsValidGlassThickness);

	/// <summary>Identifies <see cref="UseAeroCaptionButtons"/>. Default true.</summary>
	public static readonly DependencyProperty UseAeroCaptionButtonsProperty = DependencyProperty.Register(
		nameof(UseAeroCaptionButtons), typeof(bool), typeof(WindowChrome), new PropertyMetadata(true));

	/// <summary>Identifies <see cref="CornerRadius"/>. Default 0.</summary>
	public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
		nameof(CornerRadius), typeof(CornerRadius), typeof(WindowChrome),
		new PropertyMetadata(new CornerRadius(0)), IsValidCornerRadius);

	/// <summary>Identifies <see cref="NonClientFrameEdges"/>. Default None.</summary>
	public static readonly DependencyProperty NonClientFrameEdgesProperty = DependencyProperty.Register(
		nameof(NonClientFrameEdges), typeof(NonClientFrameEdges), typeof(WindowChrome),
		new PropertyMetadata(NonClientFrameEdges.None), IsValidEdges);

	/// <summary>
	/// Creates a chrome with the Windows defaults. WPF binds these to <see cref="SystemParameters"/>; here they are
	/// local values until the layer has those bindings.
	/// </summary>
	public WindowChrome()
	{
		CaptionHeight = SystemParameters.WindowCaptionHeight;
		ResizeBorderThickness = SystemParameters.WindowResizeBorderThickness;
		GlassFrameThickness = SystemParameters.WindowNonClientFrameThickness;
		CornerRadius = SystemParameters.WindowCornerRadius;
	}

	/// <summary>Gets or sets the height of the caption (drag) area.</summary>
	public double CaptionHeight
	{
		get => (double)GetValue(CaptionHeightProperty)!;
		set => SetValue(CaptionHeightProperty, value);
	}

	/// <summary>Gets or sets the thickness of the resize border. Stored only.</summary>
	public Thickness ResizeBorderThickness
	{
		get => (Thickness)GetValue(ResizeBorderThicknessProperty)!;
		set => SetValue(ResizeBorderThicknessProperty, value);
	}

	/// <summary>Gets or sets the thickness of the glass frame. Stored only.</summary>
	public Thickness GlassFrameThickness
	{
		get => (Thickness)GetValue(GlassFrameThicknessProperty)!;
		set => SetValue(GlassFrameThicknessProperty, value);
	}

	/// <summary>Gets or sets whether the system caption buttons are used. Stored only.</summary>
	public bool UseAeroCaptionButtons
	{
		get => (bool)GetValue(UseAeroCaptionButtonsProperty)!;
		set => SetValue(UseAeroCaptionButtonsProperty, value);
	}

	/// <summary>Gets or sets the corner radius of the window. Stored only.</summary>
	public CornerRadius CornerRadius
	{
		get => (CornerRadius)GetValue(CornerRadiusProperty)!;
		set => SetValue(CornerRadiusProperty, value);
	}

	/// <summary>Gets or sets the edges on which the system draws the frame. Stored only.</summary>
	public NonClientFrameEdges NonClientFrameEdges
	{
		get => (NonClientFrameEdges)GetValue(NonClientFrameEdgesProperty)!;
		set => SetValue(NonClientFrameEdgesProperty, value);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new WindowChrome();

	static bool IsLength(double d) => !double.IsNaN(d) && !double.IsInfinity(d) && d >= 0.0;

	static bool IsValidLength(object? value) => value is double d && IsLength(d);

	static bool IsThickness(Thickness t) =>
		IsLength(t.Left) && IsLength(t.Top) && IsLength(t.Right) && IsLength(t.Bottom);

	static bool IsValidThickness(object? value) => value is Thickness t && IsThickness(t);

	// The glass frame also accepts -1 on every side: the whole window.
	static bool IsValidGlassThickness(object? value) =>
		value is Thickness t && (IsThickness(t) || t == GlassFrameCompleteThickness);

	static bool IsValidCornerRadius(object? value) =>
		value is CornerRadius c && IsLength(c.TopLeft) && IsLength(c.TopRight)
		&& IsLength(c.BottomRight) && IsLength(c.BottomLeft);

	static bool IsValidEdges(object? value) =>
		value is NonClientFrameEdges e && ((int)e & ~15) == 0;
}
