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

	[Fact]
	public void Named_MatrixTransform_render_transform_is_built_and_registered()
	{
		string xaml = Win("<Canvas><Rectangle><Rectangle.RenderTransform><MatrixTransform x:Name=\"M\"/>"
			+ "</Rectangle.RenderTransform></Rectangle></Canvas>");
		string code = Compile(xaml);

		Has(code, "new System.Windows.Media.MatrixTransform()");
		Has(code, "RegisterName(\"M\"");
		Assert.DoesNotContain(Messages(xaml), m => m.StartsWith("Skipped"));
	}

	[Fact]
	public void Combined_geometry_path_data_is_built_with_its_children()
	{
		string xaml = Win("<Path Stroke=\"Black\"><Path.Data><CombinedGeometry GeometryCombineMode=\"Exclude\">"
			+ "<CombinedGeometry.Geometry1><EllipseGeometry RadiusX=\"50\" RadiusY=\"50\" Center=\"75,75\"/>"
			+ "</CombinedGeometry.Geometry1><CombinedGeometry.Geometry2><EllipseGeometry RadiusX=\"50\""
			+ " RadiusY=\"50\" Center=\"125,75\"/></CombinedGeometry.Geometry2></CombinedGeometry></Path.Data></Path>");
		string code = Compile(xaml);

		Has(code, "new System.Windows.Media.CombinedGeometry()");
		Has(code, "GeometryCombineMode");
		Has(code, ".Geometry1 = ");
		Has(code, ".Geometry2 = ");
		Assert.DoesNotContain(Messages(xaml), m => m.StartsWith("Skipped"));
	}

	[Fact]
	public void Combined_geometry_exists_at_run_time_with_its_mode_and_children()
	{
		object root = GeneratorSweep4RuntimeTests.Run("<Path " + Head + " x:Class=\"App.C\"><Path.Data>"
			+ "<CombinedGeometry GeometryCombineMode=\"Xor\"><CombinedGeometry.Geometry1>"
			+ "<EllipseGeometry RadiusX=\"5\" RadiusY=\"5\"/></CombinedGeometry.Geometry1>"
			+ "<CombinedGeometry.Geometry2><RectangleGeometry Rect=\"0,0,4,4\"/></CombinedGeometry.Geometry2>"
			+ "</CombinedGeometry></Path.Data></Path>");
		object data = GeneratorSweep4RuntimeTests.Prop(root, "Data");

		Assert.Equal("CombinedGeometry", data.GetType().Name);
		Assert.Equal("Xor", GeneratorSweep4RuntimeTests.Prop(data, "GeometryCombineMode").ToString());
		Assert.Equal("EllipseGeometry", GeneratorSweep4RuntimeTests.Prop(data, "Geometry1").GetType().Name);
		Assert.Equal("RectangleGeometry", GeneratorSweep4RuntimeTests.Prop(data, "Geometry2").GetType().Name);
	}

	const string Priority = "<TextBlock Width=\"100\"><TextBlock.Text><PriorityBinding FallbackValue=\"defaultvalue\">"
		+ "<Binding Path=\"SlowestDp\" IsAsync=\"True\"/><Binding Path=\"FastDp\"/></PriorityBinding></TextBlock.Text>"
		+ "</TextBlock>";

	[Fact]
	public void PriorityBinding_property_element_is_built_and_bound()
	{
		string xaml = Win(Priority);
		string code = Compile(xaml);

		Has(code, "new System.Windows.Data.PriorityBinding()");
		Has(code, ".FallbackValue = \"defaultvalue\"");
		Has(code, "Bindings.Add(");
		Has(code, "TextBlock.TextProperty, __pb");
		Assert.DoesNotContain(Messages(xaml), m => m.StartsWith("Skipped"));
	}

	[Fact]
	public void PriorityBinding_is_bound_and_shows_the_fallback_at_run_time()
	{
		object root = GeneratorSweep4RuntimeTests.Run("<StackPanel " + Head + " x:Class=\"App.C\">" + Priority
			+ "</StackPanel>");
		object text = ((System.Collections.IEnumerable)GeneratorSweep4RuntimeTests.Prop(root, "Children"))
			.Cast<object>().First();
		object dp = text.GetType().GetField("TextProperty")!.GetValue(null)!;
		object expr = text.GetType().Assembly.GetType("System.Windows.Data.BindingOperations")!
			.GetMethod("GetBindingExpressionBase")!.Invoke(null, new[] { text, dp })!;
		Assert.Equal("PriorityBindingExpression", expr.GetType().Name);
		Assert.Equal("defaultvalue", GeneratorSweep4RuntimeTests.Prop(text, "Text"));
	}
}
