using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

public partial class Slider
{
	// ── SmallChange DP ───────────────────────────────────────────────────────

	/// <summary>
	/// Amount the value changes per arrow-key press.
	/// Maps to the step increment of the underlying <see cref="Gtk.Scale"/>.
	/// WPF default: 1.
	/// </summary>
	public static readonly DependencyProperty SmallChangeProperty =
		DependencyProperty.Register(
			nameof(SmallChange),
			typeof(double),
			typeof(Slider),
			new PropertyMetadata(1.0, OnStepChanged));

	/// <summary>Gets or sets the small-step increment.</summary>
	public double SmallChange
	{
		get => (double?)GetValue(SmallChangeProperty) ?? 1.0;
		set => SetValue(SmallChangeProperty, value);
	}

	// ── LargeChange DP ───────────────────────────────────────────────────────

	/// <summary>
	/// Amount the value changes per page-up/page-down.
	/// Maps to the page increment of the underlying <see cref="Gtk.Scale"/>.
	/// WPF default: 10.
	/// </summary>
	public static readonly DependencyProperty LargeChangeProperty =
		DependencyProperty.Register(
			nameof(LargeChange),
			typeof(double),
			typeof(Slider),
			new PropertyMetadata(10.0, OnStepChanged));

	/// <summary>Gets or sets the large-step increment.</summary>
	public double LargeChange
	{
		get => (double?)GetValue(LargeChangeProperty) ?? 10.0;
		set => SetValue(LargeChangeProperty, value);
	}

	// ── TickFrequency DP ─────────────────────────────────────────────────────

	/// <summary>
	/// Interval between tick marks. When <see cref="TickPlacement"/> is not
	/// <see cref="Primitives.TickPlacement.None"/>, marks are added at every multiple
	/// of this value from <see cref="Minimum"/> to <see cref="Maximum"/>.
	/// WPF default: 1.
	/// </summary>
	public static readonly DependencyProperty TickFrequencyProperty =
		DependencyProperty.Register(
			nameof(TickFrequency),
			typeof(double),
			typeof(Slider),
			new PropertyMetadata(1.0, OnTickChanged));

	/// <summary>Gets or sets the tick interval.</summary>
	public double TickFrequency
	{
		get => (double?)GetValue(TickFrequencyProperty) ?? 1.0;
		set => SetValue(TickFrequencyProperty, value);
	}

	// ── TickPlacement DP ─────────────────────────────────────────────────────

	/// <summary>
	/// Where tick marks are drawn relative to the slider track.
	/// Maps to <see cref="Gtk.Scale.AddMark"/> calls on the underlying widget.
	/// WPF default: <see cref="Primitives.TickPlacement.None"/>.
	/// </summary>
	public static readonly DependencyProperty TickPlacementProperty =
		DependencyProperty.Register(
			nameof(TickPlacement),
			typeof(TickPlacement),
			typeof(Slider),
			new PropertyMetadata(TickPlacement.None, OnTickChanged));

	/// <summary>Gets or sets where tick marks appear.</summary>
	public TickPlacement TickPlacement
	{
		get => (TickPlacement?)GetValue(TickPlacementProperty) ?? TickPlacement.None;
		set => SetValue(TickPlacementProperty, value);
	}

	// ── IsSnapToTickEnabled DP ───────────────────────────────────────────────

	/// <summary>
	/// When <c>true</c>, <see cref="Value"/> is rounded to the nearest multiple of
	/// <see cref="TickFrequency"/> on every change. Snap uses round-half-away-from-zero.
	/// WPF default: <c>false</c>.
	/// </summary>
	public static readonly DependencyProperty IsSnapToTickEnabledProperty =
		DependencyProperty.Register(
			nameof(IsSnapToTickEnabled),
			typeof(bool),
			typeof(Slider),
			new PropertyMetadata(false));

	/// <summary>Gets or sets whether the value snaps to tick marks.</summary>
	public bool IsSnapToTickEnabled
	{
		get => (bool?)GetValue(IsSnapToTickEnabledProperty) ?? false;
		set => SetValue(IsSnapToTickEnabledProperty, value);
	}

	// ── IsDirectionReversed DP ───────────────────────────────────────────────

	/// <summary>
	/// When <c>true</c>, the direction of increasing value is reversed (right-to-left for
	/// horizontal, bottom-to-top for vertical). Maps to <see cref="Gtk.Range.Inverted"/>.
	/// WPF default: <c>false</c>.
	/// </summary>
	public static readonly DependencyProperty IsDirectionReversedProperty =
		DependencyProperty.Register(
			nameof(IsDirectionReversed),
			typeof(bool),
			typeof(Slider),
			new PropertyMetadata(false, OnDirectionReversedChanged));

	/// <summary>Gets or sets whether the value direction is reversed.</summary>
	public bool IsDirectionReversed
	{
		get => (bool?)GetValue(IsDirectionReversedProperty) ?? false;
		set => SetValue(IsDirectionReversedProperty, value);
	}

	// ── DP change callbacks ───────────────────────────────────────────────────

	static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var s = (Slider)d;
		if (s._scale == null) return;
		s._scale.Adjustment.StepIncrement = s.SmallChange;
		s._scale.Adjustment.PageIncrement = s.LargeChange;
	}

	static void OnTickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Slider)d).ApplyTickProps();
	}

	static void OnDirectionReversedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var s = (Slider)d;
		if (s._scale != null)
			s._scale.Inverted = s.IsDirectionReversed;
	}

	// ── GTK tick application ─────────────────────────────────────────────────

	/// <summary>
	/// Clears existing marks and adds new ones according to <see cref="TickPlacement"/>
	/// and <see cref="TickFrequency"/>. No-ops when <see cref="_scale"/> is null (pre-realize).
	/// </summary>
	/// <remarks>
	/// GTK limit: <see cref="Gtk.Scale.AddMark"/> places marks at absolute values;
	/// there is no GTK API to add marks uniformly — this method calls AddMark once per tick.
	/// For large ranges with tiny <see cref="TickFrequency"/> this can be slow; callers
	/// should keep TickFrequency at a reasonable size.
	/// </remarks>
	void ApplyTickProps()
	{
		if (_scale == null) return;

		// Update step/page increments.
		_scale.Adjustment.StepIncrement = SmallChange;
		_scale.Adjustment.PageIncrement = LargeChange;

		// Always clear existing marks first.
		_scale.ClearMarks();

		if (TickPlacement == TickPlacement.None || TickFrequency <= 0.0)
			return;

		var gtkPos = ToGtkPositionType(TickPlacement, Orientation);
		foreach (var tick in SliderSnap.TickPositions(Minimum, Maximum, TickFrequency))
			_scale.AddMark(tick, gtkPos, null);

		// When Both, add a second pass for the opposite side.
		if (TickPlacement == TickPlacement.Both)
		{
			var opp = OppositePosition(gtkPos);
			foreach (var tick in SliderSnap.TickPositions(Minimum, Maximum, TickFrequency))
				_scale.AddMark(tick, opp, null);
		}
	}

	/// <summary>Maps <see cref="TickPlacement"/> + <see cref="Orientation"/> to a GTK position type.</summary>
	static Gtk.PositionType ToGtkPositionType(TickPlacement tp, Orientation orientation)
	{
		if (orientation == Orientation.Vertical)
		{
			// TopLeft → left side; BottomRight → right side; Both → start with left.
			return (tp == TickPlacement.BottomRight)
				? Gtk.PositionType.Right
				: Gtk.PositionType.Left;
		}

		// Horizontal: TopLeft → above track; BottomRight / Both → below track.
		return (tp == TickPlacement.TopLeft)
			? Gtk.PositionType.Top
			: Gtk.PositionType.Bottom;
	}

	static Gtk.PositionType OppositePosition(Gtk.PositionType pos)
	{
		switch (pos)
		{
			case Gtk.PositionType.Top: return Gtk.PositionType.Bottom;
			case Gtk.PositionType.Bottom: return Gtk.PositionType.Top;
			case Gtk.PositionType.Left: return Gtk.PositionType.Right;
			default: return Gtk.PositionType.Left;
		}
	}
}
