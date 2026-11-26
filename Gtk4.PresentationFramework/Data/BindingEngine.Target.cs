using System.Globalization;
using System.ComponentModel;
using System.Threading;

namespace System.Windows.Data;

/// <summary>Never-throwing target writes: WPF substitutes FallbackValue / the default for invalid values.</summary>
internal static partial class BindingEngine
{
	internal static bool IsCritical(Exception ex)
		=> ex is OutOfMemoryException || ex is StackOverflowException || ex is ThreadAbortException;

	/// <summary>Converts to the DP type (strings via XAML converters, IConvertible, TypeConverter).</summary>
	internal static object? CoerceToDpType(DependencyProperty dp, object? value)
	{
		var t = Nullable.GetUnderlyingType(dp.PropertyType) ?? dp.PropertyType;
		if (value == null || t.IsInstanceOfType(value)) return value;
		if (value is string s)
			return System.Windows.Markup.XamlValueConverter.Convert(null!, dp, s);
		if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(t) && !t.IsEnum)
			return Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
		var converter = TypeDescriptor.GetConverter(t);
		if (converter.CanConvertFrom(value.GetType()))
			return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
		return value;
	}

	static bool TryCoerceValid(DependencyProperty dp, object? value, out object? result)
	{
		try
		{
			result = CoerceToDpType(dp, value);
			return dp.IsValidValue(result);
		}
		catch (Exception ex) when (!IsCritical(ex))
		{
			result = null;
			return false;
		}
	}

	/// <summary>Sets <paramref name="value"/> when valid for the DP, else the fallback, else the default.</summary>
	internal static void SetTargetSafe(
		DependencyObject target, DependencyProperty dp, object? value, object? fallback)
	{
		if (ReferenceEquals(value, Binding.DoNothing)) return;
		if (!ReferenceEquals(value, DependencyProperty.UnsetValue) && dp.IsValidValue(value))
			target.SetValue(dp, value);
		else
			SetFallbackOrDefault(target, dp, fallback);
	}

	/// <summary>Like <see cref="SetTargetSafe"/> but first converts to the DP type (strings, IConvertible).</summary>
	internal static void SetTargetConverted(
		DependencyObject target, DependencyProperty dp, object? value, object? fallback)
	{
		if (ReferenceEquals(value, Binding.DoNothing)) return;
		if (!ReferenceEquals(value, DependencyProperty.UnsetValue) && TryCoerceValid(dp, value, out var v))
			target.SetValue(dp, v);
		else
			SetFallbackOrDefault(target, dp, fallback);
	}

	/// <summary>Sets the converted fallback when valid, otherwise the DP's metadata default.</summary>
	internal static void SetFallbackOrDefault(DependencyObject target, DependencyProperty dp, object? fallback)
	{
		if (!ReferenceEquals(fallback, DependencyProperty.UnsetValue) && TryCoerceValid(dp, fallback, out var fb))
			target.SetValue(dp, fb);
		else
			target.SetValue(dp, dp.GetMetadata(target.GetType()).DefaultValue);
	}
}
