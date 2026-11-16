namespace System.Windows.Controls
{
	/// <summary>
	/// A progress indicator backed by <see cref="Gtk.ProgressBar"/>.
	/// When <see cref="IsIndeterminate"/> is <c>true</c> the bar pulses; otherwise
	/// the fill fraction is derived from <see cref="Value"/> relative to the range.
	/// </summary>
	public class ProgressBar : Control
	{
		// ── Minimum DP ───────────────────────────────────────────────────────────

		/// <summary>Minimum value of the range.</summary>
		public static readonly DependencyProperty MinimumProperty =
			DependencyProperty.Register(
				nameof(Minimum),
				typeof(double),
				typeof(ProgressBar),
				new PropertyMetadata(0.0, OnRangeOrValueChanged));

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
				typeof(ProgressBar),
				new PropertyMetadata(100.0, OnRangeOrValueChanged));

		/// <summary>Gets or sets the maximum value.</summary>
		public double Maximum
		{
			get => (double?)GetValue(MaximumProperty) ?? 100.0;
			set => SetValue(MaximumProperty, value);
		}

		// ── Value DP ─────────────────────────────────────────────────────────────

		/// <summary>Current progress value.</summary>
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
				nameof(Value),
				typeof(double),
				typeof(ProgressBar),
				new PropertyMetadata(0.0, OnRangeOrValueChanged));

		/// <summary>Gets or sets the current value.</summary>
		public double Value
		{
			get => (double?)GetValue(ValueProperty) ?? 0.0;
			set => SetValue(ValueProperty, value);
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
}
