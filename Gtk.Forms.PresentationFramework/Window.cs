using System;
using System.Threading;
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
		GLib.MainLoop? _dialogLoop;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_window = new Gtk.Window { Title = Title };
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

		/// <summary>Closes and hides the window.</summary>
		public void Close()
		{
			((Gtk.Window)Widget).Hide();
		}

		/// <summary>
		/// Opens this window as a modal dialog. Blocks until <see cref="Close(bool?)"/> is
		/// called and returns <see cref="DialogResult"/>.
		/// </summary>
		/// <remarks>
		/// A real nested <see cref="GLib.MainLoop"/> runs in an interactive session; in a
		/// non-interactive harness call <see cref="Close(bool?)"/> from another thread to
		/// unblock. GTK 4 removed <c>gtk_dialog_run</c>; this pattern is the documented
		/// replacement (spin a MainLoop, quit it on response).
		/// </remarks>
		public bool? ShowDialog()
		{
			DialogResult = null;

			var gtkWin = (Gtk.Window)Widget;
			gtkWin.Modal = true;

			if (Owner != null)
				gtkWin.TransientFor = (Gtk.Window)Owner.Widget;

			Show();

			_dialogLoop = new GLib.MainLoop();
			_dialogLoop.Run();
			_dialogLoop = null;

			return DialogResult;
		}

		/// <summary>
		/// Closes the window and unblocks <see cref="ShowDialog"/> with
		/// <paramref name="dialogResult"/>.
		/// </summary>
		public void Close(bool? dialogResult)
		{
			DialogResult = dialogResult;
			((Gtk.Window)Widget).Hide();
			_dialogLoop?.Quit();
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
