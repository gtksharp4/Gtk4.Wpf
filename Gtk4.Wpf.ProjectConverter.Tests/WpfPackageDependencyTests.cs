using System.Xml.Linq;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>
/// The WindowsDesktop shared framework supplied these assemblies implicitly; once the converter strips
/// UseWPF the Gtk4.Wpf package has to flow them to the consumer.
/// </summary>
public class WpfPackageDependencyTests
{
	static XElement[] PackageReferences(string id)
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Gtk4.Wpf.sln")))
			dir = dir.Parent;
		Assert.NotNull(dir);

		var csproj = XDocument.Load(Path.Combine(dir.FullName, "Gtk4.Wpf", "Gtk4.Wpf.csproj"));
		return csproj.Descendants("PackageReference")
			.Where(e => (string?)e.Attribute("Include") == id).ToArray();
	}

	[Fact]
	public void Wpf_package_depends_on_ConfigurationManager_10x()
	{
		var reference = Assert.Single(PackageReferences("System.Configuration.ConfigurationManager"));

		Assert.StartsWith("10.", (string)reference.Attribute("Version")!);
	}

	[Fact]
	public void ConfigurationManager_dependency_reaches_consumers_on_every_target_framework()
	{
		var reference = Assert.Single(PackageReferences("System.Configuration.ConfigurationManager"));

		Assert.Null(reference.Attribute("Condition"));
		Assert.Null(reference.Parent!.Attribute("Condition"));
		Assert.Null(reference.Attribute("PrivateAssets"));
		Assert.Null(reference.Attribute("ExcludeAssets"));
	}
}
