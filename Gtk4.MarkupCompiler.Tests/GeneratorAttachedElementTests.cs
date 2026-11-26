using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Attached property elements: values go through the owner setter, bindings through SetBinding.</summary>
public class GeneratorAttachedElementTests : GeneratorV2TestBase
{
	[Fact]
	public void BindingChild_UsesSetBinding()
	{
		string code = Gen($@"<Window {Head} x:Class=""A.W""><Button>
			<ToolTipService.ToolTip><Binding Path=""Tip""/></ToolTipService.ToolTip></Button></Window>");

		Assert.Contains(".SetBinding(global::System.Windows.Controls.ToolTipService.ToolTipProperty", code);
		Assert.DoesNotContain("ToolTipService.SetToolTip", code);
	}

	[Fact]
	public void MultiBindingChild_UsesSetBinding()
	{
		string code = Gen($@"<Window {Head} x:Class=""A.W""><Button>
			<AutomationProperties.Name><MultiBinding><Binding Path=""A""/><Binding Path=""B""/></MultiBinding>
			</AutomationProperties.Name></Button></Window>");

		Assert.Contains("SetBinding(", code);
		Assert.DoesNotContain("AutomationProperties.SetName", code);
	}

	[Fact]
	public void ValueChild_StillUsesSetter()
	{
		string code = Gen($@"<Window {Head} x:Class=""A.W"" Title=""t"">
			<WindowChrome.WindowChrome><WindowChrome CaptionHeight=""30""/></WindowChrome.WindowChrome>
			<Grid/></Window>");

		Assert.Contains("WindowChrome.SetWindowChrome(this, ", code);
	}

	const string UserHead = Head + " xmlns:local=\"clr-namespace:App\"";
	const string Owner = @"namespace App { public static class MyOwner {
public static readonly System.Windows.DependencyProperty PropProperty = System.Windows.DependencyProperty
.RegisterAttached(""Prop"", typeof(object), typeof(MyOwner)); } }";

	[Fact]
	public void UserOwner_binding_keeps_its_clr_namespace_qualification()
	{
		string code = Gen($@"<Window {UserHead} x:Class=""App.W""><Button>
			<local:MyOwner.Prop><Binding Path=""P""/></local:MyOwner.Prop></Button></Window>",
			sources: new[] { Owner });

		Assert.Contains(".SetBinding(global::App.MyOwner.PropProperty", code);
		Assert.DoesNotContain("System.Windows.MyOwner", code);
		AssertCompiles(code, Owner + "namespace App { public partial class W { } }");
	}

	[Fact]
	public void UserOwner_value_element_is_not_an_attached_element_and_is_reported()
	{
		const string xaml = "<Window " + UserHead + @" x:Class=""App.W""><Button>
			<local:MyOwner.Prop><Border/></local:MyOwner.Prop></Button></Window>";

		string code = Gen(xaml, sources: new[] { Owner });

		Assert.DoesNotContain("MyOwner.SetProp", code);
		Assert.Contains(Messages(xaml), m => m.Contains("MyOwner.Prop"));
	}
}
