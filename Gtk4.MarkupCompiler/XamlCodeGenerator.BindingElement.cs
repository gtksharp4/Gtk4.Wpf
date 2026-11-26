using System;
using System.Globalization;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Emits a local <paramref name="bVar"/> of type <c>System.Windows.Data.Binding</c> from an
		/// element-syntax <c>&lt;Binding ...&gt;</c>, including <c>&lt;Binding.ValidationRules&gt;</c>.
		/// </summary>
		void EmitBindingElementInline(IndentedWriter w, XElement bindEl, string bVar, ref int counter)
		{
			w.Line("var " + bVar + " = new System.Windows.Data.Binding();");

			foreach (XAttribute attr in bindEl.Attributes())
			{
				if (attr.IsNamespaceDeclaration) continue;
				string init = ElementBindingInit(attr.Name.LocalName, attr.Value);
				if (init != null) w.Line(bVar + "." + init + ";");
			}

			foreach (XElement prop in bindEl.Elements())
			{
				if (prop.Name.LocalName == "Binding.Converter")
				{
					EmitBindingConverterElement(w, prop, bVar, ref counter);
					continue;
				}
				if (prop.Name.LocalName != "Binding.ValidationRules")
					throw new XamlCodeGeneratorException(
						"Unsupported Binding property element <" + prop.Name.LocalName + ">.");
				EmitValidationRules(w, prop, bVar + ".ValidationRules", ref counter);
			}
		}

		// Each child of a ValidationRules property element becomes a rule added to <rulesVar>.
		void EmitValidationRules(IndentedWriter w, XElement rulesEl, string rulesVar, ref int counter)
		{
			foreach (XElement rule in rulesEl.Elements())
			{
				string ruleType = ResolveType(rule) ?? ClrNamespaceType(rule);
				if (ruleType == null)
					throw new XamlCodeGeneratorException(
						"Cannot resolve validation rule type <" + rule.Name.LocalName + ">.");
				string ruleVar = "__vr" + counter++;
				w.Line("var " + ruleVar + " = new " + ruleType + "();");
				foreach (XAttribute ra in rule.Attributes())
				{
					if (ra.IsNamespaceDeclaration) continue;
					w.Line(ruleVar + "." + ra.Name.LocalName + " = "
						+ RuleAttrExpression(ruleType, ra.Name.LocalName, ra.Value) + ";");
				}
				w.Line(rulesVar + ".Add(" + ruleVar + ");");
			}
		}

		// <Binding.Converter> with a single inline converter object.
		void EmitBindingConverterElement(IndentedWriter w, XElement prop, string bVar, ref int counter)
		{
			XNamespace xNs = _xNs ?? XNamespace.Get(WpfXmlnsMap.XNs2006);
			foreach (XElement child in prop.Elements())
			{
				string fq = ResolveType(child) ?? ClrNamespaceType(child);
				if (fq == null)
					throw new XamlCodeGeneratorException(
						"Cannot resolve converter type <" + child.Name.LocalName + ">.");
				string cVar = "__cv" + counter++;
				EmitElement(w, child, xNs, cVar, fq, ref counter);
				w.Line(bVar + ".Converter = (System.Windows.Data.IValueConverter)" + cVar + ";");
				return;
			}
		}

		string ElementBindingInit(string name, string value)
		{
			switch (name)
			{
				case "Path":
				case "ElementName":
				case "StringFormat":
				case "FallbackValue":
				case "TargetNullValue":
					return name + " = \"" + EscapeString(value) + "\"";
				case "Mode":
					return "Mode = System.Windows.Data.BindingMode." + value.Trim();
				case "UpdateSourceTrigger":
					return "UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger." + value.Trim();
				case "Converter":
					string key = ParseConverterArg(value);
					return key == null ? null
						: "Converter = (System.Windows.Data.IValueConverter)"
							+ StaticResourceLookup("this", EscapeString(key));
				case "RelativeSource":
					string rs = ParseRelativeSourceArg(value);
					return rs == null ? null : "RelativeSource = " + rs;
				default:
					return ExtraBindingInit(name, value);
			}
		}

		// Custom rules live in a clr-namespace: xmlns, which ResolveType does not map.
		static string ClrNamespaceType(XElement el)
		{
			string ns = el.Name.NamespaceName;
			const string prefix = "clr-namespace:";
			if (!ns.StartsWith(prefix, System.StringComparison.Ordinal)) return null;
			string clr = ns.Substring(prefix.Length);
			int semi = clr.IndexOf(';');
			if (semi >= 0) clr = clr.Substring(0, semi);
			return clr.Length == 0 ? null : clr + "." + el.Name.LocalName;
		}
	}
}
