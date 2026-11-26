using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// TextBlock background and text DPs, shared with <see cref="Panel"/> / <see cref="TextElement"/> via
/// <c>AddOwner</c>. CSS lands on the backing <c>Gtk.Label</c>; the Pango markup path is untouched.
/// </summary>
public partial class TextBlock
{
	/// <summary>Identifies the <see cref="Background"/> dependency property.</summary>
	public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(
		typeof(TextBlock),
		new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue));

	/// <summary>Identifies the <see cref="Foreground"/> dependency property.</summary>
	public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
		typeof(TextBlock), InheritingTextMetadata(TextElement.ForegroundProperty, false));

	/// <summary>Identifies the <see cref="FontFamily"/> dependency property.</summary>
	public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
		typeof(TextBlock), InheritingTextMetadata(TextElement.FontFamilyProperty, true));

	/// <summary>Identifies the <see cref="FontSize"/> dependency property.</summary>
	public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
		typeof(TextBlock), InheritingTextMetadata(TextElement.FontSizeProperty, true));

	/// <summary>Identifies the <see cref="FontStyle"/> dependency property.</summary>
	public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
		typeof(TextBlock), InheritingTextMetadata(TextElement.FontStyleProperty, true));

	/// <summary>Identifies the <see cref="FontWeight"/> dependency property.</summary>
	public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
		typeof(TextBlock), InheritingTextMetadata(TextElement.FontWeightProperty, true));

	/// <summary>Identifies the <see cref="FontStretch"/> dependency property.</summary>
	public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
		typeof(TextBlock), InheritingTextMetadata(TextElement.FontStretchProperty, true));

	/// <summary>Gets or sets the background brush. Does not inherit.
	/// GTK limitation: Padding is applied as widget margins, which the CSS background does not cover;
	/// WPF fills Background over the padding too.</summary>
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

	/// <summary>Gets or sets the font size in device-independent px (inherits).</summary>
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
