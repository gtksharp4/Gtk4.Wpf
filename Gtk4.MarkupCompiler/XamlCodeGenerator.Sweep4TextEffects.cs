using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string TextEffectType = "System.Windows.Media.TextEffect";

		// <TextBlock.TextEffects><TextEffect .../>: each effect is built (names registered with it) and added
		// to the element's collection. The property defaults to null, so a guard creates the collection first.
		bool TryEmitTextEffects(IndentedWriter w, XElement pe, XNamespace xNs, string parentVar, ref int counter)
		{
			if (!OnlyElements(pe, "TextEffect") || !_resolver.TypeExists(TextEffectType)) return false;
			if (_resolver.CollectionItemType(ResolveType(pe.Parent) ?? "", "TextEffects") == null) return false;
			w.Line($"if ({parentVar}.TextEffects == null) " +
				$"{parentVar}.TextEffects = new System.Windows.Media.TextEffectCollection();");
			foreach (XElement child in pe.Elements())
			{
				string v = $"__e{counter++}";
				EmitElement(w, child, xNs, v, ResolveType(child), ref counter);
				w.Line($"{parentVar}.TextEffects.Add({v});");
			}
			return true;
		}
	}
}
