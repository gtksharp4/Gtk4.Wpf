using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		static readonly string[] _baseEventFallbackTypes =
		{
			"System.Windows.Controls.Primitives.ButtonBase", "System.Windows.Controls.Control",
			"System.Windows.FrameworkElement", "System.Windows.UIElement"
		};

		static readonly string[] _baseEventNamespaces =
		{
			"System.Windows.Controls", "System.Windows.Controls.Primitives", "System.Windows",
			"System.Windows.Documents"
		};

		// An event the code-behind class inherits from a layer base (class MyButton : Button), which the
		// resolver can inspect even though the user type itself is unknown to it.
		bool UserBaseHasEvent(string fqType, string eventName)
		{
			if (CodeBehindSources == null) return false;
			string user = fqType.StartsWith("global::", System.StringComparison.Ordinal) ? fqType.Substring(8) : fqType;
			for (int hop = 0; hop < 8; hop++)
			{
				string baseName = DeclaredBaseOf(user, out string[] usings);
				if (baseName == null) return false;
				string resolved = ResolveBaseType(baseName, usings);
				if (resolved != null) return _resolver.IsEvent(resolved, eventName);
				if (!UserClassExists(baseName.Contains(".") ? baseName : NamespaceOf(user) + "." + baseName))
					return ProbeFallbackEvent(eventName);
				user = baseName.Contains(".") ? baseName : NamespaceOf(user) + "." + baseName;
			}
			return false;
		}

		bool ProbeFallbackEvent(string eventName)
		{
			foreach (string probe in _baseEventFallbackTypes)
				if (_resolver.TypeExists(probe) && _resolver.IsEvent(probe, eventName)) return true;
			return false;
		}

		static string NamespaceOf(string fq)
		{
			int dot = fq.LastIndexOf('.');
			return dot < 0 ? string.Empty : fq.Substring(0, dot);
		}

		string ResolveBaseType(string baseName, string[] usings)
		{
			if (baseName.StartsWith("global::", System.StringComparison.Ordinal)) baseName = baseName.Substring(8);
			var candidates = new List<string> { baseName };
			foreach (string ns in usings) candidates.Add(ns + "." + baseName);
			foreach (string ns in _baseEventNamespaces) candidates.Add(ns + "." + baseName);
			foreach (string candidate in candidates)
				if (_resolver.TypeExists(candidate)) return candidate;
			return null;
		}

		// The base type named by "class Name : Base" in the code-behind, plus that file's using namespaces.
		string DeclaredBaseOf(string userType, out string[] usings)
		{
			string name = userType.Substring(userType.LastIndexOf('.') + 1);
			var re = new Regex(@"\bclass\s+" + Regex.Escape(name) + @"\b\s*(?:<[^>]*>)?\s*:\s*(?<b>[\w.:]+)");
			string ns = NamespaceOf(userType);
			foreach (string source in CodeBehindSources)
			{
				string code = StripCommentsAndStrings(source);
				foreach (Match m in re.Matches(code))
				{
					if (NamespaceAt(code, m.Index) != ns) continue;
					var list = new List<string>();
					foreach (Match u in Regex.Matches(code, @"\busing\s+([\w.]+)\s*;")) list.Add(u.Groups[1].Value);
					usings = list.ToArray();
					return m.Groups["b"].Value;
				}
			}
			usings = new string[0];
			return null;
		}
	}
}
