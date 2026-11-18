using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Controls;

/// <summary>
/// Pure sort helpers for <see cref="DataGrid"/> column sorting.
/// Testable without GTK widgets.
/// </summary>
internal static class DataGridSort
{
	/// <summary>
	/// Sorts <paramref name="items"/> by <paramref name="memberPath"/> in
	/// <paramref name="direction"/>, returning a new stable-sorted list.
	/// Paths are resolved by reflection: a single segment reads that property, and a
	/// dotted path (e.g. <c>Address.City</c>) resolves recursively segment by segment
	/// (null-safe — a missing/null segment yields null). Null values sort before non-null.
	/// </summary>
	internal static List<object?> SortItems(
		IEnumerable items,
		string memberPath,
		ListSortDirection direction)
	{
		var list = new List<object?>();
		foreach (var item in items)
			list.Add(item);

		if (string.IsNullOrEmpty(memberPath))
			return list;

		// Stable sort: index used as tiebreaker.
		var indexed = new List<KeyValuePair<int, object?>>(list.Count);
		for (int i = 0; i < list.Count; i++)
			indexed.Add(new KeyValuePair<int, object?>(i, list[i]));

		indexed.Sort((a, b) =>
		{
			var va = GetMemberValue(a.Value, memberPath);
			var vb = GetMemberValue(b.Value, memberPath);
			int cmp = CompareValues(va, vb);
			if (direction == ListSortDirection.Descending)
				cmp = -cmp;
			// Tiebreaker: preserve original order (stable).
			return cmp != 0 ? cmp : a.Key.CompareTo(b.Key);
		});

		var result = new List<object?>(indexed.Count);
		foreach (var kv in indexed)
			result.Add(kv.Value);
		return result;
	}

	/// <summary>
	/// Reads the first segment of <paramref name="path"/> from <paramref name="item"/>
	/// by reflection. Returns null for null items or unresolvable paths.
	/// </summary>
	internal static object? GetMemberValue(object? item, string path)
	{
		if (item == null || string.IsNullOrEmpty(path))
			return null;

		// First segment only (nested path: "A.B" → try "A.B" first, then "A").
		var prop = item.GetType().GetProperty(
			path,
			BindingFlags.Public | BindingFlags.Instance);

		if (prop != null)
			return prop.GetValue(item);

		// Try first segment of a dotted path.
		int dot = path.IndexOf('.');
		if (dot > 0)
		{
			var first = path.Substring(0, dot);
			var nested = item.GetType().GetProperty(
				first,
				BindingFlags.Public | BindingFlags.Instance);
			if (nested != null)
			{
				var nestedItem = nested.GetValue(item);
				return GetMemberValue(nestedItem, path.Substring(dot + 1));
			}
		}

		return null;
	}

	static int CompareValues(object? a, object? b)
	{
		if (a == null && b == null) return 0;
		if (a == null) return -1;
		if (b == null) return 1;
		if (a is IComparable ca)
			return ca.CompareTo(b);
		return string.Compare(
			a.ToString(),
			b.ToString(),
			StringComparison.OrdinalIgnoreCase);
	}
}
