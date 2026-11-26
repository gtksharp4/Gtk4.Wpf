using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class BindingGroupElementTests : GeneratorV2TestBase
{
	[Fact]
	public void A_BindingGroup_property_element_is_assigned_with_its_rules()
	{
		string xaml = $@"<Window x:Class=""A.Main"" {Head} xmlns:local=""clr-namespace:A"">
<StackPanel Name=""stackPanel1""><StackPanel.BindingGroup>
<BindingGroup Name=""g"" NotifyOnValidationError=""True""><BindingGroup.ValidationRules>
<local:CrossRule ValidationStep=""ConvertedProposedValue""/></BindingGroup.ValidationRules></BindingGroup>
</StackPanel.BindingGroup></StackPanel></Window>";
		string code = Gen(xaml, out _, hasCodeBehind: false);

		Assert.Contains("new System.Windows.Data.BindingGroup()", code);
		Assert.Contains(".Name = \"g\";", code);
		Assert.Contains(".NotifyOnValidationError = true;", code);
		Assert.Contains("new A.CrossRule()", code);
		Assert.Contains(".ValidationStep = System.Windows.Controls.ValidationStep.ConvertedProposedValue;", code);
		Assert.Contains(".ValidationRules.Add(", code);
		Assert.Matches(@"\.BindingGroup = __bg\d+;", code);
	}

	[Fact]
	public void An_ItemBindingGroup_property_element_is_assigned()
	{
		string xaml = $@"<Window x:Class=""A.Main"" {Head} xmlns:local=""clr-namespace:A"">
<ItemsControl><ItemsControl.ItemBindingGroup><BindingGroup><BindingGroup.ValidationRules>
<local:CrossRule/></BindingGroup.ValidationRules></BindingGroup></ItemsControl.ItemBindingGroup></ItemsControl></Window>";
		string code = Gen(xaml, out _, hasCodeBehind: false);

		Assert.Matches(@"\.ItemBindingGroup = __bg\d+;", code);
		Assert.Contains("new A.CrossRule()", code);
	}
}
