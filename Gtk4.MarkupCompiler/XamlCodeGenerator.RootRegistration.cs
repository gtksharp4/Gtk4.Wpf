using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// An x:Class root of any element type (a Window, a Page, a Canvas) is registered under its XAML path, so
		// StartupUri, LoadComponent and navigation resolve a file whose class name differs from the file name.
		// Application and dictionary roots are handled elsewhere; a generic root cannot be named by typeof.
		bool RegistersClassRoot(XElement root, XAttribute classAttr, string xamlPath)
		{
			if (classAttr == null || string.IsNullOrEmpty(xamlPath)) return false;
			if (IsApplicationRoot(root) || IsDictionaryRoot(root)) return false;
			if (root.Attribute(_xNs + "TypeArguments") != null) return false;
			return HasPublicDefaultConstructor(classAttr.Value.Trim());
		}
	}
}
