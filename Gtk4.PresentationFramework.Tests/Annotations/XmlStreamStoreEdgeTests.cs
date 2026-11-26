using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Annotations.AnnotationServiceTests;
using static Gtk4.PresentationFramework.Tests.Annotations.XmlStreamStoreTests;

namespace Gtk4.PresentationFramework.Tests.Annotations;

public class XmlStreamStoreEdgeTests
{
	[Fact]
	public void Disposed_store_throws_and_dispose_is_idempotent()
	{
		var store = new XmlStreamStore(Stream());
		store.Dispose();
		store.Dispose();
		Assert.Throws<ObjectDisposedException>(() => store.GetAnnotations());
		Assert.Throws<ObjectDisposedException>(() => store.Flush());
	}

	[Fact]
	public void Stream_requirements()
	{
		Assert.Throws<ArgumentNullException>(() => new XmlStreamStore(null!));
		Assert.Throws<ArgumentException>(() => new XmlStreamStore(new NonSeekable()));
		Assert.Throws<XmlException>(() => new XmlStreamStore(Stream("<other/>")));
		using var readOnly = new XmlStreamStore(new MemoryStream(Array.Empty<byte>(), false));
		readOnly.AddAnnotation(Note());
		Assert.Throws<UnauthorizedAccessException>(() => readOnly.Flush());
	}

	[Fact]
	public void Unknown_namespaces_are_ignored_and_recorded()
	{
		var xml = WpfDocument.Replace("</anc:Annotations>", "<x:Extra xmlns:x=\"urn:extra\"/></anc:Annotations>");
		using var store = new XmlStreamStore(Stream(xml));
		Assert.Single(store.GetAnnotations());
		Assert.Contains(new Uri("urn:extra", UriKind.RelativeOrAbsolute), store.IgnoredNamespaces);
		Assert.Equal(3, XmlStreamStore.WellKnownNamespaces.Count);
		Assert.Null(XmlStreamStore.GetWellKnownCompatibleNamespaces(new Uri(Core)));
	}

	[Fact]
	public void Known_namespaces_are_validated()
	{
		var dup = new Dictionary<Uri, IList<Uri>> { { new Uri(Core), null! } };
		Assert.Throws<ArgumentException>(() => new XmlStreamStore(Stream(), dup));
		var ok = new Dictionary<Uri, IList<Uri>> { { new Uri("urn:mine"), new[] { new Uri("urn:old") } } };
		using var store = new XmlStreamStore(Stream(), ok);
		Assert.Empty(store.GetAnnotations());
	}

	[Fact]
	public void Missing_required_attributes_fail_the_load()
	{
		var xml = WpfDocument.Replace("Id=\"4f0b6a3e-4b43-4f05-9a43-6b3d8d1a2c11\"", string.Empty);
		Assert.Throws<XmlException>(() => new XmlStreamStore(Stream(xml)));
	}

	sealed class NonSeekable : MemoryStream
	{
		public override bool CanSeek => false;
	}
}
