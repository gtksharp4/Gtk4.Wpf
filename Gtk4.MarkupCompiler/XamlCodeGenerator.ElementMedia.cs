using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// <Image.Clip><EllipseGeometry/></Image.Clip>, <DockPanel.Background><DrawingBrush/>...: an element's own
		// property holding one media object. Names inside it register through EmitElement.
		bool TryEmitElementMediaProperty(
			IndentedWriter w, XElement pe, XNamespace xNs, string parentVar, string prop, ref int counter)
		{
			string fq = pe.Parent == null ? null : ResolveType(pe.Parent);
			if (fq == null || !_resolver.HasReferences) return false;
			string local = pe.Name.LocalName;
			if (local.Substring(0, local.IndexOf('.')) != pe.Parent.Name.LocalName) return false;
			System.Type pt = _resolver.GetPropertyType(fq, prop);
			if (pt == null || !_resolver.IsWritable(fq, prop)) return false;
			if (pe.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))) return false;
			var children = pe.Elements().ToList();
			if (children.Count != 1) return false;
			string cfq = ResolveType(children[0]);
			if (cfq == null || !IsMediaObjectType(cfq) || !_resolver.IsAssignable(cfq, pt)
				|| !MediaGraphEmittable(children[0], cfq)) return false;
			string v = $"__e{counter++}";
			EmitElement(w, children[0], xNs, v, cfq, ref counter);
			w.Line(_resolver.IsDP(fq, prop) && _resolver.IsDependencyObject(fq)
				? $"{parentVar}.SetValue({fq}.{prop}Property, {v});"
				: $"{parentVar}.{prop} = {v};");
			return true;
		}
	}
}
