using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Windows.Controls;

/// <summary>Converts <see cref="DataGridLength"/> to and from text, numbers and an InstanceDescriptor.</summary>
public class DataGridLengthConverter : TypeConverter
{
	static readonly Type[] _sourceTypes =
	{
		typeof(string), typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(byte),
		typeof(ushort), typeof(uint), typeof(ulong), typeof(float), typeof(double), typeof(decimal)
	};

	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		Array.IndexOf(_sourceTypes, sourceType) >= 0;

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string) || destinationType == typeof(InstanceDescriptor);

	/// <inheritdoc/>
	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		culture ??= CultureInfo.InvariantCulture;
		if (value is string text) return FromString(text, culture);
		if (value != null && CanConvertFrom(context, value.GetType()))
		{
			var number = Convert.ToDouble(value, culture);
			return double.IsNaN(number) ? DataGridLength.Auto : new DataGridLength(number);
		}
		throw GetConvertFromException(value);
	}

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
		if (value is DataGridLength length)
		{
			if (destinationType == typeof(string))
				return ConvertToString(length, culture ?? CultureInfo.InvariantCulture);
			if (destinationType == typeof(InstanceDescriptor))
			{
				var ctor = typeof(DataGridLength).GetConstructor(
					new[] { typeof(double), typeof(DataGridLengthUnitType) })!;
				return new InstanceDescriptor(ctor, new object[] { length.Value, length.UnitType });
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	internal static string ConvertToString(DataGridLength length, CultureInfo culture)
	{
		switch (length.UnitType)
		{
			case DataGridLengthUnitType.Auto: return "Auto";
			case DataGridLengthUnitType.SizeToCells: return "SizeToCells";
			case DataGridLengthUnitType.SizeToHeader: return "SizeToHeader";
			case DataGridLengthUnitType.Star:
				return length.Value == 1.0 ? "*" : length.Value.ToString(culture) + "*";
			default: return length.Value.ToString(culture);
		}
	}

	static DataGridLength FromString(string text, CultureInfo culture)
	{
		var s = text.Trim();
		if (s.Equals("Auto", StringComparison.OrdinalIgnoreCase)) return DataGridLength.Auto;
		if (s.Equals("SizeToCells", StringComparison.OrdinalIgnoreCase)) return DataGridLength.SizeToCells;
		if (s.Equals("SizeToHeader", StringComparison.OrdinalIgnoreCase)) return DataGridLength.SizeToHeader;
		if (s.EndsWith("*", StringComparison.Ordinal))
		{
			var weight = s.Substring(0, s.Length - 1).Trim();
			var star = weight.Length == 0 ? 1.0 : ParseWeight(weight, text, culture);
			return new DataGridLength(star, DataGridLengthUnitType.Star);
		}
		// Reuses the WindowsBase length parser (px/in/cm/pt suffixes; FormatException on bad input).
		var pixels = ConverterParsing.ParseLength(s, culture);
		return double.IsNaN(pixels) ? DataGridLength.Auto : new DataGridLength(pixels);
	}

	static double ParseWeight(string s, string original, CultureInfo culture)
	{
		if (!double.TryParse(s, NumberStyles.Float, culture, out var value))
			throw new FormatException($"'{original}' is not a valid DataGridLength.");
		return value;
	}
}
