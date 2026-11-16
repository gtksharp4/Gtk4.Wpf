using System;
using System.Windows.Data;

namespace System.Windows.Controls
{
	/// <summary>
	/// Single-line text input backed by <see cref="Gtk.Entry"/>.
	/// Two-way capable: GTK edits are written back into <see cref="Text"/>.
	/// </summary>
	public class TextBox : Control
	{
		// ── TextChanged event ────────────────────────────────────────────────────

		/// <summary>Raised when <see cref="Text"/> changes.</summary>
		public event EventHandler<TextChangedEventArgs>? TextChanged;

		// ── Text DP ──────────────────────────────────────────────────────────────

		/// <summary>The text in the box.</summary>
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(
				nameof(Text),
				typeof(string),
				typeof(TextBox),
				new PropertyMetadata(string.Empty, OnTextChanged));

		static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tb = (TextBox)d;
			if (tb._entry != null && tb._entry.Text != (string?)e.NewValue)
				tb._entry.Text = (string?)e.NewValue ?? string.Empty;
			tb.TextChanged?.Invoke(tb, new TextChangedEventArgs(tb));
		}

		/// <summary>Gets or sets the displayed text.</summary>
		public string Text
		{
			get => (string?)GetValue(TextProperty) ?? string.Empty;
			set => SetValue(TextProperty, value);
		}

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.Entry _entry = null!;
		bool _updating;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_entry = new Gtk.Entry { Text = Text };
			_entry.Changed += OnGtkChanged;
			return _entry;
		}

		void OnGtkChanged(object? sender, EventArgs e)
		{
			if (_updating) return;
			_updating = true;
			try { SetValue(TextProperty, _entry.Text); }
			finally { _updating = false; }
		}
	}

	/// <summary>Event data for <see cref="TextBox.TextChanged"/>.</summary>
	public sealed class TextChangedEventArgs : EventArgs
	{
		/// <summary>The text box whose text changed.</summary>
		public TextBox Source { get; }
		internal TextChangedEventArgs(TextBox source) => Source = source;
	}
}
