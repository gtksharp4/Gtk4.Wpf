namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>
	/// Whether content is clipped to the element's bounds. Default: false. Maps to
	/// <c>Gtk.Widget.Overflow</c> (<c>Hidden</c> when true). GTK clips to the widget's allocation,
	/// not to WPF's layout slot, which differs only for elements larger than their slot.
	/// </summary>
	public static readonly DependencyProperty ClipToBoundsProperty =
		DependencyProperty.Register(
			nameof(ClipToBounds),
			typeof(bool),
			typeof(UIElement),
			new PropertyMetadata(false, OnClipToBoundsChanged));

	static void OnClipToBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement el && el.HasBackingWidget)
			el.ApplyClipToBounds(el._widget!);
	}

	/// <summary>Gets or sets whether content is clipped to the element's bounds.</summary>
	public bool ClipToBounds
	{
		get => (bool)(GetValue(ClipToBoundsProperty) ?? false);
		set => SetValue(ClipToBoundsProperty, value);
	}

	void ApplyClipToBounds(Gtk.Widget widget)
		=> widget.Overflow = ClipToBounds ? Gtk.Overflow.Hidden : Gtk.Overflow.Visible;
}
