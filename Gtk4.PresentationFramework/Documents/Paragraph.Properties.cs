namespace System.Windows.Documents;

/// <summary>
/// Paragraph DPs with WPF defaults, all stored only. Pango markup has no first-line indent and a label has no
/// pagination, so <see cref="TextIndent"/> and the keep/orphan/widow settings do not reach the text.
/// </summary>
public partial class Paragraph
{
	/// <summary>Identifies the TextIndent property (default 0).</summary>
	public static readonly DependencyProperty TextIndentProperty = DependencyProperty.Register(
		nameof(TextIndent), typeof(double), typeof(Paragraph), new FrameworkPropertyMetadata(0.0),
		v => v is double d && !double.IsNaN(d) && !double.IsInfinity(d));

	/// <summary>Identifies the KeepTogether property (default false).</summary>
	public static readonly DependencyProperty KeepTogetherProperty = DependencyProperty.Register(
		nameof(KeepTogether), typeof(bool), typeof(Paragraph), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the KeepWithNext property (default false).</summary>
	public static readonly DependencyProperty KeepWithNextProperty = DependencyProperty.Register(
		nameof(KeepWithNext), typeof(bool), typeof(Paragraph), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the MinOrphanLines property (default 0; not negative).</summary>
	public static readonly DependencyProperty MinOrphanLinesProperty = DependencyProperty.Register(
		nameof(MinOrphanLines), typeof(int), typeof(Paragraph), new FrameworkPropertyMetadata(0),
		v => v is int i && i >= 0);

	/// <summary>Identifies the MinWidowLines property (default 0; not negative).</summary>
	public static readonly DependencyProperty MinWidowLinesProperty = DependencyProperty.Register(
		nameof(MinWidowLines), typeof(int), typeof(Paragraph), new FrameworkPropertyMetadata(0),
		v => v is int i && i >= 0);

	/// <summary>Gets or sets the first-line indent in DIPs.</summary>
	public double TextIndent
	{
		get => (double)GetValue(TextIndentProperty)!;
		set => SetValue(TextIndentProperty, value);
	}

	/// <summary>Gets or sets whether the paragraph may not be split across pages.</summary>
	public bool KeepTogether
	{
		get => (bool)GetValue(KeepTogetherProperty)!;
		set => SetValue(KeepTogetherProperty, value);
	}

	/// <summary>Gets or sets whether the paragraph stays on the page of the next one.</summary>
	public bool KeepWithNext
	{
		get => (bool)GetValue(KeepWithNextProperty)!;
		set => SetValue(KeepWithNextProperty, value);
	}

	/// <summary>Gets or sets the minimum lines left at the bottom of a page.</summary>
	public int MinOrphanLines
	{
		get => (int)GetValue(MinOrphanLinesProperty)!;
		set => SetValue(MinOrphanLinesProperty, value);
	}

	/// <summary>Gets or sets the minimum lines carried to the top of the next page.</summary>
	public int MinWidowLines
	{
		get => (int)GetValue(MinWidowLinesProperty)!;
		set => SetValue(MinWidowLinesProperty, value);
	}
}
