namespace System.Windows.Controls;

public partial class ListBox
{
	object? _pendingScrollItem;
	bool _scrollPending;

	/// <summary>
	/// Scrolls the enclosing scroll viewer so the row of <paramref name="item"/> is visible. Before the control
	/// is loaded the scroll is deferred until it is. An item that is not in <see cref="ItemsControl.Items"/> does
	/// nothing.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
	public void ScrollIntoView(object item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		if (Items.IndexOf(item) < 0) return;
		if (IsLoaded)
		{
			ScrollToItem(item);
			return;
		}
		_pendingScrollItem = item;
		if (_scrollPending) return;
		_scrollPending = true;
		Loaded += OnLoadedScroll;
	}

	void OnLoadedScroll(object sender, RoutedEventArgs e)
	{
		Loaded -= OnLoadedScroll;
		_scrollPending = false;
		var item = _pendingScrollItem;
		_pendingScrollItem = null;
		// The first layout pass has not sized the scroller yet at Loaded; scroll once it is idle.
		if (item != null)
			Dispatcher.BeginInvoke(() => ScrollToItem(item), Threading.DispatcherPriority.ApplicationIdle);
	}

	void ScrollToItem(object item)
	{
		int index = Items.IndexOf(item);
		var row = ContainerAt(index)?.Widget;
		if (row == null || !HasBackingWidget) return;
		Gtk.ScrolledWindow? scroller = null;
		for (var w = Widget.Parent; w != null && scroller == null; w = w.Parent)
			scroller = w as Gtk.ScrolledWindow;
		if (scroller == null) return;
		var content = scroller.Child is Gtk.Viewport viewport ? viewport.Child : scroller.Child;
		if (content == null || !row.ComputeBounds(content, out var bounds)) return;
		var adjustment = scroller.Vadjustment;
		double top = bounds.Y;
		double bottom = top + bounds.Height;
		if (top < adjustment.Value) adjustment.Value = top;
		else if (bottom > adjustment.Value + adjustment.PageSize) adjustment.Value = bottom - adjustment.PageSize;
	}
}
