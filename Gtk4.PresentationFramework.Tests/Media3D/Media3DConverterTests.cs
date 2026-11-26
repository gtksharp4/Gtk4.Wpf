using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media3D;

public class Media3DConverterTests
{
	[Theory]
	[InlineData(typeof(Point3D), typeof(Point3DConverter))]
	[InlineData(typeof(Vector3D), typeof(Vector3DConverter))]
	[InlineData(typeof(Size3D), typeof(Size3DConverter))]
	[InlineData(typeof(Point4D), typeof(Point4DConverter))]
	[InlineData(typeof(Quaternion), typeof(QuaternionConverter))]
	[InlineData(typeof(Matrix3D), typeof(Matrix3DConverter))]
	[InlineData(typeof(Rect3D), typeof(Rect3DConverter))]
	[InlineData(typeof(Point3DCollection), typeof(Point3DCollectionConverter))]
	[InlineData(typeof(Vector3DCollection), typeof(Vector3DCollectionConverter))]
	[InlineData(typeof(Int32Collection), typeof(Int32CollectionConverter))]
	[InlineData(typeof(KeyTime), typeof(KeyTimeConverter))]
	[InlineData(typeof(KeySpline), typeof(KeySplineConverter))]
	public void Types_carry_their_converter_and_it_handles_strings(Type type, Type converterType)
	{
		var converter = TypeDescriptor.GetConverter(type);
		Assert.IsType(converterType, converter);
		Assert.True(converter.CanConvertFrom(typeof(string)));
		Assert.True(converter.CanConvertTo(typeof(string)));
		Assert.False(converter.CanConvertFrom(typeof(int)));
	}

	[Fact]
	public void Converters_parse_strings_to_values()
	{
		Assert.Equal(new Point3D(1, 2, 3), new Point3DConverter().ConvertFromInvariantString("1,2,3"));
		Assert.Equal(new Vector3D(0, 1, 0), new Vector3DConverter().ConvertFromInvariantString("0 1 0"));
		Assert.Equal(new Size3D(1, 2, 3), new Size3DConverter().ConvertFromInvariantString("1 2 3"));
		Assert.Equal(new Point4D(1, 2, 3, 4), new Point4DConverter().ConvertFromInvariantString("1,2,3,4"));
		Assert.Equal(Quaternion.Identity, new QuaternionConverter().ConvertFromInvariantString("Identity"));
		Assert.Equal(Matrix3D.Identity, new Matrix3DConverter().ConvertFromInvariantString("Identity"));
		Assert.Equal(new Rect3D(0, 0, 0, 1, 2, 3), new Rect3DConverter().ConvertFromInvariantString("0,0,0,1,2,3"));
	}

	[Fact]
	public void Null_and_non_string_sources_throw_NotSupportedException()
	{
		var converter = new Point3DConverter();
		Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(null!));
		Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(42));
		Assert.Throws<NotSupportedException>(() => new Int32CollectionConverter().ConvertFrom(null!));
	}

	[Fact]
	public void ConvertTo_string_uses_the_culture_separator()
	{
		var converter = new Point3DConverter();
		Assert.Equal("10,20,30",
			converter.ConvertTo(null, CultureInfo.GetCultureInfo("en-US"), new Point3D(10, 20, 30), typeof(string)));
		Assert.Equal("10;20;30",
			converter.ConvertTo(null, CultureInfo.GetCultureInfo("de-DE"), new Point3D(10, 20, 30), typeof(string)));
		Assert.Throws<NotSupportedException>(() =>
			converter.ConvertTo(null, null, new Point3D(), typeof(int)));
	}

	[Fact]
	public void Collection_converters_parse_whole_lists()
	{
		var points = (Point3DCollection)new Point3DCollectionConverter().ConvertFromInvariantString("0,0,0 1,0,0 0,1,0")!;
		Assert.Equal(3, points.Count);
		Assert.Equal(new Point3D(0, 1, 0), points[2]);
		var vectors = Vector3DCollection.Parse("0,0,1 0,0,1");
		Assert.Equal(2, vectors.Count);
		var indices = (Int32Collection)new Int32CollectionConverter().ConvertFromInvariantString("0 1 2, 2 1 3")!;
		Assert.Equal(new[] { 0, 1, 2, 2, 1, 3 }, indices);
		Assert.Throws<InvalidOperationException>(() => Point3DCollection.Parse("1,2"));
		Assert.Equal("0,0,0 1,0,0", Point3DCollection.Parse("0 0 0 1 0 0").ToString(CultureInfo.InvariantCulture));
	}

	[Fact]
	public void The_xaml_value_converter_registry_knows_the_3d_types()
	{
		Assert.Equal(new Point3D(1, 2, 3), XamlValueConverter.ConvertByType(typeof(Point3D), "1,2,3"));
		Assert.Equal(new Vector3D(0, 1, 0), XamlValueConverter.ConvertByType(typeof(Vector3D), "0,1,0"));
		Assert.IsType<Point3DCollection>(XamlValueConverter.ConvertByType(typeof(Point3DCollection), "0 0 0"));
		Assert.IsType<Int32Collection>(XamlValueConverter.ConvertByType(typeof(Int32Collection), "0 1 2"));
		Assert.Equal(KeyTime.FromPercent(0.5), XamlValueConverter.ConvertByType(typeof(KeyTime), "50%"));
		Assert.IsType<KeySpline>(XamlValueConverter.ConvertByType(typeof(KeySpline), "0.1,0.2 0.3,0.4"));
	}
}
