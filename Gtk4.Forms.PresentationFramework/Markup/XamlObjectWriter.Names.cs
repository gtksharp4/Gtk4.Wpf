using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectWriter
{
	// clr-namespace URI -> prefix, in first-use order.
	readonly Dictionary<string, string> _clrPrefixes = new Dictionary<string, string>();

	static readonly ConcurrentDictionary<Type, object?> _defaults = new ConcurrentDictionary<Type, object?>();

	XNamespace NamespaceOf(Type type)
	{
		if (IsPresentationType(type)) return PresentationNs;
		string uri = "clr-namespace:" + type.Namespace + ";assembly=" + type.Assembly.GetName().Name;
		if (!_clrPrefixes.ContainsKey(uri))
			_clrPrefixes[uri] = _clrPrefixes.Count == 0 ? "clr" : "clr" + _clrPrefixes.Count;
		return uri;
	}

	static bool IsPresentationType(Type type) =>
		type.Namespace != null
		&& (type.Namespace == "System.Windows.Markup" || XamlObjectBuilder.IsPresentationNamespace(type.Namespace));

	// {x:Type ...} needs the x prefix declared on the root.
	string XType(Type type)
	{
		_usesX = true;
		return "{x:Type " + PrefixedName(type) + "}";
	}

	IEnumerable<XAttribute> NamespaceDeclarations() =>
		_clrPrefixes.Select(p => new XAttribute(XNamespace.Xmlns + p.Value, p.Key));

	static string NameOf(Type type) => type.Name;

	// "Name" in the default namespace, "prefix:Name" otherwise.
	string PrefixedName(Type type)
	{
		XNamespace ns = NamespaceOf(type);
		return ns == PresentationNs ? type.Name : _clrPrefixes[ns.NamespaceName] + ":" + type.Name;
	}

	// Public read/write CLR properties without a DependencyProperty that differ from a new instance.
	void WriteClrProperties(object obj, XElement el, string? contentName, List<object> content)
	{
		Type type = obj.GetType();
		object? fresh = _defaults.GetOrAdd(type, CreateDefault);
		foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.OrderBy(x => x.Name, StringComparer.Ordinal))
		{
			if (p.GetIndexParameters().Length > 0 || p.GetMethod == null) continue;
			if (p.SetMethod == null || !p.SetMethod.IsPublic) continue;
			if (p.Name == contentName || FindDp(type, p.Name) != null) continue;
			object? value = p.GetValue(obj);
			if (value == null || fresh != null && SameValue(value, p.GetValue(fresh))) continue;
			if (Format(value, p.PropertyType) is { } text) el.SetAttributeValue(p.Name, text);
		}
	}

	// The content property when it is a non-empty collection (Children, Inlines, ...).
	void CollectClrContent(object obj, string contentName, List<object> content)
	{
		if (FindDp(obj.GetType(), contentName) != null) return;
		// TextBlock.Text already stands for a lone Run; writing both would clear Text on load.
		if (obj is Controls.TextBlock { Text.Length: > 0 } tb && tb.Inlines.Count() == 1) return;
		PropertyInfo? p = obj.GetType().GetProperty(contentName, BindingFlags.Public | BindingFlags.Instance);
		if (p == null || p.GetIndexParameters().Length > 0 || p.GetValue(obj) is not { } value) return;
		if (value is IEnumerable items && value is not string)
			content.AddRange(items.Cast<object>().Select(WriteObject));
		else AddContent(value, content);
	}

	// Thickness.Equals is false for NaN sides (auto margins), which would write every default value.
	static bool SameValue(object a, object? b) => a is Thickness x && b is Thickness y
		? x.ToString() == y.ToString() : Equals(a, b);

	static object? CreateDefault(Type type)
	{
		try
		{
			return type.IsAbstract || type.GetConstructor(Type.EmptyTypes) == null && !type.IsValueType
				? null : Activator.CreateInstance(type);
		}
		catch (Exception ex) when (ex is TargetInvocationException || ex is MissingMethodException)
		{
			return null;
		}
	}
}
