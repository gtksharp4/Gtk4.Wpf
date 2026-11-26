namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>Identifies the <see cref="SnapsToDevicePixels"/> dependency property.</summary>
	public static readonly DependencyProperty SnapsToDevicePixelsProperty =
		DependencyProperty.Register(nameof(SnapsToDevicePixels), typeof(bool), typeof(UIElement),
			new PropertyMetadata(false));

	/// <summary>
	/// Gets or sets whether rendering aligns to device pixels. Stored only: GTK 4 snaps to the
	/// device pixel grid itself and exposes no per-element switch.
	/// </summary>
	public bool SnapsToDevicePixels
	{
		get => (bool)(GetValue(SnapsToDevicePixelsProperty) ?? false);
		set => SetValue(SnapsToDevicePixelsProperty, value);
	}
}
