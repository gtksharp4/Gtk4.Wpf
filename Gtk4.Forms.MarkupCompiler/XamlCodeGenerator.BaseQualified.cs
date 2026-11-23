using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// WPF: <GradientBrush.GradientStops> inside a <RadialGradientBrush> names the element's own property when the
		// owner is the element's type or a base of it. Renaming it to the element's own type name lets every
		// property-element path (resources, collections, objects, media) treat it as an own property.
		void NormalizeBaseQualifiedPropertyElements(XElement root)
		{
			if (!_resolver.HasReferences) return;
			foreach (XElement el in root.DescendantsAndSelf().ToList())
			{
				if (el.Name.LocalName.IndexOf('.') >= 0) continue;
				string elFq = null;
				foreach (XElement pe in el.Elements().ToList())
				{
					string local = pe.Name.LocalName;
					int dot = local.IndexOf('.');
					if (dot <= 0 || local.Substring(0, dot) == el.Name.LocalName) continue;
					elFq = elFq ?? ResolveType(el);
					var ownerEl = new XElement(pe.Name.Namespace + local.Substring(0, dot));
					string ownerFq = elFq == null ? null : ResolveType(ownerEl);
					if (ownerFq == null || ownerFq == elFq || !_resolver.DerivesFromByName(elFq, ownerFq)) continue;
					pe.Name = el.Name.Namespace + el.Name.LocalName + local.Substring(dot);
				}
			}
		}
	}
}
