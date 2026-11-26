using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	/// <summary>
	/// Handles the <c>x:</c> markup extensions:
	/// <c>{x:Null}</c>, <c>{x:Static …}</c>, <c>{x:Type …}</c>,
	/// and the <c>&lt;x:Array Type="…"&gt;</c> element form.
	/// These are checked before the DP/CLR attribute-value paths.
	/// </summary>
	public partial class XamlCodeGenerator
	{
		// ── x:Null ───────────────────────────────────────────────────────────────

		/// <summary>
		/// Returns <c>true</c> when <paramref name="value"/> is <c>{x:Null}</c>.
		/// Emits <c>null</c>.
		/// </summary>
		internal static bool IsXNull(string value)
		{
			if (value == null) return false;
			string t = value.Trim();
			return string.Equals(t, "{x:Null}", StringComparison.Ordinal);
		}

		// ── x:Type ───────────────────────────────────────────────────────────────

		/// <summary>
		/// Returns <c>true</c> when <paramref name="value"/> is <c>{x:Type LocalName}</c>
		/// or <c>{x:Type prefix:LocalName}</c>.
		/// Sets <paramref name="typeofExpr"/> to <c>typeof(global::Ns.Type)</c>.
		/// <paramref name="prefixMap"/> maps xmlns prefixes to CLR namespaces
		/// (<c>clr-namespace:MyApp</c> form); may be <c>null</c>.
		/// </summary>
		/// <exception cref="XamlCodeGeneratorException">
		/// Thrown when the type cannot be resolved via either the prefix map or
		/// <see cref="WpfXmlnsMap"/>.
		/// </exception>
		internal static bool IsXType(
			string value,
			out string typeofExpr,
			IReadOnlyDictionary<string, string> prefixMap = null,
			Func<string, string> probe = null)
		{
			typeofExpr = null;
			if (value == null) return false;
			string t = value.Trim();
			if (!t.StartsWith("{x:Type", StringComparison.Ordinal)) return false;
			if (!t.EndsWith("}")) return false;

			string inner = t
				.Substring("{x:Type".Length, t.Length - "{x:Type".Length - 1)
				.Trim();

			if (string.IsNullOrEmpty(inner)) return false;

			// Separate optional prefix from local name ("ns:Button" → prefix="ns", local="Button").
			int colon = inner.IndexOf(':');
			string prefix = colon >= 0 ? inner.Substring(0, colon).Trim() : string.Empty;
			string localName = colon >= 0 ? inner.Substring(colon + 1).Trim() : inner;

			string fqType = ResolveXType(prefix, localName, prefixMap, true, probe);
			typeofExpr = $"typeof(global::{fqType})";
			return true;
		}

		// ── x:Static ─────────────────────────────────────────────────────────────

		/// <summary>
		/// Returns <c>true</c> when <paramref name="value"/> is
		/// <c>{x:Static prefix:Type.Member}</c> or <c>{x:Static Type.Member}</c>.
		/// Sets <paramref name="staticExpr"/> to <c>global::Ns.Type.Member</c>.
		/// <paramref name="prefixMap"/> maps xmlns prefixes to CLR namespaces; may be <c>null</c>.
		/// </summary>
		/// <exception cref="XamlCodeGeneratorException">
		/// Thrown when the type cannot be resolved via either the prefix map or
		/// <see cref="WpfXmlnsMap"/>.
		/// </exception>
		internal static bool IsXStatic(
			string value,
			out string staticExpr,
			IReadOnlyDictionary<string, string> prefixMap = null,
			Func<string, string> probe = null)
		{
			staticExpr = null;
			if (value == null) return false;
			string t = value.Trim();
			if (!t.StartsWith("{x:Static", StringComparison.Ordinal)) return false;
			if (!t.EndsWith("}")) return false;

			string inner = t
				.Substring("{x:Static".Length, t.Length - "{x:Static".Length - 1)
				.Trim();

			if (string.IsNullOrEmpty(inner)) return false;

			// Separate optional xmlns prefix ("local:Foo.Bar" → prefix="local", rest="Foo.Bar").
			int colon = inner.IndexOf(':');
			string prefix = colon >= 0 ? inner.Substring(0, colon).Trim() : string.Empty;
			string qualified = colon >= 0 ? inner.Substring(colon + 1).Trim() : inner;

			// Must contain at least one dot (Type.Member).
			int dot = qualified.LastIndexOf('.');
			if (dot <= 0) return false;

			string typePart = qualified.Substring(0, dot);
			string member = qualified.Substring(dot + 1);

			string fqType = ResolveXType(prefix, typePart, prefixMap, false, probe);
			staticExpr = $"global::{fqType}.{member}";
			return true;
		}

		// ── Shared type resolution ────────────────────────────────────────────────

		/// <summary>
		/// Resolves a type name from an x: extension, given an optional xmlns prefix and local name.
		/// Resolution order:
		/// 1. If <paramref name="prefix"/> maps to a <c>clr-namespace:</c> entry in
		///    <paramref name="prefixMap"/>, prepend that CLR namespace.
		/// 2. Otherwise, try <see cref="WpfXmlnsMap.Resolve"/> (presentation-namespace map).
		/// 3. If a prefix is present, <paramref name="prefixMap"/> was supplied (namespace context
		///    is available), and the prefix is still not resolved, raise
		///    <see cref="XamlCodeGeneratorException"/>.
		/// 4. When <paramref name="errorOnUnresolvableBare"/> is true, no prefix, namespace context
		///    supplied, bare name (no dots), and not in any map → raise
		///    <see cref="XamlCodeGeneratorException"/> (V3: only for x:Type, not x:Static).
		/// 5. Otherwise return the local name as-is (global namespace fallback or no-context mode).
		/// </summary>
		static string ResolveXType(
			string prefix,
			string localName,
			IReadOnlyDictionary<string, string> prefixMap,
			bool errorOnUnresolvableBare = false,
			Func<string, string> probe = null)
		{
			// 1. User-defined clr-namespace prefix.
			if (!string.IsNullOrEmpty(prefix) && prefixMap != null)
			{
				string clrNs;
				if (prefixMap.TryGetValue(prefix, out clrNs) &&
					!string.IsNullOrEmpty(clrNs))
					return clrNs + "." + localName;
			}

			// 2. Built-in WPF presentation map (handles "Button" → "System.Windows.Controls.Button").
			string mapped = WpfXmlnsMap.Resolve(localName);
			if (mapped != null) return mapped;

			string probed = probe?.Invoke(localName);
			if (probed != null) return probed;

			// 3. Prefix present, namespace context supplied, but prefix not found → diagnostic.
			if (!string.IsNullOrEmpty(prefix) && prefixMap != null)
				throw new XamlCodeGeneratorException(
					$"Cannot resolve x: type '{prefix}:{localName}': " +
					$"xmlns prefix '{prefix}' is not declared or does not map to a CLR namespace.");

			// 4. V3: x:Type only — bare name, namespace context available, not in any map → diagnostic.
			if (errorOnUnresolvableBare &&
				string.IsNullOrEmpty(prefix) && prefixMap != null && localName.IndexOf('.') < 0)
				throw new XamlCodeGeneratorException(
					$"Cannot resolve x: type '{localName}': not in the WPF presentation namespace. " +
					$"Add an xmlns:prefix declaration mapping to the type's CLR namespace.");

			// 5. Fallback: return as-is (prefix stripped; no namespace context, or FQ name with dots).
			return localName;
		}

		// ── x:Array (element form) ───────────────────────────────────────────────

		/// <summary>
		/// Returns <c>true</c> when <paramref name="el"/> is an <c>x:Array</c> element.
		/// Emits a local variable <paramref name="arrayVar"/> holding the constructed array
		/// and sets <paramref name="arrayVar"/> to its name.
		/// Supports arrays of simple string/int/double values and of mapped WPF types.
		/// </summary>
		internal bool TryEmitXArray(
			IndentedWriter w,
			XElement el,
			XNamespace xNs,
			out string arrayVar,
			ref int counter)
		{
			arrayVar = null;

			// x:Array elements live in the x: namespace.
			if (el.Name.LocalName != "Array") return false;
			string elNs = el.Name.NamespaceName;
			if (elNs != WpfXmlnsMap.XNs && elNs != WpfXmlnsMap.XNs2006) return false;

			string typeAttr = (string)el.Attribute("Type");
			if (typeAttr == null) return false;

			// Strip namespace prefix from Type attribute.
			int colon = typeAttr.IndexOf(':');
			string typeLocal = colon >= 0 ? typeAttr.Substring(colon + 1).Trim() : typeAttr.Trim();

			// Map to FQN (sys:String → System.String, etc.)
			string fqType = MapXArrayType(typeLocal);

			arrayVar = $"__arr{counter++}";

			// Collect child elements / values.
			var items = new List<string>();
			foreach (XElement child in el.Elements())
			{
				string childLocal = child.Name.LocalName;
				// Simple value wrappers: <sys:String>text</sys:String>, <sys:Int32>1</sys:Int32>
				string mapped = MapXArrayType(childLocal);
				string text = child.Value.Trim();
				items.Add(SimpleArrayItemExpr(mapped, text));
			}

			w.Line($"var {arrayVar} = new {fqType}[] {{");
			w.Indent++;
			foreach (string item in items)
				w.Line(item + ",");
			w.Indent--;
			w.Line("};");

			return true;
		}

		/// <summary>
		/// Maps a local name used as an x:Array type or child element to its CLR FQN.
		/// Covers BCL primitives plus any name in the presentation map.
		/// </summary>
		static string MapXArrayType(string localName)
		{
			switch (localName)
			{
				case "String": return "System.String";
				case "Int32": return "System.Int32";
				case "Int64": return "System.Int64";
				case "Double": return "System.Double";
				case "Single": return "System.Single";
				case "Boolean": return "System.Boolean";
				case "Object": return "System.Object";
			}
			return WpfXmlnsMap.Resolve(localName) ?? localName;
		}

		/// <summary>
		/// Returns a C# expression for a single item in an x:Array of a simple type.
		/// </summary>
		static string SimpleArrayItemExpr(string fqType, string text)
		{
			switch (fqType)
			{
				case "System.String": return $"\"{EscapeString(text)}\"";
				case "System.Int32":
				case "System.Int64":
				case "System.Double":
				case "System.Single":
					return text; // numeric literal
				case "System.Boolean":
					return string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
						? "true" : "false";
			}
			// Unknown type — emit as string; runtime must cope.
			return $"\"{EscapeString(text)}\"";
		}
	}
}
