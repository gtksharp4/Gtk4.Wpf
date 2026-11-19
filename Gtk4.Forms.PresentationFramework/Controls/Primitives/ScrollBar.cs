namespace System.Windows.Controls.Primitives;

/// <summary>
/// A scroll bar range control, backed by <see cref="Gtk.Scrollbar"/> +
/// <see cref="Gtk.Adjustment"/>. Maps WPF's scroll-bar semantics (Value, Minimum,
/// Maximum, ViewportSize, SmallChange, LargeChange) to GTK's adjustment model.
/// <para>
/// Two-way sync: managed → adjustment on every DP change; adjustment → managed on
/// user interaction (raises <see cref="Scroll"/> and <see cref="ValueChanged"/>).
/// </para>
/// <para>
/// Headless safe: all DPs store and coerce values before the widget is realized;
/// the state is applied to the adjustment in <see cref="CreateWidget"/>.
/// </para>
/// </summary>
public partial class ScrollBar : Control
{
	// ── Scroll event ─────────────────────────────────────────────────────────

	/// <summary>Raised when the user scrolls or the thumb moves.</summary>
	public event EventHandler<ScrollEventArgs>? Scroll;

	/// <summary>Raised when <see cref="Value"/> changes for any reason.</summary>
	public event EventHandler? ValueChanged;

	// ── Orientation DP ───────────────────────────────────────────────────────

	/// <summary>
	/// Orientation of the scroll bar. WPF default: <see cref="Controls.Orientation.Vertical"/>.
	/// </summary>
	public static readonly DependencyProperty OrientationProperty =
		DependencyProperty.Register(
			nameof(Orientation),
			typeof(Orientation),
			typeof(ScrollBar),
			new PropertyMetadata(Orientation.Vertical));

	/// <summary>Gets or sets the scroll bar orientation.</summary>
	public Orientation Orientation
	{
		get => (Orientation?)GetValue(OrientationProperty) ?? Orientation.Vertical;
		set => SetValue(OrientationProperty, value);
	}

	// ── Minimum DP ───────────────────────────────────────────────────────────

	/// <summary>Minimum value of the scroll range.</summary>
	public static readonly DependencyProperty MinimumProperty =
		DependencyProperty.Register(
			nameof(Minimum),
			typeof(double),
			typeof(ScrollBar),
			new PropertyMetadata(0.0, OnRangeChanged));

	/// <summary>Gets or sets the minimum value.</summary>
	public double Minimum
	{
		get => (double?)GetValue(MinimumProperty) ?? 0.0;
		set => SetValue(MinimumProperty, value);
	}

	// ── Maximum DP ───────────────────────────────────────────────────────────

	/// <summary>Maximum value of the scroll range.</summary>
	public static readonly DependencyProperty MaximumProperty =
		DependencyProperty.Register(
			nameof(Maximum),
			typeof(double),
			typeof(ScrollBar),
			new PropertyMetadata(1.0, OnRangeChanged));

	/// <summary>Gets or sets the maximum value.</summary>
	public double Maximum
	{
		get => (double?)GetValue(MaximumProperty) ?? 1.0;
		set => SetValue(MaximumProperty, value);
	}

	// ── Value DP ─────────────────────────────────────────────────────────────

	/// <summary>Current value. Coerced into [<see cref="Minimum"/>, <see cref="Maximum"/>].</summary>
	public static readonly DependencyProperty ValueProperty =
		DependencyProperty.Register(
			nameof(Value),
			typeof(double),
			typeof(ScrollBar),
			new FrameworkPropertyMetadata(
				0.0,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
				OnValueChanged,
				(d, v) =>
				{
					var sb = (ScrollBar)d;
					var dv = (double?)v ?? 0.0;
					return (object?)Math.Max(sb.Minimum, Math.Min(sb.Maximum, dv));
				}));

	static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var sb = (ScrollBar)d;
		sb.ApplyValueToAdjustment();
		sb.ValueChanged?.Invoke(sb, EventArgs.Empty);
	}

	/// <summary>Gets or sets the current scroll position. Coerced to [Minimum, Maximum].</summary>
	public double Value
	{
		get => (double?)GetValue(ValueProperty) ?? 0.0;
		set => SetValue(ValueProperty, value);
	}

	// ── ViewportSize DP ──────────────────────────────────────────────────────

	/// <summary>
	/// Size of the viewport relative to the scrollable extent.
	/// Maps to <see cref="Gtk.Adjustment.PageSize"/>.
	/// </summary>
	public static readonly DependencyProperty ViewportSizeProperty =
		DependencyProperty.Register(
			nameof(ViewportSize),
			typeof(double),
			typeof(ScrollBar),
			new PropertyMetadata(0.0, OnAdjustmentParamChanged));

	/// <summary>Gets or sets the viewport size.</summary>
	public double ViewportSize
	{
		get => (double?)GetValue(ViewportSizeProperty) ?? 0.0;
		set => SetValue(ViewportSizeProperty, value);
	}

	// ── SmallChange DP ───────────────────────────────────────────────────────

	/// <summary>
	/// Amount scrolled per line/arrow-key step.
	/// Maps to <see cref="Gtk.Adjustment.StepIncrement"/>.
	/// </summary>
	public static readonly DependencyProperty SmallChangeProperty =
		DependencyProperty.Register(
			nameof(SmallChange),
			typeof(double),
			typeof(ScrollBar),
			new PropertyMetadata(0.1, OnAdjustmentParamChanged));

	/// <summary>Gets or sets the small-step increment.</summary>
	public double SmallChange
	{
		get => (double?)GetValue(SmallChangeProperty) ?? 0.1;
		set => SetValue(SmallChangeProperty, value);
	}

	// ── LargeChange DP ───────────────────────────────────────────────────────

	/// <summary>
	/// Amount scrolled per page-up/down.
	/// Maps to <see cref="Gtk.Adjustment.PageIncrement"/>.
	/// </summary>
	public static readonly DependencyProperty LargeChangeProperty =
		DependencyProperty.Register(
			nameof(LargeChange),
			typeof(double),
			typeof(ScrollBar),
			new PropertyMetadata(1.0, OnAdjustmentParamChanged));

	/// <summary>Gets or sets the large-step increment.</summary>
	public double LargeChange
	{
		get => (double?)GetValue(LargeChangeProperty) ?? 1.0;
		set => SetValue(LargeChangeProperty, value);
	}

	// ── DP change callbacks ───────────────────────────────────────────────────

	static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var sb = (ScrollBar)d;
		// Re-coerce Value into the new range.
		var clamped = Math.Max(sb.Minimum, Math.Min(sb.Maximum, sb.Value));
		if (clamped != sb.Value)
			sb.Value = clamped;
		sb.ApplyAdjustmentBounds();
	}

	static void OnAdjustmentParamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ScrollBar)d).ApplyAdjustmentBounds();
	}
}
