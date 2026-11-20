using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Element content of <paramref name="parent"/> in document order: child elements
		/// (property elements excluded) and processed text strings. Text follows the XAML
		/// whitespace rules unless <c>xml:space="preserve"</c> is in effect.
		/// </summary>
		static List<object> CollectContent(XElement parent, bool keepInnerSpace)
		{
			bool preserve = IsSpacePreserved(parent);
			var raw = new List<object>();
			var text = new StringBuilder();
			bool inText = false;
			void Flush()
			{
				if (inText) raw.Add(text.ToString());
				text.Clear();
				inText = false;
			}
			foreach (XNode node in parent.Nodes())
			{
				if (node is XText t)
				{
					text.Append(t.Value);
					inText = true;
				}
				else if (node is XElement e)
				{
					Flush();
					raw.Add(e);
				}
			}
			Flush();

			var result = new List<object>();
			for (int i = 0; i < raw.Count; i++)
			{
				if (!(raw[i] is string s)) { result.Add(raw[i]); continue; }
				if (!preserve)
				{
					s = CollapseWhitespace(s);
					if (!HasContentBefore(raw, i)) s = s.TrimStart(' ');
					if (!HasContentAfter(raw, i)) s = s.TrimEnd(' ');
					if (s == " " && !keepInnerSpace) s = string.Empty;
				}
				if (s.Length > 0) result.Add(s);
			}
			return result;
		}

		static bool IsContentElement(object o) => o is XElement e && e.Name.LocalName.IndexOf('.') < 0;

		static bool HasContentBefore(List<object> items, int index)
		{
			for (int i = 0; i < index; i++)
				if (IsContentElement(items[i])) return true;
			return false;
		}

		static bool HasContentAfter(List<object> items, int index)
		{
			for (int i = index + 1; i < items.Count; i++)
				if (IsContentElement(items[i])) return true;
			return false;
		}

		static bool IsSpacePreserved(XElement el)
		{
			for (XElement e = el; e != null; e = e.Parent)
			{
				string v = (string)e.Attribute(XNamespace.Xml + "space");
				if (v != null) return v == "preserve";
			}
			return false;
		}

		static string CollapseWhitespace(string s)
		{
			var sb = new StringBuilder(s.Length);
			bool space = false;
			foreach (char c in s)
			{
				if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
				{
					space = true;
					continue;
				}
				if (space) sb.Append(' ');
				space = false;
				sb.Append(c);
			}
			if (space) sb.Append(' ');
			return sb.ToString();
		}
	}
}
