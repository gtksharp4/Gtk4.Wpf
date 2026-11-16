using System.Text.RegularExpressions;
using Gtk4.Forms.ProjectConverter;
using Xunit;

namespace Gtk4.Forms.ProjectConverter.Tests;

/// <summary>Defect #2: at most one OutputType→Exe and exactly one StartupObject, even with multiple PropertyGroups.</summary>
public class CsprojConverterMultiPropertyGroupTests
{
	static readonly Regex _outputTypePattern = new(
		@"<OutputType>[^<]*</OutputType>",
		RegexOptions.IgnoreCase);

	static readonly Regex _startupObjectPattern = new(
		@"<StartupObject>[^<]*</StartupObject>",
		RegexOptions.IgnoreCase);

	const string MultiGroup =
		"""
		<Project Sdk="Microsoft.NET.Sdk">
		  <PropertyGroup>
		    <TargetFramework>net10.0</TargetFramework>
		    <OutputType>Library</OutputType>
		  </PropertyGroup>
		  <PropertyGroup Condition="'$(Configuration)'=='Debug'">
		    <OutputType>Library</OutputType>
		  </PropertyGroup>
		  <PropertyGroup Condition="'$(Configuration)'=='Release'">
		    <OutputType>Library</OutputType>
		  </PropertyGroup>
		</Project>
		""";

	[Fact]
	public void MultipleOutputTypes_OnlyFirstBecomesExe()
	{
		var result = CsprojConverter.Transform(MultiGroup, out var changed);

		Assert.True(changed);
		var exeMatches = Regex.Matches(result, @"<OutputType>\s*Exe\s*</OutputType>", RegexOptions.IgnoreCase);
		Assert.Single(exeMatches);
	}

	[Fact]
	public void MultipleOutputTypes_ExactlyOneStartupObject()
	{
		var result = CsprojConverter.Transform(MultiGroup, out _);

		var soMatches = _startupObjectPattern.Matches(result);
		Assert.Single(soMatches);
		Assert.Contains("<StartupObject>Gtk4.Forms.Host.GtkEntryPoint</StartupObject>", result);
	}

	[Fact]
	public void MultipleOutputTypes_IsIdempotent()
	{
		var once = CsprojConverter.Transform(MultiGroup, out _);
		var twice = CsprojConverter.Transform(once, out var changedOnSecond);

		Assert.False(changedOnSecond);
		Assert.Equal(once, twice);
	}

	[Fact]
	public void AlreadyExeWithStartup_MultiGroup_IsNoOp()
	{
		// A project that already has Exe + StartupObject in the first group,
		// plus Library in a conditional group — must not be touched.
		const string input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <OutputType>Exe</OutputType>
			    <StartupObject>Gtk4.Forms.Host.GtkEntryPoint</StartupObject>
			  </PropertyGroup>
			  <PropertyGroup Condition="'$(Configuration)'=='Debug'">
			    <OutputType>Library</OutputType>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		// The converter sees OutputType>Exe and StartupObject already present — no-op.
		Assert.False(changed);
		Assert.Equal(input, result);
	}
}
