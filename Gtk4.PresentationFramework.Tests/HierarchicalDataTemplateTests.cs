using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

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

	// ── Child resolution (core of the hierarchy builder) ──────────────────────
	// NOTE: exercised through the public HierarchicalDataTemplate surface
	// (ResolveChildren / ItemTemplate / SelectChildTemplate) rather than the internal
	// static TreeView.PopulateHierarchicalChildren wrapper — which just resolves the
	// children and copies them + the child template onto the TreeViewItem. The nested
	// TreeViewItem generation itself is covered by the TreeView GTK integration path.

	[Fact]
	public void HierarchicalDataTemplate_resolves_two_children_for_a_category()
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

		var children = hdt.ResolveChildren(cat);
		Assert.NotNull(children);
		Assert.Equal(2, children!.Cast<object>().Count());
	}

	[Fact]
	public void HierarchicalDataTemplate_child_ItemTemplate_is_the_configured_template()
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

		Assert.Same(childTemplate, hdt.ItemTemplate);
		Assert.Same(childTemplate, hdt.SelectChildTemplate(new ProductItem(), null));
	}

	[Fact]
	public void HierarchicalDataTemplate_resolves_empty_for_empty_collection()
	{
		var hdt = new HierarchicalDataTemplate { ItemsSource = new Binding("Products") };
		var cat = new CategoryItem { Name = "Empty", Products = new List<ProductItem>() };

		var children = hdt.ResolveChildren(cat);
		// Empty collection resolves to an empty (or null) sequence — no children.
		Assert.True(children == null || !children.Cast<object>().Any());
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

		// Resolve the child collection for each root item (what the hierarchy builder does).
		var childCounts = categories
			.Select(cat => hdt.ResolveChildren(cat)!.Cast<object>().Count())
			.ToList();

		// Level 0: 2 root nodes.
		Assert.Equal(2, childCounts.Count);

		// Level 1: first root has 3 children, second has 1.
		Assert.Equal(3, childCounts[0]);
		Assert.Equal(1, childCounts[1]);
	}
}
