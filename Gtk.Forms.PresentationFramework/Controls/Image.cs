using System.Windows.Media;

namespace System.Windows.Controls
{
	/// <summary>
	/// Displays an image backed by <see cref="Gtk.Image"/>.
	/// <see cref="Source"/> accepts a <see cref="ImageSource"/> (file path);
	/// for Phase-1 a plain <c>string</c> path is also accepted via implicit
	/// <see cref="ImageSource.FromFile"/>.
	/// </summary>
	public class Image : FrameworkElement
	{
		// ── Source DP ────────────────────────────────────────────────────────────

		/// <summary>The image to display.</summary>
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register(
				nameof(Source),
				typeof(ImageSource),
				typeof(Image),
				new PropertyMetadata(null, OnSourceChanged));

		static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var img = (Image)d;
			if (img._image != null)
				img.ApplySource((ImageSource?)e.NewValue);
		}

		/// <summary>Gets or sets the image source.</summary>
		public ImageSource? Source
		{
			get => (ImageSource?)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.Image _image = null!;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_image = new Gtk.Image();
			ApplySource(Source);
			return _image;
		}

		void ApplySource(ImageSource? source)
		{
			if (source == null)
				_image.Clear();
			else
				_image.File = source.FilePath;
		}
	}
}
