using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Round-7 compiler tests: RelativeSource, Converter={StaticResource}, StringFormat,
/// FallbackValue, TargetNullValue, UpdateSourceTrigger, and MultiBinding property element.
/// Each test fails without the matching compiler change.
/// </summary>
public class XamlCodeGeneratorBindingR7Tests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body = "") =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";

	// ── RelativeSource={RelativeSource Self} ──────────────────────────────

	[Fact]
	public void RelativeSource_Self_emits_RelativeSource_Self_singleton()
	{
		string xaml = Window(
			@"<TextBlock Text=""{Binding DataContext, RelativeSource={RelativeSource Self}}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("RelativeSource = System.Windows.Data.RelativeSource.Self", code);
		Assert.Contains(@"new System.Windows.Data.Binding(""DataContext"")", code);
	}

	// ── RelativeSource={RelativeSource AncestorType=Grid} ─────────────────

	[Fact]
	public void RelativeSource_FindAncestor_emits_constructor_call()
	{
		string xaml = Window(
			@"<TextBlock Text=""{Binding Tag, RelativeSource={RelativeSource FindAncestor, AncestorType=Grid}}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("RelativeSourceMode.FindAncestor", code);
		Assert.Contains("typeof(System.Windows.Controls.Grid)", code);
	}

	// ── RelativeSource={RelativeSource TemplatedParent} ───────────────────

	[Fact]
	public void RelativeSource_TemplatedParent_emits_singleton()
	{
		string xaml = Window(
			@"<TextBlock Text=""{Binding Tag, RelativeSource={RelativeSource TemplatedParent}}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent", code);
	}

	// ── Converter={StaticResource k} ─────────────────────────────────────

	[Fact]
	public void Converter_StaticResource_emits_IValueConverter_cast_and_FindResource()
	{
		string xaml = Window(
			@"<TextBlock Text=""{Binding Path=Name, Converter={StaticResource myConv}}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("(System.Windows.Data.IValueConverter)", code);
		Assert.Contains(@"StaticResourceScope.Find(""myConv"", this", code);
		Assert.DoesNotContain("TODO", code);
	}

	// ── StringFormat ──────────────────────────────────────────────────────

	[Fact]
	public void StringFormat_emitted_in_binding_initializer()
	{
		string xaml = Window(
			@"<TextBlock Text=""{Binding Path=Count, StringFormat=Total: {0}}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("StringFormat = \"Total: {0}\"", code);
	}

	// ── FallbackValue ─────────────────────────────────────────────────────

	[Fact]
	public void FallbackValue_emitted_in_binding_initializer()
	{
		string xaml = Window(
			@"<TextBlock Text=""{Binding Path=X, FallbackValue=N/A}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("FallbackValue = \"N/A\"", code);
	}

	// ── TargetNullValue ───────────────────────────────────────────────────

	[Fact]
	public void TargetNullValue_emitted_in_binding_initializer()
	{
		string xaml = Window(
			@"<TextBlock Text=""{Binding Path=X, TargetNullValue=(none)}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("TargetNullValue = \"(none)\"", code);
	}

	// ── UpdateSourceTrigger ───────────────────────────────────────────────

	[Fact]
	public void UpdateSourceTrigger_emitted_in_binding_initializer()
	{
		string xaml = Window(
			@"<TextBox Text=""{Binding Path=X, Mode=TwoWay, UpdateSourceTrigger=Explicit}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.Explicit", code);
	}

	// ── <MultiBinding> property element ──────────────────────────────────

	[Fact]
	public void MultiBinding_property_element_emits_MultiBinding_construction()
	{
		string xaml = Window(@"
<TextBlock>
  <TextBlock.Text>
    <MultiBinding Converter=""{StaticResource concat}"">
      <Binding Path=""First""/>
      <Binding Path=""Last""/>
    </MultiBinding>
  </TextBlock.Text>
</TextBlock>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.Windows.Data.MultiBinding", code);
		Assert.Contains("SetBinding(", code);
		Assert.Contains(".TextProperty", code);
	}

	[Fact]
	public void MultiBinding_Bindings_children_emitted_as_Add_calls()
	{
		string xaml = Window(@"
<TextBlock>
  <TextBlock.Text>
    <MultiBinding Converter=""{StaticResource c}"">
      <Binding Path=""A""/>
      <Binding Path=""B""/>
    </MultiBinding>
  </TextBlock.Text>
</TextBlock>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains(@"Bindings.Add(new System.Windows.Data.Binding(""A""))", code);
		Assert.Contains(@"Bindings.Add(new System.Windows.Data.Binding(""B""))", code);
	}

	[Fact]
	public void MultiBinding_Converter_StaticResource_emitted()
	{
		string xaml = Window(@"
<TextBlock>
  <TextBlock.Text>
    <MultiBinding Converter=""{StaticResource myConv}"">
      <Binding Path=""X""/>
    </MultiBinding>
  </TextBlock.Text>
</TextBlock>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("IMultiValueConverter", code);
		Assert.Contains(@"StaticResourceScope.Find(""myConv"", this", code);
	}

	// ── SplitBindingArgs stays correct with nested braces ─────────────────

	[Fact]
	public void SplitBindingArgs_handles_nested_braces()
	{
		var parts = XamlCodeGenerator.SplitBindingArgs(
			@"Path=X, Converter={StaticResource conv}, Mode=TwoWay");
		Assert.Equal(3, parts.Count);
		Assert.Equal("Path=X", parts[0].Trim());
		Assert.Equal("Converter={StaticResource conv}", parts[1].Trim());
		Assert.Equal("Mode=TwoWay", parts[2].Trim());
	}
}