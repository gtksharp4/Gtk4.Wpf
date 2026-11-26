using System;
using System.Collections.Generic;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Parses a type reference: <c>Button</c>, <c>p:Button</c>, <c>{x:Type Button}</c>,
		/// <c>{x:Type p:Button}</c> or <c>{x:Type TypeName=p:Button}</c>.
		/// </summary>
		internal static bool TryParseTypeRef(string value, out string prefix, out string localName)
		{
			prefix = string.Empty;
			localName = null;
			if (value == null) return false;
			string t = value.Trim();
			if (t.StartsWith("{", StringComparison.Ordinal))
			{
				string head = t.StartsWith("{x:TypeExtension", StringComparison.Ordinal) ? "{x:TypeExtension"
					: t.StartsWith("{x:Type", StringComparison.Ordinal) ? "{x:Type" : null;
				if (head == null || !t.EndsWith("}", StringComparison.Ordinal)) return false;
				if (t.Length > head.Length + 1 && !char.IsWhiteSpace(t[head.Length])) return false;
				t = t.Substring(head.Length, t.Length - head.Length - 1).Trim();
				if (t.StartsWith("TypeName=", StringComparison.Ordinal))
					t = t.Substring("TypeName=".Length).Trim();
			}
			if (t.Length == 0 || t.IndexOfAny(new[] { '{', '}', ',', ' ', '=' }) >= 0) return false;
			int colon = t.IndexOf(':');
			if (colon >= 0)
			{
				prefix = t.Substring(0, colon).Trim();
				t = t.Substring(colon + 1).Trim();
			}
			localName = t;
			return t.Length > 0;
		}

		/// <summary>
		/// Probes the presentation-namespace xmlns definitions for <paramref name="localName"/>.
		/// Returns the fully-qualified name, or <c>null</c> when no entry yields the type.
		/// </summary>
		string ProbePresentationType(string localName)
		{
			IReadOnlyList<XmlnsDefinitionEntry> entries = _xmlnsSource.GetEntries();
			for (int i = 0; i < entries.Count; i++)
			{
				if (!string.Equals(entries[i].XmlNamespace, WpfXmlnsMap.PresentationNs, StringComparison.Ordinal))
					continue;
				string candidate = entries[i].ClrNamespace + "." + localName;
				if (_resolver.TypeExists(candidate) || !_resolver.HasReferences) return candidate;
			}
			return null;
		}

		/// <summary>
		/// Fully-qualified CLR name for a type local name: built-in map, then the xmlns definitions,
		/// else the name unchanged. Used for attached-property and routed-event owners.
		/// </summary>
		string QualifyTypeName(string localName)
			=> WpfXmlnsMap.Resolve(localName) ?? ProbePresentationType(localName) ?? localName;

		/// <summary>
		/// <paramref name="name"/> as an emitted type name: resolved (dotted) names get <c>global::</c> so they bind
		/// to the root namespace; bare unresolved names stay as is.
		/// </summary>
		static string Emitted(string name)
			=> name.IndexOf('.') > 0 && !name.StartsWith("global::", StringComparison.Ordinal)
				? "global::" + name : name;

		/// <summary>
		/// Resolves a <c>TargetType</c>/<c>DataType</c> value (plain or <c>{x:Type}</c> form) to a
		/// CLR type name; user clr-namespace types come back as <c>global::Ns.Type</c>. Falls back to
		/// the bare name when unresolved. Returns <c>null</c> when the value is not a type reference.
		/// </summary>
		string ResolveTypeRefName(string value)
		{
			if (!TryParseTypeRef(value, out string prefix, out string localName)) return null;
			if (prefix.Length > 0 && _prefixMap.TryGetValue(prefix, out string clrNs) && clrNs.Length > 0)
				return "global::" + clrNs + "." + localName;
			return Emitted(QualifyTypeName(localName));
		}

		/// <summary>
		/// The <c>typeof(...)</c> expression for a type reference attribute value, or <c>null</c> when the
		/// attribute is absent.
		/// </summary>
		/// <exception cref="XamlCodeGeneratorException">The value is present but not a type reference.</exception>
		string TypeRefExpression(string value)
		{
			if (value == null) return "null";
			string name = ResolveTypeRefName(value)
				?? throw new XamlCodeGeneratorException("Unsupported type reference: " + value);
			return $"typeof({name})";
		}

		/// <summary>
		/// The <c>DataType</c> expression: <c>typeof</c> for <c>{x:Type}</c>/<c>{x:TypeExtension}</c>, the member for
		/// <c>{x:Static}</c>, else the plain string (an XML element name for XmlDataProvider templates; a leading
		/// <c>{}</c> escape is stripped), <c>{StaticResource}</c> as a resource lookup, or <c>null</c> when absent.
		/// <c>DataType</c> is a CLR property, so <c>{Binding}</c> and <c>{DynamicResource}</c> are rejected.
		/// </summary>
		/// <exception cref="XamlCodeGeneratorException">The value is an unsupported markup extension.</exception>
		string DataTypeExpression(string value)
		{
			if (value == null) return "null";
			string t = value.TrimStart();
			if (t.StartsWith("{}", StringComparison.Ordinal)) return "\"" + EscapeString(t.Substring(2)) + "\"";
			if (t.StartsWith("{x:Type", StringComparison.Ordinal)) return TypeRefExpression(value);
			if (t.StartsWith("{", StringComparison.Ordinal))
			{
				if (IsXStatic(value, out string staticExpr, _prefixMap, ProbePresentationType)) return staticExpr;
				if (t.StartsWith("{StaticResource", StringComparison.Ordinal)
					&& IsStaticOrDynamicResource(t, out string key))
					return StaticResourceLookup("this", EscapeString(key));
				throw new XamlCodeGeneratorException("Unsupported DataType value: " + value);
			}
			return "\"" + EscapeString(value) + "\"";
		}
	}
}
