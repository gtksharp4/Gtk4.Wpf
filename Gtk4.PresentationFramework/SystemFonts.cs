using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows;

/// <summary>
/// System font settings. GTK does not hand out the Windows system fonts, so every group (caption, icon, menu,
/// message, small caption, status) is the layer default: the same family, size, style and weight
/// <see cref="Documents.TextElement"/> defaults to. The <c>*Key</c> members are resource keys that
/// <c>FindResource</c> resolves to the matching value.
/// </summary>
public static partial class SystemFonts
{
	/// <summary>The Message font family.</summary>
	public static FontFamily MessageFontFamily => new FontFamily("Segoe UI");

	/// <summary>The Message font size in DIPs.</summary>
	public static double MessageFontSize => 12.0;

	/// <summary>The Message font style.</summary>
	public static FontStyle MessageFontStyle => FontStyles.Normal;

	/// <summary>The Message font weight.</summary>
	public static FontWeight MessageFontWeight => FontWeights.Normal;

	/// <summary>The Message font text decorations (none).</summary>
	public static TextDecorationCollection MessageFontTextDecorations => new TextDecorationCollection();

	/// <summary>The Caption font family.</summary>
	public static FontFamily CaptionFontFamily => new FontFamily("Segoe UI");

	/// <summary>The Caption font size in DIPs.</summary>
	public static double CaptionFontSize => 12.0;

	/// <summary>The Caption font style.</summary>
	public static FontStyle CaptionFontStyle => FontStyles.Normal;

	/// <summary>The Caption font weight.</summary>
	public static FontWeight CaptionFontWeight => FontWeights.Normal;

	/// <summary>The Caption font text decorations (none).</summary>
	public static TextDecorationCollection CaptionFontTextDecorations => new TextDecorationCollection();

	/// <summary>The Icon font family.</summary>
	public static FontFamily IconFontFamily => new FontFamily("Segoe UI");

	/// <summary>The Icon font size in DIPs.</summary>
	public static double IconFontSize => 12.0;

	/// <summary>The Icon font style.</summary>
	public static FontStyle IconFontStyle => FontStyles.Normal;

	/// <summary>The Icon font weight.</summary>
	public static FontWeight IconFontWeight => FontWeights.Normal;

	/// <summary>The Icon font text decorations (none).</summary>
	public static TextDecorationCollection IconFontTextDecorations => new TextDecorationCollection();

	/// <summary>The Menu font family.</summary>
	public static FontFamily MenuFontFamily => new FontFamily("Segoe UI");

	/// <summary>The Menu font size in DIPs.</summary>
	public static double MenuFontSize => 12.0;

	/// <summary>The Menu font style.</summary>
	public static FontStyle MenuFontStyle => FontStyles.Normal;

	/// <summary>The Menu font weight.</summary>
	public static FontWeight MenuFontWeight => FontWeights.Normal;

	/// <summary>The Menu font text decorations (none).</summary>
	public static TextDecorationCollection MenuFontTextDecorations => new TextDecorationCollection();

	/// <summary>The SmallCaption font family.</summary>
	public static FontFamily SmallCaptionFontFamily => new FontFamily("Segoe UI");

	/// <summary>The SmallCaption font size in DIPs.</summary>
	public static double SmallCaptionFontSize => 12.0;

	/// <summary>The SmallCaption font style.</summary>
	public static FontStyle SmallCaptionFontStyle => FontStyles.Normal;

	/// <summary>The SmallCaption font weight.</summary>
	public static FontWeight SmallCaptionFontWeight => FontWeights.Normal;

	/// <summary>The SmallCaption font text decorations (none).</summary>
	public static TextDecorationCollection SmallCaptionFontTextDecorations => new TextDecorationCollection();

	/// <summary>The Status font family.</summary>
	public static FontFamily StatusFontFamily => new FontFamily("Segoe UI");

	/// <summary>The Status font size in DIPs.</summary>
	public static double StatusFontSize => 12.0;

	/// <summary>The Status font style.</summary>
	public static FontStyle StatusFontStyle => FontStyles.Normal;

	/// <summary>The Status font weight.</summary>
	public static FontWeight StatusFontWeight => FontWeights.Normal;

	/// <summary>The Status font text decorations (none).</summary>
	public static TextDecorationCollection StatusFontTextDecorations => new TextDecorationCollection();
}
