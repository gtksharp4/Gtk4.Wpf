using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string Converter = "System.Windows.Markup.XamlValueConverter";

		// The element variable of a user-type resource object the resolver cannot see; its literal
		// attributes go through the runtime converter hook instead of being guessed.
		string _hookVar;

		/// <summary>
		/// The type of a keyed resource element that must be instantiated: a user <c>clr-namespace</c> type
		/// (outside <c>System</c>), a data provider or a SolidColorBrush. <c>null</c> keeps the plain-value
		/// emission.
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
				if (clrNs.Length == 0 || clrNs == "System"
					|| clrNs.StartsWith("System.", System.StringComparison.Ordinal))
					return null;
				string user = clrNs + "." + item.Name.LocalName;
				return !_resolver.TypeExists(user) && HasDynamicMarkup(item) ? null : "global::" + user;
			}
			string local = item.Name.LocalName;
			if (ns == WpfXmlnsMap.PresentationNs
				&& (local == "XmlDataProvider" || local == "ObjectDataProvider" || local == "SolidColorBrush"
					|| local == "CollectionViewSource"))
				return ResolveType(item) is string fq && AttributesResolve(item, fq) && ParametersEmittable(item)
					? fq : null;
			return MediaResourceType(item);
		}

		bool IsStaticResourceValue(string value, out string key)
			=> IsStaticOrDynamicResource(value, out key)
				&& value.TrimStart().StartsWith("{StaticResource", System.StringComparison.Ordinal);

		bool NeedsDependencyProperty(string value)
			=> IsBindingExpression(value, out _) || IsTemplateBindingExpression(value, out _)
				|| value.TrimStart().StartsWith("{DynamicResource", System.StringComparison.Ordinal);

		// Binding-like values cannot be assigned to a type the layer cannot see.
		bool HasDynamicMarkup(XElement item)
			=> item.Attributes().Any(a => !a.IsNamespaceDeclaration && a.Name.Namespace != _xNs
				&& NeedsDependencyProperty(a.Value));

		// A provider with an attribute the layer lacks (e.g. XmlNamespaceManager) keeps the plain emission,
		// as does a binding-like value on a property that is not a DependencyProperty.
		bool AttributesResolve(XElement item, string fq)
		{
			foreach (XAttribute a in item.Attributes())
			{
				if (a.IsNamespaceDeclaration || a.Name.Namespace == _xNs || a.Name.Namespace == XNamespace.Xml
					|| IsIgnorableAttribute(a))
					continue;
				string n = a.Name.LocalName;
				if (_resolver.GetPropertyType(fq, n) == null && !IsKnownEvent(fq, n)) return false;
				if (NeedsDependencyProperty(a.Value) && !IsKnownDp(fq, n)) return false;
			}
			return true;
		}

		/// <summary>
		/// Emits a literal or <c>{StaticResource}</c> attribute of an unresolved user type through the runtime
		/// hook. Returns false for anything else (the regular emission handles it).
		/// </summary>
		bool TryEmitHookProperty(IndentedWriter w, string varName, string propName, string val, string find)
		{
			if (varName != _hookVar) return false;
			string name = EscapeString(propName);
			string t = val.TrimStart();
			bool escaped = t.StartsWith("{}", System.StringComparison.Ordinal);
			if (find != null)
				w.Line($"{Converter}.SetClrValue({varName}, \"{name}\", {find});");
			else if (escaped || !t.StartsWith("{", System.StringComparison.Ordinal))
				w.Line($"{Converter}.SetClrProperty({varName}, \"{name}\", " +
					$"\"{EscapeString(escaped ? t.Substring(2) : val)}\");");
			else return false;
			return true;
		}

		/// <summary>
		/// The cast type when a <c>{StaticResource}</c> targets a plain CLR property of a layer type (a non-DP
		/// owner such as SolidColorBrush), else <c>null</c>.
		/// </summary>
		string ClrResourceTargetType(string fqType, string propName)
		{
			if (_resolver.IsDP(fqType, propName) && _resolver.IsDependencyObject(fqType)) return null;
			System.Type t = _resolver.GetPropertyType(fqType, propName);
			return t == null || t.IsGenericType ? null : "global::" + t.FullName.Replace('+', '.');
		}

		void EmitResourceObject(
			IndentedWriter w, XElement item, XNamespace xNs, string ownerVar, string key, string fq, ref int counter)
		{
			string v = $"__e{counter++}";
			string saved = _hookVar;
			_hookVar = fq.StartsWith("global::", System.StringComparison.Ordinal)
				&& !_resolver.TypeExists(fq.Substring("global::".Length)) ? v : null;
			EmitElement(w, item, xNs, v, fq, ref counter);
			_hookVar = saved;
			string local = item.Name.LocalName;
			if (local == "XmlDataProvider")
			{
				XElement xdata = item.Elements().FirstOrDefault(e => e.Name == xNs + "XData");
				XElement doc = xdata?.Elements().FirstOrDefault();
				if (doc != null)
				{
					w.Line($"{v}.Document = new global::System.Xml.XmlDocument {{ XmlResolver = null }};");
					string xml = EscapeString(doc.ToString(SaveOptions.DisableFormatting));
					w.Line($"{v}.Document.LoadXml(\"{xml}\");");
				}
			}
			else if (local == "ObjectDataProvider") EmitProviderParameters(w, item, v);
			w.Line($"{ownerVar}.Resources[\"{EscapeString(key)}\"] = {v};");
		}
	}
}
