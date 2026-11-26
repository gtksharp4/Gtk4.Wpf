using System;
using System.Reflection;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	// EventTrigger.RoutedEvent="Button.Click"; an unqualified name searches the enclosing Style/template
	// TargetType, then the framework base types.
	RoutedEvent ResolveRoutedEvent(string name)
	{
		int dot = name.LastIndexOf('.');
		if (dot > 0)
		{
			Type owner = ResolveTypeName(name.Substring(0, dot), _ctx);
			return FindRoutedEvent(owner, name.Substring(dot + 1))
				?? throw new FormatException("Cannot convert '" + name + "' to RoutedEvent.");
		}
		for (int i = _stack.Count - 1; i >= 0; i--)
		{
			Type? target = _stack[i] is Style style ? style.TargetType
				: _stack[i] is Controls.ControlTemplate template ? template.TargetType : null;
			if (target != null && FindRoutedEvent(target, name) is { } found) return found;
		}
		return FindRoutedEvent(typeof(FrameworkElement), name)
			?? throw new FormatException("Cannot convert '" + name + "' to RoutedEvent.");
	}

	static RoutedEvent? FindRoutedEvent(Type owner, string name) =>
		owner.GetField(name + "Event", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
			?.GetValue(null) as RoutedEvent;
}
