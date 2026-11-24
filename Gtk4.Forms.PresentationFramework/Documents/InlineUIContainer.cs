using System.Windows.Controls;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>An inline element that hosts a <see cref="UIElement"/>.</summary>
/// <remarks>
/// Inside a <see cref="TextBlock"/> (also through Span, Bold, Hyperlink, ...) the child is laid out inline: the
/// text carries a Pango object placeholder sized to the child, and the child's widget sits over it. In a
/// <see cref="FlowDocument"/> the container stays store-only (no child widget; the paginator is not wired to it).
/// </remarks>
[ContentProperty("Child")]
public partial class InlineUIContainer : Inline
{
	/// <summary>Creates an empty container.</summary>
	public InlineUIContainer()
	{
	}

	/// <summary>Creates a container hosting <paramref name="childUIElement"/>.</summary>
	public InlineUIContainer(UIElement childUIElement) => Child = childUIElement;

	/// <summary>The hosted element.</summary>
	public UIElement? Child
	{
		get => _child;
		set
		{
			if (ReferenceEquals(_child, value)) return;
			_child = value;
			ContentParenting.Changed(this);
		}
	}

	UIElement? _child;

	/// <inheritdoc/>
	internal override void AppendMarkup(System.Text.StringBuilder sb)
	{
		// U+FFFC is the placeholder the TextBlock gives a Pango shape; only a hosting TextBlock emits it.
		if (OwningTextBlock?.HostsInlineObjects == true) sb.Append('\uFFFC');
	}

	/// <summary>The TextBlock whose text contains this container (through any Span), or null.</summary>
	internal TextBlock? OwningTextBlock
	{
		get
		{
			DependencyObject? current = Parent;
			for (int steps = 0; current != null && steps < 1000; steps++)
			{
				if (current is TextBlock tb) return tb;
				current = (current as FrameworkContentElement)?.Parent;
			}
			return null;
		}
	}
}
