using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// x:Shared="False": every lookup builds a new instance.
		static bool IsUnshared(XElement item, XNamespace xNs)
			=> string.Equals((string)item.Attribute(xNs + "Shared"), "False", StringComparison.OrdinalIgnoreCase);

		static bool ContainsStaticResource(XElement item)
			=> item.DescendantsAndSelf().Any(e => e.Attributes().Any(IsStaticResourceAttribute)
				|| e.Nodes().OfType<XText>().Any(t => t.Value.Contains("{StaticResource")));

		static bool IsStaticResourceAttribute(XAttribute a)
			=> !a.IsNamespaceDeclaration && a.Value.TrimStart().StartsWith("{StaticResource", StringComparison.Ordinal);

		// WPF builds a dictionary value (and resolves the {StaticResource} lookups inside it) on first lookup, so a
		// key that is never read never has to exist. The value's statements go into a factory that ends in the
		// value; a shape that does not end in its keyed assignment stays eager.
		bool TryEmitDeferredResource(
			IndentedWriter w, XElement item, XNamespace xNs, string ownerVar, string key, ref int counter)
		{
			var sb = new StringBuilder();
			int saved = counter;
			EmitResourceItem(new IndentedWriter(sb), item, xNs, ownerVar, key, ref counter);
			string[] lines = sb.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			string prefix = $"{ownerVar}.Resources[\"{EscapeString(key)}\"] = ";
			string last = lines.Length == 0 ? null : lines[lines.Length - 1].Trim();
			if (last == null || !last.StartsWith(prefix, StringComparison.Ordinal)
				|| !last.EndsWith(";", StringComparison.Ordinal)
				|| lines.Take(lines.Length - 1).Any(l => l.Trim().StartsWith(prefix, StringComparison.Ordinal)))
			{
				counter = saved;
				return false;
			}
			string value = last.Substring(prefix.Length, last.Length - prefix.Length - 1);
			if (value.StartsWith("\"", StringComparison.Ordinal))
			{
				counter = saved;
				return false;
			}
			bool shared = !IsUnshared(item, xNs);
			w.Line($"{ownerVar}.Resources.SetDeferred(\"{EscapeString(key)}\", () =>");
			w.Line("{");
			w.Indent++;
			foreach (string line in lines.Take(lines.Length - 1)) w.Line(line);
			w.Line($"return {value};");
			w.Indent--;
			w.Line(shared ? "});" : "}, false);");
			return true;
		}
	}
}
