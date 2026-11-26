using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// The attached, inheriting text DPs (FontFamily, FontSize, FontStyle, FontWeight, FontStretch, Foreground).
/// <see cref="TextElement"/> is a <see cref="DependencyObject"/>, so its instance members read and write the same
/// DPs; <c>Control</c> and <c>TextBlock</c> are owners (<c>AddOwner</c>), so values inherit through them.
/// </summary>
public abstract partial class TextElement
{
	/// <summary>Identifies the attached FontFamily property (default "Segoe UI"; inherits; null is rejected).</summary>
	public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.RegisterAttached(
		"FontFamily", typeof(FontFamily), typeof(TextElement),
		new FrameworkPropertyMetadata(new FontFamily("Segoe UI"), true), v => v != null);

	/// <summary>Identifies the attached FontSize property (default 12 DIPs; inherits).
	/// Validated as in WPF: ArgumentException unless finite and in (0, 35791.394].</summary>
	public static readonly DependencyProperty FontSizeProperty = DependencyProperty.RegisterAttached(
		"FontSize", typeof(double), typeof(TextElement), new FrameworkPropertyMetadata(12.0, true),
		v => v is double d && d > 0 && d <= 35791.394);

	/// <summary>Identifies the attached FontStyle property (default Normal; inherits).</summary>
	public static readonly DependencyProperty FontStyleProperty = DependencyProperty.RegisterAttached(
		"FontStyle", typeof(FontStyle), typeof(TextElement), new FrameworkPropertyMetadata(FontStyles.Normal, true));

	/// <summary>Identifies the attached FontWeight property (default Normal; inherits).</summary>
	public static readonly DependencyProperty FontWeightProperty = DependencyProperty.RegisterAttached(
		"FontWeight", typeof(FontWeight), typeof(TextElement),
		new FrameworkPropertyMetadata(FontWeights.Normal, true));

	/// <summary>Identifies the attached FontStretch property (default Normal; inherits).</summary>
	public static readonly DependencyProperty FontStretchProperty = DependencyProperty.RegisterAttached(
		"FontStretch", typeof(FontStretch), typeof(TextElement),
		new FrameworkPropertyMetadata(FontStretches.Normal, true));

	/// <summary>Identifies the attached Foreground property (default ControlTextBrush; inherits).</summary>
	public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
		"Foreground", typeof(Brush), typeof(TextElement),
		new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, true));

	/// <summary>Gets the attached FontFamily.</summary>
	public static FontFamily GetFontFamily(DependencyObject element) => (FontFamily)Get(element, FontFamilyProperty)!;
	/// <summary>Sets the attached FontFamily.</summary>
	public static void SetFontFamily(DependencyObject element, FontFamily value) =>
		Set(element, FontFamilyProperty, value);
	/// <summary>Gets the attached FontSize.</summary>
	public static double GetFontSize(DependencyObject element) => (double)Get(element, FontSizeProperty)!;
	/// <summary>Sets the attached FontSize.</summary>
	public static void SetFontSize(DependencyObject element, double value) => Set(element, FontSizeProperty, value);
	/// <summary>Gets the attached FontStyle.</summary>
	public static FontStyle GetFontStyle(DependencyObject element) => (FontStyle)Get(element, FontStyleProperty)!;
	/// <summary>Sets the attached FontStyle.</summary>
	public static void SetFontStyle(DependencyObject element, FontStyle value) =>
		Set(element, FontStyleProperty, value);
	/// <summary>Gets the attached FontWeight.</summary>
	public static FontWeight GetFontWeight(DependencyObject element) => (FontWeight)Get(element, FontWeightProperty)!;
	/// <summary>Sets the attached FontWeight.</summary>
	public static void SetFontWeight(DependencyObject element, FontWeight value) =>
		Set(element, FontWeightProperty, value);
	/// <summary>Gets the attached FontStretch.</summary>
	public static FontStretch GetFontStretch(DependencyObject element) =>
		(FontStretch)Get(element, FontStretchProperty)!;
	/// <summary>Sets the attached FontStretch.</summary>
	public static void SetFontStretch(DependencyObject element, FontStretch value) =>
		Set(element, FontStretchProperty, value);
	/// <summary>Gets the attached Foreground.</summary>
	public static Brush GetForeground(DependencyObject element) => (Brush)Get(element, ForegroundProperty)!;
	/// <summary>Sets the attached Foreground.</summary>
	public static void SetForeground(DependencyObject element, Brush value) => Set(element, ForegroundProperty, value);

	static object? Get(DependencyObject element, DependencyProperty dp)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return element.GetValue(dp);
	}

	static void Set(DependencyObject element, DependencyProperty dp, object value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(dp, value);
	}
}
