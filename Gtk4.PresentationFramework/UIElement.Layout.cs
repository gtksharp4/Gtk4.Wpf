using System;

namespace System.Windows;

/// <summary>
/// Two-pass WPF layout contract for UIElement.
/// <para>
/// <b>Managed layout vs GTK.</b> <see cref="Measure"/>/<see cref="Arrange"/> compute
/// the WPF <see cref="DesiredSize"/>/<see cref="RenderSize"/>/<see cref="ArrangeRect"/>
/// contract and are authoritative for the WPF API and tests. They do not yet move live
/// GTK widgets — a future <c>Gtk.Fixed</c>-based application pass will wire the computed
/// rects onto native positioning (tracked as a carry-over in
/// <c>docs/plans/gtk-forms-wpf-on-gtk-1to1.md</c>).
/// </para>
/// </summary>
public abstract partial class UIElement
{
	// ── Dirty flags and cached constraints ──────────────────────────────────

	bool _measureDirty = true;
	bool _arrangeDirty = true;

	/// <summary>The constraint passed to the last successful <see cref="Measure"/> call.</summary>
	Size _lastMeasureConstraint = new Size(double.NegativeInfinity, double.NegativeInfinity);

	/// <summary>The final rect passed to the last <see cref="Arrange"/> call.</summary>
	Rect _lastArrangeRect = new Rect(double.NegativeInfinity, double.NegativeInfinity, 0, 0);

	Size _renderSize;

	/// <summary>
	/// The final render size of this element: set by <see cref="Arrange"/> and, for a mapped
	/// element, by the live GTK allocation (see <c>FrameworkElement.ActualSize.cs</c>).
	/// As in WPF the setter only stores the value; <see cref="OnRenderSizeChanged"/> (and so
	/// <c>SizeChanged</c>) is driven by the layout system, not by assigning this property.
	/// </summary>
	public Size RenderSize
	{
		get => _renderSize;
		set => _renderSize = value;
	}

	/// <summary>
	/// Called when <see cref="RenderSize"/> changes on at least one axis. The base is a no-op;
	/// <see cref="FrameworkElement"/> updates ActualWidth/ActualHeight and raises SizeChanged.
	/// </summary>
	protected internal virtual void OnRenderSizeChanged(SizeChangedInfo info) { }

	/// <summary>The final rect this element was arranged into.</summary>
	internal Rect ArrangeRect { get; private set; }

	// ── Measure ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Measures this element given <paramref name="availableSize"/>.
	/// Sets <see cref="DesiredSize"/>; derived classes override <see cref="MeasureCore"/>.
	/// <para>
	/// Re-runs <see cref="MeasureCore"/> when dirty OR when <paramref name="availableSize"/>
	/// differs from the last call — matching WPF's <c>previousAvailableSize</c> semantics so
	/// a Grid's resolved-cell second pass is never silently skipped.
	/// </para>
	/// <para>
	/// <b>Managed layout vs GTK note</b> — see class-level remark on <see cref="UIElement"/>.
	/// </para>
	/// </summary>
	public void Measure(Size availableSize)
	{
		// Re-measure when dirty OR when the constraint changed (WPF previousAvailableSize).
		bool constraintChanged = availableSize != _lastMeasureConstraint;
		if (!_measureDirty && !constraintChanged && DesiredSize != Size.Empty)
			return;

		_measureDirty = false;
		NoteLayoutWork();
		_lastMeasureConstraint = availableSize;

		// Collapsed takes no space: WPF skips MeasureCore and reports (0,0). Hidden measures normally.
		if (Visibility == Visibility.Collapsed)
		{
			DesiredSize = new Size(0, 0);
			return;
		}

		var desired = MeasureCore(availableSize);
		// W2: clamp to [0, ∞) only — not to availableSize.
		// WPF lets DesiredSize exceed the available space; Min/Max are enforced inside
		// FrameworkElement.MeasureCore, not here.
		desired = new Size(
			Math.Max(0, desired.Width),
			Math.Max(0, desired.Height));
		DesiredSize = desired;
	}

	/// <summary>Override to compute desired size. Base returns zero.</summary>
	protected virtual Size MeasureCore(Size availableSize) => new Size(0, 0);

	// ── Arrange ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Arranges this element into <paramref name="finalRect"/>.
	/// Sets <see cref="RenderSize"/> (via <see cref="ArrangeCore"/>);
	/// derived classes override <see cref="ArrangeCore"/> which is responsible for
	/// setting <see cref="RenderSize"/> to the actual rendered size.
	/// Re-arranges when dirty OR when <paramref name="finalRect"/> differs from the last call.
	/// </summary>
	public void Arrange(Rect finalRect)
	{
		bool rectChanged = finalRect != _lastArrangeRect;
		if (!_arrangeDirty && !rectChanged)
			return;

		_arrangeDirty = false;
		_lastArrangeRect = finalRect;
		NoteLayoutWork();
		ArrangeRect = finalRect;
		// Collapsed: ArrangeCore is skipped and RenderSize is left as it was (WPF records only the rect).
		if (Visibility == Visibility.Collapsed)
			return;
		// Default RenderSize = finalRect size; ArrangeCore (FrameworkElement) may narrow it
		// for alignment. FrameworkElement.ArrangeCore sets RenderSize itself.
		var previous = _renderSize;
		_arrangeDepth++;
		try
		{
			_renderSize = new Size(finalRect.Width, finalRect.Height);
			ArrangeCore(finalRect);
			// Once mapped the live GTK allocation owns RenderSize (GtkAllocationSync): a managed
			// arrange must not overwrite it, or the two writers would ping-pong SizeChanged.
			if (IsRenderSizeOwnedByGtk)
				_renderSize = previous;
			else
				QueueSizeChanged(previous);
		}
		finally
		{
			_arrangeDepth--;
		}
		// WPF fires the SizeChanged chain after the whole (outermost) arrange pass.
		if (_arrangeDepth == 0)
		{
			FlushSizeChangedChain();
			FireLayoutUpdated();
		}
	}

	/// <summary>Override to position children. Base is no-op.</summary>
	protected virtual void ArrangeCore(Rect finalRect) { }

	// ── Invalidation ────────────────────────────────────────────────────────

	/// <summary>Marks measure as dirty; a subsequent <see cref="UpdateLayout"/> will re-measure.</summary>
	public void InvalidateMeasure()
	{
		_measureDirty = true;
		_arrangeDirty = true;
	}

	/// <summary>Marks arrange as dirty.</summary>
	public void InvalidateArrange()
	{
		_arrangeDirty = true;
	}

	/// <summary>
	/// Re-runs measure and arrange if dirty, using the last-known constraints.
	/// Passes infinite available size on first call.
	/// <para>
	/// <b>Managed layout vs GTK note</b> — see class-level remark on <see cref="UIElement"/>.
	/// </para>
	/// </summary>
	public void UpdateLayout()
	{
		if (_measureDirty)
			Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		if (_arrangeDirty)
			Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
		FireLayoutUpdated();
	}
}