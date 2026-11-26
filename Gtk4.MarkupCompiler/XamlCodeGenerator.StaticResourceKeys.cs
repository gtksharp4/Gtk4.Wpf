using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		static readonly Regex _staticResourceRef = new Regex(
			@"\{\s*StaticResource\s+(?:ResourceKey\s*=\s*)?([^\s,}{]+)|<\s*(?:\w+:)?StaticResource\b[^>]*?\bResourceKey\s*=\s*""([^""{]+)""",
			RegexOptions.CultureInvariant);

		/// <summary>
		/// Keys named by a StaticResource anywhere in the project's XAML, set by the task. When null, only the
		/// document being generated is searched.
		/// </summary>
		public ISet<string> ProjectStaticResourceKeys { get; set; }

		/// <summary>Adds the keys a XAML text references through {StaticResource k} or &lt;StaticResource ResourceKey="k"/&gt;.</summary>
		public static void CollectStaticResourceKeys(string xamlText, ISet<string> keys)
		{
			foreach (Match m in _staticResourceRef.Matches(xamlText))
				keys.Add((m.Groups[1].Success ? m.Groups[1] : m.Groups[2]).Value);
		}
	}
}
