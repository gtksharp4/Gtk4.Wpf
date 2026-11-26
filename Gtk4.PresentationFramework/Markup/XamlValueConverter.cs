using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace System.Windows.Markup;

/// <summary>
/// Central runtime registry for XAML attribute-value type conversion.
/// <para>
/// The markup compiler emits calls to <see cref="Convert(Type,DependencyProperty,string)"/>
/// for every plain (non-markup-extension) attribute whose target is a
/// <see cref="DependencyProperty"/>. The converter dispatches on
/// <see cref="DependencyProperty.PropertyType"/> to produce a strongly-typed value.
/// </para>
/// <para>
/// Custom converters can be registered via
/// <see cref="Register{T}(Func{string,T})"/>; built-ins cover all WPF primitive and
/// media types shipped in this repository. When <c>PropertyType == typeof(string)</c>
/// the raw string is returned unchanged.
/// </para>
/// </summary>
public static partial class XamlValueConverter
{
	static readonly Dictionary<Type, Func<string, object?>> _converters =
		new Dictionary<Type, Func<string, object?>>();

	static XamlValueConverter()
	{
		RegisterBuiltIns();
	}

	// ── Public API ───────────────────────────────────────────────────────────

	/// <summary>
	/// Registers a converter for type <typeparamref name="T"/>.
	/// Overwrites any previously registered converter for the same type.
	/// </summary>
	public static void Register<T>(Func<string, T> converter)
	{
		if (converter == null) throw new ArgumentNullException(nameof(converter));
		_converters[typeof(T)] = raw => converter(raw);
	}

	/// <summary>
	/// Converts <paramref name="rawValue"/> to the type required by
	/// <paramref name="dp"/>.PropertyType.
	/// <list type="bullet">
	/// <item>When <c>PropertyType == typeof(string)</c>, returns <paramref name="rawValue"/> unchanged.</item>
	/// <item>When no converter is registered, returns <paramref name="rawValue"/> as-is (string).</item>
	/// <item>On parse failure throws <see cref="FormatException"/> naming the property and value.</item>
	/// </list>
	/// </summary>
	/// <param name="ownerType">Declaring type (used only for error messages).</param>
	/// <param name="dp">The dependency property whose type drives conversion.</param>
	/// <param name="rawValue">The raw string from XAML.</param>
	public static object? Convert(Type ownerType, DependencyProperty dp, string rawValue)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));

		Type targetType = dp.PropertyType;
		if (targetType == typeof(string)) return rawValue;

		// Nullable<T> → unwrap to T for lookup.
		Type lookupType = Nullable.GetUnderlyingType(targetType) ?? targetType;

		if (TryGetConverter(dp, lookupType, out var converter))
		{
			try
			{
				return converter(rawValue);
			}
			catch (FormatException ex)
			{
				throw new FormatException(
					$"Cannot convert value \"{rawValue}\" for property " +
					$"{(ownerType != null ? ownerType.Name + "." : "")}{dp.Name} " +
					$"(type {targetType.Name}): {ex.Message}", ex);
			}
			catch (Exception ex) when (ex is OverflowException || ex is ArgumentException)
			{
				throw new FormatException(
					$"Cannot convert value \"{rawValue}\" for property " +
					$"{(ownerType != null ? ownerType.Name + "." : "")}{dp.Name} " +
					$"(type {targetType.Name}): {ex.Message}", ex);
			}
		}

		// Enum: dispatch generically (case-insensitive).
		if (lookupType.IsEnum)
		{
			try
			{
				return Enum.Parse(lookupType, rawValue, ignoreCase: true);
			}
			catch (Exception ex)
			{
				throw new FormatException(
					$"Cannot convert value \"{rawValue}\" for property " +
					$"{(ownerType != null ? ownerType.Name + "." : "")}{dp.Name} " +
					$"(enum {lookupType.Name}): {ex.Message}", ex);
			}
		}

		// No converter available — return the raw string and let the
		// runtime deal with it (best-effort for unregistered types).
		return rawValue;
	}

	/// <summary>
	/// Converts <paramref name="rawValue"/> to <paramref name="targetType"/> directly
	/// (no <see cref="DependencyProperty"/> needed — used for animation value types
	/// that are not registered as DPs on the animation class itself).
	/// </summary>
	public static object? ConvertByType(Type targetType, string rawValue)
	{
		if (targetType == null) throw new ArgumentNullException(nameof(targetType));
		if (targetType == typeof(string)) return rawValue;

		Type lookupType = Nullable.GetUnderlyingType(targetType) ?? targetType;
		if (_converters.TryGetValue(lookupType, out var converter))
			return converter(rawValue);

		if (lookupType.IsEnum)
			return Enum.Parse(lookupType, rawValue, ignoreCase: true);

		return rawValue;
	}

	/// <summary>
	/// Converts <paramref name="rawValue"/> to the given <paramref name="targetType"/>,
	/// with <paramref name="ownerType"/> and a null DP (for contexts where there is no DP).
	/// Used by compiler-emitted animation property assignments.
	/// </summary>
	public static object? Convert(Type ownerType, DependencyProperty? dp, Type targetType, string rawValue)
	{
		if (targetType == null) throw new ArgumentNullException(nameof(targetType));
		return ConvertByType(targetType, rawValue);
	}

	// FontSize accepts WPF FontSizeConverter units (px/in/cm/pt); a plain double converter would reject them.
	static bool TryGetConverter(DependencyProperty dp, Type lookupType, out Func<string, object?> converter)
	{
		if (ReferenceEquals(dp, System.Windows.Documents.TextElement.FontSizeProperty))
		{
			converter = raw => FontConverters.ParseFontSize(raw);
			return true;
		}
		return _converters.TryGetValue(lookupType, out converter!);
	}

	// ── Built-in registration ────────────────────────────────────────────────

	static void RegisterBuiltIns()
	{
		// Scalars
		Register<double>(UnitConverters.ParseLength);
		Register<float>(PrimitiveConverters.ParseFloat);
		Register<int>(PrimitiveConverters.ParseInt);
		Register<long>(raw => long.Parse(raw, System.Globalization.CultureInfo.InvariantCulture));
		Register<bool>(PrimitiveConverters.ParseBool);

		// WPF primitives (WindowsBase)
		Register<Thickness>(PrimitiveConverters.ParseThickness);
		Register<CornerRadius>(PrimitiveConverters.ParseCornerRadius);
		Register<GridLength>(PrimitiveConverters.ParseGridLength);
		Register<Point>(PrimitiveConverters.ParsePoint);
		Register<Size>(PrimitiveConverters.ParseSize);
		Register<Rect>(PrimitiveConverters.ParseRect);
		Register<Media.Geometry>(Media.Geometry.Parse);

		// Media (PresentationFramework)
		Register<Color>(MediaConverters.ParseColor);
		Register<Brush>(MediaConverters.ParseBrush);
		Register<SolidColorBrush>(raw => (SolidColorBrush)MediaConverters.ParseBrush(raw));
		Register<FontFamily>(FontConverters.ParseFontFamily);
		Register<System.Windows.Markup.XmlLanguage>(System.Windows.Markup.XmlLanguage.GetLanguage);
		Register<FontWeight>(FontConverters.ParseFontWeight);
		Register<FontStyle>(FontConverters.ParseFontStyle);
		Register<FontStretch>(FontConverters.ParseFontStretch);
		Register<Transform>(UnitConverters.ParseTransform);
		Register<ImageSource>(UnitConverters.ParseImageSource);
		Register<Uri>(UnitConverters.ParseUri);
		Register<System.Windows.Shapes.PointCollection>(UnitConverters.ParsePointCollection);
		Register<System.Windows.Input.ICommand?>(CommandConverters.ParseCommand);

		RegisterInputBuiltIns();
		RegisterMedia3DBuiltIns();
		RegisterImagingBuiltIns();

		// Animation primitives
		Register<Duration>(AnimationConverters.ParseDuration);
		Register<RepeatBehavior>(AnimationConverters.ParseRepeatBehavior);
	}
}