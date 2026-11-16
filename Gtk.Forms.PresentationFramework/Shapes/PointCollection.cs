using System.Collections.Generic;

namespace System.Windows.Shapes
{
	/// <summary>
	/// An ordered list of <see cref="System.Windows.Point"/> values used by
	/// <see cref="Polygon"/> and <see cref="Polyline"/>.
	/// Mirrors <c>System.Windows.Media.PointCollection</c>.
	/// </summary>
	public class PointCollection : List<System.Windows.Point>
	{
		/// <summary>Creates an empty collection.</summary>
		public PointCollection() { }

		/// <summary>Creates a collection pre-populated with <paramref name="points"/>.</summary>
		public PointCollection(IEnumerable<System.Windows.Point> points) : base(points) { }
	}
}
