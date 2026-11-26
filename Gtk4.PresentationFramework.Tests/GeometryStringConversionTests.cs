using System.Windows;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Point, Size and Rect accept commas and/or whitespace, as WPF's converters do.</summary>
public class GeometryStringConversionTests
{
	[Theory]
	[InlineData("1,2,3,4")]
	[InlineData("1 2 3 4")]
	[InlineData("1, 2, 3, 4")]
	[InlineData(" 1  2,3 4 ")]
	public void Rect_separators(string raw)
		=> Assert.Equal(new Rect(1, 2, 3, 4), XamlValueConverter.ConvertByType(typeof(Rect), raw));

	[Theory]
	[InlineData("1 2")]
	[InlineData("1,2")]
	public void Point_and_Size_separators(string raw)
	{
		Assert.Equal(new Point(1, 2), XamlValueConverter.ConvertByType(typeof(Point), raw));
		Assert.Equal(new Size(1, 2), XamlValueConverter.ConvertByType(typeof(Size), raw));
	}

	[Theory]
	[InlineData(typeof(Rect), "1 2 3")]
	[InlineData(typeof(Point), "1 2 3")]
	[InlineData(typeof(Size), "1")]
	public void Wrong_arity_still_throws(System.Type type, string raw)
		=> Assert.Throws<System.FormatException>(() => XamlValueConverter.ConvertByType(type, raw));
}
