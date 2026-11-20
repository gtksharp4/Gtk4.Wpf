using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// False when the XAML has no code-behind: the generated partial then declares the root
		/// element type as its base and (for non-Application roots) a constructor, so it compiles alone.
		/// </summary>
		public bool HasCodeBehind { get; set; } = true;

		/// <summary>
		/// Source text of the project's C# files. When set, code-behind presence is decided by a
		/// <c>partial class</c> declaration of the x:Class there, instead of <see cref="HasCodeBehind"/>.
		/// </summary>
		public System.Collections.Generic.IReadOnlyList<string> CodeBehindSources { get; set; }

		bool HasCodeBehindFor(string className)
		{
			if (CodeBehindSources == null) return HasCodeBehind;
			string name = System.Text.RegularExpressions.Regex.Escape(
				className.Substring(className.LastIndexOf('.') + 1));
			var re = new System.Text.RegularExpressions.Regex(@"\bpartial\s+class\s+" + name + @"\b");
			foreach (string s in CodeBehindSources)
				if (re.IsMatch(s)) return true;
			return false;
		}

		/// <summary>
		/// The base type to declare on the generated partial: the root element type when the x:Class has no
		/// code-behind, else <c>null</c> (the code-behind supplies it).
		/// </summary>
		string RootBaseFor(XElement root, bool hasClassAttr, string className)
		{
			if (hasClassAttr && !HasCodeBehindFor(className) && ResolveType(root) is string rootFq)
				return "global::" + rootFq;
			return null;
		}

		static string ClassDeclaration(string typeName, string implicitBase, string rootBase)
			=> implicitBase != null
				? $"internal sealed partial class {typeName} : {implicitBase} {{"
				: rootBase != null
					? $"public partial class {typeName} : {rootBase} {{"
					: $"public partial class {typeName} {{";
	}
}
