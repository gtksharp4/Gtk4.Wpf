using System;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// WPF CommandConverter: "prefix:Type.Member" reads the public static Member (or Member + "Command") of the
		// type the prefix maps to. Resolved at compile time into a direct static reference.
		bool TryPrefixedCommand(string fqType, string propName, string val, out string expr)
		{
			expr = null;
			int colon = val.IndexOf(':');
			int dot = val.IndexOf('.');
			if (colon <= 0 || dot < colon || val.IndexOf('{') >= 0 || !_resolver.HasReferences) return false;
			Type pt = _resolver.GetPropertyType(fqType, propName);
			if (pt == null || pt.FullName != "System.Windows.Input.ICommand") return false;
			string prefix = val.Substring(0, colon).Trim();
			if (!_prefixMap.TryGetValue(prefix, out string clrNs) || string.IsNullOrEmpty(clrNs)) return false;
			string typeName = val.Substring(colon + 1, dot - colon - 1).Trim();
			string member = val.Substring(dot + 1).Trim();
			string owner = clrNs + "." + typeName;
			foreach (string candidate in new[] { member, member + "Command" })
				if (_resolver.HasStaticMember(owner, candidate))
				{
					expr = "global::" + owner + "." + candidate;
					return true;
				}
			return TryProjectStaticCommand(owner, member, out expr);
		}

		// The owner is a project type the resolver cannot see: look for its static member in the code-behind.
		bool TryProjectStaticCommand(string owner, string member, out string expr)
		{
			expr = null;
			if (_resolver.TypeExists(owner) || !UserClassExists(owner)) return false;
			foreach (string candidate in new[] { member, member + "Command" })
			{
				var re = new System.Text.RegularExpressions.Regex(
					@"\bstatic\b[^;{(=]*\b" + System.Text.RegularExpressions.Regex.Escape(candidate) + @"\b\s*(=|;|\{)");
				foreach (string source in CodeBehindSources)
					if (re.IsMatch(StripCommentsAndStrings(source)))
					{
						expr = "global::" + owner + "." + candidate;
						return true;
					}
			}
			return false;
		}
	}
}
