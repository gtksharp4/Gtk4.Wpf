namespace System.Windows.Controls;

/// <summary>
/// A range control for selecting a value within a range, backed by
/// <see cref="Gtk.Scale"/> (a GTK4-native horizontal/vertical slider).
/// Two-way: GTK value change writes back into <see cref="Value"/>.
/// <para>
/// Setting <see cref="Value"/> clamps it to [<see cref="Minimum"/>, <see cref="Maximum"/>].
/// When <see cref="IsSnapToTickEnabled"/> is <c>true</c>, the value is additionally
/// rounded to the nearest tick defined by <see cref="TickFrequency"/>.
/// </para>
/// </summary>
public partial class Slider : Control
{
	/// <summary>Raised when <see cref="Value"/> changes.</summary>
	public event EventHandler? ValueChanged;

	// ── Minimum DP ───────────────────────────────────────────────────────────

	/// <summary>Minimum value of the range.</summary>
	public static readonly DependencyProperty MinimumProperty =
		DependencyProperty.Register(
			nameof(Minimum),
			typeof(double),
			typeof(Slider),
			new PropertyMetadata(0.0, OnRangeChanged));

	/// <summary>Gets or sets the minimum value.</summary>
	public double Minimum
	{
		get => (double?)GetValue(MinimumProperty) ?? 0.0;
		set => SetValue(MinimumProperty, value);
	}

	// ── Maximum DP ───────────────────────────────────────────────────────────

	/// <summary>Maximum value of the range.</summary>
	public static readonly DependencyProperty MaximumProperty =
		DependencyProperty.Register(
			nameof(Maximum),
			typeof(double),
			typeof(Slider),
			new PropertyMetadata(100.0, OnRangeChanged));

	/// <summary>Gets or sets the maximum value.</summary>
	public double Maximum
	{
		get => (double?)GetValue(MaximumProperty) ?? 100.0;
		set => SetValue(MaximumProperty, value);
	}

	// ── Value DP ─────────────────────────────────────────────────────────────

	/// <summary>Current value. Clamped to [<see cref="Minimum"/>, <see cref="Maximum"/>];
	/// snapped to the nearest tick when <see cref="IsSnapToTickEnabled"/> is <c>true</c>.</summary>
	public static readonly DependencyProperty ValueProperty =
		DependencyProperty.Register(
			nameof(Value),
			typeof(double),
			typeof(Slider),
			new PropertyMetadata(0.0, OnValueChanged,
				(d, v) =>
				{
					var s = (Slider)d;
					var dv = (double?)v ?? 0.0;
					var clamped = Math.Max(s.Minimum, Math.Min(s.Maximum, dv));
					if (s.IsSnapToTickEnabled && s.TickFrequency > 0.0)
						clamped = SliderSnap.SnapToTick(clamped, s.Minimum, s.Maximum, s.TickFrequency);
					return (object?)clamped;
				}));

	static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var s = (Slider)d;
		if (s._scale != null)
		{
			var v = (double?)e.NewValue ?? 0.0;
			if (s._scale.Value != v)
				s._scale.Value = v;
		}
		// Raise for every real value change — including GTK-origin changes.
		// _updating only guards the GTK↔DP write loop (see OnGtkValueChanged).
		s.ValueChanged?.Invoke(s, EventArgs.Empty);
	}

	/// <summary>Gets or sets the current value.</summary>
	public double Value
	{
		get => (double?)GetValue(ValueProperty) ?? 0.0;
		set => SetValue(ValueProperty, value);
	}

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
		_scale.Value = Value;
		_scale.Inverted = IsDirectionReversed;
		_scale.ValueChanged += OnGtkValueChanged;
		ApplyTickProps();
		return _scale;
	}

	static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var s = (Slider)d;
		if (s._scale != null)
		{
			s._scale.SetRange(s.Minimum, s.Maximum);
			s.ApplyTickProps();
		}
		// Clamp value into new range.
		var clamped = Math.Max(s.Minimum, Math.Min(s.Maximum, s.Value));
		if (clamped != s.Value)
			s.Value = clamped;
	}

	void OnGtkValueChanged(object? sender, EventArgs e)
	{
		if (_updating) return;
		_updating = true;
		try { SetValue(ValueProperty, _scale.Value); }
		finally { _updating = false; }
	}
}
