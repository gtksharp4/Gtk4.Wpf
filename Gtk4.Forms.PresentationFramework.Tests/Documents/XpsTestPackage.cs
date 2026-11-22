using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Builds small XPS packages (ZIP files) for the fixed document tests.</summary>
static class XpsTestPackage
{
	const string Xps = "http://schemas.microsoft.com/xps/2005/06";
	const string Rels = "http://schemas.openxmlformats.org/package/2006/relationships";

	internal const string PageXml =
		"<FixedPage xmlns=\"" + Xps + "\" xmlns:x=\"" + Xps + "/resourcedictionary-key\" Width=\"816\" Height=\"1056\">"
		+ "<FixedPage.Resources><ResourceDictionary><SolidColorBrush x:Key=\"b0\" Color=\"#FFFF0000\" />"
		+ "</ResourceDictionary></FixedPage.Resources>"
		+ "<Path Data=\"M 0,0 L 10,0 10,10 Z\" Fill=\"{StaticResource b0}\" />"
		+ "<Canvas RenderTransform=\"1,0,0,1,5,5\"><Glyphs OriginX=\"10\" OriginY=\"20\" FontRenderingEmSize=\"12\" "
		+ "UnicodeString=\"Hi\" Fill=\"#FF000000\" FontUri=\"/Resources/f.ttf\" /></Canvas></FixedPage>";

	/// <summary>Creates a package with <paramref name="pagesPerDocument"/> pages in each of its documents.</summary>
	internal static void Create(string path, params int[] pagesPerDocument)
	{
		var entries = new Dictionary<string, string>
		{
			["_rels/.rels"] = "<Relationships xmlns=\"" + Rels + "\"><Relationship Type=\"" + Xps
				+ "/fixedrepresentation\" Target=\"/FixedDocumentSequence.fdseq\" Id=\"R1\" /></Relationships>",
			["[Content_Types].xml"] = "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
				+ "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\" />"
				+ "<Default Extension=\"ttf\" ContentType=\"application/x-font-truetype\" /></Types>",
		};
		var sequence = new StringBuilder("<FixedDocumentSequence xmlns=\"" + Xps + "\">");
		for (int d = 1; d <= pagesPerDocument.Length; d++)
		{
			sequence.Append("<DocumentReference Source=\"Documents/" + d + "/FixedDocument.fdoc\" />");
			var document = new StringBuilder("<FixedDocument xmlns=\"" + Xps + "\">");
			for (int p = 1; p <= pagesPerDocument[d - 1]; p++)
			{
				document.Append("<PageContent Source=\"Pages/" + p + ".fpage\" />");
				entries["Documents/" + d + "/Pages/" + p + ".fpage"] = PageXml;
				entries["Documents/" + d + "/Pages/_rels/" + p + ".fpage.rels"] = "<Relationships xmlns=\"" + Rels
					+ "\"><Relationship Type=\"" + Xps + "/required-resource\" "
					+ "Target=\"../../../Resources/f.ttf\" Id=\"R1\" /></Relationships>";
			}
			entries["Documents/" + d + "/FixedDocument.fdoc"] = document.Append("</FixedDocument>").ToString();
		}
		entries["FixedDocumentSequence.fdseq"] = sequence.Append("</FixedDocumentSequence>").ToString();
		using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
		{
			foreach (var kv in entries)
				Write(zip, kv.Key, Encoding.UTF8.GetBytes(kv.Value));
			Write(zip, "Resources/f.ttf", new byte[] { 0, 1, 0, 0, 0, 0 });
		}
	}

	static void Write(ZipArchive zip, string name, byte[] data)
	{
		using (var s = zip.CreateEntry(name).Open())
			s.Write(data, 0, data.Length);
	}

	internal static string TempPath() => Path.Combine(Path.GetTempPath(), "gf57b-" + Guid.NewGuid().ToString("N") + ".xps");
}
