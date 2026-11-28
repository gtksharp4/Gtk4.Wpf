using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// Displays an image backed by <see cref="Gtk.Picture"/>.
/// <see cref="Source"/> accepts a <see cref="ImageSource"/> (file path).
/// <see cref="Stretch"/> maps to <see cref="Gtk.ContentFit"/> (GTK 4.8+).
/// <see cref="StretchDirection"/> (UpOnly/DownOnly) has no direct GTK
/// equivalent and is stored only; actual clamping is not applied by GTK.
/// </summary>
public partial class Image : FrameworkElement
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
		if (img._picture != null)
			img.ApplySource((ImageSource?)e.NewValue);
		img.InvalidateMeasure();
	}

	/// <summary>Gets or sets the image source.</summary>
	public ImageSource? Source
	{
		get => (ImageSource?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	// ── Stretch DP ───────────────────────────────────────────────────────────

	/// <summary>
	/// Gets or sets how the image is scaled to fill its allocated space.
	/// Default is <see cref="Media.Stretch.Uniform"/> (preserves aspect ratio, fits inside).
	/// Maps to <see cref="Gtk.ContentFit"/> on the backing <see cref="Gtk.Picture"/>.
	/// </summary>
	public static readonly DependencyProperty StretchProperty =
		DependencyProperty.Register(
			nameof(Stretch),
			typeof(Media.Stretch),
			typeof(Image),
			new PropertyMetadata(Media.Stretch.Uniform, OnStretchChanged));

	static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var img = (Image)d;
		if (img._picture != null)
			img.ApplyStretch((Media.Stretch?)e.NewValue ?? Media.Stretch.Uniform);
	}

	/// <summary>Gets or sets the stretch mode.</summary>
	public Media.Stretch Stretch
	{
		get => (Media.Stretch?)GetValue(StretchProperty) ?? Media.Stretch.Uniform;
		set => SetValue(StretchProperty, value);
	}

	// ── StretchDirection DP ──────────────────────────────────────────────────

	/// <summary>
	/// Gets or sets the direction(s) in which scaling is permitted.
	/// Default is <see cref="Controls.StretchDirection.Both"/>.
	/// GTK has no direct equivalent; this value is stored and honoured best-effort.
	/// </summary>
	public static readonly DependencyProperty StretchDirectionProperty =
		DependencyProperty.Register(
			nameof(StretchDirection),
			typeof(StretchDirection),
			typeof(Image),
			new PropertyMetadata(StretchDirection.Both));

	/// <summary>Gets or sets the stretch direction.</summary>
	public StretchDirection StretchDirection
	{
		get => (StretchDirection?)GetValue(StretchDirectionProperty) ?? StretchDirection.Both;
		set => SetValue(StretchDirectionProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Picture _picture = null!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_picture = new SlotCappedPicture(this);
		ApplySource(Source);
		ApplyStretch(Stretch);
		return _picture;
	}

	void ApplySource(ImageSource? source)
	{
		if (source == null)
			_picture.Paintable = null;
		else if (IsInMemory(source))
			ApplyBitmap((System.Windows.Media.Imaging.BitmapSource)source);
		else
			_picture.Filename = ImageSourceLocation.ResolveFilePath(source);
	}

	void ApplyStretch(Media.Stretch stretch)
	{
		_picture.ContentFit = ImageStretchMap.ToContentFit(stretch);
		// GTK has no natural-size fit: ScaleDown never enlarges, and a picture that cannot shrink
		// requests the intrinsic (pixel) size of its paintable, so Stretch.None shows the image 1:1.
		_picture.CanShrink = stretch != Media.Stretch.None;
		ApplyAlignment();
	}
}
