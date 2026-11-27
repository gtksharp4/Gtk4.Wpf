using System.Diagnostics;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>A legacy (non-SDK) csproj is refused with a distinct exit code and an actionable message.</summary>
public class LegacyProjectTests : IDisposable
{
	const string LegacyProject =
		"""
		<?xml version="1.0" encoding="utf-8"?>
		<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
		  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" />
		  <PropertyGroup>
		    <OutputType>WinExe</OutputType>
		    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
		  </PropertyGroup>
		  <ItemGroup>
		    <Reference Include="PresentationFramework" />
		  </ItemGroup>
		  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
		</Project>
		""";

	readonly string _dir = Path.Combine(Path.GetTempPath(), "gtkforms-legacy-" + Guid.NewGuid().ToString("N"));

	public void Dispose()
	{
		try { Directory.Delete(_dir, true); } catch (IOException) { }
	}

	[Fact]
	public void A_ToolsVersion_project_with_the_old_imports_is_legacy()
		=> Assert.True(LegacyProjectDetector.IsLegacy(LegacyProject));

	[Fact]
	public void TargetFrameworkVersion_alone_marks_a_project_legacy()
		=> Assert.True(LegacyProjectDetector.IsLegacy(
			"<Project><PropertyGroup><TargetFrameworkVersion>v4.8</TargetFrameworkVersion></PropertyGroup></Project>"));

	[Fact]
	public void A_Microsoft_CSharp_targets_import_alone_marks_a_project_legacy()
		=> Assert.True(LegacyProjectDetector.IsLegacy(
			"<Project><Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" /></Project>"));

	[Theory]
	[InlineData("<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0-windows</TargetFramework>"
		+ "<UseWPF>true</UseWPF></PropertyGroup></Project>")]
	[InlineData("<Project Sdk=\"Microsoft.NET.Sdk.WindowsDesktop\" ToolsVersion=\"Current\"></Project>")]
	[InlineData("<Project><Sdk Name=\"Microsoft.NET.Sdk\" /><PropertyGroup><TargetFrameworkVersion>v4.8"
		+ "</TargetFrameworkVersion></PropertyGroup></Project>")]
	[InlineData("<Project><Import Project=\"Sdk.props\" Sdk=\"Microsoft.NET.Sdk\" />"
		+ "<Import Project=\"Sdk.targets\" Sdk=\"Microsoft.NET.Sdk\" /></Project>")]
	[InlineData("<Project><PropertyGroup><Foo>1</Foo></PropertyGroup></Project>")]
	[InlineData("this is not xml")]
	public void SDK_style_projects_and_plain_props_files_are_not_legacy(string content)
		=> Assert.False(LegacyProjectDetector.IsLegacy(content));

	[Fact]
	public void The_message_names_the_project_and_says_how_to_fix_it()
	{
		string message = LegacyProjectDetector.Message("C:\\x\\App.csproj");

		Assert.Contains("C:\\x\\App.csproj", message);
		Assert.Contains("legacy (non-SDK-style)", message);
		Assert.Contains("Microsoft.NET.Sdk", message);
		Assert.Contains("not changed", message);
	}

	static (int Exit, string Output) RunConverter(string project)
	{
		var info = new ProcessStartInfo("dotnet",
			$"\"{Path.Combine(AppContext.BaseDirectory, "Gtk4.Wpf.ProjectConverter.dll")}\" --convert-project \"{project}\"")
		{
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		using var process = Process.Start(info)!;
		string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
		process.WaitForExit();
		return (process.ExitCode, output);
	}

	[Fact]
	public void The_tool_refuses_a_legacy_project_with_exit_code_20_and_leaves_it_untouched()
	{
		Directory.CreateDirectory(_dir);
		string project = Path.Combine(_dir, "Legacy.csproj");
		File.WriteAllText(project, LegacyProject);

		var (exit, output) = RunConverter(project);

		Assert.Equal(20, exit);
		Assert.Contains("legacy (non-SDK-style)", output);
		Assert.DoesNotContain("Already converted", output);
		Assert.Equal(LegacyProject, File.ReadAllText(project));
	}

	[Fact]
	public void The_tool_still_converts_an_SDK_wpf_project_with_exit_code_10()
	{
		Directory.CreateDirectory(_dir);
		string project = Path.Combine(_dir, "Wpf.csproj");
		File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup>"
			+ "<TargetFramework>net10.0-windows</TargetFramework><UseWPF>true</UseWPF></PropertyGroup></Project>");

		Assert.Equal(10, RunConverter(project).Exit);
		Assert.Equal(0, RunConverter(project).Exit);
	}
}
