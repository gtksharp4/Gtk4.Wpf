using Gtk4.Forms.ProjectConverter;
using Xunit;

namespace Gtk4.Forms.ProjectConverter.Tests;

/// <summary>Defect #3: single surviving non-net10.0 TFM must collapse to net10.0.</summary>
public class CsprojConverterTfmRetargetTests
{
	const string Prefix = "<Project><PropertyGroup>";
	const string Suffix = "</PropertyGroup></Project>";

	static string Wrap(string inner) => $"{Prefix}{inner}{Suffix}";

	[Fact]
	public void Net8CollapseToNet10()
	{
		var input = Wrap("<TargetFramework>net8.0</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void Net472CollapseToNet10()
	{
		var input = Wrap("<TargetFramework>net472</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void NetCoreApp31CollapseToNet10()
	{
		var input = Wrap("<TargetFramework>netcoreapp3.1</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void Net10IsIdempotent()
	{
		// net10.0 already — no change, and OutputType/StartupObject are absent so the
		// converter will still mutate those; use a fully-converted project to prove no-op.
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <OutputType>Exe</OutputType>
			    <StartupObject>Gtk4.Forms.Host.GtkEntryPoint</StartupObject>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.False(changed);
		Assert.Equal(input, result);
	}

	[Fact]
	public void Net6CollapseToNet10()
	{
		var input = Wrap("<TargetFramework>net6.0</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}
}
