using System;
using System.Windows.Controls;

namespace System.Windows;

public partial class FrameworkElement
{
	/// <summary>
	/// The implicit <see cref="DataTemplate"/> for <paramref name="item"/>: the item's type, then each base type
	/// below object, searched through the element, its parents, the application and the theme resources.
	/// </summary>
	internal static DataTemplate? FindTemplateResource(FrameworkElement? element, object? item)
	{
		if (element == null || item == null) return null;
		for (var type = item.GetType(); type != null && type != typeof(object); type = type.BaseType)
		{
			if (element.TryFindResourceCore(new DataTemplateKey(type), out var value) && value is DataTemplate t)
				return t;
		}
		return null;
	}
}
