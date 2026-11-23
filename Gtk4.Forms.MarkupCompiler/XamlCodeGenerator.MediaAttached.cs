using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// An attached attribute whose owner has a setter (RenderOptions.CachingHint) is written by EmitElement as
		// Owner.SetX(el, value), so it does not stop a media graph from being built.
		bool IsEmittableAttachedAttribute(XElement el, XAttribute a)
		{
			string name = a.Name.LocalName;
			int dot = name.IndexOf('.');
			string ownerFq = ResolveType(new XElement(el.Name.Namespace + name.Substring(0, dot)));
			return ownerFq != null && !a.Value.TrimStart().StartsWith("{", System.StringComparison.Ordinal)
				&& _resolver.GetAttachedSetterValueType(ownerFq, name.Substring(dot + 1)) != null;
		}
	}
}
