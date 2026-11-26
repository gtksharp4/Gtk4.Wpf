using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Column, page and hyphenation DPs with WPF defaults and WPF validation (an invalid value throws
/// ArgumentException). They are stored only: FlowDocumentScrollViewer, like WPF's, shows the document as one
/// scrolling column, so they have nothing to lay out.
/// </summary>
public partial class FlowDocument
{
	static readonly FrameworkPropertyMetadataOptions Measure = FrameworkPropertyMetadataOptions.AffectsMeasure;

	/// <summary>Identifies the ColumnWidth property (default NaN).</summary>
	public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register(
		nameof(ColumnWidth), typeof(double), typeof(FlowDocument), new FrameworkPropertyMetadata(double.NaN, Measure),
		v => IsAutoOrFiniteNonNegative(v));

	/// <summary>Identifies the ColumnGap property (default NaN).</summary>
	public static readonly DependencyProperty ColumnGapProperty = DependencyProperty.Register(
		nameof(ColumnGap), typeof(double), typeof(FlowDocument), new FrameworkPropertyMetadata(double.NaN, Measure),
		v => IsAutoOrFiniteNonNegative(v));

	/// <summary>Identifies the ColumnRuleWidth property (default 0).</summary>
	public static readonly DependencyProperty ColumnRuleWidthProperty = DependencyProperty.Register(
		nameof(ColumnRuleWidth), typeof(double), typeof(FlowDocument), new FrameworkPropertyMetadata(0.0, Measure),
		v => v is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d >= 0);

	/// <summary>Identifies the ColumnRuleBrush property (default null).</summary>
	public static readonly DependencyProperty ColumnRuleBrushProperty = DependencyProperty.Register(
		nameof(ColumnRuleBrush), typeof(Brush), typeof(FlowDocument), new FrameworkPropertyMetadata(null));

	/// <summary>Identifies the IsColumnWidthFlexible property (default true).</summary>
	public static readonly DependencyProperty IsColumnWidthFlexibleProperty = DependencyProperty.Register(
		nameof(IsColumnWidthFlexible), typeof(bool), typeof(FlowDocument),
		new FrameworkPropertyMetadata(true, Measure));

	/// <summary>Identifies the PageWidth property (default NaN).</summary>
	public static readonly DependencyProperty PageWidthProperty = DependencyProperty.Register(
		nameof(PageWidth), typeof(double), typeof(FlowDocument), new FrameworkPropertyMetadata(double.NaN, Measure),
		v => IsAutoOrFiniteNonNegative(v));

	/// <summary>Identifies the PageHeight property (default NaN).</summary>
	public static readonly DependencyProperty PageHeightProperty = DependencyProperty.Register(
		nameof(PageHeight), typeof(double), typeof(FlowDocument), new FrameworkPropertyMetadata(double.NaN, Measure),
		v => IsAutoOrFiniteNonNegative(v));

	/// <summary>Identifies the MinPageWidth property (default 0).</summary>
	public static readonly DependencyProperty MinPageWidthProperty = DependencyProperty.Register(
		nameof(MinPageWidth), typeof(double), typeof(FlowDocument), new FrameworkPropertyMetadata(0.0, Measure),
		v => v is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d >= 0);

	/// <summary>Identifies the MinPageHeight property (default 0).</summary>
	public static readonly DependencyProperty MinPageHeightProperty = DependencyProperty.Register(
		nameof(MinPageHeight), typeof(double), typeof(FlowDocument), new FrameworkPropertyMetadata(0.0, Measure),
		v => v is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d >= 0);

	/// <summary>Identifies the MaxPageWidth property (default PositiveInfinity).</summary>
	public static readonly DependencyProperty MaxPageWidthProperty = DependencyProperty.Register(
		nameof(MaxPageWidth), typeof(double), typeof(FlowDocument),
		new FrameworkPropertyMetadata(double.PositiveInfinity, Measure),
		v => v is double d && !double.IsNaN(d) && d >= 0);

	/// <summary>Identifies the MaxPageHeight property (default PositiveInfinity).</summary>
	public static readonly DependencyProperty MaxPageHeightProperty = DependencyProperty.Register(
		nameof(MaxPageHeight), typeof(double), typeof(FlowDocument),
		new FrameworkPropertyMetadata(double.PositiveInfinity, Measure),
		v => v is double d && !double.IsNaN(d) && d >= 0);

	/// <summary>Identifies the IsHyphenationEnabled property (shared with Block; default false).</summary>
	public static readonly DependencyProperty IsHyphenationEnabledProperty =
		Block.IsHyphenationEnabledProperty.AddOwner(typeof(FlowDocument));

	/// <summary>Identifies the IsOptimalParagraphEnabled property (default false).</summary>
	public static readonly DependencyProperty IsOptimalParagraphEnabledProperty = DependencyProperty.Register(
		nameof(IsOptimalParagraphEnabled), typeof(bool), typeof(FlowDocument),
		new FrameworkPropertyMetadata(false, Measure));

	static bool IsAutoOrFiniteNonNegative(object? v) =>
		v is double d && (double.IsNaN(d) || (!double.IsInfinity(d) && d >= 0));

	/// <summary>Gets or sets the column width (NaN = automatic).</summary>
	public double ColumnWidth
	{
		get => (double)GetValue(ColumnWidthProperty)!;
		set => SetValue(ColumnWidthProperty, value);
	}

	/// <summary>Gets or sets the gap between columns (NaN = automatic).</summary>
	public double ColumnGap
	{
		get => (double)GetValue(ColumnGapProperty)!;
		set => SetValue(ColumnGapProperty, value);
	}

	/// <summary>Gets or sets the width of the rule between columns.</summary>
	public double ColumnRuleWidth
	{
		get => (double)GetValue(ColumnRuleWidthProperty)!;
		set => SetValue(ColumnRuleWidthProperty, value);
	}

	/// <summary>Gets or sets the brush of the rule between columns.</summary>
	public Brush? ColumnRuleBrush
	{
		get => (Brush?)GetValue(ColumnRuleBrushProperty);
		set => SetValue(ColumnRuleBrushProperty, value);
	}

	/// <summary>Gets or sets whether columns may grow past <see cref="ColumnWidth"/>.</summary>
	public bool IsColumnWidthFlexible
	{
		get => (bool)GetValue(IsColumnWidthFlexibleProperty)!;
		set => SetValue(IsColumnWidthFlexibleProperty, value);
	}

	/// <summary>Gets or sets the page width (NaN = automatic).</summary>
	public double PageWidth
	{
		get => (double)GetValue(PageWidthProperty)!;
		set => SetValue(PageWidthProperty, value);
	}

	/// <summary>Gets or sets the page height (NaN = automatic).</summary>
	public double PageHeight
	{
		get => (double)GetValue(PageHeightProperty)!;
		set => SetValue(PageHeightProperty, value);
	}

	/// <summary>Gets or sets the minimum page width.</summary>
	public double MinPageWidth
	{
		get => (double)GetValue(MinPageWidthProperty)!;
		set => SetValue(MinPageWidthProperty, value);
	}

	/// <summary>Gets or sets the minimum page height.</summary>
	public double MinPageHeight
	{
		get => (double)GetValue(MinPageHeightProperty)!;
		set => SetValue(MinPageHeightProperty, value);
	}

	/// <summary>Gets or sets the maximum page width.</summary>
	public double MaxPageWidth
	{
		get => (double)GetValue(MaxPageWidthProperty)!;
		set => SetValue(MaxPageWidthProperty, value);
	}

	/// <summary>Gets or sets the maximum page height.</summary>
	public double MaxPageHeight
	{
		get => (double)GetValue(MaxPageHeightProperty)!;
		set => SetValue(MaxPageHeightProperty, value);
	}

	/// <summary>Gets or sets whether automatic hyphenation is enabled.</summary>
	public bool IsHyphenationEnabled
	{
		get => Block.GetIsHyphenationEnabled(this);
		set => Block.SetIsHyphenationEnabled(this, value);
	}

	/// <summary>Gets or sets whether the optimal (total-fit) paragraph algorithm is used.</summary>
	public bool IsOptimalParagraphEnabled
	{
		get => (bool)GetValue(IsOptimalParagraphEnabledProperty)!;
		set => SetValue(IsOptimalParagraphEnabledProperty, value);
	}
}
