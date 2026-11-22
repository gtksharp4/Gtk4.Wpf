using System.Windows.Shapes;

namespace System.Windows.Media;

public partial class PathGeometry
{
	/// <summary>How overlapping figures are filled. Default <see cref="Shapes.FillRule.EvenOdd"/>.</summary>
	public FillRule FillRule { get; set; } = FillRule.EvenOdd;
}
