using System.ComponentModel;

namespace System.ComponentModel;

/// <summary>
/// Defines the direction and property name to sort by.
/// Mirrors <c>System.ComponentModel.SortDescription</c> from WPF's WindowsBase.
/// Note: <c>ListSortDirection</c> is used from the BCL (System.ComponentModel.TypeConverter).
/// </summary>
public struct SortDescription
{
	/// <summary>The property name used as the sort key. <c>null</c> or empty sorts by the item itself.</summary>
	public string? PropertyName { get; }

	/// <summary>Direction of the sort.</summary>
	public ListSortDirection Direction { get; }

	/// <summary>Initialises a new <see cref="SortDescription"/>.</summary>
	public SortDescription(string? propertyName, ListSortDirection direction)
	{
		PropertyName = propertyName;
		Direction = direction;
	}
}