namespace System.Windows.Controls;

/// <summary>
/// The WPF container of one group. This layer renders a group as a header row in the owner's item host, so
/// the type exists for code that names it (<c>AncestorType={x:Type GroupItem}</c>); it is not created.
/// </summary>
public class GroupItem : ContentControl
{
	/// <summary>Creates an empty group item.</summary>
	public GroupItem()
	{
	}
}
