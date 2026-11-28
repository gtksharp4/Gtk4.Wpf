using System;
using System.Windows.Controls;
using System.Xml;

namespace System.Windows;

public partial class FrameworkElement
{
	/// <summary>
	/// The implicit <see cref="DataTemplate"/> for <paramref name="item"/>: for an XML element the template keyed by
	/// its element name (<c>DataType="Hero"</c>), then the item's type and each base type below object, searched
	/// through the element, its parents, the application and the theme resources.
	/// </summary>
	internal static DataTemplate? FindTemplateResource(FrameworkElement? element, object? item)
	{
		if (element == null || item == null) return null;
		if (item is XmlElement xml
			&& element.TryFindResourceCore(new DataTemplateKey(xml.Name), out var named) && named is DataTemplate nt)
			return nt;
		for (var type = item.GetType(); type != null && type != typeof(object); type = type.BaseType)
		{
			if (element.TryFindResourceCore(new DataTemplateKey(type), out var value) && value is DataTemplate t)
				return t;
		}
		return null;
	}
}
