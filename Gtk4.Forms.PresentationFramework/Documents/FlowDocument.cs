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
	public BlockCollection Blocks { get; }

	/// <summary>Default text alignment, or <c>null</c> to inherit.</summary>
	public TextAlignment? TextAlignment
	{
		get => _textAlignment;
		set
		{
			_textAlignment = value;
			ContentChanged?.Invoke();
		}
	}

	TextAlignment? _textAlignment;

	/// <summary>Padding around the page content.</summary>
	public Thickness PagePadding
	{
		get => _pagePadding;
		set
		{
			_pagePadding = value;
			ContentChanged?.Invoke();
		}
	}

	// WPF default: every side auto (NaN), resolved to one line height when the page is laid out.
	Thickness _pagePadding = new Thickness(double.NaN);

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
			block.AppendBlockMarkup(sb, ExplicitFlowDirection);
		}
		return sb.ToString();
	}

	/// <summary>Identifies <see cref="FlowDirection"/> (shared with <see cref="FrameworkElement"/>).</summary>
	public static readonly DependencyProperty FlowDirectionProperty =
		FrameworkElement.FlowDirectionProperty.AddOwner(typeof(FlowDocument));

	// An inherited right-to-left direction applies too; the left-to-right default stays out of the markup.
	FlowDirection? ExplicitFlowDirection =>
		HasLocal(FlowDirectionProperty) || FlowDirection == FlowDirection.RightToLeft
			? FlowDirection : (FlowDirection?)null;

	/// <summary>
	/// Gets or sets the flow direction of the document (default LeftToRight). Rendered as a bidi isolate inside each
	/// paragraph of <see cref="BuildMarkup"/> when set locally or inherited as RightToLeft.
	/// </summary>
	public FlowDirection FlowDirection
	{
		get => (FlowDirection)GetValue(FlowDirectionProperty)!;
		set
		{
			if (!FrameworkElement.IsValidFlowDirection(value))
				throw new ArgumentException($"'{value}' is not a valid value for property 'FlowDirection'.");
			SetValue(FlowDirectionProperty, value);
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