namespace System.Windows.Controls;

/// <summary>
/// Arranges children in a single line (horizontal or vertical) using a
/// <see cref="Gtk.Box"/>.
/// </summary>
public partial class StackPanel : Panel
{
	/// <summary>Orientation of the stack. Default is <see cref="Orientation.Vertical"/>.</summary>
	public static readonly DependencyProperty OrientationProperty =
		DependencyProperty.Register(
			nameof(Orientation),
			typeof(Orientation),
			typeof(StackPanel),
			new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

	static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((StackPanel)d).ApplyOrientation();
	}

	/// <summary>Gets or sets the layout direction.</summary>
	public Orientation Orientation
	{
		get => (Orientation)GetValue(OrientationProperty)!;
		set => SetValue(OrientationProperty, value);
	}

	Gtk.Box? _box;

	/// <inheritdoc/>
	internal override bool IsWidgetCreated => _box != null;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_box = new BackdropBox(ToGtk(Orientation), 0);
		// Pack any children added before the widget was created.
		foreach (var child in Children)
			_box.Add(child.Widget);
		return _box;
	}

	/// <inheritdoc/>
	protected override void AddChildWidget(UIElement child)
	{
		_box!.Add(child.Widget);
	}

	/// <inheritdoc/>
	protected override void RemoveChildWidget(UIElement child)
	{
		_box!.Remove(child.Widget);
	}

	void ApplyOrientation()
	{
		if (_box != null)
			_box.Orientation = ToGtk(Orientation);
	}

	static Gtk.Orientation ToGtk(Orientation o) =>
		o == Orientation.Horizontal
			? Gtk.Orientation.Horizontal
			: Gtk.Orientation.Vertical;
}