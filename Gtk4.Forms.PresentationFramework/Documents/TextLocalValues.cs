using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// The typography values an element sets itself (local, style, trigger), as opposed to the effective value the
/// WPF-shaped instance properties return. Rendering and serialisation use these so inherited values are not
/// emitted on every element.
/// </summary>
static class TextLocalValues
{
	static object? Local(DependencyObject d, DependencyProperty dp) =>
		d.HasNonInheritedValue(dp) ? d.GetValue(dp) : null;

	internal static FontFamily? Family(DependencyObject d) => (FontFamily?)Local(d, TextElement.FontFamilyProperty);

	internal static FontWeight? Weight(DependencyObject d) => (FontWeight?)Local(d, TextElement.FontWeightProperty);

	internal static FontStyle? Style(DependencyObject d) => (FontStyle?)Local(d, TextElement.FontStyleProperty);

	internal static FontStretch? Stretch(DependencyObject d) =>
		(FontStretch?)Local(d, TextElement.FontStretchProperty);

	internal static Brush? Brush(DependencyObject d) => (Brush?)Local(d, TextElement.ForegroundProperty);

	// The formatting the Pango markup path emits.
	internal static bool HasMarkupFormat(TextElement e) => Weight(e) != null || Style(e) != null || Brush(e) != null;

	internal static double? Size(DependencyObject d) => (double?)Local(d, TextElement.FontSizeProperty);
}
