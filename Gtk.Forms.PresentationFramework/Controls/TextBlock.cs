namespace System.Windows.Controls
{
	/// <summary>
	/// Displays a read-only text string. Backed by a <see cref="Gtk.Label"/>;
	/// changes to <see cref="Text"/> are reflected immediately via a property-changed callback.
	/// </summary>
	public class TextBlock : FrameworkElement
	{
		/// <summary>The text to display.</summary>
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(
				nameof(Text),
				typeof(string),
				typeof(TextBlock),
				new PropertyMetadata(string.Empty, OnTextChanged));

		static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tb = (TextBlock)d;
			if (tb._label != null)
				tb._label.Text = (string?)e.NewValue ?? string.Empty;
		}

		/// <summary>Gets or sets the displayed text.</summary>
		public string Text
		{
			get => (string?)GetValue(TextProperty) ?? string.Empty;
			set => SetValue(TextProperty, value);
		}

		Gtk.Label _label = null!;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_label = new Gtk.Label(Text);
			_label.Xalign = 0f; // left-align by default, matching WPF TextBlock
			return _label;
		}
	}
}
