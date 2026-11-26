using System;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// The type of an element from a <c>clr-namespace</c> the layer cannot see, when the code-behind
		/// declares it (e.g. <c>&lt;local:MyTextBox/&gt;</c>), else <c>null</c>.
		/// </summary>
		string UserElementFqType(XElement element)
			=> UserElementType(element) is string user ? "global::" + user : null;

		string UserElementType(XElement element)
		{
			string ns = element.Name.NamespaceName;
			const string clr = "clr-namespace:";
			if (!ns.StartsWith(clr, StringComparison.Ordinal)) return null;
			string clrNs = ns.Substring(clr.Length);
			int semi = clrNs.IndexOf(';');
			if (semi >= 0) clrNs = clrNs.Substring(0, semi);
			clrNs = clrNs.Trim();
			if (clrNs.Length == 0 || clrNs == "System" || clrNs.StartsWith("System.", StringComparison.Ordinal))
				return null;
			string user = clrNs + "." + element.Name.LocalName;
			return _resolver.TypeExists(user) || UserClassExists(user) ? user : null;
		}

		// A user element whose type the resolver cannot inspect takes literal attributes through the runtime hook.
		bool IsHookedUserType(string fqType)
			=> fqType.StartsWith("global::", StringComparison.Ordinal)
				&& !_resolver.TypeExists(fqType.Substring("global::".Length));

		// An event declared by a code-behind class of a type the resolver cannot inspect.
		bool UserEventDeclared(string name)
		{
			if (CodeBehindSources == null) return false;
			var re = new System.Text.RegularExpressions.Regex(@"\bevent\s+[\w.<>,\s]+?\s" + name + @"\b\s*[;{]");
			foreach (string source in CodeBehindSources)
				if (re.IsMatch(StripCommentsAndStrings(source))) return true;
			return false;
		}

		void EmitUserNameProperty(IndentedWriter w, string target, string name)
		{
			string literal = "\"" + name.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
			w.Line($"if ((object){target} is global::System.Windows.FrameworkElement {target}_fe) " +
				$"{target}_fe.Name = {literal};");
		}
	}
}
