using System;
using System.Windows.Controls;

namespace System.Windows;

/// <summary>
/// Top-level window backed by a <see cref="Gtk.Window"/>. Hosts a single
/// <see cref="ContentControl.Content"/> element and displays a title bar.
/// Supports modal dialog semantics via <see cref="ShowDialog"/> and
/// <see cref="Close(bool?)"/>.
/// </summary>
/// <remarks>
/// Phase-0: uses a bare <see cref="Gtk.Window"/>. Phase-1 refinement will swap this
/// for the full <c>FormsWindow</c>/<c>GtkThemes</c> stack from
/// <c>Gtk4.Forms.Platform.Gtk</c> (the STA-thread host, the GtkSynchronizationContext,
/// and the full Forms layout pass).
/// </remarks>
public partial class Window : ContentControl
{
	// ── Title ────────────────────────────────────────────────────────────────

	/// <summary>The window title shown in the title bar.</summary>
	public static readonly DependencyProperty TitleProperty =
		DependencyProperty.Register(
			nameof(Title),
			typeof(string),
			typeof(Window),
			new PropertyMetadata(string.Empty, OnTitleChanged));

	static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var w = (Window)d;
		if (w._window != null)
			w._window.Title = (string?)e.NewValue ?? string.Empty;
	}

	/// <summary>Gets or sets the window title.</summary>
	public string Title
	{
		get => (string?)GetValue(TitleProperty) ?? string.Empty;
		set => SetValue(TitleProperty, value);
	}

	// ── Owner / DialogResult ──────────────────────────────────────────────────

	/// <summary>The owner window; modal dialogs are transient for it.</summary>
	public Window? Owner { get; set; }

	/// <summary>
	/// Raised once after the window has closed (WPF-compatible). <see cref="Application"/>
	/// subscribes to the main window's event to exit on close (default shutdown mode).
	/// </summary>
	public event EventHandler? Closed;

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Window? _window;
	bool _dialogClosed;
	bool _closed;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_window = new Gtk.Window { Title = Title };
		// GTK4 removed per-window image icons, so a HeaderBar titlebar hosts the window icon
		// (see Window.Icon.cs). Installed here so the icon shows from first display.
		InstallHeaderBar(_window);
		// Apply stored chrome DPs (WindowState, WindowStyle, ResizeMode) now that the
		// native window exists. Must follow InstallHeaderBar so _headerBar is set.
		ApplyChromeToWindow(_window);
		// F2: wire window-manager close (title-bar X button) to Close(null) so
		// ShowDialog unblocks when the user dismisses the window.
		_window.CloseRequest += OnCloseRequest;
		AttachAcceleratorController(_window);
		_window.Child = CreateContentOverlay();
		return _window;
	}

	/// <summary>
	/// A window's <see cref="FrameworkElement.Width"/>/<see cref="FrameworkElement.Height"/>
	/// is its initial on-screen extent, not a minimum-size request, so set the GTK top-level
	/// default size instead of a size request. When <see cref="SizeToContent"/> suppresses a
	/// dimension, that axis is left at -1 so GTK sizes it to the content's natural extent.
	/// </summary>
	protected override void ApplyExplicitSize(Gtk.Widget widget)
	{
		var stc = SizeToContent;
		int w = (stc == SizeToContent.Width || stc == SizeToContent.WidthAndHeight || double.IsNaN(Width))
			? -1
			: (int)Math.Round(Width);
		int h = (stc == SizeToContent.Height || stc == SizeToContent.WidthAndHeight || double.IsNaN(Height))
			? -1
			: (int)Math.Round(Height);
		if ((w > 0 || h > 0) && widget is Gtk.Window gtkWin)
		{
			// SetDefaultSize is the WPF-equivalent (initial size, resizable), but the
			// GTK4 Windows backend ignores it for a resizable top-level and realises the
			// window at its content's natural size. A size request is honoured, so it is
			// used to pin the window to its WPF Width/Height. Trade-off: the window cannot
			// then be resized below Width/Height (WPF allows that); acceptable for Phase-0.
			gtkWin.SetDefaultSize(w, h);
			gtkWin.SetSizeRequest(w, h);
		}
	}

	/// <summary>Parents the content, applies the start location and presents the GTK window.</summary>
	void ShowCore()
	{
		var win = (Gtk.Window)Widget;

		// Guard against double-add: OnPropertyChanged already parents the content widget
		// when Content is set after the window exists. Only set it here if it has not yet
		// been set (i.e. Content was set before Show() materialised the backing window).
		if (Content is UIElement contentElement
		    && !ReferenceEquals(_contentOverlay!.Child, contentElement.Widget))
		{
			_contentOverlay.Child = contentElement.Widget;
		}

		// Apply startup location before presenting (GTK4 has no Move() — currently a no-op).
		ApplyStartupLocation(win);

		// GTK4: Present() maps the window. ShowAll() (GTK3) realises it at the content's
		// natural size, so it is not used.
		win.Present();
		SetShown(true);
	}

	/// <summary>
	/// Closes the window; a dialog closed without a result reads <see cref="DialogResult"/> false.
	/// A <see cref="Closing"/> handler may veto the close by setting
	/// <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to <see langword="true"/>.
	/// </summary>
	public void Close() => RequestClose();

	/// <summary>
	/// Opens this window as a modal dialog. Blocks until <see cref="Close()"/> or
	/// <see cref="Close(bool?)"/> is called, or the window manager closes the window,
	/// then returns <see cref="DialogResult"/>.
	/// </summary>
	/// <remarks>
	/// GTK 4 removed <c>gtk_dialog_run</c>. This pumps the GTK main loop on the
	/// <b>current (GTK) thread</b> until a close path sets the closed flag, rather than
	/// running a nested blocking <see cref="GLib.MainLoop"/>: a blocking loop would freeze
	/// the single GTK thread so the same-thread response (a button handler, a timeout)
	/// could never run. All close paths must be invoked on the GTK thread (GTK is not
	/// thread-safe) — e.g. from a widget event handler or a <c>GLib.Timeout</c>/
	/// <c>GLib.Idle</c> callback, which dispatch inside this pump.
	/// </remarks>
	public bool? ShowDialog()
	{
		VerifyNotClosed();
		_dialogResult = null;
		_dialogClosed = false;
		_showingAsDialog = true;

		var gtkWin = (Gtk.Window)Widget;
		gtkWin.Modal = true;

		if (Owner != null)
			gtkWin.TransientFor = (Gtk.Window)Owner.Widget;

		Show();

		// Pump until a close path sets _dialogClosed. RunIteration(true) blocks for the
		// next event and dispatches it (including the close handler), keeping the thread live.
		try
		{
			while (!_dialogClosed)
				Gtk.Application.RunIteration(true);
		}
		finally
		{
			_showingAsDialog = false;
			_dialogResult ??= false; // WPF: closing without a result is false
		}
		return _dialogResult;
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property != ContentProperty)
			return;
		// When Content changes after Show() has been called, re-parent the widget.
		if (_window != null)
			SetOverlayContent(e.NewValue);
		// WPF Window.OnContentChanged: every Content change re-arms ContentRendered.
		OnContentChangedForRender();
	}
}