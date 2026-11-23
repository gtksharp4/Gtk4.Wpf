namespace System.Windows.Controls;

public partial class ItemsControl
{
	static readonly ItemsPanelTemplate _defaultItemsPanel = CreateDefaultItemsPanel();

	static ItemsPanelTemplate CreateDefaultItemsPanel()
	{
		var template = new ItemsPanelTemplate(() => new StackPanel());
		template.Seal();
		return template;
	}

	/// <summary>Identifies the <see cref="ItemsPanel"/> dependency property (default: a sealed StackPanel template).</summary>
	public static readonly DependencyProperty ItemsPanelProperty =
		DependencyProperty.Register("ItemsPanel", typeof(ItemsPanelTemplate), typeof(ItemsControl),
			new FrameworkPropertyMetadata(_defaultItemsPanel, OnItemsPanelChanged));

	/// <summary>
	/// The panel that lays out the items. Honoured by a plain ItemsControl (and subclasses that keep the default
	/// vertical Gtk.Box host): a non-default template hosts its Panel (e.g. a WrapPanel) and places the item
	/// containers in it. Selector subclasses (ListBox, ComboBox, ...) keep their native GTK widget, so the value is
	/// stored only. The default template is the vertical box and is kept as such.
	/// </summary>
	public ItemsPanelTemplate ItemsPanel
	{
		get => (ItemsPanelTemplate)GetValue(ItemsPanelProperty)!;
		set => SetValue(ItemsPanelProperty, value);
	}

	Panel? _itemsPanelHost;

	/// <summary>The hosted non-default items panel; null when the default vertical box is used.</summary>
	internal Panel? HostedItemsPanel => _itemsPanelHost;

	static void OnItemsPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (ItemsControl)d;
		if (control._defaultBox == null) return;
		foreach (var (_, widget) in control._rows)
			control.RemoveDefaultHostItem(widget);
		control.UninstallItemsPanel();
		control.InstallItemsPanel();
		foreach (var (_, widget) in control._rows)
			control.AddDefaultHostItem(widget);
	}

	void InstallItemsPanel()
	{
		var template = ItemsPanel;
		if (template == null || ReferenceEquals(template, _defaultItemsPanel)) return;
		_itemsPanelHost = template.CreatePanel();
		_itemsPanelHost.LogicalParent = this;
		_defaultBox!.Add(_itemsPanelHost.Widget);
	}

	void UninstallItemsPanel()
	{
		if (_itemsPanelHost == null) return;
		_defaultBox!.Remove(_itemsPanelHost.Widget);
		_itemsPanelHost.LogicalParent = null;
		_itemsPanelHost = null;
	}

	UIElement? ContainerOf(Gtk.Widget widget)
	{
		for (int i = _rows.Count - 1; i >= 0; i--)
			if (ReferenceEquals(_rows[i].Widget, widget)) return _rows[i].Container;
		return null;
	}

	void AddDefaultHostItem(Gtk.Widget widget)
	{
		var container = ContainerOf(widget);
		if (_itemsPanelHost != null && container != null)
			_itemsPanelHost.Children.Add(container);
		else
		{
			_defaultBox!.Add(widget);
			if (container != null) container.LogicalParent = this;
		}
	}

	void RemoveDefaultHostItem(Gtk.Widget widget)
	{
		var container = ContainerOf(widget);
		if (_itemsPanelHost != null && container != null)
			_itemsPanelHost.Children.Remove(container);
		else
			_defaultBox!.Remove(widget);
	}
}
