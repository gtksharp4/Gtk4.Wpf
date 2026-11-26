using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class ControlMemberTests : WpfGtkTestBase
{
	sealed class Person
	{
		public string Name { get; set; } = "";
		public Person? Boss { get; set; }
	}

	[Fact]
	public void MenuItem_is_a_Control_with_Padding_and_font_members()
	{
		var item = new MenuItem { Padding = new Thickness(3, 4, 5, 6), FontSize = 20 };
		Assert.IsAssignableFrom<Control>(item);
		Assert.Equal(new Thickness(3, 4, 5, 6), item.Padding);
		Assert.Equal(20.0, item.FontSize);
		Assert.False(item.IsSubmenuOpen);
		var style = new Style(typeof(MenuItem));
		item.ItemContainerStyle = style;
		Assert.Same(style, item.ItemContainerStyle);
	}

	[Fact]
	public void MenuItem_IsSubmenuOpen_opens_and_closes_the_popover()
	{
		Run(() =>
		{
			var parent = new MenuItem { Header = "File" };
			Assert.True(((FrameworkPropertyMetadata)MenuItem.IsSubmenuOpenProperty.GetMetadata(typeof(MenuItem)))
				.BindsTwoWayByDefault);
			parent.Items.Add(new MenuItem { Header = "Open" });
			var button = (Gtk.MenuButton)parent.Widget;
			parent.IsSubmenuOpen = true;
			Assert.True(parent.IsSubmenuOpen);
			parent.IsSubmenuOpen = false;
			Assert.NotNull(button.Popover);
		});
	}

	[Fact]
	public void Frame_members_have_WPF_defaults()
	{
		var frame = new Frame();
		Assert.Equal(NavigationUIVisibility.Automatic, frame.NavigationUIVisibility);
		Assert.Null(frame.CurrentSource);
		frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
		Assert.Equal(NavigationUIVisibility.Hidden, frame.NavigationUIVisibility);
		Assert.Throws<ArgumentException>(() => frame.NavigationUIVisibility = (NavigationUIVisibility)7);
	}

	[Fact]
	public void TabItem_IsSelected_syncs_both_ways_with_the_TabControl()
	{
		Run(() =>
		{
			var a = new TabItem { Header = "a" };
			var b = new TabItem { Header = "b" };
			var tabs = new TabControl();
			tabs.Items.Add(a);
			tabs.Items.Add(b);
			_ = tabs.Widget;
			var meta = (FrameworkPropertyMetadata)TabItem.IsSelectedProperty.GetMetadata(typeof(TabItem));
			Assert.True(meta.BindsTwoWayByDefault);
			Assert.True(meta.Journal);
			Assert.True(a.IsSelected);
			b.IsSelected = true;
			Assert.Same(b, tabs.SelectedItem);
			tabs.SelectedItem = a;
			Assert.True(a.IsSelected);
			Assert.False(b.IsSelected);
		});
	}

	[Fact]
	public void TabStripPlacement_defaults_to_Top_and_moves_the_Notebook_tabs()
	{
		Run(() =>
		{
			var tabs = new TabControl();
			Assert.Equal(Dock.Top, tabs.TabStripPlacement);
			var notebook = (Gtk.Notebook)tabs.Widget;
			Assert.Equal(Gtk.PositionType.Top, notebook.TabPos);
			tabs.TabStripPlacement = Dock.Left;
			Assert.Equal(Gtk.PositionType.Left, notebook.TabPos);
			tabs.TabStripPlacement = Dock.Bottom;
			Assert.Equal(Gtk.PositionType.Bottom, notebook.TabPos);
			Assert.Throws<ArgumentException>(() => tabs.TabStripPlacement = (Dock)9);
		});
	}

	[Fact]
	public void Expander_TreeView_GridSplitter_Popup_defaults()
	{
		Assert.Equal(ExpandDirection.Down, new Expander().ExpandDirection);
		Assert.Equal(string.Empty, new TreeView().SelectedValuePath);
		Assert.False(new GridSplitter().ShowsPreview);
		var popup = new Popup();
		Assert.False(popup.AllowsTransparency);
		Assert.Equal(PopupAnimation.None, popup.PopupAnimation);
		Assert.Throws<ArgumentException>(() => popup.PopupAnimation = (PopupAnimation)9);
	}

	[Fact]
	public void TreeView_SelectedValue_follows_the_path()
	{
		var tree = new TreeView { SelectedValuePath = "Boss.Name" };
		Assert.Null(tree.SelectedValue);
		tree.SetValue(TreeView.SelectedItemProperty, new Person { Boss = new Person { Name = "Ada" } });
		Assert.Equal("Ada", tree.SelectedValue);
	}

	[Fact]
	public void WrapPanel_ItemWidth_and_ItemHeight_size_every_child()
	{
		var panel = new WrapPanel();
		Assert.True(double.IsNaN(panel.ItemWidth));
		Assert.True(double.IsNaN(panel.ItemHeight));
		panel.ItemWidth = 50;
		panel.ItemHeight = 20;
		for (int i = 0; i < 5; i++)
			panel.Children.Add(new Border { Width = 10 + i * 5, Height = 5 });
		panel.Measure(new Size(120, double.PositiveInfinity));
		Assert.Equal(new Size(100, 60), panel.DesiredSize);
		Assert.Throws<ArgumentException>(() => panel.ItemWidth = -1);
	}

	[Fact]
	public void ScrollViewer_CanContentScroll_is_attached_and_defaults_false()
	{
		var viewer = new ScrollViewer();
		Assert.False(viewer.CanContentScroll);
		viewer.CanContentScroll = true;
		Assert.True(ScrollViewer.GetCanContentScroll(viewer));
		var list = new ListBox();
		ScrollViewer.SetCanContentScroll(list, true);
		Assert.True(ScrollViewer.GetCanContentScroll(list));
	}

	[Fact]
	public void DataGrid_tail_has_WPF_defaults()
	{
		var grid = new DataGrid();
		Assert.Equal(DataGridHeadersVisibility.All, grid.HeadersVisibility);
		Assert.True(grid.CanUserResizeColumns);
		Assert.True(grid.CanUserResizeRows);
		Assert.Null(grid.ColumnHeaderStyle);
		grid.HeadersVisibility = DataGridHeadersVisibility.Column;
		Assert.Equal(1, (int)grid.HeadersVisibility);
		Assert.IsAssignableFrom<Control>(new DataGridRow());
		Assert.IsAssignableFrom<ButtonBase>(new DataGridColumnHeader());
		Assert.False(new DataGridRow().IsSelected);
	}

	[Fact]
	public void ListViewItem_is_a_ListBoxItem()
	{
		Assert.IsAssignableFrom<ListBoxItem>(new ListViewItem());
	}

	[Fact]
	public void SystemParameters_report_WPF_defaults_and_stable_keys()
	{
		Assert.False(SystemParameters.HighContrast);
		Assert.Equal(4.0, SystemParameters.MinimumHorizontalDragDistance);
		Assert.Equal(4.0, SystemParameters.MinimumVerticalDragDistance);
		Assert.Equal(PopupAnimation.Fade, SystemParameters.MenuPopupAnimation);
		Assert.Equal(SystemParameters.VerticalScrollBarWidthKey, SystemParameters.VerticalScrollBarWidthKey);
		Assert.NotEqual(SystemParameters.VerticalScrollBarWidthKey, SystemParameters.HorizontalScrollBarHeightKey);
		Assert.NotNull(SystemParameters.MenuPopupAnimationKey);
		Assert.True(SystemParameters.WorkArea.Width > 0);
		SystemParameters.StaticPropertyChanged += (_, _) => { };
	}
}
