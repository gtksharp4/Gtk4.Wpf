using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// <Transform3DGroup.Children><Transform3DCollection>items</Transform3DCollection></...>: the explicit
		// collection element is unwrapped so its items are emitted one by one into the collection property.
		internal void UnwrapExplicitCollections(XElement root)
		{
			if (!_resolver.HasReferences) return;
			foreach (XElement pe in root.Descendants().Where(e => e.Name.LocalName.IndexOf('.') > 0).ToList())
			{
				XElement owner = pe.Parent;
				var kids = pe.Elements().ToList();
				if (owner == null || kids.Count != 1 || kids[0].HasAttributes
					|| pe.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))) continue;
				string local = pe.Name.LocalName;
				int dot = local.IndexOf('.');
				string ownerFq = ResolveType(owner);
				string prop = local.Substring(dot + 1);
				if (ownerFq == null || local.Substring(0, dot) != owner.Name.LocalName) continue;
				System.Type item = _resolver.CollectionItemType(ownerFq, prop);
				string kidFq = ResolveType(kids[0]);
				if (item == null || kidFq == null || kids[0].Name.LocalName.IndexOf('.') >= 0
					|| _resolver.IsAssignable(kidFq, item)
					|| !_resolver.IsAssignable(kidFq, _resolver.GetPropertyType(ownerFq, prop))) continue;
				var nodes = kids[0].Nodes().ToList();
				foreach (XNode n in nodes) n.Remove();
				kids[0].ReplaceWith(nodes);
			}
		}
	}
}
