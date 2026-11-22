using System;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// <StaticResource ResourceKey="K"/> as an item of a media property element: resolved once, typed by the
		// property (or its collection item).
		bool TryEmitResourceElementValue(
			IndentedWriter w, XElement child, string parentVar, string prop, Type target, bool collection)
		{
			string key = (string)child.Attribute("ResourceKey");
			if (child.Name.LocalName != "StaticResource" || key == null || target == null || target.IsGenericType
				|| target.FullName == null)
				return false;
			string type = "global::" + target.FullName.Replace('+', '.');
			string value = $"({type}){StaticResourceLookup("this", EscapeString(key))}";
			w.Line(collection ? $"{parentVar}.{prop}.Add({value});" : $"{parentVar}.{prop} = {value};");
			return true;
		}
	}
}
