using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// A range control for selecting a value within a range, backed by
/// <see cref="Gtk.Scale"/> (a GTK4-native horizontal/vertical slider).
/// Two-way: GTK value change writes back into <see cref="RangeBase.Value"/>.
/// <para>
/// The range DPs come from <see cref="RangeBase"/>; <see cref="Slider"/> overrides the default
/// (Maximum 10; SmallChange 0.1 and LargeChange 1 stay at the RangeBase defaults). When
/// <see cref="IsSnapToTickEnabled"/> is <c>true</c>
/// the value is additionally rounded to the nearest tick defined by <see cref="TickFrequency"/>.
/// </para>
/// </summary>
public partial class Slider : RangeBase
{
	// ── Orientation DP ───────────────────────────────────────────────────────

	/// <summary>Orientation of the slider. WPF default: <see cref="Controls.Orientation.Horizontal"/>.</summary>
	public static readonly DependencyProperty OrientationProperty =
		DependencyProperty.Register(
			nameof(Orientation),
			typeof(Orientation),
			typeof(Slider),
			new PropertyMetadata(Orientation.Horizontal));

	/// <summary>Gets or sets the slider orientation.</summary>
	public Orientation Orientation
	{
		get => (Orientation?)GetValue(OrientationProperty) ?? Orientation.Horizontal;
		set => SetValue(OrientationProperty, value);
	}

	static void OverrideRangeMetadata()
	{
		MaximumProperty.OverrideMetadata(typeof(Slider), new FrameworkPropertyMetadata(10.0));
		SmallChangeProperty.OverrideMetadata(typeof(Slider), new FrameworkPropertyMetadata(OnStepChanged));
		LargeChangeProperty.OverrideMetadata(typeof(Slider), new FrameworkPropertyMetadata(OnStepChanged));
		ValueProperty.OverrideMetadata(typeof(Slider), new FrameworkPropertyMetadata(null, CoerceSnappedValue));
	}

	// Clamp into the range, then snap to the nearest tick when snapping is on.
	static object? CoerceSnappedValue(DependencyObject d, object? baseValue)
	{
		var s = (Slider)d;
		var clamped = (double)CoerceToRange(d, baseValue)!;
		if (s.IsSnapToTickEnabled && s.TickFrequency > 0.0)
			clamped = SliderSnap.SnapToTick(clamped, s.Minimum, s.Maximum, s.TickFrequency);
		return clamped;
	}

	// ── GTK backing ─────────────────────────────────────────────────────────

	Gtk.Scale _scale = null!;
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		var gtkOrientation = Orientation == Orientation.Vertical
			? Gtk.Orientation.Vertical
			: Gtk.Orientation.Horizontal;

		_scale = new Gtk.Scale(gtkOrientation, Minimum, Maximum, SmallChange > 0.0 ? SmallChange : 1.0);
		_updating = true;
		try { _scale.Value = Value; }
		finally { _updating = false; }
		_scale.Inverted = IsDirectionReversed;
		_scale.ValueChanged += OnGtkValueChanged;
		ApplyTickProps();
		return _scale;
	}

	/// <inheritdoc/>
	protected override void OnMinimumChanged(double oldMinimum, double newMinimum) => ApplyRange();

	/// <inheritdoc/>
	protected override void OnMaximumChanged(double oldMaximum, double newMaximum) => ApplyRange();

	void ApplyRange()
	{
		if (_scale == null) return;
		// SetRange clamps the adjustment and echoes back; push the effective Value afterwards so a range that
		// grows back restores it.
		var wasUpdating = _updating;
		_updating = true;
		try
		{
			_scale.SetRange(Minimum, Maximum);
			if (_scale.Value != Value) _scale.Value = Value;
		}
		finally { _updating = wasUpdating; }
		ApplyTickProps();
	}

	/// <inheritdoc/>
	protected override void OnValueChanged(double oldValue, double newValue)
	{
		if (_scale != null && _scale.Value != newValue)
		{
			var wasUpdating = _updating;
			_updating = true;
			try { _scale.Value = newValue; }
			finally { _updating = wasUpdating; }
		}
		// Raised for every effective change, including GTK-origin ones; _updating only guards the write loop.
		base.OnValueChanged(oldValue, newValue);
	}

	void OnGtkValueChanged(object? sender, EventArgs e)
	{
		if (_updating || _scale.Value == Value) return;
		_updating = true;
		try { SetValue(ValueProperty, _scale.Value); }
		finally { _updating = false; }
	}
}
