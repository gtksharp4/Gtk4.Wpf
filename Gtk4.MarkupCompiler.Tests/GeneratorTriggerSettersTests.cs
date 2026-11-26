using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Setters are the trigger content property: direct and property-element forms are emitted.</summary>
public class GeneratorTriggerSettersTests : GeneratorV2TestBase
{
	static string Win(string resources)
		=> $@"<Window x:Class=""A.W"" {Head}><Window.Resources>{resources}</Window.Resources></Window>";

	// WalkthroughFirstWPFApp/csharp/Styles.xaml, the data-header style's MultiDataTrigger, verbatim.
	const string WalkthroughStyle = @"<Style x:Key=""DataHeader"" TargetType=""{x:Type Label}"">
<Style.Triggers>
<MultiDataTrigger>
<MultiDataTrigger.Conditions>
<Condition Binding=""{Binding Path=(SystemParameters.HighContrast)}"" Value=""True"" />
<Condition Binding=""{Binding Path=IsKeyboardFocused, RelativeSource={RelativeSource Mode=Self}}"" Value=""True"" />
</MultiDataTrigger.Conditions>
<MultiDataTrigger.Setters>
<Setter Property=""Background"" Value=""{DynamicResource {x:Static SystemColors.HighlightBrushKey}}"" />
<Setter Property=""Foreground"" Value=""{DynamicResource {x:Static SystemColors.HighlightTextBrushKey}}"" />
<Setter Property=""BorderBrush"" Value=""{DynamicResource {x:Static SystemColors.HighlightTextBrushKey}}""/>
<Setter Property=""BorderThickness"" Value=""2""/>
</MultiDataTrigger.Setters>
</MultiDataTrigger>
<DataTrigger Binding=""{Binding Path=State}"" Value=""WA"">
<DataTrigger.Setters><Setter Property=""Foreground"" Value=""Red""/></DataTrigger.Setters>
</DataTrigger>
</Style.Triggers></Style>";

	[Fact]
	public void A_MultiDataTrigger_Setters_property_element_emits_all_four_setters()
	{
		string code = Gen(Win(WalkthroughStyle));
		int multi = code.IndexOf("new System.Windows.MultiDataTrigger()", System.StringComparison.Ordinal);
		Assert.True(multi >= 0);
		int data = code.IndexOf("new System.Windows.DataTrigger", multi, System.StringComparison.Ordinal);
		string body = code.Substring(multi, data - multi);
		Assert.All(new[] { "Background", "Foreground", "BorderBrush", "BorderThickness" },
			n => Assert.Contains($"PropertyName = \"{n}\"", body));
		Assert.Equal(4, body.Split(new[] { ".Setters.Add(" }, System.StringSplitOptions.None).Length - 1);
	}

	[Fact]
	public void A_DataTrigger_Setters_property_element_emits_its_setter()
	{
		string code = Gen(Win(WalkthroughStyle));
		int data = code.IndexOf("new System.Windows.DataTrigger", System.StringComparison.Ordinal);
		Assert.True(data >= 0);
		Assert.Contains("PropertyName = \"Foreground\"", code.Substring(data));
	}
}
