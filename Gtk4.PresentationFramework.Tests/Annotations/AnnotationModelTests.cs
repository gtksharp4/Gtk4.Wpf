using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Annotations;
using System.Xml;
using System.Xml.Serialization;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Annotations;

public class AnnotationModelTests
{
	internal static readonly XmlQualifiedName Type = new XmlQualifiedName("Highlight",
		"http://schemas.microsoft.com/windows/annotations/2003/11/base");

	[Fact]
	public void Annotation_ctor_stamps_id_and_times()
	{
		var a = new Annotation(Type);
		Assert.NotEqual(Guid.Empty, a.Id);
		Assert.Equal(Type, a.AnnotationType);
		Assert.Equal(a.CreationTime, a.LastModificationTime);
		Assert.Empty(a.Authors);
		Assert.Empty(a.Anchors);
		Assert.Empty(a.Cargos);
	}

	[Fact]
	public void Annotation_ctor_validates()
	{
		Assert.Throws<ArgumentNullException>(() => new Annotation(null!));
		Assert.Throws<ArgumentException>(() => new Annotation(new XmlQualifiedName("x")));
		var now = DateTime.Now;
		Assert.Throws<ArgumentException>(() => new Annotation(Type, Guid.Empty, now, now));
		Assert.Throws<ArgumentException>(() => new Annotation(Type, Guid.NewGuid(), now, now.AddSeconds(-1)));
	}

	[Fact]
	public void Author_changes_raise_events_in_order()
	{
		var a = new Annotation(Type);
		var seen = new List<string>();
		a.AuthorChanged += (s, e) => seen.Add(e.Action + ":" + e.Author);
		a.Authors.Add("ann");
		a.Authors.Add("bob");
		a.Authors.Remove("ann");
		Assert.Equal(new[] { "Added:ann", "Added:bob", "Removed:ann" }, seen);
	}

	[Fact]
	public void Anchor_and_cargo_changes_raise_events_and_bump_the_modification_time()
	{
		var a = new Annotation(Type, Guid.NewGuid(), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1));
		var before = a.LastModificationTime;
		var seen = new List<string>();
		a.AnchorChanged += (s, e) => seen.Add("anchor " + e.Action);
		a.CargoChanged += (s, e) => seen.Add("cargo " + e.Action);
		var anchor = new AnnotationResource("A");
		a.Anchors.Add(anchor);
		var cargo = new AnnotationResource("C");
		a.Cargos.Add(cargo);
		cargo.Name = "D";
		a.Anchors.Remove(anchor);
		Assert.Equal(new[] { "anchor Added", "cargo Added", "cargo Modified", "anchor Removed" }, seen);
		Assert.True(a.LastModificationTime > before);
	}

	[Fact]
	public void A_resource_can_belong_to_one_collection_only()
	{
		var a = new Annotation(Type);
		var r = new AnnotationResource();
		a.Anchors.Add(r);
		Assert.Throws<InvalidOperationException>(() => a.Cargos.Add(r));
		a.Anchors.Remove(r);
		a.Cargos.Add(r);
		Assert.Throws<ArgumentNullException>(() => a.Cargos.Add(null!));
	}

	[Fact]
	public void Resource_ctors_and_properties()
	{
		Assert.Throws<ArgumentException>(() => new AnnotationResource(Guid.Empty));
		Assert.Throws<ArgumentNullException>(() => new AnnotationResource((string)null!));
		var id = Guid.NewGuid();
		var r = new AnnotationResource(id);
		Assert.Equal(id, r.Id);
		Assert.Null(r.Name);
		var changes = new List<string?>();
		((INotifyPropertyChanged)r).PropertyChanged += (s, e) => changes.Add(e.PropertyName);
		r.Name = "n";
		r.Name = "n";
		r.ContentLocators.Add(new ContentLocator());
		r.Contents.Add(new XmlDocument().CreateElement("x"));
		Assert.Equal(new[] { "Name", "Locators", "Contents" }, changes);
	}

	[Fact]
	public void Part_ctor_validates_and_equality_compares_pairs()
	{
		Assert.Throws<ArgumentNullException>(() => new ContentLocatorPart(null!));
		Assert.Throws<ArgumentException>(() => new ContentLocatorPart(new XmlQualifiedName("p")));
		var a = new ContentLocatorPart(AnnotationAnchorTests.Range);
		var b = new ContentLocatorPart(AnnotationAnchorTests.Range);
		a.NameValuePairs["k"] = "v";
		Assert.NotEqual(a, b);
		b.NameValuePairs.Add("k", "v");
		Assert.Equal(a, b);
		Assert.Equal(a, (ContentLocatorPart)a.Clone());
	}

	[Fact]
	public void Locator_StartsWith_matches_a_prefix_of_parts()
	{
		var full = AnnotationAnchorTests.Locator(("a", "1"), ("b", "2"));
		var prefix = AnnotationAnchorTests.Locator(("a", "1"));
		Assert.True(full.StartsWith(prefix));
		Assert.False(prefix.StartsWith(full));
		Assert.True(full.StartsWith(new ContentLocator()));
		Assert.False(full.StartsWith(AnnotationAnchorTests.Locator(("a", "9"))));
		Assert.Throws<ArgumentNullException>(() => full.StartsWith(null!));
	}

	[Fact]
	public void Locator_group_clone_is_deep_and_collections_are_owned()
	{
		var group = new ContentLocatorGroup();
		group.Locators.Add(AnnotationAnchorTests.Locator(("a", "1")));
		var clone = (ContentLocatorGroup)group.Clone();
		Assert.NotSame(group.Locators[0], clone.Locators[0]);
		Assert.Throws<InvalidOperationException>(() => clone.Locators.Add(group.Locators[0]));
	}

	[Fact]
	public void Annotation_serializes_through_XmlSerializer()
	{
		var a = new Annotation(Type);
		a.Authors.Add("ann");
		var serializer = new XmlSerializer(typeof(Annotation));
		var writer = new StringWriter();
		serializer.Serialize(writer, a);
		var back = (Annotation)serializer.Deserialize(new StringReader(writer.ToString()))!;
		Assert.Equal(a.Id, back.Id);
		Assert.Equal(new[] { "ann" }, back.Authors);
	}
}
