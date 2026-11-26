using System;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Markup;

/// <summary>Layer hooks the markup compiler emits for types it cannot inspect at build time.</summary>
public static partial class XamlValueConverter
{
	/// <summary>
	/// Layer hook: assigns <paramref name="raw"/> to the public instance property <paramref name="name"/>
	/// of <paramref name="target"/>, converted to the property type.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void SetClrProperty(object target, string name, string raw)
	{
		PropertyInfo prop = FindClrProperty(target, name);
		prop.SetValue(target, ConvertText(prop.PropertyType, raw));
	}

	/// <summary>
	/// Layer hook: assigns <paramref name="value"/> (a resource) to the property <paramref name="name"/>
	/// of <paramref name="target"/>; a string is converted to the property type first.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void SetClrValue(object target, string name, object? value)
	{
		PropertyInfo prop = FindClrProperty(target, name);
		prop.SetValue(target, CoerceResource(prop.PropertyType, value));
	}

	/// <summary>
	/// Layer hook: a resource stored as XAML text (e.g. <c>&lt;Color x:Key="c"&gt;Red&lt;/Color&gt;</c>) is
	/// converted to <paramref name="targetType"/>; any other value is returned unchanged.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static object? CoerceResource(Type targetType, object? value)
	{
		if (targetType == null) throw new ArgumentNullException(nameof(targetType));
		return value is string s && !targetType.IsInstanceOfType(value) ? ConvertText(targetType, s) : value;
	}

	static PropertyInfo FindClrProperty(object target, string name)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		return target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
			?? throw new InvalidOperationException(
				$"Type '{target.GetType().FullName}' has no public property '{name}'.");
	}
}
