using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Phase-1 tests for <see cref="XamlCodeGenerator"/>: full WPF binding syntax.
/// </summary>
public class XamlCodeGeneratorPhase1Tests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body = "", string attrs = "") =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	{attrs}>
	{body}
</Window>";

	// ── {Binding X} positional path ──────────────────────────────────────────

	[Fact]
	public void Positional_path_emits_ctor_arg()
	{
		string xaml = Window(@"<TextBlock Text=""{Binding Greeting}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains(@"new System.Windows.Data.Binding(""Greeting"")", code);
		// No initializer block needed.
		Assert.DoesNotContain("Mode =", code);
	}

	// ── {Binding Path=X, Mode=TwoWay} named args ────────────────────────────

	[Fact]
	public void Named_Path_and_Mode_emit_ctor_and_initializer()
	{
		string xaml = Window(@"<TextBlock Text=""{Binding Path=Greeting, Mode=TwoWay}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains(@"new System.Windows.Data.Binding(""Greeting"")", code);
		Assert.Contains("Mode = System.Windows.Data.BindingMode.TwoWay", code);
	}

	// ── {Binding ElementName=e, Path=Y} ──────────────────────────────────────

	[Fact]
	public void ElementName_and_Path_emitted()
	{
		string xaml = Window(@"<TextBlock Text=""{Binding ElementName=other, Path=Value}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains(@"new System.Windows.Data.Binding(""Value"")", code);
		Assert.Contains(@"ElementName = ""other""", code);
	}

	// ── {Binding} empty path ─────────────────────────────────────────────────

	[Fact]
	public void Empty_binding_emits_default_ctor()
	{
		string xaml = Window(@"<TextBlock Text=""{Binding}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("new System.Windows.Data.Binding()", code);
		Assert.DoesNotContain("Mode =", code);
		Assert.DoesNotContain("ElementName =", code);
	}

	// ── Converter={StaticResource} now emits real FindResource call ────────

	[Fact]
	public void Converter_StaticResource_emits_FindResource_call()
	{
		string xaml = Window(@"<TextBlock Text=""{Binding Path=X, Converter={StaticResource conv}}"" />");
		string code = _gen.Generate(xaml, out _);
		// Round 7: carry-over removed; real FindResource converter emitted.
		Assert.DoesNotContain("TODO(StaticResource)", code);
		Assert.Contains(@"new System.Windows.Data.Binding(""X"")", code);
		Assert.Contains("IValueConverter", code);
		Assert.Contains(@"StaticResourceScope.Find(""conv"", this", code);
	}

	// ── Mode=OneWay does not add ElementName ──────────────────────────────────

	[Fact]
	public void Mode_only_no_ElementName()
	{
		string xaml = Window(@"<TextBlock Text=""{Binding Path=Foo, Mode=OneWay}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("Mode = System.Windows.Data.BindingMode.OneWay", code);
		Assert.DoesNotContain("ElementName", code);
	}

	// ── Existing Phase-0 test: plain {Binding Greeting} still works ──────────

	[Fact]
	public void Phase0_positional_still_works()
	{
		string xaml = Window(@"<Grid><TextBlock x:Name=""tb"" Text=""{Binding Greeting}"" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("SetBinding(System.Windows.Controls.TextBlock.TextProperty,", code);
		Assert.Contains(@"new System.Windows.Data.Binding(""Greeting"")", code);
	}
}