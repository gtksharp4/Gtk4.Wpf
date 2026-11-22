namespace System.Windows.Media.Imaging;

public sealed partial class BitmapImage
{
	/// <summary>Whether the bitmap can be frozen: not while <see cref="BeginInit"/> is open.</summary>
	public override bool CanFreeze => !_initializing && base.CanFreeze;

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new BitmapImage();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var clone = (BitmapImage)destination;
		clone._uriSource = _uriSource;
		clone._streamSource = _streamSource;
		clone._loaded = _loaded;
		clone._decodePixelWidth = _decodePixelWidth;
		clone._decodePixelHeight = _decodePixelHeight;
		clone._cacheOption = _cacheOption;
		clone._createOptions = _createOptions;
		clone._rotation = _rotation;
		clone._sourceRect = _sourceRect;
		clone._baseUri = _baseUri;
		clone._uriCachePolicy = _uriCachePolicy;
		clone._initialized = _initialized;
		if (!_initialized)
			return;
		clone._decoded = true;
		clone._pixels = (byte[])GetPixelData().Clone();
		clone.PixelWidth = PixelWidth;
		clone.PixelHeight = PixelHeight;
		clone.DpiX = DpiX;
		clone.DpiY = DpiY;
		clone.Format = Format;
		clone.Palette = Palette;
	}
}
