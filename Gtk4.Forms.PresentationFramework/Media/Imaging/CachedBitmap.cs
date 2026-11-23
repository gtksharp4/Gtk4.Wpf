namespace System.Windows.Media.Imaging;

/// <summary>
/// WPF <c>CachedBitmap</c>: a snapshot of a <see cref="BitmapSource"/> taken when it is created, so later changes
/// to the source (for example a <see cref="RenderTargetBitmap"/> rendered again) do not show through, not even in
/// its clones. The <c>createOptions</c> and <c>cacheOption</c> are accepted for compatibility; the snapshot is
/// always taken now.
/// </summary>
public sealed class CachedBitmap : BitmapSource
{
	readonly RealizedBitmap _snapshot;

	/// <summary>Caches <paramref name="source"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public CachedBitmap(BitmapSource source, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));
		_snapshot = new RealizedBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY,
			source.Format, source.Palette, (byte[])source.GetPixelData().Clone());
		RealizeNow();
	}

	CachedBitmap(CachedBitmap original)
	{
		var s = original._snapshot;
		_snapshot = new RealizedBitmap(s.Width, s.Height, s.DpiX, s.DpiY, s.Format, s.Palette, (byte[])s.Data.Clone());
		RealizeNow();
	}

	/// <summary>Creates an unfrozen, independent copy holding the same cached pixels.</summary>
	public new CachedBitmap Clone() => (CachedBitmap)base.Clone();

	/// <summary>Creates an unfrozen, independent copy holding the same cached pixels.</summary>
	public new CachedBitmap CloneCurrentValue() => (CachedBitmap)base.CloneCurrentValue();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new CachedBitmap(this);

	internal override RealizedBitmap? Realize() => _snapshot;
}
