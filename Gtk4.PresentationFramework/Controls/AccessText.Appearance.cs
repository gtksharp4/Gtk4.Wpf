using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// The TextBlock-like appearance properties of <see cref="AccessText"/>, owned through <c>AddOwner</c>.
/// </summary>
public partial class AccessText
{
	/// <summary>Identifies the <see cref="Background"/> dependency property.</summary>
	public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(
		typeof(AccessText), new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue));

	/// <summary>Identifies the <see cref="Foreground"/> dependency property.</summary>
	public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
		typeof(AccessText), InheritingTextMetadata(TextElement.ForegroundProperty, false));

	/// <summary>Identifies the <see cref="FontFamily"/> dependency property.</summary>
	public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
		typeof(AccessText), InheritingTextMetadata(TextElement.FontFamilyProperty, true));

	/// <summary>Identifies the <see cref="FontSize"/> dependency property.</summary>
	public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
		typeof(AccessText), InheritingTextMetadata(TextElement.FontSizeProperty, true));

	/// <summary>Identifies the <see cref="FontStyle"/> dependency property.</summary>
	public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
		typeof(AccessText), InheritingTextMetadata(TextElement.FontStyleProperty, true));

	/// <summary>Identifies the <see cref="FontWeight"/> dependency property.</summary>
	public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
		typeof(AccessText), InheritingTextMetadata(TextElement.FontWeightProperty, true));

	/// <summary>Identifies the <see cref="FontStretch"/> dependency property.</summary>
	public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
		typeof(AccessText), InheritingTextMetadata(TextElement.FontStretchProperty, true));

	/// <summary>Gets or sets the background brush.</summary>
	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}

	/// <summary>Gets or sets the text brush (inherits).</summary>
	public Brush Foreground
	{
		get => (Brush)GetValue(ForegroundProperty)!;
		set => SetValue(ForegroundProperty, value);
	}

	/// <summary>Gets or sets the font family (inherits).</summary>
	public FontFamily FontFamily
	{
		get => (FontFamily)GetValue(FontFamilyProperty)!;
		set => SetValue(FontFamilyProperty, value);
	}

	/// <summary>Gets or sets the font size (inherits).</summary>
	public double FontSize
	{
		get => (double)GetValue(FontSizeProperty)!;
		set => SetValue(FontSizeProperty, value);
	}

	/// <summary>Gets or sets the font style (inherits).</summary>
	public FontStyle FontStyle
	{
		get => (FontStyle)GetValue(FontStyleProperty)!;
		set => SetValue(FontStyleProperty, value);
	}

	/// <summary>Gets or sets the font weight (inherits).</summary>
	public FontWeight FontWeight
	{
		get => (FontWeight)GetValue(FontWeightProperty)!;
		set => SetValue(FontWeightProperty, value);
	}

	/// <summary>Gets or sets the font stretch (inherits).</summary>
	public FontStretch FontStretch
	{
		get => (FontStretch)GetValue(FontStretchProperty)!;
		set => SetValue(FontStretchProperty, value);
	}

	internal override string BuildAppearanceCss() =>
		ElementCssStyle.Join(ElementCssStyle.Background(this), ElementCssStyle.Text(this));
}
