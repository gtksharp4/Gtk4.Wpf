using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Xml.Linq;

namespace System.Windows.Markup;

/// <summary>Builds the XAML element tree behind <see cref="XamlWriter"/>.</summary>
internal sealed partial class XamlObjectWriter
{
	static readonly XNamespace XNs = "http://schemas.microsoft.com/winfx/2006/xaml";
	static readonly XNamespace PresentationNs = XamlObjectBuilder.PresentationNs;

	bool _usesX;

	internal XElement Write(object obj)
	{
		if (obj == null) throw new ArgumentNullException(nameof(obj));
		XElement root = WriteObject(obj);
		if (_usesX) root.SetAttributeValue(XNamespace.Xmlns + "x", XNs.NamespaceName);
		foreach (XAttribute a in NamespaceDeclarations()) root.SetAttributeValue(a.Name, a.Value);
		return root;
	}

	XElement WriteObject(object obj)
	{
		Type type = obj.GetType();
		XNamespace ns = NamespaceOf(type);
		var el = new XElement(ns + type.Name);
		if ((type == typeof(string) || type.IsValueType) && Format(obj, type) is { } text)
		{
			el.Value = text;
			return el;
		}
		string? contentName = type.GetCustomAttribute<ContentPropertyAttribute>(true)?.Name;
		var content = new List<object>();
		if (obj is ResourceDictionary rd) WriteDictionary(rd, el, ns);
		else
		{
			if (obj is DependencyObject d) WriteLocalValues(d, el, ns, contentName, content);
			WriteClrProperties(obj, el, contentName, content);
			if (obj is FrameworkElement fe && fe.Resources.Count > 0)
				el.Add(new XElement(ns + (type.Name + ".Resources"), WriteObject(fe.Resources)));
			if (contentName != null) CollectClrContent(obj, contentName, content);
		}
		foreach (object child in content) el.Add(child);
		return el;
	}

	void WriteDictionary(ResourceDictionary rd, XElement el, XNamespace ns)
	{
		if (rd.MergedDictionaries.Count > 0)
			el.Add(new XElement(
				ns + "ResourceDictionary.MergedDictionaries", rd.MergedDictionaries.Select(WriteObject)));
		foreach (var pair in rd)
		{
			if (pair.Value == null) continue;
			XElement entry = WriteObject(pair.Value);
			_usesX = true;
			entry.SetAttributeValue(XNs + "Key", KeyText(pair.Key));
			el.Add(entry);
		}
	}

	string KeyText(object key) => key is Type t
		? XType(t)
		: Convert.ToString(key, System.Globalization.CultureInfo.InvariantCulture)!;

	void WriteLocalValues(DependencyObject d, XElement el, XNamespace ns, string? contentName, List<object> content)
	{
		Type type = d.GetType();
		var entries = new List<LocalValueEntry>();
		LocalValueEnumerator values = d.GetLocalValueEnumerator();
		while (values.MoveNext()) entries.Add(values.Current);
		foreach (LocalValueEntry e in entries.OrderBy(x => x.Property.Name, StringComparer.Ordinal))
		{
			DependencyProperty dp = e.Property;
			if (dp.ReadOnly || e.Value == null || ReferenceEquals(e.Value, DependencyProperty.UnsetValue)) continue;
			bool attached = FindDp(type, dp.Name) != dp;
			string name = attached ? NameOf(dp.OwnerType) + "." + dp.Name : dp.Name;
			if (ExtensionText(d, dp) is { } extension)
			{
				el.SetAttributeValue(name, extension);
				continue;
			}
			if (BindingOperations.IsDataBound(d, dp) || IsDefault(dp, type, e.Value)) continue;
			if (!attached && dp.Name == contentName) { AddContent(e.Value, content); continue; }
			if (Format(e.Value, dp.PropertyType) is { } text)
			{
				el.SetAttributeValue(name, text);
				continue;
			}
			if (IsWritableObject(e.Value))
			{
				string owner = NameOf(attached ? dp.OwnerType : type);
				el.Add(new XElement(ns + (owner + "." + dp.Name), ChildrenOf(e.Value)));
			}
		}
	}

	// A default-equal local value on an inheriting property still overrides an inherited non-default value.
	static bool IsDefault(DependencyProperty dp, Type type, object value)
	{
		var metadata = dp.GetMetadata(type);
		if (metadata is FrameworkPropertyMetadata { Inherits: true }) return false;
		return Equals(metadata.DefaultValue, value);
	}

	object[] ChildrenOf(object value) =>
		value is IEnumerable items && value is not string && value is not ResourceDictionary
		&& !(value is DependencyObject)
			? items.Cast<object>().Select(WriteObject).ToArray()
			: new object[] { WriteObject(value) };

	void AddContent(object value, List<object> content)
	{
		if (Format(value, value.GetType()) is { } text) content.Add(text);
		else if (IsWritableObject(value)) content.AddRange(ChildrenOf(value));
	}

	static bool IsWritableObject(object value) =>
		value is DependencyObject || value is ResourceDictionary || value is IEnumerable && value is not string;

	static DependencyProperty? FindDp(Type type, string name) =>
		type.GetField(name + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
			?.GetValue(null) as DependencyProperty;
}
