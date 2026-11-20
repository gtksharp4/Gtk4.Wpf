using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// For a <c>Page</c>/<c>Window</c> root without <c>x:Class</c>: the class name derived from the
		/// file name in the root namespace, and the base type to emit. <c>null</c> otherwise.
		/// </summary>
		string ImplicitClass(XElement root, string xamlPath, string rootNamespace, out string baseType)
		{
			baseType = null;
			if (string.IsNullOrEmpty(xamlPath)) return null;
			string fq = ResolveType(root);
			if (fq != "System.Windows.Controls.Page" && fq != "System.Windows.Window") return null;

			string stem = Path.GetFileNameWithoutExtension(xamlPath);
			var sb = new StringBuilder();
			foreach (char c in stem)
				sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
			if (sb.Length == 0) return null;
			if (char.IsDigit(sb[0])) sb.Insert(0, '_');

			baseType = "global::" + fq;
			return string.IsNullOrEmpty(rootNamespace) ? sb.ToString() : rootNamespace + "." + sb;
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
