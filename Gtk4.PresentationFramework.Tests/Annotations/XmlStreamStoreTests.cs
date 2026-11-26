using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Xml;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Annotations;

public class XmlStreamStoreTests
{
	internal const string Core = "http://schemas.microsoft.com/windows/annotations/2003/11/core";
	internal const string Base = "http://schemas.microsoft.com/windows/annotations/2003/11/base";

	// A document written the way WPF writes it: Highlight with a character-range anchor and a color cargo.
	internal static readonly string WpfDocument =
		"<?xml version=\"1.0\" encoding=\"utf-8\"?>"
		+ "<anc:Annotations xmlns:anc=\"" + Core + "\" xmlns:anb=\"" + Base + "\">"
		+ "<anc:Annotation Id=\"4f0b6a3e-4b43-4f05-9a43-6b3d8d1a2c11\" CreationTime=\"2020-05-01T10:00:00.0000000+00:00\""
		+ " LastModificationTime=\"2020-05-02T10:00:00.0000000+00:00\" Type=\"anb:Highlight\">"
		+ "<anc:Authors><anb:StringAuthor>ann</anb:StringAuthor></anc:Authors>"
		+ "<anc:Anchors><anc:Resource Id=\"a9b0f8c4-0c1a-4a5e-8a6e-0d6f3f6a7b22\">"
		+ "<anc:ContentLocator><anb:CharacterRange><anc:Item Name=\"Count\" Value=\"1\" />"
		+ "<anc:Item Name=\"Segment0\" Value=\"3,9\" /></anb:CharacterRange></anc:ContentLocator>"
		+ "</anc:Resource></anc:Anchors>"
		+ "<anc:Cargos><anc:Resource Id=\"c1c2c3c4-0c1a-4a5e-8a6e-0d6f3f6a7b33\" Name=\"Highlight\">"
		+ "<anb:Colors Background=\"#54FFFF00\" /></anc:Resource></anc:Cargos>"
		+ "</anc:Annotation></anc:Annotations>";

	internal static MemoryStream Stream(string xml = "")
	{
		var stream = new MemoryStream();
		var bytes = Encoding.UTF8.GetBytes(xml);
		stream.Write(bytes, 0, bytes.Length);
		stream.Position = 0;
		return stream;
	}

	internal static Annotation Note(string author = "ann", int start = 0, int end = 4)
	{
		var a = new Annotation(AnnotationModelTests.Type);
		a.Authors.Add(author);
		var anchor = new AnnotationResource("Anchor");
		var part = new ContentLocatorPart(AnnotationAnchorTests.Range);
		part.NameValuePairs.Add("Count", "1");
		part.NameValuePairs.Add("Segment0", start + "," + end);
		var locator = new ContentLocator();
		locator.Parts.Add(part);
		anchor.ContentLocators.Add(locator);
		a.Anchors.Add(anchor);
		return a;
	}

	[Fact]
	public void An_empty_stream_loads_as_zero_annotations()
	{
		using var store = new XmlStreamStore(Stream());
		Assert.Empty(store.GetAnnotations());
	}

	[Fact]
	public void Loads_a_hand_written_WPF_document()
	{
		using var store = new XmlStreamStore(Stream(WpfDocument));
		var a = Assert.Single(store.GetAnnotations());
		Assert.Equal(new Guid("4f0b6a3e-4b43-4f05-9a43-6b3d8d1a2c11"), a.Id);
		Assert.Equal(new XmlQualifiedName("Highlight", Base), a.AnnotationType);
		Assert.Equal(new[] { "ann" }, a.Authors);
		var part = ((ContentLocator)Assert.Single(a.Anchors[0].ContentLocators)).Parts[0];
		Assert.Equal(AnnotationAnchorTests.Range, part.PartType);
		Assert.Equal("3,9", part.NameValuePairs["Segment0"]);
		Assert.Equal("Highlight", a.Cargos[0].Name);
		Assert.Equal("#54FFFF00", a.Cargos[0].Contents[0].GetAttribute("Background"));
		Assert.Equal(new DateTime(2020, 5, 2, 10, 0, 0, DateTimeKind.Utc).ToUniversalTime(),
			a.LastModificationTime.ToUniversalTime());
	}

	[Fact]
	public void Round_trip_keeps_every_field_and_writes_the_core_and_base_namespaces()
	{
		var stream = Stream(WpfDocument);
		var store = new XmlStreamStore(stream);
		var original = store.GetAnnotations()[0];
		store.AddAnnotation(Note("bob", 1, 2));
		store.Flush();
		var xml = Encoding.UTF8.GetString(stream.ToArray());
		Assert.Contains("xmlns:anc=\"" + Core + "\"", xml);
		Assert.Contains("xmlns:anb=\"" + Base + "\"", xml);
		using var reloaded = new XmlStreamStore(new MemoryStream(stream.ToArray()));
		var all = reloaded.GetAnnotations();
		Assert.Equal(2, all.Count);
		var again = reloaded.GetAnnotation(original.Id)!;
		Assert.Equal(original.CreationTime, again.CreationTime);
		Assert.Equal(original.LastModificationTime, again.LastModificationTime);
		Assert.Equal(original.Anchors[0].Id, again.Anchors[0].Id);
		Assert.Equal("#54FFFF00", again.Cargos[0].Contents[0].GetAttribute("Background"));
		Assert.Equal("Colors", again.Cargos[0].Contents[0].LocalName);
	}

	[Fact]
	public void AutoFlush_writes_every_change_back_to_the_stream()
	{
		var stream = Stream();
		using var store = new XmlStreamStore(stream) { AutoFlush = true };
		var a = Note();
		store.AddAnnotation(a);
		Assert.Contains(a.Id.ToString(), Encoding.UTF8.GetString(stream.ToArray()));
		a.Authors.Add("carol");
		Assert.Contains("carol", Encoding.UTF8.GetString(stream.ToArray()));
		store.DeleteAnnotation(a.Id);
		Assert.DoesNotContain(a.Id.ToString(), Encoding.UTF8.GetString(stream.ToArray()));
	}

	[Fact]
	public void Without_AutoFlush_nothing_is_written_until_Flush()
	{
		var stream = Stream();
		using var store = new XmlStreamStore(stream);
		store.AddAnnotation(Note());
		Assert.Equal(0, stream.Length);
		store.Flush();
		Assert.True(stream.Length > 0);
	}

	[Fact]
	public void StoreContentChanged_reports_action_and_order()
	{
		using var store = new XmlStreamStore(Stream());
		var seen = new List<string>();
		store.StoreContentChanged += (s, e) => seen.Add(e.Action + ":" + e.Annotation.Authors[0]);
		var a = Note("a");
		var b = Note("b");
		store.AddAnnotation(a);
		store.AddAnnotation(b);
		store.DeleteAnnotation(a.Id);
		Assert.Equal(new[] { "Added:a", "Added:b", "Deleted:a" }, seen);
		Assert.Null(store.DeleteAnnotation(a.Id));
		Assert.Equal(3, seen.Count);
	}

	[Fact]
	public void Annotation_changes_reach_the_store_events()
	{
		using var store = new XmlStreamStore(Stream());
		var a = Note();
		store.AddAnnotation(a);
		var seen = new List<string>();
		store.AuthorChanged += (s, e) => seen.Add("author");
		store.AnchorChanged += (s, e) => seen.Add("anchor");
		store.CargoChanged += (s, e) => seen.Add("cargo");
		a.Authors.Add("x");
		a.Anchors[0].Name = "y";
		a.Cargos.Add(new AnnotationResource("c"));
		Assert.Equal(new[] { "author", "anchor", "cargo" }, seen);
	}

	[Fact]
	public void Add_get_and_duplicate_rules()
	{
		using var store = new XmlStreamStore(Stream());
		var a = Note();
		store.AddAnnotation(a);
		Assert.Same(a, store.GetAnnotation(a.Id));
		Assert.Null(store.GetAnnotation(Guid.NewGuid()));
		Assert.Throws<ArgumentException>(() => store.AddAnnotation(a));
		Assert.Throws<ArgumentNullException>(() => store.AddAnnotation(null!));
	}

	[Fact]
	public void GetAnnotations_by_locator_matches_prefix_and_overlap()
	{
		using var store = new XmlStreamStore(Stream());
		var a = Note("a", 0, 5);
		var b = Note("b", 20, 30);
		store.AddAnnotation(a);
		store.AddAnnotation(b);
		var query = new ContentLocator();
		var part = new ContentLocatorPart(AnnotationAnchorTests.Range);
		part.NameValuePairs.Add("Count", "1");
		part.NameValuePairs.Add("Segment0", "3,10");
		part.NameValuePairs.Add("IncludeOverlaps", "true");
		query.Parts.Add(part);
		Assert.Equal(new[] { a }, store.GetAnnotations(query));
		Assert.Equal(2, store.GetAnnotations(new ContentLocator()).Count);
		Assert.Throws<ArgumentNullException>(() => store.GetAnnotations(null!));
	}
}
