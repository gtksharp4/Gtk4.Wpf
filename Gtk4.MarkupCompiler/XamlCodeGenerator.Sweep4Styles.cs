using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string StyleType = "System.Windows.Style";

		// <Style x:Key="k" ...> in a resource dictionary: built, then filed under its key.
		void EmitStyleResource(
			IndentedWriter w, XElement styleEl, XNamespace xNs, string ownerVar, string key, ref int counter)
		{
			string styleVar = EmitStyleObject(w, styleEl, xNs, ownerVar, ref counter);
			string styleKey = StyleResourceKey(key, (string)styleEl.Attribute("TargetType"));
			if (styleKey != null)
				w.Line($"{ownerVar}.Resources[{styleKey}] = {styleVar};");
		}

		// <ListBox.ItemContainerStyle><Style .../></ListBox.ItemContainerStyle>: a Style-typed property.
		bool TryEmitStyleProperty(
			IndentedWriter w, XElement pe, XNamespace xNs, string parentVar, string prop, ref int counter)
		{
			string fq = ResolveType(pe.Parent);
			if (fq == null || !OnlyElements(pe, "Style") || !_resolver.IsWritable(fq, prop)
				|| _resolver.GetPropertyType(fq, prop)?.FullName != StyleType) return false;
			string v = EmitStyleObject(w, pe.Elements().First(), xNs, parentVar, ref counter);
			w.Line(IsKnownDp(fq, prop) ? $"{parentVar}.SetValue({fq}.{prop}Property, {v});"
				: $"{parentVar}.{prop} = {v};");
			return true;
		}
	}
}
