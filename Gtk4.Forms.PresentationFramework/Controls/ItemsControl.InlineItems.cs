
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Controls;

public partial class ItemsControl
{
	ItemList? _itemList;
	bool _itemsFromSource;

	void FillItemsFromSource()
	{
		var list = (ItemList)Items;
		list.Filling = true;
		try
		{
			list.Clear();
			if (ItemsSource != null)
				foreach (var item in EffectiveItems)
					list.Add(item);
		}
		finally { list.Filling = false; }
		_itemsFromSource = ItemsSource != null;
	}

	sealed class ItemList : Collection<object?>
	{
		readonly ItemsControl _owner;

		public ItemList(ItemsControl owner) => _owner = owner;

		public bool Filling { get; set; }

		void Guard()
		{
			if (!Filling && _owner.ItemsSource != null)
				throw new InvalidOperationException(
					"Operation is not valid while ItemsSource is in use. Access and modify elements with ItemsControl.ItemsSource instead.");
		}

		void Changed()
		{
			if (!Filling) _owner.RebuildItems();
		}

		protected override void InsertItem(int index, object? item)
		{
			Guard();
			base.InsertItem(index, item);
			Changed();
		}

		protected override void SetItem(int index, object? item)
		{
			Guard();
			base.SetItem(index, item);
			Changed();
		}

		protected override void RemoveItem(int index)
		{
			Guard();
			base.RemoveItem(index);
			Changed();
		}

		protected override void ClearItems()
		{
			Guard();
			base.ClearItems();
			Changed();
		}
	}

	/// <summary>Returns whether <paramref name="item"/> is its own container (WPF: <c>item is UIElement</c>).</summary>
	protected virtual bool IsItemItsOwnContainerOverride(object? item) => item is UIElement;

	/// <summary>Creates the container for an item (WPF default: a <see cref="ContentPresenter"/>).</summary>
	protected virtual DependencyObject GetContainerForItemOverride() => new ContentPresenter();

	/// <summary>Whether prefix text search is enabled (store-only; GTK drop-down has no prefix search).</summary>
	public static readonly DependencyProperty IsTextSearchEnabledProperty =
		DependencyProperty.Register(nameof(IsTextSearchEnabled), typeof(bool), typeof(ItemsControl),
			new PropertyMetadata(true));

	/// <summary>Gets or sets whether text search is enabled.</summary>
	public bool IsTextSearchEnabled
	{
		get => (bool)GetValue(IsTextSearchEnabledProperty)!;
		set => SetValue(IsTextSearchEnabledProperty, value);
	}

	/// <summary>Whether text matching (e.g. <c>ComboBox.Text</c>) is case-sensitive.</summary>
	public static readonly DependencyProperty IsTextSearchCaseSensitiveProperty =
		DependencyProperty.Register(nameof(IsTextSearchCaseSensitive), typeof(bool), typeof(ItemsControl),
			new PropertyMetadata(false));

	/// <summary>Gets or sets whether text matching is case-sensitive.</summary>
	public bool IsTextSearchCaseSensitive
	{
		get => (bool)GetValue(IsTextSearchCaseSensitiveProperty)!;
		set => SetValue(IsTextSearchCaseSensitiveProperty, value);
	}
}
