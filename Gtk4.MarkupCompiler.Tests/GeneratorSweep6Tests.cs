using System.Collections;
using System.Linq;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Round 78: the item-view property elements (ListView.View, GroupStyle, CompositeCollection).</summary>
public class GeneratorSweep6Tests : Sweep3Base
{
	static string Win(string body, string resources = "") => Page(body, resources);

	static string Root(string body) => "<StackPanel " + Head + " x:Class=\"App.C\">" + body + "</StackPanel>";

	static object First(object panel) => ((IEnumerable)GeneratorSweep4RuntimeTests.Prop(panel, "Children"))
		.Cast<object>().First();

	static object P(object o, string name) => GeneratorSweep4RuntimeTests.Prop(o, name);

	const string Grid = "<ListView x:Name=\"L\"><ListView.View><GridView>"
		+ "<GridViewColumn Header=\"Item\" Width=\"120\" DisplayMemberBinding=\"{Binding Path=Description}\"/>"
		+ "<GridViewColumn Header=\"Price\" DisplayMemberBinding=\"{Binding Path=Price, StringFormat=c}\"/>"
		+ "<GridViewColumn><GridViewColumn.Header><TextBlock Text=\"Tpl\"/></GridViewColumn.Header>"
		+ "<GridViewColumn.CellTemplate><DataTemplate><TextBlock Text=\"{Binding Path=Name}\"/></DataTemplate>"
		+ "</GridViewColumn.CellTemplate></GridViewColumn>"
		+ "</GridView></ListView.View></ListView>";

	[Fact]
	public void ListView_View_GridView_is_emitted_and_compiles()
	{
		string xaml = Win(Grid);
		string code = Compile(xaml);

		Has(code, "new System.Windows.Controls.GridView()");
		Has(code, "new System.Windows.Controls.GridViewColumn()");
		Has(code, "ListView.ViewProperty");
		Assert.DoesNotContain(Messages(xaml), m => m.StartsWith("Skipped"));
	}

	[Fact]
	public void GridView_columns_exist_at_run_time_with_their_members()
	{
		object root = GeneratorSweep4RuntimeTests.Run(Root(Grid));
		object view = P(First(root), "View");
		var columns = ((IEnumerable)P(view, "Columns")).Cast<object>().ToList();

		Assert.Equal("GridView", view.GetType().Name);
		Assert.Equal(3, columns.Count);
		Assert.Equal("Item", P(columns[0], "Header"));
		Assert.Equal(120d, P(columns[0], "Width"));
		Assert.Equal("Binding", P(columns[0], "DisplayMemberBinding").GetType().Name);
		Assert.Equal("c", P(P(columns[1], "DisplayMemberBinding"), "StringFormat"));
		Assert.Equal("TextBlock", P(columns[2], "Header").GetType().Name);
		Assert.Equal("DataTemplate", P(columns[2], "CellTemplate").GetType().Name);
	}

	const string Groups = "<ListBox x:Name=\"L\"><ListBox.GroupStyle>"
		+ "<GroupStyle HidesIfEmpty=\"True\"><GroupStyle.HeaderTemplate><DataTemplate>"
		+ "<TextBlock Text=\"{Binding Path=Name}\"/></DataTemplate></GroupStyle.HeaderTemplate></GroupStyle>"
		+ "<x:Static Member=\"GroupStyle.Default\"/></ListBox.GroupStyle></ListBox>";

	[Fact]
	public void GroupStyle_collection_is_emitted_with_templates_and_the_static_default()
	{
		string xaml = Win(Groups);
		string code = Compile(xaml);

		Has(code, "new System.Windows.Controls.GroupStyle()");
		Has(code, ".GroupStyle.Add(global::System.Windows.Controls.GroupStyle.Default);");
		Assert.DoesNotContain(Messages(xaml), m => m.StartsWith("Skipped"));
	}

	[Fact]
	public void GroupStyle_exists_at_run_time_with_its_header_template()
	{
		object root = GeneratorSweep4RuntimeTests.Run(Root(Groups));
		var styles = ((IEnumerable)P(First(root), "GroupStyle")).Cast<object>().ToList();

		Assert.Equal(2, styles.Count);
		Assert.Equal(true, P(styles[0], "HidesIfEmpty"));
		Assert.Equal("DataTemplate", P(styles[0], "HeaderTemplate").GetType().Name);
		Assert.Same(styles[1].GetType().GetProperty("Default")!.GetValue(null), styles[1]);
	}

	[Fact]
	public void GroupStyle_on_an_ItemsControl_takes_a_header_template_resource()
	{
		string xaml = Win("<ItemsControl x:Name=\"I\"><ItemsControl.GroupStyle><GroupStyle"
			+ " HeaderTemplate=\"{StaticResource H}\"/></ItemsControl.GroupStyle></ItemsControl>",
			"<DataTemplate x:Key=\"H\"><TextBlock Text=\"{Binding Name}\"/></DataTemplate>");
		string code = Compile(xaml);

		Has(code, "new System.Windows.Controls.GroupStyle()");
		Has(code, ".HeaderTemplate = ");
	}

	const string Composite = "<ListBox x:Name=\"L\"><ListBox.ItemsSource><CompositeCollection>"
		+ "<CollectionContainer Collection=\"{Binding Source={StaticResource Data}}\"/>"
		+ "<ListBoxItem Foreground=\"Red\">Other 1</ListBoxItem>"
		+ "<CollectionContainer Collection=\"{Binding Items}\"/>"
		+ "</CompositeCollection></ListBox.ItemsSource></ListBox>";

	[Fact]
	public void CompositeCollection_is_emitted_with_containers_and_items()
	{
		string xaml = Win(Composite, "<SolidColorBrush x:Key=\"Data\" Color=\"Red\"/>");
		string code = Compile(xaml);

		Has(code, "new System.Windows.Data.CompositeCollection()");
		Has(code, "new System.Windows.Data.CollectionContainer()");
		Assert.DoesNotContain(Messages(xaml), m => m.StartsWith("Skipped"));
	}

	[Fact]
	public void CompositeCollection_exists_at_run_time_as_the_ItemsSource()
	{
		const string body = "<ListBox x:Name=\"L\"><ListBox.ItemsSource><CompositeCollection>"
			+ "<CollectionContainer Collection=\"{Binding Path=Items}\"/><ListBoxItem>Other 1</ListBoxItem>"
			+ "<ListBoxItem>Other 2</ListBoxItem></CompositeCollection></ListBox.ItemsSource></ListBox>";
		object root = GeneratorSweep4RuntimeTests.Run(Root(body));
		object source = P(First(root), "ItemsSource");

		Assert.Equal("CompositeCollection", source.GetType().Name);
		Assert.Equal(3, P(source, "Count")); // entries: the container and two items
	}
}
