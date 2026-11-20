using System;
using System.Collections.Generic;

namespace Gtk4.Forms.MarkupCompiler
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
				if (!t.StartsWith("{x:Type", StringComparison.Ordinal) || !t.EndsWith("}", StringComparison.Ordinal))
					return false;
				t = t.Substring("{x:Type".Length, t.Length - "{x:Type".Length - 1).Trim();
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
		/// Resolves a <c>TargetType</c>/<c>DataType</c> value (plain or <c>{x:Type}</c> form) to a
		/// CLR type name; user clr-namespace types come back as <c>global::Ns.Type</c>. Falls back to
		/// the bare name when unresolved. Returns <c>null</c> when the value is not a type reference.
		/// </summary>
		string ResolveTypeRefName(string value)
		{
			if (!TryParseTypeRef(value, out string prefix, out string localName)) return null;
			if (prefix.Length > 0 && _prefixMap.TryGetValue(prefix, out string clrNs) && clrNs.Length > 0)
				return "global::" + clrNs + "." + localName;
			return QualifyTypeName(localName);
		}

		/// <summary>The <c>typeof(...)</c> expression for a type reference, or <c>null</c> for none.</summary>
		string TypeRefExpression(string value)
		{
			string name = value == null ? null : ResolveTypeRefName(value);
			return name == null ? "null" : $"typeof({name})";
		}
	}
}
