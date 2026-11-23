using System;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>The hook for CLR properties of an unresolved user type converts text through the type's converter.</summary>
public class XamlValueConverterClrTextTests
{
	public class Probe
	{
		public TimeSpan Delay { get; set; }
		public Guid Id { get; set; }
		public string Label { get; set; } = "";
		public int? Count { get; set; }
	}

	[Fact]
	public void SetClrProperty_uses_the_TypeConverter_for_types_without_a_registered_converter()
	{
		var p = new Probe();

		XamlValueConverter.SetClrProperty(p, nameof(Probe.Delay), "0:0:2.5");
		XamlValueConverter.SetClrProperty(p, nameof(Probe.Id), "6b1f1d2c-0000-0000-0000-000000000001");

		Assert.Equal(TimeSpan.FromSeconds(2.5), p.Delay);
		Assert.Equal(new Guid("6b1f1d2c-0000-0000-0000-000000000001"), p.Id);
	}

	[Fact]
	public void Strings_and_registered_types_are_unchanged()
	{
		var p = new Probe();

		XamlValueConverter.SetClrProperty(p, nameof(Probe.Label), " a b ");
		XamlValueConverter.SetClrProperty(p, nameof(Probe.Count), "7");

		Assert.Equal(" a b ", p.Label);
		Assert.Equal(7, p.Count);
	}

	[Fact]
	public void CoerceResource_converts_text_to_the_target_type()
	{
		Assert.Equal(TimeSpan.FromMinutes(1), XamlValueConverter.CoerceResource(typeof(TimeSpan), "0:1:0"));
		Assert.Equal("x", XamlValueConverter.CoerceResource(typeof(object), "x"));
	}
}
