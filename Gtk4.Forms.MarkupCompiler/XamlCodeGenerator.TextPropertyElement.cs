using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// <TabItem.Header>Add Control</TabItem.Header>, <TextBlock.Text>hi</TextBlock.Text>: a text-only property
		// element of the element's own property. Whitespace collapses and trims as XAML does for such text; the value
		// goes through the same conversion as the attribute form.
		bool TryEmitTextPropertyElement(IndentedWriter w, XElement pe, string parentVar, string prop)
		{
			string fq = pe.Parent == null ? null : ResolveType(pe.Parent);
			if (fq == null || !_resolver.HasReferences || IsHookedUserType(fq) || pe.Elements().Any()) return false;
			string local = pe.Name.LocalName;
			if (local.Substring(0, local.IndexOf('.')) != pe.Parent.Name.LocalName) return false;
			string text = Regex.Replace(pe.Value, @"\s+", " ").Trim();
			System.Type pt = _resolver.GetPropertyType(fq, prop);
			if (text.Length == 0 || pt == null || !_resolver.IsWritable(fq, prop) || text.StartsWith("{")) return false;
			bool dp = IsKnownDp(fq, prop);
			if (!dp && pt != typeof(string) && pt != typeof(object)) return false;
			w.Line(dp
				? $"{parentVar}.SetValue({fq}.{prop}Property, {DpValueExpression(fq, prop, text)});"
				: $"{parentVar}.{prop} = \"{EscapeString(text)}\";");
			return true;
		}
	}
}
