using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		static readonly string[] _primitiveElements =
		{
			"String", "Boolean", "Byte", "SByte", "Char", "Decimal", "Double", "Single", "Int16", "Int32", "Int64",
			"UInt16", "UInt32", "UInt64"
		};

		static readonly string[] _colorAttributes = { "A", "R", "G", "B" };
		static readonly string[] _scColorAttributes = { "ScA", "ScR", "ScG", "ScB" };

		// A keyed element no earlier branch built: a text-only primitive keeps its string, a Color of literals, a
		// converter-backed struct of text and an
		// attribute-less instantiable object are built, anything else gets no entry (WPF never stores its text).
		void EmitFallbackResource(IndentedWriter w, XElement item, XNamespace xNs, string ownerVar, string key)
		{
			string target = $"{ownerVar}.Resources[\"{EscapeString(key)}\"]";
			if (!_resolver.HasReferences || IsPrimitiveValueElement(item, xNs))
				w.Line($"{target} = \"{EscapeString(item.Value.Trim())}\";");
			else if (ColorLiteral(item, xNs) is string color)
				w.Line($"{target} = {color};");
			else if (IsTextValue(item, xNs) && ConverterBackedStructs.Contains(ResolveType(item) ?? ""))
				w.Line($"{target} = {Converter}.ConvertByType(typeof(global::{ResolveType(item)}), "
					+ $"\"{EscapeString(item.Value.Trim())}\");");
			else if (IsBareObject(item, xNs, out string fq))
				w.Line($"{target} = new global::{fq}();");
			else if (IsStaticResourceTarget(item, key))
			{
				// Kept so a {StaticResource} to this key resolves (a missing key would throw at load).
				w.Line($"{target} = \"{EscapeString(item.Value.Trim())}\";");
				Note($"Resource {item.Name.LocalName}{LineOf(item)} kept as a placeholder: a StaticResource uses it.");
			}
			else
				Note($"Skipped resource {item.Name.LocalName}{LineOf(item)}: not supported by the markup compiler.");
		}

		bool IsPrimitiveValueElement(XElement item, XNamespace xNs)
		{
			string ns = item.Name.NamespaceName;
			bool system = ns == xNs.NamespaceName || ns == "clr-namespace:System"
				|| ns.StartsWith("clr-namespace:System;", StringComparison.Ordinal);
			return system && !item.HasElements && !HasPlainAttributes(item, xNs)
				&& Array.IndexOf(_primitiveElements, item.Name.LocalName) >= 0;
		}

		bool IsTextValue(XElement item, XNamespace xNs)
			=> !item.HasElements && item.Value.Trim().Length > 0 && !HasPlainAttributes(item, xNs);

		bool HasPlainAttributes(XElement item, XNamespace xNs)
			=> item.Attributes().Any(a => !a.IsNamespaceDeclaration && a.Name.Namespace != xNs
				&& a.Name.Namespace != XNamespace.Xml && !IsIgnorableAttribute(a));

		bool IsBareObject(XElement item, XNamespace xNs, out string fq)
		{
			fq = ResolveType(item);
			return fq != null && !item.HasElements && item.Value.Trim().Length == 0 && !HasPlainAttributes(item, xNs)
				&& !IsMediaObjectType(fq) && _resolver.IsInstantiable(fq);
		}

		// <Color R= G= B= A=/> or <Color ScA= ScR= .../> of byte/float literals, else null.
		string ColorLiteral(XElement item, XNamespace xNs)
		{
			if (item.Name.NamespaceName != WpfXmlnsMap.PresentationNs || item.Name.LocalName != "Color"
				|| item.HasElements || item.Value.Trim().Length > 0) return null;
			var attrs = item.Attributes().Where(a => !a.IsNamespaceDeclaration && a.Name.Namespace != xNs).ToList();
			if (attrs.Count == 0 || attrs.Any(a => a.Name.Namespace != XNamespace.None)) return null;
			bool sc = attrs.Any(a => Array.IndexOf(_scColorAttributes, a.Name.LocalName) >= 0);
			string[] names = sc ? _scColorAttributes : _colorAttributes;
			if (attrs.Any(a => Array.IndexOf(names, a.Name.LocalName) < 0)) return null;
			var values = new string[4];
			for (int i = 0; i < 4; i++)
			{
				string v = (string)attrs.FirstOrDefault(a => a.Name.LocalName == names[i]);
				if (v == null) values[i] = "0";
				else if (sc && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out float f)
					&& !float.IsNaN(f) && !float.IsInfinity(f))
					values[i] = f.ToString("R", CultureInfo.InvariantCulture) + "f";
				else if (!sc && byte.TryParse(v.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out byte b))
					values[i] = b.ToString(CultureInfo.InvariantCulture);
				else return null;
			}
			return $"global::System.Windows.Media.Color.{(sc ? "FromScRgb" : "FromArgb")}({string.Join(", ", values)})";
		}

		bool IsStaticResourceTarget(XElement item, string key)
		{
			if (ProjectStaticResourceKeys?.Contains(key) == true) return true;
			var re = new System.Text.RegularExpressions.Regex(@"\{\s*StaticResource\s+(ResourceKey\s*=\s*)?"
				+ System.Text.RegularExpressions.Regex.Escape(key) + @"\s*[,}]");
			return item.Document?.Root.DescendantsAndSelf().Any(e => e.Attributes().Any(a => re.IsMatch(a.Value))) == true;
		}

		static string LineOf(XObject at)
			=> at is System.Xml.IXmlLineInfo li && li.HasLineInfo() ? $" (line {li.LineNumber})" : "";
	}
}
