using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace System.Windows.Markup;

/// <summary>Reflection-based object builder behind <see cref="XamlReader"/>; one instance per load.</summary>
internal sealed partial class XamlObjectBuilder
{
	internal const string PresentationNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
	const string XNs = "http://schemas.microsoft.com/winfx/2009/xaml";
	const string XNs2006 = "http://schemas.microsoft.com/winfx/2006/xaml";
	const string McNs = "http://schemas.openxmlformats.org/markup-compatibility/2006";
	const string DesignNs = "http://schemas.microsoft.com/expression/blend/2008";

	// Objects under construction, outermost first; also the lookup chain for {StaticResource}.
	readonly List<object> _stack = new List<object>();
	readonly HashSet<string> _ignorable = new HashSet<string> { McNs, DesignNs };
	FrameworkElement? _scopeOwner;
	XElement _ctx = null!;

	/// <summary>The load's base URI (<see cref="ParserContext.BaseUri"/>): given to every created <see cref="IUriContext"/>.</summary>
	internal Uri? BaseUri { get; set; }

	internal object Build(XElement root)
	{
		foreach (XAttribute a in root.Attributes(XName.Get("Ignorable", McNs)))
			foreach (string prefix in a.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
				if (root.GetNamespaceOfPrefix(prefix) is XNamespace ns) _ignorable.Add(ns.NamespaceName);
		_ctx = root;
		return BuildElement(root, null, null)
			?? throw Fail(root, "The root element must create an object.");
	}

	static bool IsX(XNamespace ns) => ns.NamespaceName == XNs || ns.NamespaceName == XNs2006;

	object? BuildElement(XElement el, object? target, object? targetMember)
	{
		XElement saved = _ctx;
		_ctx = el;
		try
		{
			if (IsX(el.Name.Namespace)) return BuildXElement(el, target, targetMember);
			Type type = ResolveType(el.Name.NamespaceName, el.Name.LocalName, el);
			object instance = Create(type, el);
			if (BaseUri != null && instance is IUriContext uriContext && uriContext.BaseUri == null) uriContext.BaseUri = BaseUri;
			if (instance is FrameworkElement fe && _scopeOwner == null) _scopeOwner = fe;
			_stack.Add(instance);
			try
			{
				ApplyAttributes(el, instance);
				bool fromText = (instance is string || instance.GetType().IsValueType) && TextOf(el) != null;
				if (!fromText) ApplyChildren(el, instance);
			}
			finally
			{
				_stack.RemoveAt(_stack.Count - 1);
			}
			if (instance is MarkupExtension me) return me.ProvideValue(Provider(el, target, targetMember));
			if (instance is TemplateBindingExtension tb) return tb.ToBinding();
			return instance;
		}
		catch (XamlParseException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw Fail(el, ex.Message, Unwrap(ex));
		}
		finally
		{
			_ctx = saved;
		}
	}

	object? BuildXElement(XElement el, object? target, object? targetMember)
	{
		string local = el.Name.LocalName;
		if (local == "Null") return null;
		string? value = (string?)el.Attribute(local == "Type" ? "TypeName" : "Member") ?? TextOf(el);
		if (string.IsNullOrEmpty(value)) throw Fail(el, "x:" + local + " needs a value.");
		if (local == "Type") return ResolveTypeName(value!, el);
		if (local == "Static") return new StaticExtension(value!).ProvideValue(Provider(el, target, targetMember));
		throw Fail(el, "Unsupported XAML element 'x:" + local + "'.");
	}

	object Create(Type type, XElement el)
	{
		string? text = TextOf(el);
		bool valueLike = type == typeof(string) || type.IsPrimitive || type.IsEnum
			|| (type.IsValueType && !el.HasElements && text != null);
		if (valueLike && text != null) return ConvertString(type, text, null)!;
		if (type.IsAbstract || type.IsInterface) throw Fail(el, "Cannot create an instance of '" + type.Name + "'.");
		if (!type.IsValueType && !HasDefaultConstructor(type))
			throw Fail(el, "'" + type.Name + "' has no public parameterless constructor.");
		return Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance
			| BindingFlags.OptionalParamBinding, null, new object[0], null)!;
	}

	// A parameterless constructor, or one whose parameters are all optional (RotateTransform(double angle = 0)).
	static bool HasDefaultConstructor(Type type)
	{
		foreach (ConstructorInfo c in type.GetConstructors())
			if (System.Linq.Enumerable.All(c.GetParameters(), p => p.IsOptional)) return true;
		return false;
	}

	// The element's text when it has no child elements and non-blank text.
	static string? TextOf(XElement el)
	{
		if (el.HasElements) return null;
		string text = Normalize(el.Value);
		return text.Length == 0 ? null : text;
	}

	static string Normalize(string text)
	{
		var sb = new System.Text.StringBuilder();
		bool space = false;
		foreach (char c in text.Trim())
		{
			if (char.IsWhiteSpace(c)) { space = true; continue; }
			if (space) sb.Append(' ');
			space = false;
			sb.Append(c);
		}
		return sb.ToString();
	}

	static Exception Unwrap(Exception ex) => ex is TargetInvocationException { InnerException: { } inner } ? inner : ex;

	static XamlParseException Fail(XObject node, string message, Exception? inner = null)
	{
		int line = 0, pos = 0;
		if (node is IXmlLineInfo { } info && info.HasLineInfo()) { line = info.LineNumber; pos = info.LinePosition; }
		string full = line > 0 ? message + " Line number '" + line + "' and line position '" + pos + "'." : message;
		return inner == null ? new XamlParseException(full, line, pos) : new XamlParseException(full, line, pos, inner);
	}
}
