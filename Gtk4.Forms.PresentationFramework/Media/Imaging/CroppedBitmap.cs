using System.ComponentModel;

namespace System.Windows.Media.Imaging;

/// <summary>
/// WPF <c>CroppedBitmap</c>: a rectangular region of a <see cref="BitmapSource"/>, keeping its pixel format.
/// An empty <see cref="SourceRect"/> means the whole source.
/// </summary>
public sealed class CroppedBitmap : BitmapSource, ISupportInitialize
{
	/// <summary>Identifies the Source property.</summary>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
		nameof(Source), typeof(BitmapSource), typeof(CroppedBitmap),
		new PropertyMetadata(null, null, (d, v) => CoerceInput(d, SourceProperty, v)));

	/// <summary>Identifies the SourceRect property.</summary>
	public static readonly DependencyProperty SourceRectProperty = DependencyProperty.Register(
		nameof(SourceRect), typeof(Int32Rect), typeof(CroppedBitmap),
		new PropertyMetadata(Int32Rect.Empty, null, (d, v) => CoerceInput(d, SourceRectProperty, v)));

	/// <summary>Creates an uninitialised bitmap; set the properties between BeginInit and EndInit.</summary>
	public CroppedBitmap() { }

	/// <summary>Crops <paramref name="source"/> to <paramref name="sourceRect"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="ArgumentException">The rectangle is outside the source.</exception>
	public CroppedBitmap(BitmapSource source, Int32Rect sourceRect)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));
		BeginInit();
		Source = source;
		SourceRect = sourceRect;
		EndInit();
	}

	/// <summary>The bitmap that is cropped.</summary>
	public BitmapSource? Source
	{
		get => (BitmapSource?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	/// <summary>The region of <see cref="Source"/> in pixels; empty means all of it.</summary>
	public Int32Rect SourceRect
	{
		get => (Int32Rect)GetValue(SourceRectProperty)!;
		set => SetValue(SourceRectProperty, value);
	}

	/// <summary>Begins initialisation.</summary>
	public void BeginInit() => BeginInitCore();

	/// <summary>Ends initialisation and validates the rectangle.</summary>
	/// <exception cref="ArgumentException">The rectangle is outside the source.</exception>
	public void EndInit() => EndInitCore();

	/// <inheritdoc/>
	public override bool CanFreeze => !IsInitializing && base.CanFreeze
		&& CanFreezeInput(Source);

	/// <summary>Creates an unfrozen, independent copy of this bitmap.</summary>
	public new CroppedBitmap Clone() => (CroppedBitmap)base.Clone();

	/// <summary>Creates an unfrozen, independent copy of this bitmap using its current values.</summary>
	public new CroppedBitmap CloneCurrentValue() => (CroppedBitmap)base.CloneCurrentValue();

	internal override DependencyProperty[] InputProperties => new[] { SourceProperty, SourceRectProperty };

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new CroppedBitmap();

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		RealizeNow();
		FreezeValue(Source);
		base.OnFreeze();
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		InvalidateRealized();
		base.OnPropertyChanged(e);
	}

	internal override RealizedBitmap? Realize()
	{
		var source = Source;
		if (source == null)
			return null;
		var r = SourceRect;
		int sw = source.PixelWidth, sh = source.PixelHeight;
		if (r.IsEmpty)
			r = new Int32Rect(0, 0, sw, sh);
		else if (r.X < 0 || r.Y < 0 || r.Width <= 0 || r.Height <= 0 || r.X + r.Width > sw || r.Y + r.Height > sh)
			throw new ArgumentException("The source rectangle is outside the source bitmap.", nameof(SourceRect));
		var format = source.Format;
		var data = PixelBufferOps.Crop(source.GetPixelData(), sw, format.BitsPerPixel, r);
		return new RealizedBitmap(r.Width, r.Height, source.DpiX, source.DpiY, format, source.Palette, data);
	}
}
