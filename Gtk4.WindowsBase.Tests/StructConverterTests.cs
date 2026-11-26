using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class StructConverterTests
{
	static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
	static readonly ThicknessConverter T = new ThicknessConverter();
	static readonly RectConverter R = new RectConverter();

	[Theory]
	[InlineData("1", 1, 1, 1, 1)]
	[InlineData("1,2", 1, 2, 1, 2)]
	[InlineData("1 2 3 4", 1, 2, 3, 4)]
	[InlineData("  5 ,6  ", 5, 6, 5, 6)]
	[InlineData("1in", 96, 96, 96, 96)]
	[InlineData("1px 2", 1, 2, 1, 2)]
	public void Thickness_parses_one_two_or_four_lengths(string text, double l, double t, double r, double b) =>
		Assert.Equal(new Thickness(l, t, r, b), T.ConvertFromString(null, Inv, text));

	[Fact]
	public void Thickness_units_and_auto()
	{
		var th = (Thickness)T.ConvertFromString(null, Inv, "2cm,3pt")!;
		Assert.Equal(96 / 2.54 * 2, th.Left, 9);
		Assert.Equal(4, th.Top, 9);
		Assert.True(double.IsNaN(((Thickness)T.ConvertFromString(null, Inv, "Auto")!).Left));
	}

	[Theory]
	[InlineData("")]
	[InlineData("1,2,3")]
	[InlineData("1,2,3,4,5")]
	[InlineData("abc")]
	public void Thickness_rejects_bad_text(string text) =>
		Assert.Throws<FormatException>(() => T.ConvertFromString(null, Inv, text));

	[Fact]
	public void Thickness_numeric_sources_and_string_target()
	{
		Assert.True(T.CanConvertFrom(typeof(int)));
		Assert.True(T.CanConvertFrom(typeof(double)));
		Assert.False(T.CanConvertFrom(typeof(byte)));
		Assert.Equal(new Thickness(5), T.ConvertFrom(null, Inv, 5));
		Assert.Equal("1,2,3,4", T.ConvertTo(null, Inv, new Thickness(1, 2, 3, 4), typeof(string)));
		Assert.Equal("Auto,2,3,4.5", T.ConvertTo(null, Inv, new Thickness(double.NaN, 2, 3, 4.5), typeof(string)));
		Assert.Throws<NotSupportedException>(() => T.ConvertFrom(null, Inv, null!));
	}

	[Fact]
	public void Thickness_uses_culture_list_separator()
	{
		var de = new CultureInfo("de-DE");
		Assert.Equal("1,5;2;3;4", T.ConvertTo(null, de, new Thickness(1.5, 2, 3, 4), typeof(string)));
		Assert.Equal(new Thickness(1.5, 2, 1.5, 2), T.ConvertFrom(null, de, "1,5;2"));
	}

	[Fact]
	public void Rect_parses_and_formats()
	{
		Assert.Equal(new Rect(1, 2, 3, 4), R.ConvertFromString(null, Inv, "1,2,3,4"));
		Assert.Equal(new Rect(1, 2, 3, 4), R.ConvertFromString(null, Inv, "1 2 3 4"));
		Assert.True(((Rect)R.ConvertFromString(null, Inv, "Empty")!).IsEmpty);
		Assert.Equal("1,2,3,4.5", R.ConvertTo(null, Inv, new Rect(1, 2, 3, 4.5), typeof(string)));
		Assert.Equal("Empty", R.ConvertTo(null, Inv, Rect.Empty, typeof(string)));
	}

	[Theory]
	[InlineData("1,2,-3,4")]
	[InlineData("1,2,3,-4")]
	public void Rect_negative_size_throws_ArgumentException(string text) =>
		Assert.Throws<ArgumentException>(() => R.ConvertFromString(null, Inv, text));

	[Fact]
	public void Rect_bad_text_errors_match_WPF()
	{
		Assert.Throws<InvalidOperationException>(() => R.ConvertFromString(null, Inv, "1,2,3"));
		Assert.Throws<FormatException>(() => R.ConvertFromString(null, Inv, "a,b,c,d"));
		Assert.False(R.CanConvertFrom(typeof(int)));
	}

	[Fact]
	public void TypeDescriptor_finds_the_converters()
	{
		Assert.IsType<ThicknessConverter>(TypeDescriptor.GetConverter(typeof(Thickness)));
		Assert.IsType<RectConverter>(TypeDescriptor.GetConverter(typeof(Rect)));
		Assert.Equal(new Thickness(1, 2, 3, 4), TypeDescriptor.GetConverter(typeof(Thickness)).ConvertFromInvariantString("1,2,3,4"));
	}

	[Fact]
	public void Rect_Size_and_Location_get_set_and_empty_throws()
	{
		var r = new Rect(1, 2, 3, 4);
		Assert.Equal(new Size(3, 4), r.Size);
		Assert.Equal(new Point(1, 2), r.Location);
		r.Size = new Size(10, 20);
		r.Location = new Point(5, 6);
		Assert.Equal(new Rect(5, 6, 10, 20), r);
		var empty = Rect.Empty;
		Assert.Equal(Size.Empty, empty.Size);
		Assert.Throws<InvalidOperationException>(() => empty.Size = new Size(1, 1));
		Assert.Throws<InvalidOperationException>(() => empty.Location = new Point(1, 1));
	}

	[Fact]
	public void Rect_edges_and_containment()
	{
		var r = new Rect(1, 2, 3, 4);
		Assert.Equal(4, r.Right);
		Assert.Equal(6, r.Bottom);
		Assert.Equal(new Point(4, 6), r.BottomRight);
		Assert.True(r.Contains(new Point(1, 2)));
		Assert.True(r.Contains(new Point(4, 6)));
		Assert.False(r.Contains(new Point(4.1, 3)));
		Assert.False(Rect.Empty.Contains(new Point(0, 0)));
		Assert.Equal(new Rect(0, 0, 5, 7), new Rect(new Size(5, 7)));
	}
}
