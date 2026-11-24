using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>Pure item-text and exact-match helpers for <see cref="ComboBox"/>.</summary>
internal static class ComboBoxItemText
{
	/// <summary>
	/// Text of an item: <paramref name="displayMemberPath"/> value, then a
	/// <see cref="ComboBoxItem"/>'s content, then <c>ToString()</c>.
	/// </summary>
	internal static string GetText(object? item, string? displayMemberPath)
	{
		if (item == null) return string.Empty;
		if (!string.IsNullOrEmpty(displayMemberPath))
		{
			var prop = item.GetType().GetProperty(displayMemberPath!);
			if (prop != null && prop.GetIndexParameters().Length == 0)
				return prop.GetValue(item)?.ToString() ?? string.Empty;
		}
		if (item is ComboBoxItem cbi)
			return ContentResolution.ToText(cbi.Content, cbi.ContentStringFormat);
		return ContentResolution.ToText(item, null);
	}

	/// <summary>Index of the first text equal to <paramref name="text"/>, or -1.</summary>
	internal static int IndexOfExact(IList<string> texts, string? text, bool caseSensitive)
	{
		if (text == null) return -1;
		var cmp = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
		for (int i = 0; i < texts.Count; i++)
			if (string.Equals(texts[i], text, cmp)) return i;
		return -1;
	}
}
