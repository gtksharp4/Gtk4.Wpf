using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ValueConversionAttributeTests
{
	[ValueConversion(typeof(int), typeof(string), ParameterType = typeof(double))]
	sealed class IntToString : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
	}

	[Fact]
	public void Attribute_exposes_source_target_and_parameter_types()
	{
		var attr = typeof(IntToString).GetCustomAttribute<ValueConversionAttribute>()!;
		Assert.Equal(typeof(int), attr.SourceType);
		Assert.Equal(typeof(string), attr.TargetType);
		Assert.Equal(typeof(double), attr.ParameterType);
	}

	[Fact]
	public void ParameterType_defaults_to_null()
	{
		var attr = new ValueConversionAttribute(typeof(int), typeof(string));
		Assert.Null(attr.ParameterType);
	}

	[Fact]
	public void Null_types_throw()
	{
		Assert.Throws<ArgumentNullException>(() => new ValueConversionAttribute(null!, typeof(string)));
		Assert.Throws<ArgumentNullException>(() => new ValueConversionAttribute(typeof(int), null!));
	}

	[Fact]
	public void Attribute_is_class_only()
	{
		var usage = typeof(ValueConversionAttribute).GetCustomAttribute<AttributeUsageAttribute>()!;
		Assert.Equal(AttributeTargets.Class, usage.ValidOn);
	}
}
