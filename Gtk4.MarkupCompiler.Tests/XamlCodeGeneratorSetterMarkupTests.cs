using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class XamlCodeGeneratorSetterMarkupTests
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
	public void Setter_Value_ControlTemplate_is_emitted_inline()
	{
		string code = Gen(Style(@"<Setter Property=""Template""><Setter.Value>
<ControlTemplate TargetType=""Button""><Border/></ControlTemplate></Setter.Value></Setter>"));
		Assert.Contains("new System.Windows.Controls.ControlTemplate", code);
		Assert.Contains("new System.Windows.Controls.Border()", code);
		Assert.Contains(@"PropertyName = ""Template"", Value = __sv", code);
	}

	[Fact]
	public void Setter_Value_DataTemplate_is_emitted_inline()
	{
		string code = Gen(Style(@"<Setter Property=""ContentTemplate""><Setter.Value>
<DataTemplate><TextBlock/></DataTemplate></Setter.Value></Setter>"));
		Assert.Contains("new System.Windows.Controls.DataTemplate()", code);
		Assert.Contains("ItemFactory", code);
	}

	[Fact]
	public void Setter_Value_MultiBinding_is_emitted_inline()
	{
		string code = Gen(Style(@"<Setter Property=""Tag""><Setter.Value>
<MultiBinding><Binding Path=""A""/><Binding Path=""B""/></MultiBinding></Setter.Value></Setter>"));
		Assert.Contains("new System.Windows.Data.MultiBinding()", code);
	}

	[Fact]
	public void Setter_Value_object_element_is_constructed_and_assigned()
	{
		string code = Gen(Style(@"<Setter Property=""Content""><Setter.Value>
<Border/></Setter.Value></Setter>"));
		Assert.Contains("new System.Windows.Controls.Border()", code);
		Assert.Contains("Value = __sv", code);
	}

	[Fact]
	public void Setter_Value_text_content_is_trimmed_literal()
	{
		string code = Gen(Style(@"<Setter Property=""Tag""><Setter.Value>  hello  </Setter.Value></Setter>"));
		Assert.Contains(@"PropertyName = ""Tag"", Value = ""hello""", code);
	}

	[Fact]
	public void Value_attribute_wins_over_property_element()
	{
		string code = Gen(Style(@"<Setter Property=""Tag"" Value=""a""><Setter.Value>b</Setter.Value></Setter>"));
		Assert.Contains(@"Value = ""a""", code);
		Assert.DoesNotContain(@"""b""", code);
	}

	[Fact]
	public void Trigger_setter_property_element_is_emitted()
	{
		string code = Gen(Style(@"<Style.Triggers><Trigger Property=""IsEnabled"" Value=""False"">
<Setter Property=""Template""><Setter.Value><ControlTemplate><Border/></ControlTemplate></Setter.Value></Setter>
</Trigger></Style.Triggers>"));
		Assert.Contains("new System.Windows.Controls.ControlTemplate", code);
	}

	[Fact]
	public void Two_property_elements_get_distinct_variables()
	{
		string code = Gen(Style(@"<Setter Property=""Template""><Setter.Value><ControlTemplate><Border/></ControlTemplate></Setter.Value></Setter>
<Setter Property=""ContentTemplate""><Setter.Value><DataTemplate><TextBlock/></DataTemplate></Setter.Value></Setter>"));
		Assert.Contains("var __sv10000 ", code);
		Assert.Matches(@"var __sv100\d\d ", code.Substring(code.IndexOf("var __sv10000") + 20));
	}
}
