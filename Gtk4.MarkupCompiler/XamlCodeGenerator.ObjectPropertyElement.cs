using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// Elements and media objects are what EmitElement builds as values; styles, templates and triggers are not.
		bool IsElementObject(string fq)
			=> IsMediaObjectType(fq) || _resolver.DerivesFromByName(fq, FrameworkElementType)
				|| _resolver.DerivesFromByName(fq, FrameworkContentElementType);

		// <Button.ContextMenu><ContextMenu/></Button.ContextMenu>, <Expander.Header><StackPanel/></Expander.Header>:
		// an element's own property holding objects. A writable property takes the single child (DP or CLR
		// setter); a read-only collection property gets each child added.
		bool TryEmitObjectPropertyElement(
			IndentedWriter w, XElement pe, XNamespace xNs, string parentVar, string prop, ref int counter)
		{
			string fq = pe.Parent == null ? null : ResolveType(pe.Parent);
			if (fq == null || !_resolver.HasReferences || IsHookedUserType(fq)) return false;
			string local = pe.Name.LocalName;
			if (local.Substring(0, local.IndexOf('.')) != pe.Parent.Name.LocalName) return false;
			System.Type pt = _resolver.GetPropertyType(fq, prop);
			if (pt == null || pe.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))) return false;
			var children = pe.Elements().ToList();
			if (children.Count == 0) return false;
			System.Type item = _resolver.CollectionItemType(fq, prop);
			bool add = item != null && !_resolver.IsWritable(fq, prop);
			if (!add && (children.Count != 1 || !_resolver.IsWritable(fq, prop))) return false;
			foreach (XElement c in children)
			{
				string cfq = ResolveType(c);
				if (cfq == null || IsDeferredPropertyChild(c.Name.LocalName) || !_resolver.IsInstantiable(cfq)
					|| !IsElementObject(cfq) || (IsMediaObjectType(cfq) && !MediaGraphEmittable(c, cfq))
					|| !_resolver.IsAssignable(cfq, add ? item : pt)) return false;
			}
			foreach (XElement c in children)
			{
				string cfq = ResolveType(c);
				string v = $"__e{counter++}";
				EmitElement(w, c, xNs, v, cfq, ref counter);
				if (add) w.Line($"{parentVar}.{prop}.Add({v});");
				else w.Line(IsKnownDp(fq, prop) ? $"{parentVar}.SetValue({fq}.{prop}Property, {v});"
					: $"{parentVar}.{prop} = {v};");
			}
			return true;
		}
	}
}
