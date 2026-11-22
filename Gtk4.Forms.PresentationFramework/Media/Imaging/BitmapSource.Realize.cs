namespace System.Windows.Media.Imaging;

/// <summary>The computed result of a bitmap derived from another one (crop, convert, transform, cache).</summary>
sealed class RealizedBitmap
{
	internal RealizedBitmap(int width, int height, double dpiX, double dpiY, PixelFormat format,
		BitmapPalette? palette, byte[] data)
	{
		Width = width;
		Height = height;
		DpiX = dpiX;
		DpiY = dpiY;
		Format = format;
		Palette = palette;
		Data = data;
	}

	internal int Width { get; }

	internal int Height { get; }

	internal double DpiX { get; }

	internal double DpiY { get; }

	internal PixelFormat Format { get; }

	internal BitmapPalette? Palette { get; }

	internal byte[] Data { get; }
}

public abstract partial class BitmapSource
{
	RealizedBitmap? _realized;
	bool _realizing;

	/// <summary>True between BeginInit and EndInit of a derived bitmap; nothing is computed meanwhile.</summary>
	internal bool IsInitializing { get; set; }

	/// <summary>Computes the bitmap from its inputs; <see langword="null"/> for a bitmap that stores its pixels.</summary>
	internal virtual RealizedBitmap? Realize() => null;

	/// <summary>Drops the computed result; the next read computes it again.</summary>
	internal void InvalidateRealized() => _realized = null;

	byte[]? RealizedPixels()
	{
		EnsureRealized();
		return _realized?.Data;
	}

	void RealizeLazily()
	{
		if (_realized != null || _realizing || IsInitializing)
			return;
		_realizing = true;
		try
		{
			var r = Realize();
			if (r == null)
				return;
			_realized = r;
			_pixelWidth = r.Width;
			_pixelHeight = r.Height;
			_dpiX = r.DpiX;
			_dpiY = r.DpiY;
			_format = r.Format;
			_palette = r.Palette;
		}
		finally
		{
			_realizing = false;
		}
	}

	internal void BeginInitCore()
	{
		if (IsInitializing)
			throw new InvalidOperationException("BeginInit was already called.");
		IsInitializing = true;
	}

	internal void EndInitCore()
	{
		if (!IsInitializing)
			throw new InvalidOperationException("EndInit requires a matching BeginInit.");
		IsInitializing = false;
		RealizeNow();
	}

	/// <summary>Computes the result now, so invalid inputs throw at <c>EndInit</c> as in WPF.</summary>
	internal void RealizeNow()
	{
		_realized = null;
		RealizeLazily();
	}
}
