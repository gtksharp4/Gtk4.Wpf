using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
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
			string ownerFq = ResolveAttachedOwner(pe, ownerLocalName);
			if (ownerFq == null) return false;
			string parentFq = pe.Parent == null ? null : ResolveType(pe.Parent);
			if (ownerFq == parentFq || (parentFq != null && _resolver.DerivesFrom(parentFq, ownerFq))) return false;
			if (_resolver.GetAttachedSetterValueType(ownerFq, propName) == null) return false;
			XElement child = pe.Elements().FirstOrDefault();
			if (child != null && IsDeferredPropertyChild(child.Name.LocalName)) return false;
			string childFq = child == null ? null : ResolveType(child);
			if (childFq == null || pe.Elements().Count() != 1) return false;
			string v = $"__e{counter++}";
			EmitElement(w, child, xNs, v, childFq, ref counter);
			w.Line($"{Emitted(ownerFq)}.Set{propName}({parentVar}, {v});");
			return true;
		}

		// Bindings, templates and markup extensions are not values: the SetBinding/template branches take them.
		static bool IsDeferredPropertyChild(string local)
			=> local == "Binding" || local == "MultiBinding" || local == "PriorityBinding"
				|| local.EndsWith("Template", System.StringComparison.Ordinal)
				|| local.EndsWith("Extension", System.StringComparison.Ordinal);

		// The owner type for a binding on an attached property element: a clr-namespace owner keeps its own
		// namespace (global::App.MyOwner), anything else is a layer type.
		string AttachedOwnerType(XElement pe, string ownerLocalName)
		{
			const string clr = "clr-namespace:";
			string ns = pe.Name.NamespaceName;
			if (!ns.StartsWith(clr, System.StringComparison.Ordinal)) return Emitted(QualifyTypeName(ownerLocalName));
			int semi = ns.IndexOf(';');
			string clrNs = (semi >= 0 ? ns.Substring(clr.Length, semi - clr.Length) : ns.Substring(clr.Length)).Trim();
			return "global::" + clrNs + "." + ownerLocalName;
		}

		// The owner type named by the property element's own xmlns prefix, else null.
		string ResolveAttachedOwner(XElement pe, string ownerLocalName)
		{
			var owner = new XElement(pe.Name.Namespace + ownerLocalName);
			return ResolveType(owner) ?? UserElementType(owner)
				?? (pe.Name.NamespaceName == WpfXmlnsMap.PresentationNs ? QualifyTypeName(ownerLocalName) : null);
		}
	}
}
