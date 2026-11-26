using System.Windows.Annotations;
using System.Xml;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Annotations;

public class AnnotationAnchorTests
{
	internal static readonly XmlQualifiedName Range = new XmlQualifiedName("CharacterRange",
		"http://schemas.microsoft.com/windows/annotations/2003/11/base");

	internal static ContentLocator Locator(params (string Name, string Value)[] parts)
	{
		var locator = new ContentLocator();
		foreach (var (name, value) in parts)
		{
			var part = new ContentLocatorPart(Range);
			part.NameValuePairs.Add(name, value);
			locator.Parts.Add(part);
		}
		return locator;
	}

	static ContentLocatorPart Seg(string range, bool overlaps)
	{
		var part = new ContentLocatorPart(Range);
		part.NameValuePairs.Add("Count", "1");
		part.NameValuePairs.Add("Segment0", range);
		if (overlaps) part.NameValuePairs.Add("IncludeOverlaps", "true");
		return part;
	}

	[Fact]
	public void IncludeOverlaps_matches_overlapping_ranges_only()
	{
		var query = new ContentLocator();
		query.Parts.Add(Seg("10,20", true));
		var inside = new ContentLocator();
		inside.Parts.Add(Seg("15,18", false));
		var straddling = new ContentLocator();
		straddling.Parts.Add(Seg("5,12", false));
		var apart = new ContentLocator();
		apart.Parts.Add(Seg("30,40", false));
		Assert.True(inside.StartsWith(query));
		Assert.True(straddling.StartsWith(query));
		Assert.False(apart.StartsWith(query));
	}

	[Fact]
	public void Without_IncludeOverlaps_ranges_must_be_equal()
	{
		var query = new ContentLocator();
		query.Parts.Add(Seg("10,20", false));
		var other = new ContentLocator();
		other.Parts.Add(Seg("10,21", false));
		Assert.False(other.StartsWith(query));
	}
}
