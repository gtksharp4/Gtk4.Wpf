using System.Globalization;

namespace System.Windows.Data;

/// <summary>
/// Provides a way to apply custom logic to a binding. Applied in the
/// source → target direction by <see cref="Binding.Converter"/>.
/// </summary>
public interface IValueConverter
{
	/// <summary>Converts a value from the source to the target type.</summary>
	object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

	/// <summary>
	/// Converts a value back from the target to the source type.
	/// Required for <see cref="BindingMode.TwoWay"/> (Phase-1).
	/// </summary>
	object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture);
}