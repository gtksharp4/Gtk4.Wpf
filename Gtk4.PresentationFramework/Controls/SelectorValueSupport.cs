using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Windows.Controls;

/// <summary>
/// Shared helper for <c>SelectedValue</c> / <c>SelectedValuePath</c> resolution
/// used by <see cref="ListBox"/> and <see cref="ComboBox"/>.
/// </summary>
internal static class SelectorValueSupport
{
	/// <summary>
	/// Resolves the value of <paramref name="item"/> at <paramref name="path"/>.
	/// <list type="bullet">
	/// <item><description>Empty/null path → returns <paramref name="item"/> itself.</description></item>
	/// <item><description>Dotted path → walks each segment in turn.</description></item>
	/// <item><description>Missing segment or null intermediate → returns <c>null</c>.</description></item>
	/// </list>
	/// </summary>
	/// <param name="item">The source object (may be <c>null</c>).</param>
	/// <param name="path">Property path string (e.g. <c>"Id"</c> or <c>"Address.City"</c>).</param>
	/// <returns>The resolved value, or <c>null</c> if unresolvable.</returns>
	internal static object? GetValue(object? item, string? path)
	{
		if (string.IsNullOrEmpty(path))
			return item;

		object? current = item;
		var segments = path!.Split('.');
		foreach (var segment in segments)
		{
			if (current == null) return null;
			var prop = current.GetType().GetProperty(segment, BindingFlags.Instance | BindingFlags.Public);
			if (prop == null) return null;
			current = prop.GetValue(current, null);
		}
		return current;
	}

	/// <summary>
	/// Returns the first item in <paramref name="items"/> whose value at
	/// <paramref name="path"/> equals <paramref name="value"/>
	/// (using <see cref="object.Equals(object,object)"/>), or <c>null</c> if none match.
	/// </summary>
	/// <param name="items">The candidate items to search.</param>
	/// <param name="path">The value path (forwarded to <see cref="GetValue"/>).</param>
	/// <param name="value">The value to match.</param>
	/// <returns>The first matching item, or <c>null</c>.</returns>
	internal static object? FindByValue(IList items, string? path, object? value)
	{
		foreach (var item in items)
		{
			if (Equals(GetValue(item, path), value))
				return item;
		}
		return null;
	}
}
