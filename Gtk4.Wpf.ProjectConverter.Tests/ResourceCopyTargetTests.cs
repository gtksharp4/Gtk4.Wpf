using System.Diagnostics;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>Builds a throwaway project that imports the shipped targets and checks the &lt;Resource&gt; copy.</summary>
public class ResourceCopyTargetTests : IDisposable
{
	readonly string _dir = Path.Combine(Path.GetTempPath(), "gtkforms-rescopy-" + Guid.NewGuid().ToString("N"));

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

	string Build(string extraProperties, string target = "Build")
	{
		Directory.CreateDirectory(Path.Combine(_dir, "Images"));
		File.WriteAllBytes(Path.Combine(_dir, "Images", "a.png"), new byte[] { 1, 2, 3 });
		File.WriteAllText(Path.Combine(_dir, "Probe.csproj"), $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Library</OutputType>
    <EnableDefaultItems>false</EnableDefaultItems>
    {extraProperties}
  </PropertyGroup>
  <ItemGroup><Resource Include=""Images\a.png"" /></ItemGroup>
  <Import Project=""{RepoFile("Gtk4.Wpf/Gtk4.Wpf.props")}"" />
  <Import Project=""{RepoFile("Gtk4.Wpf/Gtk4.Wpf.targets")}"" />
</Project>");
		var info = new ProcessStartInfo("dotnet", $"build \"{Path.Combine(_dir, "Probe.csproj")}\" -nodeReuse:false -t:{target}")
		{
			WorkingDirectory = _dir,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		using var process = Process.Start(info)!;
		string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
		process.WaitForExit();
		Assert.True(process.ExitCode == 0, output);
		return Path.Combine(_dir, "bin", "Debug", "net10.0", "Images", "a.png");
	}

	[Fact]
	public void Resource_files_are_copied_when_conversion_is_skipped()
	{
		string copy = Build("<SkipGtkFormsConvert>true</SkipGtkFormsConvert>");
		Assert.True(File.Exists(copy), copy);
	}

	[Fact]
	public void SkipGtkFormsCopyResources_opts_out_of_the_copy()
	{
		string copy = Build("<SkipGtkFormsConvert>true</SkipGtkFormsConvert>"
			+ "<SkipGtkFormsCopyResources>true</SkipGtkFormsCopyResources>");
		Assert.False(File.Exists(copy), copy);
	}

	[Fact]
	public void Clean_removes_the_copied_resources()
	{
		string copy = Build("<SkipGtkFormsConvert>true</SkipGtkFormsConvert>");
		Assert.True(File.Exists(copy));
		Build("<SkipGtkFormsConvert>true</SkipGtkFormsConvert>", "Clean");
		Assert.False(File.Exists(copy), copy);
	}
}
