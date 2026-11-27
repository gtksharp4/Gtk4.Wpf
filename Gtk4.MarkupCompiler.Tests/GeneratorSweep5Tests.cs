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
}
