using System.Collections.Generic;

namespace System.Windows.Input;

/// <summary>
/// Pure helper for WPF tab-order computation.
/// GTK 4 uses visual-tree order for native tab navigation; this helper provides
/// the managed WPF tab order for consumers that want to drive focus programmatically.
/// </summary>
internal static class TabOrderHelper
{
	/// <summary>
	/// Returns elements in WPF tab order: sorted ascending by <paramref name="getTabIndex"/>,
	/// ties broken by declaration order (stable sort). Elements where
	/// <paramref name="getIsTabStop"/> returns <c>false</c> are excluded.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	/// <param name="elements">Source sequence (declaration order).</param>
	/// <param name="getTabIndex">Selector for the element's TabIndex value.</param>
	/// <param name="getIsTabStop">Selector for the element's IsTabStop value.</param>
	/// <returns>Ordered list; never null.</returns>
	internal static List<T> Order<T>(
		IEnumerable<T> elements,
		Func<T, int> getTabIndex,
		Func<T, bool> getIsTabStop)
	{
		if (elements == null) throw new ArgumentNullException(nameof(elements));
		if (getTabIndex == null) throw new ArgumentNullException(nameof(getTabIndex));
		if (getIsTabStop == null) throw new ArgumentNullException(nameof(getIsTabStop));

		// Build a list of (element, tabIndex, declarationOrder) tuples for stable sort.
		// netstandard2.0 has ValueTuple; using List<(T, int, int)> is safe here.
		var indexed = new List<(T Element, int TabIndex, int DeclarationOrder)>();
		int declarationOrder = 0;
		foreach (T el in elements)
		{
			if (getIsTabStop(el))
				indexed.Add((el, getTabIndex(el), declarationOrder));
			declarationOrder++;
		}

		// netstandard2.0-compatible stable sort via Comparison<T>:
		// sort by (TabIndex ASC, declarationOrder ASC).
		indexed.Sort((a, b) =>
		{
			int cmp = a.TabIndex.CompareTo(b.TabIndex);
			return cmp != 0 ? cmp : a.DeclarationOrder.CompareTo(b.DeclarationOrder);
		});

		var result = new List<T>(indexed.Count);
		for (int i = 0; i < indexed.Count; i++)
			result.Add(indexed[i].Element);
		return result;
	}
}
