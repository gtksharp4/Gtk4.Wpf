using System.Windows.Media;

namespace System.Windows;

/// <summary>
/// System font settings. GTK does not hand out the Windows message font, so these are the layer defaults (the
/// same family, size, style and weight <see cref="Documents.TextElement"/> defaults to).
/// </summary>
public static class SystemFonts
{
	/// <summary>The message font family.</summary>
	public static FontFamily MessageFontFamily => new FontFamily("Segoe UI");

	/// <summary>The message font size in DIPs.</summary>
	public static double MessageFontSize => 12.0;

	/// <summary>The message font style.</summary>
	public static FontStyle MessageFontStyle => FontStyles.Normal;

	/// <summary>The message font weight.</summary>
	public static FontWeight MessageFontWeight => FontWeights.Normal;
}
