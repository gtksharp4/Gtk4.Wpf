using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// x:Type keywords of the XAML language namespace, as C# keywords.
		static readonly Dictionary<string, string> XamlKeywordTypes = new Dictionary<string, string>
		{
			{ "String", "string" }, { "Int32", "int" }, { "Int64", "long" }, { "Int16", "short" },
			{ "Double", "double" }, { "Single", "float" }, { "Boolean", "bool" }, { "Byte", "byte" },
			{ "Char", "char" }, { "Decimal", "decimal" }, { "Object", "object" },
		};

		/// <summary>
		/// The generic base of a root with <c>x:TypeArguments</c>, e.g.
		/// <c>global::System.Windows.Navigation.PageFunction&lt;global::Wizard.WizardResult&gt;</c>;
		/// <c>null</c> when the root declares none or its generic definition cannot be found.
		/// </summary>
		string GenericRootBase(XElement root)
		{
			string typeArgs = (string)root.Attribute(_xNs + "TypeArguments");
			if (string.IsNullOrWhiteSpace(typeArgs)) return null;

			var args = new List<string>();
			foreach (string part in typeArgs.Split(','))
				args.Add(TypeArgumentName(part.Trim()));

			string definition = GenericDefinition(root.Name, args.Count);
			return definition == null ? null : "global::" + definition + "<" + string.Join(", ", args) + ">";
		}

		string TypeArgumentName(string value)
		{
			if (TryParseTypeRef(value, out string prefix, out string local)
				&& prefix.Length > 0 && !_prefixMap.ContainsKey(prefix)
				&& XamlKeywordTypes.TryGetValue(local, out string keyword))
				return keyword;
			return ResolveTypeRefName(value) ?? value;
		}

		string GenericDefinition(XName name, int arity)
		{
			string local = name.LocalName + "`" + arity;
			if (name.NamespaceName == WpfXmlnsMap.PresentationNs)
			{
				string probed = ProbePresentationType(local);
				if (probed != null && _resolver.TypeExists(probed)) return StripArity(probed);
				// The layer's navigation types may not be visible to the resolver yet.
				return name.LocalName == "PageFunction" ? "System.Windows.Navigation.PageFunction" : null;
			}
			foreach (XmlnsDefinitionEntry entry in _xmlnsSource.GetEntries())
				if (entry.XmlNamespace == name.NamespaceName && _resolver.TypeExists(entry.ClrNamespace + "." + local))
					return entry.ClrNamespace + "." + name.LocalName;
			const string clr = "clr-namespace:";
			if (name.NamespaceName.StartsWith(clr, StringComparison.Ordinal))
			{
				string ns = name.NamespaceName.Substring(clr.Length);
				int semi = ns.IndexOf(';');
				return (semi >= 0 ? ns.Substring(0, semi) : ns).Trim() + "." + name.LocalName;
			}
			return null;
		}

		static string StripArity(string name)
		{
			int tick = name.IndexOf('`');
			return tick >= 0 ? name.Substring(0, tick) : name;
		}
	}
}
