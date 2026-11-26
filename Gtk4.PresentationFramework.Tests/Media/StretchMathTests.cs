using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Unit tests for <see cref="StretchMath.ComputeDestRect"/> — pure function, no GTK.
/// </summary>
public class StretchMathTests
{
	const double Tol = 1e-9;

	// ── Stretch.Fill ─────────────────────────────────────────────────────────

	[Fact]
	public void Fill_uses_full_dest_area()
	{
		var (x, y, w, h) = StretchMath.ComputeDestRect(100, 50, 200, 200, Stretch.Fill);
		Assert.Equal(0, x);
		Assert.Equal(0, y);
		Assert.Equal(200, w);
		Assert.Equal(200, h);
	}

	// ── Stretch.None ─────────────────────────────────────────────────────────

	[Fact]
	public void None_centres_natural_size()
	{
		// 100×50 image in 200×200 box → centred at (50, 75).
		var (x, y, w, h) = StretchMath.ComputeDestRect(100, 50, 200, 200, Stretch.None);
		Assert.Equal(50, x, Tol);
		Assert.Equal(75, y, Tol);
		Assert.Equal(100, w, Tol);
		Assert.Equal(50, h, Tol);
	}

	[Fact]
	public void None_larger_than_dest_has_negative_offset()
	{
		// 300×200 image in 100×100 box → offset (-100, -50).
		var (x, y, w, h) = StretchMath.ComputeDestRect(300, 200, 100, 100, Stretch.None);
		Assert.Equal(-100, x, Tol);
		Assert.Equal(-50, y, Tol);
		Assert.Equal(300, w, Tol);
		Assert.Equal(200, h, Tol);
	}

	// ── Stretch.Uniform ──────────────────────────────────────────────────────

	[Fact]
	public void Uniform_wide_image_in_square_box_limited_by_width()
	{
		// 100×50 image in 200×200 box.
		// scale = min(200/100, 200/50) = min(2, 4) = 2 → 200×100, centred vertically.
		var (x, y, w, h) = StretchMath.ComputeDestRect(100, 50, 200, 200, Stretch.Uniform);
		Assert.Equal(0, x, Tol);
		Assert.Equal(50, y, Tol);   // (200-100)/2
		Assert.Equal(200, w, Tol);
		Assert.Equal(100, h, Tol);
	}

	[Fact]
	public void Uniform_square_image_in_wide_box_limited_by_height()
	{
		// 100×100 in 300×100 → scale = min(3, 1) = 1 → 100×100 centred horizontally.
		var (x, y, w, h) = StretchMath.ComputeDestRect(100, 100, 300, 100, Stretch.Uniform);
		Assert.Equal(100, x, Tol);  // (300-100)/2
		Assert.Equal(0, y, Tol);
		Assert.Equal(100, w, Tol);
		Assert.Equal(100, h, Tol);
	}

	// ── Stretch.UniformToFill ─────────────────────────────────────────────────

	[Fact]
	public void UniformToFill_wide_image_in_square_box_clips_horizontally()
	{
		// 100×50 image in 200×200 box.
		// scale = max(200/100, 200/50) = max(2, 4) = 4 → 400×200, centred.
		var (x, y, w, h) = StretchMath.ComputeDestRect(100, 50, 200, 200, Stretch.UniformToFill);
		Assert.Equal(-100, x, Tol);  // (200-400)/2
		Assert.Equal(0, y, Tol);
		Assert.Equal(400, w, Tol);
		Assert.Equal(200, h, Tol);
	}

	[Fact]
	public void UniformToFill_exact_match_no_offset()
	{
		// 200×200 in 200×200 → scale 1, no offset.
		var (x, y, w, h) = StretchMath.ComputeDestRect(200, 200, 200, 200, Stretch.UniformToFill);
		Assert.Equal(0, x, Tol);
		Assert.Equal(0, y, Tol);
		Assert.Equal(200, w, Tol);
		Assert.Equal(200, h, Tol);
	}

	// ── Edge cases ────────────────────────────────────────────────────────────

	[Fact]
	public void Zero_source_width_returns_full_dest()
	{
		var (x, y, w, h) = StretchMath.ComputeDestRect(0, 100, 200, 200, Stretch.Uniform);
		Assert.Equal(0, x);
		Assert.Equal(0, y);
		Assert.Equal(200, w);
		Assert.Equal(200, h);
	}

	[Fact]
	public void Zero_source_height_returns_full_dest()
	{
		var (x, y, w, h) = StretchMath.ComputeDestRect(100, 0, 200, 200, Stretch.Fill);
		Assert.Equal(0, x);
		Assert.Equal(0, y);
		Assert.Equal(200, w);
		Assert.Equal(200, h);
	}
}
