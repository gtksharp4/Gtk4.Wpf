using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The .NET 9 <c>RowDefinitions="Auto,*,Auto"</c> / <c>ColumnDefinitions</c> shorthand through XamlReader.</summary>
public class XamlReaderGridShorthandTests : WpfGtkTestBase
{
	const string Ns = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"";

	[Fact]
	public void Row_and_column_shorthand_add_definitions_in_order() => Run(() =>
	{
		var g = (Grid)XamlReader.Parse("<Grid " + Ns + " RowDefinitions=\"Auto,*,Auto\" ColumnDefinitions=\"100, 2* 3in\"/>");

		Assert.Equal(3, g.RowDefinitions.Count);
		Assert.Equal(GridLength.Auto, g.RowDefinitions[0].Height);
		Assert.Equal(new GridLength(1, GridUnitType.Star), g.RowDefinitions[1].Height);
		Assert.Equal(new GridLength(100), g.ColumnDefinitions[0].Width);
		Assert.Equal(new GridLength(2, GridUnitType.Star), g.ColumnDefinitions[1].Width);
		Assert.Equal(new GridLength(288), g.ColumnDefinitions[2].Width);
	});

	[Fact]
	public void A_bad_token_is_a_parse_error() => Run(() =>
		Assert.Throws<XamlParseException>(() => XamlReader.Parse("<Grid " + Ns + " RowDefinitions=\"Auto,zzz\"/>")));
}
