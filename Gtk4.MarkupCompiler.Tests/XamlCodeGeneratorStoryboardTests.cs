using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Round-9A compiler tests: keyed Storyboard resources, EventTrigger+BeginStoryboard.
/// Each test fails without the matching feature.
/// </summary>
public class XamlCodeGeneratorStoryboardTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";

	// ── Keyed <Storyboard> in <Window.Resources> ─────────────────────────────

	[Fact]
	public void Storyboard_resource_emits_Storyboard_construction()
	{
		string xaml = Window(@"
<Window.Resources>
  <Storyboard x:Key=""fadeIn"">
    <DoubleAnimation Storyboard.TargetName=""myEl""
                     Storyboard.TargetProperty=""Opacity""
                     From=""0"" To=""1"" Duration=""0:0:1"" />
  </Storyboard>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Media.Animation.Storyboard()", code);
	}

	[Fact]
	public void Storyboard_resource_emits_DoubleAnimation_construction()
	{
		string xaml = Window(@"
<Window.Resources>
  <Storyboard x:Key=""s"">
    <DoubleAnimation Storyboard.TargetName=""x""
                     Storyboard.TargetProperty=""Opacity""
                     From=""0"" To=""1"" Duration=""0:0:1"" />
  </Storyboard>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Media.Animation.DoubleAnimation()", code);
	}

	[Fact]
	public void Storyboard_resource_emits_From_and_To()
	{
		string xaml = Window(@"
<Window.Resources>
  <Storyboard x:Key=""s"">
    <DoubleAnimation Storyboard.TargetName=""x""
                     Storyboard.TargetProperty=""Opacity""
                     From=""0"" To=""1"" Duration=""0:0:1"" />
  </Storyboard>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".From = ", code);
		Assert.Contains(".To = ", code);
	}

	[Fact]
	public void Storyboard_resource_emits_SetTargetName()
	{
		string xaml = Window(@"
<Window.Resources>
  <Storyboard x:Key=""s"">
    <DoubleAnimation Storyboard.TargetName=""myEl""
                     Storyboard.TargetProperty=""Opacity""
                     From=""0"" To=""1"" Duration=""0:0:1"" />
  </Storyboard>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("SetTargetName(", code);
		Assert.Contains(@"""myEl""", code);
	}

	[Fact]
	public void Storyboard_resource_emits_SetTargetPropertyName()
	{
		string xaml = Window(@"
<Window.Resources>
  <Storyboard x:Key=""s"">
    <DoubleAnimation Storyboard.TargetName=""x""
                     Storyboard.TargetProperty=""Opacity""
                     From=""0"" To=""1"" Duration=""0:0:1"" />
  </Storyboard>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("SetTargetPropertyName(", code);
		Assert.Contains(@"""Opacity""", code);
	}

	[Fact]
	public void Storyboard_resource_assigned_to_Resources_dict()
	{
		string xaml = Window(@"
<Window.Resources>
  <Storyboard x:Key=""fadeIn"">
    <DoubleAnimation Storyboard.TargetProperty=""Opacity"" From=""0"" To=""1"" Duration=""0:0:1"" />
  </Storyboard>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(@"this.Resources[""fadeIn""]", code);
	}

	[Fact]
	public void Storyboard_resource_emits_Children_Add()
	{
		string xaml = Window(@"
<Window.Resources>
  <Storyboard x:Key=""s"">
    <DoubleAnimation Storyboard.TargetProperty=""Opacity"" From=""0"" To=""1"" Duration=""0:0:1"" />
  </Storyboard>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Children.Add(", code);
	}

	[Fact]
	public void Storyboard_Duration_emits_XamlValueConverter_call()
	{
		string xaml = Window(@"
<Window.Resources>
  <Storyboard x:Key=""s"">
    <DoubleAnimation Storyboard.TargetProperty=""Opacity"" From=""0"" To=""1"" Duration=""0:0:1"" />
  </Storyboard>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains(@"""0:0:1""", code);
	}

	// ── EventTrigger + BeginStoryboard inside Style.Triggers ─────────────────

	[Fact]
	public void EventTrigger_BeginStoryboard_emits_BeginStoryboard_construction()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Style.Triggers>
      <EventTrigger RoutedEvent=""Button.Click"">
        <BeginStoryboard>
          <Storyboard>
            <DoubleAnimation Storyboard.TargetProperty=""Opacity"" From=""1"" To=""0"" Duration=""0:0:0.3"" />
          </Storyboard>
        </BeginStoryboard>
      </EventTrigger>
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.BeginStoryboard()", code);
	}

	[Fact]
	public void EventTrigger_BeginStoryboard_emits_Storyboard_assigned()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Style.Triggers>
      <EventTrigger RoutedEvent=""Button.Click"">
        <BeginStoryboard>
          <Storyboard>
            <DoubleAnimation Storyboard.TargetProperty=""Opacity"" From=""1"" To=""0"" Duration=""0:0:0.3"" />
          </Storyboard>
        </BeginStoryboard>
      </EventTrigger>
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Storyboard = ", code);
	}

	[Fact]
	public void EventTrigger_BeginStoryboard_emits_Actions_Add()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Style.Triggers>
      <EventTrigger RoutedEvent=""Button.Click"">
        <BeginStoryboard>
          <Storyboard>
            <DoubleAnimation Storyboard.TargetProperty=""Opacity"" From=""1"" To=""0"" Duration=""0:0:0.3"" />
          </Storyboard>
        </BeginStoryboard>
      </EventTrigger>
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Actions.Add(", code);
	}

	[Fact]
	public void EventTrigger_BeginStoryboard_still_emits_Triggers_Add()
	{
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Style.Triggers>
      <EventTrigger RoutedEvent=""Button.Click"">
        <BeginStoryboard>
          <Storyboard>
            <DoubleAnimation Storyboard.TargetProperty=""Opacity"" From=""1"" To=""0"" Duration=""0:0:0.3"" />
          </Storyboard>
        </BeginStoryboard>
      </EventTrigger>
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Triggers.Add(", code);
	}

	[Fact]
	public void EventTrigger_without_actions_still_emits_EventTrigger()
	{
		// Regression: plain EventTrigger without BeginStoryboard still works.
		string xaml = Window(@"
<Window.Resources>
  <Style x:Key=""s"" TargetType=""Button"">
    <Style.Triggers>
      <EventTrigger RoutedEvent=""Button.Click"" />
    </Style.Triggers>
  </Style>
</Window.Resources>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.EventTrigger", code);
		Assert.Contains(".Triggers.Add(", code);
	}
}