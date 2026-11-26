using Gtk4.Wpf.ProjectConverter;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>Transform rule: direct pins below 10 of packages the Wpf package now flows are raised (NU1605).</summary>
public class CsprojConverterWpfFrameworkPackageTests
{
	static string Project(string line) =>
		"<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    " + line + "\n  </ItemGroup>\n</Project>";

	[Fact]
	public void Raises_prerelease_EventLog_pin_and_changes_nothing_else()
	{
		var input = Project(
			"<PackageReference Include=\"System.Diagnostics.EventLog\" Version=\"4.6.0-preview8.19358.3\" />");

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Equal(input.Replace("4.6.0-preview8.19358.3", "10.0.0"), result);
		Assert.Contains("Version=\"10.0.0\"", result);
	}

	[Fact]
	public void Raises_ConfigurationManager_below_10()
	{
		var result = CsprojConverter.Transform(
			Project("<PackageReference Include=\"System.Configuration.ConfigurationManager\" Version=\"8.0.1\" />"),
			out var changed);

		Assert.True(changed);
		Assert.Contains("Version=\"10.0.0\"", result);
	}

	[Fact]
	public void Raises_version_child_element()
	{
		var input = Project("<PackageReference Include=\"System.Security.Cryptography.ProtectedData\">"
			+ "<Version>6.0.0</Version></PackageReference>");

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<Version>10.0.0</Version>", result);
	}

	[Fact]
	public void Raises_when_version_precedes_include()
	{
		var result = CsprojConverter.Transform(
			Project("<PackageReference Version=\"6.0.0\" Include=\"System.Diagnostics.EventLog\" />"),
			out var changed);

		Assert.True(changed);
		Assert.Contains("Version=\"10.0.0\" Include", result);
	}

	[Theory]
	[InlineData("10.0.2")]
	[InlineData("11.0.0")]
	[InlineData("$(CmVersion)")]
	[InlineData("[8.0,)")]
	[InlineData("(8.0,9.0)")]
	public void Leaves_other_versions_unchanged(string version)
	{
		var input = Project(
			$"<PackageReference Include=\"System.Configuration.ConfigurationManager\" Version=\"{version}\" />");

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.False(changed);
		Assert.Equal(input, result);
	}

	[Fact]
	public void Leaves_unversioned_reference_unchanged()
	{
		var input = Project("<PackageReference Include=\"System.Diagnostics.EventLog\" />");

		Assert.Equal(input, CsprojConverter.Transform(input, out var changed));
		Assert.False(changed);
	}

	[Fact]
	public void Leaves_unrelated_package_unchanged()
	{
		var input = Project("<PackageReference Include=\"Newtonsoft.Json\" Version=\"8.0.1\" />");

		Assert.Equal(input, CsprojConverter.Transform(input, out var changed));
		Assert.False(changed);
	}

	[Fact]
	public void Is_idempotent()
	{
		var first = CsprojConverter.Transform(
			Project("<PackageReference Include=\"System.Diagnostics.EventLog\" Version=\"4.6.0\" />"), out _);

		var second = CsprojConverter.Transform(first, out var changed);

		Assert.False(changed);
		Assert.Equal(first, second);
	}

	[Fact]
	public void Matches_include_case_insensitively()
	{
		var result = CsprojConverter.Transform(
			Project("<PackageReference Include=\"system.diagnostics.eventlog\" Version=\"4.7.0\" />"),
			out var changed);

		Assert.True(changed);
		Assert.Contains("Version=\"10.0.0\"", result);
	}

	[Fact]
	public void Raises_10_0_0_prerelease_pin()
	{
		var result = CsprojConverter.Transform(
			Project("<PackageReference Include=\"System.Diagnostics.EventLog\" Version=\"10.0.0-rc.2.25502.107\" />"),
			out var changed);

		Assert.True(changed);
		Assert.Contains("Version=\"10.0.0\"", result);
	}

	[Fact]
	public void Leaves_10_0_1_prerelease_pin()
	{
		var input = Project("<PackageReference Include=\"System.Diagnostics.EventLog\" Version=\"10.0.1-rc.1\" />");

		Assert.Equal(input, CsprojConverter.Transform(input, out var changed));
		Assert.False(changed);
	}
}
