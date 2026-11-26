using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Compiler tests for trigger code generation:
/// &lt;Style.Triggers&gt; containing Trigger, DataTrigger, MultiTrigger, EventTrigger.
/// </summary>
public class XamlCodeGeneratorTriggerTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";

	// ── Trigger ───────────────────────────────────────────────────────────────

	[Fact]
	public void StyleTrigger_emits_Trigger_construction()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Style.Triggers>
      <Trigger Property=""IsEnabled"" Value=""False"">
        <Setter Property=""Background"" Value=""Gray"" />
      </Trigger>
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Trigger", code);
		Assert.Contains("IsEnabled", code);
		Assert.Contains("False", code);
	}

	[Fact]
	public void StyleTrigger_emits_Setter_inside_trigger()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Style.Triggers>
      <Trigger Property=""IsEnabled"" Value=""False"">
        <Setter Property=""Background"" Value=""Gray"" />
      </Trigger>
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(@"PropertyName = ""Background""", code);
		Assert.Contains(@"Value = ""Gray""", code);
	}

	[Fact]
	public void StyleTrigger_emits_Triggers_Add()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Style.Triggers>
      <Trigger Property=""IsEnabled"" Value=""False"" />
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Triggers.Add(", code);
	}

	// ── DataTrigger ───────────────────────────────────────────────────────────

	[Fact]
	public void DataTrigger_emits_construction_with_Binding_and_Value()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""TextBlock"">
    <Style.Triggers>
      <DataTrigger Binding=""{Binding IsActive}"" Value=""True"">
        <Setter Property=""Foreground"" Value=""Green"" />
      </DataTrigger>
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.DataTrigger", code);
		Assert.Contains("IsActive", code);
		Assert.Contains("True", code);
	}

	[Fact]
	public void DataTrigger_emits_Triggers_Add()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""TextBlock"">
    <Style.Triggers>
      <DataTrigger Binding=""{Binding X}"" Value=""1"" />
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Triggers.Add(", code);
	}

	// ── MultiTrigger ─────────────────────────────────────────────────────────

	[Fact]
	public void MultiTrigger_emits_construction_with_Conditions_and_Setter()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Style.Triggers>
      <MultiTrigger>
        <MultiTrigger.Conditions>
          <Condition Property=""IsMouseOver"" Value=""True"" />
          <Condition Property=""IsEnabled"" Value=""True"" />
        </MultiTrigger.Conditions>
        <Setter Property=""Background"" Value=""LightBlue"" />
      </MultiTrigger>
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.MultiTrigger", code);
		Assert.Contains("Conditions.Add", code);
		Assert.Contains("IsMouseOver", code);
		Assert.Contains("IsEnabled", code);
		Assert.Contains("LightBlue", code);
	}

	// ── Style Setters + Triggers coexist ─────────────────────────────────────

	[Fact]
	public void Style_with_both_Setters_and_Triggers_emits_both()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Setter Property=""Background"" Value=""White"" />
    <Style.Triggers>
      <Trigger Property=""IsEnabled"" Value=""False"">
        <Setter Property=""Background"" Value=""Gray"" />
      </Trigger>
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(@"Value = ""White""", code);
		Assert.Contains("new System.Windows.Trigger", code);
		Assert.Contains(@"Value = ""Gray""", code);
	}
}