using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Gtk4.Integration.Tests;

/// <summary>Locates the repository's local NuGet feed and the latest version of a package in it.</summary>
static class PackageFeed
{
	/// <summary>
	/// Walks up from the test assembly location to the repository root (identified by
	/// <c>Gtk4.Wpf.sln</c>) and returns its <c>Packages</c> folder, or null when not found.
	/// </summary>
	public static string? FindPackagesDirectory()
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir != null)
		{
			string packages = Path.Combine(dir.FullName, "Packages");
			if (File.Exists(Path.Combine(dir.FullName, "Gtk4.Wpf.sln")) && Directory.Exists(packages))
				return packages;
			dir = dir.Parent;
		}
		return null;
	}

	/// <summary>Highest 4-part version of <paramref name="packageId"/> in the feed, or null.</summary>
	public static string? LatestVersion(string packagesDir, string packageId)
	{
		var rx = new Regex("^" + Regex.Escape(packageId) + @"\.(\d+\.\d+\.\d+\.\d+)\.nupkg$",
			RegexOptions.IgnoreCase);

		Version? best = null;
		foreach (string file in Directory.EnumerateFiles(packagesDir, packageId + ".*.nupkg"))
		{
			var m = rx.Match(Path.GetFileName(file));
			if (m.Success && Version.TryParse(m.Groups[1].Value, out var v) && (best == null || v > best))
				best = v;
		}
		return best?.ToString();
	}
}