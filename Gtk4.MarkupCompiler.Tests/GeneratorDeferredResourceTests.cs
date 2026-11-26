using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

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

	[Fact]
	public void BasedOn_a_type_key_tolerates_a_missing_default_style()
	{
		const string stub = "namespace App { public partial class C { } }";
		string code = Gen("<Page " + Head + @" x:Class=""App.C""><Page.Resources>
			<Style TargetType=""{x:Type Button}"" BasedOn=""{StaticResource {x:Type Button}}""/>
			<Style x:Key=""K"" TargetType=""Button"" BasedOn=""{StaticResource Other}""/></Page.Resources>
			<Button/></Page>", sources: new[] { stub });

		Assert.Contains("TryFindResource(typeof(", code);
		Assert.Matches(@"Find(Resource)?[(]""Other""", code);
		AssertCompiles(code, stub);
	}

	[Fact]
	public void Unshared_resource_is_deferred_and_built_per_lookup()
	{
		const string stub = "namespace App { public partial class C { } }";
		string code = Gen("<Page " + Head + @" x:Class=""App.C""><Page.Resources>
			<ModelVisual3D x:Key=""Cube"" x:Shared=""False""/></Page.Resources><Button/></Page>",
			sources: new[] { stub });

		Assert.Contains("SetDeferred(\"Cube\", () =>", code);
		Assert.Contains("}, false);", code);
		AssertCompiles(code, stub);
	}
}
