using System;
using System.Windows.Controls;

namespace System.Windows
{
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
	public class Window : ContentControl
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
		/// The dialog result set by <see cref="Close(bool?)"/>; <c>null</c> when the
		/// window was closed without setting an explicit result.
		/// </summary>
		public bool? DialogResult { get; private set; }

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.Window? _window;
		bool _dialogClosed;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_window = new Gtk.Window { Title = Title };
			// F2: wire window-manager close (title-bar X button) to Close(null) so
			// ShowDialog unblocks when the user dismisses the window.
			_window.CloseRequest += OnCloseRequest;
			return _window;
		}

		/// <summary>Shows the window (makes it visible).</summary>
		public void Show()
		{
			var win = (Gtk.Window)Widget;

			// Guard against double-add: OnPropertyChanged already parents the content widget
			// when Content is set after the window exists. Only add it here if it has not yet
			// been added (i.e. Content was set before Show() materialised the backing window).
			if (Content is UIElement contentElement
				&& !ReferenceEquals(win.Child, contentElement.Widget))
			{
				win.Add(contentElement.Widget);
			}

			win.ShowAll();
		}

		/// <summary>
		/// Closes the window, leaving <see cref="DialogResult"/> unchanged (null if never
		/// explicitly set). Unblocks any pending <see cref="ShowDialog"/> call.
		/// This is the WPF-idiomatic close path (e.g. a Cancel button with no explicit result).
		/// </summary>
		public void Close() => CloseCore(keepResult: true);

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
			DialogResult = null;
			_dialogClosed = false;

			var gtkWin = (Gtk.Window)Widget;
			gtkWin.Modal = true;

			if (Owner != null)
				gtkWin.TransientFor = (Gtk.Window)Owner.Widget;

			Show();

			// Pump until a close path sets _dialogClosed. RunIteration(true) blocks for the
			// next event and dispatches it (including the close handler), keeping the thread live.
			while (!_dialogClosed)
				Gtk.Application.RunIteration(true);

			return DialogResult;
		}

		/// <summary>
		/// Closes the window and unblocks <see cref="ShowDialog"/> with
		/// <paramref name="dialogResult"/>. Must be called on the GTK thread.
		/// </summary>
		public void Close(bool? dialogResult)
		{
			DialogResult = dialogResult;
			CloseCore(keepResult: true);
		}

		// ── Internal helpers ──────────────────────────────────────────────────────

		/// <summary>
		/// Common close implementation. Sets the flag and hides the backing window.
		/// <paramref name="keepResult"/> is always true from public callers — the parameter
		/// exists so the window-manager handler can be routed here without resetting DialogResult.
		/// </summary>
		void CloseCore(bool keepResult)
		{
			_ = keepResult; // reserved for future use
			_dialogClosed = true;
			((Gtk.Window)Widget).Hide();
		}

		/// <summary>
		/// Handles the GTK close-request signal (window-manager X button / Alt+F4).
		/// Sets the closed flag so <see cref="ShowDialog"/> unblocks with
		/// <see cref="DialogResult"/> = null (no explicit result set by the app).
		/// </summary>
		void OnCloseRequest(object sender, Gtk.CloseRequestArgs e)
		{
			CloseCore(keepResult: true);
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			// When Content changes after Show() has been called, re-parent the widget.
			if (e.Property == ContentProperty && _window != null)
			{
				if (e.OldValue is UIElement old)
					_window.Remove(old.Widget);
				if (e.NewValue is UIElement newEl)
					_window.Add(newEl.Widget);
			}
		}
	}
}
