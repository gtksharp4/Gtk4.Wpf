using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

/// <summary>
/// Rasterises each distinct material brush once into a 256x256 surface for the length of one paint pass, so the
/// per-triangle fills reuse a pattern instead of decoding the brush again.
/// </summary>
sealed class TextureCache : IDisposable
{
	internal const int Size = 256;

	readonly Dictionary<Brush, Cairo.ImageSurface?> _surfaces = new Dictionary<Brush, Cairo.ImageSurface?>();

	/// <summary>Number of brush rasterisations since startup (test hook).</summary>
	internal static int RasterizeCount { get; private set; }

	/// <summary>The brush as a texture, or null when it paints nothing (an empty or unloadable brush).</summary>
	internal Cairo.ImageSurface? Get(Brush brush)
	{
		if (_surfaces.TryGetValue(brush, out var cached)) return cached;
		var surface = Rasterize(brush);
		_surfaces[brush] = surface;
		return surface;
	}

	static Cairo.ImageSurface? Rasterize(Brush brush)
	{
		RasterizeCount++;
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, Size, Size);
		using (var cr = new Cairo.Context(surface))
		{
			// A brush that sets no source (missing image, empty visual) must leave the texture transparent,
			// not fall back to cairo's default opaque black.
			cr.SetSourceRGBA(0, 0, 0, 0);
			brush.ApplyToCairo(cr, Size, Size);
			cr.Paint();
		}
		return surface;
	}

	public void Dispose()
	{
		foreach (var surface in _surfaces.Values) surface?.Dispose();
		_surfaces.Clear();
	}
}
