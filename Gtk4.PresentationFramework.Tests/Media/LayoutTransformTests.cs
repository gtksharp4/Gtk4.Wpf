using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

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
