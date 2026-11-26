using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// A cell of a <see cref="TableRow"/> holding blocks. Spans, border, padding, alignment and line height are
/// stored; see <see cref="Table"/> for what is drawn.
/// <para>
/// Gap: <c>Block</c> has no Padding, TextAlignment or LineHeight DPs yet, so <see cref="PaddingProperty"/> and
/// <see cref="TextAlignmentProperty"/> are registered here and <see cref="LineHeightProperty"/> is owned from
/// <c>FlowDocument</c>. WPF uses <c>Block.*Property.AddOwner</c> for all three; switch once Block's layout members
/// are DPs. <see cref="TextAlignmentProperty"/> inherits, as in WPF.
/// </para>
/// </summary>
[ContentProperty("Blocks")]
public class TableCell : TextElement
{
	/// <summary>Identifies the ColumnSpan property (default 1; at least 1).</summary>
	public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.Register(
		nameof(ColumnSpan), typeof(int), typeof(TableCell),
		new FrameworkPropertyMetadata(1), v => v is int i && i >= 1);

	/// <summary>Identifies the RowSpan property (default 1; at least 1).</summary>
	public static readonly DependencyProperty RowSpanProperty = DependencyProperty.Register(
		nameof(RowSpan), typeof(int), typeof(TableCell),
		new FrameworkPropertyMetadata(1), v => v is int i && i >= 1);

	/// <summary>Identifies the BorderBrush property (default null).</summary>
	public static readonly DependencyProperty BorderBrushProperty =
		Block.BorderBrushProperty.AddOwner(typeof(TableCell));

	/// <summary>Identifies the BorderThickness property (default 0).</summary>
	public static readonly DependencyProperty BorderThicknessProperty =
		Block.BorderThicknessProperty.AddOwner(typeof(TableCell));

	/// <summary>Identifies the Padding property (default 0).</summary>
	public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
		nameof(Padding), typeof(Thickness), typeof(TableCell), new FrameworkPropertyMetadata(new Thickness(0)));

	/// <summary>Identifies the TextAlignment property (default Left; inherits).</summary>
	public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
		nameof(TextAlignment), typeof(TextAlignment), typeof(TableCell),
		new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Identifies the LineHeight property (default NaN; otherwise 0.0034 to 160000).</summary>
	public static readonly DependencyProperty LineHeightProperty =
		FlowDocument.LineHeightProperty.AddOwner(typeof(TableCell));

	/// <summary>Creates an empty cell.</summary>
	public TableCell() => Blocks = new BlockCollection(this);

	/// <summary>Creates a cell holding <paramref name="blockItem"/>.</summary>
	public TableCell(Block blockItem) : this()
	{
		if (blockItem == null) throw new ArgumentNullException(nameof(blockItem));
		Blocks.Add(blockItem);
	}

	/// <summary>The cell content; the XAML content property.</summary>
	public BlockCollection Blocks { get; }

	/// <summary>Gets or sets how many columns the cell spans.</summary>
	public int ColumnSpan
	{
		get => (int)GetValue(ColumnSpanProperty)!;
		set => SetValue(ColumnSpanProperty, value);
	}

	/// <summary>Gets or sets how many rows the cell spans.</summary>
	public int RowSpan
	{
		get => (int)GetValue(RowSpanProperty)!;
		set => SetValue(RowSpanProperty, value);
	}

	/// <summary>Gets or sets the border brush.</summary>
	public Brush? BorderBrush
	{
		get => (Brush?)GetValue(BorderBrushProperty);
		set => SetValue(BorderBrushProperty, value);
	}

	/// <summary>Gets or sets the border thickness.</summary>
	public Thickness BorderThickness
	{
		get => (Thickness)GetValue(BorderThicknessProperty)!;
		set => SetValue(BorderThicknessProperty, value);
	}

	/// <summary>Gets or sets the padding.</summary>
	public Thickness Padding
	{
		get => (Thickness)GetValue(PaddingProperty)!;
		set => SetValue(PaddingProperty, value);
	}

	/// <summary>Gets or sets the text alignment.</summary>
	public TextAlignment TextAlignment
	{
		get => (TextAlignment)GetValue(TextAlignmentProperty)!;
		set => SetValue(TextAlignmentProperty, value);
	}

	/// <summary>Gets or sets the line height (NaN = from the font).</summary>
	public double LineHeight
	{
		get => (double)GetValue(LineHeightProperty)!;
		set => SetValue(LineHeightProperty, value);
	}

	/// <summary>Gets or sets the flow direction (default LeftToRight).</summary>
	public FlowDirection FlowDirection
	{
		get => GetFlowDirection();
		set => SetFlowDirection(value);
	}

	internal void AppendPlainText(System.Text.StringBuilder sb)
	{
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append(' ');
			first = false;
			block.AppendPlainText(sb);
		}
	}

	internal void AppendMarkup(System.Text.StringBuilder sb) => AppendWrappedMarkup(sb, s =>
	{
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) s.Append(' ');
			first = false;
			block.AppendWrappedMarkup(s);
		}
	});
}
