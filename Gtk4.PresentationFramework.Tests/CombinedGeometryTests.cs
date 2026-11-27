using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>CombinedGeometry fills the union, intersection, symmetric difference or difference of its two shapes.</summary>
public class CombinedGeometryTests : WpfGtkTestBase
{
	const int Size = 100;

	static CombinedGeometry Combine(GeometryCombineMode mode) => new CombinedGeometry(mode,
		new EllipseGeometry(new Point(30, 50), 25, 25), new EllipseGeometry(new Point(70, 50), 25, 25));

	// Whether (left-only, overlap, right-only, outside) are painted.
	static bool[] Painted(Geometry geometry)
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, Size, Size);
		using (var cr = new Cairo.Context(surface))
		{
			cr.Save();
			geometry.BuildTransformedPath(cr);
			cr.SetSourceRGB(1, 0, 0);
			cr.Fill();
			cr.Restore();
		}
		surface.Flush();
		byte[] data = surface.Data;
		bool At(int x, int y) => data[y * surface.Stride + x * 4 + 3] == 255;
		return new[] { At(15, 50), At(50, 50), At(85, 50), At(50, 5) };
	}

	[Theory]
	[InlineData(GeometryCombineMode.Union, new[] { true, true, true, false })]
	[InlineData(GeometryCombineMode.Intersect, new[] { false, true, false, false })]
	[InlineData(GeometryCombineMode.Xor, new[] { true, false, true, false })]
	[InlineData(GeometryCombineMode.Exclude, new[] { true, false, false, false })]
	public void Each_mode_fills_its_region(GeometryCombineMode mode, bool[] expected) => Run(() =>
		Assert.Equal(expected, Painted(Combine(mode))));

	[Fact]
	public void The_clip_of_Intersect_stays_inside_the_callers_Save_scope() => Run(() =>
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, Size, Size);
		using var cr = new Cairo.Context(surface);
		cr.Save();
		Combine(GeometryCombineMode.Intersect).BuildTransformedPath(cr);
		cr.Restore();
		cr.NewPath();
		cr.Rectangle(0, 0, Size, Size);
		cr.SetSourceRGB(1, 0, 0);
		cr.Fill();
		surface.Flush();
		Assert.Equal(255, surface.Data[5 * surface.Stride + 5 * 4 + 3]);
	});

	[Fact]
	public void Bounds_follow_the_mode() => Run(() =>
	{
		Assert.Equal(new Rect(5, 25, 90, 50), Combine(GeometryCombineMode.Union).Bounds);
		Assert.Equal(new Rect(45, 25, 10, 50), Combine(GeometryCombineMode.Intersect).Bounds);
		Assert.Equal(new Rect(5, 25, 50, 50), Combine(GeometryCombineMode.Exclude).Bounds);
	});

	[Fact]
	public void Changing_a_child_raises_Changed() => Run(() =>
	{
		var first = new EllipseGeometry(new Point(30, 50), 25, 25);
		var combined = new CombinedGeometry(GeometryCombineMode.Union, first, new EllipseGeometry());
		int raised = 0;
		combined.Changed += (s, e) => raised++;

		first.RadiusX = 10;

		Assert.True(raised > 0);
	});

	[Fact]
	public void The_default_mode_is_Union_and_geometries_are_null() => Run(() =>
	{
		var combined = new CombinedGeometry();
		Assert.Equal(GeometryCombineMode.Union, combined.GeometryCombineMode);
		Assert.Null(combined.Geometry1);
		Assert.Equal(new Rect(0, 0, 0, 0), combined.Bounds);
	});
}
