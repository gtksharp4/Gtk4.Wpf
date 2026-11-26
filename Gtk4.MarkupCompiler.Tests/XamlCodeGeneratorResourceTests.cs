using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Phase-1 Round-2 compiler tests: &lt;X.Resources&gt;, Style construction,
/// {StaticResource} and {DynamicResource} attribute values.
/// </summary>
public class XamlCodeGeneratorResourceTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body = "", string attrs = "") =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	{attrs}>
	{body}
</Window>";

	// ── <Window.Resources> with a Style ──────────────────────────────────────

	[Fact]
	public void Resources_style_emits_Style_construction()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""btnStyle"" TargetType=""Button"">
    <Setter Property=""Background"" Value=""Red"" />
  </Style>
</Window.Resources>
<Grid />");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Style(typeof(global::System.Windows.Controls.Button))", code);
		Assert.Contains("btnStyle", code);
		Assert.Contains("Background", code);
		Assert.Contains("Red", code);
	}

	[Fact]
	public void Resources_style_assigned_to_Resources_dict()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s1"" TargetType=""Button"" />
</Window.Resources>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains(@"this.Resources[""s1""] = ", code);
	}

	[Fact]
	public void Resources_style_emits_Setter_with_PropertyName_and_Value()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Setter Property=""Foreground"" Value=""Blue"" />
  </Style>
</Window.Resources>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains(@"PropertyName = ""Foreground""", code);
		Assert.Contains(@"Value = ""Blue""", code);
	}

	[Fact]
	public void Resources_style_emits_before_element_tree()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"" />
</Window.Resources>
<Grid />");
		string code = _gen.Generate(xaml, out _);

		// Resources assignment must appear before Grid construction.
		int resourcesIdx = code.IndexOf(@"this.Resources[""s""]");
		int gridIdx = code.IndexOf("new System.Windows.Controls.Grid()");
		Assert.True(resourcesIdx < gridIdx,
			"Resources must be emitted before the element tree.");
	}

	// ── <ResourceDictionary> wrapper ──────────────────────────────────────────

	[Fact]
	public void Resources_wrapped_in_ResourceDictionary_element_still_emits()
	{
		string xaml = Window(@"
<Window.Resources>
  <ResourceDictionary>
    <Style x:Key=""wrapStyle"" TargetType=""Button"" />
  </ResourceDictionary>
</Window.Resources>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("wrapStyle", code);
		Assert.Contains("new System.Windows.Style(", code);
	}

	// ── Style="{StaticResource k}" on a child ────────────────────────────────

	[Fact]
	public void StaticResource_on_Style_attr_emits_FindResource_cast()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""btnStyle"" TargetType=""Button"" />
</Window.Resources>
<StackPanel>
  <Button Style=""{StaticResource btnStyle}"" />
</StackPanel>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("(System.Windows.Style)", code);
		Assert.Contains("StaticResourceScope.Find(", code);
		Assert.Contains(@"""btnStyle""", code);
	}

	[Fact]
	public void StaticResource_on_Style_uses_Style_property_not_SetValue()
	{
		string xaml = Window(@"
<StackPanel>
  <Button Style=""{StaticResource myStyle}"" />
</StackPanel>");
		string code = _gen.Generate(xaml, out _);

		// Should use .Style = (Style)…FindResource(…) not SetValue.
		Assert.Contains(".Style = (System.Windows.Style)", code);
		Assert.DoesNotContain(".SetValue(System.Windows.Controls.Button.StyleProperty", code);
	}

	// ── {StaticResource k} on a non-Style attribute ───────────────────────────

	[Fact]
	public void StaticResource_on_non_Style_attr_emits_SetValue_FindResource()
	{
		string xaml = Window(@"
<StackPanel>
  <TextBlock Tag=""{StaticResource myRes}"" />
</StackPanel>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("SetValue(", code);
		Assert.Contains("StaticResourceScope.Find(", code);
		Assert.Contains(@"""myRes""", code);
	}

	// ── {DynamicResource k} resolved like StaticResource at load ─────────────

	[Fact]
	public void DynamicResource_on_a_DP_emits_a_deferred_reference()
	{
		string xaml = Window(@"
<StackPanel>
  <Button Style=""{DynamicResource btnStyle}"" />
</StackPanel>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains("SetLocalResourceReference(", code);
		Assert.Contains(@"""btnStyle""", code);
	}

	// ── Style BasedOn emitted ─────────────────────────────────────────────────

	[Fact]
	public void Style_BasedOn_StaticResource_emits_BasedOn_assignment()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""base"" TargetType=""Button"" />
  <Style x:Key=""derived"" TargetType=""Button"" BasedOn=""{StaticResource base}"" />
</Window.Resources>");
		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".BasedOn = (System.Windows.Style)", code);
		Assert.Contains(@"""base""", code);
	}

	// ── Property-element children not emitted as regular children ────────────

	[Fact]
	public void PropertyElement_children_not_constructed_as_elements()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"" />
</Window.Resources>
<Grid />");
		string code = _gen.Generate(xaml, out _);

		// Window.Resources itself must not appear as a type construction.
		Assert.DoesNotContain("new System.Windows.Window.Resources()", code);
	}

	// ── Existing Phase-0/1 tests still pass (regression) ─────────────────────

	[Fact]
	public void Regression_plain_window_still_generates()
	{
		string xaml = Window(@"<Grid><TextBlock Text=""hi"" /></Grid>");
		string code = _gen.Generate(xaml, out string className);
		Assert.Equal("MyApp.MainWindow", className);
		Assert.Contains("new System.Windows.Controls.Grid()", code);
	}
}