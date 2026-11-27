using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Floater / AnchoredBlock object model, as the EditingExaminerDemo tree view uses it.</summary>
public class AnchoredBlockTests : WpfGtkTestBase
{
	[Fact]
	public void A_Floater_is_an_AnchoredBlock_and_an_Inline_and_exposes_its_Blocks()
	{
		var paragraph = new Paragraph(new Run("inside"));
		var floater = new Floater(paragraph);

		Assert.IsAssignableFrom<AnchoredBlock>(floater);
		Assert.IsAssignableFrom<Inline>(floater);
		Assert.Same(paragraph, Assert.Single(floater.Blocks));
		Assert.Same(floater, paragraph.Parent);
	}

	[Fact]
	public void Defaults_match_WPF()
	{
		var floater = new Floater();

		Assert.Equal(HorizontalAlignment.Stretch, floater.HorizontalAlignment);
		Assert.True(double.IsNaN(floater.Width));
		Assert.True(double.IsNaN(floater.Margin.Left));
		Assert.Equal(new Thickness(0), floater.BorderThickness);
		Assert.Null(floater.BorderBrush);
		Assert.Empty(floater.Blocks);
	}

	[Theory]
	[InlineData(-1.0)]
	[InlineData(1000001.0)]
	public void Width_outside_the_valid_range_is_rejected(double width)
	{
		var floater = new Floater();

		Assert.Throws<ArgumentException>(() => floater.Width = width);
	}

	[Fact]
	public void A_Floater_can_sit_in_a_Paragraph_and_loads_from_XAML() => Run(() =>
	{
		var paragraph = (Paragraph)XamlReader.Parse(
			"<Paragraph xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
			+ "<Floater Width=\"120\"><Paragraph>note</Paragraph></Floater></Paragraph>");

		var floater = Assert.IsType<Floater>(Assert.Single(paragraph.Inlines));
		Assert.Equal(120, floater.Width);
		Assert.Single(floater.Blocks);
	});
}
