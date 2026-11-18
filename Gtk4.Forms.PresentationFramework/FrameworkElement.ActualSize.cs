namespace System.Windows;

/// <summary>
/// <see cref="ActualWidth"/>/<see cref="ActualHeight"/> and <see cref="SizeChanged"/>.
/// </summary>
/// <remarks>
/// Two sources feed <see cref="UIElement.RenderSize"/>, and through it these members:
/// <list type="bullet">
/// <item>the managed <see cref="UIElement.Arrange"/> pass — authoritative headless and before
/// the hosting window is mapped;</item>
/// <item>the live GTK allocation — once the element's widget is mapped, the toplevel
/// <c>Gdk.FrameClock</c> <c>layout</c> phase copies <c>gtk_widget_get_width/height</c> into
/// <see cref="UIElement.RenderSize"/> (<c>GtkAllocationSync</c>), so the GTK allocation wins over
/// the managed arrange for on-screen elements.</item>
/// </list>
/// </remarks>
public abstract partial class FrameworkElement
{
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
