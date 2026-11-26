using System;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Base class for all flow-document elements. The typography members have their WPF shape: each returns the
/// effective value (local, style, trigger or inherited, never null or NaN) and each setter calls
/// <c>SetValue</c>; <c>ClearValue</c> restores inheritance. Rendering and serialisation read only the values an
/// element sets itself (see <c>TextLocalValues</c>) so inherited values are not emitted.
/// </summary>
public abstract partial class TextElement : FrameworkContentElement
{
	/// <summary>Gets or sets the font family. Null is rejected by the property's validation, as in WPF.</summary>
	public FontFamily FontFamily
	{
		get => (FontFamily)GetValue(FontFamilyProperty)!;
		set => SetValue(FontFamilyProperty, value);
	}

	/// <summary>Gets or sets the font weight (e.g. <c>FontWeights.Bold</c>).</summary>
	public FontWeight FontWeight
	{
		get => (FontWeight)GetValue(FontWeightProperty)!;
		set => SetValue(FontWeightProperty, value);
	}

	/// <summary>Gets or sets the font style (e.g. <c>FontStyles.Italic</c>).</summary>
	public FontStyle FontStyle
	{
		get => (FontStyle)GetValue(FontStyleProperty)!;
		set => SetValue(FontStyleProperty, value);
	}

	/// <summary>Gets or sets the font stretch.</summary>
	public FontStretch FontStretch
	{
		get => (FontStretch)GetValue(FontStretchProperty)!;
		set => SetValue(FontStretchProperty, value);
	}

	/// <summary>Gets or sets the foreground brush.</summary>
	public Brush Foreground
	{
		get => (Brush)GetValue(ForegroundProperty)!;
		set => SetValue(ForegroundProperty, value);
	}

	internal bool HasLocalFormat => TextLocalValues.HasMarkupFormat(this);

	/// <summary>Text decorations (underline, strikethrough, etc.), or <c>null</c> for none.</summary>
	public TextDecorationCollection? TextDecorations { get; set; }
}
