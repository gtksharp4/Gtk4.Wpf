using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// The document's default font family, size and foreground. They keep their original CLR shape (string family,
/// <c>NaN</c> and <c>null</c> meaning inherit) and are backed by the shared text DPs, so a XAML attribute and the
/// CLR property agree.
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

	/// <summary>Default font family for the document, or <c>null</c> to inherit.</summary>
	public string? FontFamily
	{
		get => HasNonInheritedValue(FontFamilyProperty) ? ((FontFamily)GetValue(FontFamilyProperty)!).Source : null;
		set
		{
			if (value == null) ClearValue(FontFamilyProperty);
			else SetValue(FontFamilyProperty, new FontFamily(value));
		}
	}

	/// <summary>Default font size in device-independent pixels, or <c>double.NaN</c> to inherit.</summary>
	public double FontSize
	{
		get => HasNonInheritedValue(FontSizeProperty) ? (double)GetValue(FontSizeProperty)! : double.NaN;
		set
		{
			if (double.IsNaN(value)) ClearValue(FontSizeProperty);
			else SetValue(FontSizeProperty, value);
		}
	}

	/// <summary>Default foreground brush, or <c>null</c> to inherit.</summary>
	public Brush? Foreground
	{
		get => HasNonInheritedValue(ForegroundProperty) ? (Brush?)GetValue(ForegroundProperty) : null;
		set
		{
			if (value == null) ClearValue(ForegroundProperty);
			else SetValue(ForegroundProperty, value);
		}
	}
}
