using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class FreezableGeneratorTests : GeneratorV2TestBase
{
	const string PageStub = "namespace A { public partial class P : System.Windows.Controls.Page { } }";

	const string ClipPage = @"<Page x:Class=""A.P"" " + Head + @"><StackPanel>
<Border Name=""b""><Border.Clip><EllipseGeometry x:Name=""g1"" RadiusX=""100"" RadiusY=""75"" Center=""100,75""/></Border.Clip>
<Border.Triggers><EventTrigger RoutedEvent=""Border.Loaded""><BeginStoryboard><Storyboard>
<PointAnimation Storyboard.TargetName=""g1"" Storyboard.TargetProperty=""(EllipseGeometry.Center)""
 From=""0,0"" To=""200,150"" Duration=""0:0:3""/>
</Storyboard></BeginStoryboard></EventTrigger></Border.Triggers></Border></StackPanel></Page>";

	[Fact]
	public void A_named_Freezable_in_a_property_element_is_registered_in_the_name_scope()
	{
		string code = Gen(ClipPage);
		Assert.Contains("this.RegisterName(\"g1\", ", code);
		Assert.Contains("this.RegisterName(\"b\", ", code);
		AssertCompiles(code, PageStub);
	}

	[Fact]
	public void PointAnimation_From_and_To_go_through_the_Point_converter()
	{
		string code = Gen(ClipPage);
		Assert.Contains("new System.Windows.Media.Animation.PointAnimation()", code);
		Assert.Contains(".From = (System.Windows.Point?)System.Windows.Markup.XamlValueConverter.ConvertByType(" +
			"typeof(System.Windows.Point), \"0,0\")", code);
		Assert.Contains("SetTargetPropertyName(", code);
	}

	[Fact]
	public void An_application_root_does_not_register_names()
	{
		string code = Gen(@"<Application x:Class=""A.P"" " + Head + @"/>");
		Assert.DoesNotContain("RegisterName", code);
	}

	[Fact]
	public void An_attached_property_set_to_a_TemplateBinding_becomes_a_binding()
	{
		string code = Gen(@"<Page x:Class=""A.P"" " + Head + @"><Page.Resources><ControlTemplate x:Key=""t""
 TargetType=""Button""><ContentPresenter TextBlock.Foreground=""{TemplateBinding Foreground}""/></ControlTemplate>
</Page.Resources></Page>");
		Assert.Contains(".SetBinding(global::System.Windows.Controls.TextBlock.ForegroundProperty, ", code);
		Assert.DoesNotContain("TextBlock.SetForeground(", code);
		AssertCompiles(code, PageStub);
	}
}
