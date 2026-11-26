using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Round-6 compiler tests: &lt;ControlTemplate&gt;, &lt;DataTemplate&gt; parsing and
/// factory emission, keyed template resources, and Template={StaticResource} wiring.
/// Each test fails without the template-emission feature.
/// </summary>
public class XamlCodeGeneratorTemplateTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body = "") =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";

	// ── <Button.Template><ControlTemplate…> inline ──────────────────────────

	[Fact]
	public void Inline_ControlTemplate_emits_ControlTemplate_construction()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border>
        <TextBlock Text=""X"" />
      </Border>
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Controls.ControlTemplate(", code);
	}

	[Fact]
	public void Inline_ControlTemplate_emits_Factory_lambda()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border />
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Factory = () =>", code);
	}

	[Fact]
	public void Inline_ControlTemplate_factory_builds_Border_root()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border>
        <TextBlock Text=""X"" />
      </Border>
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Controls.Border()", code);
		Assert.Contains("new System.Windows.Controls.TextBlock()", code);
	}

	[Fact]
	public void Inline_ControlTemplate_factory_returns_root_element()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border />
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);

		// Lambda body must have a return statement returning the root variable.
		Assert.Contains("return ", code);
	}

	[Fact]
	public void Inline_ControlTemplate_assigned_to_Template_property()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border />
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Template = ", code);
	}

	[Fact]
	public void Inline_ControlTemplate_TargetType_resolved()
	{
		string xaml = Window(@"
<Button>
  <Button.Template>
    <ControlTemplate TargetType=""Button"">
      <Border />
    </ControlTemplate>
  </Button.Template>
</Button>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("typeof(global::System.Windows.Controls.Button)", code);
	}

	// ── Keyed <ControlTemplate x:Key="t"> in resources ──────────────────────

	[Fact]
	public void Keyed_ControlTemplate_resource_emits_ControlTemplate_in_Resources()
	{
		string xaml = Window(@"
<Window.Resources>
  <ControlTemplate x:Key=""t"" TargetType=""Button"">
    <Border />
  </ControlTemplate>
</Window.Resources>
<Button />");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Controls.ControlTemplate(", code);
		Assert.Contains(@"this.Resources[""t""] = ", code);
	}

	[Fact]
	public void Keyed_ControlTemplate_resource_emits_before_element_tree()
	{
		string xaml = Window(@"
<Window.Resources>
  <ControlTemplate x:Key=""t"" TargetType=""Button"">
    <Border />
  </ControlTemplate>
</Window.Resources>
<Button />");
		string code = _gen.Generate(xaml, out _);

		int templateIdx = code.IndexOf(@"this.Resources[""t""]");
		int buttonIdx = code.IndexOf("new System.Windows.Controls.Button()");
		Assert.True(templateIdx < buttonIdx,
			"ControlTemplate resource must be emitted before the element tree.");
	}

	[Fact]
	public void Keyed_ControlTemplate_StaticResource_on_Template_emits_SetValue_FindResource()
	{
		string xaml = Window(@"
<Window.Resources>
  <ControlTemplate x:Key=""t"" TargetType=""Button"">
    <Border />
  </ControlTemplate>
</Window.Resources>
<Button Template=""{StaticResource t}"" />");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("SetValue(", code);
		Assert.Contains("StaticResourceScope.Find(", code);
		Assert.Contains(@"""t""", code);
	}

	// ── <DataTemplate> as ItemTemplate ──────────────────────────────────────

	[Fact]
	public void ItemTemplate_DataTemplate_emits_DataTemplate_construction()
	{
		string xaml = Window(@"
<ListBox>
  <ListBox.ItemTemplate>
    <DataTemplate>
      <TextBlock Text=""X"" />
    </DataTemplate>
  </ListBox.ItemTemplate>
</ListBox>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Controls.DataTemplate()", code);
	}

	[Fact]
	public void ItemTemplate_DataTemplate_emits_ItemFactory_lambda()
	{
		string xaml = Window(@"
<ListBox>
  <ListBox.ItemTemplate>
    <DataTemplate>
      <TextBlock Text=""X"" />
    </DataTemplate>
  </ListBox.ItemTemplate>
</ListBox>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".ItemFactory = __item =>", code);
	}

	[Fact]
	public void ItemTemplate_DataTemplate_factory_builds_TextBlock()
	{
		string xaml = Window(@"
<ListBox>
  <ListBox.ItemTemplate>
    <DataTemplate>
      <TextBlock Text=""hi"" />
    </DataTemplate>
  </ListBox.ItemTemplate>
</ListBox>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Controls.TextBlock()", code);
	}

	[Fact]
	public void ItemTemplate_DataTemplate_sets_DataContext_to_item()
	{
		string xaml = Window(@"
<ListBox>
  <ListBox.ItemTemplate>
    <DataTemplate>
      <TextBlock Text=""hi"" />
    </DataTemplate>
  </ListBox.ItemTemplate>
</ListBox>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("DataContext = __item", code);
	}

	[Fact]
	public void ItemTemplate_DataTemplate_assigned_to_ItemTemplate_property()
	{
		string xaml = Window(@"
<ListBox>
  <ListBox.ItemTemplate>
    <DataTemplate>
      <TextBlock />
    </DataTemplate>
  </ListBox.ItemTemplate>
</ListBox>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".ItemTemplate = ", code);
	}

	// ── Keyed <DataTemplate x:Key="d"> in resources ──────────────────────────

	[Fact]
	public void Keyed_DataTemplate_resource_emits_DataTemplate_in_Resources()
	{
		string xaml = Window(@"
<Window.Resources>
  <DataTemplate x:Key=""d"">
    <TextBlock Text=""item"" />
  </DataTemplate>
</Window.Resources>
<ListBox />");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Controls.DataTemplate()", code);
		Assert.Contains(@"this.Resources[""d""] = ", code);
	}

	[Fact]
	public void Keyed_DataTemplate_resource_emits_before_element_tree()
	{
		string xaml = Window(@"
<Window.Resources>
  <DataTemplate x:Key=""d"">
    <TextBlock />
  </DataTemplate>
</Window.Resources>
<ListBox />");
		string code = _gen.Generate(xaml, out _);

		int dtIdx = code.IndexOf(@"this.Resources[""d""]");
		int lbIdx = code.IndexOf("new System.Windows.Controls.ListBox()");
		Assert.True(dtIdx < lbIdx,
			"DataTemplate resource must be emitted before the element tree.");
	}

	// ── Binding inside DataTemplate ────────────────────────────────────────

	[Fact]
	public void Binding_inside_DataTemplate_factory_emits_SetBinding()
	{
		string xaml = Window(@"
<ListBox>
  <ListBox.ItemTemplate>
    <DataTemplate>
      <TextBlock Text=""{Binding Name}"" />
    </DataTemplate>
  </ListBox.ItemTemplate>
</ListBox>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("SetBinding(", code);
		Assert.Contains(@"new System.Windows.Data.Binding(""Name"")", code);
	}

	// ── Regression: existing features still work ──────────────────────────────

	[Fact]
	public void Regression_plain_button_unaffected()
	{
		string xaml = Window(@"<StackPanel><Button Content=""OK"" /></StackPanel>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Controls.Button()", code);
		Assert.Contains("new System.Windows.Controls.StackPanel()", code);
	}

	[Fact]
	public void Regression_style_resource_unaffected()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"" />
</Window.Resources>
<Button />");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Style(", code);
		Assert.Contains(@"this.Resources[""s""]", code);
	}

	// ── H1 regression: two keyed templates in Resources ───────────────────────

	[Fact]
	public void Keyed_ControlTemplate_resource_no_Resources_assignment()
	{
		// H1: EmitPropertyElement must not emit {owner}.Resources = {tmplVar}
		string xaml = Window(@"
<Window.Resources>
  <ControlTemplate x:Key=""t"" TargetType=""Button"">
    <Border />
  </ControlTemplate>
</Window.Resources>
<Button />");
		string code = _gen.Generate(xaml, out _);

		// Resources is get-only — no assignment must appear.
		Assert.DoesNotContain(".Resources = ", code);
	}

	[Fact]
	public void Two_keyed_templates_in_Resources_both_emitted_no_assignment()
	{
		// H1: a Window.Resources with two keyed templates must emit both
		// and must not emit any .Resources = assignment.
		string xaml = Window(@"
<Window.Resources>
  <ControlTemplate x:Key=""a"" TargetType=""Button"">
    <Border />
  </ControlTemplate>
  <DataTemplate x:Key=""b"">
    <TextBlock />
  </DataTemplate>
</Window.Resources>
<Button />");
		string code = _gen.Generate(xaml, out _);

		// Both keys must be present.
		Assert.Contains(@"Resources[""a""]", code);
		Assert.Contains(@"Resources[""b""]", code);
		// No bogus assignment to get-only Resources property.
		Assert.DoesNotContain(".Resources = ", code);
	}
}