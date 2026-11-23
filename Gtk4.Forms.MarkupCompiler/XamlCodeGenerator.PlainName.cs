using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string FrameworkContentElementTypeName = "System.Windows.FrameworkContentElement";

		// A plain Name="..." names the element only on FrameworkElement/FrameworkContentElement or a type with
		// [RuntimeNameProperty("Name")]; elsewhere (and for a markup extension value) it is an ordinary property.
		// An unresolved or user type, or one without a Name property at all, keeps the legacy reading.
		bool IsNamingAttribute(XElement el, XAttribute a)
		{
			if (a == null) return false;
			if (a.Name.Namespace != XNamespace.None) return true;
			if (a.Value.TrimStart().StartsWith("{", System.StringComparison.Ordinal)) return false;
			string fq = ResolveType(el);
			if (fq == null || !_resolver.HasReferences) return true;
			if (_resolver.GetPropertyType(fq, "Name") == null) return true;
			return _resolver.DerivesFromByName(fq, FrameworkElementType)
				|| _resolver.DerivesFromByName(fq, FrameworkContentElementTypeName)
				|| _resolver.HasRuntimeNameProperty(fq);
		}
	}
}
