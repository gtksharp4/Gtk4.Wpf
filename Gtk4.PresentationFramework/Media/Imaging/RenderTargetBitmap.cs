using System;

namespace System.Windows.Media.Imaging;

/// <summary>
/// An off-screen bitmap that can be rendered into by calling <see cref="Render"/>.
/// Mirrors <c>System.Windows.Media.Imaging.RenderTargetBitmap</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Supported visual type:</b> only <see cref="DrawingVisual"/> with a committed
/// <see cref="DrawingVisual.Drawing"/> produces pixel output.  Any other
/// <see cref="Visual"/> is accepted but renders nothing (transparent pixels), matching
/// WPF's documented behaviour.
/// </para>
/// <para>
/// <b>Headless / no display:</b> rendering uses a Cairo <see cref="Cairo.ImageSurface"/>
/// (ARGB32 in-memory surface) which does <b>not</b> require a GTK display or an X server.
/// Rendering works in headless environments.
/// </para>
/// <para>
/// <b>Pixel format:</b> the internal buffer is always pre-multiplied BGRA (Cairo ARGB32).
/// <c>CopyPixels</c> copies raw BGRA bytes; pass
/// <see cref="PixelFormats.Pbgra32"/> to the constructor as WPF convention — the value is
/// accepted but does not change the backing format.
/// </para>
/// </remarks>
public sealed class RenderTargetBitmap : BitmapSource
{
	byte[]? _pixels;   // BGRA, row-major, stride = PixelWidth * 4

	/// <summary>
	/// Creates a render target of the given size.
	/// </summary>
	/// <param name="pixelWidth">Width in pixels (must be &gt; 0).</param>
	/// <param name="pixelHeight">Height in pixels (must be &gt; 0).</param>
	/// <param name="dpiX">Horizontal DPI (e.g. 96).</param>
	/// <param name="dpiY">Vertical DPI (e.g. 96).</param>
	/// <param name="pixelFormat">
	/// Accepted for WPF API compatibility; the internal format is always
	/// pre-multiplied BGRA32 (<see cref="PixelFormats.Pbgra32"/>).
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="pixelWidth"/> or <paramref name="pixelHeight"/> is &lt;= 0.
	/// </exception>
	public RenderTargetBitmap(
		int pixelWidth,
		int pixelHeight,
		double dpiX,
		double dpiY,
		PixelFormat pixelFormat)
	{
		if (pixelWidth <= 0)
			throw new ArgumentOutOfRangeException(nameof(pixelWidth), "Must be > 0.");
		if (pixelHeight <= 0)
			throw new ArgumentOutOfRangeException(nameof(pixelHeight), "Must be > 0.");

		PixelWidth  = pixelWidth;
		PixelHeight = pixelHeight;
		DpiX        = dpiX;
		DpiY        = dpiY;
	}

	// ── Render ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Renders <paramref name="visual"/> into this bitmap.
	/// </summary>
	/// <remarks>
	/// Only <see cref="DrawingVisual"/> instances with a committed drawing produce output.
	/// Any other visual (including <see langword="null"/>) is a silent no-op.
	/// Each call replaces the previous pixel data.
	/// </remarks>
	/// <param name="visual">The visual to render, or <see langword="null"/>.</param>
	/// <exception cref="InvalidOperationException">The bitmap is frozen.</exception>
	public void Render(Visual? visual)
	{
		ThrowIfFrozen();
		int stride = PixelWidth * 4;
		byte[] pixels = new byte[PixelHeight * stride];

		if (visual is DrawingVisual dv)
		{
			DrawingContext? ctx = dv.Drawing;
			if (ctx != null && ctx.Commands.Count > 0)
				RenderDrawingContext(ctx, pixels, stride);
		}

		_pixels = pixels;
	}

	void RenderDrawingContext(DrawingContext ctx, byte[] pixels, int stride)
	{
		try
		{
			using var surface = new Cairo.ImageSurface(
				Cairo.Format.Argb32, PixelWidth, PixelHeight);
			using (var cr = new Cairo.Context(surface))
				DrawingContextRenderer.Render(ctx, cr);

			surface.Flush();
			CopyFromSurface(surface, pixels, stride);
		}
		catch
		{
			// If Cairo surface creation fails (no native lib), leave pixels transparent.
		}
	}

	static void CopyFromSurface(Cairo.ImageSurface surface, byte[] pixels, int stride)
	{
		byte[] data = surface.Data;
		int len = Math.Min(data.Length, pixels.Length);
		Array.Copy(data, 0, pixels, 0, len);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() =>
		new RenderTargetBitmap(PixelWidth, PixelHeight, DpiX, DpiY, Format);

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		((RenderTargetBitmap)destination)._pixels = (byte[]?)_pixels?.Clone();
	}

	internal override byte[] GetPixelData() => _pixels ?? new byte[PixelHeight * PixelWidth * 4];

	// ── Internal pixel access for tests ──────────────────────────────────────

	/// <summary>
	/// Returns the BGRA components of a single pixel (for unit tests).
	/// Returns all-zero when <see cref="Render"/> has not been called.
	/// </summary>
	internal (byte B, byte G, byte R, byte A) GetPixel(int x, int y)
	{
		if (_pixels == null) return (0, 0, 0, 0);
		int stride = PixelWidth * 4;
		int offset = y * stride + x * 4;
		return (_pixels[offset], _pixels[offset + 1], _pixels[offset + 2], _pixels[offset + 3]);
	}
}
