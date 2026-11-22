using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>A dictionary value holding {StaticResource} is deferred to first lookup (ExpenseItIntro).</summary>
public class GeneratorDeferredResourceTests : GeneratorV2TestBase
{
	const string Dict = "<ResourceDictionary " + Head + @">
	<Style x:Key=""Fwd"" TargetType=""Button"">
		<Setter Property=""FocusVisualStyle"" Value=""{StaticResource NoSuchKey}"" />
	</Style>
	<SolidColorBrush x:Key=""Plain"" Color=""Red"" />
</ResourceDictionary>";

	[Fact]
	public void Style_with_a_static_resource_is_deferred_and_still_compiles()
	{
		string code = Gen(Dict, out _, false, xamlPath: "D.xaml", rootNamespace: "App");

		Assert.Contains(".Resources.SetDeferred(\"Fwd\", () =>", code);
		Assert.Contains("return __style", code);
		Assert.DoesNotContain("Resources[\"Fwd\"]", code);
		Assert.Contains("CopyEntriesFrom(__owner.Resources)", code);
		AssertCompiles(code);
	}

	[Fact]
	public void Value_without_a_static_resource_stays_eager()
	{
		string code = Gen(Dict, out _, false, xamlPath: "D.xaml", rootNamespace: "App");

		Assert.Contains("Resources[\"Plain\"] =", code);
	}
}
