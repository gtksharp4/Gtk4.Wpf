using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// <Window.DataContext><local:ViewModel/></Window.DataContext>: one project-type object assigned to a
		// writable System.Object property.
		bool TryEmitUserObjectProperty(
			IndentedWriter w, XElement pe, XNamespace xNs, string parentVar, string prop, ref int counter)
		{
			string fq = ResolveType(pe.Parent);
			var kids = pe.Elements().ToList();
			if (fq == null || kids.Count != 1 || kids[0].Name.LocalName.IndexOf('.') >= 0
				|| pe.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))
				|| !_resolver.IsWritable(fq, prop) || _resolver.GetPropertyType(fq, prop) != typeof(object)) return false;
			string kidFq = UserElementFqType(kids[0]);
			if (kidFq == null || ResolveType(kids[0]) != null) return false;
			string v = $"__e{counter++}";
			EmitElement(w, kids[0], xNs, v, kidFq, ref counter);
			w.Line(IsKnownDp(fq, prop) ? $"{parentVar}.SetValue({fq}.{prop}Property, {v});"
				: $"{parentVar}.{prop} = {v};");
			return true;
		}
	}
}
