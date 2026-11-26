using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

/// <summary>
/// Rasterises each distinct material brush once into a 256x256 surface for the length of one paint pass, so the
/// per-triangle fills reuse one pattern instead of decoding the brush again.
/// </summary>
sealed class TextureCache : IDisposable
{
	internal const int Size = 256;

	readonly Dictionary<Brush, Cairo.SurfacePattern> _patterns = new Dictionary<Brush, Cairo.SurfacePattern>();
	readonly List<Cairo.ImageSurface> _surfaces = new List<Cairo.ImageSurface>();

	/// <summary>Number of brush rasterisations since startup (test hook).</summary>
	internal static int RasterizeCount { get; private set; }

	/// <summary>The brush as a pattern over a 256x256 texture; a brush with no paint source is transparent.</summary>
	internal Cairo.SurfacePattern Get(Brush brush)
	{
		if (_patterns.TryGetValue(brush, out var cached)) return cached;
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
		var pattern = new Cairo.SurfacePattern(surface) { Extend = Cairo.Extend.Pad, Filter = Cairo.Filter.Bilinear };
		_surfaces.Add(surface);
		_patterns[brush] = pattern;
		return pattern;
	}

	public void Dispose()
	{
		foreach (var pattern in _patterns.Values) pattern.Dispose();
		foreach (var surface in _surfaces) surface.Dispose();
		_patterns.Clear();
		_surfaces.Clear();
	}
}
