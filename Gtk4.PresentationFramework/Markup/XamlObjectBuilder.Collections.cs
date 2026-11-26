using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	static bool IsCollection(object value) =>
		value is IList || value is IDictionary || value is ResourceDictionary || value is IAddChild
		|| ElementTypeOf(value.GetType()) != null;

	static Type? ElementTypeOf(Type type)
	{
		foreach (Type i in type.GetInterfaces())
			if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))
				return i.GetGenericArguments()[0];
		return AddMethodOf(type)?.GetParameters()[0].ParameterType;
	}

	// Collections that only expose a public Add(T) and IEnumerable (InlineCollection).
	static MethodInfo? AddMethodOf(Type type)
	{
		if (type == typeof(string) || !typeof(System.Collections.IEnumerable).IsAssignableFrom(type)) return null;
		foreach (MethodInfo m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
			if (m.Name == "Add" && m.GetParameters().Length == 1) return m;
		return null;
	}

	object? ReadKey(XElement el)
	{
		XAttribute? attr = el.Attribute(XName.Get("Key", XNs)) ?? el.Attribute(XName.Get("Key", XNs2006));
		if (attr == null) return null;
		return IsExtension(attr.Value) ? EvaluateExtension(attr.Value, null, null) : attr.Value;
	}

	void AddValue(object collection, object? item, Node node, XElement source)
	{
		XElement at = node.Element ?? source;
		if (item is string text && node.Element == null && !TextFitsCollection(collection))
			item = MakeRun(collection, text, at);
		if (collection is ResourceDictionary rd)
		{
			if (item is ResourceDictionary merged && (node.Element == null || ReadKey(node.Element) == null))
			{
				rd.CopyEntriesFrom(merged);
				return;
			}
			rd[KeyFor(node, item, at)] = item;
			return;
		}
		if (collection is IDictionary dict) { dict.Add(KeyFor(node, item, at), item); return; }
		if (collection is IList list) { list.Add(item); return; }
		if (collection is IAddChild add) { add.AddChild(item!); return; }
		MethodInfo? method = AddMethodOf(collection.GetType());
		if (method == null) throw Fail(at, "'" + collection.GetType().Name + "' cannot take this item.");
		method.Invoke(collection, new[] { item });
	}

	object KeyFor(Node node, object? item, XElement at) =>
		(node.Element != null ? ReadKey(node.Element) : null) ?? ImplicitKey(item)
			?? throw Fail(at, "Every resource needs an x:Key.");

	static object? ImplicitKey(object? item) => item is Style { TargetType: { } t } ? t : null;

	static bool TextFitsCollection(object collection)
	{
		Type? element = ElementTypeOf(collection.GetType());
		return element == null || element == typeof(object) || element == typeof(string);
	}

	// Bare text in an inline collection becomes a Run (TextBlock content).
	object MakeRun(object collection, string text, XElement at)
	{
		Type? element = ElementTypeOf(collection.GetType());
		if (element != null && element.IsAssignableFrom(typeof(Documents.Run))) return new Documents.Run { Text = text };
		throw Fail(at, "Text is not allowed in '" + collection.GetType().Name + "'.");
	}
}
