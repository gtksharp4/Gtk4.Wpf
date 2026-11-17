using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk.Forms.PresentationFramework.Tests;

/// <summary>
/// Pure tests for <see cref="HierarchicalDataTemplate"/> hierarchy generation.
/// </summary>
public class HierarchicalDataTemplateTests
{
	// ── Data model ─────────────────────────────────────────────────────────────

	sealed class CategoryItem
	{
		public string Name { get; set; } = "";
		public List<ProductItem> Products { get; set; } = new();
	}

	sealed class ProductItem
	{
		public string Name { get; set; } = "";
	}

	// ── HierarchicalDataTemplate.ResolveChildren ──────────────────────────────

	[Fact]
	public void HierarchicalDataTemplate_ResolveChildren_returns_null_when_no_binding()
	{
		var hdt = new HierarchicalDataTemplate();
		var result = hdt.ResolveChildren(new CategoryItem { Name = "X" });
		Assert.Null(result);
	}

	[Fact]
	public void HierarchicalDataTemplate_ResolveChildren_returns_collection_via_path()
	{
		var hdt = new HierarchicalDataTemplate
		{
			ItemsSource = new Binding("Products")
		};
		var cat = new CategoryItem
		{
			Name = "Electronics",
			Products = new List<ProductItem>
			{
				new ProductItem { Name = "TV" },
				new ProductItem { Name = "Radio" }
			}
		};

		var children = hdt.ResolveChildren(cat);
		Assert.NotNull(children);
		var list = children!.Cast<object>().ToList();
		Assert.Equal(2, list.Count);
	}

	[Fact]
	public void HierarchicalDataTemplate_ResolveChildren_returns_null_for_null_item()
	{
		var hdt = new HierarchicalDataTemplate { ItemsSource = new Binding("Products") };
		Assert.Null(hdt.ResolveChildren(null));
	}

	// ── PopulateHierarchicalChildren ──────────────────────────────────────────

	[Fact]
	public void TreeView_PopulateHierarchicalChildren_sets_ItemsSource_on_tvi()
	{
		var hdt = new HierarchicalDataTemplate
		{
			ItemsSource = new Binding("Products"),
			ItemFactory = item => new TextBlock { Text = (item as CategoryItem)?.Name ?? "" }
		};

		var cat = new CategoryItem
		{
			Name = "Food",
			Products = new List<ProductItem>
			{
				new ProductItem { Name = "Apple" },
				new ProductItem { Name = "Banana" }
			}
		};

		var tvi = new TreeViewItem();
		TreeView.PopulateHierarchicalChildren(tvi, cat, hdt);

		Assert.NotNull(tvi.ItemsSource);
		var childItems = (tvi.ItemsSource as IEnumerable<object?>)!.ToList();
		Assert.Equal(2, childItems.Count);
	}

	[Fact]
	public void TreeView_PopulateHierarchicalChildren_propagates_child_ItemTemplate()
	{
		var childTemplate = new DataTemplate
		{
			ItemFactory = item => new TextBlock { Text = (item as ProductItem)?.Name ?? "" }
		};
		var hdt = new HierarchicalDataTemplate
		{
			ItemsSource = new Binding("Products"),
			ItemTemplate = childTemplate
		};

		var cat = new CategoryItem
		{
			Name = "Sports",
			Products = new List<ProductItem> { new ProductItem { Name = "Ball" } }
		};

		var tvi = new TreeViewItem();
		TreeView.PopulateHierarchicalChildren(tvi, cat, hdt);

		Assert.Same(childTemplate, tvi.ItemTemplate);
	}

	[Fact]
	public void TreeView_PopulateHierarchicalChildren_no_children_when_collection_empty()
	{
		var hdt = new HierarchicalDataTemplate
		{
			ItemsSource = new Binding("Products")
		};
		var cat = new CategoryItem { Name = "Empty", Products = new List<ProductItem>() };
		var tvi = new TreeViewItem();

		TreeView.PopulateHierarchicalChildren(tvi, cat, hdt);

		// No ItemsSource set when collection is empty.
		Assert.Null(tvi.ItemsSource);
	}

	// ── SelectChildTemplate ───────────────────────────────────────────────────

	[Fact]
	public void HierarchicalDataTemplate_SelectChildTemplate_uses_ItemTemplate()
	{
		var childTemplate = new DataTemplate();
		var hdt = new HierarchicalDataTemplate { ItemTemplate = childTemplate };

		var selected = hdt.SelectChildTemplate(new ProductItem(), null);

		Assert.Same(childTemplate, selected);
	}

	[Fact]
	public void HierarchicalDataTemplate_SelectChildTemplate_selector_wins_over_ItemTemplate()
	{
		var childTemplate = new DataTemplate();
		var selectorTemplate = new DataTemplate();

		var hdt = new HierarchicalDataTemplate
		{
			ItemTemplate = childTemplate,
			ItemTemplateSelector = new FixedTemplateSelector(selectorTemplate)
		};

		var selected = hdt.SelectChildTemplate(new ProductItem(), null);

		Assert.Same(selectorTemplate, selected);
		Assert.NotSame(childTemplate, selected);
	}

	sealed class FixedTemplateSelector : DataTemplateSelector
	{
		readonly DataTemplate _t;
		internal FixedTemplateSelector(DataTemplate t) => _t = t;
		public override DataTemplate? SelectTemplate(object? item, DependencyObject? container) => _t;
	}

	// ── Items count at each level (pure model, 2-level hierarchy) ────────────

	[Fact]
	public void Two_level_hierarchy_reports_correct_child_counts()
	{
		var hdt = new HierarchicalDataTemplate
		{
			ItemsSource = new Binding("Products"),
			ItemFactory = item => new TextBlock { Text = (item as CategoryItem)?.Name ?? "" }
		};

		var categories = new List<CategoryItem>
		{
			new CategoryItem
			{
				Name = "A",
				Products = new List<ProductItem>
				{
					new ProductItem { Name = "P1" },
					new ProductItem { Name = "P2" },
					new ProductItem { Name = "P3" }
				}
			},
			new CategoryItem
			{
				Name = "B",
				Products = new List<ProductItem>
				{
					new ProductItem { Name = "P4" }
				}
			}
		};

		// Simulate what TreeView.GetContainerForItem does for each root item.
		var rootTvis = new List<TreeViewItem>();
		foreach (var cat in categories)
		{
			var tvi = new TreeViewItem();
			TreeView.PopulateHierarchicalChildren(tvi, cat, hdt);
			rootTvis.Add(tvi);
		}

		// Level 0: 2 root nodes.
		Assert.Equal(2, rootTvis.Count);

		// Level 1: first root has 3 children, second has 1.
		var firstChildren = (rootTvis[0].ItemsSource as IEnumerable<object?>)!.ToList();
		var secondChildren = (rootTvis[1].ItemsSource as IEnumerable<object?>)!.ToList();
		Assert.Equal(3, firstChildren.Count);
		Assert.Equal(1, secondChildren.Count);
	}
}
