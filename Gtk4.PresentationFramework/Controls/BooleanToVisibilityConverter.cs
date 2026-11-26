using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>
/// Converts <see cref="bool"/> to <see cref="Visibility"/>: true is Visible, anything else
/// (false, null, a non-bool) is Collapsed.
/// </summary>
public sealed class BooleanToVisibilityConverter : IValueConverter
{
	/// <summary>Maps a <see cref="bool"/> or <c>bool?</c> true to Visible, otherwise Collapsed.</summary>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

	/// <summary>Maps Visible to true, any other value to false.</summary>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is Visibility v && v == Visibility.Visible;
}
