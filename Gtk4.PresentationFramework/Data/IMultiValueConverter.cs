using System.Globalization;

namespace System.Windows.Data;

/// <summary>
/// Provides a way to combine multiple binding values into a single target value.
/// Used by <see cref="MultiBinding"/>.
/// </summary>
public interface IMultiValueConverter
{
	/// <summary>
	/// Converts an array of source values to the target value.
	/// </summary>
	/// <param name="values">Array of values from each child <see cref="Binding"/>.</param>
	/// <param name="targetType">Type of the binding target property.</param>
	/// <param name="parameter">Optional converter parameter.</param>
	/// <param name="culture">Culture to use in the converter.</param>
	object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture);

	/// <summary>
	/// Converts a target value back to source values (for TwoWay bindings).
	/// Return <see cref="Binding.DoNothing"/> or an array containing
	/// <see cref="DependencyProperty.UnsetValue"/> for unimplemented conversions.
	/// </summary>
	object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture);
}