namespace System.Windows.Controls.Primitives;

public partial class ScrollBar
{
	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Scrollbar? _scrollbar;
	Gtk.Adjustment? _adjustment;
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		var gtkOrientation = Orientation == Orientation.Vertical
			? Gtk.Orientation.Vertical
			: Gtk.Orientation.Horizontal;

		// Adjustment ctor: (value, lower, upper, stepIncrement, pageIncrement, pageSize)
		// GTK upper = Maximum + ViewportSize (WPF upper is the last visible position, not the last value).
		_adjustment = new Gtk.Adjustment(
			Value,
			Minimum,
			Maximum + ViewportSize,
			SmallChange,
			LargeChange,
			ViewportSize);

		_adjustment.ValueChanged += OnAdjustmentValueChanged;

		_scrollbar = new Gtk.Scrollbar(gtkOrientation, _adjustment);
		return _scrollbar;
	}

	void OnAdjustmentValueChanged(object? sender, EventArgs e)
	{
		if (_updating || _adjustment == null) return;
		_updating = true;
		try
		{
			var newVal = Math.Max(Minimum, Math.Min(Maximum, _adjustment.Value));
			if (newVal != Value)
			{
				SetValue(ValueProperty, newVal);
				var args = new ScrollEventArgs(ScrollEventType.ThumbTrack, newVal);
				Scroll?.Invoke(this, args);
			}
		}
		finally
		{
			_updating = false;
		}
	}

	/// <summary>Applies current <see cref="ScrollBar.Value"/> to the adjustment (no event loop).</summary>
	void ApplyValueToAdjustment()
	{
		if (_adjustment == null) return;
		if (_updating) return;
		_updating = true;
		try
		{
			if (_adjustment.Value != Value)
				_adjustment.Value = Value;
		}
		finally
		{
			_updating = false;
		}
	}

	/// <summary>Applies Minimum/Maximum/ViewportSize/SmallChange/LargeChange to the adjustment.</summary>
	void ApplyAdjustmentBounds()
	{
		if (_adjustment == null) return;
		// Configure: (value, lower, upper, stepIncrement, pageIncrement, pageSize)
		_adjustment.Configure(
			Value,
			Minimum,
			Maximum + ViewportSize,
			SmallChange,
			LargeChange,
			ViewportSize);
	}

}
