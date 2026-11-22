using System.ComponentModel;

namespace System.Windows.Media.Imaging;

/// <summary>
/// WPF <c>TransformedBitmap</c>: a <see cref="BitmapSource"/> rotated by multiples of 90 degrees and scaled
/// (negative scale flips). Any other transform, including skew, throws <see cref="InvalidOperationException"/>;
/// a translation is ignored. Scaling is nearest-neighbour. The pixel format is kept.
/// </summary>
public sealed class TransformedBitmap : BitmapSource, ISupportInitialize
{
	const double Epsilon = 1e-6;

	/// <summary>Identifies the Source property.</summary>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
		nameof(Source), typeof(BitmapSource), typeof(TransformedBitmap), new PropertyMetadata(null));

	/// <summary>Identifies the Transform property.</summary>
	public static readonly DependencyProperty TransformProperty = DependencyProperty.Register(
		nameof(Transform), typeof(Transform), typeof(TransformedBitmap), new PropertyMetadata(Transform.Identity));

	/// <summary>Creates an uninitialised bitmap; set the properties between BeginInit and EndInit.</summary>
	public TransformedBitmap() { }

	/// <summary>Transforms <paramref name="source"/> by <paramref name="newTransform"/>.</summary>
	/// <exception cref="ArgumentNullException">An argument is null.</exception>
	/// <exception cref="InvalidOperationException">The transform is not a 90-degree rotation and scale.</exception>
	public TransformedBitmap(BitmapSource source, Transform newTransform)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));
		if (newTransform == null)
			throw new ArgumentNullException(nameof(newTransform));
		BeginInit();
		Source = source;
		Transform = newTransform;
		EndInit();
	}

	/// <summary>The bitmap that is transformed.</summary>
	public BitmapSource? Source
	{
		get => (BitmapSource?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	/// <summary>The transform: rotation by a multiple of 90 degrees and scale.</summary>
	public Transform Transform
	{
		get => (Transform)GetValue(TransformProperty)!;
		set => SetValue(TransformProperty, value);
	}

	/// <summary>Begins initialisation.</summary>
	public void BeginInit() => BeginInitCore();

	/// <summary>Ends initialisation and validates the transform.</summary>
	/// <exception cref="InvalidOperationException">The transform is not supported.</exception>
	public void EndInit() => EndInitCore();

	/// <inheritdoc/>
	public override bool CanFreeze => !IsInitializing && base.CanFreeze;

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new TransformedBitmap();

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		RealizeNow();
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
		var m = Transform.Value;
		bool diagonal = Math.Abs(m.M12) < Epsilon && Math.Abs(m.M21) < Epsilon && Math.Abs(m.M11) > Epsilon
			&& Math.Abs(m.M22) > Epsilon;
		bool turned = Math.Abs(m.M11) < Epsilon && Math.Abs(m.M22) < Epsilon && Math.Abs(m.M12) > Epsilon
			&& Math.Abs(m.M21) > Epsilon;
		if (!diagonal && !turned)
			throw new InvalidOperationException("Only rotations by multiples of 90 degrees and scales are supported.");
		int sw = source.PixelWidth, sh = source.PixelHeight, bpp = source.Format.BitsPerPixel;
		int dw = Math.Max(1, (int)Math.Round((turned ? sh * Math.Abs(m.M21) : sw * Math.Abs(m.M11))));
		int dh = Math.Max(1, (int)Math.Round((turned ? sw * Math.Abs(m.M12) : sh * Math.Abs(m.M22))));
		bool flipX = (turned ? m.M21 : m.M11) < 0, flipY = (turned ? m.M12 : m.M22) < 0;
		int Reverse(bool flip, int i, int n) => flip ? n - 1 - i : i;
		var data = PixelBufferOps.Remap(source.GetPixelData(), sw, bpp, dw, dh, (x, y) => turned
			? (Reverse(flipY, y, dh) * sw / dh, Reverse(flipX, x, dw) * sh / dw)
			: (Reverse(flipX, x, dw) * sw / dw, Reverse(flipY, y, dh) * sh / dh));
		return new RealizedBitmap(dw, dh, turned ? source.DpiY : source.DpiX, turned ? source.DpiX : source.DpiY,
			source.Format, source.Palette, data);
	}
}
