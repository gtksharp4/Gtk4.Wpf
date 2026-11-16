using System.Windows.Controls;

namespace System.Windows
{
	/// <summary>
	/// Top-level window backed by a <see cref="Gtk.Window"/>. Hosts a single
	/// <see cref="ContentControl.Content"/> element and displays a title bar.
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

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.Window? _window;

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
