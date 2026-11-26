namespace System.Windows.Controls;

public partial class ScrollViewer
{
	// ── Reentrancy guard (mirrors ScrollBar.Gtk.cs) ──────────────────────────

	bool _scrollUpdating;

	// ── Snapshot for delta computation ───────────────────────────────────────

	double _prevHOffset;
	double _prevVOffset;
	double _prevExtentW;
	double _prevExtentH;
	double _prevViewportW;
	double _prevViewportH;

	// ── Adjustment wiring ────────────────────────────────────────────────────

	/// <summary>Subscribes to adjustment value-changed events; called after widget creation.</summary>
	internal void AttachAdjustmentHandlers()
	{
		if (_scroll?.Hadjustment != null)
			_scroll.Hadjustment.ValueChanged += OnAdjustmentChanged;
		if (_scroll?.Vadjustment != null)
			_scroll.Vadjustment.ValueChanged += OnAdjustmentChanged;
	}

	void OnAdjustmentChanged(object? sender, EventArgs e)
	{
		if (_scrollUpdating) return;
		RaiseScrollChanged();
	}

	void RaiseScrollChanged()
	{
		double hOff = HorizontalOffset;
		double vOff = VerticalOffset;
		double extW = ExtentWidth;
		double extH = ExtentHeight;
		double vpW  = ViewportWidth;
		double vpH  = ViewportHeight;

		var args = new ScrollChangedEventArgs(
			ScrollChangedEvent,
			hOff, vOff, extW, extH, vpW, vpH,
			hOff - _prevHOffset,
			vOff - _prevVOffset,
			extW - _prevExtentW,
			extH - _prevExtentH,
			vpW  - _prevViewportW,
			vpH  - _prevViewportH);

		_prevHOffset   = hOff;
		_prevVOffset   = vOff;
		_prevExtentW   = extW;
		_prevExtentH   = extH;
		_prevViewportW = vpW;
		_prevViewportH = vpH;

		RaiseEvent(args);
	}

	// ── Internal apply helpers ────────────────────────────────────────────────

	void ApplyVerticalOffset(double value)
	{
		if (_scroll?.Vadjustment == null) return;
		if (_scrollUpdating) return;
		_scrollUpdating = true;
		try { _scroll.Vadjustment.Value = value; }
		finally { _scrollUpdating = false; }
		RaiseScrollChanged();
	}

	void ApplyHorizontalOffset(double value)
	{
		if (_scroll?.Hadjustment == null) return;
		if (_scrollUpdating) return;
		_scrollUpdating = true;
		try { _scroll.Hadjustment.Value = value; }
		finally { _scrollUpdating = false; }
		RaiseScrollChanged();
	}
}
