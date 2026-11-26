using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Int32Rect and PixelFormat text conversion (WPF Int32RectConverter and PixelFormatConverter).</summary>
public class XamlValueConverterImagingTests
{
	[Theory]
	[InlineData("1,2,3,4", 1, 2, 3, 4)]
	[InlineData(" 10 20 30 40 ", 10, 20, 30, 40)]
	[InlineData("1, 2 ,3,\t4", 1, 2, 3, 4)]
	[InlineData("-1,0,5,6", -1, 0, 5, 6)]
	public void Int32Rect_parses_comma_or_space_separated(string raw, int x, int y, int w, int h)
		=> Assert.Equal(new Int32Rect(x, y, w, h), XamlValueConverter.ConvertByType(typeof(Int32Rect), raw));

	[Fact]
	public void Int32Rect_Empty_is_case_sensitive()
	{
		Assert.Equal(Int32Rect.Empty, XamlValueConverter.ConvertByType(typeof(Int32Rect), " Empty "));
		Assert.Throws<FormatException>(() => XamlValueConverter.ConvertByType(typeof(Int32Rect), "empty"));
		Assert.Throws<FormatException>(() => XamlValueConverter.ConvertByType(typeof(Int32Rect), "EMPTY"));
	}

	[Theory]
	[InlineData("1,2,3")]
	[InlineData("1,2,3,4,5")]
	[InlineData("a,b,c,d")]
	[InlineData("1.5,2,3,4")]
	public void Int32Rect_rejects_malformed_text(string raw)
		=> Assert.ThrowsAny<FormatException>(() => XamlValueConverter.ConvertByType(typeof(Int32Rect), raw));

	[Theory]
	[InlineData("Bgra32")]
	[InlineData("bgra32")]
	[InlineData(" Pbgra32 ")]
	[InlineData("Gray8")]
	public void PixelFormat_resolves_names_ignoring_case(string raw)
	{
		var pf = (PixelFormat)XamlValueConverter.ConvertByType(typeof(PixelFormat), raw)!;
		Assert.Equal(raw.Trim(), pf.ToString(), ignoreCase: true);
	}

	[Fact]
	public void PixelFormat_unknown_name_is_a_FormatException()
		=> Assert.Throws<FormatException>(() => XamlValueConverter.ConvertByType(typeof(PixelFormat), "Nope"));

	[Fact]
	public void Imaging_types_are_converted_through_a_dependency_property()
	{
		Assert.Equal(PixelFormats.Bgr24, XamlValueConverter.Convert(
			typeof(System.Windows.Media.Imaging.FormatConvertedBitmap),
			System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationFormatProperty, "Bgr24"));
		Assert.Equal(new Int32Rect(1, 2, 3, 4), XamlValueConverter.Convert(
			typeof(System.Windows.Media.Imaging.CroppedBitmap),
			System.Windows.Media.Imaging.CroppedBitmap.SourceRectProperty, "1,2,3,4"));
	}
}
