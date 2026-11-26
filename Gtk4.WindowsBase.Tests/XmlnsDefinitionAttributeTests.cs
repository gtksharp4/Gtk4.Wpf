using System;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>
/// Tests for <see cref="XmlnsDefinitionAttribute"/>: shape, properties, and AttributeUsage.
/// </summary>
public class XmlnsDefinitionAttributeTests
{
	[Fact]
	public void Ctor_SetsXmlNamespace()
	{
		var attr = new XmlnsDefinitionAttribute(
			"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
			"System.Windows");
		Assert.Equal("http://schemas.microsoft.com/winfx/2006/xaml/presentation", attr.XmlNamespace);
	}

	[Fact]
	public void Ctor_SetsClrNamespace()
	{
		var attr = new XmlnsDefinitionAttribute(
			"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
			"System.Windows.Controls");
		Assert.Equal("System.Windows.Controls", attr.ClrNamespace);
	}

	[Fact]
	public void Ctor_NullXmlNamespace_Throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new XmlnsDefinitionAttribute(null!, "System.Windows"));
	}

	[Fact]
	public void Ctor_NullClrNamespace_Throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new XmlnsDefinitionAttribute("http://example.com/ns", null!));
	}

	[Fact]
	public void AttributeUsage_IsAssembly()
	{
		var usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
			typeof(XmlnsDefinitionAttribute),
			typeof(AttributeUsageAttribute))!;
		Assert.Equal(AttributeTargets.Assembly, usage.ValidOn);
	}

	[Fact]
	public void AttributeUsage_AllowMultipleIsTrue()
	{
		var usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
			typeof(XmlnsDefinitionAttribute),
			typeof(AttributeUsageAttribute))!;
		Assert.True(usage.AllowMultiple);
	}

}
