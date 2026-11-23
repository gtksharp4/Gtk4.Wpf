using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>Shared helpers of the sweep III generator tests: compile-checked generation with a code-behind stub.</summary>
public abstract class Sweep3Base : GeneratorV2TestBase
{
	protected const string Stub = "namespace App { public partial class C { } }";

	protected static string Page(string body, string resources = "", string attrs = "")
		=> "<Page " + Head + " x:Class=\"App.C\" " + attrs + ">" + (resources.Length == 0 ? ""
			: "<Page.Resources>" + resources + "</Page.Resources>") + body + "</Page>";

	protected static void Has(string code, string s) => Assert.True(code.Contains(s), "missing " + s + " in " + code);

	protected static void Lacks(string code, string s) => Assert.False(code.Contains(s), "has " + s + " in " + code);

	/// <summary>Generates with <paramref name="stub"/> as the code-behind and asserts the result compiles.</summary>
	protected static string Compile(string xaml, string stub = Stub)
	{
		string code = Gen(xaml, sources: new[] { stub });
		AssertCompiles(code, stub);
		return code;
	}
}
