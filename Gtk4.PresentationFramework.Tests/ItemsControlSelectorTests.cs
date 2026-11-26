using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for ItemsControl selector/style/DisplayMemberPath DPs and behaviour.
/// Each test fails without the feature introduced in ItemsControl.Selectors.cs.
/// </summary>
public class ItemsControlSelectorTests
{
	// ── Helpers ────────────────────────────────────────────────────────────────

	sealed class NamedItem
	{
		public string Name { get; set; } = "";
		public override string ToString() => Name;
	}

	sealed class AltItem
	{
		public string Label { get; set; } = "";
	}

	/// <summary>Tracks which containers were created, without touching GTK.</summary>
	sealed class InspectableItemsControl : ItemsControl
	{
		internal readonly List<UIElement> CreatedContainers = new();

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");

		protected override UIElement GetContainerForItem(object? item)
		{
			var container = base.GetContainerForItem(item);
			CreatedContainers.Add(container);
			return container;
		}

		/// <summary>Triggers the pure item-generation path without GTK.</summary>
		internal void Populate(IEnumerable<object?> items)
		{
			CreatedContainers.Clear();
			foreach (var item in items)
				GetContainerForItem(item);
		}
	}

	// ── DisplayMemberPath DP round-trip ───────────────────────────────────────

	[Fact]
	public void DisplayMemberPath_dp_round_trips()
	{
		var ic = new InspectableItemsControl { DisplayMemberPath = "Name" };
		Assert.Equal("Name", ic.DisplayMemberPath);
	}

	[Fact]
	public void DisplayMemberPath_null_by_default()
	{
		var ic = new InspectableItemsControl();
		Assert.Null(ic.DisplayMemberPath);
	}

	// ── DisplayMemberPath text extraction ─────────────────────────────────────

	[Fact]
	public void DisplayMemberPath_picks_property_text_from_container()
	{
		var ic = new InspectableItemsControl { DisplayMemberPath = "Name" };

		var item = new NamedItem { Name = "Alpha" };
		ic.Populate(new object?[] { item });

		var cp = ic.CreatedContainers[0] as ContentPresenter;
		Assert.NotNull(cp);
		Assert.Equal("Alpha", cp!.Content as string);
	}

	[Fact]
	public void DisplayMemberPath_different_property_name_works()
	{
		var ic = new InspectableItemsControl { DisplayMemberPath = "Label" };

		var item = new AltItem { Label = "Beta" };
		ic.Populate(new object?[] { item });

		var cp = ic.CreatedContainers[0] as ContentPresenter;
		Assert.NotNull(cp);
		Assert.Equal("Beta", cp!.Content as string);
	}

	[Fact]
	public void Without_DisplayMemberPath_falls_back_to_whole_item()
	{
		var ic = new InspectableItemsControl();
		var item = new NamedItem { Name = "Gamma" };
		ic.Populate(new object?[] { item });

		var cp = ic.CreatedContainers[0] as ContentPresenter;
		Assert.NotNull(cp);
		Assert.Same(item, cp!.Content);
	}

	// ── ItemContainerStyle applied ────────────────────────────────────────────

	[Fact]
	public void ItemContainerStyle_dp_round_trips()
	{
		var style = new Style(typeof(ContentPresenter));
		var ic = new InspectableItemsControl { ItemContainerStyle = style };
		Assert.Same(style, ic.ItemContainerStyle);
	}

	[Fact]
	public void ItemContainerStyle_null_by_default()
	{
		Assert.Null(new InspectableItemsControl().ItemContainerStyle);
	}

	// ── ItemTemplateSelector DP round-trip ────────────────────────────────────

	[Fact]
	public void ItemTemplateSelector_dp_round_trips()
	{
		var sel = new DataTemplateSelector();
		var ic = new InspectableItemsControl { ItemTemplateSelector = sel };
		Assert.Same(sel, ic.ItemTemplateSelector);
	}

	[Fact]
	public void ItemTemplateSelector_null_by_default()
	{
		Assert.Null(new InspectableItemsControl().ItemTemplateSelector);
	}

	// ── ItemTemplateSelector chosen over ItemTemplate ─────────────────────────

	[Fact]
	public void ItemTemplateSelector_wins_over_ItemTemplate_when_set()
	{
		// Selector that always returns a "selector template".
		var selectorTemplate = new DataTemplate
		{
			ItemFactory = _ => new TextBlock { Text = "selector" }
		};
		var staticTemplate = new DataTemplate
		{
			ItemFactory = _ => new TextBlock { Text = "static" }
		};

		var sel = new PickingTemplateSelector(selectorTemplate);
		var ic = new InspectableItemsControl
		{
			ItemTemplate = staticTemplate,
			ItemTemplateSelector = sel
		};

		ic.Populate(new object?[] { new NamedItem { Name = "X" } });

		// Selector was called.
		Assert.Equal(1, sel.InvokeCount);
		// Container is a TextBlock (from selector template), not from static template.
		var tb = ic.CreatedContainers[0] as TextBlock;
		Assert.NotNull(tb);
		Assert.Equal("selector", tb!.Text);
	}

	sealed class PickingTemplateSelector : DataTemplateSelector
	{
		readonly DataTemplate _template;

		internal PickingTemplateSelector(DataTemplate template) => _template = template;

		public override DataTemplate? SelectTemplate(object? item, DependencyObject? container)
		{
			InvokeCount++;
			return _template;
		}

		/// <summary>Number of times the selector was asked for a template.</summary>
		internal int InvokeCount { get; private set; }
	}

	// Revised test to avoid ref-int closure issue — use InvokeCount directly.
	[Fact]
	public void ItemTemplateSelector_invoked_per_item()
	{
		var template = new DataTemplate { ItemFactory = _ => new TextBlock { Text = "t" } };
		int callCount = 0;

		var sel = new CountingTemplateSelector(template, () => callCount++);
		var ic = new InspectableItemsControl { ItemTemplateSelector = sel };

		ic.Populate(new object?[] { new NamedItem(), new NamedItem(), new NamedItem() });

		Assert.Equal(3, callCount);
	}

	sealed class CountingTemplateSelector : DataTemplateSelector
	{
		readonly DataTemplate _t;
		readonly Action _onInvoke;
		internal CountingTemplateSelector(DataTemplate t, Action onInvoke)
		{
			_t = t;
			_onInvoke = onInvoke;
		}
		public override DataTemplate? SelectTemplate(object? item, DependencyObject? container)
		{
			_onInvoke();
			return _t;
		}
	}

	// ── ItemContainerStyleSelector wins over ItemContainerStyle ───────────────

	[Fact]
	public void ItemContainerStyleSelector_dp_round_trips()
	{
		var ss = new StyleSelector();
		var ic = new InspectableItemsControl { ItemContainerStyleSelector = ss };
		Assert.Same(ss, ic.ItemContainerStyleSelector);
	}
}
