using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Returns <c>true</c> when <paramref name="value"/> is a
		/// <c>{StaticResource key}</c> or <c>{DynamicResource key}</c> markup extension.
		/// Sets <paramref name="resourceKey"/> to the key string.
		/// </summary>
		static bool IsStaticOrDynamicResource(string value, out string resourceKey)
		{
			resourceKey = null;
			if (value == null) return false;
			string trimmed = value.Trim();
			string prefix = null;
			if (trimmed.StartsWith("{StaticResource", StringComparison.Ordinal))
				prefix = "{StaticResource";
			else if (trimmed.StartsWith("{DynamicResource", StringComparison.Ordinal))
				prefix = "{DynamicResource";
			if (prefix == null) return false;
			if (!trimmed.EndsWith("}")) return false;
			resourceKey = trimmed.Substring(prefix.Length, trimmed.Length - prefix.Length - 1).Trim();
			return !string.IsNullOrEmpty(resourceKey);
		}

		/// <summary>
		/// Returns true when <paramref name="value"/> is a <c>{Binding ...}</c> markup
		/// extension and sets <paramref name="bindingInit"/> to the C# object-initializer
		/// expression for the resulting <see cref="System.Windows.Data.Binding"/>.
		/// Round 7: also parses RelativeSource, Converter={StaticResource k},
		/// StringFormat, FallbackValue, TargetNullValue, UpdateSourceTrigger.
		/// </summary>
		bool IsBindingExpression(string value, out string bindingInit)
		{
			bindingInit = null;
			if (value == null) return false;
			string trimmed = value.Trim();
			if (!trimmed.StartsWith("{Binding", StringComparison.Ordinal)) return false;
			if (!trimmed.EndsWith("}")) return false;

			// Remove exactly the outer closing brace (last char), not all trailing braces.
			string inner = trimmed
				.Substring("{Binding".Length, trimmed.Length - "{Binding".Length - 1)
				.Trim();

			if (string.IsNullOrEmpty(inner))
			{
				bindingInit = "new System.Windows.Data.Binding()";
				return true;
			}

			var args = SplitBindingArgs(inner);

			string path = null;
			string mode = null;
			string elementName = null;
			string converterResourceKey = null;  // set when Converter={StaticResource k}
			string relativeSourceExpr = null;    // C# expression for RelativeSource
			string stringFormat = null;
			string fallbackValue = null;
			string targetNullValue = null;
			string updateSourceTrigger = null;
			var extras = new List<string>();

			bool firstPositional = true;
			foreach (string arg in args)
			{
				string a = arg.Trim();
				if (a.StartsWith("Path=", StringComparison.Ordinal))
					path = a.Substring("Path=".Length).Trim();
				else if (a.StartsWith("Mode=", StringComparison.Ordinal))
					mode = a.Substring("Mode=".Length).Trim();
				else if (a.StartsWith("ElementName=", StringComparison.Ordinal))
					elementName = a.Substring("ElementName=".Length).Trim();
				else if (a.StartsWith("Converter=", StringComparison.Ordinal))
					converterResourceKey = ParseConverterArg(a.Substring("Converter=".Length).Trim());
				else if (a.StartsWith("RelativeSource=", StringComparison.Ordinal))
					relativeSourceExpr = ParseRelativeSourceArg(a.Substring("RelativeSource=".Length).Trim());
				else if (a.StartsWith("StringFormat=", StringComparison.Ordinal))
					stringFormat = a.Substring("StringFormat=".Length).Trim();
				else if (a.StartsWith("FallbackValue=", StringComparison.Ordinal))
					fallbackValue = a.Substring("FallbackValue=".Length).Trim();
				else if (a.StartsWith("TargetNullValue=", StringComparison.Ordinal))
					targetNullValue = a.Substring("TargetNullValue=".Length).Trim();
				else if (a.StartsWith("UpdateSourceTrigger=", StringComparison.Ordinal))
					updateSourceTrigger = a.Substring("UpdateSourceTrigger=".Length).Trim();
				else if (TryExtraBindingArg(a, extras)) { }
				else if (firstPositional && !a.Contains("="))
					path = a;
				firstPositional = false;
			}

			var sb = new StringBuilder();
			if (path != null)
				sb.Append("new System.Windows.Data.Binding(\"" + EscapeString(path) + "\")");
			else
				sb.Append("new System.Windows.Data.Binding()");

			bool hasInit = mode != null || elementName != null
				|| converterResourceKey != null || relativeSourceExpr != null
				|| stringFormat != null || fallbackValue != null
				|| targetNullValue != null || updateSourceTrigger != null
				|| extras.Count > 0;

			if (hasInit)
			{
				sb.Append(" { ");
				bool comma = false;
				if (mode != null)
				{
					sb.Append("Mode = System.Windows.Data.BindingMode." + mode);
					comma = true;
				}
				if (elementName != null)
				{
					if (comma) sb.Append(", ");
					sb.Append("ElementName = \"" + EscapeString(elementName) + "\"");
					comma = true;
				}
				if (relativeSourceExpr != null)
				{
					if (comma) sb.Append(", ");
					sb.Append("RelativeSource = " + relativeSourceExpr);
					comma = true;
				}
				if (converterResourceKey != null)
				{
					if (comma) sb.Append(", ");
					// Converter resolved lazily from the resource dictionary at runtime.
					sb.Append("Converter = (System.Windows.Data.IValueConverter)"
						+ StaticResourceLookup("this", EscapeString(converterResourceKey)));
					comma = true;
				}
				if (stringFormat != null)
				{
					if (comma) sb.Append(", ");
					sb.Append("StringFormat = \"" + EscapeString(stringFormat) + "\"");
					comma = true;
				}
				if (fallbackValue != null)
				{
					if (comma) sb.Append(", ");
					sb.Append("FallbackValue = \"" + EscapeString(fallbackValue) + "\"");
					comma = true;
				}
				if (targetNullValue != null)
				{
					if (comma) sb.Append(", ");
					sb.Append("TargetNullValue = \"" + EscapeString(targetNullValue) + "\"");
					comma = true;
				}
				if (updateSourceTrigger != null)
				{
					if (comma) sb.Append(", ");
					sb.Append("UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger."
						+ updateSourceTrigger);
					comma = true;
				}
				foreach (string extra in extras)
				{
					if (comma) sb.Append(", ");
					sb.Append(extra);
					comma = true;
				}
				sb.Append(" }");
			}

			bindingInit = sb.ToString();
			return true;
		}

		/// <summary>
		/// Parses <c>{StaticResource k}</c> (or bare <c>k</c>) from a Converter= argument value.
		/// Returns the resource key, or null when not a StaticResource reference.
		/// </summary>
		static string ParseConverterArg(string raw)
		{
			if (raw == null) return null;
			string t = raw.Trim();
			if (IsStaticOrDynamicResource(t, out string key)) return key;
			// Bare identifier — treat as resource key.
			return t.Length > 0 ? t : null;
		}

		/// <summary>
		/// Parses <c>{RelativeSource Self}</c>, <c>{RelativeSource TemplatedParent}</c>,
		/// or <c>{RelativeSource FindAncestor, AncestorType=Grid}</c> and returns the
		/// C# expression to construct the matching <see cref="System.Windows.Data.RelativeSource"/>.
		/// Returns null when the syntax is unrecognised.
		/// </summary>
		string ParseRelativeSourceArg(string raw)
		{
			if (raw == null) return null;
			string t = raw.Trim();

			// Must be {RelativeSource ...}
			if (!t.StartsWith("{RelativeSource", StringComparison.Ordinal)) return null;
			if (!t.EndsWith("}")) return null;

			// Remove exactly the outer closing brace.
			string inner = t
				.Substring("{RelativeSource".Length, t.Length - "{RelativeSource".Length - 1)
				.Trim();

			if (string.IsNullOrEmpty(inner))
				return null;

			var parts = SplitBindingArgs(inner);
			string modeStr = null;
			string ancestorType = null;
			string ancestorLevel = null;

			bool first = true;
			foreach (string p in parts)
			{
				string a = p.Trim();
				if (a.StartsWith("AncestorType=", StringComparison.Ordinal))
				{
					string at = a.Substring("AncestorType=".Length).Trim();
					// Strip "{x:Type X}" wrapper if present.
					ancestorType = NestedTypeName(at);
				}
				else if (a.StartsWith("AncestorLevel=", StringComparison.Ordinal))
					ancestorLevel = a.Substring("AncestorLevel=".Length).Trim();
				else if (first && !a.Contains("="))
					modeStr = a;
				first = false;
			}

			// First positional arg without '=' is the mode name.
			if (modeStr == null && parts.Count > 0 && !parts[0].Trim().Contains("="))
				modeStr = parts[0].Trim();

			if (modeStr == null) return null;

			switch (modeStr)
			{
				case "Self":
					return "System.Windows.Data.RelativeSource.Self";

				case "TemplatedParent":
					return "System.Windows.Data.RelativeSource.TemplatedParent";

				case "PreviousData":
					return "System.Windows.Data.RelativeSource.PreviousData";

				case "FindAncestor":
					if (ancestorType == null) return null;
					string level = ancestorLevel ?? "1";
					return "new System.Windows.Data.RelativeSource("
						+ "System.Windows.Data.RelativeSourceMode.FindAncestor, "
						+ "typeof(" + ancestorType + "), " + level + ")";

				default:
					return null;
			}
		}

		/// <summary>
		/// Emits a local variable <paramref name="mbVar"/> of type
		/// <see cref="System.Windows.Data.MultiBinding"/> constructed from the
		/// child <c>&lt;Binding&gt;</c> elements inside the XAML <c>&lt;MultiBinding&gt;</c>.
		/// </summary>
		void EmitMultiBindingInline(
			IndentedWriter w,
			XElement mbEl,
			XNamespace xNs,
			string mbVar,
			ref int counter)
		{
			w.Line("var " + mbVar + " = new System.Windows.Data.MultiBinding();");

			// Converter={StaticResource k}
			string convAttr = (string)mbEl.Attribute("Converter");
			if (convAttr != null && IsStaticOrDynamicResource(convAttr, out string convKey))
				w.Line(mbVar + ".Converter = (System.Windows.Data.IMultiValueConverter)"
					+ StaticResourceLookup("this", EscapeString(convKey)) + ";");

			// Mode
			string modeAttr = (string)mbEl.Attribute("Mode");
			if (modeAttr != null)
				w.Line(mbVar + ".Mode = System.Windows.Data.BindingMode." + modeAttr + ";");

			// StringFormat
			string sfAttr = (string)mbEl.Attribute("StringFormat");
			if (sfAttr != null)
				w.Line(mbVar + ".StringFormat = \"" + EscapeString(sfAttr) + "\";");

			// FallbackValue
			string fbAttr = (string)mbEl.Attribute("FallbackValue");
			if (fbAttr != null)
				w.Line(mbVar + ".FallbackValue = \"" + EscapeString(fbAttr) + "\";");

			// TargetNullValue
			string tnvAttr = (string)mbEl.Attribute("TargetNullValue");
			if (tnvAttr != null)
				w.Line(mbVar + ".TargetNullValue = \"" + EscapeString(tnvAttr) + "\";");

			// Child <Binding> elements.
			foreach (XElement child in mbEl.Elements())
			{
				if (child.Name.LocalName != "Binding") continue;

				// Build a Binding from the child element's attributes.
				string childPath = (string)child.Attribute("Path");
				string childMode = (string)child.Attribute("Mode");
				string childEn = (string)child.Attribute("ElementName");
				string childConvAttr = (string)child.Attribute("Converter");
				string childConvKey = childConvAttr != null
					? ParseConverterArg(childConvAttr) : null;
				string childSf = (string)child.Attribute("StringFormat");

				string childBindingInit;
				if (childPath != null)
					childBindingInit = "new System.Windows.Data.Binding(\""
						+ EscapeString(childPath) + "\")";
				else
					childBindingInit = "new System.Windows.Data.Binding()";

				bool hasInit = childMode != null || childEn != null
					|| childConvKey != null || childSf != null;
				if (hasInit)
				{
					childBindingInit += " { ";
					bool comma = false;
					if (childMode != null)
					{
						childBindingInit += "Mode = System.Windows.Data.BindingMode." + childMode;
						comma = true;
					}
					if (childEn != null)
					{
						if (comma) childBindingInit += ", ";
						childBindingInit += "ElementName = \"" + EscapeString(childEn) + "\"";
						comma = true;
					}
					if (childConvKey != null)
					{
						if (comma) childBindingInit += ", ";
						childBindingInit += "Converter = (System.Windows.Data.IValueConverter)"
							+ StaticResourceLookup("this", EscapeString(childConvKey));
						comma = true;
					}
					if (childSf != null)
					{
						if (comma) childBindingInit += ", ";
						childBindingInit += "StringFormat = \"" + EscapeString(childSf) + "\"";
					}
					childBindingInit += " }";
				}

				w.Line(mbVar + ".Bindings.Add(" + childBindingInit + ");");
			}
		}

		/// <summary>
		/// Returns <c>true</c> when <paramref name="value"/> is a
		/// <c>{TemplateBinding Property}</c> markup extension.
		/// Sets <paramref name="bindingInit"/> to a C# <see cref="System.Windows.Data.Binding"/>
		/// expression that binds in one-way mode against
		/// <see cref="System.Windows.Data.RelativeSource.TemplatedParent"/>.
		/// <para>
		/// Only single-property form is supported: <c>{TemplateBinding Prop}</c>.
		/// If additional comma-separated arguments are present (e.g.
		/// <c>{TemplateBinding Prop, Converter=…}</c>) only the first token before the
		/// comma is used as the path; the extra arguments are silently ignored.
		/// </para>
		/// </summary>
		internal static bool IsTemplateBindingExpression(string value, out string bindingInit)
		{
			bindingInit = null;
			if (value == null) return false;
			string trimmed = value.Trim();
			if (!trimmed.StartsWith("{TemplateBinding", StringComparison.Ordinal)) return false;
			if (!trimmed.EndsWith("}")) return false;

			// Extract the content between {TemplateBinding and the closing brace.
			string inner = trimmed
				.Substring("{TemplateBinding".Length, trimmed.Length - "{TemplateBinding".Length - 1)
				.Trim();

			if (string.IsNullOrEmpty(inner)) return false;

			// Take only the first comma-separated token as the path so that
			// {TemplateBinding Prop, Converter=…} does not produce a malformed path.
			int comma = inner.IndexOf(',');
			string propertyPath = comma >= 0 ? inner.Substring(0, comma).Trim() : inner;

			if (string.IsNullOrEmpty(propertyPath)) return false;

			// {TemplateBinding Prop} → new Binding("Prop") { RelativeSource = RS.TemplatedParent, Mode = OneWay }
			bindingInit = "new System.Windows.Data.Binding(\"" + EscapeString(propertyPath) + "\") { "
				+ "RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent, "
				+ "Mode = System.Windows.Data.BindingMode.OneWay }";
			return true;
		}

		/// <summary>
		/// Splits binding arguments on commas, respecting nested braces.
		/// e.g. <c>Path=X, Mode=TwoWay, Converter={StaticResource c}</c>
		/// produces three tokens.
		/// </summary>
		internal static List<string> SplitBindingArgs(string inner)
		{
			var result = new List<string>();
			int depth = 0;
			int start = 0;
			bool quoted = false;
			for (int i = 0; i < inner.Length; i++)
			{
				char c = inner[i];
				// A quote opens only at a value start (after '='), so stray apostrophes are inert.
				if (c == '\'' && (quoted || (i > 0 && inner[i - 1] == '=')))
					quoted = !quoted;
				else if (quoted) continue;
				else if (c == '{') depth++;
				else if (c == '}') depth--;
				else if (c == ',' && depth == 0)
				{
					result.Add(inner.Substring(start, i - start));
					start = i + 1;
				}
			}
			if (start < inner.Length)
				result.Add(inner.Substring(start));
			return result;
		}

		/// <summary>
		/// Escapes a raw string value so it can be emitted as a C# string literal.
		/// </summary>
		internal static string EscapeString(string s) =>
			s.Replace("\\", "\\\\")
			 .Replace("\"", "\\\"")
			 .Replace("\r", "\\r")
			 .Replace("\n", "\\n")
			 .Replace("\t", "\\t");
	}
}
