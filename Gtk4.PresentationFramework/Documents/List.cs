using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>Specifies the marker style for a <see cref="List"/>.</summary>
public enum TextMarkerStyle
{
	/// <summary>No marker.</summary>
	None,
	/// <summary>Filled disc bullet (default).</summary>
	Disc,
	/// <summary>Open circle bullet.</summary>
	Circle,
	/// <summary>Filled square bullet.</summary>
	Square,
	/// <summary>Decimal numbers (1, 2, 3…).</summary>
	Decimal,
	/// <summary>Lowercase letters (a, b, c…).</summary>
	LowerLatin,
	/// <summary>Uppercase letters (A, B, C…).</summary>
	UpperLatin,
	/// <summary>Lowercase Roman numerals (i, ii, iii…).</summary>
	LowerRoman,
	/// <summary>Uppercase Roman numerals (I, II, III…).</summary>
	UpperRoman,
}

/// <summary>
/// A block element that contains an ordered or unordered list of <see cref="ListItem"/>s.
/// </summary>
[ContentProperty("ListItems")]
public partial class List : Block
{
	/// <summary>The marker style for the list items.</summary>
	public TextMarkerStyle MarkerStyle { get; set; } = TextMarkerStyle.Disc;

	/// <summary>The items in the list.</summary>
	public ListItemCollection ListItems { get; }

	/// <summary>Creates an empty list.</summary>
	public List() => ListItems = new ListItemCollection(this);

	/// <inheritdoc/>
	internal override void AppendPlainText(System.Text.StringBuilder sb)
	{
		int number = 1;
		foreach (var item in ListItems)
		{
			sb.Append(GetMarker(MarkerStyle, number));
			sb.Append(' ');
			item.AppendPlainText(sb);
			sb.Append('\n');
			number++;
		}
	}

	/// <inheritdoc/>
	internal override void AppendMarkup(System.Text.StringBuilder sb) => AppendItems(sb, null);

	/// <inheritdoc/>
	internal override void AppendFlowMarkup(System.Text.StringBuilder sb, FlowDirection? effective) =>
		AppendLanguage(sb, s => AppendItems(s, effective));

	// Each item line is its own paragraph, so it carries its own isolate.
	void AppendItems(System.Text.StringBuilder sb, FlowDirection? effective)
	{
		int number = 1;
		foreach (var item in ListItems)
		{
			string marker = PangoMarkup.Escape(GetMarker(MarkerStyle, number)) + " ";
			if ((item.ExplicitFlowDirection ?? effective) is FlowDirection d)
				BidiMarks.AppendIsolated(sb, d, true, s => { s.Append(marker); item.AppendMarkup(s); });
			else
			{
				sb.Append(marker);
				item.AppendMarkup(sb);
			}
			sb.Append('\n');
			number++;
		}
	}

	internal static string GetMarker(TextMarkerStyle style, int number) => style switch
	{
		TextMarkerStyle.None => string.Empty,
		TextMarkerStyle.Disc => "•",
		TextMarkerStyle.Circle => "◦",
		TextMarkerStyle.Square => "▪",
		TextMarkerStyle.Decimal => number.ToString(),
		TextMarkerStyle.LowerLatin => ToAlpha(number, lower: true),
		TextMarkerStyle.UpperLatin => ToAlpha(number, lower: false),
		TextMarkerStyle.LowerRoman => ToRoman(number, lower: true),
		TextMarkerStyle.UpperRoman => ToRoman(number, lower: false),
		_ => "•"
	};

	static string ToAlpha(int n, bool lower)
	{
		n = Math.Max(1, n);
		var c = (char)('a' + (n - 1) % 26);
		return lower ? c.ToString() : char.ToUpperInvariant(c).ToString();
	}

	static string ToRoman(int n, bool lower)
	{
		if (n < 1 || n > 3999) return n.ToString();
		var result = new System.Text.StringBuilder();
		int[] vals = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
		string[] syms = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
		for (int i = 0; i < vals.Length; i++)
			while (n >= vals[i]) { result.Append(syms[i]); n -= vals[i]; }
		return lower ? result.ToString().ToLowerInvariant() : result.ToString();
	}
}

/// <summary>
/// An ordered collection of <see cref="ListItem"/>s owned by a <see cref="List"/>.
/// </summary>
public sealed partial class ListItemCollection : IEnumerable<ListItem>
{
	readonly List<ListItem> _items = new List<ListItem>();
	readonly DependencyObject? _owner;

	/// <summary>Creates a collection with no owner.</summary>
	public ListItemCollection() { }

	internal ListItemCollection(DependencyObject owner) => _owner = owner;

	/// <summary>Number of items.</summary>
	public int Count => _items.Count;

	/// <summary>Gets the item at the given index.</summary>
	public ListItem this[int index] => _items[index];

	/// <summary>Adds a list item.</summary>
	public void Add(ListItem item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		ContentParenting.Adopt(_owner, item, strict: false);
		_items.Add(item);
		ContentParenting.Changed(_owner);
	}

	/// <summary>Removes an item; false when it is not in the collection.</summary>
	public bool Remove(ListItem item)
	{
		if (!_items.Remove(item)) return false;
		ContentParenting.Release(_owner, item);
		ContentParenting.Changed(_owner);
		return true;
	}

	/// <summary>Removes all list items.</summary>
	public void Clear()
	{
		foreach (var item in _items)
			ContentParenting.Release(_owner, item);
		_items.Clear();
		ContentParenting.Changed(_owner);
	}

	/// <inheritdoc/>
	public IEnumerator<ListItem> GetEnumerator() => _items.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}