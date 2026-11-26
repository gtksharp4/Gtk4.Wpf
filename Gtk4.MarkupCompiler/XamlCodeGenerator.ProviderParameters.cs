using System.Collections.Generic;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		static bool IsParameterList(XElement pe)
			=> pe.Name.LocalName == "ObjectDataProvider.MethodParameters"
				|| pe.Name.LocalName == "ObjectDataProvider.ConstructorParameters";

		IEnumerable<XElement> ParameterItems(XElement provider)
		{
			foreach (XElement pe in provider.Elements())
				if (IsParameterList(pe))
					foreach (XElement p in pe.Elements())
						yield return p;
		}

		// An ObjectDataProvider keeps the plain emission unless every parameter can be emitted.
		bool ParametersEmittable(XElement item)
		{
			if (item.Name.LocalName != "ObjectDataProvider") return true;
			foreach (XElement p in ParameterItems(item))
				if (ParameterExpression(p) == null) return false;
			return true;
		}

		string ParameterExpression(XElement p)
		{
			try
			{
				if (p.Name.Namespace == _xNs) return XParameterExpression(p);
				string pt = ResourceObjectType(p);
				if (pt == null && p.Name.NamespaceName.StartsWith(
					"clr-namespace:System", System.StringComparison.Ordinal))
					pt = "global::" + p.Name.NamespaceName.Substring("clr-namespace:".Length).Split(';')[0].Trim()
						+ "." + p.Name.LocalName;
				return pt == null ? null : "System.Windows.Markup.XamlValueConverter.ConvertByType(" +
					$"typeof({pt}), \"{EscapeString(p.Value.Trim())}\")";
			}
			catch (XamlCodeGeneratorException)
			{
				return null;
			}
		}

		string XParameterExpression(XElement p)
		{
			switch (p.Name.LocalName)
			{
				case "Null": return "null";
				case "Type":
					string tn = (string)p.Attribute("TypeName");
					return tn == null ? null : TypeRefExpression(tn);
				case "Static":
					string member = (string)p.Attribute("Member");
					return member != null
						&& IsXStatic("{x:Static " + member + "}", out string se, _prefixMap, ProbePresentationType)
						? se : null;
			}
			return null;
		}

		void EmitProviderParameters(IndentedWriter w, XElement item, string v)
		{
			foreach (XElement pe in item.Elements())
			{
				if (!IsParameterList(pe)) continue;
				string list = pe.Name.LocalName.Substring(pe.Name.LocalName.IndexOf('.') + 1);
				foreach (XElement p in pe.Elements())
					w.Line($"{v}.{list}.Add({ParameterExpression(p)});");
			}
		}
	}
}
