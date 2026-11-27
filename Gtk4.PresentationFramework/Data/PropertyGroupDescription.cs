using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Data;

/// <summary>
/// Describes the grouping of items using a named property on each item.
/// </summary>
public class PropertyGroupDescription : GroupDescription
{
	/// <summary>The property whose value determines the group.</summary>
	public string? PropertyName { get; set; }

	/// <summary>Initialises with no property (group by item itself).</summary>
	public PropertyGroupDescription() { }

	/// <summary>Initialises with the specified property name.</summary>
	public PropertyGroupDescription(string propertyName) => PropertyName = propertyName;

	/// <inheritdoc/>
	public override object? GroupNameFromItem(object item, int level, CultureInfo culture)
	{
		if (item == null) return null;
		if (string.IsNullOrEmpty(PropertyName)) return item;
		// An XML item is grouped by one of its attributes, written "@Type".
		if (item is System.Xml.XmlNode node && PropertyName!.StartsWith("@", StringComparison.Ordinal))
			return node.Attributes?[PropertyName.Substring(1)]?.Value;
		var prop = item.GetType().GetProperty(PropertyName!,
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		return prop?.GetValue(item);
	}
}
