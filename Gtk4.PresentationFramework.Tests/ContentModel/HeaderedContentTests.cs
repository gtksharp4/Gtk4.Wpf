using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ContentModel;

/// <summary>Headers of Expander, GroupBox, TabItem, TreeViewItem and DataGrid columns use the shared resolution.</summary>
public class HeaderedContentTests : ContentModelTestBase
{
	[Fact]
	public void Expander_shows_a_TextBlock_header_as_its_label_widget() => Run(() =>
	{
		var header = new TextBlock { Text = "Animation" };
		var e = new Expander { Header = header };
		Assert.Same(header.Widget, ((Gtk.Expander)e.Widget).LabelWidget);
		Assert.True(e.HasHeader);
		Assert.Contains("Animation", Texts(e));
	});

	[Fact]
	public void Expander_keeps_the_mnemonic_for_a_string_header_and_its_content_hosting() => Run(() =>
	{
		var e = new Expander { Header = "_Open", Content = new TextBlock { Text = "body" } };
		var native = (Gtk.Expander)e.Widget;
		Assert.True(native.UseUnderline);
		Assert.Equal("_Open", native.Label);
		e.IsExpanded = true;
		Assert.True(native.Expanded);
		Assert.Contains("body", Texts(e));
		e.Content = "plain body";
		Assert.Contains("plain body", Texts(e));
	});

	[Fact]
	public void Expander_applies_the_header_template_and_string_format() => Run(() =>
	{
		var e = new Expander { Header = new Place { Name = "H" }, HeaderTemplate = Template("h:") };
		Assert.Contains("h:H", Texts(e));
		var f = new Expander { Header = 7, HeaderStringFormat = "n{0}" };
		Assert.Equal("n7", ((Gtk.Expander)f.Widget).Label);
	});

	[Fact]
	public void Expander_finds_an_implicit_template_for_a_data_header() => Run(() =>
	{
		var panel = new StackPanel();
		panel.Resources[new DataTemplateKey(typeof(Place))] = Template("imp:");
		var e = new Expander { Header = new Place { Name = "K" } };
		panel.Children.Add(e);
		Assert.Contains("imp:K", Texts(e));
	});

	[Fact]
	public void GroupBox_shows_an_element_header_and_a_template_header() => Run(() =>
	{
		var header = new TextBlock { Text = "grp" };
		var g = new GroupBox { Header = header, Content = "body" };
		Assert.Same(header.Widget, ((Gtk.Frame)g.Widget).LabelWidget);
		Assert.Contains("body", Texts(g));
		var t = new GroupBox { Header = new Place { Name = "G" }, HeaderTemplate = Template("g:") };
		Assert.Contains("g:G", Texts(t));
	});

	[Fact]
	public void TabItem_hosts_an_element_header_in_the_tab_widget() => Run(() =>
	{
		var header = new TextBlock { Text = "tab" };
		var item = new TabItem { Header = header, Content = new TextBlock { Text = "page" } };
		var tab = item.CreateTabWidget();
		Assert.True(IsInside(tab, header.Widget));
		Assert.Contains("page", Texts(item));
		item.Header = "text";
		Assert.DoesNotContain(header.Widget, Descendants(tab));
		Assert.Contains(Descendants(tab).OfType<Gtk.Label>(), l => l.Text == "text");
	});

	[Fact]
	public void TreeViewItem_hosts_an_element_header() => Run(() =>
	{
		var header = new TextBlock { Text = "node" };
		var tvi = new TreeViewItem { Header = header };
		Assert.Same(header.Widget, ((Gtk.Expander)tvi.Widget).LabelWidget);
		Assert.True(tvi.HasHeader);
		var t = new TreeViewItem { Header = new Place { Name = "T" }, HeaderTemplate = Template("n:") };
		Assert.Contains("n:T", Texts(t));
	});

	[Fact]
	public void DataGrid_column_header_hosts_element_and_template_headers() => Run(() =>
	{
		var grid = new DataGrid { AutoGenerateColumns = false, ItemsSource = new[] { new Place { Name = "a" } } };
		grid.Columns.Add(new DataGridTextColumn { Header = new TextBlock { Text = "ElemHeader" }, Binding = new System.Windows.Data.Binding("Name") });
		grid.Columns.Add(new DataGridTextColumn { Header = new Place { Name = "x" }, HeaderTemplate = Template("hd:"), Binding = new System.Windows.Data.Binding("Name") });
		grid.Columns.Add(new DataGridTextColumn { Header = 5, HeaderStringFormat = "#{0}", Binding = new System.Windows.Data.Binding("Name") });
		var texts = Texts(grid);
		Assert.Contains("ElemHeader", texts);
		Assert.Contains("hd:x", texts);
		Assert.Contains("#5", texts);
	});

	[Fact]
	public void MenuItem_projects_an_element_header_to_its_text() => Run(() =>
	{
		var mi = new MenuItem { Header = new TextBlock { Text = "Open" } };
		Assert.Equal("Open", ((Gtk.Button)mi.Widget).Label);
	});

	[Fact]
	public void ComboBoxItem_text_uses_the_string_rules() => Run(() =>
	{
		Assert.Equal("5", ComboBoxItemText.GetText(new ComboBoxItem { Content = 5 }, null));
		Assert.Equal("n5", ComboBoxItemText.GetText(new ComboBoxItem { Content = 5, ContentStringFormat = "n{0}" }, null));
	});
}
