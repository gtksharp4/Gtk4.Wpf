using System;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Review fixes of sweep IV: TextEffects null guard and dropped CommandBinding attributes.</summary>
public class GeneratorSweep4ReviewTests : Sweep3Base
{
	const string Guard = "TextEffects == null)";

	[Theory]
	[InlineData("<StackPanel><TextBlock><Run><Run.TextEffects><TextEffect PositionCount=\"1\"/></Run.TextEffects>"
		+ "</Run></TextBlock></StackPanel>")]
	[InlineData("<FlowDocument><Paragraph><Paragraph.TextEffects><TextEffect PositionCount=\"1\"/>"
		+ "</Paragraph.TextEffects></Paragraph></FlowDocument>")]
	[InlineData("<StackPanel><TextBlock><TextBlock.TextEffects><TextEffect PositionCount=\"1\"/>"
		+ "</TextBlock.TextEffects></TextBlock></StackPanel>")]
	public void TextEffects_are_null_guarded_before_the_first_Add(string body)
	{
		string code = Compile("<Window " + Head + " x:Class=\"App.C\">" + body + "</Window>");

		int guard = code.IndexOf(Guard, StringComparison.Ordinal);
		int add = code.IndexOf(".TextEffects.Add(", StringComparison.Ordinal);
		Assert.True(guard >= 0 && add > guard, "no null guard before Add in " + code);
		Has(code, ".TextEffects = new System.Windows.Media.TextEffectCollection();");
	}

	[Fact]
	public void TextEffects_guard_is_emitted_once_per_property_element()
	{
		string code = Compile("<Window " + Head + " x:Class=\"App.C\"><TextBlock><TextBlock.TextEffects>"
			+ "<TextEffect PositionCount=\"1\"/><TextEffect PositionCount=\"2\"/></TextBlock.TextEffects>"
			+ "</TextBlock></Window>");

		Assert.Equal(1, code.Split(new[] { Guard }, StringSplitOptions.None).Length - 1);
		Assert.Equal(2, code.Split(new[] { ".TextEffects.Add(" }, StringSplitOptions.None).Length - 1);
	}

	[Fact]
	public void Unsupported_CommandBinding_attributes_are_reported_and_handlers_kept()
	{
		const string stub = "namespace App { public partial class C {"
			+ " void H(object s, System.Windows.Input.CanExecuteRoutedEventArgs e) { }"
			+ " void E(object s, System.Windows.Input.ExecutedRoutedEventArgs e) { } } }";
		string xaml = "<Window " + Head + " x:Class=\"App.C\"><StackPanel><StackPanel.CommandBindings>"
			+ "<CommandBinding Command=\"ApplicationCommands.Open\" PreviewCanExecute=\"H\" Executed=\"E\"/>"
			+ "</StackPanel.CommandBindings></StackPanel></Window>";

		string code = Compile(xaml, stub);

		Has(code, ".Executed += this.E;");
		Lacks(code, "PreviewCanExecute");
		Assert.Contains("Skipped attribute PreviewCanExecute on CommandBinding: not available in the layer",
			string.Join("\n", Messages(xaml)));
	}
}
