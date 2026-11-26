namespace System.Windows.Controls;

/// <summary>
/// Positions children at explicit coordinates using the Canvas.Left/Top/Right/Bottom
/// attached properties.
///
/// GTK backing: <see cref="Gtk.Fixed"/>. Children are added via
/// <c>Fixed.Add(widget)</c> (GTK 4 form; places at 0,0) then positioned with
/// <c>widget.MoveTo(x, y)</c>. Right/Bottom are resolved to Left/Top at the time the
/// widget is sized (GTK 4 Fixed does not expose the allocated size to the positioning
/// call, so Right/Bottom are approximated as <c>Left=0</c>/<c>Top=0</c> until a
/// resize callback is wired — noted approximation, Phase 2).
/// </summary>
public partial class Canvas : Panel
{
	// ── Attached properties ──────────────────────────────────────────────────────

	/// <summary>Distance from the left edge of the canvas.</summary>
	public static readonly DependencyProperty LeftProperty =
		DependencyProperty.RegisterAttached("Left", typeof(double), typeof(Canvas),
			new PropertyMetadata(double.NaN, OnPositionChanged));

	/// <summary>Distance from the top edge of the canvas.</summary>
	public static readonly DependencyProperty TopProperty =
		DependencyProperty.RegisterAttached("Top", typeof(double), typeof(Canvas),
			new PropertyMetadata(double.NaN, OnPositionChanged));

	/// <summary>Distance from the right edge of the canvas.</summary>
	public static readonly DependencyProperty RightProperty =
		DependencyProperty.RegisterAttached("Right", typeof(double), typeof(Canvas),
			new PropertyMetadata(double.NaN, OnPositionChanged));

	/// <summary>Distance from the bottom edge of the canvas.</summary>
	public static readonly DependencyProperty BottomProperty =
		DependencyProperty.RegisterAttached("Bottom", typeof(double), typeof(Canvas),
			new PropertyMetadata(double.NaN, OnPositionChanged));

	/// <summary>Gets the left offset of <paramref name="element"/>.</summary>
	public static double GetLeft(UIElement element) => (double)element.GetValue(LeftProperty)!;
	/// <summary>Sets the left offset of <paramref name="element"/>.</summary>
	public static void SetLeft(UIElement element, double value) => element.SetValue(LeftProperty, value);

	/// <summary>Gets the top offset of <paramref name="element"/>.</summary>
	public static double GetTop(UIElement element) => (double)element.GetValue(TopProperty)!;
	/// <summary>Sets the top offset of <paramref name="element"/>.</summary>
	public static void SetTop(UIElement element, double value) => element.SetValue(TopProperty, value);

	/// <summary>Gets the right offset of <paramref name="element"/>.</summary>
	public static double GetRight(UIElement element) => (double)element.GetValue(RightProperty)!;
	/// <summary>Sets the right offset of <paramref name="element"/>.</summary>
	public static void SetRight(UIElement element, double value) => element.SetValue(RightProperty, value);

	/// <summary>Gets the bottom offset of <paramref name="element"/>.</summary>
	public static double GetBottom(UIElement element) => (double)element.GetValue(BottomProperty)!;
	/// <summary>Sets the bottom offset of <paramref name="element"/>.</summary>
	public static void SetBottom(UIElement element, double value) => element.SetValue(BottomProperty, value);

	// ── GTK backing ──────────────────────────────────────────────────────────────

	Gtk.Fixed? _fixed;

	/// <inheritdoc/>
	internal override bool IsWidgetCreated => _fixed != null;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_fixed = new BackdropFixed();
		foreach (var child in Children)
			PlaceChild(child);
		return _fixed;
	}

	/// <inheritdoc/>
	protected override void AddChildWidget(UIElement child)
	{
		PlaceChild(child);
	}

	/// <inheritdoc/>
	protected override void RemoveChildWidget(UIElement child)
	{
		_fixed!.Remove(child.Widget);
	}

	void PlaceChild(UIElement child)
	{
		double x = ResolveCoord(GetLeft(child));
		double y = ResolveCoord(GetTop(child));
		// Right/Bottom: approximated as 0 until Phase-2 resize callback.
		// Fixed.Put adds the widget at the given coordinates (GTK 4 compat form).
		_fixed!.Put(child.Widget, x, y);
	}

	static double ResolveCoord(double v) => double.IsNaN(v) ? 0.0 : v;

	static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement child && child.LogicalParent is Canvas canvas && canvas._fixed != null)
		{
			double x = ResolveCoord(GetLeft(child));
			double y = ResolveCoord(GetTop(child));
			canvas._fixed.Move(child.Widget, x, y);
		}
	}
}