using System;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.FlowDirectionTests;

/// <summary>The instance surface of NumberSubstitution and the tag validation of XmlLanguage, as in WPF.</summary>
public class NumberSubstitutionInstanceTests
{
	[Fact]
	public void The_constructor_values_round_trip()
	{
		var ar = new CultureInfo("ar-SA");
		var ns = new NumberSubstitution(NumberCultureSource.Override, ar, NumberSubstitutionMethod.Traditional);

		Assert.Equal(NumberCultureSource.Override, ns.CultureSource);
		Assert.Same(ar, ns.CultureOverride);
		Assert.Equal(NumberSubstitutionMethod.Traditional, ns.Substitution);
	}

	[Fact]
	public void The_parameterless_constructor_matches_the_WPF_defaults()
	{
		var ns = new NumberSubstitution();

		Assert.Equal(NumberCultureSource.Text, ns.CultureSource);
		Assert.Null(ns.CultureOverride);
		Assert.Equal(NumberSubstitutionMethod.AsCulture, ns.Substitution);
	}

	[Fact]
	public void Equal_instances_are_Equal_with_the_same_hash()
	{
		var a = new NumberSubstitution(NumberCultureSource.Override, new CultureInfo("ar-SA"),
			NumberSubstitutionMethod.Traditional);
		var b = new NumberSubstitution(NumberCultureSource.Override, new CultureInfo("ar-SA"),
			NumberSubstitutionMethod.Traditional);

		Assert.Equal(a, b);
		Assert.Equal(a.GetHashCode(), b.GetHashCode());
		Assert.NotEqual(a, new NumberSubstitution());
	}

	[Fact]
	public void NumberSubstitution_and_XmlLanguage_are_not_sealed()
	{
		Assert.False(typeof(NumberSubstitution).IsSealed);
		Assert.False(typeof(XmlLanguage).IsSealed);
	}

	[Theory]
	[InlineData("not a tag!!")]
	[InlineData("1a")]
	[InlineData("a-")]
	[InlineData("-a")]
	[InlineData("en_US")]
	[InlineData("abcdefghi")]
	public void GetLanguage_rejects_a_tag_that_is_not_RFC_3066(string tag) =>
		Assert.Throws<ArgumentException>(() => XmlLanguage.GetLanguage(tag));

	[Theory]
	[InlineData("ar-SA")]
	[InlineData("")]
	[InlineData("x-1234")]
	[InlineData("en")]
	public void GetLanguage_accepts_a_valid_tag(string tag) => Assert.NotNull(XmlLanguage.GetLanguage(tag));
}
