using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class DataGridLengthTests
{
	static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
	static DataGridLength Star(double v) => new DataGridLength(v, DataGridLengthUnitType.Star);

	[Fact]
	public void UnitType_ordinals_match_WPF()
	{
		Assert.Equal(0, (int)DataGridLengthUnitType.Auto);
		Assert.Equal(1, (int)DataGridLengthUnitType.Pixel);
		Assert.Equal(2, (int)DataGridLengthUnitType.SizeToCells);
		Assert.Equal(3, (int)DataGridLengthUnitType.SizeToHeader);
		Assert.Equal(4, (int)DataGridLengthUnitType.Star);
	}

	[Theory]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void Value_must_be_finite(double v) =>
		Assert.Throws<ArgumentException>(() => new DataGridLength(v));

	[Fact]
	public void Desired_and_display_must_not_be_infinite_but_may_be_NaN()
	{
		var unit = DataGridLengthUnitType.Star;
		Assert.Throws<ArgumentException>(() => new DataGridLength(1, unit, double.PositiveInfinity, 0));
		Assert.Throws<ArgumentException>(() => new DataGridLength(1, unit, 0, double.NegativeInfinity));
		var l = Star(2);
		Assert.True(double.IsNaN(l.DesiredValue));
		Assert.True(double.IsNaN(l.DisplayValue));
	}

	[Fact]
	public void Undefined_unit_type_throws() =>
		Assert.Throws<ArgumentException>(() => new DataGridLength(1, (DataGridLengthUnitType)9));

	[Theory]
	[InlineData(DataGridLengthUnitType.Auto)]
	[InlineData(DataGridLengthUnitType.SizeToCells)]
	[InlineData(DataGridLengthUnitType.SizeToHeader)]
	public void Content_sized_units_normalise_value_to_one(DataGridLengthUnitType type)
	{
		var l = new DataGridLength(42, type);
		Assert.Equal(1.0, l.Value);
		Assert.Equal(type, l.UnitType);
	}

	[Fact]
	public void Pixel_ctor_sets_value_desired_and_display()
	{
		var l = new DataGridLength(30);
		Assert.True(l.IsAbsolute);
		Assert.Equal(30, l.Value);
		Assert.Equal(30, l.DesiredValue);
		Assert.Equal(30, l.DisplayValue);
	}

	[Fact]
	public void Flags_and_statics()
	{
		Assert.True(DataGridLength.Auto.IsAuto);
		Assert.True(DataGridLength.SizeToCells.IsSizeToCells);
		Assert.True(DataGridLength.SizeToHeader.IsSizeToHeader);
		Assert.True(Star(3).IsStar);
	}

	[Fact]
	public void Implicit_conversion_from_double_is_pixel()
	{
		DataGridLength l = 12.5;
		Assert.Equal(DataGridLengthUnitType.Pixel, l.UnitType);
		Assert.Equal(12.5, l.Value);
	}

	[Fact]
	public void Equality_treats_NaN_desired_and_display_as_equal()
	{
		var a = Star(2);
		var b = Star(2);
		Assert.True(a == b);
		Assert.True(a.Equals(b));
		Assert.True(a.Equals((object)b));
		Assert.Equal(a.GetHashCode(), b.GetHashCode());
		Assert.True(a != Star(3));
		Assert.True(a != new DataGridLength(2, DataGridLengthUnitType.Star, 5, 5));
		Assert.False(a.Equals("x"));
	}

	[Theory]
	[InlineData("Auto", DataGridLengthUnitType.Auto, 1.0)]
	[InlineData("  auto ", DataGridLengthUnitType.Auto, 1.0)]
	[InlineData("SIZETOCELLS", DataGridLengthUnitType.SizeToCells, 1.0)]
	[InlineData("sizetoheader", DataGridLengthUnitType.SizeToHeader, 1.0)]
	[InlineData("*", DataGridLengthUnitType.Star, 1.0)]
	[InlineData(" 2.5* ", DataGridLengthUnitType.Star, 2.5)]
	[InlineData("40", DataGridLengthUnitType.Pixel, 40.0)]
	[InlineData("40px", DataGridLengthUnitType.Pixel, 40.0)]
	[InlineData("1in", DataGridLengthUnitType.Pixel, 96.0)]
	[InlineData("72pt", DataGridLengthUnitType.Pixel, 96.0)]
	[InlineData("2.54cm", DataGridLengthUnitType.Pixel, 96.0)]
	public void Converter_parses_text(string text, DataGridLengthUnitType type, double value)
	{
		var l = (DataGridLength)new DataGridLengthConverter().ConvertFrom(null, Inv, text)!;
		Assert.Equal(type, l.UnitType);
		Assert.Equal(value, l.Value, 6);
	}

	[Theory]
	[InlineData("bogus")]
	[InlineData("abc*")]
	[InlineData("")]
	public void Converter_bad_text_throws_FormatException(string text) =>
		Assert.Throws<FormatException>(() => new DataGridLengthConverter().ConvertFrom(null, Inv, text));

	[Fact]
	public void Converter_numbers_and_NaN()
	{
		var c = new DataGridLengthConverter();
		Assert.Equal(new DataGridLength(7), c.ConvertFrom(null, Inv, 7));
		Assert.Equal(new DataGridLength(7.5), c.ConvertFrom(null, null, 7.5));
		Assert.Equal(DataGridLength.Auto, c.ConvertFrom(null, Inv, double.NaN));
	}

	[Fact]
	public void Converter_can_convert()
	{
		var c = new DataGridLengthConverter();
		Assert.True(c.CanConvertFrom(typeof(string)));
		Assert.True(c.CanConvertFrom(typeof(int)));
		Assert.False(c.CanConvertFrom(typeof(object)));
		Assert.True(c.CanConvertTo(typeof(string)));
		Assert.True(c.CanConvertTo(typeof(InstanceDescriptor)));
		Assert.False(c.CanConvertTo(typeof(int)));
	}

	[Fact]
	public void Converter_converts_to_text_and_instance_descriptor()
	{
		var c = new DataGridLengthConverter();
		Assert.Equal("Auto", c.ConvertTo(null, Inv, DataGridLength.Auto, typeof(string)));
		Assert.Equal("SizeToCells", c.ConvertTo(null, Inv, DataGridLength.SizeToCells, typeof(string)));
		Assert.Equal("SizeToHeader", c.ConvertTo(null, Inv, DataGridLength.SizeToHeader, typeof(string)));
		Assert.Equal("*", c.ConvertTo(null, Inv, Star(1), typeof(string)));
		Assert.Equal("2*", c.ConvertTo(null, Inv, Star(2), typeof(string)));
		Assert.Equal("12.5", c.ConvertTo(null, Inv, new DataGridLength(12.5), typeof(string)));
		var d = Assert.IsType<InstanceDescriptor>(
			c.ConvertTo(null, Inv, new DataGridLength(5), typeof(InstanceDescriptor)));
		Assert.Equal(new DataGridLength(5), d.Invoke());
	}

	[Fact]
	public void Converter_ConvertTo_errors()
	{
		var c = new DataGridLengthConverter();
		Assert.Throws<ArgumentNullException>(() => c.ConvertTo(null, Inv, DataGridLength.Auto, null!));
		Assert.Throws<NotSupportedException>(() => c.ConvertTo(null, Inv, 3, typeof(string)));
		Assert.Throws<NotSupportedException>(() => c.ConvertTo(null, Inv, DataGridLength.Auto, typeof(int)));
	}

	[Fact]
	public void ToString_uses_the_converter()
	{
		Assert.Equal("Auto", DataGridLength.Auto.ToString());
		Assert.Equal("*", Star(1).ToString());
	}

	[Fact]
	public void TypeDescriptor_finds_the_converter() =>
		Assert.IsType<DataGridLengthConverter>(TypeDescriptor.GetConverter(typeof(DataGridLength)));

	[Fact]
	public void Markup_conversion_of_ColumnWidth_star_yields_star_one()
	{
		var v = XamlValueConverter.Convert(typeof(DataGrid), DataGrid.ColumnWidthProperty, "*");
		var l = Assert.IsType<DataGridLength>(v);
		Assert.True(l.IsStar);
		Assert.Equal(1.0, l.Value);
	}
}
