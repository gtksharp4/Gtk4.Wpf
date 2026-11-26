using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// WPF's tokenizer ignores whitespace around the extension name and arguments but keeps it inside
		// quoted values. Normalising up front lets every consumer match "{StaticResource Key}" literally.
		static void NormalizeMarkupAttributes(XElement root)
		{
			foreach (XElement el in root.DescendantsAndSelf())
				foreach (XAttribute a in el.Attributes().Where(a => !a.IsNamespaceDeclaration).ToList())
				{
					string normalized = NormalizeMarkupValue(a.Value);
					if (!ReferenceEquals(normalized, a.Value)) a.Value = normalized;
				}
		}

		static bool IsDynamicResourceValue(string value, out string key)
			=> IsStaticOrDynamicResource(value, out key)
			&& value.TrimStart().StartsWith("{DynamicResource", System.StringComparison.Ordinal);

		internal static string NormalizeMarkupValue(string value)
		{
			string t = value.Trim();
			bool braced = t.Length >= 2 && t[0] == '{' && t[t.Length - 1] == '}';
			if (!braced || t.StartsWith("{}", System.StringComparison.Ordinal))
				return value;

			var sb = new StringBuilder(t.Length);
			char quote = '\0';
			bool pendingSpace = false;
			foreach (char c in t)
			{
				if (quote != '\0')
				{
					sb.Append(c);
					if (c == quote) quote = '\0';
					continue;
				}
				if (char.IsWhiteSpace(c))
				{
					pendingSpace = true;
					continue;
				}
				if (pendingSpace)
				{
					bool afterOpen = sb.Length > 0 && sb[sb.Length - 1] == '{';
					if (!afterOpen && c != '}') sb.Append(' ');
					pendingSpace = false;
				}
				if (c == '\'' || c == '"') quote = c;
				sb.Append(c);
			}
			return sb.ToString() == value ? value : sb.ToString();
		}
	}
}
