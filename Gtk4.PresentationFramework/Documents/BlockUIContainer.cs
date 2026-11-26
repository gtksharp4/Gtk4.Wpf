namespace System.Windows.Documents;

/// <summary>
/// A block element that hosts an arbitrary <see cref="UIElement"/>.
/// In Pango markup rendering it emits nothing (the hosted element is not text);
/// in plain-text rendering it emits a placeholder.
/// </summary>
public class BlockUIContainer : Block
{
	/// <summary>The UI element hosted by this block.</summary>
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
	internal override void AppendPlainText(System.Text.StringBuilder sb)
	{
		// A UIElement has no text representation; emit nothing.
	}

	/// <inheritdoc/>
	internal override void AppendMarkup(System.Text.StringBuilder sb)
	{
		// A UIElement has no Pango markup representation; emit nothing.
	}
}