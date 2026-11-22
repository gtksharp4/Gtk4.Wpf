using System;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// An attribute of a media object the layer cannot express: an attached property whose owner or setter is
		// missing (RenderOptions.CachingHint), or a {Binding} on a plain CLR property (VisualBrush.Visual). It is
		// dropped with a message; the rest of the object is still built.
		bool SkippableMediaAttribute(XElement el, string fq, XAttribute a)
		{
			if (!_resolver.HasReferences || !IsMediaObjectType(fq) || !IsPlainAttribute(a)) return false;
			string name = a.Name.LocalName;
			int dot = name.IndexOf('.');
			if (dot > 0)
			{
				string ownerFq = ResolveType(new XElement(el.Name.Namespace + name.Substring(0, dot)));
				return ownerFq == null || _resolver.GetAttachedSetterValueType(ownerFq, name.Substring(dot + 1)) == null;
			}
			return IsBindingExpression(a.Value, out _) && !IsKnownDp(fq, name);
		}
	}
}
