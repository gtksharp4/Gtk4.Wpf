using System.ComponentModel;
using System.IO;

namespace System.Windows.Media.Imaging;

/// <summary>
/// WPF <c>BitmapImage</c>: loads an image from a URI or stream. Decoding is lazy (first size, format or
/// pixel access) and goes through the same codecs as <see cref="BitmapDecoder"/>; a failure raises
/// <see cref="DecodeFailed"/> and leaves the bitmap empty instead of throwing.
/// </summary>
public sealed partial class BitmapImage : BitmapSource, ISupportInitialize
{
	bool _initializing;
	bool _initialized;
	bool _decoded;
	byte[] _pixels = Array.Empty<byte>();
	Uri? _uriSource;
	Stream? _streamSource;
	byte[]? _loaded;

	/// <summary>Creates an empty bitmap; set <see cref="UriSource"/> between
	/// <see cref="BeginInit"/> and <see cref="EndInit"/>.</summary>
	public BitmapImage() { }

	/// <summary>Creates a bitmap whose content is loaded from <paramref name="uriSource"/>.</summary>
	public BitmapImage(Uri uriSource)
	{
		if (uriSource == null)
			throw new ArgumentNullException(nameof(uriSource));
		BeginInit();
		UriSource = uriSource;
		EndInit();
	}

	/// <summary>Raised when the image cannot be decoded.</summary>
	public event EventHandler<ExceptionEventArgs>? DecodeFailed;

	/// <summary>The URI the image is loaded from, or <see langword="null"/> when none is set.</summary>
	/// <exception cref="InvalidOperationException">Set after initialization.</exception>
	public Uri? UriSource
	{
		get => _uriSource;
		set
		{
			ThrowIfInitialized();
			_uriSource = value;
		}
	}

	/// <summary>The stream the image is loaded from, or <see langword="null"/> when none is set.</summary>
	/// <exception cref="InvalidOperationException">Set after initialization.</exception>
	public Stream? StreamSource
	{
		get => _streamSource;
		set
		{
			ThrowIfInitialized();
			_streamSource = value;
		}
	}

	/// <summary>Begins a batch of property initialisation.</summary>
	/// <exception cref="InvalidOperationException">Already initializing.</exception>
	public void BeginInit()
	{
		if (_initializing)
			throw new InvalidOperationException("BeginInit was already called.");
		_initializing = true;
	}

	/// <summary>Ends a batch of property initialisation.</summary>
	/// <exception cref="InvalidOperationException">No BeginInit, or neither source is set.</exception>
	public void EndInit()
	{
		if (!_initializing)
			throw new InvalidOperationException("EndInit requires a matching BeginInit.");
		if (_uriSource == null && _streamSource == null)
			throw new InvalidOperationException("Set UriSource or StreamSource before EndInit.");
		_initializing = false;
		_initialized = true;
		LoadEagerly();
	}

	/// <summary>Reads the source bytes now when the caller may release the source: a stream always, a local
	/// file with <see cref="BitmapCacheOption.OnLoad"/>. A failure raises <see cref="DecodeFailed"/>.</summary>
	void LoadEagerly()
	{
		try
		{
			if (_streamSource != null)
				_loaded = BitmapDecoder.ReadStream(_streamSource);
			else if (_cacheOption == BitmapCacheOption.OnLoad && _uriSource != null)
			{
				string path = ImageSourceLocation.ResolveUri(_uriSource);
				if (File.Exists(path))
					_loaded = File.ReadAllBytes(path);
			}
		}
		catch (Exception e)
		{
			_decoded = true;
			DecodeFailed?.Invoke(this, new ExceptionEventArgs(e));
		}
	}

	void ThrowIfInitialized()
	{
		if (_initialized)
			throw new InvalidOperationException("The property cannot be changed after initialization.");
	}

	/// <inheritdoc/>
	public override int PixelWidth
	{
		get { Decode(); return base.PixelWidth; }
	}

	/// <inheritdoc/>
	public override int PixelHeight
	{
		get { Decode(); return base.PixelHeight; }
	}

	/// <inheritdoc/>
	public override double DpiX
	{
		get { Decode(); return base.DpiX; }
	}

	/// <inheritdoc/>
	public override double DpiY
	{
		get { Decode(); return base.DpiY; }
	}

	/// <inheritdoc/>
	public override PixelFormat Format
	{
		get { Decode(); return base.Format; }
	}

	/// <inheritdoc/>
	public override BitmapPalette? Palette
	{
		get { Decode(); return base.Palette; }
	}

	internal override byte[] GetPixelData()
	{
		Decode();
		return _pixels;
	}

	void Decode()
	{
		if (_decoded)
			return;
		_decoded = true;
		if (_loaded == null && _uriSource == null && _streamSource == null)
			return;
		try
		{
			var decoder = _loaded != null || _streamSource != null
				? BitmapDecoder.Create(_loaded ?? BitmapDecoder.ReadStream(_streamSource!))
				: BitmapDecoder.Create(_uriSource!, CreateOptions, CacheOption);
			var frame = decoder.Frames[0];
			Apply(DecodedImage.FromSource(frame));
		}
		catch (Exception e)
		{
			DecodeFailed?.Invoke(this, new ExceptionEventArgs(e));
		}
	}
}
