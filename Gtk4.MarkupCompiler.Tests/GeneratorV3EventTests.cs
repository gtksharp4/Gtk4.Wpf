using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class GeneratorV3EventTests : GeneratorV2TestBase
{
	const string Stub = @"namespace A { public partial class W : System.Windows.Window {
void H(object s, System.Windows.RoutedEventArgs e) { }
void M(object s, System.Windows.Input.MouseButtonEventArgs e) { } } }";

	static string Window(string content, string attrs = "")
		=> $@"<Window x:Class=""A.W"" {Head} {attrs}>{content}</Window>";

	[Fact]
	public void G4_Button_Click_on_a_panel_adds_a_routed_handler()
	{
		string code = Gen(Window(@"<StackPanel Button.Click=""H""/>"));
		Assert.Contains(".AddHandler(System.Windows.Controls.Button.ClickEvent, " +
			"new System.Windows.RoutedEventHandler(this.H));", code.Replace("global::", ""));
		Assert.DoesNotContain("SetClick", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G4_RadioButton_Checked_on_a_panel_adds_a_routed_handler()
	{
		string code = Gen(Window(@"<StackPanel RadioButton.Checked=""H""/>"));
		Assert.Contains("AddHandler(", code);
		Assert.Contains("CheckedEvent, new System.Windows.RoutedEventHandler(this.H)", code.Replace("global::", ""));
		Assert.DoesNotContain("SetChecked", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G4_the_root_element_accepts_an_attached_event_too()
	{
		string code = Gen(Window("", "Button.Click=\"H\""));
		Assert.Contains("this.AddHandler(", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G4_PreviewMouseDown_and_Up_wire_the_clr_events()
	{
		string code = Gen(Window(@"<Button PreviewMouseDown=""M"" PreviewMouseUp=""M""/>"));
		Assert.Contains(".PreviewMouseDown += this.M;", code);
		Assert.Contains(".PreviewMouseUp += this.M;", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G4_Mouse_attached_event_syntax_uses_the_owner_delegate_type()
	{
		string code = Gen(Window(@"<StackPanel Button.PreviewMouseDown=""M""/>"));
		Assert.Contains("PreviewMouseDownEvent, new System.Windows.Input.MouseButtonEventHandler(this.M)",
			code.Replace("global::", ""));
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G4_an_owner_qualified_event_on_its_own_type_is_a_plain_event()
	{
		string code = Gen(Window(@"<Button Button.Click=""H""/>"));
		Assert.Contains(".Click += this.H;", code);
		Assert.DoesNotContain("AddHandler", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G4_attached_properties_are_unchanged()
	{
		string code = Gen(Window(@"<Grid><Button Grid.Row=""1""/></Grid>"));
		Assert.Contains("Grid.SetRow(", code);
		Assert.DoesNotContain("AddHandler", code);
	}
}

public class GeneratorV3GenericEventTests : GeneratorV2TestBase
{
	const string Stub = @"namespace A { public partial class W : System.Windows.Window {
void OnErr(object s, System.Windows.Controls.ValidationErrorEventArgs e) { } } }";

	[Fact]
	public void Validation_Error_uses_the_generic_Add_handler_delegate_type()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Grid Validation.Error=""OnErr""/></Window>");
		Assert.Contains("global::System.EventHandler<global::System.Windows.Controls.ValidationErrorEventArgs>", code);
		Assert.DoesNotContain("global::global::", code);
		AssertCompiles(code, Stub);
	}

}
