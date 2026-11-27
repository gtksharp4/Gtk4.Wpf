namespace System.Windows.Controls;

/// <summary>An item container of a <see cref="ListView"/>.</summary>
public class ListViewItem : ListBoxItem
{
	/// <summary>Creates an empty item.</summary>
	public ListViewItem()
	{
	}

	/// <summary>The list view that shows this item; its <see cref="ListView.View"/> shapes the row.</summary>
	internal ListView? OwnerListView { get; set; }

	/// <inheritdoc/>
	internal override UIElement? CreateViewPresenter() => OwnerListView?.View?.CreateRowPresenter(this);
}
