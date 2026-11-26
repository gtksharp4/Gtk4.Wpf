using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure tests for Transform matrix math: TranslateTransform, ScaleTransform,
/// RotateTransform, MatrixTransform, and matrix composition.
/// </summary>
public class TransformTests
{
	const double Eps = 1e-9;

	// ── Matrix.Identity ──────────────────────────────────────────────────────

	[Fact]
	public void Matrix_Identity_is_identity()
	{
		var m = Matrix.Identity;
		Assert.Equal(1, m.M11); Assert.Equal(0, m.M12);
		Assert.Equal(0, m.M21); Assert.Equal(1, m.M22);
		Assert.Equal(0, m.OffsetX); Assert.Equal(0, m.OffsetY);
		Assert.True(m.IsIdentity);
	}

	// ── TranslateTransform ───────────────────────────────────────────────────

	[Fact]
	public void TranslateTransform_produces_translation_matrix()
	{
		var t = new TranslateTransform(30, 50);
		var m = t.Value;
		Assert.Equal(1, m.M11);
		Assert.Equal(0, m.M12);
		Assert.Equal(0, m.M21);
		Assert.Equal(1, m.M22);
		Assert.Equal(30, m.OffsetX);
		Assert.Equal(50, m.OffsetY);
	}

	[Fact]
	public void TranslateTransform_moves_a_point()
	{
		var t = new TranslateTransform(10, 20);
		var p = t.Value.Transform(new Point(5, 5));
		Assert.Equal(15, p.X, Eps);
		Assert.Equal(25, p.Y, Eps);
	}

	// ── ScaleTransform ───────────────────────────────────────────────────────

	[Fact]
	public void ScaleTransform_produces_scale_matrix()
	{
		var s = new ScaleTransform(2, 3);
		var m = s.Value;
		Assert.Equal(2, m.M11);
		Assert.Equal(3, m.M22);
		Assert.Equal(0, m.M12);
		Assert.Equal(0, m.M21);
		Assert.Equal(0, m.OffsetX, Eps);
		Assert.Equal(0, m.OffsetY, Eps);
	}

	[Fact]
	public void ScaleTransform_doubles_a_point()
	{
		var s = new ScaleTransform(2, 2);
		var p = s.Value.Transform(new Point(5, 7));
		Assert.Equal(10, p.X, Eps);
		Assert.Equal(14, p.Y, Eps);
	}

	[Fact]
	public void ScaleTransform_with_center_keeps_center_fixed()
	{
		var s = new ScaleTransform(2, 2, 10, 10);
		var p = s.Value.Transform(new Point(10, 10));
		Assert.Equal(10, p.X, Eps);
		Assert.Equal(10, p.Y, Eps);
	}

	// ── RotateTransform ──────────────────────────────────────────────────────

	[Fact]
	public void RotateTransform_90_degrees_maps_x_axis_to_y_axis()
	{
		var r = new RotateTransform(90);
		var p = r.Value.Transform(new Point(1, 0));
		Assert.Equal(0, p.X, 1e-6);
		Assert.Equal(1, p.Y, 1e-6);
	}

	[Fact]
	public void RotateTransform_0_degrees_is_identity()
	{
		var r = new RotateTransform(0);
		Assert.True(r.Value.IsIdentity);
	}

	[Fact]
	public void RotateTransform_360_degrees_returns_to_start()
	{
		var r = new RotateTransform(360);
		var p = r.Value.Transform(new Point(5, 3));
		Assert.Equal(5, p.X, 1e-6);
		Assert.Equal(3, p.Y, 1e-6);
	}

	// ── MatrixTransform ───────────────────────────────────────────────────────

	[Fact]
	public void MatrixTransform_Value_returns_stored_matrix()
	{
		var m = new Matrix(2, 0, 0, 3, 10, 20);
		var t = new MatrixTransform(m);
		Assert.Equal(m, t.Value);
	}

	// ── Matrix.Multiply (composition) ────────────────────────────────────────

	[Fact]
	public void Multiply_translate_then_scale_composes_correctly()
	{
		// translate (10,0) then scale (2,2)
		var translate = new Matrix(1, 0, 0, 1, 10, 0);
		var scale = new Matrix(2, 0, 0, 2, 0, 0);
		var composed = translate.Multiply(scale);
		// point (0,0) → translate → (10,0) → scale → (20,0)
		var p = composed.Transform(new Point(0, 0));
		Assert.Equal(20, p.X, Eps);
		Assert.Equal(0, p.Y, Eps);
	}

	[Fact]
	public void Matrix_equality_works()
	{
		var a = new Matrix(1, 2, 3, 4, 5, 6);
		var b = new Matrix(1, 2, 3, 4, 5, 6);
		Assert.Equal(a, b);
		Assert.True(a == b);
	}

	[Fact]
	public void Matrix_inequality_works()
	{
		var a = new Matrix(1, 0, 0, 1, 0, 0);
		var b = new Matrix(2, 0, 0, 1, 0, 0);
		Assert.NotEqual(a, b);
		Assert.True(a != b);
	}
}