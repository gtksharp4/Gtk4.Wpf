using Gtk4.Wpf.ProjectConverter;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>
/// Transform rules: <c>OutputType</c> is preserved. The WPF converter must never rewrite it —
/// a WPF app keeps its <c>WinExe</c> (its entry point comes from the compiled
/// <c>ApplicationDefinition</c>, which works for any executable output type).
/// </summary>
public class CsprojConverterWpfOutputTypeTests
{
	[Fact]
	public void WinExeIsPreservedDuringConversion()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0-windows</TargetFramework>
			    <UseWPF>true</UseWPF>
			    <OutputType>WinExe</OutputType>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		// TFM + UseWPF still trigger a conversion...
		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
		Assert.DoesNotContain("UseWPF", result);
		// ...but OutputType is left exactly as the author wrote it.
		Assert.Contains("<OutputType>WinExe</OutputType>", result);
		Assert.DoesNotContain("<OutputType>Exe</OutputType>", result);
	}

	[Fact]
	public void WinExeAloneIsNoOp()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <OutputType>WinExe</OutputType>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		// Nothing to collapse or strip, and OutputType is not touched → byte-for-byte identical.
		Assert.False(changed);
		Assert.Equal(input, result);
	}

	[Fact]
	public void OutputTypeIsNotInsertedWhenAbsent()
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
		Assert.DoesNotContain("<OutputType>", result);
	}
}
