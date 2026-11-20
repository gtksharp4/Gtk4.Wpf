using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// Elements that own a root-scope field. Templates have their own name scope, so their names get none.
		readonly HashSet<XElement> _fieldElements = new HashSet<XElement>();
		readonly HashSet<string> _fieldNames = new HashSet<string>(StringComparer.Ordinal);

		static bool IsNameScopeHost(XElement e)
			=> e.Name.NamespaceName == WpfXmlnsMap.PresentationNs
				&& e.Name.LocalName.EndsWith("Template", StringComparison.Ordinal);

		static string NameOf(XElement el, XNamespace xNs)
			=> (string)(el.Attribute(xNs + "Name") ?? el.Attribute("Name"));

		/// <summary>Collects the root-scope named elements; the first element of a name owns its field.</summary>
		void CollectNamedElements(XElement parent, XNamespace xNs, List<(string, string)> results)
		{
			foreach (XElement child in parent.Elements())
			{
				if (IsNameScopeHost(child)) continue;
				string name = NameOf(child, xNs);
				if (name != null && ResolveType(child) is string fqType && _fieldNames.Add(name))
				{
					results.Add((name, fqType));
					_fieldElements.Add(child);
				}
				CollectNamedElements(child, xNs, results);
			}
		}

		/// <summary>The root's own <c>x:Name</c> field (bound to <c>this</c>), unless it would clash.</summary>
		string RootNameField(XElement root, XNamespace xNs, string typeName, List<(string, string)> fields)
		{
			string name = NameOf(root, xNs);
			string fq = ResolveType(root);
			if (name == null || name == typeName || fq == null || !_fieldNames.Add(name)) return null;
			fields.Insert(0, (name, fq));
			return name;
		}
	}
}
