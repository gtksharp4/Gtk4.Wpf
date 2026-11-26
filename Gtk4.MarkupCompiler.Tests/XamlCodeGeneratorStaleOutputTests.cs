using System.IO;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class XamlCodeGeneratorStaleOutputTests : GeneratorV2TestBase
{
	const string Stale =
		"public partial class X : global::System.Windows.Window { public X() { InitializeComponent(); } }";

	[Fact]
	public void A_previous_generated_file_is_not_treated_as_code_behind()
	{
		string root = Path.Combine(Path.GetTempPath(), "gf45-" + Path.GetRandomFileName());
		string obj = Path.Combine(root, "obj");
		Directory.CreateDirectory(obj);
		try
		{
			string stale = Path.Combine(obj, "X.g.cs");
			string real = Path.Combine(root, "Real.cs");
			File.WriteAllText(stale, Stale);
			File.WriteAllText(real, "class Other { }");

			string[] sources = CodeBehindScan.ReadSources(new[] { stale, real }, obj);

			Assert.Equal(new[] { "class Other { }" }, sources);
			string code = Gen($@"<Window x:Class=""N.X"" {Head}/>", sources: sources);
			Assert.Contains("public partial class X : global::System.Windows.Window {", code);
			Assert.Contains("public X() { InitializeComponent(); }", code);
		}
		finally
		{
			Directory.Delete(root, true);
		}
	}
}
