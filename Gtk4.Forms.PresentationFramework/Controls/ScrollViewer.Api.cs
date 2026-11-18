namespace System.Windows.Controls;

public partial class ScrollViewer
{
	// ── ScrollChanged routed event ───────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="ScrollChanged"/> routed event.
	/// Raised whenever a scroll offset, extent, or viewport dimension changes.
	/// </summary>
	public static readonly RoutedEvent ScrollChangedEvent =
		EventManager.RegisterRoutedEvent(
			nameof(ScrollChanged),
			RoutingStrategy.Bubble,
			typeof(EventHandler<ScrollChangedEventArgs>),
			typeof(ScrollViewer));

	/// <summary>
	/// Raised when any scroll offset, extent, or viewport dimension changes.
	/// Not raised headless (no GTK widget).
	/// </summary>
	public event EventHandler<ScrollChangedEventArgs>? ScrollChanged
	{
		add    => AddHandler(ScrollChangedEvent, value!);
		remove => RemoveHandler(ScrollChangedEvent, value!);
	}

	// ── Pure helpers (testable without a widget) ─────────────────────────────

	/// <summary>
	/// Clamps <paramref name="requested"/> into [0, <paramref name="scrollable"/>].
	/// </summary>
	internal static double ClampOffset(double requested, double scrollable)
		=> Math.Max(0.0, Math.Min(scrollable, requested));

	/// <summary>
	/// Returns <c>stepIncrement</c>, or <paramref name="fallback"/> when it is ≤ 0.
	/// </summary>
	internal static double EffectiveStep(double stepIncrement, double fallback = 16.0)
		=> stepIncrement > 0.0 ? stepIncrement : fallback;

	// ── Read-only offset / extent / viewport ────────────────────────────────

	/// <summary>Current horizontal scroll offset; 0 when not realized.</summary>
	public double HorizontalOffset  => _scroll?.Hadjustment?.Value    ?? 0.0;

	/// <summary>Current vertical scroll offset; 0 when not realized.</summary>
	public double VerticalOffset    => _scroll?.Vadjustment?.Value    ?? 0.0;

	/// <summary>Total horizontal extent of the scrollable content; 0 when not realized.</summary>
	public double ExtentWidth       => _scroll?.Hadjustment?.Upper    ?? 0.0;

	/// <summary>Total vertical extent of the scrollable content; 0 when not realized.</summary>
	public double ExtentHeight      => _scroll?.Vadjustment?.Upper    ?? 0.0;

	/// <summary>Width of the visible viewport; 0 when not realized.</summary>
	public double ViewportWidth     => _scroll?.Hadjustment?.PageSize ?? 0.0;

	/// <summary>Height of the visible viewport; 0 when not realized.</summary>
	public double ViewportHeight    => _scroll?.Vadjustment?.PageSize ?? 0.0;

	/// <summary>Maximum horizontal offset (ExtentWidth − ViewportWidth, floored at 0); 0 when not realized.</summary>
	public double ScrollableWidth   => Math.Max(0.0, ExtentWidth  - ViewportWidth);

	/// <summary>Maximum vertical offset (ExtentHeight − ViewportHeight, floored at 0); 0 when not realized.</summary>
	public double ScrollableHeight  => Math.Max(0.0, ExtentHeight - ViewportHeight);

	// ── Scroll commands ──────────────────────────────────────────────────────

	/// <summary>Scrolls to <paramref name="offset"/>, clamped to [0, <see cref="ScrollableHeight"/>]. No-op headless.</summary>
	public void ScrollToVerticalOffset(double offset)
		=> ApplyVerticalOffset(ClampOffset(offset, ScrollableHeight));

	/// <summary>Scrolls to <paramref name="offset"/>, clamped to [0, <see cref="ScrollableWidth"/>]. No-op headless.</summary>
	public void ScrollToHorizontalOffset(double offset)
		=> ApplyHorizontalOffset(ClampOffset(offset, ScrollableWidth));

	/// <summary>Scrolls up by one step. No-op headless.</summary>
	public void LineUp()
		=> ApplyVerticalOffset(ClampOffset(VerticalOffset   - EffectiveStep(_scroll?.Vadjustment?.StepIncrement ?? 0), ScrollableHeight));

	/// <summary>Scrolls down by one step. No-op headless.</summary>
	public void LineDown()
		=> ApplyVerticalOffset(ClampOffset(VerticalOffset   + EffectiveStep(_scroll?.Vadjustment?.StepIncrement ?? 0), ScrollableHeight));

	/// <summary>Scrolls left by one step. No-op headless.</summary>
	public void LineLeft()
		=> ApplyHorizontalOffset(ClampOffset(HorizontalOffset - EffectiveStep(_scroll?.Hadjustment?.StepIncrement ?? 0), ScrollableWidth));

	/// <summary>Scrolls right by one step. No-op headless.</summary>
	public void LineRight()
		=> ApplyHorizontalOffset(ClampOffset(HorizontalOffset + EffectiveStep(_scroll?.Hadjustment?.StepIncrement ?? 0), ScrollableWidth));

	/// <summary>Scrolls up by one page. No-op headless.</summary>
	public void PageUp()    => ApplyVerticalOffset(ClampOffset(VerticalOffset   - ViewportHeight, ScrollableHeight));

	/// <summary>Scrolls down by one page. No-op headless.</summary>
	public void PageDown()  => ApplyVerticalOffset(ClampOffset(VerticalOffset   + ViewportHeight, ScrollableHeight));

	/// <summary>Scrolls left by one page. No-op headless.</summary>
	public void PageLeft()  => ApplyHorizontalOffset(ClampOffset(HorizontalOffset - ViewportWidth,  ScrollableWidth));

	/// <summary>Scrolls right by one page. No-op headless.</summary>
	public void PageRight() => ApplyHorizontalOffset(ClampOffset(HorizontalOffset + ViewportWidth,  ScrollableWidth));

	/// <summary>Scrolls to the top. No-op headless.</summary>
	public void ScrollToTop()      => ApplyVerticalOffset(0.0);

	/// <summary>Scrolls to the bottom. No-op headless.</summary>
	public void ScrollToBottom()   => ApplyVerticalOffset(ScrollableHeight);

	/// <summary>Scrolls to the left edge. No-op headless.</summary>
	public void ScrollToLeftEnd()  => ApplyHorizontalOffset(0.0);

	/// <summary>Scrolls to the right edge. No-op headless.</summary>
	public void ScrollToRightEnd() => ApplyHorizontalOffset(ScrollableWidth);

	/// <summary>Scrolls to the top-left corner. No-op headless.</summary>
	public void ScrollToHome()  { ApplyVerticalOffset(0.0);              ApplyHorizontalOffset(0.0); }

	/// <summary>Scrolls to the bottom-right corner. No-op headless.</summary>
	public void ScrollToEnd()   { ApplyVerticalOffset(ScrollableHeight); ApplyHorizontalOffset(ScrollableWidth); }
}
