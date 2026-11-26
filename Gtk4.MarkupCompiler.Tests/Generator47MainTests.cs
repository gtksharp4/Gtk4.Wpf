using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class Generator47MainTests : GeneratorV2TestBase
{
	const string Custom = $@"<lib:Base x:Class=""A.App"" xmlns:lib=""clr-namespace:A.Lib"" {Head} StartupUri=""M.xaml""/>";
	const string LibStub = "namespace A.Lib { public class Base : System.Windows.Application { } }";
	const string AppXaml = $@"<Application x:Class=""A.App"" {Head}/>";

	[Fact]
	public void V5_a_custom_Application_subclass_root_gets_Main_on_its_own_class()
	{
		string code = Gen(Custom, hasCodeBehind: false, sources: new[] { LibStub });
		Assert.Contains("static void Main()", code);
		Assert.Contains("var app = new App();", code);
		Assert.Contains("partial class App : global::A.Lib.Base", code);
		AssertCompiles(code, LibStub);
	}

	[Fact]
	public void V5_a_user_Application_subclass_declared_in_the_sources_is_recognised()
	{
		string code = Gen(Custom.Replace("A.Lib", "A"), hasCodeBehind: false,
			sources: new[] { "namespace A { public class Base : System.Windows.Application { } }" });
		Assert.Contains("static void Main()", code);
	}

	[Fact]
	public void V5_a_non_Application_user_root_gets_no_Main()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}/>");
		Assert.DoesNotContain("Main", code);
	}

	[Fact]
	public void V5_Main_is_not_generated_when_the_project_declares_one()
	{
		string src = "namespace A { public partial class App : System.Windows.Application "
			+ "{ [System.STAThread] public static void Main() { } } }";
		string code = Gen(AppXaml, sources: new[] { src });
		Assert.DoesNotContain("static void Main()", code);
		AssertCompiles(code, src);
	}

	[Fact]
	public void V5_a_Main_in_a_comment_or_string_does_not_count()
	{
		string src = "namespace A { public partial class App : System.Windows.Application "
			+ "{ // static void Main()\n string s = \"static void Main()\"; } }";
		Assert.Contains("static void Main()", Gen(AppXaml, sources: new[] { src }));
	}

	[Fact]
	public void V5_a_plain_Application_still_gets_Main()
	{
		string code = Gen(AppXaml, sources: new[] { "namespace A { public partial class App : System.Windows.Application { } }" });
		Assert.Contains("[System.STAThread]", code);
		Assert.Contains("static void Main()", code);
	}
}
