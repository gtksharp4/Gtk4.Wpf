using System.ComponentModel;

namespace System.Windows.Controls;

public sealed partial class ItemCollection : IEditableCollectionView
{
	// The editing surface forwards to the editable view over ItemsSource. Inline items have no view to
	// edit through, so they report nothing editable.
	NewItemPlaceholderPosition _placeholder;

	IEditableCollectionView? EditView => _sourceView as IEditableCollectionView;

	IEditableCollectionView RequireView() => EditView
		?? throw new InvalidOperationException("The items are not editable without an editable ItemsSource.");

	NewItemPlaceholderPosition IEditableCollectionView.NewItemPlaceholderPosition
	{
		get => EditView?.NewItemPlaceholderPosition ?? _placeholder;
		set
		{
			if (EditView is { } view) view.NewItemPlaceholderPosition = value;
			else _placeholder = value;
		}
	}

	bool IEditableCollectionView.CanAddNew => EditView?.CanAddNew ?? false;

	object IEditableCollectionView.AddNew() => RequireView().AddNew();

	void IEditableCollectionView.CommitNew() => RequireView().CommitNew();

	void IEditableCollectionView.CancelNew() => RequireView().CancelNew();

	bool IEditableCollectionView.IsAddingNew => EditView?.IsAddingNew ?? false;

	object IEditableCollectionView.CurrentAddItem => EditView?.CurrentAddItem!;

	bool IEditableCollectionView.CanRemove => EditView?.CanRemove ?? false;

	void IEditableCollectionView.RemoveAt(int index) => RequireView().RemoveAt(index);

	void IEditableCollectionView.Remove(object item) => RequireView().Remove(item);

	bool IEditableCollectionView.CanCancelEdit => EditView?.CanCancelEdit ?? false;

	void IEditableCollectionView.EditItem(object item) => RequireView().EditItem(item);

	void IEditableCollectionView.CommitEdit() => RequireView().CommitEdit();

	void IEditableCollectionView.CancelEdit() => RequireView().CancelEdit();

	bool IEditableCollectionView.IsEditingItem => EditView?.IsEditingItem ?? false;

	object IEditableCollectionView.CurrentEditItem => EditView?.CurrentEditItem!;
}
