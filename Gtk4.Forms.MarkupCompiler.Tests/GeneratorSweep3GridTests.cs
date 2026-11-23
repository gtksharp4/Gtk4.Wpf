using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>Sweep III: the .NET 9 Grid RowDefinitions/ColumnDefinitions attribute shorthand.</summary>
public class GeneratorSweep3GridTests : Sweep3Base
{
	[Fact]
	public void Row_and_column_shorthand_add_one_definition_per_token()
	{
		string code = Compile(Page("<Grid RowDefinitions=\"Auto,*,Auto\" ColumnDefinitions=\"100, 2*  *\"/>"));

		Assert.Equal(3, code.Split(new[] { ".RowDefinitions.Add(" }, System.StringSplitOptions.None).Length - 1);
		Assert.Equal(3, code.Split(new[] { ".ColumnDefinitions.Add(" }, System.StringSplitOptions.None).Length - 1);
		Has(code, "\"2*\"");
		Lacks(code, "RowDefinitions = ");
	}

	[Fact]
	public void Shorthand_can_be_combined_with_children_and_attached_properties()
	{
		string code = Compile(Page("<Grid RowDefinitions=\"*,*\"><Button Grid.Row=\"1\"/></Grid>"));

		Has(code, ".RowDefinitions.Add(");
		Has(code, "Grid.SetRow(");
	}
}
