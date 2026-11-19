using System;
using System.Collections.Concurrent;

namespace System.Windows;

public partial class Setter
{
	static readonly ConcurrentDictionary<string, Type?> _ownerTypes = new ConcurrentDictionary<string, Type?>();

	/// <summary>
	/// Resolves <see cref="Property"/>, else <see cref="PropertyName"/> against the target type.
	/// Accepts <c>Owner.Name</c> and <c>(Owner.Name)</c>; the owner is looked up among the target's
	/// base types, then the types of this assembly.
	/// </summary>
	DependencyProperty? ResolveProperty(DependencyObject target)
	{
		if (_property != null) return _property;
		var name = PropertyName?.Trim();
		if (string.IsNullOrEmpty(name)) return null;
		if (name!.Length > 1 && name[0] == '(' && name[name.Length - 1] == ')')
			name = name.Substring(1, name.Length - 2);
		int dot = name.LastIndexOf('.');
		if (dot < 0) return DependencyProperty.FindByName(name, target.GetType());
		var owner = FindOwnerType(name.Substring(0, dot), target.GetType());
		return owner == null ? null : DependencyProperty.FindByName(name.Substring(dot + 1), owner);
	}

	/// <summary>Converts a string value to the property type; other values pass through unchanged.</summary>
	static object? CoerceValue(DependencyProperty dp, object? rawValue)
	{
		if (rawValue is string s && dp.PropertyType != typeof(string))
			return System.Windows.Markup.XamlValueConverter.Convert(dp.OwnerType, dp, s);
		return rawValue;
	}

	static Type? FindOwnerType(string ownerName, Type targetType)
	{
		for (var t = targetType; t != null; t = t.BaseType)
			if (t.Name == ownerName) return t;
		return _ownerTypes.GetOrAdd(ownerName, n =>
		{
			foreach (var t in typeof(Setter).Assembly.GetTypes())
				if (t.Name == n && typeof(DependencyObject).IsAssignableFrom(t) && t.IsPublic) return t;
			return null;
		});
	}
}
