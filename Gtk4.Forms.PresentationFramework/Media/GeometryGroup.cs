using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace System.Windows.Media;

/// <summary>
/// An ordered, mutable collection of <see cref="Geometry"/> objects.
/// Mirrors <c>System.Windows.Media.GeometryCollection</c>.
/// </summary>
public class GeometryCollection : Collection<Geometry> { }

/// <summary>
/// A geometry that combines its <see cref="Children"/> into one path.
/// Mirrors <c>System.Windows.Media.GeometryGroup</c>.
/// </summary>
/// <remarks>
/// <b>Rendering gap:</b> the children paths are appended to one Cairo path filled with
/// <see cref="FillRule"/>; the outermost group's rule applies. <c>Geometry.Transform</c> of the group and of the
/// children is not applied.
/// </remarks>
[ContentProperty("Children")]
public class GeometryGroup : Geometry
{
	/// <summary>The geometries combined by this group.</summary>
	public GeometryCollection Children { get; set; } = new GeometryCollection();

	/// <summary>How the combined area is filled. Default <see cref="Shapes.FillRule.EvenOdd"/>.</summary>
	public FillRule FillRule { get; set; } = FillRule.EvenOdd;

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var g = (GeometryGroup)destination;
		g.FillRule = FillRule;
		foreach (var child in Children)
			g.Children.Add((Geometry)(currentValueOnly ? child.CloneCurrentValue() : child.Clone()));
	}

	/// <inheritdoc/>
	public override bool CanFreeze => base.CanFreeze && CanFreezeAll(Children);

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		foreach (var child in Children) child.Freeze();
	}

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds
	{
		get
		{
			System.Windows.Rect bounds = System.Windows.Rect.Empty;
			foreach (Geometry g in Children) bounds = System.Windows.Rect.Union(bounds, g.Bounds);
			return bounds.IsEmpty ? new System.Windows.Rect(0, 0, 0, 0) : bounds;
		}
	}

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		foreach (Geometry g in Children) g.BuildPath(cr);
		// Cairo has one fill rule per fill and WPF fills the combined shape with the outermost group's rule, so the
		// outer group, which finishes last, decides.
		cr.FillRule = FillRule == FillRule.Nonzero ? Cairo.FillRule.Winding : Cairo.FillRule.EvenOdd;
	}
}
