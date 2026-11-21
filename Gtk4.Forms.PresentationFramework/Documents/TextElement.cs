using System;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Base class for all flow-document elements. Carries optional typography properties
/// that affect Pango markup rendering when set on an inline.
/// <para>
/// The instance properties below keep their original CLR shape (a string family, nullable weight, style and
/// brush, <c>null</c> meaning inherit) and are backed by the attached text DPs: they read as <c>null</c> until
/// the DP is set locally, by a style or by a trigger, and assigning <c>null</c> clears it. XAML attributes
/// reach them through the DPs (<c>Run.FontWeightProperty</c>), so both routes agree.
/// </para>
/// </summary>
public abstract partial class TextElement : FrameworkContentElement
{
	/// <summary>Font family name, or <c>null</c> to inherit.</summary>
	public string? FontFamily
	{
		get => HasNonInheritedValue(FontFamilyProperty) ? ((FontFamily)GetValue(FontFamilyProperty)!).Source : null;
		set => SetOrClear(FontFamilyProperty, value == null ? null : new FontFamily(value));
	}

	/// <summary>Font weight (e.g. <c>FontWeights.Bold</c>), or <c>null</c> to inherit.</summary>
	public FontWeight? FontWeight
	{
		get => HasNonInheritedValue(FontWeightProperty) ? (FontWeight)GetValue(FontWeightProperty)! : (FontWeight?)null;
		set => SetOrClear(FontWeightProperty, value);
	}

	/// <summary>Font style (e.g. <c>FontStyles.Italic</c>), or <c>null</c> to inherit.</summary>
	public FontStyle? FontStyle
	{
		get => HasNonInheritedValue(FontStyleProperty) ? (FontStyle)GetValue(FontStyleProperty)! : (FontStyle?)null;
		set => SetOrClear(FontStyleProperty, value);
	}

	/// <summary>Foreground brush, or <c>null</c> to inherit.</summary>
	public Brush? Foreground
	{
		get => HasNonInheritedValue(ForegroundProperty) ? (Brush?)GetValue(ForegroundProperty) : null;
		set => SetOrClear(ForegroundProperty, value);
	}

	/// <summary>Text decorations (underline, strikethrough, etc.), or <c>null</c> for none.</summary>
	public TextDecorationCollection? TextDecorations { get; set; }

	void SetOrClear(DependencyProperty dp, object? value)
	{
		if (value == null) ClearValue(dp);
		else SetValue(dp, value);
	}
}
