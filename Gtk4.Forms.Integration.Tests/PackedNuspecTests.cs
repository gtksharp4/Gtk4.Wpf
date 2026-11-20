using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Gtk4.Forms.Integration.Tests;

/// <summary>Inspects the packed <c>Gtk4.Forms.Wpf</c> nuspec in the repository feed.</summary>
public class PackedNuspecTests
{
	[Fact]
	public void Packed_nuspec_declares_ConfigurationManager_in_every_dependency_group()
	{
		string? packagesDir = PackageFeed.FindPackagesDirectory();
		if (packagesDir == null)
			Assert.Skip("Repository Packages folder not found.");
		string? version = PackageFeed.LatestVersion(packagesDir, "Gtk4.Forms.Wpf");
		if (version == null)
			Assert.Skip("Gtk4.Forms.Wpf package not in the Packages folder; build the solution first.");

		using var zip = ZipFile.OpenRead(Path.Combine(packagesDir, $"Gtk4.Forms.Wpf.{version}.nupkg"));
		using var stream = zip.Entries.Single(e => e.FullName.EndsWith(".nuspec")).Open();
		var groups = XDocument.Load(stream).Descendants().Where(e => e.Name.LocalName == "group").ToList();

		Assert.NotEmpty(groups);
		foreach (var group in groups)
		{
			var dep = group.Elements().SingleOrDefault(e =>
				(string?)e.Attribute("id") == "System.Configuration.ConfigurationManager");
			Assert.True(dep != null, $"Missing in group {group.Attribute("targetFramework")}");
			Assert.StartsWith("10.", (string)dep.Attribute("version")!);
		}
	}
}
