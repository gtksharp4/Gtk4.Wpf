namespace System.Windows.Media.Imaging;

public sealed partial class BitmapImage
{
	int _decodePixelWidth;
	int _decodePixelHeight;
	BitmapCacheOption _cacheOption;
	BitmapCreateOptions _createOptions;
	Rotation _rotation;
	Int32Rect _sourceRect;
	Uri? _baseUri;

	/// <summary>The source bytes read at <see cref="EndInit"/>, or null when not read eagerly.</summary>
	internal byte[]? LoadedBytes => _loaded;
	System.Net.Cache.RequestCachePolicy? _uriCachePolicy;

	/// <summary>Width the image is scaled to while decoding (0 keeps the source width).</summary>
	public int DecodePixelWidth
	{
		get => _decodePixelWidth;
		set { ThrowIfInitialized(); _decodePixelWidth = Math.Max(0, value); }
	}

	/// <summary>Height the image is scaled to while decoding (0 keeps the source height).</summary>
	public int DecodePixelHeight
	{
		get => _decodePixelHeight;
		set { ThrowIfInitialized(); _decodePixelHeight = Math.Max(0, value); }
	}

	/// <summary>When the image is cached (stored; the decoded pixels are always kept).</summary>
	public BitmapCacheOption CacheOption
	{
		get => _cacheOption;
		set { ThrowIfInitialized(); _cacheOption = value; }
	}

	/// <summary>Options that control how the bitmap is created; <see cref="BitmapCreateOptions.PreservePixelFormat"/>
	/// keeps the decoder pixel format instead of converting to Pbgra32.</summary>
	public BitmapCreateOptions CreateOptions
	{
		get => _createOptions;
		set { ThrowIfInitialized(); _createOptions = value; }
	}

	/// <summary>Rotation applied while decoding.</summary>
	public Rotation Rotation
	{
		get => _rotation;
		set { ThrowIfInitialized(); _rotation = value; }
	}

	/// <summary>The sub-rectangle of the source that is decoded; empty means all of it.</summary>
	public Int32Rect SourceRect
	{
		get => _sourceRect;
		set { ThrowIfInitialized(); _sourceRect = value; }
	}

	/// <summary>The base URI used to resolve a relative <see cref="UriSource"/> (stored).</summary>
	public Uri? BaseUri
	{
		get => _baseUri;
		set { ThrowIfInitialized(); _baseUri = value; }
	}

	/// <summary>The request cache policy (stored; no network loading is performed).</summary>
	public System.Net.Cache.RequestCachePolicy? UriCachePolicy
	{
		get => _uriCachePolicy;
		set { ThrowIfInitialized(); _uriCachePolicy = value; }
	}

	/// <summary>Whether decoding changes the pixels (scale, rotate, crop) so a file path cannot be used.</summary>
	internal bool HasDecodeOptions =>
		_decodePixelWidth > 0 || _decodePixelHeight > 0 || _rotation != Rotation.Rotate0 || !_sourceRect.IsEmpty;

	void Apply(DecodedImage image)
	{
		byte[]? bgra = null;
		int w = image.Width, h = image.Height;
		if (!_sourceRect.IsEmpty)
		{
			bgra = Crop(image.ToBgra32(), w, _sourceRect);
			w = _sourceRect.Width;
			h = _sourceRect.Height;
		}
		if (_rotation != Rotation.Rotate0)
		{
			bgra ??= image.ToBgra32();
			(bgra, w, h) = PixelTransforms.Apply(bgra, w, h, _rotation, false, false);
		}
		if (_decodePixelWidth > 0 || _decodePixelHeight > 0)
		{
			bgra ??= image.ToBgra32();
			double ratio = (double)w / h;
			int nw = _decodePixelWidth > 0
				? _decodePixelWidth : Math.Max(1, (int)Math.Round(_decodePixelHeight * ratio));
			int nh = _decodePixelHeight > 0
				? _decodePixelHeight : Math.Max(1, (int)Math.Round(_decodePixelWidth / ratio));
			bgra = PixelTransforms.Resize(bgra, w, h, nw, nh);
			w = nw;
			h = nh;
		}
		PixelWidth = w;
		PixelHeight = h;
		DpiX = image.DpiX;
		DpiY = image.DpiY;
		if (bgra == null && (_createOptions & BitmapCreateOptions.PreservePixelFormat) != 0)
		{
			Format = image.Format;
			Palette = image.Palette == null ? null : new BitmapPalette(image.Palette);
			_pixels = image.Data;
			return;
		}
		bgra ??= image.ToBgra32();
		Format = (_createOptions & BitmapCreateOptions.PreservePixelFormat) != 0
			? PixelFormats.Bgra32 : PixelFormats.Pbgra32;
		Palette = null;
		_pixels = Format == PixelFormats.Pbgra32 ? Premultiply(bgra) : bgra;
	}

	static byte[] Crop(byte[] bgra, int width, Int32Rect r)
	{
		var o = new byte[r.Width * r.Height * 4];
		for (int y = 0; y < r.Height; y++)
			Buffer.BlockCopy(bgra, ((r.Y + y) * width + r.X) * 4, o, y * r.Width * 4, r.Width * 4);
		return o;
	}

	static byte[] Premultiply(byte[] bgra)
	{
		var o = new byte[bgra.Length];
		for (int i = 0; i < bgra.Length; i += 4)
		{
			int a = bgra[i + 3];
			o[i] = (byte)((bgra[i] * a + 127) / 255);
			o[i + 1] = (byte)((bgra[i + 1] * a + 127) / 255);
			o[i + 2] = (byte)((bgra[i + 2] * a + 127) / 255);
			o[i + 3] = (byte)a;
		}
		return o;
	}
}
