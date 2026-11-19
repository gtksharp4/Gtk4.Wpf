namespace System.Windows;

/// <summary>
/// <see cref="ActualWidth"/>/<see cref="ActualHeight"/> and <see cref="SizeChanged"/>.
/// </summary>
/// <remarks>
/// Two sources feed <see cref="UIElement.RenderSize"/>, and through it these members — but only
/// one at a time:
/// <list type="bullet">
/// <item>the managed <see cref="UIElement.Arrange"/> pass — authoritative headless and before
/// the element is loaded with a mapped widget; its SizeChanged notifications are queued and
/// raised after the outermost arrange pass (top-down, as WPF's size-changed chain);</item>
/// <item>the live GTK allocation — once the element is loaded and its widget is mapped, the
/// toplevel <c>Gdk.FrameClock</c> <c>layout</c> phase copies the widget's border box
/// (<c>gtk_widget_compute_bounds</c>: allocation minus CSS margin, including CSS border and
/// padding) into <see cref="UIElement.RenderSize"/> (<c>GtkAllocationSync</c>). From then on a
/// managed arrange no longer writes RenderSize.</item>
/// </list>
/// The ActualWidth/ActualHeight DP values are written when the size changes rather than computed
/// on read (WPF's <c>GetReadOnlyValueCallback</c>); the CLR getters read RenderSize directly.
/// </remarks>
public abstract partial class FrameworkElement
{
	/// <inheritdoc/>
	internal override bool IsRenderSizeOwnedByGtk => IsLoaded && HasBackingWidget && Widget.IsMapped;

	static readonly DependencyPropertyKey _actualWidthPropertyKey =
		DependencyProperty.RegisterReadOnly(
			nameof(ActualWidth), typeof(double), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(0.0));

	/// <summary>Identifies the read-only <see cref="ActualWidth"/> dependency property.</summary>
	public static readonly DependencyProperty ActualWidthProperty = _actualWidthPropertyKey.DependencyProperty;

	static readonly DependencyPropertyKey _actualHeightPropertyKey =
		DependencyProperty.RegisterReadOnly(
			nameof(ActualHeight), typeof(double), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(0.0));

	/// <summary>Identifies the read-only <see cref="ActualHeight"/> dependency property.</summary>
	public static readonly DependencyProperty ActualHeightProperty = _actualHeightPropertyKey.DependencyProperty;

	/// <summary>The rendered width of this element (<see cref="UIElement.RenderSize"/>.Width).</summary>
	public double ActualWidth => RenderSize.Width;

	/// <summary>The rendered height of this element (<see cref="UIElement.RenderSize"/>.Height).</summary>
	public double ActualHeight => RenderSize.Height;

	/// <summary>Identifies the <see cref="SizeChanged"/> routed event (Direct).</summary>
	public static readonly RoutedEvent SizeChangedEvent =
		EventManager.RegisterRoutedEvent(
			"SizeChanged", RoutingStrategy.Direct, typeof(SizeChangedEventHandler), typeof(FrameworkElement));

	/// <summary>
	/// Occurs when <see cref="ActualWidth"/> or <see cref="ActualHeight"/> changes — after a
	/// managed arrange to a new size, or when the live GTK allocation of a mapped element changes.
	/// </summary>
	public event SizeChangedEventHandler SizeChanged
	{
		add => AddHandler(SizeChangedEvent, value);
		remove => RemoveHandler(SizeChangedEvent, value);
	}

	/// <summary>
	/// Updates the <see cref="ActualWidthProperty"/>/<see cref="ActualHeightProperty"/> values for
	/// the changed axes (so bindings observe them), then raises <see cref="SizeChanged"/>.
	/// </summary>
	protected internal override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		if (sizeInfo == null) throw new ArgumentNullException(nameof(sizeInfo));
		var newSize = sizeInfo.NewSize;
		if (sizeInfo.WidthChanged)
			SetValue(_actualWidthPropertyKey, newSize.Width);
		if (sizeInfo.HeightChanged)
			SetValue(_actualHeightPropertyKey, newSize.Height);
		RaiseEvent(new SizeChangedEventArgs(this, sizeInfo));
	}
}
