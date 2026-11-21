using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// Appearance and navigation-host properties of <see cref="Page"/>. Background and the text properties
/// render through the same CSS path as <see cref="Panel"/> and <see cref="TextBlock"/>.
/// <see cref="Template"/>, <see cref="KeepAlive"/> and <see cref="ShowsNavigationUI"/> are store-only:
/// the page has no template visuals, and there is no journal or navigation chrome to consume them.
/// </summary>
public partial class Page
{
	/// <summary>Identifies the <see cref="Background"/> dependency property (null default; does not inherit).</summary>
	public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(
		typeof(Page), new FrameworkPropertyMetadata(null, OnAppearanceChanged));

	/// <summary>Identifies the <see cref="Foreground"/> dependency property.</summary>
	public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
		typeof(Page), InheritingTextMetadata(TextElement.ForegroundProperty, false));

	/// <summary>Identifies the <see cref="FontFamily"/> dependency property.</summary>
	public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
		typeof(Page), InheritingTextMetadata(TextElement.FontFamilyProperty, true));

	/// <summary>Identifies the <see cref="FontSize"/> dependency property.</summary>
	public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
		typeof(Page), InheritingTextMetadata(TextElement.FontSizeProperty, true));

	/// <summary>Identifies the <see cref="Template"/> dependency property.</summary>
	public static readonly DependencyProperty TemplateProperty = Control.TemplateProperty.AddOwner(
		typeof(Page), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="KeepAlive"/> dependency property.</summary>
	public static readonly DependencyProperty KeepAliveProperty = DependencyProperty.Register(
		nameof(KeepAlive), typeof(bool), typeof(Page), new PropertyMetadata(false));

	/// <summary>Gets or sets the brush that fills the page.</summary>
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

	/// <summary>Gets or sets the control template (store-only).</summary>
	public ControlTemplate? Template
	{
		get => (ControlTemplate?)GetValue(TemplateProperty);
		set => SetValue(TemplateProperty, value);
	}

	/// <summary>Gets or sets whether the page stays alive in the journal (store-only).</summary>
	public bool KeepAlive
	{
		get => (bool)GetValue(KeepAliveProperty)!;
		set => SetValue(KeepAliveProperty, value);
	}

	/// <summary>Gets or sets whether the host shows its navigation UI for this page (store-only).</summary>
	public bool ShowsNavigationUI { get; set; } = true;

	internal override string BuildAppearanceCss() =>
		ElementCssStyle.Join(ElementCssStyle.Background(this), ElementCssStyle.Text(this));
}
