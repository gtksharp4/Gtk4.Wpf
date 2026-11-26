using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class PrimitiveConverterTests
{
	[Fact]
	public void Point_vector_and_matrix_converters_delegate_to_parse()
	{
		Assert.Equal(new Point(1, 2), new PointConverter().ConvertFromInvariantString("1,2"));
		Assert.Equal(new Vector(3, 4), new VectorConverter().ConvertFromInvariantString("3 4"));
		Assert.Equal(Matrix.Identity, new MatrixConverter().ConvertFromInvariantString("Identity"));
		Assert.Equal(new Matrix(1, 2, 3, 4, 5, 6), new MatrixConverter().ConvertFromInvariantString("1,2,3,4,5,6"));
	}

	[Fact]
	public void Converters_accept_and_produce_strings_only()
	{
		foreach (TypeConverter converter in new TypeConverter[]
			{ new PointConverter(), new VectorConverter(), new MatrixConverter() })
		{
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.True(converter.CanConvertTo(typeof(string)));
			Assert.False(converter.CanConvertFrom(typeof(double)));
			Assert.False(converter.CanConvertTo(typeof(double)));
		}
	}

	[Fact]
	public void Null_and_non_string_sources_throw_NotSupportedException()
	{
		Assert.Throws<NotSupportedException>(() => new PointConverter().ConvertFrom(null!));
		Assert.Throws<NotSupportedException>(() => new VectorConverter().ConvertFrom(5));
		Assert.Throws<NotSupportedException>(() => new MatrixConverter().ConvertFrom(null!));
	}

	[Fact]
	public void ConvertTo_string_uses_the_culture_list_separator()
	{
		var de = CultureInfo.GetCultureInfo("de-DE");
		Assert.Equal("1;2", new PointConverter().ConvertTo(null, de, new Point(1, 2), typeof(string)));
		Assert.Equal("1,2", new VectorConverter().ConvertTo(null, CultureInfo.InvariantCulture, new Vector(1, 2),
			typeof(string)));
		Assert.Equal("Identity", new MatrixConverter().ConvertTo(null, null, Matrix.Identity, typeof(string)));
	}
}
