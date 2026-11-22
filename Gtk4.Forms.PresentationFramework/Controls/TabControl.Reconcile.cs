namespace System.Windows.Controls;

public partial class TabControl
{
	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		// Adding the first page makes GTK select it, so the marked tab is picked before the pages exist.
		var marked = FirstMarkedTab(null);
		var widget = base.CreateWidget();
		ReconcileSelectedTab(marked);
		return widget;
	}

	internal override void OnItemsRebuildCompleted()
	{
		base.OnItemsRebuildCompleted();
		ReconcileSelectedTab(FirstMarkedTab(SelectedItem));
	}

	TabItem? FirstMarkedTab(object? except)
	{
		foreach (var item in Items)
			if (item is TabItem { IsSelected: true } tab && !ReferenceEquals(tab, except)) return tab;
		return null;
	}

	// A tab marked IsSelected before it had an owner (XAML attribute order) was ignored. The marked tab becomes
	// the selection and any other tab still marked is cleared.
	void ReconcileSelectedTab(TabItem? chosen)
	{
		if (chosen == null) return;
		if (!ReferenceEquals(SelectedItem, chosen)) SelectedItem = chosen;
		foreach (var item in Items)
			if (item is TabItem tab && !ReferenceEquals(tab, chosen) && tab.IsSelected)
				tab.SetValue(TabItem.IsSelectedProperty, false);
	}
}
