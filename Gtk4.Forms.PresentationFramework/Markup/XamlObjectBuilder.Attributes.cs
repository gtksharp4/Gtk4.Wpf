using System;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	void ApplyAttributes(XElement el, object instance)
	{
		foreach (XAttribute attr in el.Attributes())
		{
			string ns = attr.Name.NamespaceName;
			if (attr.IsNamespaceDeclaration || _ignorable.Contains(ns) || ns == XNamespace.Xml.NamespaceName) continue;
			try
			{
				if (IsX(attr.Name.Namespace)) ApplyXAttribute(attr, instance);
				else ApplyAttribute(el, attr, instance);
			}
			catch (XamlParseException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw Fail(attr, ex.Message, Unwrap(ex));
			}
		}
	}

	void ApplyXAttribute(XAttribute attr, object instance)
	{
		switch (attr.Name.LocalName)
		{
			case "Name":
				RegisterName(instance, attr.Value);
				break;
			case "Class":
			case "Subclass":
				throw Fail(attr, "x:" + attr.Name.LocalName + " is not allowed in loose XAML.");
			case "TypeArguments":
				throw Fail(attr, "x:TypeArguments is not supported.");
		}
	}

	void RegisterName(object instance, string name)
	{
		if (instance is FrameworkElement fe) fe.Name = name;
		if (_scopeOwner != null) NameScope.Register(_scopeOwner, name, instance);
	}

	void ApplyAttribute(XElement el, XAttribute attr, object instance)
	{
		if (TryApplySetterProperty(el, attr, instance)) return;
		string name = attr.Name.LocalName;
		Type type = instance.GetType();
		Member? member = FindMemberForAttribute(el, attr, type);
		if (member == null)
		{
			if (name.IndexOf('.') < 0 && type.GetEvent(name) != null)
				throw Fail(attr, "Event handler '" + name + "' cannot be set in loose XAML (no code-behind).");
			throw Fail(attr, "Cannot find property '" + name + "' in type '" + type.Name + "'.");
		}
		if (member.Prop != null && TryApplyDefinitionShorthand(instance, name, attr.Value)) return;
		object? value = EvaluateValue(attr.Value, instance, member);
		if (ReferenceEquals(value, Applied)) return;
		Assign(instance, member, value);
		if (name == "Name" && value is string text && member.Prop != null) RegisterName(instance, text);
	}

	Member? FindMemberForAttribute(XElement el, XAttribute attr, Type type)
	{
		string name = attr.Name.LocalName;
		int dot = name.IndexOf('.');
		if (dot < 0) return FindMember(type, name);
		string ns = attr.Name.NamespaceName.Length > 0 ? attr.Name.NamespaceName : el.GetDefaultNamespace().NamespaceName;
		Type owner = ResolveType(ns, name.Substring(0, dot), el);
		string prop = name.Substring(dot + 1);
		return owner.IsAssignableFrom(type) ? FindMember(type, prop) : FindAttached(owner, prop);
	}

	// Plain text goes through the converters; "{...}" is a markup extension.
	object? EvaluateValue(string raw, object target, Member member)
	{
		if (IsExtension(raw)) return EvaluateExtension(raw, target, member);
		if (raw.StartsWith("{}", StringComparison.Ordinal)) raw = raw.Substring(2);
		return ConvertString(member.ValueType, raw, member.Dp);
	}
}
