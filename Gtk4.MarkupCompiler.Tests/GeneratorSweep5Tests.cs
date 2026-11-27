using System.Linq;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Sweep V: property elements that used to be skipped.</summary>
public class GeneratorSweep5Tests : Sweep3Base
{
	static string Win(string body) => "<Window " + Head + " x:Class=\"App.C\">" + body + "</Window>";

	const string Grid =
		"<DataGrid AutoGenerateColumns=\"False\"><DataGrid.Columns>"
		+ "<DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\"/>"
		+ "<DataGridTextColumn Header=\"Cost\"><DataGridTextColumn.Binding><Binding Path=\"Cost\""
		+ " UpdateSourceTrigger=\"PropertyChanged\"/></DataGridTextColumn.Binding></DataGridTextColumn>"
		+ "</DataGrid.Columns></DataGrid>";

	[Fact]
	public void DataGrid_columns_are_built_and_added()
	{
		string code = Compile(Win(Grid));

		Has(code, "Columns.Add(");
		Lacks(code, "SetBinding(System.Windows.Controls.DataGridTextColumn.BindingProperty");
		Lacks(code, "TrySetBinding");
		Assert.DoesNotContain(Messages(Win(Grid)), m => m.StartsWith("Skipped"));
	}

	[Fact]
	public void Column_binding_is_assigned_not_bound()
	{
		string code = Compile(Win(Grid));

		Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(code, @"\.Binding = ").Count);
	}

	// Binding path of the i-th column of the grid at the root's first child.
	static string[] ColumnPaths(object root)
	{
		object grid = ((System.Collections.IEnumerable)GeneratorSweep4RuntimeTests.Prop(root, "Children")).Cast<object>().First();
		var cols = (System.Collections.IEnumerable)GeneratorSweep4RuntimeTests.Prop(grid, "Columns");
		return cols.Cast<object>().Select(c => (string)GeneratorSweep4RuntimeTests.Prop(
			GeneratorSweep4RuntimeTests.Prop(GeneratorSweep4RuntimeTests.Prop(c, "Binding"), "Path"), "Path")).ToArray();
	}

	[Fact]
	public void DataGrid_columns_exist_at_run_time_with_their_bindings()
	{
		object root = GeneratorSweep4RuntimeTests.Run(
			"<StackPanel " + Head + " x:Class=\"App.C\">" + Grid + "</StackPanel>");

		Assert.Equal(new[] { "Name", "Cost" }, ColumnPaths(root));
	}

	const string Bindings =
		"<Grid.InputBindings><KeyBinding Command=\"ApplicationCommands.Open\" Gesture=\"CTRL+R\"/>"
		+ "<KeyBinding Command=\"ApplicationCommands.Close\" Key=\"F4\" Modifiers=\"Alt\" CommandParameter=\"p\"/>"
		+ "<MouseBinding Command=\"ApplicationCommands.Open\" MouseAction=\"LeftDoubleClick\"/></Grid.InputBindings>";

	[Fact]
	public void InputBindings_are_emitted_and_compile()
	{
		string xaml = Win("<Grid>" + Bindings + "</Grid>");
		string code = Compile(xaml);

		Has(code, "InputBindings.Add(new global::System.Windows.Input.KeyBinding(");
		Has(code, "ModifierKeys.Control");
		Has(code, "CommandParameter = \"p\"");
		Has(code, "new global::System.Windows.Input.MouseBinding(");
		Assert.DoesNotContain(Messages(xaml), m => m.StartsWith("Skipped"));
	}

	[Fact]
	public void Window_input_bindings_are_emitted()
	{
		string xaml = "<Window " + Head + " x:Class=\"App.C\"><Window.InputBindings>"
			+ "<KeyBinding Command=\"ApplicationCommands.Open\" Gesture=\"CTRL+R\"/></Window.InputBindings></Window>";

		Has(Compile(xaml), "this.InputBindings.Add(");
	}

	[Fact]
	public void An_unknown_gesture_is_still_reported_not_guessed()
	{
		string xaml = Win("<Grid><Grid.InputBindings><KeyBinding Command=\"ApplicationCommands.Open\""
			+ " Gesture=\"Hyper+R\"/></Grid.InputBindings></Grid>");

		Assert.Contains(Messages(xaml), m => m.StartsWith("Skipped property element Grid.InputBindings"));
	}

	[Fact]
	public void Input_bindings_exist_at_run_time_with_their_gestures()
	{
		object root = GeneratorSweep4RuntimeTests.Run(
			"<Grid " + Head + " x:Class=\"App.C\">" + Bindings + "</Grid>");
		var list = ((System.Collections.IEnumerable)GeneratorSweep4RuntimeTests.Prop(root, "InputBindings"))
			.Cast<object>().ToList();

		Assert.Equal(3, list.Count);
		object g0 = GeneratorSweep4RuntimeTests.Prop(list[0], "Gesture");
		Assert.Equal("R", GeneratorSweep4RuntimeTests.Prop(g0, "Key").ToString());
		Assert.Equal("Control", GeneratorSweep4RuntimeTests.Prop(g0, "Modifiers").ToString());
		object g1 = GeneratorSweep4RuntimeTests.Prop(list[1], "Gesture");
		Assert.Equal("F4", GeneratorSweep4RuntimeTests.Prop(g1, "Key").ToString());
		Assert.Equal("Alt", GeneratorSweep4RuntimeTests.Prop(g1, "Modifiers").ToString());
		Assert.Equal("p", GeneratorSweep4RuntimeTests.Prop(list[1], "CommandParameter"));
		Assert.Equal("LeftDoubleClick", GeneratorSweep4RuntimeTests.Prop(
			GeneratorSweep4RuntimeTests.Prop(list[2], "Gesture"), "MouseAction").ToString());
	}

	const string Panel = "<ListBox x:Name=\"L\"><ListBox.ItemsPanel><ItemsPanelTemplate>"
		+ "<WrapPanel Orientation=\"Horizontal\" Margin=\"10\"/></ItemsPanelTemplate></ListBox.ItemsPanel></ListBox>";

	[Fact]
	public void ItemsPanel_template_gets_a_panel_factory()
	{
		string xaml = Win(Panel);
		string code = Compile(xaml);

		Has(code, "new System.Windows.Controls.ItemsPanelTemplate()");
		Has(code, ".Factory = () =>");
		Has(code, ".ItemsPanel = __ip");
		Assert.DoesNotContain(Messages(xaml), m => m.StartsWith("Skipped"));
	}

	[Fact]
	public void ItemsPanel_factory_builds_a_fresh_panel_per_call()
	{
		object root = GeneratorSweep4RuntimeTests.Run("<StackPanel " + Head + " x:Class=\"App.C\">" + Panel
			+ "</StackPanel>");
		object list = ((System.Collections.IEnumerable)GeneratorSweep4RuntimeTests.Prop(root, "Children"))
			.Cast<object>().First();
		object template = GeneratorSweep4RuntimeTests.Prop(list, "ItemsPanel");
		object f = template.GetType().GetProperty("Factory")!.GetValue(template)!;
		object a = ((System.Delegate)f).DynamicInvoke()!;
		object b = ((System.Delegate)f).DynamicInvoke()!;

		Assert.Equal("WrapPanel", a.GetType().Name);
		Assert.NotSame(a, b);
		Assert.Equal("Horizontal", GeneratorSweep4RuntimeTests.Prop(a, "Orientation").ToString());
	}
}
