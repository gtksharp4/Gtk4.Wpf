using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// A keyless DataTemplate / HierarchicalDataTemplate with a DataType is stored under
		// DataTemplateKey(DataType), WPF's DictionaryKeyProperty, so it applies implicitly.
		bool EmitImplicitDataTemplate(
			IndentedWriter w, XElement item, XNamespace xNs, string ownerVar, ref int counter)
		{
			if (item.Name.NamespaceName != WpfXmlnsMap.PresentationNs) return false;
			string name = item.Name.LocalName;
			if (name != "DataTemplate" && name != "HierarchicalDataTemplate") return false;
			string dataTypeAttr = (string)item.Attribute("DataType");
			if (dataTypeAttr == null) return false;

			string tmplVar = $"__dt{counter++}";
			if (name == "DataTemplate")
				EmitDataTemplateInline(w, item, xNs, tmplVar, ref counter);
			else
				EmitHierarchicalTemplateInline(w, item, xNs, tmplVar, ownerVar, ref counter);
			string keyExpr = DataTypeExpression(dataTypeAttr);
			w.Line($"{ownerVar}.Resources[new System.Windows.DataTemplateKey({keyExpr})] = {tmplVar};");
			return true;
		}
	}
}
