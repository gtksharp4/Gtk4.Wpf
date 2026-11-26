namespace System.Windows;

public partial class Window
{
	/// <summary>
	/// Gets or sets whether the window is activated when first shown (default true). Stored only:
	/// GTK 4 gives no portable way to map a toplevel without focusing it.
	/// </summary>
	public bool ShowActivated
	{
		get => (bool)GetValue(ShowActivatedProperty)!;
		set => SetValue(ShowActivatedProperty, value);
	}

	/// <summary>Identifies the <see cref="ShowActivated"/> dependency property.</summary>
	public static readonly DependencyProperty ShowActivatedProperty = DependencyProperty.Register(
		nameof(ShowActivated), typeof(bool), typeof(Window), new FrameworkPropertyMetadata(true));
}
