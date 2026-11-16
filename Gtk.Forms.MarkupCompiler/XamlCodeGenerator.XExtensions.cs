using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Gtk.Forms.MarkupCompiler
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
		/// </summary>
		internal static bool IsXType(string value, out string typeofExpr)
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

			// Strip optional prefix ("ns:Button" → "Button")
			int colon = inner.IndexOf(':');
			string localName = colon >= 0 ? inner.Substring(colon + 1).Trim() : inner;

			string fqType = WpfXmlnsMap.Resolve(localName) ?? localName;
			typeofExpr = $"typeof(global::{fqType})";
			return true;
		}

		// ── x:Static ─────────────────────────────────────────────────────────────

		/// <summary>
		/// Returns <c>true</c> when <paramref name="value"/> is
		/// <c>{x:Static prefix:Type.Member}</c> or <c>{x:Static Type.Member}</c>.
		/// Sets <paramref name="staticExpr"/> to <c>global::Ns.Type.Member</c>.
		/// </summary>
		internal static bool IsXStatic(string value, out string staticExpr)
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

			// Strip optional xmlns prefix ("local:Foo.Bar" → "Foo.Bar")
			int colon = inner.IndexOf(':');
			string qualified = colon >= 0 ? inner.Substring(colon + 1).Trim() : inner;

			// Must contain at least one dot (Type.Member)
			int dot = qualified.LastIndexOf('.');
			if (dot <= 0) return false;

			string typePart = qualified.Substring(0, dot);
			string member = qualified.Substring(dot + 1);

			// Resolve type part against the xmlns map; fall back to as-is.
			string fqType = WpfXmlnsMap.Resolve(typePart) ?? typePart;

			staticExpr = $"global::{fqType}.{member}";
			return true;
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
