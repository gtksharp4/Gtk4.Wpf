using Gtk4.Wpf.ProjectConverter;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>A converted client also converts the WPF projects it references, before restore (NU1201).</summary>
public sealed class ProjectReferenceConversionTests : IDisposable
{
	readonly string _root = Path.Combine(Path.GetTempPath(), "gf-prjref-" + Guid.NewGuid().ToString("N"));

	public ProjectReferenceConversionTests() => Directory.CreateDirectory(_root);

	public void Dispose() => Directory.Delete(_root, recursive: true);

	string Write(string relative, string content)
	{
		var path = Path.Combine(_root, relative);
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, content);
		return path;
	}

	const string WpfApp =
		"<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0-windows</TargetFramework>\n" +
		"    <UseWpf>true</UseWpf>\n  </PropertyGroup>\n";

	static string Client(string references, string package = "") =>
		WpfApp.Replace("<UseWpf>", "<UseWPF>").Replace("</UseWpf>", "</UseWPF>") + "  <ItemGroup>\n" + package + references +
		"  </ItemGroup>\n</Project>";

	const string Package = "    <PackageReference Include=\"Gtk4.Wpf\" Version=\"1.0.0.26325\" />\n";

	static string Reference(string include) =>
		"    <ProjectReference Include=\"" + include + "\">\n      <SetTargetFramework>TargetFramework=$(TargetFramework)" +
		"</SetTargetFramework>\n    </ProjectReference>\n";

	static string WpfLibrary(string references = "") =>
		"<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0-windows</TargetFramework>\n" +
		"    <UseWPF>true</UseWPF>\n  </PropertyGroup>\n" + (references.Length == 0 ? "" : "  <ItemGroup>\n" + references + "  </ItemGroup>\n") +
		"</Project>";

	[Fact]
	public void Converts_the_referenced_WPF_sibling_with_the_client()
	{
		var client = Write("App/App.csproj", Client(Reference("..\\Lib\\Lib.csproj"), Package));
		var lib = Write("Lib/Lib.csproj", WpfLibrary());

		Assert.True(CsprojConverter.TryConvertFile(client));

		var converted = File.ReadAllText(lib);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", converted);
		Assert.DoesNotContain("UseWPF", converted);
		Assert.DoesNotContain("-windows", File.ReadAllText(client));
	}

	[Fact]
	public void Gives_the_sibling_the_clients_Gtk4_Wpf_package_reference()
	{
		var client = Write("App/App.csproj", Client(Reference("../Lib/Lib.csproj"), Package));
		var lib = Write("Lib/Lib.csproj", WpfLibrary());

		CsprojConverter.TryConvertFile(client);

		Assert.Contains("<PackageReference Include=\"Gtk4.Wpf\" Version=\"1.0.0.26325\" />", File.ReadAllText(lib));
	}

	[Fact]
	public void Conversion_is_idempotent_across_the_tree()
	{
		var client = Write("App/App.csproj", Client(Reference("..\\Lib\\Lib.csproj"), Package));
		var lib = Write("Lib/Lib.csproj", WpfLibrary());
		CsprojConverter.TryConvertFile(client);
		var clientText = File.ReadAllText(client);
		var libText = File.ReadAllText(lib);

		Assert.False(CsprojConverter.TryConvertFile(client));

		Assert.Equal(clientText, File.ReadAllText(client));
		Assert.Equal(libText, File.ReadAllText(lib));
	}

	[Fact]
	public void Converts_transitive_references_and_survives_cycles()
	{
		var client = Write("App/App.csproj", Client(Reference("..\\A\\A.csproj"), Package));
		var a = Write("A/A.csproj", WpfLibrary(Reference("..\\B\\B.csproj")));
		var b = Write("B/B.csproj", WpfLibrary(Reference("..\\A\\A.csproj")));

		Assert.True(CsprojConverter.TryConvertFile(client));

		Assert.DoesNotContain("-windows", File.ReadAllText(a));
		Assert.DoesNotContain("-windows", File.ReadAllText(b));
	}

	[Fact]
	public void Leaves_non_WPF_siblings_and_missing_references_alone()
	{
		const string plain = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n" +
			"    <TargetFramework>netstandard2.0</TargetFramework>\n  </PropertyGroup>\n</Project>";
		var client = Write("App/App.csproj",
			Client(Reference("..\\Plain\\Plain.csproj") + Reference("..\\Gone\\Gone.csproj"), Package));
		var lib = Write("Plain/Plain.csproj", plain);

		CsprojConverter.TryConvertFile(client);

		Assert.Equal(plain, File.ReadAllText(lib));
	}

	[Fact]
	public void Does_not_add_a_package_reference_when_the_client_has_no_literal_version()
	{
		var client = Write("App/App.csproj", Client(Reference("..\\Lib\\Lib.csproj")));
		var lib = Write("Lib/Lib.csproj", WpfLibrary());

		CsprojConverter.TryConvertFile(client);

		Assert.DoesNotContain("PackageReference", File.ReadAllText(lib));
		Assert.Contains("net10.0</TargetFramework>", File.ReadAllText(lib));
	}

	[Fact]
	public void Reports_a_change_when_only_the_sibling_needed_converting()
	{
		var converted = WpfApp.Replace("net10.0-windows", "net10.0").Replace("    <UseWpf>true</UseWpf>\n", "") +
			"  <ItemGroup>\n" + Package + Reference("..\\Lib\\Lib.csproj") + "  </ItemGroup>\n</Project>";
		var client = Write("App/App.csproj", converted);
		Write("Lib/Lib.csproj", WpfLibrary());

		Assert.True(CsprojConverter.TryConvertFile(client));
		Assert.Equal(converted, File.ReadAllText(client));
	}

	const string WinOnly = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n" +
		"    <TargetFramework>net8.0-windows</TargetFramework>\n  </PropertyGroup>\n</Project>";

	const string WinForms = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n" +
		"    <TargetFramework>net8.0-windows</TargetFramework>\n    <UseWindowsForms>true</UseWindowsForms>\n" +
		"  </PropertyGroup>\n</Project>";

	[Fact]
	public void Leaves_a_windows_only_sibling_without_UseWPF_or_UseWindowsForms_alone()
	{
		var client = Write("App/App.csproj", Client(Reference("../Win/Win.csproj"), Package));
		var lib = Write("Win/Win.csproj", WinOnly);

		CsprojConverter.TryConvertFile(client);

		Assert.Equal(WinOnly, File.ReadAllText(lib));
	}

	[Fact]
	public void Converts_a_UseWindowsForms_sibling()
	{
		var client = Write("App/App.csproj", Client(Reference("../Forms/Forms.csproj"), Package));
		var lib = Write("Forms/Forms.csproj", WinForms);

		CsprojConverter.TryConvertFile(client);

		Assert.Contains("Gtk4.Wpf", File.ReadAllText(lib));
	}
}
