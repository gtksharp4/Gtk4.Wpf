using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string ApplicationType = "System.Windows.Application";

		static readonly Regex MainMethod = new Regex(
			@"\bstatic\s+(?:async\s+)?(?:void|int|(?:global::)?(?:System\.Threading\.Tasks\.)?Task(?:\s*<\s*int\s*>)?)"
			+ @"\s+Main\s*\(");

		// The CLR type named by a clr-namespace root element ("global::Ns.Type"), else null.
		static string UserRootType(XElement root)
		{
			const string clr = "clr-namespace:";
			string ns = root.Name.NamespaceName;
			if (!ns.StartsWith(clr, System.StringComparison.Ordinal)) return null;
			string clrNs = ns.Substring(clr.Length).Split(';')[0].Trim();
			return clrNs.Length == 0 ? null : "global::" + clrNs + "." + root.Name.LocalName;
		}

		/// <summary>
		/// True for an <c>Application</c> root, or a user type deriving from it (found in a reference or declared
		/// in the code-behind sources): WPF generates <c>Main</c> for such an application definition.
		/// </summary>
		bool IsApplicationRoot(XElement root)
		{
			if (root.Name.LocalName == "Application") return true;
			string user = UserRootType(root);
			return user != null && (_resolver.DerivesFrom(user.Substring("global::".Length), ApplicationType)
				|| SourcesDeriveFromApplication(root.Name.LocalName));
		}

		bool SourcesDeriveFromApplication(string typeName)
		{
			if (CodeBehindSources == null) return false;
			var re = new Regex(@"\bclass\s+" + Regex.Escape(typeName)
				+ @"\s*:\s*(?:global::)?(?:System\.Windows\.)?Application\b");
			foreach (string s in CodeBehindSources)
				if (re.IsMatch(StripCommentsAndStrings(s))) return true;
			return false;
		}

		/// <summary>
		/// The MSBuild item type of the XAML being generated (<c>Page</c>, <c>ApplicationDefinition</c>). Only an
		/// <c>ApplicationDefinition</c> gets <c>Main</c>; <c>null</c> (direct callers) treats an Application root as
		/// one.
		/// </summary>
		public string ItemType { get; set; }

		bool ShouldEmitMain(bool isAppDef, string className) =>
			isAppDef && (ItemType == null || ItemType == "ApplicationDefinition")
			&& !ClassDeclaresMain(className.Substring(className.LastIndexOf('.') + 1));

		// True when a source declares Main inside the class itself (else it would be CS0111); an unrelated
		// class's Main is a different type's entry point.
		bool ClassDeclaresMain(string typeName)
		{
			if (CodeBehindSources == null) return false;
			var decl = new Regex(@"\bclass\s+" + Regex.Escape(typeName) + @"\b");
			foreach (string s in CodeBehindSources)
			{
				string code = StripCommentsAndStrings(s);
				foreach (Match m in decl.Matches(code))
				{
					int open = code.IndexOf('{', m.Index);
					if (open < 0) continue;
					int depth = 0, i = open;
					for (; i < code.Length; i++)
					{
						if (code[i] == '{') depth++;
						else if (code[i] == '}' && --depth == 0) break;
					}
					if (MainMethod.IsMatch(code.Substring(open, i - open))) return true;
				}
			}
			return false;
		}
	}
}
