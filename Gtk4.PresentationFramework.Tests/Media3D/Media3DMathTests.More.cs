using System;
using System.Globalization;
using System.Windows.Media.Media3D;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media3D;

public partial class Media3DMathTests
{
	[Fact]
	public void Formatting_uses_the_culture_list_separator_and_parse_is_invariant()
	{
		var p = new Point3D(10, 20, 30);
		Assert.Equal("10,20,30", p.ToString(CultureInfo.GetCultureInfo("en-US")));
		Assert.Equal("10;20;30", p.ToString(CultureInfo.GetCultureInfo("de-DE")));
		Assert.Equal(new Point3D(1.5, 2, 3), Point3D.Parse("1.5, 2 3"));
		Assert.Equal(Matrix3D.Identity, Matrix3D.Parse("Identity"));
		Assert.Equal("Identity", Matrix3D.Identity.ToString());
		Assert.Equal(Quaternion.Identity, Quaternion.Parse("Identity"));
		Assert.Equal(new Rect3D(1, 2, 3, 4, 5, 6), Rect3D.Parse(new Rect3D(1, 2, 3, 4, 5, 6).ToString()));
		Assert.True(Size3D.Parse("Empty").IsEmpty);
		Assert.Throws<InvalidOperationException>(() => Point3D.Parse("1,2"));
	}

	[Fact]
	public void Equality_and_hash_codes_are_consistent()
	{
		Assert.True(new Point3D(1, 2, 3) == new Point3D(1, 2, 3));
		Assert.True(new Vector3D(1, 2, 3) != new Vector3D(1, 2, 4));
		Assert.Equal(new Point4D(1, 2, 3, 4).GetHashCode(), new Point4D(1, 2, 3, 4).GetHashCode());
		Assert.Equal(default(Matrix3D).GetHashCode(), Matrix3D.Identity.GetHashCode());
		Assert.Equal(default(Quaternion), Quaternion.Identity);
		Assert.False(new Point3D(1, 2, 3).Equals("x"));
	}

	[Fact]
	public void Point4D_and_non_affine_transform_divide_by_w()
	{
		var p4 = new Point4D(1, 2, 3, 1) * Translation(1, 1, 1);
		Assert.Equal(new Point4D(2, 3, 4, 1), p4);
		var persp = new Matrix3D(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0.5, 0, 0, 0, 1);
		Assert.False(persp.IsAffine);
		var p = persp.Transform(new Point3D(2, 2, 2));
		Assert.Equal(1.0, p.X, 9);
		Assert.Equal(1.0, p.Z, 9);
	}
}
