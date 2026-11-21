using System;
using System.Collections;

namespace System.Windows;

public partial class Application
{
	readonly IDictionary _properties = Hashtable.Synchronized(new Hashtable());

	/// <summary>Application-scoped state shared by all code; synchronized.</summary>
	public IDictionary Properties => _properties;

	/// <summary>
	/// Looks the key up in <see cref="Resources"/>, then in the built-in theme styles (for a
	/// <see cref="Type"/> key). Throws <see cref="InvalidOperationException"/> when absent, as
	/// <see cref="FrameworkElement.FindResource"/> does.
	/// </summary>
	public object? FindResource(object resourceKey)
	{
		if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));
		if (TryFindResourceCore(resourceKey, out var value))
			return value;
		throw new InvalidOperationException($"Resource with key '{resourceKey}' not found.");
	}

	/// <summary>Like <see cref="FindResource"/>, but returns <see langword="null"/> when absent.</summary>
	public object? TryFindResource(object resourceKey)
	{
		if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));
		TryFindResourceCore(resourceKey, out var value);
		return value;
	}

	bool TryFindResourceCore(object key, out object? value)
	{
		if (Resources.TryGetValue(key, out value))
			return true;
		if (key is Type type && Themes.ThemeManager.FindDefaultStyle(type) is { } style)
		{
			value = style;
			return true;
		}
		return SystemResources.TryFind(key, out value);
	}
}
