using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// Border background / brush / corner radius. CSS goes on the backing <c>Gtk.Frame</c> node, which keeps
/// its child. Gap: Image/Visual/Drawing brushes are store-only.
/// </summary>
public partial class Border
{
	static Border() { }

	/// <summary>Identifies the <see cref="Background"/> dependency property.</summary>
	public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(
		typeof(Border),
		new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue));

	/// <summary>Identifies the <see cref="BorderBrush"/> dependency property.</summary>
	public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
		nameof(BorderBrush), typeof(Brush), typeof(Border),
		new FrameworkPropertyMetadata(null, OnAppearanceChanged));

	/// <summary>Identifies the <see cref="CornerRadius"/> dependency property.</summary>
	public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
		nameof(CornerRadius), typeof(CornerRadius), typeof(Border),
		new FrameworkPropertyMetadata(new CornerRadius(), OnAppearanceChanged));

	/// <summary>Gets or sets the background brush.</summary>
	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}

	/// <summary>Gets or sets the brush that draws the border.</summary>
	public Brush? BorderBrush
	{
		get => (Brush?)GetValue(BorderBrushProperty);
		set => SetValue(BorderBrushProperty, value);
	}

	/// <summary>Gets or sets the corner radius.</summary>
	public CornerRadius CornerRadius
	{
		get => (CornerRadius)GetValue(CornerRadiusProperty)!;
		set => SetValue(CornerRadiusProperty, value);
	}

	internal override string BuildAppearanceCss()
	{
		string radius = HasNonInheritedValue(CornerRadiusProperty) ? BrushCss.BorderRadius(CornerRadius) : string.Empty;
		return ElementCssStyle.Join(ElementCssStyle.Background(this), ElementCssStyle.Border(this), radius);
	}
}
