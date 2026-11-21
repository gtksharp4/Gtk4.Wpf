using System.ComponentModel;

namespace System.Windows.Controls;

public partial class ItemsControl
{
	// The editing surface of Items: forwards to the editable default view over ItemsSource. Inline items have
	// no view to edit through, so they report nothing editable.
	sealed partial class ItemList : IEditableCollectionView
	{
		NewItemPlaceholderPosition _placeholder;

		IEditableCollectionView? View => _owner._collectionView as IEditableCollectionView;

		IEditableCollectionView RequireView() => View
			?? throw new InvalidOperationException("The items are not editable without an editable ItemsSource.");

		NewItemPlaceholderPosition IEditableCollectionView.NewItemPlaceholderPosition
		{
			get => View?.NewItemPlaceholderPosition ?? _placeholder;
			set
			{
				if (View is { } view) view.NewItemPlaceholderPosition = value;
				else _placeholder = value;
			}
		}

		bool IEditableCollectionView.CanAddNew => View?.CanAddNew ?? false;

		object IEditableCollectionView.AddNew() => RequireView().AddNew();

		void IEditableCollectionView.CommitNew() => RequireView().CommitNew();

		void IEditableCollectionView.CancelNew() => RequireView().CancelNew();

		bool IEditableCollectionView.IsAddingNew => View?.IsAddingNew ?? false;

		object IEditableCollectionView.CurrentAddItem => View?.CurrentAddItem!;

		bool IEditableCollectionView.CanRemove => View?.CanRemove ?? false;

		void IEditableCollectionView.RemoveAt(int index) => RequireView().RemoveAt(index);

		void IEditableCollectionView.Remove(object item) => RequireView().Remove(item);

		bool IEditableCollectionView.CanCancelEdit => View?.CanCancelEdit ?? false;

		void IEditableCollectionView.EditItem(object item) => RequireView().EditItem(item);

		void IEditableCollectionView.CommitEdit() => RequireView().CommitEdit();

		void IEditableCollectionView.CancelEdit() => RequireView().CancelEdit();

		bool IEditableCollectionView.IsEditingItem => View?.IsEditingItem ?? false;

		object IEditableCollectionView.CurrentEditItem => View?.CurrentEditItem!;
	}
}
