using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// <Path.Data><StaticResource ResourceKey="K"/></Path.Data>: the element form of {StaticResource K} on a
		// writable CLR property of an object type (a geometry, a brush); the looked-up value is cast to it.
		bool TryEmitStaticResourceElement(IndentedWriter w, XElement pe, string parentVar, string prop)
		{
			string fq = pe.Parent == null ? null : ResolveType(pe.Parent);
			if (fq == null || !_resolver.HasReferences || pe.Elements().Count() != 1
				|| pe.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))) return false;
			XElement child = pe.Elements().First();
			string key = (string)child.Attribute("ResourceKey");
			if (child.Name.LocalName != "StaticResource" || string.IsNullOrWhiteSpace(key) || key.IndexOf('{') >= 0)
				return false;
			System.Type pt = _resolver.GetPropertyType(fq, prop);
			if (pt == null || !_resolver.IsWritable(fq, prop) || pt.IsValueType || pt == typeof(object)) return false;
			w.Line($"{parentVar}.{prop} = ({"global::" + pt.FullName.Replace('+', '.')})" +
				StaticResourceLookup(parentVar, EscapeString(key.Trim())) + ";");
			return true;
		}
	}
}
