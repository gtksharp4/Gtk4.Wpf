using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure (headless) tests for the <c>FrameworkElement.LayoutTransformBounds</c> geometric
/// helper: the axis-aligned bounding box of a size after a transform matrix.
/// </summary>
public class LayoutTransformBoundsTests
{
	const double Eps = 1e-6;

	[Fact]
	public void Identity_returns_original_size()
	{
		Size result = FrameworkElement.LayoutTransformBounds(new Size(100, 50), Matrix.Identity);
		Assert.Equal(100, result.Width, Eps);
		Assert.Equal(50, result.Height, Eps);
	}

	[Fact]
	public void Rotate90_swaps_width_and_height()
	{
		var r = new RotateTransform(90);
		Size result = FrameworkElement.LayoutTransformBounds(new Size(100, 50), r.Value);
		// 90° rotation: bounding box is ~50 wide × 100 tall
		Assert.Equal(50, result.Width, Eps);
		Assert.Equal(100, result.Height, Eps);
	}

	[Fact]
	public void Rotate180_returns_original_size()
	{
		var r = new RotateTransform(180);
		Size result = FrameworkElement.LayoutTransformBounds(new Size(100, 50), r.Value);
		Assert.Equal(100, result.Width, Eps);
		Assert.Equal(50, result.Height, Eps);
	}

	[Fact]
	public void Rotate45_enlarges_bounding_box()
	{
		var r = new RotateTransform(45);
		Size result = FrameworkElement.LayoutTransformBounds(new Size(100, 50), r.Value);
		// At 45° both width and height increase; bounding box > original in both dims.
		double expected = (100 + 50) / Math.Sqrt(2); // ~106.07
		Assert.True(result.Width > 100, $"Width {result.Width} should exceed 100");
		Assert.True(result.Height > 50, $"Height {result.Height} should exceed 50");
		Assert.Equal(expected, result.Width, 1e-3);
		Assert.Equal(expected, result.Height, 1e-3);
	}

	[Fact]
	public void Scale_2_3_produces_correct_size()
	{
		var s = new ScaleTransform(2, 3);
		Size result = FrameworkElement.LayoutTransformBounds(new Size(100, 50), s.Value);
		Assert.Equal(200, result.Width, Eps);
		Assert.Equal(150, result.Height, Eps);
	}

	[Fact]
	public void Scale_identity_values_1_1_returns_original()
	{
		var s = new ScaleTransform(1, 1);
		Size result = FrameworkElement.LayoutTransformBounds(new Size(80, 40), s.Value);
		Assert.Equal(80, result.Width, Eps);
		Assert.Equal(40, result.Height, Eps);
	}

	[Fact]
	public void Zero_size_always_returns_zero()
	{
		var r = new RotateTransform(45);
		Size result = FrameworkElement.LayoutTransformBounds(new Size(0, 0), r.Value);
		Assert.Equal(0, result.Width, Eps);
		Assert.Equal(0, result.Height, Eps);
	}
}
