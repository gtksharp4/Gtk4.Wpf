using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// For a <c>Page</c>/<c>Window</c>/<c>ResourceDictionary</c> root without <c>x:Class</c>: the class name derived from the
		/// file name in the root namespace, and the base type to emit. <c>null</c> otherwise.
		/// </summary>
		string ImplicitClass(XElement root, string xamlPath, string rootNamespace, out string baseType)
		{
			baseType = null;
			if (string.IsNullOrEmpty(xamlPath)) return null;
			string fq = ResolveType(root);
			bool app = fq == ApplicationType && ItemType == "ApplicationDefinition";
			if (fq != "System.Windows.Controls.Page" && fq != "System.Windows.Window" && fq != DictionaryType && !app)
				return null;

			// Backslash is not a separator on Unix, so normalise first: the class name must not depend on the OS.
			string normalized = xamlPath.Replace('\\', '/');
			// Dictionaries are looked up by path, so same-named files in different folders need distinct classes.
			string stem = fq == DictionaryType
				? normalized.Substring(0, normalized.Length - Path.GetExtension(normalized).Length).TrimStart('/')
				: Path.GetFileNameWithoutExtension(normalized);
			var sb = new StringBuilder();
			if (fq == DictionaryType) sb.Append(InjectiveIdentifier(stem));
			else
				foreach (char c in stem)
					sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
			if (sb.Length == 0) return null;
			if (char.IsDigit(sb[0])) sb.Insert(0, '_');

			baseType = "global::" + fq;
			return string.IsNullOrEmpty(rootNamespace) ? sb.ToString() : rootNamespace + "." + sb;
		}

		// Injective, so A_B/C and A/B_C get different classes: '_' doubles, '/' becomes _S_, other
		// non-identifier characters become _xHHHH_.
		static string InjectiveIdentifier(string stem)
		{
			var sb = new StringBuilder();
			foreach (char c in stem)
			{
				if (c == '_') sb.Append("__");
				else if (c == '/') sb.Append("_S_");
				else if (char.IsLetterOrDigit(c)) sb.Append(c);
				else sb.Append("_x").Append(((int)c).ToString("X4")).Append('_');
			}
			return sb.ToString();
		}

		// A hand-written class owns the derived name: keep the XAML's own class under a name that cannot clash.
		static string InternalClassName(string className, string xamlPath)
		{
			var sb = new StringBuilder("__Xaml_");
			string path = xamlPath.Substring(0, xamlPath.Length - System.IO.Path.GetExtension(xamlPath).Length);
			foreach (char c in path)
				sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
			int dot = className.LastIndexOf('.');
			return dot < 0 ? sb.ToString() : className.Substring(0, dot + 1) + sb;
		}

		// An x:Class dictionary is registered too, so Source/merged lookups find it; it needs a public parameterless
		// constructor for the registry to create it.
		bool RegistersDictionary(XElement root, XAttribute classAttr, string xamlPath, string className)
		{
			if (classAttr == null || string.IsNullOrEmpty(xamlPath) || !IsDictionaryRoot(root)) return false;
			if (HasPublicDefaultConstructor(className)) return true;
			Note(xamlPath + ": ResourceDictionary " + className
				+ " has no public parameterless constructor; it is not registered for Source/merge lookup.");
			return false;
		}

		bool HasPublicDefaultConstructor(string className) => DeclaredConstructors(className) != Constructors.Other;

		enum Constructors { None, PublicDefault, Other }

		// A parenthesised group whose inner parentheses balance (default values, attribute arguments).
		const string BalancedParens = @"\((?>[^()]+|\((?<d>)|\)(?<-d>))*(?(d)(?!))\)";

		// Scans the code-behind for instance constructors; a declaration without a modifier is private, so Other.
		Constructors DeclaredConstructors(string className)
		{
			if (CodeBehindSources == null || !HasCodeBehindFor(className)) return Constructors.None;
			string name = Regex.Escape(className.Substring(className.LastIndexOf('.') + 1));
			var declared = new Regex(@"(?<!\bnew\s+)(?<!\bstatic\s+)(?<![\w.])" + name
				+ @"\s*" + BalancedParens + @"\s*(?::\s*(?:base|this)\s*" + BalancedParens + @"\s*)?\{");
			var open = new Regex(@"\bpublic\s+" + name + @"\s*\(\s*\)");
			bool any = false;
			foreach (string s in CodeBehindSources)
			{
				string code = StripCommentsAndStrings(s);
				if (open.IsMatch(code)) return Constructors.PublicDefault;
				any |= declared.IsMatch(code);
			}
			return any ? Constructors.Other : Constructors.None;
		}

		// Without a hand-written constructor a dictionary would never run InitializeComponent and load empty.
		bool NeedsGeneratedConstructor(XElement root, string className) =>
			!HasCodeBehindFor(className)
			|| (IsDictionaryRoot(root) && CodeBehindSources != null
				&& DeclaredConstructors(className) == Constructors.None);

		/// <summary>Registers the class under its XAML path for <c>StartupUri</c> and navigation lookup.</summary>
		static void EmitRegistration(IndentedWriter w, string xamlPath, string className)
		{
			string key = xamlPath.Replace('\\', '/');
			w.Line("[assembly: global::System.Windows.Markup.GtkFormsXamlRegistration("
				+ $"\"{EscapeString(key)}\", typeof(global::{className}))]");
			w.Line();
		}
	}
}
