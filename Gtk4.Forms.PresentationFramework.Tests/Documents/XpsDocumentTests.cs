using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class XpsDocumentTests : WpfGtkTestBase
{
	static XpsDocument Open(out string path, params int[] pages)
	{
		path = XpsTestPackage.TempPath();
		XpsTestPackage.Create(path, pages);
		return new XpsDocument(path, FileAccess.Read);
	}

	[Fact]
	public void The_sequence_lists_documents_pages_and_relative_sources() => Run(() =>
	{
		using var xps = Open(out string path, 2, 3);
		var sequence = xps.GetFixedDocumentSequence()!;
		Assert.Equal(2, sequence.References.Count);
		Assert.Equal("Documents/2/FixedDocument.fdoc", sequence.References[1].Source!.OriginalString);
		var document = sequence.References[1].GetDocument(false)!;
		Assert.Equal(3, document.Pages.Count);
		Assert.Equal("Pages/3.fpage", document.Pages[2].Source!.OriginalString);
		Assert.Equal(5, sequence.DocumentPaginator.PageCount);
		xps.Close();
		File.Delete(path);
	});

	[Fact]
	public void A_page_root_loads_its_path_canvas_and_glyphs() => Run(() =>
	{
		using var xps = Open(out string path, 1);
		var page = xps.GetFixedDocumentSequence()!.References[0].GetDocument(false)!.Pages[0].GetPageRoot(false)!;
		Assert.Equal(816, page.Width);
		Assert.Equal(1056, page.Height);
		var shape = Assert.IsType<System.Windows.Shapes.Path>(page.Children[0]);
		Assert.NotNull(shape.Data);
		Assert.NotNull(shape.Fill);
		var canvas = Assert.IsType<Canvas>(page.Children[1]);
		var glyphs = Assert.IsType<Glyphs>(canvas.Children[0]);
		Assert.Equal("Hi", glyphs.UnicodeString);
		Assert.Equal(12, glyphs.FontRenderingEmSize);
		Assert.Equal(20, glyphs.OriginY);
		Assert.Equal("/Resources/f.ttf", glyphs.FontUri!.OriginalString);
		Assert.NotNull(canvas.RenderTransform);
		xps.Close();
		File.Delete(path);
	});

	[Fact]
	public void GetPageRoot_caches_until_forced() => Run(() =>
	{
		using var xps = Open(out string path, 1);
		var content = xps.GetFixedDocumentSequence()!.References[0].GetDocument(false)!.Pages[0];
		var first = content.GetPageRoot(false);
		Assert.Same(first, content.GetPageRoot(false));
		Assert.NotSame(first, content.GetPageRoot(true));
		xps.Close();
		File.Delete(path);
	});

	[Fact]
	public void Write_then_read_keeps_the_pages_and_resources() => Run(() =>
	{
		using var source = Open(out string sourcePath, 2, 1);
		string target = XpsTestPackage.TempPath();
		using (var output = new XpsDocument(target, FileAccess.ReadWrite))
			XpsDocument.CreateXpsDocumentWriter(output).Write(source.GetFixedDocumentSequence()!);
		using var read = new XpsDocument(target, FileAccess.Read);
		var sequence = read.GetFixedDocumentSequence()!;
		Assert.Equal(new[] { 2, 1 }, sequence.References.Select(r => r.GetDocument(false)!.Pages.Count).ToArray());
		var glyphs = sequence.References[1].GetDocument(false)!.Pages[0].GetPageRoot(false)!.Children
			.OfType<Canvas>().Single().Children.OfType<Glyphs>().Single();
		Assert.Equal("Hi", glyphs.UnicodeString);
		Assert.True(read.Package.PartExists("/Resources/f.ttf"));
		read.Close();
		source.Close();
		File.Delete(target);
		File.Delete(sourcePath);
	});

	[Fact]
	public void Merging_pages_from_two_documents_through_new_references_round_trips() => Run(() =>
	{
		using var a = Open(out string pathA, 2);
		using var b = Open(out string pathB, 3);
		var merged = new FixedDocumentSequence();
		foreach (var xps in new[] { a, b })
		{
			var reference = xps.GetFixedDocumentSequence()!.References[0];
			var copy = new DocumentReference { Source = reference.Source };
			((IUriContext)copy).BaseUri = ((IUriContext)reference).BaseUri;
			merged.References.Add(copy);
		}
		Assert.Equal(5, merged.DocumentPaginator.PageCount);
		string target = XpsTestPackage.TempPath();
		using (var output = new XpsDocument(target, FileAccess.ReadWrite))
			XpsDocument.CreateXpsDocumentWriter(output).Write(merged);
		using var read = new XpsDocument(target, FileAccess.Read);
		Assert.Equal(5, read.GetFixedDocumentSequence()!.DocumentPaginator.PageCount);
		read.Close();
		a.Close();
		b.Close();
		File.Delete(target);
		File.Delete(pathA);
		File.Delete(pathB);
	});

	[Fact]
	public void In_memory_glyph_pages_are_serialised_and_paths_are_rejected() => Run(() =>
	{
		var page = new FixedPage { Width = 100, Height = 50 };
		page.Children.Add(new Glyphs { UnicodeString = "A", FontRenderingEmSize = 10, OriginX = 1, OriginY = 9 });
		var content = new PageContent();
		((IAddChild)content).AddChild(page);
		var document = new FixedDocument();
		document.Pages.Add(content);
		string target = XpsTestPackage.TempPath();
		using (var output = new XpsDocument(target, FileAccess.ReadWrite))
			XpsDocument.CreateXpsDocumentWriter(output).Write(document);
		using (var read = new XpsDocument(target, FileAccess.Read))
		{
			var first = read.GetFixedDocumentSequence()!.References[0].GetDocument(false)!;
			var loaded = first.Pages[0].GetPageRoot(false)!;
			Assert.Equal(100, loaded.Width);
			Assert.Equal("A", ((Glyphs)loaded.Children[0]).UnicodeString);
		}
		page.Children.Add(new System.Windows.Shapes.Path());
		using var again = new XpsDocument(target, FileAccess.ReadWrite);
		Assert.Throws<NotSupportedException>(() => XpsDocument.CreateXpsDocumentWriter(again).Write(document));
		again.Close();
		File.Delete(target);
	});

	[Fact]
	public void Closed_and_read_only_documents_reject_use() => Run(() =>
	{
		var xps = Open(out string path, 1);
		var sequence = xps.GetFixedDocumentSequence()!;
		Assert.Throws<InvalidOperationException>(() =>
			XpsDocument.CreateXpsDocumentWriter(xps).Write(sequence));
		xps.Close();
		Assert.Throws<ObjectDisposedException>(() => xps.GetFixedDocumentSequence());
		Assert.Throws<ArgumentNullException>(() => XpsDocument.CreateXpsDocumentWriter(null!));
		File.Delete(path);
	});

	[Fact]
	public void A_missing_package_throws_FileNotFoundException() =>
		Assert.Throws<FileNotFoundException>(() => new XpsDocument(XpsTestPackage.TempPath(), FileAccess.Read));

	[Fact]
	public void Part_names_are_normalised_with_forward_slashes_on_every_os()
	{
		Assert.Equal("/Resources/f.ttf",
			XpsPackage.ResolveTarget("/Documents/1/Pages/1.fpage", "../../../Resources/f.ttf"));
		Assert.Equal("/a/b.xml", XpsPackage.NormalizePart("\\a\\.\\c\\..\\b.xml"));
		Assert.Equal("/x/y", XpsPackage.ResolveTarget("/z", "/x/y"));
	}

	[Fact]
	public void The_ODTTF_key_is_an_involution()
	{
		var data = Enumerable.Range(0, 64).Select(i => (byte)i).ToArray();
		var copy = (byte[])data.Clone();
		XpsFontNames.Deobfuscate(copy, "/Resources/7f79d960-6410-4321-8e96-1651f34a0799.ODTTF");
		Assert.NotEqual(data, copy);
		XpsFontNames.Deobfuscate(copy, "/Resources/7f79d960-6410-4321-8e96-1651f34a0799.ODTTF");
		Assert.Equal(data, copy);
	}
}
