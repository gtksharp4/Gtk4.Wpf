using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Emits <c>&lt;Owner.Prop&gt;&lt;Value/&gt;&lt;/Owner.Prop&gt;</c> as <c>Owner.SetProp(parent, value)</c> when
		/// <c>Owner</c> is not the parent's own type and declares that static setter (an attached property
		/// element, such as <c>WindowChrome.WindowChrome</c> on a Window root).
		/// </summary>
		bool TryEmitAttachedPropertyElement(
			IndentedWriter w, XElement pe, XNamespace xNs, string parentVar, string ownerLocalName,
			string propName, ref int counter)
		{
			if (!_resolver.HasReferences) return false;
			string ownerFq = QualifyTypeName(ownerLocalName);
			string parentFq = pe.Parent == null ? null : ResolveType(pe.Parent);
			if (ownerFq == parentFq || (parentFq != null && _resolver.DerivesFrom(parentFq, ownerFq))) return false;
			if (_resolver.GetAttachedSetterValueType(ownerFq, propName) == null) return false;
			XElement child = pe.Elements().FirstOrDefault();
			string childFq = child == null ? null : ResolveType(child);
			if (childFq == null || pe.Elements().Count() != 1) return false;
			string v = $"__e{counter++}";
			EmitElement(w, child, xNs, v, childFq, ref counter);
			w.Line($"{Emitted(ownerFq)}.Set{propName}({parentVar}, {v});");
			return true;
		}
	}
}
