using System.Windows.Controls;
using System.Windows.Data;

namespace System.Windows;

/// <summary>
/// A <see cref="DataTemplate"/> that specifies how a hierarchical data item should
/// be displayed in a <see cref="Controls.TreeView"/>. When <see cref="ItemsSource"/>
/// is set, child <see cref="Controls.TreeViewItem"/>s are generated from the value
/// of that binding on the data item; each child uses <see cref="ItemTemplate"/> (or
/// <see cref="ItemTemplateSelector"/>) as its own template.
/// <para>
/// Limitation: recursion depth is unbounded (the tree mirrors the data model); each
/// level generates one child <see cref="Controls.TreeViewItem"/> per bound item.
/// Generating nested containers requires a <see cref="Controls.TreeViewItem"/> root
/// (see <see cref="Controls.TreeView.GetContainerForItem"/> handling below).
/// </para>
/// </summary>
public class HierarchicalDataTemplate : DataTemplate
{
	/// <summary>
	/// A binding applied to a data item to obtain the child items collection.
	/// When set, <see cref="Controls.TreeView"/> (and <see cref="Controls.TreeViewItem"/>)
	/// will recursively generate child nodes.
	/// </summary>
	public Binding? ItemsSource { get; set; }

	/// <summary>
	/// Template used to display each child item. Applied recursively when itself a
	/// <see cref="HierarchicalDataTemplate"/>.
	/// </summary>
	public DataTemplate? ItemTemplate { get; set; }

	/// <summary>
	/// Selector used to pick the template for each child item.
	/// Takes precedence over <see cref="ItemTemplate"/> when set.
	/// </summary>
	public DataTemplateSelector? ItemTemplateSelector { get; set; }

	/// <summary>
	/// Resolves the child items for <paramref name="dataItem"/> using
	/// <see cref="ItemsSource"/> binding path. Returns <c>null</c> when
	/// <see cref="ItemsSource"/> is not set or the path resolves to nothing.
	/// </summary>
	internal System.Collections.IEnumerable? ResolveChildren(object? dataItem, DependencyObject? scope = null)
	{
		if (ItemsSource == null || dataItem == null) return null;
		if (ItemsSource.XPath != null && dataItem is System.Xml.XmlNode)
			return BindingEngine.SelectXmlNodes(dataItem, ItemsSource.XPath, scope);
		var path = PropertyPath.ResolveOwner(dataItem, ItemsSource.PathString);
		if (string.IsNullOrEmpty(path)) return dataItem as System.Collections.IEnumerable;
		var prop = dataItem.GetType().GetProperty(path);
		return prop?.GetValue(dataItem) as System.Collections.IEnumerable;
	}

	/// <summary>
	/// Picks the effective child template for a given child item.
	/// Priority: <see cref="ItemTemplateSelector"/> → <see cref="ItemTemplate"/> → <c>null</c>.
	/// </summary>
	internal DataTemplate? SelectChildTemplate(object? child, DependencyObject? container)
	{
		if (ItemTemplateSelector != null)
		{
			var selected = ItemTemplateSelector.SelectTemplate(child, container);
			if (selected != null) return selected;
		}
		return ItemTemplate;
	}
}
