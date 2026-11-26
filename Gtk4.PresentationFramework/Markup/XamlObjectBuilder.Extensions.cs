using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	static bool IsExtension(string raw) =>
		raw.Length > 1 && raw[0] == '{' && raw[1] != '}' && raw[raw.Length - 1] == '}';

	// target/member give the provider its context; null for a key or other free-standing value.
	object? EvaluateExtension(string raw, object? target, Member? member)
	{
		string body = raw.Substring(1, raw.Length - 2).Trim();
		int space = 0;
		while (space < body.Length && !char.IsWhiteSpace(body[space])) space++;
		string name = body.Substring(0, space);
		var positional = new List<string>();
		var named = new List<KeyValuePair<string, string>>();
		foreach (string part in SplitTopLevel(body.Substring(space)))
		{
			int eq = IndexOfTopLevel(part, '=');
			if (eq > 0)
			{
				string value = Unquote(part.Substring(eq + 1));
				named.Add(new KeyValuePair<string, string>(part.Substring(0, eq).Trim(), value));
			}
			else if (part.Trim().Length > 0) positional.Add(Unquote(part));
		}
		string kind = KindOf(name);
		string? first = positional.Count > 0 ? positional[0] : null;
		switch (kind)
		{
			case "Null": return null;
			case "Type": return ResolveTypeName(first ?? Named(named, "TypeName") ?? "", _ctx);
			case "Static":
				return new StaticExtension(first ?? Named(named, "Member") ?? "")
					.ProvideValue(Provider(_ctx, target, (object?)member?.Dp ?? member?.Prop));
			case "StaticResource": return LookupStatic(Argument(first ?? Named(named, "ResourceKey")));
			case "DynamicResource": return Dynamic(Argument(first ?? Named(named, "ResourceKey")), target, member);
			case "TemplateBinding":
				return new TemplateBindingExtension(first ?? Named(named, "Property")).ToBinding();
		}
		return Construct(name, positional, named, target, member);
	}

	// "x:Static" -> Static; plain StaticResource/DynamicResource/TemplateBinding keep their name.
	string KindOf(string name)
	{
		int colon = name.IndexOf(':');
		if (colon < 0) return name;
		XNamespace? ns = _ctx.GetNamespaceOfPrefix(name.Substring(0, colon));
		return ns != null && IsX(ns) ? name.Substring(colon + 1) : name;
	}

	static string? Named(List<KeyValuePair<string, string>> named, string key)
	{
		foreach (var pair in named) if (pair.Key == key) return pair.Value;
		return null;
	}

	object Argument(string? raw)
	{
		if (raw == null) throw new InvalidOperationException("The markup extension needs a key.");
		return IsExtension(raw) ? EvaluateExtension(raw, null, null)! : raw;
	}

	object? Dynamic(object key, object? target, Member? member)
	{
		if (target is FrameworkElement fe && member?.Dp != null)
		{
			XamlValueConverter.SetLocalResourceReference(fe, member.Dp, key);
			return Applied;
		}
		return TryLookup(key, out object? value) ? value : null;
	}

	object? Construct(string name, List<string> positional, List<KeyValuePair<string, string>> named,
		object? target, Member? member)
	{
		Type type = TryResolveExtensionType(name)
			?? throw new InvalidOperationException("Cannot find markup extension '" + name + "'.");
		object instance = CreateWithArguments(type, positional);
		foreach (var pair in named)
		{
			Member m = FindMember(type, pair.Key)
				?? throw new InvalidOperationException("'" + type.Name + "' has no property '" + pair.Key + "'.");
			object? value = EvaluateValue(pair.Value, instance, m);
			if (!ReferenceEquals(value, Applied)) Assign(instance, m, value);
		}
		if (instance is MarkupExtension me)
			return me.ProvideValue(Provider(_ctx, target, (object?)member?.Dp ?? member?.Prop));
		return instance;
	}

	Type? TryResolveExtensionType(string name)
	{
		int colon = name.IndexOf(':');
		string prefix = colon < 0 ? "" : name.Substring(0, colon);
		string local = colon < 0 ? name : name.Substring(colon + 1);
		XNamespace? ns = prefix.Length == 0 ? _ctx.GetDefaultNamespace() : _ctx.GetNamespaceOfPrefix(prefix);
		if (ns == null) return null;
		return TryResolveType(ns.NamespaceName, local + "Extension") ?? TryResolveType(ns.NamespaceName, local);
	}

	object CreateWithArguments(Type type, List<string> positional)
	{
		if (positional.Count == 0) return Activator.CreateInstance(type)!;
		foreach (ConstructorInfo ctor in type.GetConstructors())
		{
			ParameterInfo[] ps = ctor.GetParameters();
			if (ps.Length != positional.Count) continue;
			try
			{
				var args = new object?[ps.Length];
				for (int i = 0; i < ps.Length; i++)
					args[i] = IsExtension(positional[i])
						? EvaluateExtension(positional[i], null, null)
						: ConvertString(ps[i].ParameterType, positional[i], null);
				return ctor.Invoke(args);
			}
			catch (Exception ex) when (
				ex is FormatException || ex is TargetInvocationException || ex is InvalidCastException)
			{
			}
		}
		throw new InvalidOperationException(
			"No constructor of '" + type.Name + "' takes " + positional.Count + " argument(s).");
	}

	static List<string> SplitTopLevel(string text)
	{
		var parts = new List<string>();
		int depth = 0, start = 0;
		char quote = '\0';
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (quote != '\0') { if (c == quote) quote = '\0'; continue; }
			if (c == '\'' || c == '"') quote = c;
			else if (c == '{') depth++;
			else if (c == '}') depth--;
			else if (c == ',' && depth == 0) { parts.Add(text.Substring(start, i - start)); start = i + 1; }
		}
		parts.Add(text.Substring(start));
		return parts;
	}

	static int IndexOfTopLevel(string text, char find)
	{
		int depth = 0;
		char quote = '\0';
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (quote != '\0') { if (c == quote) quote = '\0'; continue; }
			if (c == '\'' || c == '"') quote = c;
			else if (c == '{') depth++;
			else if (c == '}') depth--;
			else if (c == find && depth == 0) return i;
		}
		return -1;
	}

	static string Unquote(string text)
	{
		string t = text.Trim();
		bool quoted = t.Length >= 2 && (t[0] == '\'' || t[0] == '"') && t[t.Length - 1] == t[0];
		if (quoted) return t.Substring(1, t.Length - 2);
		return t;
	}
}
