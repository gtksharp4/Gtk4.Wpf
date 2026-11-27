using System.Diagnostics;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>
/// The packaged targets stop the build of a legacy project with the converter's message. The probe is a real
/// non-SDK file (ToolsVersion, no Sdk) with a minimal Build target, so MSBuild can evaluate it; the shipped targets
/// run the real converter from a package-shaped <c>tools/</c> folder.
/// </summary>
public class LegacyProjectTargetTests : IDisposable
{
	readonly string _dir = Path.Combine(Path.GetTempPath(), "gtkforms-legacytarget-" + Guid.NewGuid().ToString("N"));

	public void Dispose()
	{
		try { Directory.Delete(_dir, true); } catch (IOException) { }
	}

	static string RepoFile(string relative)
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Gtk4.Wpf.sln")))
			dir = dir.Parent;
		Assert.NotNull(dir);
		return Path.Combine(dir.FullName, relative);
	}

	// Lays out <dir>/pkg/build and <dir>/pkg/tools the way the package does.
	string Package()
	{
		string build = Path.Combine(_dir, "pkg", "build");
		string tools = Path.Combine(_dir, "pkg", "tools");
		Directory.CreateDirectory(build);
		Directory.CreateDirectory(tools);
		File.Copy(RepoFile("Gtk4.Wpf/Gtk4.Wpf.props"), Path.Combine(build, "Gtk4.Wpf.props"));
		File.Copy(RepoFile("Gtk4.Wpf/Gtk4.Wpf.targets"), Path.Combine(build, "Gtk4.Wpf.targets"));
		foreach (string file in Directory.GetFiles(AppContext.BaseDirectory, "Gtk4.Wpf.ProjectConverter.*"))
			File.Copy(file, Path.Combine(tools, Path.GetFileName(file)));
		return build;
	}

	const string Legacy =
		"""
		<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
		  <PropertyGroup>
		    <OutputType>Library</OutputType>
		    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
		  </PropertyGroup>
		  <Target Name="BeforeBuild" />
		  <Target Name="Build" DependsOnTargets="BeforeBuild" />
		  <Import Project="@BUILD@\Gtk4.Wpf.props" />
		  <Import Project="@BUILD@\Gtk4.Wpf.targets" />
		</Project>
		""";

	(int Exit, string Output, string Text) Build(string template)
	{
		string text = template.Replace("@BUILD@", Package());
		string project = Path.Combine(_dir, "Legacy.csproj");
		File.WriteAllText(project, text);
		var info = new ProcessStartInfo("dotnet", $"build \"{project}\" -nodeReuse:false")
		{
			WorkingDirectory = _dir,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		using var process = Process.Start(info)!;
		string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
		process.WaitForExit();
		return (process.ExitCode, output, text);
	}

	[Fact]
	public void The_build_of_a_legacy_project_stops_with_the_actionable_message()
	{
		var (exit, output, _) = Build(Legacy);

		Assert.NotEqual(0, exit);
		Assert.Contains("Gtk4.Wpf cannot convert 'Legacy': it is a legacy (non-SDK-style) project", output);
		Assert.Contains("Microsoft.NET.Sdk", output);
		Assert.DoesNotContain("conversion failed", output);
		Assert.DoesNotContain("from WPF to the Gtk4.Wpf compatibility layer", output);
	}

	[Fact]
	public void The_legacy_project_file_is_not_modified()
	{
		var (_, _, text) = Build(Legacy);

		Assert.Equal(text, File.ReadAllText(Path.Combine(_dir, "Legacy.csproj")));
	}

	[Fact]
	public void SkipGtkFormsConvert_still_opts_out()
	{
		var (exit, output, _) = Build(Legacy.Replace("<OutputType>",
			"<SkipGtkFormsConvert>true</SkipGtkFormsConvert><OutputType>"));

		Assert.Equal(0, exit);
		Assert.DoesNotContain("legacy (non-SDK-style)", output);
	}
}
