using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// A flow-content document containing a sequence of <see cref="Block"/> elements.
/// Supports rendering to Pango markup and plain text via <see cref="BuildMarkup"/>
/// and <see cref="BuildPlainText"/>; both are pure and testable without GTK.
/// </summary>
[ContentProperty("Blocks")]
public partial class FlowDocument
{
	/// <summary>The blocks in this document.</summary>
	public BlockCollection Blocks { get; } = new BlockCollection();

	/// <summary>Default font family for the document, or <c>null</c> to inherit.</summary>
	public string? FontFamily { get; set; }

	/// <summary>Default font size in device-independent pixels, or <c>double.NaN</c> to inherit.</summary>
	public double FontSize { get; set; } = double.NaN;

	/// <summary>Default foreground brush, or <c>null</c> to inherit.</summary>
	public Brush? Foreground { get; set; }

	/// <summary>Default text alignment, or <c>null</c> to inherit.</summary>
	public TextAlignment? TextAlignment { get; set; }

	/// <summary>Padding around the page content.</summary>
	public Thickness PagePadding { get; set; }

	/// <summary>
	/// Builds a Pango markup string for the entire document.
	/// Paragraphs are separated by a blank line (<c>\n\n</c>); other block types
	/// use their own <see cref="Block.AppendMarkup"/> rendering.
	/// </summary>
	public string BuildMarkup()
	{
		var sb = new System.Text.StringBuilder();
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append("\n\n");
			first = false;
			block.AppendBlockMarkup(sb, _flowDirection);
		}
		return sb.ToString();
	}

	FlowDirection? _flowDirection;

	/// <summary>Gets or sets the flow direction of the document (default LeftToRight). Rendered as a bidi
	/// isolate inside each paragraph of <see cref="BuildMarkup"/>; not inherited, not a dependency property here.</summary>
	public FlowDirection FlowDirection
	{
		get => _flowDirection ?? FlowDirection.LeftToRight;
		set
		{
			if (!FrameworkElement.IsValidFlowDirection(value))
				throw new ArgumentException($"'{value}' is not a valid value for property 'FlowDirection'.");
			_flowDirection = value;
		}
	}

	/// <summary>
	/// Builds a plain-text representation of the entire document.
	/// Paragraphs are separated by blank lines; lists use bullet/number markers.
	/// </summary>
	public string BuildPlainText()
	{
		var sb = new System.Text.StringBuilder();
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append("\n\n");
			first = false;
			block.AppendPlainText(sb);
		}
		return sb.ToString();
	}

	/// <summary>
	/// Returns all text content of the document as a single plain string
	/// with paragraphs joined by newlines (no blank-line separators).
	/// Used by <see cref="Controls.RichTextBox"/> for the buffer.
	/// </summary>
	internal string GetAllText()
	{
		var sb = new System.Text.StringBuilder();
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append('\n');
			first = false;
			block.AppendPlainText(sb);
		}
		return sb.ToString();
	}
}