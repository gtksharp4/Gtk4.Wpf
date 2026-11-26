using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

public partial class Slider
{
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

}
