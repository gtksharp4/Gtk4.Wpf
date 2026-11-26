using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure tests for <see cref="SkewTransform"/>, <see cref="TransformGroup"/>,
/// and <see cref="TransformCollection"/> matrix math.
/// No GTK runtime required.
/// </summary>
public class SkewAndGroupTransformTests
{
	const double LooseEps = 1e-6;
	const double Eps = 1e-9;

	// ── SkewTransform ─────────────────────────────────────────────────────────

	[Fact]
	public void SkewTransform_zero_angles_is_identity()
	{
		var s = new SkewTransform(0, 0);
		Assert.True(s.Value.IsIdentity);
	}

	[Fact]
	public void SkewTransform_AngleX_45_shears_y_on_x_axis()
	{
		// AngleX=45: tan(45°)=1 → point (0,1) becomes (1,1)
		var s = new SkewTransform(45, 0);
		var p = s.Value.Transform(new Point(0, 1));
		Assert.Equal(1, p.X, LooseEps);
		Assert.Equal(1, p.Y, LooseEps);
	}

	[Fact]
	public void SkewTransform_AngleY_45_shears_x_on_y_axis()
	{
		// AngleY=45: tan(45°)=1 → point (1,0) becomes (1,1)
		var s = new SkewTransform(0, 45);
		var p = s.Value.Transform(new Point(1, 0));
		Assert.Equal(1, p.X, LooseEps);
		Assert.Equal(1, p.Y, LooseEps);
	}

	[Fact]
	public void SkewTransform_with_center_keeps_center_fixed()
	{
		var s = new SkewTransform(45, 0, centerX: 5, centerY: 5);
		var p = s.Value.Transform(new Point(5, 5));
		Assert.Equal(5, p.X, LooseEps);
		Assert.Equal(5, p.Y, LooseEps);
	}

	// ── TransformGroup ────────────────────────────────────────────────────────

	[Fact]
	public void TransformGroup_empty_is_identity()
	{
		var g = new TransformGroup();
		Assert.True(g.Value.IsIdentity);
	}

	[Fact]
	public void TransformGroup_single_child_matches_child_value()
	{
		var g = new TransformGroup();
		g.Children.Add(new TranslateTransform(10, 20));
		var m = g.Value;
		Assert.Equal(10, m.OffsetX, Eps);
		Assert.Equal(20, m.OffsetY, Eps);
	}

	[Fact]
	public void TransformGroup_translate_then_scale_composes_first_applied_first()
	{
		// Translate(10,0) then Scale(2,2): (0,0)→(10,0)→(20,0)
		var g = new TransformGroup();
		g.Children.Add(new TranslateTransform(10, 0));
		g.Children.Add(new ScaleTransform(2, 2));
		var p = g.Value.Transform(new Point(0, 0));
		Assert.Equal(20, p.X, Eps);
		Assert.Equal(0, p.Y, Eps);
	}

	[Fact]
	public void TransformGroup_rotate90_then_translate_places_point_correctly()
	{
		// Rotate 90°: (1,0)→(0,1). Translate(0,5): →(0,6)
		var g = new TransformGroup();
		g.Children.Add(new RotateTransform(90));
		g.Children.Add(new TranslateTransform(0, 5));
		var p = g.Value.Transform(new Point(1, 0));
		Assert.Equal(0, p.X, LooseEps);
		Assert.Equal(6, p.Y, LooseEps);
	}

	[Fact]
	public void TransformGroup_two_translations_add_up()
	{
		var g = new TransformGroup();
		g.Children.Add(new TranslateTransform(3, 4));
		g.Children.Add(new TranslateTransform(7, 6));
		var m = g.Value;
		Assert.Equal(10, m.OffsetX, Eps);
		Assert.Equal(10, m.OffsetY, Eps);
	}

	// ── TransformCollection ───────────────────────────────────────────────────

	[Fact]
	public void TransformCollection_can_hold_multiple_transforms()
	{
		var col = new TransformCollection
		{
			new TranslateTransform(1, 2),
			new ScaleTransform(3, 4)
		};
		Assert.Equal(2, col.Count);
	}
}
