using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// The document's default font family, size and foreground, with their WPF shape: effective values backed by the
/// shared text DPs, so a XAML attribute and the CLR property agree.
/// </summary>
public partial class FlowDocument
{
	/// <summary>Identifies the FontFamily property (shared with TextElement).</summary>
	public static readonly DependencyProperty FontFamilyProperty =
		TextElement.FontFamilyProperty.AddOwner(typeof(FlowDocument));

	/// <summary>Identifies the FontSize property (shared with TextElement).</summary>
	public static readonly DependencyProperty FontSizeProperty =
		TextElement.FontSizeProperty.AddOwner(typeof(FlowDocument));

	/// <summary>Identifies the Foreground property (shared with TextElement).</summary>
	public static readonly DependencyProperty ForegroundProperty =
		TextElement.ForegroundProperty.AddOwner(typeof(FlowDocument));

	/// <summary>Gets or sets the default font family (effective value; null is rejected, as in WPF).</summary>
	public FontFamily FontFamily
	{
		get => (FontFamily)GetValue(FontFamilyProperty)!;
		set => SetValue(FontFamilyProperty, value);
	}

	/// <summary>Gets or sets the default font size in device-independent pixels (effective value).</summary>
	public double FontSize
	{
		get => (double)GetValue(FontSizeProperty)!;
		set => SetValue(FontSizeProperty, value);
	}

	/// <summary>Gets or sets the default foreground brush (effective value).</summary>
	public Brush Foreground
	{
		get => (Brush)GetValue(ForegroundProperty)!;
		set => SetValue(ForegroundProperty, value);
	}
}
