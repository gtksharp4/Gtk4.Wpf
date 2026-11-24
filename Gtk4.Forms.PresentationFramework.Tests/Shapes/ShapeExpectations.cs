using System;
using System.Collections.Generic;

namespace Gtk4.Forms.PresentationFramework.Tests.Shapes;

/// <summary>Independent (non-Cairo) stroke-bounds arithmetic used to check the implementation.</summary>
static class ShapeExpectations
{
	/// <summary>Max X / max Y of a closed polygon stroked with a miter join (bevel past the limit).</summary>
	public static (double Right, double Bottom) PolygonFarCorner(
		IReadOnlyList<(double X, double Y)> pts, double thickness, double miterLimit)
	{
		double hw = thickness / 2, right = double.MinValue, bottom = double.MinValue;
		int n = pts.Count;
		void Add(double x, double y) { right = Math.Max(right, x); bottom = Math.Max(bottom, y); }
		(double X, double Y) Dir(int a, int b)
		{
			double dx = pts[b].X - pts[a].X, dy = pts[b].Y - pts[a].Y, len = Math.Sqrt(dx * dx + dy * dy);
			return (dx / len, dy / len);
		}
		for (int i = 0; i < n; i++)
		{
			int j = (i + 1) % n, h = (i + n - 1) % n;
			var d = Dir(i, j);
			var nl = (X: -d.Y, Y: d.X);
			foreach (int k in new[] { i, j })
			{
				Add(pts[k].X + hw * nl.X, pts[k].Y + hw * nl.Y);
				Add(pts[k].X - hw * nl.X, pts[k].Y - hw * nl.Y);
			}
			// Join at vertex i between edge h->i and edge i->j.
			var d1 = Dir(h, i);
			double cross = d1.X * d.Y - d1.Y * d.X, dot = d1.X * d.X + d1.Y * d.Y;
			if (Math.Abs(cross) < 1e-12) continue;
			double side = cross > 0 ? -1 : 1; // outer side of the turn
			var n1 = (X: side * -d1.Y, Y: side * d1.X);
			var n2 = (X: side * -d.Y, Y: side * d.X);
			double sinHalf = Math.Sqrt((1 + dot) / 2); // sin(interior angle / 2) = cos(turn / 2)
			if (1 / sinHalf > miterLimit) continue;
			double k2 = hw / (1 + n1.X * n2.X + n1.Y * n2.Y);
			Add(pts[i].X + (n1.X + n2.X) * k2, pts[i].Y + (n1.Y + n2.Y) * k2);
		}
		return (right, bottom);
	}
}
