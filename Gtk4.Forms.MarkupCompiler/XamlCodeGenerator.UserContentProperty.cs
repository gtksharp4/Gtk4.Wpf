using System.Text.RegularExpressions;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// Content property of a type declared in the project being compiled: its own [ContentProperty] (string or
		// nameof form), else the one the declared base class inherits (a user class or a referenced type).
		ContentInfo UserContentInfo(string userType, int depth = 0)
		{
			if (depth > 8 || CodeBehindSources == null) return null;
			int dot = userType.LastIndexOf('.');
			string ns = dot >= 0 ? userType.Substring(0, dot) : string.Empty;
			string cls = Regex.Escape(userType.Substring(dot + 1));
			var own = new Regex(@"\[\s*(?:[\w.]*\.)?ContentProperty(?:Attribute)?\s*\(\s*(?:""(?<s>\w+)""|nameof\s*\("
				+ @"\s*(?:\w+\s*\.\s*)*(?<n>\w+)\s*\))\s*\)\s*\](?:\s|\[[^\]]*\]|public|internal|sealed|abstract|partial"
				+ @"|static)*class\s+" + cls + @"\b");
			var baseRe = new Regex(@"\bclass\s+" + cls + @"\b(?:\s*<[^>]*>)?\s*:\s*(?:global::)?([\w.]+)");
			foreach (string source in CodeBehindSources)
			{
				string code = StripCommentsAndStrings(source);
				Match decl = Regex.Match(code, @"\bclass\s+" + cls + @"\b");
				if (!decl.Success || NamespaceAt(code, decl.Index) != ns) continue;
				Match m = own.Match(source);
				if (m.Success)
				{
					string prop = m.Groups["s"].Success ? m.Groups["s"].Value : m.Groups["n"].Value;
					return new ContentInfo { Name = prop, IsCollection = DeclaresCollectionProperty(source, prop) };
				}
				Match b = baseRe.Match(code);
				if (!b.Success) return null;
				string baseName = b.Groups[1].Value;
				string userBase = baseName.IndexOf('.') >= 0 ? baseName : ns + "." + baseName;
				if (UserClassExists(userBase)) return UserContentInfo(userBase, depth + 1);
				return GetContentInfo(baseName.IndexOf('.') >= 0 ? baseName : QualifyTypeName(baseName));
			}
			return null;
		}

		static bool DeclaresCollectionProperty(string source, string prop)
		{
			Match m = Regex.Match(source, @"\b(?:public|internal|protected)\s+(?:new\s+)?([\w.<>,\s?]+?)\s+"
				+ Regex.Escape(prop) + @"\s*(?:\{|=>)");
			return m.Success && Regex.IsMatch(m.Groups[1].Value, @"Collection|List|IEnumerable");
		}
	}
}
