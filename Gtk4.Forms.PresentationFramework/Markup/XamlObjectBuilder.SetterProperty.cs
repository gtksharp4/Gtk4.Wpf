using System;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	// Setter, Trigger and Condition .Property name a DP by short name, resolved against the enclosing
	// Style/ControlTemplate TargetType ("Grid.Row" names its owner). Unresolvable names stay PropertyName.
	bool TryApplySetterProperty(XElement el, XAttribute attr, object instance)
	{
		if (attr.Name.LocalName != "Property" || attr.Name.NamespaceName.Length != 0) return false;
		if (instance is not Setter && instance is not Trigger && instance is not Condition) return false;
		string raw = attr.Value.Trim();
		DependencyProperty? dp = ResolveSetterDp(el, raw);
		if (instance is Setter setter)
		{
			if (dp != null) setter.Property = dp;
			else setter.PropertyName = raw;
		}
		else if (instance is Condition condition)
		{
			if (dp != null) condition.Property = dp;
			else condition.PropertyName = raw;
		}
		else
		{
			var trigger = (Trigger)instance;
			if (dp != null) trigger.Property = dp;
			else trigger.PropertyName = raw;
		}
		return true;
	}

	DependencyProperty? ResolveSetterDp(XElement el, string name)
	{
		int dot = name.IndexOf('.');
		if (dot > 0)
		{
			Type owner = ResolveType(el.GetDefaultNamespace().NamespaceName, name.Substring(0, dot), el);
			return FindDp(owner, name.Substring(dot + 1));
		}
		for (int i = _stack.Count - 1; i >= 0; i--)
		{
			Type? target = _stack[i] is Style style ? style.TargetType
				: _stack[i] is Controls.ControlTemplate template ? template.TargetType : null;
			if (target != null) return FindDp(target, name);
		}
		return null;
	}
}
