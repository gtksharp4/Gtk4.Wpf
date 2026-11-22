using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// A progress indicator backed by <see cref="Gtk.ProgressBar"/>. The range DPs come from
/// <see cref="RangeBase"/> (Maximum defaults to 100). When <see cref="IsIndeterminate"/> is <c>true</c>
/// the bar pulses; otherwise the fill fraction is derived from <see cref="RangeBase.Value"/>.
/// </summary>
public partial class ProgressBar : RangeBase
{
	static ProgressBar()
	{
		MaximumProperty.OverrideMetadata(typeof(ProgressBar), new FrameworkPropertyMetadata(100.0));
	}

	// ── IsIndeterminate DP ───────────────────────────────────────────────────

	/// <summary><c>true</c> for a pulsing (indeterminate) bar.</summary>
	public static readonly DependencyProperty IsIndeterminateProperty =
		DependencyProperty.Register(
			nameof(IsIndeterminate),
			typeof(bool),
			typeof(ProgressBar),
			new PropertyMetadata(false, OnRangeOrValueChanged));

	/// <summary>Gets or sets whether the bar is indeterminate.</summary>
	public bool IsIndeterminate
	{
		get => (bool?)GetValue(IsIndeterminateProperty) == true;
		set => SetValue(IsIndeterminateProperty, value);
	}

	// ── GTK backing ─────────────────────────────────────────────────────────

	Gtk.ProgressBar _bar = null!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_bar = new Gtk.ProgressBar();
		ApplyState();
		return _bar;
	}

	/// <inheritdoc/>
	protected override void OnMinimumChanged(double oldMinimum, double newMinimum) => ApplyState();

	/// <inheritdoc/>
	protected override void OnMaximumChanged(double oldMaximum, double newMaximum) => ApplyState();

	/// <inheritdoc/>
	protected override void OnValueChanged(double oldValue, double newValue)
	{
		ApplyState();
		base.OnValueChanged(oldValue, newValue);
	}

	static void OnRangeOrValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ProgressBar)d).ApplyState();
	}

	void ApplyState()
	{
		if (_bar == null) return;
		if (IsIndeterminate)
		{
			_bar.Pulse();
		}
		else
		{
			var range = Maximum - Minimum;
			var fraction = range <= 0 ? 0.0 : Math.Max(0, Math.Min(1, (Value - Minimum) / range));
			_bar.Fraction = fraction;
		}
	}
}