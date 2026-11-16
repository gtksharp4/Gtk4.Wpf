using System;
using System.Windows.Media;

namespace System.Windows.Shapes
{
	/// <summary>
	/// Draws a closed polygon defined by a collection of points.
	/// Mirrors <c>System.Windows.Shapes.Polygon</c>.
	/// </summary>
	public class Polygon : Shape
	{
		/// <summary>The vertices of the polygon.</summary>
		public PointCollection Points { get; set; } = new PointCollection();

		/// <summary>Fill rule applied when filling the polygon.</summary>
		public FillRule FillRule { get; set; } = FillRule.EvenOdd;

		/// <inheritdoc/>
		protected override void BuildShapePath(Cairo.Context cr, int width, int height)
		{
			if (Points == null || Points.Count < 2) return;
			cr.FillRule = FillRule == FillRule.Nonzero
				? Cairo.FillRule.Winding
				: Cairo.FillRule.EvenOdd;
			cr.MoveTo(Points[0].X, Points[0].Y);
			for (int i = 1; i < Points.Count; i++)
				cr.LineTo(Points[i].X, Points[i].Y);
			cr.ClosePath();
		}
	}

	/// <summary>
	/// Draws an open polyline through a collection of points.
	/// Mirrors <c>System.Windows.Shapes.Polyline</c>.
	/// </summary>
	public class Polyline : Shape
	{
		/// <summary>The vertices of the polyline.</summary>
		public PointCollection Points { get; set; } = new PointCollection();

		/// <summary>Fill rule applied when filling the polyline.</summary>
		public FillRule FillRule { get; set; } = FillRule.EvenOdd;

		/// <inheritdoc/>
		protected override void BuildShapePath(Cairo.Context cr, int width, int height)
		{
			if (Points == null || Points.Count < 2) return;
			cr.FillRule = FillRule == FillRule.Nonzero
				? Cairo.FillRule.Winding
				: Cairo.FillRule.EvenOdd;
			cr.MoveTo(Points[0].X, Points[0].Y);
			for (int i = 1; i < Points.Count; i++)
				cr.LineTo(Points[i].X, Points[i].Y);
		}
	}

	/// <summary>WPF fill-rule enumeration.</summary>
	public enum FillRule
	{
		/// <summary>Even-odd fill rule.</summary>
		EvenOdd,
		/// <summary>Non-zero winding fill rule.</summary>
		Nonzero,
	}
}
