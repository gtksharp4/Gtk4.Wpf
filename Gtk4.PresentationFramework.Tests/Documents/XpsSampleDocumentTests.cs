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

namespace Gtk4.PresentationFramework.Tests;

public class XpsSampleDocumentTests : WpfGtkTestBase
{
	[Fact]
	public void The_sample_packages_read_when_present() => Run(() =>
	{
		string? dir = FindContent();
		Assert.SkipWhen(dir == null, "WPF-Samples is not checked out.");
		foreach (var (file, documents) in new[] { ("chocolate.xps", 1), ("officeinhealthcare.xps", 2) })
		{
			using var xps = new XpsDocument(System.IO.Path.Combine(dir!, file), FileAccess.Read);
			var sequence = xps.GetFixedDocumentSequence()!;
			Assert.Equal(documents, sequence.References.Count);
			var first = sequence.References[0].GetDocument(false)!;
			Assert.True(first.Pages.Count >= 4);
			var page = first.Pages[0].GetPageRoot(false)!;
			Assert.True(page.Width > 0);
			Assert.Contains(page.Children.OfType<UIElement>(), c => c is Glyphs);
		}
	});

	[Fact]
	public void The_sample_packages_merge_and_round_trip_when_present() => Run(() =>
	{
		string? dir = FindContent();
		Assert.SkipWhen(dir == null, "WPF-Samples is not checked out.");
		using var a = new XpsDocument(System.IO.Path.Combine(dir!, "chocolate.xps"), FileAccess.Read);
		using var b = new XpsDocument(System.IO.Path.Combine(dir!, "officeinhealthcare.xps"), FileAccess.Read);
		var merged = new FixedDocumentSequence();
		var references = a.GetFixedDocumentSequence()!.References.Concat(b.GetFixedDocumentSequence()!.References);
		foreach (var reference in references)
		{
			var copy = new DocumentReference { Source = reference.Source };
			((IUriContext)copy).BaseUri = ((IUriContext)reference).BaseUri;
			merged.References.Add(copy);
		}
		int expected = merged.DocumentPaginator.PageCount;
		string target = XpsTestPackage.TempPath();
		using (var output = new XpsDocument(target, FileAccess.ReadWrite))
			XpsDocument.CreateXpsDocumentWriter(output).Write(merged);
		using var read = new XpsDocument(target, FileAccess.Read);
		Assert.Equal(expected, read.GetFixedDocumentSequence()!.DocumentPaginator.PageCount);
		Assert.True(expected > 8);
		read.Close();
		File.Delete(target);
	});

	static string? FindContent()
	{
		for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent)
		{
			string candidate = System.IO.Path.Combine(d.FullName, "WPF-Samples", "Documents", "Fixed Documents",
				"DocumentMerge", "content");
			if (Directory.Exists(candidate))
				return candidate;
		}
		return null;
	}
}
