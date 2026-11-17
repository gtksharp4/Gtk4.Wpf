using System.Windows;

namespace System.Windows.Controls;

/// <summary>
/// Extends <see cref="TreeView"/> to support <see cref="HierarchicalDataTemplate"/>.
/// When <see cref="ItemsControl.ItemTemplate"/> is a
/// <see cref="HierarchicalDataTemplate"/>, the generated <see cref="TreeViewItem"/>
/// also binds its own <c>Items</c> from the template's <see cref="HierarchicalDataTemplate.ItemsSource"/>
/// and recurses into child levels using
/// <see cref="HierarchicalDataTemplate.ItemTemplate"/>/
/// <see cref="HierarchicalDataTemplate.ItemTemplateSelector"/>.
/// </summary>
public partial class TreeView
{
	/// <inheritdoc/>
	protected override UIElement GetContainerForItem(object? item)
	{
		var hdt = ItemTemplate as HierarchicalDataTemplate;
		var tvi = item as TreeViewItem ?? new TreeViewItem();

		if (hdt != null)
		{
			// Apply the visual for the header using the HierarchicalDataTemplate.
			var headerVisual = hdt.CreateVisualTreeForItem(item);
			tvi.Header = (object?)headerVisual ?? item?.ToString();

			// Populate children from ItemsSource binding on the data item.
			PopulateHierarchicalChildren(tvi, item, hdt);
		}
		else
		{
			tvi.Header ??= item?.ToString();
		}

		tvi.Selected += OnItemSelected;
		return tvi;
	}

	/// <summary>
	/// Recursively populates <paramref name="tvi"/> children from
	/// <paramref name="hdt"/>.<see cref="HierarchicalDataTemplate.ItemsSource"/>.
	/// Each child level uses the template's
	/// <see cref="HierarchicalDataTemplate.ItemTemplate"/>.
	/// </summary>
	internal static void PopulateHierarchicalChildren(
		TreeViewItem tvi, object? dataItem, HierarchicalDataTemplate hdt)
	{
		var children = hdt.ResolveChildren(dataItem);
		if (children == null) return;

		var childItems = new System.Collections.Generic.List<object?>();
		foreach (var c in children) childItems.Add(c);
		if (childItems.Count == 0) return;

		// Set ItemsSource on the TreeViewItem so its own items render correctly.
		// The child template is provided by the HierarchicalDataTemplate.ItemTemplate.
		var childTemplate = hdt.ItemTemplate;
		var childSelector = hdt.ItemTemplateSelector;
		tvi.ItemTemplate = childTemplate;
		tvi.ItemTemplateSelector = childSelector;
		tvi.ItemsSource = childItems;
	}
}
