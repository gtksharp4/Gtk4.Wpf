namespace System.Windows.Media.Imaging;

/// <summary>
/// WPF <c>CachedBitmap</c>: a snapshot of a <see cref="BitmapSource"/> taken when it is created, so later changes
/// to the source (for example a <see cref="RenderTargetBitmap"/> rendered again) do not show through. The
/// <c>createOptions</c> and <c>cacheOption</c> are accepted for compatibility; the snapshot is always taken now.
/// </summary>
public sealed class CachedBitmap : BitmapSource
{
	readonly BitmapSource _source;
	readonly BitmapCreateOptions _createOptions;
	readonly BitmapCacheOption _cacheOption;

	/// <summary>Caches <paramref name="source"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public CachedBitmap(BitmapSource source, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
	{
		_source = source ?? throw new ArgumentNullException(nameof(source));
		_createOptions = createOptions;
		_cacheOption = cacheOption;
		RealizeNow();
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new CachedBitmap(_source, _createOptions, _cacheOption);

	internal override RealizedBitmap? Realize() => new RealizedBitmap(_source.PixelWidth, _source.PixelHeight,
		_source.DpiX, _source.DpiY, _source.Format, _source.Palette, (byte[])_source.GetPixelData().Clone());
}
