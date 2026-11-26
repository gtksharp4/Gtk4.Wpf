using System;
using System.Globalization;
using System.Windows.Media.Media3D;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media3D;

public partial class Media3DMathTests
{
	[Fact]
	public void Point_and_vector_operators_follow_wpf()
	{
		var a = new Point3D(5, 7, 9);
		var b = new Point3D(1, 2, 3);
		Vector3D d = a - b;
		Assert.Equal(new Vector3D(4, 5, 6), d);
		Assert.Equal(new Point3D(2, 4, 6), b + new Vector3D(1, 2, 3));
		Assert.Equal(new Point3D(4, 5, 6), a - new Vector3D(1, 2, 3));
		Assert.Equal(new Point3D(11, 22, 33), new Point3D(1, 2, 3) * Translation(10, 20, 30));
	}

	static Matrix3D Translation(double x, double y, double z)
	{
		var m = Matrix3D.Identity;
		m.Translate(new Vector3D(x, y, z));
		return m;
	}

	[Fact]
	public void Vector_length_normalize_dot_cross_and_angle()
	{
		var v = new Vector3D(3, 0, 4);
		Assert.Equal(5.0, v.Length, 12);
		Assert.Equal(25.0, v.LengthSquared, 12);
		v.Normalize();
		Assert.Equal(1.0, v.Length, 12);
		Assert.Equal(0.0, Vector3D.DotProduct(new Vector3D(1, 0, 0), new Vector3D(0, 1, 0)));
		Assert.Equal(new Vector3D(0, 0, 1), Vector3D.CrossProduct(new Vector3D(1, 0, 0), new Vector3D(0, 1, 0)));
		Assert.Equal(90.0, Vector3D.AngleBetween(new Vector3D(1, 0, 0), new Vector3D(0, 5, 0)), 9);
		Assert.Equal(180.0, Vector3D.AngleBetween(new Vector3D(1, 0, 0), new Vector3D(-2, 0, 0)), 9);
	}

	[Fact]
	public void Size3D_rejects_negative_components_and_empty_is_negative_infinity()
	{
		Assert.Throws<ArgumentException>(() => new Size3D(-1, 0, 0));
		Assert.True(Size3D.Empty.IsEmpty);
		Assert.Equal(double.NegativeInfinity, Size3D.Empty.X);
		Assert.Throws<InvalidOperationException>(() =>
		{
			var s = Size3D.Empty;
			s.X = 1;
		});
		Assert.False(new Size3D(0, 0, 0).IsEmpty);
	}

	[Fact]
	public void Rect3D_empty_union_contains_and_intersect()
	{
		Assert.True(Rect3D.Empty.IsEmpty);
		var box = new Rect3D(0, 0, 0, 2, 2, 2);
		Assert.True(box.Contains(new Point3D(1, 1, 1)));
		Assert.False(box.Contains(new Point3D(3, 1, 1)));
		var union = Rect3D.Union(box, new Point3D(5, 0, 0));
		Assert.Equal(5.0, union.SizeX);
		Assert.Equal(new Rect3D(1, 1, 1, 1, 1, 1), Rect3D.Intersect(box, new Rect3D(1, 1, 1, 4, 4, 4)));
		Assert.True(Rect3D.Intersect(box, new Rect3D(9, 9, 9, 1, 1, 1)).IsEmpty);
		Assert.Throws<ArgumentException>(() => new Rect3D(0, 0, 0, -1, 0, 0));
	}

	[Fact]
	public void Default_quaternion_and_matrix_are_identity()
	{
		Assert.True(default(Quaternion).IsIdentity);
		Assert.Equal(1.0, default(Quaternion).W);
		Assert.True(default(Quaternion).IsNormalized);
		Assert.True(default(Matrix3D).IsIdentity);
		Assert.Equal(1.0, default(Matrix3D).M33);
		Assert.Equal(Matrix3D.Identity, default(Matrix3D));
	}

	[Fact]
	public void Quaternion_axis_angle_round_trips_and_zero_axis_throws()
	{
		var q = new Quaternion(new Vector3D(0, 0, 2), 90);
		Assert.Equal(90.0, q.Angle, 9);
		Assert.Equal(new Vector3D(0, 0, 1), q.Axis);
		Assert.True(q.IsNormalized);
		Assert.Throws<InvalidOperationException>(() => new Quaternion(new Vector3D(0, 0, 0), 10));
	}

	[Fact]
	public void Quaternion_multiply_conjugate_invert_and_slerp()
	{
		var quarter = new Quaternion(new Vector3D(0, 1, 0), 90);
		var half = quarter * quarter;
		Assert.Equal(180.0, half.Angle, 9);
		var inverse = quarter;
		inverse.Invert();
		Assert.True((quarter * inverse).IsIdentity || Math.Abs((quarter * inverse).W - 1) < 1e-12);
		var mid = Quaternion.Slerp(Quaternion.Identity, quarter, 0.5);
		Assert.Equal(45.0, mid.Angle, 9);
		Assert.Equal(Quaternion.Identity, Quaternion.Slerp(Quaternion.Identity, quarter, 0));
		var scaled = new Quaternion(0, 0, 0, 4);
		scaled.Normalize();
		Assert.Equal(1.0, scaled.W, 12);
	}

	[Fact]
	public void Matrix_rotation_scale_and_translation_compose_in_order()
	{
		var m = Matrix3D.Identity;
		m.Rotate(new Quaternion(new Vector3D(0, 1, 0), 90));
		var p = m.Transform(new Point3D(1, 0, 0));
		Assert.Equal(0.0, p.X, 9);
		Assert.Equal(-1.0, p.Z, 9);

		var s = Matrix3D.Identity;
		s.Scale(new Vector3D(2, 2, 2));
		s.Translate(new Vector3D(1, 0, 0));
		Assert.Equal(new Point3D(3, 2, 2), s.Transform(new Point3D(1, 1, 1)));

		var around = Matrix3D.Identity;
		around.ScaleAt(new Vector3D(2, 2, 2), new Point3D(1, 1, 1));
		Assert.Equal(new Point3D(1, 1, 1), around.Transform(new Point3D(1, 1, 1)));
		Assert.Equal(new Vector3D(2, 4, 6), s.Transform(new Vector3D(1, 2, 3)));
	}

	[Fact]
	public void Matrix_invert_determinant_and_non_invertible_throws()
	{
		var m = Matrix3D.Identity;
		m.Scale(new Vector3D(2, 4, 8));
		m.Translate(new Vector3D(1, 2, 3));
		Assert.Equal(64.0, m.Determinant, 9);
		Assert.True(m.IsAffine);
		var inv = m;
		inv.Invert();
		var back = (new Point3D(1, 1, 1) * m) * inv;
		Assert.Equal(1.0, back.X, 9);
		Assert.Equal(1.0, back.Y, 9);
		Assert.Equal(1.0, back.Z, 9);

		var flat = new Matrix3D(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
		Assert.False(flat.HasInverse);
		Assert.Throws<InvalidOperationException>(() => flat.Invert());
	}

	[Fact]
	public void Matrix_prepend_and_append_differ_and_copies_do_not_alias()
	{
		var t = Translation(1, 0, 0);
		var s = Matrix3D.Identity;
		s.Scale(new Vector3D(2, 2, 2));
		var appended = t;
		appended.Append(s);
		var prepended = t;
		prepended.Prepend(s);
		Assert.Equal(new Point3D(2, 0, 0), appended.Transform(new Point3D(0, 0, 0)));
		Assert.Equal(new Point3D(1, 0, 0), prepended.Transform(new Point3D(0, 0, 0)));

		var copy = t;
		copy.OffsetX = 9;
		Assert.Equal(1.0, t.OffsetX);
	}
}
