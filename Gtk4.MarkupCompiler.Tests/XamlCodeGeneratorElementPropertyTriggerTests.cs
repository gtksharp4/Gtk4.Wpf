using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Compiler tests for element-level property <c>&lt;Trigger&gt;</c> codegen:
/// construction, the nested <c>&lt;Setter&gt;</c>, the <c>Triggers.Add</c> target
/// and the <c>AttachElementTriggers</c> call.
/// </summary>
public class XamlCodeGeneratorElementPropertyTriggerTests : XamlCodeGeneratorElementTriggerTestBase
{
	[Fact]
	public void ElementTriggers_PropertyTrigger_emits_construction()
	{
		string xaml = Window(@"
<Button>
  <Button.Triggers>
    <Trigger Property=""IsMouseOver"" Value=""True"">
      <Setter Property=""Opacity"" Value=""0.5"" />
    </Trigger>
  </Button.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Trigger", code);
		Assert.Contains("IsMouseOver", code);
		Assert.Contains("True", code);
	}

	[Fact]
	public void ElementTriggers_PropertyTrigger_emits_Setter_inside_trigger()
	{
		string xaml = Window(@"
<Button>
  <Button.Triggers>
    <Trigger Property=""IsMouseOver"" Value=""True"">
      <Setter Property=""Opacity"" Value=""0.5"" />
    </Trigger>
  </Button.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(@"PropertyName = ""Opacity""", code);
		Assert.Contains(@"Value = ""0.5""", code);
	}

	[Fact]
	public void ElementTriggers_PropertyTrigger_emits_Triggers_Add()
	{
		string xaml = Window(@"
<Button>
  <Button.Triggers>
    <Trigger Property=""IsMouseOver"" Value=""True"" />
  </Button.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Triggers.Add(", code);
	}

	[Fact]
	public void ElementTriggers_PropertyTrigger_emits_AttachElementTriggers()
	{
		string xaml = Window(@"
<Button>
  <Button.Triggers>
    <Trigger Property=""IsMouseOver"" Value=""True"" />
  </Button.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".AttachElementTriggers()", code);
	}
}
