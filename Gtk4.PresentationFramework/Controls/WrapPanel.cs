using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>
/// Arranges children in sequence, wrapping to the next line/column when space runs out.
///
/// GTK backing: <see cref="Gtk.FlowBox"/> (GTK 4). Children are wrapped in
/// <see cref="Gtk.FlowBoxChild"/> (the required FlowBox child container).
/// <see cref="Orientation.Horizontal"/> (default) → flow left-to-right then wrap down;
/// <see cref="Orientation.Vertical"/> → flow top-to-bottom then wrap right.
///
/// Approximation: FlowBox sizes all children to the same minimum cell size (the
/// natural size of the largest child). WPF's WrapPanel sizes each child independently.
/// This diverges from WPF for mixed-size children; noted for Phase 2.
/// </summary>
public partial class WrapPanel : Panel
{
	/// <summary>Direction children flow before wrapping.</summary>
	public static readonly DependencyProperty OrientationProperty =
		DependencyProperty.Register(
			nameof(Orientation), typeof(Orientation), typeof(WrapPanel),
			new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

	static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((WrapPanel)d).ApplyOrientation();
	}

	/// <summary>Gets or sets the layout direction.</summary>
	public Orientation Orientation
	{
		get => (Orientation)GetValue(OrientationProperty)!;
		set => SetValue(OrientationProperty, value);
	}

	Gtk.FlowBox? _flow;

	// Mapping from UIElement → its FlowBoxChild wrapper.
	readonly Dictionary<UIElement, Gtk.FlowBoxChild> _wrappers =
		new Dictionary<UIElement, Gtk.FlowBoxChild>();

	/// <inheritdoc/>
	internal override bool IsWidgetCreated => _flow != null;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_flow = new Gtk.FlowBox();
		_flow.Orientation = ToGtk(Orientation);
		_flow.Homogeneous = false;
		_flow.MinChildrenPerLine = 1;
		_flow.MaxChildrenPerLine = 1000;
		_flow.SelectionMode = Gtk.SelectionMode.None;
		foreach (var child in Children)
			WrapAndAppend(child);
		return _flow;
	}

	/// <inheritdoc/>
	protected override void AddChildWidget(UIElement child)
	{
		WrapAndAppend(child);
	}

	/// <inheritdoc/>
	protected override void RemoveChildWidget(UIElement child)
	{
		if (_wrappers.TryGetValue(child, out var wrapper))
		{
			_flow!.Remove(wrapper);
			_wrappers.Remove(child);
		}
	}

	void WrapAndAppend(UIElement child)
	{
		var wrapper = new Gtk.FlowBoxChild();
		wrapper.Child = child.Widget;
		wrapper.Hexpand = true;
		_wrappers[child] = wrapper;
		_flow!.Append(wrapper);
	}

	void ApplyOrientation()
	{
		if (_flow != null)
			_flow.Orientation = ToGtk(Orientation);
	}

	static Gtk.Orientation ToGtk(Orientation o) =>
		o == Orientation.Horizontal
			? Gtk.Orientation.Horizontal
			: Gtk.Orientation.Vertical;
}