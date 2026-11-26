using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pins the WPF-compatible <see cref="ThemeInfoAttribute"/> + <see cref="ResourceDictionaryLocation"/>
/// shapes that the default WPF <c>AssemblyInfo.cs</c> depends on:
/// <c>[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]</c>.
/// </summary>
public class ThemeInfoAttributeTests
{
	[Fact]
	public void Constructor_stores_both_locations()
	{
		var attr = new ThemeInfoAttribute(
			ResourceDictionaryLocation.None,
			ResourceDictionaryLocation.SourceAssembly);

		Assert.Equal(ResourceDictionaryLocation.None, attr.ThemeDictionaryLocation);
		Assert.Equal(ResourceDictionaryLocation.SourceAssembly, attr.GenericDictionaryLocation);
	}

	[Fact]
	public void Targets_assembly_only()
	{
		var usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
			typeof(ThemeInfoAttribute), typeof(AttributeUsageAttribute))!;

		Assert.Equal(AttributeTargets.Assembly, usage.ValidOn);
	}

	[Fact]
	public void ResourceDictionaryLocation_values_match_wpf()
	{
		Assert.Equal(0, (int)ResourceDictionaryLocation.None);
		Assert.Equal(1, (int)ResourceDictionaryLocation.SourceAssembly);
		Assert.Equal(2, (int)ResourceDictionaryLocation.ExternalAssembly);
	}
}