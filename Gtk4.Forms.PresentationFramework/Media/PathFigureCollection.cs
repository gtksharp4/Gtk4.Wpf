using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// An ordered collection of <see cref="PathFigure"/> objects.
/// Mirrors <c>System.Windows.Media.PathFigureCollection</c>.
/// </summary>
public class PathFigureCollection : List<PathFigure>
{
	/// <summary>Creates an empty collection.</summary>
	public PathFigureCollection() { }

	/// <summary>Creates a collection holding <paramref name="figures"/>.</summary>
	public PathFigureCollection(IEnumerable<PathFigure> figures) : base(figures) { }

	/// <summary>Parses Path Markup into its figures.</summary>
	/// <exception cref="FormatException">Unrecognised command or malformed number.</exception>
	public static PathFigureCollection Parse(string source)
		=> new PathFigureCollection(PathMarkupParser.Parse(source).Figures);
}
