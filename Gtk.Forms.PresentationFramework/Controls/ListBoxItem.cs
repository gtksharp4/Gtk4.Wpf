namespace System.Windows.Controls
{
	/// <summary>
	/// An item container inside a <see cref="ListBox"/>. Wraps the item's visual and
	/// adds selection state.
	/// <para>
	/// Backed by a <see cref="Gtk.ListBoxRow"/> that hosts the item's
	/// <see cref="UIElement"/> widget.
	/// </para>
	/// </summary>
	public class ListBoxItem : ContentControl
	{
		// ── IsSelected DP ────────────────────────────────────────────────────────

		/// <summary>Whether this item is currently selected.</summary>
		public static readonly DependencyProperty IsSelectedProperty =
			DependencyProperty.Register(
				nameof(IsSelected),
				typeof(bool),
				typeof(ListBoxItem),
				new PropertyMetadata(false));

		/// <summary>Gets or sets the selected state.</summary>
		public bool IsSelected
		{
			get => (bool?)GetValue(IsSelectedProperty) ?? false;
			set => SetValue(IsSelectedProperty, value);
		}

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.ListBoxRow? _row;

		/// <summary>The backing <see cref="Gtk.ListBoxRow"/>.</summary>
		internal Gtk.ListBoxRow? Row => _row;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_row = new Gtk.ListBoxRow();

			// Host the content inside the row.
			if (Content is UIElement el)
				_row.Child = el.Widget;
			else
			{
				var label = new Gtk.Label(Content?.ToString() ?? string.Empty);
				_row.Child = label;
			}

			return _row;
		}
	}
}
