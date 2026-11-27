using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// <ItemsPanelTemplate><WrapPanel/></ItemsPanelTemplate>: a template whose factory builds the panel.
		void EmitItemsPanelTemplateInline(
			IndentedWriter w, XElement tmplEl, XNamespace xNs, string tmplVar, ref int counter)
		{
			w.Line($"var {tmplVar} = new System.Windows.Controls.ItemsPanelTemplate();");
			XElement root = FindTemplateRoot(tmplEl);
			string rootFqType = root == null ? null : ResolveType(root);
			if (rootFqType == null) return;
			string rootVar = $"__e{counter++}";
			w.Line($"{tmplVar}.Factory = () => {{");
			w.Indent++;
			EmitElement(w, root, xNs, rootVar, rootFqType, ref counter);
			w.Line($"return {rootVar};");
			w.Indent--;
			w.Line("};");
		}
	}
}
