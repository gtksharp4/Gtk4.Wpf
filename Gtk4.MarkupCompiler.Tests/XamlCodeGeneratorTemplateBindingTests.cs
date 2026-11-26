using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Round-8 compiler tests: <c>{TemplateBinding Property}</c> inside a
/// <c>&lt;ControlTemplate&gt;</c> emits a one-way binding against
/// <c>RelativeSource.TemplatedParent</c>.
/// Each test fails without the TemplateBinding compiler support.
/// </summary>
public class XamlCodeGeneratorTemplateBindingTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body = "") =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";

	// ── IsTemplateBindingExpression unit tests ────────────────────────────

	[Fact]
	public void IsTemplateBindingExpression_parses_simple_property()
	{
		bool result = XamlCodeGenerator.IsTemplateBindingExpression(
			"{TemplateBinding Background}", out string init);
		Assert.True(result);
		Assert.NotNull(init);
	}

	[Fact]
	public void IsTemplateBindingExpression_emits_TemplatedParent_RelativeSource()
	{
		XamlCodeGenerator.IsTemplateBindingExpression(
			"{TemplateBinding Background}", out string init);
		Assert.Contains("RelativeSource.TemplatedParent", init);
	}

	[Fact]
	public void IsTemplateBindingExpression_emits_OneWay_mode()
	{
		XamlCodeGenerator.IsTemplateBindingExpression(
			"{TemplateBinding Background}", out string init);
		Assert.Contains("BindingMode.OneWay", init);
	}

	[Fact]
	public void IsTemplateBindingExpression_includes_property_path()
	{
		XamlCodeGenerator.IsTemplateBindingExpression(
			"{TemplateBinding Foreground}", out string init);
		Assert.Contains("\"Foreground\"", init);
	}

	[Fact]
	public void IsTemplateBindingExpression_returns_false_for_plain_binding()
	{
		bool result = XamlCodeGenerator.IsTemplateBindingExpression(
			"{Binding Background}", out _);
		Assert.False(result);
	}

	[Fact]
	public void IsTemplateBindingExpression_returns_false_for_empty_string()
	{
		bool result = XamlCodeGenerator.IsTemplateBindingExpression(string.Empty, out _);
		Assert.False(result);
	}

	[Fact]
	public void IsTemplateBindingExpression_returns_false_for_null()
	{
		bool result = XamlCodeGenerator.IsTemplateBindingExpression(null, out _);
		Assert.False(result);
	}

	// ── Full code-generation tests ────────────────────────────────────────

	[Fact]
	public void TemplateBinding_inside_ControlTemplate_emits_SetBinding()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border Background=""{TemplateBinding Background}"" />
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("SetBinding(", code);
	}

	[Fact]
	public void TemplateBinding_inside_ControlTemplate_emits_TemplatedParent_source()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border Background=""{TemplateBinding Background}"" />
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("RelativeSource.TemplatedParent", code);
	}

	[Fact]
	public void TemplateBinding_inside_ControlTemplate_emits_OneWay_mode()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border Background=""{TemplateBinding Background}"" />
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("BindingMode.OneWay", code);
	}

	[Fact]
	public void TemplateBinding_inside_ControlTemplate_includes_property_name()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border Background=""{TemplateBinding Background}"" />
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("\"Background\"", code);
	}

	[Fact]
	public void TemplateBinding_control_template_still_builds_element_tree()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border Background=""{TemplateBinding Background}"">
        <TextBlock Text=""hi"" />
      </Border>
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);

		// ControlTemplate construction is still there.
		Assert.Contains("new System.Windows.Controls.ControlTemplate(", code);
		// Border and TextBlock are still emitted.
		Assert.Contains("new System.Windows.Controls.Border()", code);
		Assert.Contains("new System.Windows.Controls.TextBlock()", code);
	}

	[Fact]
	public void TemplateBinding_does_not_affect_regular_binding_emission()
	{
		// A regular {Binding} next to a {TemplateBinding} should still emit normally.
		string xaml = Window(@"
<StackPanel>
  <TextBlock Text=""{Binding Name}"" />
</StackPanel>");
		string code = _gen.Generate(xaml, out _);
		// Regular Binding must still appear without TemplatedParent.
		Assert.Contains("new System.Windows.Data.Binding(\"Name\")", code);
		Assert.DoesNotContain("TemplatedParent", code);
	}

	// ── T7: TemplateBinding with extra comma args uses only the first token ─

	[Fact]
	public void T7_TemplateBinding_with_comma_extra_args_uses_only_property_path()
	{
		// {TemplateBinding Prop, Converter=…} — the fix takes only "Prop" as the path,
		// not the whole comma-containing string.
		bool result = XamlCodeGenerator.IsTemplateBindingExpression(
			"{TemplateBinding Background, Converter={StaticResource c}}", out string init);

		Assert.True(result);
		// Path must be "Background" only, not "Background, Converter=…"
		Assert.Contains("\"Background\"", init);
		Assert.DoesNotContain(",", init.Split('"')[1]); // first quoted token has no comma
	}

	[Fact]
	public void T7_TemplateBinding_simple_property_unaffected()
	{
		// Without a comma the behaviour is unchanged.
		XamlCodeGenerator.IsTemplateBindingExpression(
			"{TemplateBinding FontSize}", out string init);
		Assert.Contains("\"FontSize\"", init);
	}
}