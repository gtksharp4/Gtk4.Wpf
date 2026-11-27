using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Base class for an inline that anchors block content (<see cref="Floater"/>). Mirrors
/// <c>System.Windows.Documents.AnchoredBlock</c>.
/// </summary>
/// <remarks>
/// Gap: the document is shown in a single Pango label, so the blocks are written into the flow where the element
/// sits instead of floating beside the text, and they are not part of the <see cref="TextPointer"/> tree.
/// </remarks>
[ContentProperty("Blocks")]
public abstract class AnchoredBlock : Inline
{
	/// <summary>Identifies the Margin property (default auto: NaN on every side).</summary>
	public static readonly DependencyProperty MarginProperty = DependencyProperty.Register(
		nameof(Margin), typeof(Thickness), typeof(AnchoredBlock),
		new FrameworkPropertyMetadata(new Thickness(double.NaN)));

	/// <summary>Identifies the Padding property (default NaN on every side).</summary>
	public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
		nameof(Padding), typeof(Thickness), typeof(AnchoredBlock),
		new FrameworkPropertyMetadata(new Thickness(double.NaN)));

	/// <summary>Identifies the BorderThickness property (default 0).</summary>
	public static readonly DependencyProperty BorderThicknessProperty =
		Block.BorderThicknessProperty.AddOwner(typeof(AnchoredBlock));

	/// <summary>Identifies the BorderBrush property (default null).</summary>
	public static readonly DependencyProperty BorderBrushProperty =
		Block.BorderBrushProperty.AddOwner(typeof(AnchoredBlock));

	/// <summary>Identifies the TextAlignment property (default Left; inherits).</summary>
	public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
		nameof(TextAlignment), typeof(TextAlignment), typeof(AnchoredBlock),
		new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Identifies the LineHeight property (default NaN).</summary>
	public static readonly DependencyProperty LineHeightProperty =
		FlowDocument.LineHeightProperty.AddOwner(typeof(AnchoredBlock));

	/// <summary>Identifies the LineStackingStrategy property (default MaxHeight).</summary>
	public static readonly DependencyProperty LineStackingStrategyProperty =
		FlowDocument.LineStackingStrategyProperty.AddOwner(typeof(AnchoredBlock));

	/// <summary>Anchors <paramref name="block"/> (when not null) at <paramref name="insertionPosition"/> (when not null).</summary>
	protected AnchoredBlock(Block? block, TextPointer? insertionPosition)
	{
		Blocks = new BlockCollection(this);
		if (insertionPosition != null)
			TextTreeEdit.InsertInline(insertionPosition.Document, insertionPosition.Offset, this);
		if (block != null) Blocks.Add(block);
	}

	/// <summary>The block content; the XAML content property.</summary>
	public BlockCollection Blocks { get; }

	/// <summary>Gets or sets the outer margin.</summary>
	public Thickness Margin
	{
		get => (Thickness)GetValue(MarginProperty)!;
		set => SetValue(MarginProperty, value);
	}

	/// <summary>Gets or sets the inner padding.</summary>
	public Thickness Padding
	{
		get => (Thickness)GetValue(PaddingProperty)!;
		set => SetValue(PaddingProperty, value);
	}

	/// <summary>Gets or sets the border thickness.</summary>
	public Thickness BorderThickness
	{
		get => (Thickness)GetValue(BorderThicknessProperty)!;
		set => SetValue(BorderThicknessProperty, value);
	}

	/// <summary>Gets or sets the border brush.</summary>
	public Brush? BorderBrush
	{
		get => (Brush?)GetValue(BorderBrushProperty);
		set => SetValue(BorderBrushProperty, value);
	}

	/// <summary>Gets or sets the text alignment.</summary>
	public TextAlignment TextAlignment
	{
		get => (TextAlignment)GetValue(TextAlignmentProperty)!;
		set => SetValue(TextAlignmentProperty, value);
	}

	/// <summary>Gets or sets the line height.</summary>
	public double LineHeight
	{
		get => (double)GetValue(LineHeightProperty)!;
		set => SetValue(LineHeightProperty, value);
	}

	/// <summary>Gets or sets how lines are stacked.</summary>
	public LineStackingStrategy LineStackingStrategy
	{
		get => (LineStackingStrategy)GetValue(LineStackingStrategyProperty)!;
		set => SetValue(LineStackingStrategyProperty, value);
	}

	/// <inheritdoc/>
	internal override void AppendMarkup(System.Text.StringBuilder sb)
	{
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append('\n');
			first = false;
			block.AppendBlockMarkup(sb, null);
		}
	}
}
