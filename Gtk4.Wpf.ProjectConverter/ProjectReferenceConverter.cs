using System.Text;
using System.Text.RegularExpressions;

namespace Gtk4.Wpf.ProjectConverter;

/// <summary>
/// Converts a project together with the WPF projects it references (transitively), so that restore of the
/// converted client no longer sees a sibling still on <c>net10.0-windows7.0</c> (NU1201). Only siblings with
/// <c>UseWPF</c> or <c>UseWindowsForms</c> are converted; a plain <c>-windows</c> TFM library (no WPF, no
/// WinForms) is left untouched and keeps its Windows-only target, so retarget it by hand if the client needs it.
/// A converted sibling that has no <c>Gtk4.Wpf</c> reference gets the client's one, since it no longer
/// compiles against WPF otherwise.
/// </summary>
internal static class ProjectReferenceConverter
{
	const string PackageId = "Gtk4.Wpf";

	static readonly Regex _projectReference = new(
		@"<ProjectReference\b[^>]*?\sInclude\s*=\s*""(?<path>[^""]+)""",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	static readonly Regex _windowsProject = new(
		@"<UseWPF>\s*true\s*</UseWPF>|<UseWindowsForms>\s*true\s*</UseWindowsForms>",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	static readonly Regex _packageVersion = new(
		@"<PackageReference\b[^>]*\sInclude\s*=\s*""Gtk4.Wpf""[^>]*?\sVersion\s*=\s*""(?<v>[^""$]+)""",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	static readonly Regex _hasPackage = new(
		@"<PackageReference\b[^>]*\sInclude\s*=\s*""Gtk4.Wpf""",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	/// <summary>Converts <paramref name="csprojPath"/> and the WPF projects it references.</summary>
	/// <returns>true when any file changed.</returns>
	internal static bool ConvertTree(string csprojPath) =>
		Convert(Path.GetFullPath(csprojPath), new HashSet<string>(StringComparer.OrdinalIgnoreCase), null);

	static bool Convert(string path, HashSet<string> visited, string? packageVersion)
	{
		if (!visited.Add(path)) return false;
		var changed = CsprojConverter.TryConvertSingle(path);
		var text = File.ReadAllText(path);
		packageVersion ??= _packageVersion.Match(text) is { Success: true } m ? m.Groups["v"].Value : null;

		foreach (Match reference in _projectReference.Matches(text))
		{
			var sibling = Resolve(path, reference.Groups["path"].Value);
			if (sibling == null || visited.Contains(sibling)) continue;
			if (!_windowsProject.IsMatch(File.ReadAllText(sibling))) continue;
			changed |= Convert(sibling, visited, packageVersion);
			changed |= AddPackageReference(sibling, packageVersion);
		}
		return changed;
	}

	// The referenced csproj as an absolute path, or null; '\' separators are accepted on every OS.
	static string? Resolve(string fromProject, string include)
	{
		if (include.Contains("$(") || include.Contains("*")) return null;
		var relative = include.Replace('\\', '/');
		var full = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fromProject)!, relative));
		return full.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) && File.Exists(full) ? full : null;
	}

	static bool AddPackageReference(string path, string? version)
	{
		if (version == null) return false;
		var bytes = File.ReadAllBytes(path);
		var hadBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
		var text = File.ReadAllText(path);
		var close = text.LastIndexOf("</Project>", StringComparison.OrdinalIgnoreCase);
		if (close < 0 || _hasPackage.IsMatch(text)) return false;
		var nl = text.Contains("\r\n") ? "\r\n" : "\n";
		var item = "  <ItemGroup>" + nl
			+ "    <PackageReference Include=\"" + PackageId + "\" Version=\"" + version + "\" />" + nl
			+ "  </ItemGroup>" + nl;
		File.WriteAllText(path, text.Insert(close, item), new UTF8Encoding(hadBom));
		return true;
	}
}
