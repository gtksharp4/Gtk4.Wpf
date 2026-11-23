using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace System.Windows;

/// <summary>Converts a string to and from <see cref="ThemeMode"/>.</summary>
#if NET8_0_OR_GREATER
[Experimental("WPF0001")]
#endif
public class ThemeModeConverter : TypeConverter
{
	/// <inheritdoc />
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		Type.GetTypeCode(sourceType) == TypeCode.String;

	/// <inheritdoc />
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string);

	/// <inheritdoc />
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value != null)
			return new ThemeMode(value.ToString()!);
		throw GetConvertFromException(value);
	}

	/// <inheritdoc />
	public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
		Type destinationType)
	{
		if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
		if (value is ThemeMode mode)
		{
			if (destinationType == typeof(string))
				return mode.Value;
			if (destinationType == typeof(InstanceDescriptor))
				return new InstanceDescriptor(typeof(ThemeMode).GetConstructor(new[] { typeof(string) }), new object[] { mode.Value });
		}
		throw GetConvertToException(value, destinationType);
	}
}
