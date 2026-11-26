using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// True when a code-behind source declares <c>class {Name}</c> in the namespace of
		/// <paramref name="className"/>. Comments and string literals do not count.
		/// </summary>
		bool UserClassExists(string className)
		{
			if (CodeBehindSources == null) return false;
			int dot = className.LastIndexOf('.');
			string ns = dot >= 0 ? className.Substring(0, dot) : string.Empty;
			var re = new Regex(@"\bclass\s+" + Regex.Escape(className.Substring(dot + 1)) + @"\b");
			foreach (string source in CodeBehindSources)
			{
				string code = StripCommentsAndStrings(source);
				foreach (Match m in re.Matches(code))
					if (NamespaceAt(code, m.Index) == ns) return true;
			}
			return false;
		}

		// The namespace enclosing a position: block namespaces by brace matching, else a file-scoped one.
		static string NamespaceAt(string code, int pos)
		{
			string fileScoped = string.Empty;
			var open = new List<(string name, int depth)>();
			int depth = 0;
			foreach (Match t in Regex.Matches(code, @"namespace\s+([\w.]+)\s*(;|\{)|\{|\}"))
			{
				if (t.Index >= pos) break;
				if (t.Groups[1].Success && t.Groups[2].Value == ";")
					fileScoped = t.Groups[1].Value;
				else if (t.Groups[1].Success)
				{
					string outer = open.Count > 0 ? open[open.Count - 1].name : fileScoped;
					open.Add((outer.Length == 0 ? t.Groups[1].Value : outer + "." + t.Groups[1].Value, ++depth));
				}
				else if (t.Value == "{") depth++;
				else
				{
					if (open.Count > 0 && open[open.Count - 1].depth == depth) open.RemoveAt(open.Count - 1);
					depth--;
				}
			}
			return open.Count > 0 ? open[open.Count - 1].name : fileScoped;
		}

		static string StripCommentsAndStrings(string s)
		{
			var sb = new StringBuilder(s.Length);
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				char n = i + 1 < s.Length ? s[i + 1] : '\0';
				if (c == '/' && n == '/')
				{
					while (i < s.Length && s[i] != '\n') i++;
					sb.Append('\n');
				}
				else if (c == '/' && n == '*')
				{
					int end = s.IndexOf("*/", i + 2, StringComparison.Ordinal);
					i = end < 0 ? s.Length : end + 1;
				}
				else if (c == '"' || c == '\'')
				{
					bool verbatim = c == '"' && i > 0 && s[i - 1] == '@';
					for (i++; i < s.Length; i++)
					{
						if (s[i] == '\\' && !verbatim) i++;
						else if (s[i] == c)
						{
							if (verbatim && i + 1 < s.Length && s[i + 1] == c) i++;
							else break;
						}
					}
				}
				else sb.Append(c);
			}
			return sb.ToString();
		}
	}
}
