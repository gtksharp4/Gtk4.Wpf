using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Gtk4.Forms.Wpf.ProjectConverter.Tests;

/// <summary>Inspects the nuspec of the just-built <c>Gtk4.Forms.Wpf</c> package (built first via ProjectReference).</summary>
public class PackedNuspecTests
{
	static string LatestNupkg()
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Gtk4.Wpf.sln")))
			dir = dir.Parent;
		Assert.NotNull(dir);

		var packages = Path.Combine(dir.FullName, "Packages");
		var rx = new Regex(@"^Gtk4\.Forms\.Wpf\.(\d+\.\d+\.\d+\.\d+)\.nupkg$", RegexOptions.IgnoreCase);
		var best = Directory.Exists(packages)
			? Directory.EnumerateFiles(packages, "Gtk4.Forms.Wpf.*.nupkg")
				.Select(f => (File: f, Match: rx.Match(Path.GetFileName(f))))
				.Where(x => x.Match.Success)
				.OrderByDescending(x => Version.Parse(x.Match.Groups[1].Value))
				.FirstOrDefault()
			: default;
		if (best.File == null)
			Assert.Fail("Gtk4.Forms.Wpf nupkg not found in Packages/; build the solution first.");
		return best.File;
	}

	[Fact]
	public void Packed_nuspec_declares_ConfigurationManager_10x_in_every_dependency_group()
	{
		using var zip = ZipFile.OpenRead(LatestNupkg());
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

	[Fact]
	public void Packed_nuspec_declares_SystemEvents_10x_in_every_dependency_group()
	{
		using var zip = ZipFile.OpenRead(LatestNupkg());
		using var stream = zip.Entries.Single(e => e.FullName.EndsWith(".nuspec")).Open();
		var groups = XDocument.Load(stream).Descendants().Where(e => e.Name.LocalName == "group").ToList();

		Assert.NotEmpty(groups);
		foreach (var group in groups)
		{
			var dep = group.Elements().SingleOrDefault(e => (string?)e.Attribute("id") == "Microsoft.Win32.SystemEvents");
			Assert.True(dep != null, $"Missing in group {group.Attribute("targetFramework")}");
			Assert.StartsWith("10.0.0", ((string)dep.Attribute("version")!).TrimStart('[', '('));
		}
	}
}
