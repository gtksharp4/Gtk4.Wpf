using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>
/// Edge cases for the Path markup parser: the current point after <c>Z</c> (B2) and the
/// best-effort skip of unsupported commands keeping the token stream in sync (B3).
/// </summary>
public class PathMarkupParserEdgeTests
{
	[Fact]
	public void Relative_command_after_Z_accumulates_from_figure_start()
	{
		// After Z the current point returns to the closed figure's start (10,10),
		// so the relative l5,0 resolves to (15,10) — NOT (25,20) from the last point.
		var pg = (PathGeometry)Geometry.Parse("M10,10 L20,20 Z l5,0");

		Assert.Equal(2, pg.Figures.Count);
		var seg = Assert.IsType<LineSegment>(pg.Figures[1].Segments[0]);
		Assert.Equal(new Point(15, 10), seg.Point);
	}

	[Fact]
	public void Arc_command_is_skipped_and_following_line_still_parses()
	{
		// A consumes exactly 7 params (rx ry rot large sweep x y); the trailing L must
		// still resolve correctly, proving the skip did not desync the token stream.
		var pg = (PathGeometry)Geometry.Parse("M0,0 A5,5 0 0 1 10,10 L20,20");

		var seg = Assert.IsType<LineSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(new Point(20, 20), seg.Point);
	}

	[Fact]
	public void Smooth_cubic_S_is_skipped_and_following_line_still_parses()
	{
		// S consumes 4 params; the trailing L must still resolve.
		var pg = (PathGeometry)Geometry.Parse("M0,0 S1,1 2,2 L3,3");

		var seg = Assert.IsType<LineSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(new Point(3, 3), seg.Point);
	}

	[Fact]
	public void Smooth_quadratic_T_is_skipped_and_following_line_still_parses()
	{
		// T consumes 2 params; the trailing L must still resolve.
		var pg = (PathGeometry)Geometry.Parse("M0,0 T1,1 L4,4");

		var seg = Assert.IsType<LineSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(new Point(4, 4), seg.Point);
	}
}
