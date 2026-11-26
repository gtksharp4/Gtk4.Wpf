using Gtk4.Wpf.ProjectConverter;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>Transform rules: removal of Windows-only SDK opt-ins.</summary>
public class CsprojConverterWpfOptInTests
{
	[Fact]
	public void RemovesUseWpf()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0-windows</TargetFramework>
			    <UseWPF>true</UseWPF>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("UseWPF", result);
	}

	[Fact]
	public void RemovesUseWindowsForms()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0-windows</TargetFramework>
			    <UseWindowsForms>true</UseWindowsForms>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("UseWindowsForms", result);
	}

	[Fact]
	public void RemovesBothWpfAndWindowsForms()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0-windows</TargetFramework>
			    <UseWPF>true</UseWPF>
			    <UseWindowsForms>true</UseWindowsForms>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("UseWPF", result);
		Assert.DoesNotContain("UseWindowsForms", result);
	}

	[Fact]
	public void UseWpfAbsent_IsNoOp_ForThatRule()
	{
		// When UseWPF is already absent, that specific field does not trigger a change.
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <OutputType>Exe</OutputType>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.False(changed);
		Assert.Equal(input, result);
	}
}
