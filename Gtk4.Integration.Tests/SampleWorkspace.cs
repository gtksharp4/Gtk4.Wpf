using System;
using System.IO;

namespace Gtk4.Integration.Tests;

/// <summary>
/// Materialises the embedded WPF sample solution into a temporary directory and wires it to
/// build against the locally-built <c>Gtk4.Wpf</c> package.
/// </summary>
sealed class SampleWorkspace : IDisposable
{
	// Embedded resource (LogicalName) -> path relative to the workspace root.
	static readonly (string Resource, string RelativePath)[] Files =
	{
		("WpfAppToConvert.slnx",   "WpfAppToConvert.slnx"),
		("WpfAppToConvert.csproj", "WpfAppToConvert/WpfAppToConvert.csproj"),
		("App.xaml",               "WpfAppToConvert/App.xaml"),
		("App.xaml.cs",            "WpfAppToConvert/App.xaml.cs"),
		("MainWindow.xaml",        "WpfAppToConvert/MainWindow.xaml"),
		("MainWindow.xaml.cs",     "WpfAppToConvert/MainWindow.xaml.cs"),
		("AssemblyInfo.cs",        "WpfAppToConvert/AssemblyInfo.cs"),
	};

	public string Root { get; }
	public string SolutionPath => Path.Combine(Root, "WpfAppToConvert.slnx");
	public string ProjectPath => Path.Combine(Root, "WpfAppToConvert", "WpfAppToConvert.csproj");

	SampleWorkspace(string root) => Root = root;

	/// <summary>Writes every embedded sample file into a fresh temporary directory.</summary>
	public static SampleWorkspace Create()
	{
		string root = Path.Combine(Path.GetTempPath(), "GtkFormsIntegration_" + Guid.NewGuid().ToString("N"));
		var asm = typeof(SampleWorkspace).Assembly;

		foreach (var (resource, relative) in Files)
		{
			string target = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
			Directory.CreateDirectory(Path.GetDirectoryName(target)!);

			using Stream s = asm.GetManifestResourceStream(resource)
			                 ?? throw new InvalidOperationException($"Embedded resource '{resource}' not found.");
			using var file = File.Create(target);
			s.CopyTo(file);
		}

		return new SampleWorkspace(root);
	}

	/// <summary>
	/// Adds the <c>Gtk4.Wpf</c> PackageReference (this is what converts the WPF project
	/// on build) and writes a nuget.config that resolves it from the local feed.
	/// </summary>
	public void UseGtkFormsWpf(string packagesDir, string version)
	{
		string csproj = File.ReadAllText(ProjectPath);
		string itemGroup =
			"  <ItemGroup>\n" +
			$"    <PackageReference Include=\"Gtk4.Wpf\" Version=\"{version}\" />\n" +
			"  </ItemGroup>\n\n";
		File.WriteAllText(ProjectPath, csproj.Replace("</Project>", itemGroup + "</Project>"));

		File.WriteAllText(Path.Combine(Root, "nuget.config"),
			"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
			"<configuration>\n" +
			"  <packageSources>\n" +
			"    <clear />\n" +
			$"    <add key=\"LocalGtkForms\" value=\"{packagesDir}\" />\n" +
			"    <add key=\"nuget.org\" value=\"https://api.nuget.org/v3/index.json\" protocolVersion=\"3\" />\n" +
			"  </packageSources>\n" +
			"</configuration>\n");
	}

	public void Dispose()
	{
		try { if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true); }
		catch { /* best-effort temp cleanup; a running/locked file must not fail the test */ }
	}
}