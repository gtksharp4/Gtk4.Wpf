using System.Runtime.CompilerServices;

namespace System.Windows.Documents;

/// <summary>
/// Per-element font size set through <see cref="TextRange.ApplyPropertyValue"/>. <see cref="TextElement"/> has no
/// instance FontSize here, so the value is stored on the side: readable back, not yet rendered.
/// </summary>
static class TextRangeFontSize
{
	static readonly ConditionalWeakTable<TextElement, object> Sizes = new ConditionalWeakTable<TextElement, object>();

	internal static double? Get(TextElement element) => Sizes.TryGetValue(element, out var v) ? (double)v : (double?)null;

	internal static void Set(TextElement element, double size)
	{
		Sizes.Remove(element);
		Sizes.Add(element, size);
	}

	internal static void Copy(TextElement from, TextElement to)
	{
		var size = Get(from);
		if (size != null) Set(to, size.Value);
	}
}
