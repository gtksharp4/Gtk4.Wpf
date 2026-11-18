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
/// <see cref="DispatcherPriority.Loaded"/>; the first frame-clock <c>after-paint</c> once loaded
/// raises ContentRendered (first paint only — GTK has no per-HWND composition notification);
/// <see cref="Closed"/> (or unrealize) broadcasts Unloaded. Activated comes from
/// <c>notify::is-active</c>, whose timing is up to the window manager.
/// </remarks>
public partial class Window
{
	bool _sourceInitialized;
	bool _contentRendered;
	bool _paintedBeforeLoaded;
	Gdk.FrameClock? _frameClock;

	/// <summary>Raised once when the native window has been created (GTK realize), before it is shown.</summary>
	public event EventHandler? SourceInitialized;

	/// <summary>Raised once after the window's content has been rendered for the first time.</summary>
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
		clock.AfterPaint += OnFrameAfterPaint;
	}

	void OnNativeUnrealized()
	{
		if (_frameClock != null)
		{
			_frameClock.Layout -= OnFrameLayout;
			_frameClock.AfterPaint -= OnFrameAfterPaint;
			_frameClock = null;
		}
		LifecycleBroadcaster.BroadcastUnloaded(this);
	}

	void OnFrameLayout(object? sender, EventArgs e)
	{
		if (IsLoaded)
			GtkAllocationSync.Sync(this);
	}

	void OnFrameAfterPaint(object? sender, EventArgs e)
	{
		if (IsLoaded)
			GtkAllocationSync.Sync(this);
		OnFramePainted();
		if (_contentRendered && _frameClock != null)
			_frameClock.AfterPaint -= OnFrameAfterPaint;
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
			// GTK has usually allocated (and laid out) before this idle work runs, and a static
			// window lays out no further frames: publish the current allocation now.
			if (_window != null && _window.IsMapped)
				GtkAllocationSync.Sync(this);
			// A frame painted before Loaded ran (GTK paints at a higher priority than idle work)
			// already rendered the content: report it now rather than waiting for another frame.
			if (_paintedBeforeLoaded)
				RaiseContentRenderedOnce();
		}, DispatcherPriority.Loaded);
	}

	void OnFramePainted()
	{
		if (IsLoaded)
			RaiseContentRenderedOnce();
		else
			_paintedBeforeLoaded = true;
	}

	void RaiseContentRenderedOnce()
	{
		if (_contentRendered) return;
		_contentRendered = true;
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
