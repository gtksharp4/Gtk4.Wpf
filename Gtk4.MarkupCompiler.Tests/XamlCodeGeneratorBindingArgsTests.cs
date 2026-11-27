using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Binding markup arguments beyond the core set: parameter, culture, source, validation flags.</summary>
public class XamlCodeGeneratorBindingArgsTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:local=""clr-namespace:MyApp"">
	{body}
</Window>";

	string Gen(string binding) =>
		_gen.Generate(Window($@"<TextBlock Text=""{binding}"" />"), out _);

	[Fact]
	public void ConverterParameter_literal_is_a_string()
		=> Assert.Contains(@"ConverterParameter = ""abc""", Gen("{Binding Age, ConverterParameter=abc}"));

	[Fact]
	public void ConverterParameter_quoted_literal_is_unquoted()
		=> Assert.Contains(@"ConverterParameter = ""a, b""", Gen("{Binding Age, ConverterParameter='a, b'}"));

	[Fact]
	public void ConverterParameter_XStatic_emits_the_member()
		=> Assert.Contains("ConverterParameter = global::MyApp.Consts.Limit",
			Gen("{Binding Age, ConverterParameter={x:Static local:Consts.Limit}}"));

	[Fact]
	public void ConverterParameter_StaticResource_emits_FindResource()
		=> Assert.Contains(@"ConverterParameter = System.Windows.Markup.StaticResourceScope.Find(""p"", this",
			Gen("{Binding Age, ConverterParameter={StaticResource p}}"));

	[Fact]
	public void ConverterCulture_emits_GetCultureInfo()
		=> Assert.Contains(@"ConverterCulture = System.Globalization.CultureInfo.GetCultureInfo(""de-DE"")",
			Gen("{Binding Age, ConverterCulture=de-DE}"));

	[Fact]
	public void Source_StaticResource_emits_FindResource()
		=> Assert.Contains(@"Source = System.Windows.Markup.StaticResourceScope.Find(""src"", this", Gen("{Binding Age, Source={StaticResource src}}"));

	[Fact]
	public void Source_XStatic_emits_the_member()
		=> Assert.Contains("Source = global::MyApp.Defaults.Model",
			Gen("{Binding Age, Source={x:Static local:Defaults.Model}}"));

	[Theory]
	[InlineData("ValidatesOnDataErrors")]
	[InlineData("ValidatesOnExceptions")]
	[InlineData("ValidatesOnNotifyDataErrors")]
	[InlineData("NotifyOnValidationError")]
	[InlineData("BindsDirectlyToSource")]
	public void Validation_flags_emit_bool(string flag)
	{
		Assert.Contains(flag + " = true", Gen("{Binding Age, " + flag + "=True}"));
		Assert.Contains(flag + " = false", Gen("{Binding Age, " + flag + "=False}"));
	}

	[Fact]
	public void Nested_braces_stay_intact_with_following_args()
	{
		string code = Gen("{Binding Age, Source={StaticResource src}, Mode=TwoWay, ValidatesOnExceptions=True}");
		Assert.Contains(@"Source = System.Windows.Markup.StaticResourceScope.Find(""src"", this", code);
		Assert.Contains("Mode = System.Windows.Data.BindingMode.TwoWay", code);
		Assert.Contains("ValidatesOnExceptions = true", code);
	}

	[Fact]
	public void SplitBindingArgs_keeps_nested_braces()
	{
		var parts = XamlCodeGenerator.SplitBindingArgs("A={x:Static l:T.M, q}, B=1");
		Assert.Equal(2, parts.Count);
		Assert.Equal("A={x:Static l:T.M, q}", parts[0]);
	}

	[Fact]
	public void Existing_binding_output_is_unchanged()
	{
		string code = Gen("{Binding Age, Mode=TwoWay, StringFormat=N2}");
		Assert.Contains(@"new System.Windows.Data.Binding(""Age"") { Mode = System.Windows.Data.BindingMode.TwoWay, "
			+ @"StringFormat = ""N2"" }", code);
	}

	[Fact]
	public void Plain_binding_without_extras_has_no_initializer()
		=> Assert.Contains(@"new System.Windows.Data.Binding(""Age"")", Gen("{Binding Age}"));
}
