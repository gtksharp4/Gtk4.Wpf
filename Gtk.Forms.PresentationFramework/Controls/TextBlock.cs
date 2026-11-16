using System;
using System.Windows.Documents;
using System.Windows.Markup;

namespace System.Windows.Controls
{
	/// <summary>
	/// Displays a read-only text string or a sequence of <see cref="Inline"/> elements.
	/// Backed by a <see cref="Gtk.Label"/>; Pango markup is used when inlines are present.
	/// <para>Setting <see cref="Text"/> creates a single <see cref="Run"/> and clears any
	/// existing inlines. Adding inlines clears <see cref="Text"/>.</para>
	/// </summary>
	[ContentProperty("Inlines")]
	public partial class TextBlock : FrameworkElement
	{
		// ── Text DP ──────────────────────────────────────────────────────────────────

		/// <summary>The plain text to display.</summary>
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(
				nameof(Text),
				typeof(string),
				typeof(TextBlock),
				new PropertyMetadata(string.Empty, OnTextChanged));

		static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tb = (TextBlock)d;
			var text = (string?)e.NewValue ?? string.Empty;
			if (tb._settingFromInlines) return;
			// Setting Text: replace inlines with a single Run, suppress the inlines→Text loop.
			tb._settingFromText = true;
			try
			{
				tb._inlines.Clear();
				if (text.Length > 0)
					tb._inlines.Add(new Run { Text = text });
			}
			finally { tb._settingFromText = false; }
			tb.ApplyToLabel();
		}

		/// <summary>Gets or sets the displayed text.</summary>
		public string Text
		{
			get => (string?)GetValue(TextProperty) ?? string.Empty;
			set => SetValue(TextProperty, value);
		}

		// ── GTK ──────────────────────────────────────────────────────────────────────

		Gtk.Label _label = null!;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_label = new Gtk.Label(string.Empty);
			_label.Xalign = 0f;
			ApplyToLabel();
			return _label;
		}

		/// <summary>
		/// Applies the current content (plain text or inlines) to the backing GTK label.
		/// When inlines are present, Pango markup is used (<see cref="BuildMarkup"/>).
		/// </summary>
		void ApplyToLabel()
		{
			if (_label == null) return;
			if (_inlines.Count == 0)
			{
				_label.UseMarkup = false;
				_label.Text = string.Empty;
				return;
			}
			// Check if the single inline is a plain Run with no formatting.
			if (_inlines.Count == 1 && _inlines[0] is Run r &&
				r.Foreground == null && !r.FontWeight.HasValue && !r.FontStyle.HasValue)
			{
				_label.UseMarkup = false;
				_label.Text = r.Text;
				return;
			}
			_label.UseMarkup = true;
			_label.Markup = BuildMarkup();
		}

		/// <summary>
		/// Builds the Pango markup string from the current <see cref="Inlines"/> collection.
		/// Exposed internally for unit testing without requiring a live GTK label.
		/// </summary>
		internal string BuildMarkup()
		{
			var sb = new System.Text.StringBuilder();
			foreach (var inline in _inlines)
				inline.AppendMarkup(sb);
			return sb.ToString();
		}
	}
}
