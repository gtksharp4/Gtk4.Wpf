namespace System.Windows.Controls;

/// <summary>
/// A <see cref="ListBox"/> whose rows can be shaped by a <see cref="View"/>. With a <see cref="GridView"/> the
/// list shows a header row of column headers above the rows, one cell per column.
/// <para>
/// Mapping: the list is still the <see cref="ListBox"/> (a <see cref="Gtk.ListBox"/>, one row per item, so
/// selection, keyboard and automation are unchanged). The widget is a vertical box of the view's header host and
/// that list; each <see cref="ListViewItem"/> hosts the view's row presenter instead of a content presenter. The
/// header is part of the widget, so inside a scroll viewer it scrolls with the rows (WPF keeps it fixed).
/// </para>
/// </summary>
public partial class ListView : ListBox
{
	/// <summary>Identifies the <see cref="View"/> property.</summary>
	public static readonly DependencyProperty ViewProperty =
		DependencyProperty.Register(nameof(View), typeof(ViewBase), typeof(ListView),
			new PropertyMetadata(null, OnViewChanged));

	Gtk.Box? _headerHost;
	UIElement? _header;

	/// <summary>The header element of the current view, or null.</summary>
	internal UIElement? HeaderElement => _header;

	/// <summary>Gets or sets the view that shapes the header and the rows; null shows the plain list.</summary>
	public ViewBase? View
	{
		get => (ViewBase?)GetValue(ViewProperty);
		set => SetValue(ViewProperty, value);
	}

	static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((ListView)d).ApplyView();

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		var list = base.CreateItemsWidget();
		list.Vexpand = true;
		_headerHost = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		var box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		box.Append(_headerHost);
		box.Append(list);
		ApplyHeader();
		return box;
	}

	/// <inheritdoc/>
	protected override bool IsItemItsOwnContainerOverride(object? item) => item is ListViewItem;

	internal override ListBoxItem CreateItemContainer() => new ListViewItem();

	internal override void OnContainerCreated(ListBoxItem container)
	{
		if (container is ListViewItem item) item.OwnerListView = this;
	}

	void ApplyView()
	{
		ApplyHeader();
		foreach (var container in _containers)
			if (container is ListViewItem) container.RebuildRowContent();
	}

	void ApplyHeader()
	{
		if (_headerHost == null) return;
		if (_header != null)
		{
			_headerHost.Remove(_header.Widget);
			_header.LogicalParent = null;
			_header = null;
		}
		_header = View?.CreateHeader();
		if (_header == null) return;
		_header.LogicalParent = this;
		_headerHost.Append(_header.Widget);
	}
}
