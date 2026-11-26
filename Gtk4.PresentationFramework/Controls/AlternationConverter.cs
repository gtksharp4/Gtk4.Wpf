using System.Collections;
using System.Globalization;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// Maps an <see cref="ItemsControl.AlternationIndexProperty"/> value to the entry of
/// <see cref="Values"/> at <c>index % Values.Count</c>.
/// </summary>
[ContentProperty("Values")]
public class AlternationConverter : Data.IValueConverter
{
	/// <summary>Gets the values the alternation index selects from.</summary>
	public IList Values { get; } = new ArrayList();

	/// <inheritdoc />
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (Values.Count == 0 || value is not int index) return DependencyProperty.UnsetValue;
		int i = index % Values.Count;
		if (i < 0) i += Values.Count;
		return Values[i];
	}

	/// <inheritdoc />
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		for (int i = 0; i < Values.Count; i++)
			if (object.Equals(Values[i], value)) return i;
		return -1;
	}
}
