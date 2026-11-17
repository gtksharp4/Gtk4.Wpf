using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>
/// Pure (headless) tests for <see cref="FrameworkElement.LayoutTransform"/>:
/// the geometric helper <c>LayoutTransformBounds</c>, the DP contract, and the
/// effect on <see cref="UIElement.DesiredSize"/> after <see cref="UIElement.Measure"/>.
/// No GTK runtime is required.
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

/// <summary>
/// Tests for the <see cref="FrameworkElement.LayoutTransform"/> DP contract.
/// </summary>
public class LayoutTransformDpTests
{
	[Fact]
	public void Default_is_identity()
	{
		var el = new TextBlock();
		Assert.NotNull(el.LayoutTransform);
		Assert.True(el.LayoutTransform!.Value.IsIdentity);
	}

	[Fact]
	public void Set_and_get_roundtrip()
	{
		var el = new TextBlock();
		var rt = new RotateTransform(45);
		el.LayoutTransform = rt;
		Assert.Same(rt, el.LayoutTransform);
	}

	[Fact]
	public void Null_resets_to_identity()
	{
		var el = new TextBlock();
		el.LayoutTransform = new RotateTransform(30);
		el.LayoutTransform = null;
		Assert.True(el.LayoutTransform!.Value.IsIdentity);
	}

	[Fact]
	public void Set_before_widget_creation_does_not_throw()
	{
		var el = new TextBlock();
		// No widget created; should not throw.
		el.LayoutTransform = new RotateTransform(90);
		Assert.False(el.LayoutTransform!.Value.IsIdentity);
	}
}

// Minimal FrameworkElement for headless layout tests with LayoutTransform.
file sealed class FixedLayoutElement : FrameworkElement
{
	readonly Size _inner;
	public FixedLayoutElement(double w, double h) => _inner = new Size(w, h);
	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException("No GTK in pure tests.");
	protected override Size MeasureOverride(Size _) => _inner;
}

/// <summary>
/// Tests that <see cref="FrameworkElement.LayoutTransform"/> is honoured during
/// <see cref="UIElement.Measure"/> so <see cref="UIElement.DesiredSize"/> reflects
/// the transformed bounding box.
/// </summary>
public class LayoutTransformMeasureTests
{
	const double Eps = 1e-6;

	[Fact]
	public void Identity_desired_size_unchanged()
	{
		var el = new FixedLayoutElement(100, 50);
		el.LayoutTransform = Transform.Identity;
		el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		Assert.Equal(100, el.DesiredSize.Width, Eps);
		Assert.Equal(50, el.DesiredSize.Height, Eps);
	}

	[Fact]
	public void Rotate90_desired_size_is_swapped_bounding_box()
	{
		var el = new FixedLayoutElement(100, 50);
		el.LayoutTransform = new RotateTransform(90);
		el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		// 100×50 rotated 90° → bounding box ~50×100
		Assert.Equal(50, el.DesiredSize.Width, Eps);
		Assert.Equal(100, el.DesiredSize.Height, Eps);
	}

	[Fact]
	public void Scale_2_3_desired_size_is_scaled_bounding_box()
	{
		var el = new FixedLayoutElement(100, 50);
		el.LayoutTransform = new ScaleTransform(2, 3);
		el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		Assert.Equal(200, el.DesiredSize.Width, Eps);
		Assert.Equal(150, el.DesiredSize.Height, Eps);
	}

	[Fact]
	public void Rotate45_desired_size_is_enlarged_bounding_box()
	{
		var el = new FixedLayoutElement(100, 50);
		el.LayoutTransform = new RotateTransform(45);
		el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		double expected = (100 + 50) / Math.Sqrt(2);
		Assert.Equal(expected, el.DesiredSize.Width, 1e-3);
		Assert.Equal(expected, el.DesiredSize.Height, 1e-3);
	}

	[Fact]
	public void Margin_included_in_desired_size_with_transform()
	{
		var el = new FixedLayoutElement(100, 50)
		{
			Margin = new Thickness(10),
			LayoutTransform = new RotateTransform(90)
		};
		el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		// Inner 100×50 → rotate90 → 50×100; then +20 margin each axis → 70×120
		Assert.Equal(70, el.DesiredSize.Width, Eps);
		Assert.Equal(120, el.DesiredSize.Height, Eps);
	}

	[Fact]
	public void Resetting_to_identity_restores_original_desired_size()
	{
		var el = new FixedLayoutElement(100, 50);
		el.LayoutTransform = new RotateTransform(90);
		el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		// Rotated: ~50×100

		el.LayoutTransform = Transform.Identity;
		el.InvalidateMeasure();
		el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		Assert.Equal(100, el.DesiredSize.Width, Eps);
		Assert.Equal(50, el.DesiredSize.Height, Eps);
	}
}
