using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml.Linq;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Copying a page rewrites its resource references for the new location of the page.</summary>
public class XpsCopyPageTests : WpfGtkTestBase
{
	const string Xps = "http://schemas.microsoft.com/xps/2005/06";
	const string Rels = "http://schemas.openxmlformats.org/package/2006/relationships";

	static string Relationships(params (string Id, string Target)[] targets) =>
		"<Relationships xmlns=\"" + Rels + "\">" + string.Concat(targets.Select(t => "<Relationship Type=\"" + Xps
			+ "/required-resource\" Target=\"" + t.Target + "\" Id=\"" + t.Id + "\" />")) + "</Relationships>";

	// The page sits at /Pages/1.fpage, one level above where the writer puts it.
	static string CreateShallowPackage(string path, string pageXml, params (string Id, string Target)[] rels)
	{
		var entries = new Dictionary<string, byte[]>
		{
			["_rels/.rels"] = Encoding.UTF8.GetBytes("<Relationships xmlns=\"" + Rels + "\"><Relationship Type=\"" + Xps
				+ "/fixedrepresentation\" Target=\"/FixedDocumentSequence.fdseq\" Id=\"R1\" /></Relationships>"),
			["[Content_Types].xml"] = Encoding.UTF8.GetBytes(
				"<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
				+ "<Default Extension=\"png\" ContentType=\"image/png\" />"
				+ "<Default Extension=\"ttf\" ContentType=\"application/x-font-truetype\" /></Types>"),
			["FixedDocumentSequence.fdseq"] = Encoding.UTF8.GetBytes("<FixedDocumentSequence xmlns=\"" + Xps
				+ "\"><DocumentReference Source=\"Doc/FixedDocument.fdoc\" /></FixedDocumentSequence>"),
			["Doc/FixedDocument.fdoc"] = Encoding.UTF8.GetBytes("<FixedDocument xmlns=\"" + Xps
				+ "\"><PageContent Source=\"../Pages/1.fpage\" /></FixedDocument>"),
			["Pages/1.fpage"] = Encoding.UTF8.GetBytes(pageXml),
			["Pages/_rels/1.fpage.rels"] = Encoding.UTF8.GetBytes(Relationships(rels)),
			["Resources/Images/1.png"] = new byte[] { 1, 2, 3 },
			["Resources/f.ttf"] = new byte[] { 0, 1, 0, 0 },
		};
		using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
		{
			foreach (var kv in entries)
				using (var s = zip.CreateEntry(kv.Key).Open())
					s.Write(kv.Value, 0, kv.Value.Length);
		}
		return path;
	}

	static string PageWithRelativeResources =>
		"<FixedPage xmlns=\"" + Xps + "\" Width=\"100\" Height=\"100\">"
		+ "<Path Data=\"M 0,0 L 10,0 10,10 Z\"><Path.Fill><ImageBrush ImageSource=\"../Resources/Images/1.png\" "
		+ "Viewbox=\"0,0,1,1\" ViewportUnits=\"Absolute\" Viewport=\"0,0,1,1\" /></Path.Fill></Path>"
		+ "<Glyphs OriginX=\"1\" OriginY=\"9\" FontRenderingEmSize=\"9\" UnicodeString=\"A\" "
		+ "FontUri=\"../Resources/f.ttf\" /></FixedPage>";

	[Fact]
	public void A_page_with_relative_resource_uris_still_finds_them_after_the_copy() => Run(() =>
	{
		string source = CreateShallowPackage(XpsTestPackage.TempPath(), PageWithRelativeResources,
			("R1", "../Resources/Images/1.png"), ("R2", "../Resources/f.ttf"));
		string target = XpsTestPackage.TempPath();
		using (var input = new XpsDocument(source, FileAccess.Read))
		using (var output = new XpsDocument(target, FileAccess.ReadWrite))
			XpsDocument.CreateXpsDocumentWriter(output).Write(input.GetFixedDocumentSequence()!);
		using var read = new XpsDocument(target, FileAccess.Read);
		string pagePart = "/Documents/1/Pages/1.fpage";
		Assert.True(read.Package.PartExists(pagePart));
		var page = XDocument.Parse(Encoding.UTF8.GetString(read.Package.ReadPart(pagePart)).TrimStart('\uFEFF'));
		var references = page.Descendants().SelectMany(e => e.Attributes())
			.Where(a => a.Name.LocalName == "ImageSource" || a.Name.LocalName == "FontUri").ToList();
		Assert.Equal(2, references.Count);
		foreach (var attribute in references)
			Assert.True(read.Package.PartExists(XpsPackage.ResolveTarget(pagePart, attribute.Value)),
				attribute.Name.LocalName + "=" + attribute.Value + " does not name a part of the new package");
		foreach (var rel in read.Package.Relationships(pagePart))
			Assert.True(read.Package.PartExists(rel.Target));
		read.Close();
		File.Delete(source);
		File.Delete(target);
	});

	[Fact]
	public void Absolute_uris_and_the_relationships_follow_a_renamed_resource() => Run(() =>
	{
		string a = CreateShallowPackage(XpsTestPackage.TempPath(),
			PageWithRelativeResources.Replace("../Resources", "/Resources"),
			("R1", "/Resources/Images/1.png"), ("R2", "/Resources/f.ttf"));
		string b = CreateShallowPackage(XpsTestPackage.TempPath(),
			PageWithRelativeResources.Replace("../Resources", "/Resources"),
			("R1", "/Resources/Images/1.png"), ("R2", "/Resources/f.ttf"));
		string target = XpsTestPackage.TempPath();
		using (var one = new XpsDocument(a, FileAccess.Read))
		using (var two = new XpsDocument(b, FileAccess.Read))
		using (var output = new XpsDocument(target, FileAccess.ReadWrite))
		{
			var merged = new FixedDocumentSequence();
			foreach (var xps in new[] { one, two })
			{
				var reference = xps.GetFixedDocumentSequence()!.References[0];
				var copy = new DocumentReference { Source = reference.Source };
				((System.Windows.Markup.IUriContext)copy).BaseUri =
					((System.Windows.Markup.IUriContext)reference).BaseUri;
				merged.References.Add(copy);
			}
			XpsDocument.CreateXpsDocumentWriter(output).Write(merged);
		}
		using var read = new XpsDocument(target, FileAccess.Read);
		Assert.Equal(2, read.GetFixedDocumentSequence()!.DocumentPaginator.PageCount);
		Assert.True(read.Package.PartExists("/Resources/Images/1.png"));
		read.Close();
		File.Delete(a);
		File.Delete(b);
		File.Delete(target);
	});
}
