using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Media;

/// <summary>
/// An ordered collection of <see cref="PathFigure"/> objects; every edit raises the owner's change notification.
/// Mirrors <c>System.Windows.Media.PathFigureCollection</c>.
/// </summary>
public class PathFigureCollection : Collection<PathFigure>
{
	internal Action? Changed;

	/// <summary>Creates an empty collection.</summary>
	public PathFigureCollection() { }

	/// <summary>Creates a collection holding <paramref name="figures"/>.</summary>
	public PathFigureCollection(IEnumerable<PathFigure> figures)
	{
		foreach (var figure in figures) Add(figure);
	}

	/// <summary>Parses Path Markup into its figures.</summary>
	/// <exception cref="FormatException">Unrecognised command or malformed number.</exception>
	public static PathFigureCollection Parse(string source)
		=> new PathFigureCollection(PathMarkupParser.Parse(source).Figures);

	/// <inheritdoc/>
	protected override void InsertItem(int index, PathFigure item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		base.InsertItem(index, item);
		item.Changed += Notify;
		Notify();
	}

	/// <inheritdoc/>
	protected override void SetItem(int index, PathFigure item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		this[index].Changed -= Notify;
		base.SetItem(index, item);
		item.Changed += Notify;
		Notify();
	}

	/// <inheritdoc/>
	protected override void RemoveItem(int index)
	{
		this[index].Changed -= Notify;
		base.RemoveItem(index);
		Notify();
	}

	/// <inheritdoc/>
	protected override void ClearItems()
	{
		foreach (var figure in this) figure.Changed -= Notify;
		base.ClearItems();
		Notify();
	}

	void Notify() => Changed?.Invoke();
}
