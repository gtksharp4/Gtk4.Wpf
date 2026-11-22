using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class FixedDocumentTests : WpfGtkTestBase
{
	static PageContent PageOf(double width, double height)
	{
		var page = new FixedPage { Width = width, Height = height };
		var content = new PageContent();
		((IAddChild)content).AddChild(page);
		return content;
	}

	static FixedDocument DocumentOf(params double[] widths)
	{
		var document = new FixedDocument();
		foreach (double w in widths)
			document.Pages.Add(PageOf(w, 100));
		return document;
	}

	[Fact]
	public void A_sequence_built_in_code_counts_and_pages_through_its_paginator() => Run(() =>
	{
		var sequence = new FixedDocumentSequence();
		foreach (var document in new[] { DocumentOf(10, 11), DocumentOf(12) })
		{
			var reference = new DocumentReference();
			reference.SetDocument(document);
			sequence.References.Add(reference);
		}
		var paginator = sequence.DocumentPaginator;
		Assert.True(paginator.IsPageCountValid);
		Assert.Equal(3, paginator.PageCount);
		Assert.Equal(12, paginator.GetPage(2).Size.Width);
		Assert.Equal(10, paginator.PageSize.Width);
		Assert.Same(sequence, paginator.Source);
		Assert.Throws<ArgumentOutOfRangeException>(() => paginator.GetPage(3));
		Assert.Throws<ArgumentOutOfRangeException>(() => paginator.GetPage(-1));
	});

	[Fact]
	public void The_document_paginator_returns_the_page_roots() => Run(() =>
	{
		var document = DocumentOf(10, 20);
		var paginator = document.DocumentPaginator;
		Assert.Same(document.Pages[1].Child, paginator.GetPage(1).Visual);
		Assert.Equal(new Size(20, 100), paginator.GetPage(1).Size);
		Assert.Throws<ArgumentOutOfRangeException>(() => paginator.GetPage(2));
		paginator.PageSize = new Size(1, 2);
		Assert.Equal(new Size(1, 2), paginator.PageSize);
	});

	[Fact]
	public void The_paginator_raises_PagesChanged_when_a_page_is_added() => Run(() =>
	{
		var document = DocumentOf(10);
		int raised = 0;
		document.DocumentPaginator.PagesChanged += (s, e) => raised++;
		document.Pages.Add(PageOf(5, 5));
		Assert.Equal(1, raised);
	});

	[Fact]
	public void GetPageRoot_returns_the_child_and_null_without_content() => Run(() =>
	{
		var content = PageOf(10, 10);
		Assert.Same(content.Child, content.GetPageRoot(false));
		Assert.Same(content.Child, content.GetPageRoot(true));
		Assert.Null(new PageContent().GetPageRoot(false));
	});

	[Fact]
	public void A_relative_source_without_a_base_uri_cannot_load() => Run(() =>
	{
		var content = new PageContent { Source = new Uri("Pages/1.fpage", UriKind.Relative) };
		Assert.Throws<InvalidOperationException>(() => content.GetPageRoot(false));
		var reference = new DocumentReference { Source = new Uri("a.fdoc", UriKind.Relative) };
		Assert.Throws<InvalidOperationException>(() => reference.GetDocument(false));
	});

	[Fact]
	public void SetDocument_and_GetDocument_round_trip() => Run(() =>
	{
		var document = DocumentOf(10);
		var reference = new DocumentReference();
		Assert.Null(reference.GetDocument(false));
		reference.SetDocument(document);
		Assert.Same(document, reference.GetDocument(false));
		Assert.Same(document, reference.GetDocument(true));
		Assert.Throws<ArgumentNullException>(() => reference.SetDocument(null!));
		var withSource = new DocumentReference { Source = new Uri("x.fdoc", UriKind.Relative) };
		Assert.Throws<InvalidOperationException>(() => withSource.SetDocument(document));
	});

	[Fact]
	public void IUriContext_round_trips_on_every_fixed_type() => Run(() =>
	{
		var uri = new Uri("http://example.com/a/b.fdoc");
		foreach (IUriContext context in new IUriContext[]
			{ new FixedPage(), new PageContent(), new FixedDocument(), new DocumentReference(), new FixedDocumentSequence() })
		{
			Assert.Null(context.BaseUri);
			context.BaseUri = uri;
			Assert.Equal(uri, context.BaseUri);
		}
	});

	[Fact]
	public void AddChild_rejects_wrong_types_and_second_children() => Run(() =>
	{
		var content = PageOf(1, 1);
		Assert.Throws<InvalidOperationException>(() => ((IAddChild)content).AddChild(new FixedPage()));
		Assert.Throws<ArgumentException>(() => ((IAddChild)new PageContent()).AddChild(new Button()));
		Assert.Throws<ArgumentException>(() => ((IAddChild)new FixedDocument()).AddChild(new FixedPage()));
		Assert.Throws<ArgumentException>(() => ((IAddChild)new FixedDocumentSequence()).AddChild(new PageContent()));
		Assert.Throws<ArgumentNullException>(() => ((IAddChild)new FixedDocument()).AddChild(null!));
		((IAddChild)new FixedDocument()).AddText("  ");
		Assert.Throws<ArgumentException>(() => ((IAddChild)new FixedDocument()).AddText("x"));
	});

	[Fact]
	public void Collections_validate_arguments() => Run(() =>
	{
		var document = new FixedDocument();
		Assert.Throws<ArgumentNullException>(() => document.Pages.Add(null!));
		Assert.Throws<ArgumentOutOfRangeException>(() => document.Pages[0].ToString());
		var sequence = new FixedDocumentSequence();
		Assert.Throws<ArgumentNullException>(() => sequence.References.Add(null!));
		var reference = new DocumentReference();
		sequence.References.Add(reference);
		Assert.Equal(0, sequence.References.IndexOf(reference));
		Assert.True(sequence.References.Remove(reference));
		Assert.Empty(sequence.References);
	});

	[Fact]
	public void FixedPage_attached_properties_have_WPF_defaults() => Run(() =>
	{
		var element = new Button();
		Assert.True(double.IsNaN(FixedPage.GetLeft(element)));
		FixedPage.SetLeft(element, 4);
		FixedPage.SetBottom(element, 7);
		Assert.Equal(4, FixedPage.GetLeft(element));
		Assert.Equal(7, FixedPage.GetBottom(element));
		Assert.Null(FixedPage.GetNavigateUri(element));
		FixedPage.SetNavigateUri(element, new Uri("http://x/"));
		Assert.Equal("http://x/", FixedPage.GetNavigateUri(element)!.OriginalString);
		var page = new FixedPage();
		page.PrintTicket = "ticket";
		Assert.Equal("ticket", page.PrintTicket);
		Assert.Throws<ArgumentNullException>(() => FixedPage.GetTop(null!));
	});

	[Fact]
	public void A_DocumentViewer_shows_page_one_of_a_FixedDocument() => Run(() =>
	{
		var document = DocumentOf(200, 300);
		document.Pages[0].Child!.Children.Add(new Glyphs { UnicodeString = "One", FontRenderingEmSize = 16, OriginY = 20 });
		var viewer = new DocumentViewer { Document = document };
		Assert.Equal(2, viewer.PageCount);
		Assert.Equal(1, viewer.MasterPageNumber);
		Assert.NotNull(viewer.Widget);
		Assert.NotNull(document.Pages[0].Child!.Widget.Parent);
		Assert.NotNull(document.Pages[1].Child!.Widget.Parent);
	});

	[Fact]
	public void A_DocumentViewer_shows_a_FixedDocumentSequence() => Run(() =>
	{
		var sequence = new FixedDocumentSequence();
		var reference = new DocumentReference();
		reference.SetDocument(DocumentOf(100));
		sequence.References.Add(reference);
		var viewer = new DocumentViewer { Document = sequence };
		Assert.Equal(1, viewer.PageCount);
		Assert.Same(sequence, viewer.Document);
	});
}
