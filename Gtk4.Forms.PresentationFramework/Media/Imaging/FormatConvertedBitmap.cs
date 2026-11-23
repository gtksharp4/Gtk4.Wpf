using System.ComponentModel;

namespace System.Windows.Media.Imaging;

/// <summary>
/// WPF <c>FormatConvertedBitmap</c>: a <see cref="BitmapSource"/> converted to another pixel format. Gray (1, 2,
/// 4, 8, 16 bits), Bgr24, Rgb24, Bgr32, Bgra32, Pbgra32 and Indexed1/2/4/8 are supported; any other destination
/// throws <see cref="NotSupportedException"/>. Gray uses Rec.709 luma on the sRGB values (WIC converts through
/// linear light, so values differ slightly). An indexed destination without a palette gets one built from the source.
/// </summary>
public sealed class FormatConvertedBitmap : BitmapSource, ISupportInitialize
{
	/// <summary>Identifies the Source property.</summary>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
		nameof(Source), typeof(BitmapSource), typeof(FormatConvertedBitmap),
		new PropertyMetadata(null, null, (d, v) => CoerceInput(d, SourceProperty, v)));

	/// <summary>Identifies the DestinationFormat property.</summary>
	public static readonly DependencyProperty DestinationFormatProperty = DependencyProperty.Register(
		nameof(DestinationFormat), typeof(PixelFormat), typeof(FormatConvertedBitmap),
		new PropertyMetadata(PixelFormats.Pbgra32, null, (d, v) => CoerceInput(d, DestinationFormatProperty, v)));

	/// <summary>Identifies the DestinationPalette property.</summary>
	public static readonly DependencyProperty DestinationPaletteProperty = DependencyProperty.Register(
		nameof(DestinationPalette), typeof(BitmapPalette), typeof(FormatConvertedBitmap),
		new PropertyMetadata(null, null, (d, v) => CoerceInput(d, DestinationPaletteProperty, v)));

	/// <summary>Identifies the AlphaThreshold property.</summary>
	public static readonly DependencyProperty AlphaThresholdProperty = DependencyProperty.Register(
		nameof(AlphaThreshold), typeof(double), typeof(FormatConvertedBitmap),
		new PropertyMetadata(0.0, null, (d, v) => CoerceInput(d, AlphaThresholdProperty, v)));

	/// <summary>Creates an uninitialised bitmap; set the properties between BeginInit and EndInit.</summary>
	public FormatConvertedBitmap() { }

	/// <summary>Converts <paramref name="source"/> to <paramref name="destinationFormat"/>.</summary>
	/// <param name="source">The bitmap to convert.</param>
	/// <param name="destinationFormat">The target pixel format.</param>
	/// <param name="destinationPalette">The palette of an indexed target; may be null.</param>
	/// <param name="alphaThreshold">Percent (0 to 100): below this alpha an indexed pixel takes the
	/// palette's transparent entry.</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public FormatConvertedBitmap(BitmapSource source, PixelFormat destinationFormat,
		BitmapPalette? destinationPalette, double alphaThreshold)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));
		BeginInit();
		Source = source;
		DestinationFormat = destinationFormat;
		DestinationPalette = destinationPalette;
		AlphaThreshold = alphaThreshold;
		EndInit();
	}

	/// <summary>The bitmap that is converted.</summary>
	public BitmapSource? Source
	{
		get => (BitmapSource?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	/// <summary>The pixel format of the result (default Pbgra32).</summary>
	public PixelFormat DestinationFormat
	{
		get => (PixelFormat)GetValue(DestinationFormatProperty)!;
		set => SetValue(DestinationFormatProperty, value);
	}

	/// <summary>The palette of an indexed result.</summary>
	public BitmapPalette? DestinationPalette
	{
		get => (BitmapPalette?)GetValue(DestinationPaletteProperty);
		set => SetValue(DestinationPaletteProperty, value);
	}

	/// <summary>The alpha percentage below which an indexed pixel is transparent (default 0).</summary>
	public double AlphaThreshold
	{
		get => (double)GetValue(AlphaThresholdProperty)!;
		set => SetValue(AlphaThresholdProperty, value);
	}

	/// <summary>Begins initialisation.</summary>
	public void BeginInit() => BeginInitCore();

	/// <summary>Ends initialisation and converts.</summary>
	/// <exception cref="NotSupportedException">The destination format is not supported.</exception>
	public void EndInit() => EndInitCore();

	/// <inheritdoc/>
	public override bool CanFreeze => !IsInitializing && base.CanFreeze
		&& CanFreezeInput(Source);

	/// <summary>Creates an unfrozen, independent copy of this bitmap.</summary>
	public new FormatConvertedBitmap Clone() => (FormatConvertedBitmap)base.Clone();

	/// <summary>Creates an unfrozen, independent copy of this bitmap using its current values.</summary>
	public new FormatConvertedBitmap CloneCurrentValue() => (FormatConvertedBitmap)base.CloneCurrentValue();

	internal override DependencyProperty[] InputProperties => new[]
	{
		SourceProperty, DestinationFormatProperty, DestinationPaletteProperty, AlphaThresholdProperty,
	};

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new FormatConvertedBitmap();

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
		var format = DestinationFormat;
		var bgra = source.ToBgra32();
		var palette = DestinationPalette;
		if (format.Name.StartsWith("Indexed", StringComparison.Ordinal))
			palette ??= new BitmapPalette(source, 1 << format.BitsPerPixel);
		else
			palette = null;
		var data = FormatWriter.Write(bgra, source.PixelWidth, source.PixelHeight, format, palette?.ColorArray,
			AlphaThreshold);
		return new RealizedBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, format, palette,
			data);
	}
}
