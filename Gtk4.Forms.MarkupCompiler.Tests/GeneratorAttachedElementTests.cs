using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

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
}
