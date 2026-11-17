using Gtk4.Forms.MarkupCompiler;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>
/// Compiler tests for element-level trigger code generation:
/// <c>&lt;Button.Triggers&gt;</c> / <c>&lt;FrameworkElement.Triggers&gt;</c>
/// declared directly on a FrameworkElement (not inside a Style).
/// </summary>
public class XamlCodeGeneratorElementTriggerTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";

	// ── EventTrigger on element ───────────────────────────────────────────────

	[Fact]
	public void ElementTriggers_EventTrigger_emits_construction()
	{
		string xaml = Window(@"
<Button>
  <Button.Triggers>
    <EventTrigger RoutedEvent=""Button.Click"">
      <BeginStoryboard>
        <Storyboard />
      </BeginStoryboard>
    </EventTrigger>
  </Button.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		// RoutedEvent is resolved via GetRoutedEvent: owner type + event name separately.
		Assert.Contains("new System.Windows.EventTrigger", code);
		Assert.Contains("GetRoutedEvent", code);
		Assert.Contains("\"Click\"", code);
	}

	[Fact]
	public void ElementTriggers_EventTrigger_emits_Triggers_Add()
	{
		string xaml = Window(@"
<Button>
  <Button.Triggers>
    <EventTrigger RoutedEvent=""Button.Click"" />
  </Button.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Triggers.Add(", code);
	}

	[Fact]
	public void ElementTriggers_EventTrigger_emits_AttachElementTriggers()
	{
		string xaml = Window(@"
<Button>
  <Button.Triggers>
    <EventTrigger RoutedEvent=""Button.Click"" />
  </Button.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".AttachElementTriggers()", code);
	}

	// ── Property Trigger on element ───────────────────────────────────────────

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

	// ── FrameworkElement.Triggers form ───────────────────────────────────────

	[Fact]
	public void ElementTriggers_FrameworkElementTriggers_form_emits_triggers()
	{
		string xaml = Window(@"
<Button>
  <FrameworkElement.Triggers>
    <EventTrigger RoutedEvent=""Button.Click"" />
  </FrameworkElement.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.EventTrigger", code);
		Assert.Contains(".Triggers.Add(", code);
		Assert.Contains(".AttachElementTriggers()", code);
	}

	// ── Multiple triggers in one block ───────────────────────────────────────

	[Fact]
	public void ElementTriggers_multiple_triggers_all_added()
	{
		string xaml = Window(@"
<Button>
  <Button.Triggers>
    <EventTrigger RoutedEvent=""Button.Click"" />
    <Trigger Property=""IsMouseOver"" Value=""True"" />
  </Button.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.EventTrigger", code);
		Assert.Contains("new System.Windows.Trigger", code);
	}

	// ── AttachElementTriggers called once only ────────────────────────────────

	[Fact]
	public void ElementTriggers_AttachElementTriggers_called_once_per_element()
	{
		string xaml = Window(@"
<Button>
  <Button.Triggers>
    <EventTrigger RoutedEvent=""Button.Click"" />
    <Trigger Property=""IsMouseOver"" Value=""True"" />
  </Button.Triggers>
</Button>");

		string code = _gen.Generate(xaml, out _);

		int count = 0;
		int idx = 0;
		while ((idx = code.IndexOf(".AttachElementTriggers()", idx)) >= 0)
		{
			count++;
			idx++;
		}
		Assert.Equal(1, count);
	}

	// ── Style.Triggers unchanged ──────────────────────────────────────────────

	[Fact]
	public void StyleTriggers_still_emits_to_style_not_element()
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
		// Style.Triggers path must NOT emit AttachElementTriggers.
		Assert.DoesNotContain(".AttachElementTriggers()", code);
	}
}
