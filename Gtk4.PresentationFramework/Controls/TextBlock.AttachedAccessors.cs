using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>Static attached-property accessors, as on WPF's <c>TextBlock</c>.</summary>
public partial class TextBlock
{
	static DependencyObject Check(DependencyObject element) =>
		element ?? throw new ArgumentNullException(nameof(element));

	/// <summary>Sets the foreground on an element.</summary>
	public static void SetForeground(DependencyObject element, Brush value) =>
		Check(element).SetValue(ForegroundProperty, value);

	/// <summary>Gets the foreground of an element.</summary>
	public static Brush GetForeground(DependencyObject element) => (Brush)Check(element).GetValue(ForegroundProperty)!;

	/// <summary>Sets the font family on an element.</summary>
	public static void SetFontFamily(DependencyObject element, FontFamily value) =>
		Check(element).SetValue(FontFamilyProperty, value);

	/// <summary>Gets the font family of an element.</summary>
	public static FontFamily GetFontFamily(DependencyObject element) =>
		(FontFamily)Check(element).GetValue(FontFamilyProperty)!;

	/// <summary>Sets the font size on an element.</summary>
	public static void SetFontSize(DependencyObject element, double value) =>
		Check(element).SetValue(FontSizeProperty, value);

	/// <summary>Gets the font size of an element.</summary>
	public static double GetFontSize(DependencyObject element) => (double)Check(element).GetValue(FontSizeProperty)!;

	/// <summary>Sets the font style on an element.</summary>
	public static void SetFontStyle(DependencyObject element, FontStyle value) =>
		Check(element).SetValue(FontStyleProperty, value);

	/// <summary>Gets the font style of an element.</summary>
	public static FontStyle GetFontStyle(DependencyObject element) =>
		(FontStyle)Check(element).GetValue(FontStyleProperty)!;

	/// <summary>Sets the font weight on an element.</summary>
	public static void SetFontWeight(DependencyObject element, FontWeight value) =>
		Check(element).SetValue(FontWeightProperty, value);

	/// <summary>Gets the font weight of an element.</summary>
	public static FontWeight GetFontWeight(DependencyObject element) =>
		(FontWeight)Check(element).GetValue(FontWeightProperty)!;

	/// <summary>Sets the font stretch on an element.</summary>
	public static void SetFontStretch(DependencyObject element, FontStretch value) =>
		Check(element).SetValue(FontStretchProperty, value);

	/// <summary>Gets the font stretch of an element.</summary>
	public static FontStretch GetFontStretch(DependencyObject element) =>
		(FontStretch)Check(element).GetValue(FontStretchProperty)!;

	/// <summary>Sets the baseline offset on an element.</summary>
	public static void SetBaselineOffset(DependencyObject element, double value) =>
		Check(element).SetValue(BaselineOffsetProperty, value);

	/// <summary>Gets the baseline offset of an element.</summary>
	public static double GetBaselineOffset(DependencyObject element) =>
		(double)Check(element).GetValue(BaselineOffsetProperty)!;

	/// <summary>Sets the line height on an element.</summary>
	public static void SetLineHeight(DependencyObject element, double value) =>
		Check(element).SetValue(LineHeightProperty, value);

	/// <summary>Gets the line height of an element.</summary>
	public static double GetLineHeight(DependencyObject element) =>
		(double)Check(element).GetValue(LineHeightProperty)!;

	/// <summary>Sets the line stacking strategy on an element.</summary>
	public static void SetLineStackingStrategy(DependencyObject element, LineStackingStrategy value) =>
		Check(element).SetValue(LineStackingStrategyProperty, value);

	/// <summary>Gets the line stacking strategy of an element.</summary>
	public static LineStackingStrategy GetLineStackingStrategy(DependencyObject element) =>
		(LineStackingStrategy)Check(element).GetValue(LineStackingStrategyProperty)!;

	/// <summary>Sets the text alignment on an element.</summary>
	public static void SetTextAlignment(DependencyObject element, TextAlignment value) =>
		Check(element).SetValue(TextAlignmentProperty, value);

	/// <summary>Gets the text alignment of an element.</summary>
	public static TextAlignment GetTextAlignment(DependencyObject element) =>
		(TextAlignment)Check(element).GetValue(TextAlignmentProperty)!;
}
