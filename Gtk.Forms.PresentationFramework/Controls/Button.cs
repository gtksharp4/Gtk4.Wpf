using System;

namespace System.Windows.Controls
{
	/// <summary>
	/// A clickable button backed by a <see cref="Gtk.Button"/>.
	/// Raises the CLR <see cref="Click"/> event when the user clicks it,
	/// and updates the GTK button label when <see cref="ContentControl.Content"/>
	/// is a string.
	/// </summary>
	public class Button : ContentControl
	{
		// ── Click event ──────────────────────────────────────────────────────────

		/// <summary>
		/// Routed click event identifier. Registered as Bubble so parent elements
		/// can also handle it.
		/// </summary>
		public static readonly RoutedEvent ClickEvent =
			EventManager.RegisterRoutedEvent(
				nameof(Click),
				RoutingStrategy.Bubble,
				typeof(RoutedEventHandler),
				typeof(Button));

		/// <summary>Raised when the button is clicked.</summary>
		public event RoutedEventHandler Click
		{
			add => AddHandler(ClickEvent, value);
			remove => RemoveHandler(ClickEvent, value);
		}

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.Button _button = null!;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_button = new Gtk.Button();
			_button.Label = (Content as string) ?? string.Empty;
			_button.Clicked += OnGtkClicked;
			return _button;
		}

		/// <inheritdoc/>
		protected override Gtk.Box ContentContainer =>
			throw new InvalidOperationException("Button manages its own content via Label.");

		/// <inheritdoc/>
		protected override Gtk.Label ContentLabel =>
			throw new InvalidOperationException("Button manages its own content via Label.");

		void OnGtkClicked(object? sender, EventArgs e)
		{
			var args = new RoutedEventArgs(ClickEvent) { Source = this };
			RaiseEvent(args);
		}

		/// <summary>
		/// Keeps the GTK button label in sync when <see cref="ContentControl.Content"/> changes.
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == ContentProperty && _button != null && e.NewValue is string s)
				_button.Label = s;
		}
	}
}
