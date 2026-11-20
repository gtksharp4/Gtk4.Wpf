using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
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

		/// <summary>True when the project's sources declare an entry point, so <c>Main</c> is not added.</summary>
		bool SourcesDeclareMain()
		{
			if (CodeBehindSources == null) return false;
			foreach (string s in CodeBehindSources)
				if (MainMethod.IsMatch(StripCommentsAndStrings(s))) return true;
			return false;
		}
	}
}
