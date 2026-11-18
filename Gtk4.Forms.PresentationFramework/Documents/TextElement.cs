using System;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Base class for all flow-document elements. Carries optional typography properties
/// that affect Pango markup rendering when set on an inline.
/// </summary>
public abstract class TextElement
{
	/// <summary>Font family name, or <c>null</c> to inherit.</summary>
	public string? FontFamily { get; set; }

	/// <summary>Font weight (e.g. <c>FontWeights.Bold</c>), or <c>null</c> to inherit.</summary>
	public FontWeight? FontWeight { get; set; }

	/// <summary>Font style (e.g. <c>FontStyles.Italic</c>), or <c>null</c> to inherit.</summary>
	public FontStyle? FontStyle { get; set; }

	/// <summary>Foreground brush, or <c>null</c> to inherit.</summary>
	public Brush? Foreground { get; set; }

	/// <summary>Text decorations (underline, strikethrough, etc.), or <c>null</c> for none.</summary>
	public TextDecorationCollection? TextDecorations { get; set; }
}