using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Grid, Panel and ContentControl long-tail members used by official samples.</summary>
public class LongTailLayoutMemberTests : WpfGtkTestBase
{
	[Fact]
	public void Row_and_column_actual_sizes_are_zero_before_arrange_and_track_arrange() => Run(() =>
	{
		var grid = new Grid();
		var row = new RowDefinition { Height = new GridLength(30) };
		var star = new RowDefinition();
		var col = new ColumnDefinition { Width = new GridLength(40) };
		grid.RowDefinitions.Add(row);
		grid.RowDefinitions.Add(star);
		grid.ColumnDefinitions.Add(col);
		Assert.Equal(0, row.ActualHeight);
		Assert.Equal(0, col.ActualWidth);

		grid.Measure(new Size(200, 100));
		grid.Arrange(new Rect(0, 0, 200, 100));

		Assert.Equal(30, row.ActualHeight);
		Assert.Equal(70, star.ActualHeight);
		Assert.Equal(40, col.ActualWidth);
	});

	[Fact]
	public void SharedSizeGroup_is_stored_and_defaults_to_null()
	{
		Assert.Null(new ColumnDefinition().SharedSizeGroup);
		Assert.Equal("g", new RowDefinition { SharedSizeGroup = "g" }.SharedSizeGroup);
	}

	[Fact]
	public void IsSharedSizeScope_defaults_false_and_round_trips() => Run(() =>
	{
		var grid = new Grid();
		Assert.False(Grid.GetIsSharedSizeScope(grid));
		Grid.SetIsSharedSizeScope(grid, true);
		Assert.True(Grid.GetIsSharedSizeScope(grid));
		Assert.True((bool)grid.GetValue(Grid.IsSharedSizeScopeProperty)!);
		Assert.Throws<ArgumentNullException>(() => Grid.GetIsSharedSizeScope(null!));
		Assert.Throws<ArgumentNullException>(() => Grid.SetIsSharedSizeScope(null!, true));
	});

	[Fact]
	public void IsSharedSizeScope_is_settable_on_any_element() => Run(() =>
	{
		var panel = new StackPanel();
		Grid.SetIsSharedSizeScope(panel, true);
		Assert.True(Grid.GetIsSharedSizeScope(panel));
	});

	[Fact]
	public void ShowGridLines_defaults_false_and_round_trips() => Run(() =>
	{
		var grid = new Grid();
		Assert.False(grid.ShowGridLines);
		grid.ShowGridLines = true;
		Assert.True((bool)grid.GetValue(Grid.ShowGridLinesProperty)!);
	});

	[Fact]
	public void IsItemsHost_defaults_false_and_round_trips() => Run(() =>
	{
		var panel = new StackPanel();
		Assert.False(panel.IsItemsHost);
		panel.IsItemsHost = true;
		Assert.True((bool)panel.GetValue(Panel.IsItemsHostProperty)!);
		Assert.True(Panel.IsItemsHostProperty.DefaultMetadata is FrameworkPropertyMetadata
			{ IsNotDataBindable: true });
	});

	[Fact]
	public void HasContent_tracks_Content_and_is_read_only() => Run(() =>
	{
		var button = new Button();
		Assert.False(button.HasContent);
		button.Content = "x";
		Assert.True(button.HasContent);
		button.Content = string.Empty;
		Assert.True(button.HasContent);
		button.Content = null;
		Assert.False(button.HasContent);
		Assert.True(ContentControl.HasContentProperty.IsReadOnly);
		Assert.Throws<InvalidOperationException>(() => button.SetValue(ContentControl.HasContentProperty, true));
	});

	[Fact]
	public void ContentControl_AddChild_sets_content_and_rejects_a_second_child() => Run(() =>
	{
		var button = new Button();
		var first = new Border();
		((IAddChild)button).AddChild(first);
		Assert.Same(first, button.Content);
		Assert.Throws<InvalidOperationException>(() => ((IAddChild)button).AddChild(new Border()));
		((IAddChild)button).AddChild(null!);
		Assert.Null(button.Content);
		((IAddChild)button).AddText("hi");
		Assert.Equal("hi", button.Content);
	});

	[Fact]
	public void Window_AddChild_is_callable_from_a_subclass() => Run(() =>
	{
		var w = new AddChildWindow();
		var panel = new StackPanel();
		w.Add(panel);
		Assert.Same(panel, w.Content);
	});

	[Fact]
	public void Panel_AddChild_appends_elements_and_validates() => Run(() =>
	{
		var panel = new StackPanel();
		var child = new Border();
		((IAddChild)panel).AddChild(child);
		Assert.Contains(child, panel.Children);
		Assert.Throws<ArgumentNullException>(() => ((IAddChild)panel).AddChild(null!));
		Assert.Throws<ArgumentException>(() => ((IAddChild)panel).AddChild("text"));
		((IAddChild)panel).AddText("  \n");
		Assert.Throws<ArgumentException>(() => ((IAddChild)panel).AddText("words"));
		((IAddChild)panel).AddText(null!);
	});

	[Fact]
	public void Panel_AddChild_on_an_items_host_throws() => Run(() =>
	{
		var panel = new StackPanel { IsItemsHost = true };
		Assert.Throws<InvalidOperationException>(() => ((IAddChild)panel).AddChild(new Border()));
	});

	sealed class AddChildWindow : Window
	{
		public void Add(object child) => AddChild(child);
	}
}
