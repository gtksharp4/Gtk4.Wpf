using System.Collections.ObjectModel;

namespace System.Windows;

/// <summary>Specifies which part of a text line a decoration is applied to.</summary>
public enum TextDecorationLocation
{
	/// <summary>Underline — rendered below the text baseline.</summary>
	Underline,
	/// <summary>Strikethrough — rendered through the middle of the text.</summary>
	Strikethrough,
	/// <summary>Overline — rendered above the text.
	/// <para>Pango support: requires <c>overline</c> attribute (Pango ≥ 1.46 / GTK ≥ 4.0).
	/// Emitted as <c>overline="single"</c>; falls back to no decoration on older runtimes.</para>
	/// </summary>
	OverLine,
	/// <summary>Baseline — rendered at the text baseline.
	/// <para>Pango has no direct baseline decoration; this location is emitted as
	/// <c>underline="single"</c> as the closest available approximation.</para>
	/// </summary>
	Baseline,
}

/// <summary>
/// Describes a single text decoration (underline, strikethrough, etc.).
/// </summary>
public class TextDecoration
{
	/// <summary>The part of the text line where the decoration appears.</summary>
	public TextDecorationLocation Location { get; set; }

	/// <summary>Initialises a decoration at the specified <paramref name="location"/>.</summary>
	public TextDecoration(TextDecorationLocation location) =>
		Location = location;

	/// <summary>Initialises a decoration at <see cref="TextDecorationLocation.Underline"/>.</summary>
	public TextDecoration() { }
}

/// <summary>An ordered, mutable collection of <see cref="TextDecoration"/> instances.</summary>
public class TextDecorationCollection : Collection<TextDecoration>
{
}
