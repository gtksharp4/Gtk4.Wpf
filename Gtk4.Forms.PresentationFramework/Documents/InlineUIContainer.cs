using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>An inline element that hosts a <see cref="UIElement"/>.</summary>
/// <remarks>Store-only: rendering of the hosted UIElement inside text is not implemented.</remarks>
[ContentProperty("Child")]
public class InlineUIContainer : Inline
{
	/// <summary>Creates an empty container.</summary>
	public InlineUIContainer()
	{
	}

	/// <summary>Creates a container hosting <paramref name="childUIElement"/>.</summary>
	public InlineUIContainer(UIElement childUIElement) => Child = childUIElement;

	/// <summary>The hosted element.</summary>
	public UIElement? Child { get; set; }

	/// <inheritdoc/>
	internal override void AppendMarkup(System.Text.StringBuilder sb)
	{
		// A UIElement has no Pango markup representation; emit nothing.
	}
}
