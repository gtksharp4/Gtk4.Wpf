using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class GridViewTests : WpfGtkTestBase
{
	sealed record Item(string Name, double Price);

	static readonly Item[] Items = { new("Pen", 1.5), new("Book", 12), new("Lamp", 30.25) };

	static List<Gtk.Widget> Descendants(Gtk.Widget root)
	{
		var all = new List<Gtk.Widget>();
		for (var c = root.FirstChild; c != null; c = c.NextSibling)
		{
			all.Add(c);
			all.AddRange(Descendants(c));
		}
		return all;
	}

	static List<string> Texts(UIElement element) =>
		Descendants(element.Widget).OfType<Gtk.Label>().Select(l => l.Text).Where(t => t.Length > 0).ToList();

	static GridView TwoColumns() => new GridView
	{
		Columns =
		{
			new GridViewColumn { Header = "Item", DisplayMemberBinding = new Binding("Name") },
			new GridViewColumn { Header = "Price", DisplayMemberBinding = new Binding("Price") { StringFormat = "0.00" } },
		}
	};

	static ListView Make(ViewBase? view = null)
	{
		var lv = new ListView { View = view ?? TwoColumns(), ItemsSource = Items };
		_ = lv.Widget;
		return lv;
	}

	[Fact]
	public void The_ListView_View_property_defaults_to_null_and_the_list_is_plain() => Run(() =>
	{
		var lv = new ListView { ItemsSource = Items };
		_ = lv.Widget;

		Assert.Null(lv.View);
		Assert.Null(lv.HeaderElement);
		Assert.Null(((ListViewItem)lv.ContainerAt(0)!).ViewPresenter);
		Assert.Equal(3, lv.Items.Count);
	});

	[Fact]
	public void A_GridView_header_has_one_column_header_per_column() => Run(() =>
	{
		var lv = Make();

		var header = Assert.IsType<GridViewRowPresenter>(lv.HeaderElement);
		var cells = header.VisualChildren.Cast<GridViewColumnHeader>().ToList();
		Assert.Equal(2, cells.Count);
		Assert.Equal(new object?[] { "Item", "Price" }, cells.Select(c => c.Content));
		Assert.Same(((GridView)lv.View!).Columns[1], cells[1].Column);
		Assert.Equal(new[] { "Item", "Price" }, Texts(header));
	});

	[Fact]
	public void Each_row_shows_its_DisplayMemberBinding_values_in_column_order() => Run(() =>
	{
		var lv = Make();

		var row = (GridViewRowPresenter)((ListViewItem)lv.ContainerAt(1)!).ViewPresenter!;
		Assert.Equal(new[] { "Book", "12.00" }, Texts(row));
		Assert.Equal(2, row.VisualChildren.Count());
	});

	[Fact]
	public void A_column_without_a_binding_shows_the_item_through_its_CellTemplate() => Run(() =>
	{
		var view = new GridView();
		view.Columns.Add(new GridViewColumn
		{
			Header = "Cell",
			CellTemplate = new DataTemplate { ItemFactory = i => new TextBlock { Text = "<" + ((Item)i!).Name + ">" } },
		});
		var lv = Make(view);

		Assert.Equal(new[] { "<Lamp>" }, Texts(((ListViewItem)lv.ContainerAt(2)!).ViewPresenter!));
	});

	[Fact]
	public void The_header_cell_uses_the_HeaderTemplate_and_the_view_template_as_a_fallback() => Run(() =>
	{
		var view = new GridView
		{
			ColumnHeaderTemplate = new DataTemplate { ItemFactory = h => new TextBlock { Text = "V:" + h } },
		};
		view.Columns.Add(new GridViewColumn { Header = "a" });
		view.Columns.Add(new GridViewColumn
		{
			Header = "b",
			HeaderTemplate = new DataTemplate { ItemFactory = h => new TextBlock { Text = "C:" + h } },
		});
		var lv = Make(view);

		Assert.Equal(new[] { "V:a", "C:b" }, Texts(lv.HeaderElement!));
	});

	[Fact]
	public void An_explicit_column_Width_sizes_the_cells_and_an_automatic_one_joins_a_size_group() => Run(() =>
	{
		var view = TwoColumns();
		view.Columns[0].Width = 140;
		var lv = Make(view);

		var row = (GridViewRowPresenter)((ListViewItem)lv.ContainerAt(0)!).ViewPresenter!;
		Assert.Equal(140, ((FrameworkElement)row.VisualChildren.First()).Width);
		Assert.Equal(140, view.Columns[0].ActualWidth);
		Assert.True(double.IsNaN(((FrameworkElement)row.VisualChildren.Last()).Width));
		var group = view.SizeGroupFor(view.Columns[1]);
		// the header cell plus one cell per row
		Assert.Equal(4, group.Widgets.Length);
	});

	[Fact]
	public void Adding_a_column_after_the_rows_exist_rebuilds_the_header_and_the_rows() => Run(() =>
	{
		var view = TwoColumns();
		var lv = Make(view);

		view.Columns.Add(new GridViewColumn { Header = "Again", DisplayMemberBinding = new Binding("Name") });

		Assert.Equal(3, ((GridViewRowPresenter)lv.HeaderElement!).VisualChildren.Count());
		var row = (GridViewRowPresenter)((ListViewItem)lv.ContainerAt(0)!).ViewPresenter!;
		Assert.Equal(new[] { "Pen", "1.50", "Pen" }, Texts(row));
	});

	[Fact]
	public void Changing_a_DisplayMemberBinding_updates_the_cells() => Run(() =>
	{
		var view = TwoColumns();
		var lv = Make(view);

		view.Columns[0].DisplayMemberBinding = new Binding("Price");

		var row = (GridViewRowPresenter)((ListViewItem)lv.ContainerAt(0)!).ViewPresenter!;
		Assert.Equal("1.5", Texts(row)[0]);
	});

	[Fact]
	public void Setting_View_after_the_widget_exists_adds_the_header_and_swaps_the_row_content() => Run(() =>
	{
		var lv = new ListView { ItemsSource = Items };
		_ = lv.Widget;

		lv.View = TwoColumns();

		Assert.NotNull(lv.HeaderElement);
		Assert.NotNull(((ListViewItem)lv.ContainerAt(0)!).ViewPresenter);
		lv.View = null;
		Assert.Null(lv.HeaderElement);
		Assert.Null(((ListViewItem)lv.ContainerAt(0)!).ViewPresenter);
	});

	[Fact]
	public void Selection_still_maps_rows_to_items() => Run(() =>
	{
		var lv = Make();

		lv.SelectedIndex = 1;

		Assert.Equal("Book", ((Item)lv.SelectedItem!).Name);
	});

	[Fact]
	public void A_GridView_without_columns_shows_an_empty_header_and_rows() => Run(() =>
	{
		var lv = Make(new GridView());

		Assert.Empty(((GridViewRowPresenter)lv.HeaderElement!).VisualChildren);
		Assert.Equal(3, lv.Items.Count);
	});

	[Fact]
	public void GridView_is_a_ViewBase_with_Columns_as_its_content_property()
	{
		Assert.IsAssignableFrom<ViewBase>(new GridView());
		var attr = typeof(GridView).GetCustomAttributes(typeof(System.Windows.Markup.ContentPropertyAttribute), true)
			.Cast<System.Windows.Markup.ContentPropertyAttribute>().Single();
		Assert.Equal("Columns", attr.Name);
		Assert.True(double.IsNaN(new GridViewColumn().Width));
		Assert.True(new GridView().AllowsColumnReorder);
	}
}
