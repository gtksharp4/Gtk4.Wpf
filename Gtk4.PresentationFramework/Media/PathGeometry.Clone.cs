namespace System.Windows.Media;

public partial class PathGeometry
{
	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var g = (PathGeometry)destination;
		g.FillRule = FillRule;
		foreach (var figure in Figures) g.Figures.Add(figure);
	}
}
