namespace System.Windows;

public partial class Window
{
	// ── SizeToContent ─────────────────────────────────────────────────────────

	/// <summary>Gets or sets whether the window auto-sizes to fit its content.</summary>
	public static readonly DependencyProperty SizeToContentProperty =
		DependencyProperty.Register(
			nameof(SizeToContent),
			typeof(SizeToContent),
			typeof(Window),
			new PropertyMetadata(SizeToContent.Manual));

	/// <summary>
	/// Gets or sets whether the window auto-sizes to fit its content.
	/// When set to anything other than <see cref="SizeToContent.Manual"/>, the
	/// affected dimension(s) are left at the GTK natural size (no explicit size request),
	/// so the window wraps its content.
	/// </summary>
	public SizeToContent SizeToContent
	{
		get => (SizeToContent)(GetValue(SizeToContentProperty) ?? SizeToContent.Manual);
		set => SetValue(SizeToContentProperty, value);
	}
}
