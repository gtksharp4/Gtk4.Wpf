using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// False when the XAML has no code-behind: the generated partial then declares the root
		/// element type as its base and (for non-Application roots) a constructor, so it compiles alone.
		/// </summary>
		public bool HasCodeBehind { get; set; } = true;

		/// <summary>
		/// Source text of the project's C# files. When set, code-behind presence is decided by a
		/// <c>partial class</c> declaration of the x:Class there, instead of <see cref="HasCodeBehind"/>.
		/// </summary>
		public System.Collections.Generic.IReadOnlyList<string> CodeBehindSources { get; set; }

		bool HasCodeBehindFor(string className)
		{
			if (CodeBehindSources == null) return HasCodeBehind;
			string name = System.Text.RegularExpressions.Regex.Escape(className.Substring(className.LastIndexOf('.') + 1));
			var re = new System.Text.RegularExpressions.Regex(@"\bpartial\s+class\s+" + name + @"\b");
			foreach (string s in CodeBehindSources)
				if (re.IsMatch(s)) return true;
			return false;
		}

		/// <summary>
		/// The type of a keyed resource element that must be instantiated: a user <c>clr-namespace</c> type
		/// (outside <c>System</c>), a data provider or a SolidColorBrush. <c>null</c> keeps the plain-value emission.
		/// </summary>
		string ResourceObjectType(XElement item)
		{
			string ns = item.Name.NamespaceName;
			const string clr = "clr-namespace:";
			if (ns.StartsWith(clr, System.StringComparison.Ordinal))
			{
				string clrNs = ns.Substring(clr.Length);
				int semi = clrNs.IndexOf(';');
				if (semi >= 0) clrNs = clrNs.Substring(0, semi);
				clrNs = clrNs.Trim();
				if (clrNs.Length == 0 || clrNs == "System" || clrNs.StartsWith("System.", System.StringComparison.Ordinal))
					return null;
				return "global::" + clrNs + "." + item.Name.LocalName;
			}
			string local = item.Name.LocalName;
			if (ns == WpfXmlnsMap.PresentationNs
				&& (local == "XmlDataProvider" || local == "ObjectDataProvider" || local == "SolidColorBrush"))
				return ResolveType(item) is string fq && AttributesResolve(item, fq) ? fq : null;
			return null;
		}

		// A provider with an attribute the layer lacks (e.g. XmlNamespaceManager) keeps the plain emission.
		bool AttributesResolve(XElement item, string fq)
		{
			foreach (XAttribute a in item.Attributes())
			{
				if (a.IsNamespaceDeclaration || a.Name.Namespace == _xNs || a.Name.Namespace == XNamespace.Xml
					|| IsIgnorableAttribute(a))
					continue;
				string n = a.Name.LocalName;
				if (_resolver.GetPropertyType(fq, n) == null && !IsKnownEvent(fq, n)) return false;
			}
			return true;
		}

		void EmitResourceObject(
			IndentedWriter w, XElement item, XNamespace xNs, string ownerVar, string key, string fq, ref int counter)
		{
			string v = $"__e{counter++}";
			EmitElement(w, item, xNs, v, fq, ref counter);
			string local = item.Name.LocalName;
			if (local == "XmlDataProvider")
			{
				XElement xdata = item.Elements().FirstOrDefault(e => e.Name == xNs + "XData");
				XElement doc = xdata?.Elements().FirstOrDefault();
				if (doc != null)
				{
					w.Line($"{v}.Document = new global::System.Xml.XmlDocument {{ XmlResolver = null }};");
					w.Line($"{v}.Document.LoadXml(\"{EscapeString(doc.ToString(SaveOptions.DisableFormatting))}\");");
				}
			}
			else if (local == "ObjectDataProvider")
			{
				foreach (XElement pe in item.Elements())
				{
					string pn = pe.Name.LocalName;
					if (pn != "ObjectDataProvider.MethodParameters" && pn != "ObjectDataProvider.ConstructorParameters")
						continue;
					foreach (XElement p in pe.Elements())
					{
						string pt = ResourceObjectType(p);
						if (pt == null && p.Name.NamespaceName.StartsWith("clr-namespace:System", System.StringComparison.Ordinal))
							pt = "global::" + p.Name.NamespaceName.Substring("clr-namespace:".Length).Split(';')[0].Trim()
								+ "." + p.Name.LocalName;
						if (pt == null) continue;
						w.Line($"{v}.{pn.Substring(pn.IndexOf('.') + 1)}.Add(System.Windows.Markup.XamlValueConverter" +
							$".ConvertByType(typeof({pt}), \"{EscapeString(p.Value.Trim())}\"));");
					}
				}
			}
			w.Line($"{ownerVar}.Resources[\"{EscapeString(key)}\"] = {v};");
		}
	}
}
