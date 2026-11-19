using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
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
				if (prop.Name.LocalName != "Binding.ValidationRules") continue;
				foreach (XElement rule in prop.Elements())
				{
					string ruleType = ResolveType(rule) ?? ClrNamespaceType(rule);
					if (ruleType == null) continue;
					string ruleVar = "__vr" + counter++;
					w.Line("var " + ruleVar + " = new " + ruleType + "();");
					foreach (XAttribute ra in rule.Attributes())
					{
						if (ra.IsNamespaceDeclaration) continue;
						w.Line(ruleVar + "." + ra.Name.LocalName + " = "
							+ ClrValueExpression(ra.Name.LocalName, ra.Value) + ";");
					}
					w.Line(bVar + ".ValidationRules.Add(" + ruleVar + ");");
				}
			}
		}

		static string ElementBindingInit(string name, string value)
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
						: "Converter = (System.Windows.Data.IValueConverter)this.FindResource(\""
							+ EscapeString(key) + "\")";
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
