using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// Background / border / text DPs of <see cref="Control"/>. They share their <see cref="DependencyProperty"/>
/// instances with <see cref="Panel"/>, <see cref="Border"/> and <see cref="TextElement"/> via
/// <c>AddOwner</c>, as in WPF. GTK mapping: see <see cref="ElementCssStyle"/>.
/// </summary>
public abstract partial class Control
{

	/// <summary>Identifies the <see cref="Background"/> dependency property.</summary>
	public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(
		typeof(Control),
		new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue));

	/// <summary>Identifies the <see cref="BorderBrush"/> dependency property.</summary>
	public static readonly DependencyProperty BorderBrushProperty = Border.BorderBrushProperty.AddOwner(
		typeof(Control),
		new FrameworkPropertyMetadata(Border.BorderBrushProperty.DefaultMetadata.DefaultValue));

	/// <summary>Identifies the <see cref="BorderThickness"/> dependency property.</summary>
	public static readonly DependencyProperty BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner(
		typeof(Control), new FrameworkPropertyMetadata(new Thickness()));

	/// <summary>Identifies the <see cref="Foreground"/> dependency property.</summary>
	public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
		typeof(Control), InheritingTextMetadata(TextElement.ForegroundProperty, false));

	/// <summary>Identifies the <see cref="FontFamily"/> dependency property.</summary>
	public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
		typeof(Control), InheritingTextMetadata(TextElement.FontFamilyProperty, true));

	/// <summary>Identifies the <see cref="FontSize"/> dependency property.</summary>
	public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
		typeof(Control), InheritingTextMetadata(TextElement.FontSizeProperty, true));

	/// <summary>Identifies the <see cref="FontStyle"/> dependency property.</summary>
	public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
		typeof(Control), InheritingTextMetadata(TextElement.FontStyleProperty, true));

	/// <summary>Identifies the <see cref="FontWeight"/> dependency property.</summary>
	public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
		typeof(Control), InheritingTextMetadata(TextElement.FontWeightProperty, true));

	/// <summary>Identifies the <see cref="FontStretch"/> dependency property.</summary>
	public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
		typeof(Control), InheritingTextMetadata(TextElement.FontStretchProperty, true));

	/// <summary>Gets or sets the brush that fills the control background. Does not inherit.</summary>
	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}

	/// <summary>Gets or sets the brush that draws the control border.</summary>
	public Brush? BorderBrush
	{
		get => (Brush?)GetValue(BorderBrushProperty);
		set => SetValue(BorderBrushProperty, value);
	}

	/// <summary>Gets or sets the border thickness.</summary>
	public Thickness BorderThickness
	{
		get => (Thickness)GetValue(BorderThicknessProperty)!;
		set => SetValue(BorderThicknessProperty, value);
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
}
