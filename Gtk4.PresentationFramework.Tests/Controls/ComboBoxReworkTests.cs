using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

file sealed class CountingListBox : ListBox
{
	public int Rebuilds;
	protected override void OnItemsRebuilt() { Rebuilds++; base.OnItemsRebuilt(); }
}

file sealed class CountingTabControl : TabControl
{
	public int Rebuilds;
	protected override void OnItemsRebuilt() { Rebuilds++; base.OnItemsRebuilt(); }
}

/// <summary>Round 40 review fixes for the ComboBox ItemsControl work.</summary>
public class ComboBoxReworkTests : WpfGtkTestBase
{
	[Fact]
	public void IsTextSearchEnabled_defaults_false_except_ComboBox()
	{
		Assert.False(new ItemsControl().IsTextSearchEnabled);
		Assert.False(new ListBox().IsTextSearchEnabled);
		Assert.False(new TabControl().IsTextSearchEnabled);
		Assert.True(new ComboBox().IsTextSearchEnabled);
		Assert.IsType<FrameworkPropertyMetadata>(ItemsControl.IsTextSearchEnabledProperty.GetMetadata(typeof(ListBox)));
		Assert.IsType<FrameworkPropertyMetadata>(
			ItemsControl.IsTextSearchCaseSensitiveProperty.GetMetadata(typeof(ItemsControl)));
	}

	[Fact]
	public void Text_does_not_change_selection_when_text_search_disabled()
	{
		var cb = new ComboBox { IsTextSearchEnabled = false };
		cb.Items.Add("a");
		cb.Items.Add("b");
		cb.SelectedIndex = 0;
		cb.Text = "b";
		Assert.Equal(0, cb.SelectedIndex);
		Assert.Equal("a", cb.SelectedItem);
		cb.Text = "zzz";
		Assert.Equal(0, cb.SelectedIndex);
	}

	[Fact]
	public void ListBox_inline_items_after_realize_do_not_rebuild() => Run(() =>
	{
		var lb = new CountingListBox();
		_ = lb.Widget;
		lb.Items.Add("a");
		lb.Items.Add("b");
		lb.Items.RemoveAt(0);
		Assert.Equal(0, lb.Rebuilds);
		lb.Items.Clear();
		lb.ItemsSource = new[] { "x" };
		Assert.Equal(1, lb.Rebuilds);
	});

	[Fact]
	public void TabControl_inline_items_after_realize_do_not_rebuild() => Run(() =>
	{
		var tc = new CountingTabControl();
		_ = tc.Widget;
		tc.Items.Add(new TabItem { Header = "h" });
		Assert.Equal(0, tc.Rebuilds);
	});

	[Fact]
	public void ComboBox_inline_item_gets_logical_parent_and_resources()
	{
		var cb = new ComboBox();
		cb.Resources["k"] = "v";
		var item = new ComboBoxItem();
		cb.Items.Add(item);
		Assert.Same(cb, item.LogicalParent);
		Assert.Equal("v", item.TryFindResource("k"));
		cb.Items.Clear();
		Assert.Null(item.LogicalParent);
	}

	[Fact]
	public void ComboBox_exposes_Items_as_content_property()
	{
		var attr = typeof(ComboBox).GetCustomAttribute<ContentPropertyAttribute>(true);
		Assert.Equal("Items", attr?.Name);
	}

	[Fact]
	public void Clearing_items_empties_model_and_resets_selection_DPs() => Run(() =>
	{
		var cb = new ComboBox();
		var dd = (Gtk.DropDown)cb.Widget;
		cb.Items.Add("a");
		cb.SelectedIndex = 0;
		cb.Items.Clear();
		Assert.Equal(-1, cb.SelectedIndex);
		Assert.Null(cb.SelectedItem);
		Assert.Equal(string.Empty, cb.Text);
		// An empty model has no valid position; a non-empty one cannot show "no selection" (see ComboBox remarks).
		Assert.Equal(uint.MaxValue, dd.Selected);
	});

	[Fact]
	public void IsReadOnly_is_shared_with_TextBox()
	{
		Assert.Same(TextBox.IsReadOnlyProperty, ComboBox.IsReadOnlyProperty);
		var cb = new ComboBox { IsReadOnly = true };
		Assert.True(cb.IsReadOnly);
	}

	[Fact]
	public void IsDropDownOpen_is_store_only_and_does_not_touch_popover() => Run(() =>
	{
		var cb = new ComboBox { ItemsSource = new[] { "a" } };
		_ = cb.Widget;
		int o = 0, c = 0;
		cb.DropDownOpened += (_, _) => o++;
		cb.DropDownClosed += (_, _) => c++;
		cb.IsDropDownOpen = true;
		Assert.True(cb.IsDropDownOpen);
		cb.IsDropDownOpen = false;
		Assert.False(cb.IsDropDownOpen);
		Assert.Equal((1, 1), (o, c));
	});

	[Fact]
	public void IsDropDownOpen_set_before_widget_is_kept() => Run(() =>
	{
		var cb = new ComboBox { IsDropDownOpen = true };
		_ = cb.Widget;
		Assert.True(cb.IsDropDownOpen);
	});
}
