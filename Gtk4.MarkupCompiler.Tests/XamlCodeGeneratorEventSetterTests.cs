using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class XamlCodeGeneratorEventSetterTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	<Window.Resources>
{body}
	</Window.Resources>
</Window>";

	static string Style(string children) =>
		Window($@"<Style x:Key=""s"" TargetType=""Button"">{children}</Style>");

	string Gen(string xaml) => _gen.Generate(xaml, out _);

	[Fact]
	public void Unqualified_event_uses_TargetType_owner()
	{
		string code = Gen(Style(@"<EventSetter Event=""Click"" Handler=""OnClick""/>"));
		Assert.Contains(
			"Setters.Add(new System.Windows.EventSetter(global::System.Windows.Controls.Button.ClickEvent, " +
			"System.Delegate.CreateDelegate(global::System.Windows.Controls.Button.ClickEvent.HandlerType, " +
			"this, typeof(global::MyApp.MainWindow).GetMethod(\"OnClick\", System.Reflection.BindingFlags.Instance | " +
			"System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)))", code);
	}

	[Fact]
	public void Qualified_event_uses_named_owner()
	{
		string code = Gen(Style(@"<EventSetter Event=""Border.MouseDown"" Handler=""OnDown""/>"));
		Assert.Contains("EventSetter(global::System.Windows.Controls.Border.MouseDownEvent, " +
			"System.Delegate.CreateDelegate(global::System.Windows.Controls.Border.MouseDownEvent.HandlerType, " +
			"this, typeof(global::MyApp.MainWindow).GetMethod(\"OnDown\", System.Reflection.BindingFlags.Instance | " +
			"System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)))", code);
	}

	[Fact]
	public void HandledEventsToo_is_emitted()
	{
		string code = Gen(Style(@"<EventSetter Event=""Click"" Handler=""H"" HandledEventsToo=""True""/>"));
		Assert.Contains("NonPublic))) { HandledEventsToo = true }", code);
	}

	[Fact]
	public void EventSetter_without_handler_is_skipped()
	{
		string code = Gen(Style(@"<EventSetter Event=""Click""/>"));
		Assert.DoesNotContain("new System.Windows.EventSetter", code);
	}

	[Fact]
	public void EventSetter_and_Setter_coexist_in_order()
	{
		string code = Gen(Style(@"<Setter Property=""Tag"" Value=""x""/><EventSetter Event=""Click"" Handler=""H""/>"));
		Assert.True(code.IndexOf("PropertyName = \"Tag\"") < code.IndexOf("new System.Windows.EventSetter"));
	}

	[Fact]
	public void Handler_is_bound_against_the_x_Class_not_GetType()
	{
		string code = Gen(Style(@"<EventSetter Event=""Click"" Handler=""OnClick""/>"));
		Assert.Contains("typeof(global::MyApp.MainWindow).GetMethod(", code);
		Assert.DoesNotContain("GetType().GetMethod(", code);
	}
}
