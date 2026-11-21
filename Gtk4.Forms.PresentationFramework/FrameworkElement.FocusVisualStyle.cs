namespace System.Windows;

/// <summary>
/// <see cref="FocusVisualStyle"/>. GTK draws its own focus ring and has no per-widget focus-visual
/// template, so the style is stored and returned but not applied.
/// </summary>
public partial class FrameworkElement
{
	static readonly Style _defaultFocusVisualStyle = CreateDefaultFocusVisualStyle();

	static Style CreateDefaultFocusVisualStyle()
	{
		var style = new Style();
		style.Seal();
		return style;
	}

	/// <summary>Identifies the <see cref="FocusVisualStyle"/> property. Default: a shared sealed style.</summary>
	public static readonly DependencyProperty FocusVisualStyleProperty =
		DependencyProperty.Register(nameof(FocusVisualStyle), typeof(Style), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(_defaultFocusVisualStyle));

	/// <summary>Gets or sets the style that draws the focus rectangle. Stored only on GTK.</summary>
	public Style? FocusVisualStyle
	{
		get => (Style?)GetValue(FocusVisualStyleProperty);
		set => SetValue(FocusVisualStyleProperty, value);
	}
}
