using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// A surface for ink and elements. GTK has no ink stack, so the editing modes are stored and raised as events
/// but no strokes are captured or drawn (gap). The background and the children render through an inner
/// <see cref="Canvas"/> (see <c>InkCanvas.Surface.cs</c>).
/// </summary>
[ContentProperty("Children")]
public partial class InkCanvas : FrameworkElement
{
	/// <summary>Identifies the Background property (default null).</summary>
	public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(
		typeof(InkCanvas),
		new FrameworkPropertyMetadata(null, (d, e) => ((InkCanvas)d)._surface.Background = (Brush?)e.NewValue));

	/// <summary>Identifies the EditingMode property (default Ink).</summary>
	public static readonly DependencyProperty EditingModeProperty = DependencyProperty.Register(
		nameof(EditingMode), typeof(InkCanvasEditingMode), typeof(InkCanvas),
		new FrameworkPropertyMetadata(InkCanvasEditingMode.Ink, OnEditingModeChanged), IsValidMode);

	/// <summary>Identifies the EditingModeInverted property (default EraseByStroke).</summary>
	public static readonly DependencyProperty EditingModeInvertedProperty = DependencyProperty.Register(
		nameof(EditingModeInverted), typeof(InkCanvasEditingMode), typeof(InkCanvas),
		new FrameworkPropertyMetadata(InkCanvasEditingMode.EraseByStroke, OnEditingModeInvertedChanged), IsValidMode);

	static readonly DependencyPropertyKey _activeEditingModeKey = DependencyProperty.RegisterReadOnly(
		nameof(ActiveEditingMode), typeof(InkCanvasEditingMode), typeof(InkCanvas),
		new FrameworkPropertyMetadata(InkCanvasEditingMode.Ink));

	/// <summary>Identifies the read-only ActiveEditingMode property.</summary>
	public static readonly DependencyProperty ActiveEditingModeProperty = _activeEditingModeKey.DependencyProperty;

	/// <summary>Identifies the <see cref="EditingModeChanged"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent EditingModeChangedEvent = EventManager.RegisterRoutedEvent(
		nameof(EditingModeChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InkCanvas));

	/// <summary>Identifies the <see cref="EditingModeInvertedChanged"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent EditingModeInvertedChangedEvent = EventManager.RegisterRoutedEvent(
		nameof(EditingModeInvertedChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InkCanvas));

	static bool IsValidMode(object? v) => v is InkCanvasEditingMode m && m >= InkCanvasEditingMode.None
		&& m <= InkCanvasEditingMode.EraseByStroke;

	/// <summary>Gets or sets the brush behind the children.</summary>
	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}

	/// <summary>Gets or sets the editing mode of the primary input.</summary>
	public InkCanvasEditingMode EditingMode
	{
		get => (InkCanvasEditingMode)GetValue(EditingModeProperty)!;
		set => SetValue(EditingModeProperty, value);
	}

	/// <summary>Gets or sets the editing mode of the inverted (eraser) input.</summary>
	public InkCanvasEditingMode EditingModeInverted
	{
		get => (InkCanvasEditingMode)GetValue(EditingModeInvertedProperty)!;
		set => SetValue(EditingModeInvertedProperty, value);
	}

	/// <summary>The mode in effect; without an eraser input it follows <see cref="EditingMode"/>.</summary>
	public InkCanvasEditingMode ActiveEditingMode => (InkCanvasEditingMode)GetValue(ActiveEditingModeProperty)!;

	/// <summary>Occurs when <see cref="EditingMode"/> changes.</summary>
	public event RoutedEventHandler EditingModeChanged
	{
		add => AddHandler(EditingModeChangedEvent, value);
		remove => RemoveHandler(EditingModeChangedEvent, value);
	}

	/// <summary>Occurs when <see cref="EditingModeInverted"/> changes.</summary>
	public event RoutedEventHandler EditingModeInvertedChanged
	{
		add => AddHandler(EditingModeInvertedChangedEvent, value);
		remove => RemoveHandler(EditingModeInvertedChangedEvent, value);
	}

	static void OnEditingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var canvas = (InkCanvas)d;
		canvas.SetValue(_activeEditingModeKey, e.NewValue);
		canvas.RaiseEvent(new RoutedEventArgs(EditingModeChangedEvent));
	}

	static void OnEditingModeInvertedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((InkCanvas)d).RaiseEvent(new RoutedEventArgs(EditingModeInvertedChangedEvent));
}
