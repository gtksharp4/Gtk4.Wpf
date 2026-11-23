using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// Property elements that need object construction the generic paths do not do (sweep IV).
		bool TryEmitSweep4PropertyElement(
			IndentedWriter w, XElement pe, XNamespace xNs, string parentVar, string prop, ref int counter)
		{
			if (pe.Parent == null || !_resolver.HasReferences) return false;
			string local = pe.Name.LocalName;
			if (local.Substring(0, local.IndexOf('.')) != pe.Parent.Name.LocalName) return false;
			switch (prop)
			{
				case "CommandBindings": return TryEmitCommandBindings(w, pe, parentVar);
				case "TextEffects": return TryEmitTextEffects(w, pe, xNs, parentVar, ref counter);
				default:
					return TryEmitStyleProperty(w, pe, xNs, parentVar, prop, ref counter)
						|| TryEmitUserObjectProperty(w, pe, xNs, parentVar, prop, ref counter);
			}
		}

		bool OnlyElements(XElement pe, string name)
			=> pe.Elements().Any() && pe.Elements().All(e => e.Name.LocalName == name)
				&& !pe.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value));
	}
}
