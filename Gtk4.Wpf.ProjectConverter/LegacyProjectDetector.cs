using System.Xml;
using System.Xml.Linq;

namespace Gtk4.Wpf.ProjectConverter;

/// <summary>
/// Recognises a legacy (non-SDK-style) project file: no <c>Sdk</c> attribute, <c>&lt;Sdk&gt;</c> element or
/// <c>Sdk</c> import, and one of the old-style markers (<c>ToolsVersion</c>, an import of
/// <c>Microsoft.CSharp.targets</c> / <c>Microsoft.VisualBasic.targets</c> / <c>Microsoft.Common.props</c>, or
/// <c>TargetFrameworkVersion</c>). Such a project targets .NET Framework and cannot be rewritten by the text
/// rules of <see cref="CsprojConverter"/>, so the converter refuses it.
/// </summary>
internal static class LegacyProjectDetector
{
	/// <summary>Exit code of the converter when it refuses a legacy project.</summary>
	internal const int RefusedExitCode = 20;

	static readonly string[] _legacyImports =
	{
		"Microsoft.CSharp.targets",
		"Microsoft.VisualBasic.targets",
		"Microsoft.Common.props",
	};

	/// <summary>True when <paramref name="content"/> is a legacy, non-SDK project file.</summary>
	internal static bool IsLegacy(string content)
	{
		XDocument document;
		try
		{
			document = XDocument.Parse(content);
		}
		catch (XmlException)
		{
			return false;
		}

		var root = document.Root;
		if (root == null || root.Name.LocalName != "Project") return false;
		if (root.Attribute("Sdk") != null) return false;

		var elements = root.DescendantsAndSelf().ToList();
		if (elements.Any(e => e.Name.LocalName == "Sdk" || (e.Name.LocalName == "Import" && e.Attribute("Sdk") != null)))
			return false;

		return root.Attribute("ToolsVersion") != null
			|| elements.Any(e => e.Name.LocalName == "TargetFrameworkVersion")
			|| elements.Any(e => e.Name.LocalName == "Import"
				&& e.Attribute("Project")?.Value is { } project
				&& _legacyImports.Any(i => project.Contains(i, StringComparison.OrdinalIgnoreCase)));
	}

	/// <summary>The actionable message printed when a legacy project is refused.</summary>
	internal static string Message(string projectPath) =>
		$"Gtk4.Wpf cannot convert '{projectPath}': it is a legacy (non-SDK-style) project "
		+ "(ToolsVersion / Microsoft.CSharp.targets import / TargetFrameworkVersion, no Sdk attribute) "
		+ "that targets .NET Framework. Convert it to an SDK-style project first "
		+ "(<Project Sdk=\"Microsoft.NET.Sdk\"> with <TargetFramework>net10.0</TargetFramework>; the .NET Upgrade "
		+ "Assistant or try-convert can do it), then build again. The project file was not changed.";
}
