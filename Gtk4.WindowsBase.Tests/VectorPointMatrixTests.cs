using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class VectorPointMatrixTests
{
	[Fact]
	public void VectorLengthAndNormalize()
	{
		var v = new Vector(3, 4);
		Assert.Equal(5, v.Length);
		Assert.Equal(25, v.LengthSquared);
		v.Normalize();
		Assert.Equal(0.6, v.X, 10);
		Assert.Equal(0.8, v.Y, 10);
	}

	[Fact]
	public void VectorNormalizeLargeDoesNotOverflow()
	{
		var v = new Vector(1e200, 1e200);
		v.Normalize();
		Assert.Equal(1, v.Length, 10);
	}

	[Fact]
	public void VectorAngleBetweenIsSignedDegrees()
	{
		Assert.Equal(90, Vector.AngleBetween(new Vector(1, 0), new Vector(0, 1)), 10);
		Assert.Equal(-90, Vector.AngleBetween(new Vector(0, 1), new Vector(1, 0)), 10);
		Assert.Equal(1, Vector.CrossProduct(new Vector(1, 0), new Vector(0, 1)));
		Assert.Equal(-1, Vector.Determinant(new Vector(0, 1), new Vector(1, 0)));
	}

	[Fact]
	public void VectorOperators()
	{
		var a = new Vector(1, 2);
		var b = new Vector(3, 5);
		Assert.Equal(new Vector(4, 7), a + b);
		Assert.Equal(new Vector(-2, -3), a - b);
		Assert.Equal(new Vector(2, 4), a * 2);
		Assert.Equal(new Vector(2, 4), 2 * a);
		Assert.Equal(new Vector(0.5, 1), a / 2);
		Assert.Equal(13, a * b);
		Assert.Equal(new Vector(-1, -2), -a);
		Assert.Equal(new Point(11, 22), a + new Point(10, 20));
		Assert.True(a != b);
		var n = a;
		n.Negate();
		Assert.Equal(new Vector(-1, -2), n);
	}

	[Fact]
	public void VectorMatrixIgnoresTranslation()
	{
		var m = new Matrix(2, 0, 0, 3, 100, 100);
		Assert.Equal(new Vector(2, 3), new Vector(1, 1) * m);
	}

	[Fact]
	public void VectorConversions()
	{
		Assert.Equal(new Size(1, 2), (Size)new Vector(-1, 2));
		Assert.Equal(new Point(1, 2), (Point)new Vector(1, 2));
		Assert.Equal(new Vector(1, 2), (Vector)new Point(1, 2));
		Assert.Equal(new Size(3, 4), (Size)new Point(3, -4));
	}

	[Fact]
	public void VectorEqualityTreatsNaNAsEqualInEquals()
	{
		var a = new Vector(double.NaN, 1);
		Assert.True(a.Equals(a));
		var a2 = a;
		Assert.False(a == a2);
		Assert.Equal(a.GetHashCode(), new Vector(double.NaN, 1).GetHashCode());
	}

	[Theory]
	[InlineData("1,2", 1, 2)]
	[InlineData("1 2", 1, 2)]
	[InlineData(" 1.5 , -2.5 ", 1.5, -2.5)]
	public void VectorAndPointParse(string s, double x, double y)
	{
		Assert.Equal(new Vector(x, y), Vector.Parse(s));
		Assert.Equal(new Point(x, y), Point.Parse(s));
	}

	[Fact]
	public void ParseErrorsMatchWpf()
	{
		Assert.Throws<InvalidOperationException>(() => Point.Parse("1"));
		Assert.Throws<InvalidOperationException>(() => Point.Parse("1,2,3"));
		Assert.Throws<FormatException>(() => Point.Parse("a,2"));
		Assert.Throws<InvalidOperationException>(() => Point.Parse("1,2,"));
		Assert.Throws<InvalidOperationException>(() => Point.Parse("1,,2"));
		Assert.Throws<InvalidOperationException>(() => Vector.Parse("1,,2"));
		Assert.Throws<InvalidOperationException>(() => Matrix.Parse("1,0,0,1,0,0,"));
		Assert.Throws<InvalidOperationException>(() => Vector.Parse(""));
		Assert.Throws<ArgumentNullException>(() => Vector.Parse(null!));
	}

	[Fact]
	public void ToStringUsesCultureListSeparator()
	{
		var de = new CultureInfo("de-DE");
		Assert.Equal("1,5;2", new Vector(1.5, 2).ToString(de));
		Assert.Equal("1,5;2", new Point(1.5, 2).ToString(de));
		Assert.Equal("1.5,2", new Vector(1.5, 2).ToString(CultureInfo.InvariantCulture));
		Assert.Equal("1.50,2.00", new Vector(1.5, 2).ToString("F2", CultureInfo.InvariantCulture));
	}

	[Fact]
	public void PointVectorArithmetic()
	{
		var p = new Point(1, 2);
		p.Offset(2, 3);
		Assert.Equal(new Point(3, 5), p);
		Assert.Equal(new Point(4, 7), p + new Vector(1, 2));
		Assert.Equal(new Point(2, 3), p - new Vector(1, 2));
		Assert.Equal(new Vector(2, 3), p - new Point(1, 2));
		Assert.Equal(new Point(4, 7), Point.Add(p, new Vector(1, 2)));
		Assert.Equal(new Vector(2, 3), Point.Subtract(p, new Point(1, 2)));
		Assert.Equal(new Point(2, 3), Point.Subtract(p, new Vector(1, 2)));
	}

	[Fact]
	public void PointMatrixMultiply()
	{
		var m = new Matrix(1, 0, 0, 1, 10, 20);
		Assert.Equal(new Point(11, 22), new Point(1, 2) * m);
		Assert.Equal(new Point(11, 22), Point.Multiply(new Point(1, 2), m));
	}
}
