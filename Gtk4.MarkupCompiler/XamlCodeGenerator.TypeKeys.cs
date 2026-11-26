using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		static readonly Regex TypeKeyRef =
			new Regex(@"\{\s*StaticResource\s+\{\s*x:Type\s+([^}\s]+)\s*\}\s*\}", RegexOptions.Compiled);

		// Types whose key-less Styles the runtime applies on its own (generated ListBox containers).
		static readonly HashSet<string> ImplicitStyleTargets =
			new HashSet<string>(StringComparer.Ordinal) { "global::System.Windows.Controls.ListBoxItem" };

		// Types referenced as {StaticResource {x:Type T}}: their key-less Styles are registered under typeof(T).
		HashSet<string> _typeKeyRefs = new HashSet<string>(StringComparer.Ordinal);

		void CollectTypeKeyRefs(string xaml)
		{
			_typeKeyRefs = new HashSet<string>(StringComparer.Ordinal);
			foreach (Match m in TypeKeyRef.Matches(xaml))
				if (ResolveTypeRefName(m.Groups[1].Value) is string name) _typeKeyRefs.Add(name);
		}

		/// <summary>
		/// The key of a Style: its <c>x:Key</c>, or its type when a <c>{x:Type}</c> reference needs it.
		/// </summary>
		string StyleResourceKey(string key, string targetTypeAttr)
		{
			if (key != null) return ResourceKeyExpression(EscapeString(key));
			return targetTypeAttr != null && ResolveTypeRefName(targetTypeAttr) is string t
				&& (_typeKeyRefs.Contains(t) || ImplicitStyleTargets.Contains(t))
				? $"typeof({t})" : null;
		}

		/// <summary>A resource key as C#: <c>typeof(T)</c> for <c>{x:Type T}</c>, else a string literal.</summary>
		string ResourceKeyExpression(string escapedKey)
		{
			if (escapedKey.StartsWith("{x:Type", StringComparison.Ordinal) && ResolveTypeRefName(escapedKey) is string t)
				return $"typeof({t})";
			// A nested {x:Static ...} is the key object itself (e.g. a ResourceKey), not its text.
			if (IsXStatic(escapedKey, out string staticExpr, _prefixMap, ProbePresentationType)) return staticExpr;
			return "\"" + escapedKey + "\"";
		}

		// A type name nested in a markup extension: [prefix:]Type, optionally wrapped in {x:Type}.
		string NestedTypeName(string value)
		{
			if (!TryParseTypeRef(value, out string prefix, out string local)) return value;
			if (prefix.Length > 0 && _prefixMap.TryGetValue(prefix, out string clrNs) && clrNs.Length > 0)
				return "global::" + clrNs + "." + local;
			return WpfXmlnsMap.Resolve(local) ?? ProbePresentationType(local) ?? local;
		}
	}
}
