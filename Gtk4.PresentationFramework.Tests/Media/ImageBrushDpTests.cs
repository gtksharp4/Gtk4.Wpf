using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure (no GTK) tests for <see cref="ImageBrush"/>, <see cref="TileBrush"/>,
/// <see cref="TileMode"/>, and the <see cref="System.Windows.Media.Stretch"/> enum
/// as used by tile brushes.
/// </summary>
public class ImageBrushDpTests
{
	// ── Default property values ───────────────────────────────────────────────

	[Fact]
	public void ImageBrush_default_ImageSource_is_null()
	{
		var b = new ImageBrush();
		Assert.Null(b.ImageSource);
	}

	[Fact]
	public void ImageBrush_default_Stretch_is_Fill()
	{
		// WPF default for TileBrush.Stretch is Fill.
		var b = new ImageBrush();
		Assert.Equal(Stretch.Fill, b.Stretch);
	}

	[Fact]
	public void ImageBrush_default_TileMode_is_None()
	{
		var b = new ImageBrush();
		Assert.Equal(TileMode.None, b.TileMode);
	}

	[Fact]
	public void ImageBrush_default_Opacity_is_one()
	{
		var b = new ImageBrush();
		Assert.Equal(1.0, b.Opacity);
	}

	// ── ImageSource constructor ───────────────────────────────────────────────

	[Fact]
	public void ImageBrush_ctor_with_ImageSource_sets_property()
	{
		var src = ImageSource.FromFile("test.png");
		var b = new ImageBrush(src);
		Assert.Same(src, b.ImageSource);
	}

	// ── Property round-trips ──────────────────────────────────────────────────

	[Fact]
	public void ImageBrush_ImageSource_roundtrip()
	{
		var b = new ImageBrush();
		var src = ImageSource.FromFile("/img/logo.png");
		b.ImageSource = src;
		Assert.Same(src, b.ImageSource);
	}

	[Fact]
	public void ImageBrush_Stretch_roundtrip()
	{
		var b = new ImageBrush();
		b.Stretch = Stretch.Uniform;
		Assert.Equal(Stretch.Uniform, b.Stretch);
		b.Stretch = Stretch.UniformToFill;
		Assert.Equal(Stretch.UniformToFill, b.Stretch);
		b.Stretch = Stretch.None;
		Assert.Equal(Stretch.None, b.Stretch);
	}

	[Fact]
	public void ImageBrush_TileMode_roundtrip()
	{
		var b = new ImageBrush();
		b.TileMode = TileMode.Tile;
		Assert.Equal(TileMode.Tile, b.TileMode);
		b.TileMode = TileMode.FlipX;
		Assert.Equal(TileMode.FlipX, b.TileMode);
	}

	[Fact]
	public void ImageBrush_accepts_BitmapImage_as_source()
	{
		var bmp = new BitmapImage(new Uri("https://example.test/img.png"));
		var b = new ImageBrush();
		b.ImageSource = bmp;
		Assert.IsAssignableFrom<ImageSource>(b.ImageSource);
	}

	// ── Type hierarchy ────────────────────────────────────────────────────────

	[Fact]
	public void ImageBrush_is_a_TileBrush()
	{
		Assert.IsAssignableFrom<TileBrush>(new ImageBrush());
	}

	[Fact]
	public void ImageBrush_is_a_Brush()
	{
		Assert.IsAssignableFrom<Brush>(new ImageBrush());
	}

	// ── Headless: assigning to Shape.Fill does not throw ─────────────────────

	[Fact]
	public void ImageBrush_assigned_to_Rectangle_Fill_does_not_throw()
	{
		var r = new Rectangle();
		var brush = new ImageBrush { ImageSource = ImageSource.FromFile("nonexistent.png") };
		var ex = Record.Exception(() => r.Fill = brush);
		Assert.Null(ex);
		Assert.Same(brush, r.Fill);
	}

	[Fact]
	public void ImageBrush_assigned_to_Ellipse_Fill_does_not_throw()
	{
		var e = new Ellipse();
		var brush = new ImageBrush();
		var ex = Record.Exception(() => e.Fill = brush);
		Assert.Null(ex);
	}

	// ── ApplyToCairo with no source is a no-op ────────────────────────────────

	[Fact]
	public void ApplyToCairo_with_no_ImageSource_does_not_throw()
	{
		// No display needed; we just call the internal method.
		var b = new ImageBrush();
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => b.ApplyToCairo(cr, 100, 100));
		Assert.Null(ex);
	}

	[Fact]
	public void ApplyToCairo_with_missing_file_does_not_throw()
	{
		var b = new ImageBrush { ImageSource = ImageSource.FromFile("does_not_exist.png") };
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => b.ApplyToCairo(cr, 100, 100));
		Assert.Null(ex);
	}
}
