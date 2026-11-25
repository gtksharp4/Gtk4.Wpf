using System.Windows.Media;

namespace System.Windows.Shapes;

// Partial class: stroke-style dependency properties.
// Core Shape DPs and rendering live in Shape.cs.
public abstract partial class Shape
{
	// ── StrokeDashArray ──────────────────────────────────────────────────────

	/// <summary>
	/// Pattern of dashes and gaps for the stroke.
	/// Values are multiples of <see cref="StrokeThickness"/> (WPF semantics).
	/// Null or empty means a solid stroke.
	/// </summary>
	public static readonly DependencyProperty StrokeDashArrayProperty =
		DependencyProperty.Register(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Shape),
			new PropertyMetadata(null, (d, _) => ((Shape)d).QueueRedraw()));

	/// <summary>Gets or sets the stroke dash array.</summary>
	public DoubleCollection? StrokeDashArray
	{
		get => (DoubleCollection?)GetValue(StrokeDashArrayProperty);
		set => SetValue(StrokeDashArrayProperty, value);
	}

	// ── StrokeDashOffset ─────────────────────────────────────────────────────

	/// <summary>
	/// Offset into the dash sequence where the pattern starts.
	/// Units are multiples of <see cref="StrokeThickness"/>.
	/// </summary>
	public static readonly DependencyProperty StrokeDashOffsetProperty =
		DependencyProperty.Register(nameof(StrokeDashOffset), typeof(double), typeof(Shape),
			new PropertyMetadata(0.0, (d, _) => ((Shape)d).QueueRedraw()));

	/// <summary>Gets or sets the dash offset.</summary>
	public double StrokeDashOffset
	{
		get => (double)GetValue(StrokeDashOffsetProperty)!;
		set => SetValue(StrokeDashOffsetProperty, value);
	}

	// ── StrokeDashCap ────────────────────────────────────────────────────────

	/// <summary>
	/// Cap applied to the ends of each dash segment.
	/// Cairo has no per-dash cap; this property is accepted for WPF API
	/// compatibility but is not applied to Cairo rendering.
	/// </summary>
	public static readonly DependencyProperty StrokeDashCapProperty =
		DependencyProperty.Register(nameof(StrokeDashCap), typeof(PenDashCap), typeof(Shape),
			new PropertyMetadata(PenDashCap.Flat, (d, _) => ((Shape)d).QueueRedraw()));

	/// <summary>Gets or sets the per-dash cap style.</summary>
	public PenDashCap StrokeDashCap
	{
		get => (PenDashCap)GetValue(StrokeDashCapProperty)!;
		set => SetValue(StrokeDashCapProperty, value);
	}

	// ── StrokeStartLineCap ───────────────────────────────────────────────────

	/// <summary>
	/// Cap at the start of an open stroke path.
	/// Mapped to Cairo's single <c>LineCap</c> (see <see cref="StrokeEndLineCap"/>
	/// for the limitation note).
	/// </summary>
	public static readonly DependencyProperty StrokeStartLineCapProperty =
		DependencyProperty.Register(nameof(StrokeStartLineCap), typeof(PenLineCap), typeof(Shape),
			new FrameworkPropertyMetadata(PenLineCap.Flat, MeasureAndRender, MeasureChanged));

	/// <summary>Gets or sets the start line cap.</summary>
	public PenLineCap StrokeStartLineCap
	{
		get => (PenLineCap)GetValue(StrokeStartLineCapProperty)!;
		set => SetValue(StrokeStartLineCapProperty, value);
	}

	// ── StrokeEndLineCap ─────────────────────────────────────────────────────

	/// <summary>
	/// Cap at the end of an open stroke path.
	/// <para>
	/// <b>Cairo limitation:</b> Cairo has a single <c>LineCap</c> applied to both
	/// path ends.  <c>StrokeStartLineCap</c> is used for Cairo's cap; this property
	/// is accepted for WPF API compatibility but its value does not independently
	/// affect Cairo rendering.
	/// </para>
	/// </summary>
	public static readonly DependencyProperty StrokeEndLineCapProperty =
		DependencyProperty.Register(nameof(StrokeEndLineCap), typeof(PenLineCap), typeof(Shape),
			new FrameworkPropertyMetadata(PenLineCap.Flat, MeasureAndRender, MeasureChanged));

	/// <summary>Gets or sets the end line cap.</summary>
	public PenLineCap StrokeEndLineCap
	{
		get => (PenLineCap)GetValue(StrokeEndLineCapProperty)!;
		set => SetValue(StrokeEndLineCapProperty, value);
	}

	// ── StrokeLineJoin ───────────────────────────────────────────────────────

	/// <summary>How path segments join at vertices.</summary>
	public static readonly DependencyProperty StrokeLineJoinProperty =
		DependencyProperty.Register(nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(Shape),
			new FrameworkPropertyMetadata(PenLineJoin.Miter, MeasureAndRender, MeasureChanged));

	/// <summary>Gets or sets the line join style.</summary>
	public PenLineJoin StrokeLineJoin
	{
		get => (PenLineJoin)GetValue(StrokeLineJoinProperty)!;
		set => SetValue(StrokeLineJoinProperty, value);
	}

	// ── StrokeMiterLimit ─────────────────────────────────────────────────────

	/// <summary>
	/// Maximum miter length as a multiple of <see cref="StrokeThickness"/>.
	/// When the miter would exceed this, Cairo falls back to a bevel join.
	/// WPF default is 10.
	/// </summary>
	public static readonly DependencyProperty StrokeMiterLimitProperty =
		DependencyProperty.Register(nameof(StrokeMiterLimit), typeof(double), typeof(Shape),
			new FrameworkPropertyMetadata(10.0, MeasureAndRender, MeasureChanged));

	/// <summary>Gets or sets the miter limit.</summary>
	public double StrokeMiterLimit
	{
		get => (double)GetValue(StrokeMiterLimitProperty)!;
		set => SetValue(StrokeMiterLimitProperty, value);
	}
}
