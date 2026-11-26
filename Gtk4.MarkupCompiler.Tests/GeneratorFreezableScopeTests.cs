using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class GeneratorFreezableScopeTests : GeneratorV2TestBase
{
	const string WindowStub = "namespace A { public partial class W : System.Windows.Window { } }";

	[Fact]
	public void DynamicResource_on_a_Freezable_passes_the_enclosing_element_as_a_scope()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Grid><Grid.Resources>
<Color x:Key=""c"">Red</Color><SolidColorBrush x:Key=""b"" Color=""{{DynamicResource c}}""/>
</Grid.Resources></Grid></Window>");
		var line = System.Array.Find(code.Split('\n'), l => l.Contains("XamlValueConverter.SetResourceValue("));
		Assert.NotNull(line);
		Assert.Matches(@"ColorProperty, ""c"", (__e\d+)\);", line);
		var gridVar = System.Text.RegularExpressions.Regex.Match(line, @"""c"", (__e\d+)\)").Groups[1].Value;
		Assert.Contains($"var {gridVar} = new System.Windows.Controls.Grid();", code);
		AssertCompiles(code, WindowStub);
	}
}
