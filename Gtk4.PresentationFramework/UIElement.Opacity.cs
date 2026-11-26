namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>
	/// Opacity DP: double in [0,1], default 1.0.
	/// Applied to the backing <see cref="Gtk.Widget"/> immediately when the widget exists;
	/// deferred to <see cref="OnWidgetCreated"/> otherwise (lazy-safe). A <see cref="Visibility.Hidden"/>
	/// element keeps GTK opacity 0 whatever this value is (see <c>UIElement.Visibility.cs</c>).
	/// </summary>
	public static readonly DependencyProperty OpacityProperty =
		DependencyProperty.Register(
			nameof(Opacity),
			typeof(double),
			typeof(UIElement),
			new PropertyMetadata(1.0, OnOpacityChanged));

	static void OnOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement el && el.HasBackingWidget)
			el._widget!.Opacity = VisibilityGtkMap.EffectiveOpacity(
				el.WidgetVisibility, (double)(e.NewValue ?? 1.0));
	}

	/// <summary>Gets or sets the opacity (0.0 = fully transparent, 1.0 = fully opaque).</summary>
	public double Opacity
	{
		get => (double)(GetValue(OpacityProperty) ?? 1.0);
		set => SetValue(OpacityProperty, value);
	}
}
