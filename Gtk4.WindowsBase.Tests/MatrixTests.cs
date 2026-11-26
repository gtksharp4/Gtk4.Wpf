using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class MatrixTests
{
	[Fact]
	public void DefaultMatrixIsIdentity()
	{
		Matrix m = default;
		Assert.True(m.IsIdentity);
		Assert.Equal(1, m.M11);
		Assert.Equal(Matrix.Identity, m);
		Assert.Equal("Identity", m.ToString());
		m.OffsetX = 5;
		Assert.Equal(1, m.M22);
		Assert.False(m.IsIdentity);
	}

	[Fact]
	public void MatrixInvertAndDeterminant()
	{
		var m = new Matrix(2, 0, 0, 4, 6, 8);
		Assert.Equal(8, m.Determinant);
		Assert.True(m.HasInverse);
		var inv = m;
		inv.Invert();
		Assert.True((m * inv).IsIdentity);
		var singular = new Matrix(1, 2, 2, 4, 0, 0);
		Assert.False(singular.HasInverse);
		Assert.Throws<InvalidOperationException>(() => singular.Invert());
	}

	[Fact]
	public void MatrixAppendVersusPrepend()
	{
		var a = Matrix.Identity;
		a.Scale(2, 2);
		a.Translate(10, 0);
		Assert.Equal(new Point(12, 2), a.Transform(new Point(1, 1)));
		var b = Matrix.Identity;
		b.Scale(2, 2);
		b.TranslatePrepend(10, 0);
		Assert.Equal(new Point(22, 2), b.Transform(new Point(1, 1)));
	}

	[Fact]
	public void MatrixRotateAndSkew()
	{
		var m = Matrix.Identity;
		m.Rotate(90);
		var p = m.Transform(new Point(1, 0));
		Assert.Equal(0, p.X, 10);
		Assert.Equal(1, p.Y, 10);
		var at = Matrix.Identity;
		at.RotateAt(180, 1, 1);
		var q = at.Transform(new Point(0, 0));
		Assert.Equal(2, q.X, 10);
		Assert.Equal(2, q.Y, 10);
		var s = Matrix.Identity;
		s.Skew(45, 0);
		Assert.Equal(1, s.M21, 10);
		var sa = Matrix.Identity;
		sa.ScaleAt(2, 2, 1, 1);
		Assert.Equal(new Point(1, 1), sa.Transform(new Point(1, 1)));
	}

	[Fact]
	public void MatrixParseAndToString()
	{
		Assert.Equal(Matrix.Identity, Matrix.Parse("Identity"));
		var m = Matrix.Parse("1,2,3,4,5,6");
		Assert.Equal(new Matrix(1, 2, 3, 4, 5, 6), m);
		Assert.Equal("1,2,3,4,5,6", m.ToString(CultureInfo.InvariantCulture));
		Assert.Throws<InvalidOperationException>(() => Matrix.Parse("1,2,3"));
	}

	[Fact]
	public void MatrixTransformsArraysInPlace()
	{
		var pts = new[] { new Point(1, 1), new Point(2, 2) };
		new Matrix(1, 0, 0, 1, 1, 1).Transform(pts);
		Assert.Equal(new Point(3, 3), pts[1]);
	}
}
