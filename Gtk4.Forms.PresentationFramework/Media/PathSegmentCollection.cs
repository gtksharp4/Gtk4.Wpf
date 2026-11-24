using System.Collections.ObjectModel;

namespace System.Windows.Media;

/// <summary>
/// An ordered collection of <see cref="PathSegment"/> objects; every edit raises the owner's change notification.
/// Mirrors <c>System.Windows.Media.PathSegmentCollection</c>.
/// </summary>
public class PathSegmentCollection : Collection<PathSegment>
{
	internal Action? Changed;

	/// <summary>Creates an empty collection.</summary>
	public PathSegmentCollection() { }

	/// <summary>Creates a collection holding <paramref name="segments"/>.</summary>
	public PathSegmentCollection(System.Collections.Generic.IEnumerable<PathSegment> segments)
	{
		foreach (var segment in segments) Add(segment);
	}

	/// <inheritdoc/>
	protected override void InsertItem(int index, PathSegment item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		base.InsertItem(index, item);
		item.Changed += Notify;
		Notify();
	}

	/// <inheritdoc/>
	protected override void SetItem(int index, PathSegment item)
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
		foreach (var segment in this) segment.Changed -= Notify;
		base.ClearItems();
		Notify();
	}

	void Notify() => Changed?.Invoke();
}
