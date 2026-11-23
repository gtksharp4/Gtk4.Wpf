using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// A keyed element with attributes or children is an object (WPF never stores its text): an element or a
		// DependencyObject the layer can instantiate (Border, DoubleAnimation). Media objects and data providers
		// have their own paths; the type it returns is built through the normal element path.
		string ObjectResourceType(XElement item, XNamespace xNs)
		{
			bool content = item.HasElements || item.Attributes().Any(a => !a.IsNamespaceDeclaration
				&& a.Name.Namespace != xNs && a.Name.Namespace != XNamespace.Xml && !IsIgnorableAttribute(a));
			string fq = content ? ResolveType(item) : null;
			return fq != null && !IsMediaObjectType(fq) && _resolver.IsInstantiable(fq)
				&& (IsElementObject(fq) || _resolver.IsDependencyObject(fq)) ? fq : null;
		}
	}
}
