using System.Windows.Threading;

namespace System.Windows;

/// <summary>
/// Window lifecycle: <see cref="SourceInitialized"/>, the root Loaded/Unloaded trigger and
/// <see cref="ContentRendered"/>, driven by the native GTK window's realize/map signals and its
/// <c>Gdk.FrameClock</c>.
/// </summary>
/// <remarks>
/// WPF order on <see cref="Show"/>: SourceInitialized → Activated → Loaded (pre-order) →
/// ContentRendered. GTK mapping: <c>realize</c> (before map — the HWND-created moment) raises
/// SourceInitialized; toplevel <c>map</c> posts the Loaded broadcast at
/// <see cref="DispatcherPriority.Loaded"/>; <see cref="Closed"/> (or unrealize) broadcasts
/// Unloaded. Activated comes from <c>notify::is-active</c>, whose timing is up to the window manager.
/// As in WPF's <c>Window.OnContentChanged</c>, every <see cref="ContentControl.Content"/> change
/// arms ContentRendered: before Loaded it is raised by the first frame-clock <c>after-paint</c>
/// once loaded (or right after the Loaded broadcast when GTK painted first); on a loaded window it
/// is raised by the next <c>after-paint</c> (a redraw is queued). A window whose Content was
/// never set raises no ContentRendered.
/// </remarks>
public partial class Window
{
	bool _sourceInitialized;
	bool _contentRenderPending;
	bool _paintedBeforeLoaded;
	bool _afterPaintHooked;
	bool _laidOut;
	Gdk.FrameClock? _frameClock;

	/// <summary>Raised once when the native window has been created (GTK realize), before it is shown.</summary>
	public event EventHandler? SourceInitialized;

	/// <summary>
	/// Raised after the window's content has been rendered: after the first render once loaded,
	/// and again after each later <see cref="ContentControl.Content"/> change.
	/// </summary>
	public event EventHandler? ContentRendered;

	/// <summary>Raises <see cref="SourceInitialized"/>.</summary>
	protected virtual void OnSourceInitialized(EventArgs e) => SourceInitialized?.Invoke(this, e);

	/// <summary>Raises <see cref="ContentRendered"/>.</summary>
	protected virtual void OnContentRendered(EventArgs e) => ContentRendered?.Invoke(this, e);

	/// <summary>
	/// Called from <see cref="ApplyChromeToWindow"/>: connects realize/map/unrealize to the
	/// lifecycle. The frame clock is connected on realize. (The Unloaded broadcast after
	/// <see cref="Closed"/> is raised by <c>CloseCore</c>, after every Closed handler ran.)
	/// </summary>
	internal void WireLifecycleSignals(Gtk.Window win)
	{
		win.Realized += (_, _) => OnNativeRealized(win);
		win.Mapped += (_, _) => PostLoadedBroadcast();
		win.Unrealized += (_, _) => OnNativeUnrealized();
	}

	void OnNativeRealized(Gtk.Window win)
	{
		RaiseSourceInitializedOnce();
		var clock = win.FrameClock;
		if (clock == null || ReferenceEquals(clock, _frameClock))
			return;
		_frameClock = clock;
		clock.Layout += OnFrameLayout;
		HookAfterPaint();
	}

	void OnNativeUnrealized()
	{
		_laidOut = false;
		if (_frameClock != null)
		{
			_frameClock.Layout -= OnFrameLayout;
			UnhookAfterPaint();
			_frameClock = null;
		}
		LifecycleBroadcaster.BroadcastUnloaded(this);
	}

	void HookAfterPaint()
	{
		if (_afterPaintHooked || _frameClock == null) return;
		_afterPaintHooked = true;
		_frameClock.AfterPaint += OnFrameAfterPaint;
	}

	void UnhookAfterPaint()
	{
		if (!_afterPaintHooked || _frameClock == null) return;
		_afterPaintHooked = false;
		_frameClock.AfterPaint -= OnFrameAfterPaint;
	}

	void OnFrameLayout(object? sender, EventArgs e)
	{
		_laidOut = true;
		if (IsLoaded)
		{
			GtkAllocationSync.Sync(this);
			ImplicitAdornerLayer.RepositionAll();
		}
	}

	void OnFrameAfterPaint(object? sender, EventArgs e)
	{
		if (IsLoaded)
			GtkAllocationSync.Sync(this);
		OnFramePainted();
		// Nothing left to report: stop listening until the next Content change re-arms it.
		if (IsLoaded && !_contentRenderPending)
			UnhookAfterPaint();
	}

	void RaiseSourceInitializedOnce()
	{
		if (_sourceInitialized) return;
		_sourceInitialized = true;
		OnSourceInitialized(EventArgs.Empty);
	}

	void PostLoadedBroadcast()
	{
		Dispatcher.CurrentDispatcher.BeginInvoke(() =>
		{
			if (_closed) return;
			LifecycleBroadcaster.BroadcastLoaded(this);
			// If GTK already laid out a frame, a static window lays out no further frames: publish the allocation
			// now. Otherwise (dispatcher work runs ahead of GTK layout) the first layout pass syncs, so the window
			// is never published with children that are still 0x0.
			if (_window != null && _window.IsMapped && _laidOut)
				GtkAllocationSync.Sync(this);
			else
				_window?.QueueResize();   // guarantees the layout pass (and its sync) rather than assuming it
			// A frame painted before Loaded ran (GTK paints at a higher priority than idle work)
			// already rendered the content: report it now rather than waiting for another frame.
			if (_paintedBeforeLoaded)
				RaiseContentRenderedIfPending();
		}, DispatcherPriority.Loaded);
	}

	void OnFramePainted()
	{
		if (IsLoaded)
			RaiseContentRenderedIfPending();
		else
			_paintedBeforeLoaded = true;
	}

	/// <summary>
	/// WPF <c>Window.OnContentChanged</c>: arms ContentRendered. Before Loaded the Loaded broadcast
	/// (or the first paint after it) raises it; once loaded the next painted frame does — or, with
	/// no realized frame clock, dispatcher work at <see cref="DispatcherPriority.Input"/> (WPF's
	/// <c>PostContentRendered</c> priority).
	/// </summary>
	void OnContentChangedForRender()
	{
		_contentRenderPending = true;
		if (!IsLoaded || _closed)
			return;
		if (_frameClock != null)
		{
			HookAfterPaint();
			_window?.QueueDraw();
		}
		else
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(RaiseContentRenderedIfPending, DispatcherPriority.Input);
		}
	}

	void RaiseContentRenderedIfPending()
	{
		if (!_contentRenderPending || _closed) return;
		_contentRenderPending = false;
		OnContentRendered(EventArgs.Empty);
	}

	// ── Test seams ────────────────────────────────────────────────────────────

	/// <summary>
	/// Headless seam: runs the realize → map → first-paint sequence without a realized
	/// <see cref="Gtk.Window"/> (the Loaded broadcast is still posted through the dispatcher).
	/// </summary>
	internal void RaiseLifecycleForTest()
	{
		RaiseSourceInitializedOnce();
		PostLoadedBroadcast();
		OnFramePainted();
	}

	/// <summary>Headless seam: the Unloaded broadcast that follows <see cref="Closed"/>.</summary>
	internal void RaiseClosedLifecycleForTest() => LifecycleBroadcaster.BroadcastUnloaded(this);
}
