using System.Globalization;

namespace System.ComponentModel;

/// <summary>
/// Base class for group descriptions used by <see cref="System.Windows.Data.ICollectionView.GroupDescriptions"/>.
/// </summary>
public abstract class GroupDescription
{
	/// <summary>Returns the group key for <paramref name="item"/>.</summary>
	public abstract object? GroupNameFromItem(object item, int level, CultureInfo culture);
}
