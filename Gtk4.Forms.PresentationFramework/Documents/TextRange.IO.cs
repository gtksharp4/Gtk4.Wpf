using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace System.Windows.Documents;

public partial class TextRange
{
	const string PackageDocumentPart = "Xaml/Document.xaml";

	static bool IsKnownFormat(string? dataFormat) =>
		dataFormat == DataFormats.Text || dataFormat == DataFormats.UnicodeText || dataFormat == DataFormats.Xaml
		|| dataFormat == DataFormats.Rtf || dataFormat == DataFormats.XamlPackage;

	/// <summary>True when <paramref name="dataFormat"/> can be loaded (Text, Xaml, Rtf, ...).</summary>
	public bool CanLoad(string dataFormat)
	{
		if (dataFormat == null) throw new ArgumentNullException(nameof(dataFormat));
		return IsKnownFormat(dataFormat);
	}

	/// <summary>True when <paramref name="dataFormat"/> can be saved (Text, Xaml, Rtf, ...).</summary>
	public bool CanSave(string dataFormat)
	{
		if (dataFormat == null) throw new ArgumentNullException(nameof(dataFormat));
		return IsKnownFormat(dataFormat);
	}

	/// <summary>Replaces the range content with the stream content in the given format.</summary>
	/// <exception cref="ArgumentException">The format is not supported, or the data cannot be read.</exception>
	public void Load(Stream stream, string dataFormat)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		if (dataFormat == null) throw new ArgumentNullException(nameof(dataFormat));
		if (!IsKnownFormat(dataFormat))
			throw new ArgumentException($"The data format '{dataFormat}' is not supported.", nameof(dataFormat));
		var paragraphs = Parse(stream, dataFormat);
		if (WholeOwner && _owner!.Document == null) _owner.Document = new FlowDocument();
		var doc = Start.Document;
		int s = Start.Offset, e = End.Offset;
		doc.EditBatch(() => TextTreeEdit.ReplaceContent(doc, s, e, paragraphs));
		Changed?.Invoke(this, EventArgs.Empty);
	}

	static List<Paragraph> Parse(Stream stream, string format)
	{
		if (format == DataFormats.XamlPackage) return TextRangeXamlReader.Read(ReadPackage(stream));
		var encoding = format == DataFormats.UnicodeText ? Encoding.Unicode : new UTF8Encoding(false);
		string content = ReadString(stream, encoding);
		if (format == DataFormats.Xaml) return TextRangeXamlReader.Read(content);
		if (format == DataFormats.Rtf) return TextRangeRtfReader.Read(content);
		var lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();
		if (lines.Count > 0 && lines[lines.Count - 1].Length == 0) lines.RemoveAt(lines.Count - 1);
		return lines.Select(l =>
		{
			var p = new Paragraph();
			if (l.Length > 0) p.Inlines.Add(new Run { Text = l });
			return p;
		}).ToList();
	}

	static string ReadString(Stream stream, Encoding encoding)
	{
		using (var reader = new StreamReader(stream, encoding, true, 1024, leaveOpen: true))
			return reader.ReadToEnd();
	}

	/// <summary>Writes the range content in the given format.</summary>
	/// <exception cref="ArgumentException">The format is not supported.</exception>
	public void Save(Stream stream, string dataFormat) => Save(stream, dataFormat, false);

	/// <summary>
	/// Writes the range content in the given format. <paramref name="preserveTextElements"/> is accepted for
	/// source compatibility: the XAML output always keeps its Paragraph and Run elements.
	/// </summary>
	public void Save(Stream stream, string dataFormat, bool preserveTextElements)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		if (dataFormat == null) throw new ArgumentNullException(nameof(dataFormat));
		if (!IsKnownFormat(dataFormat))
			throw new ArgumentException($"The data format '{dataFormat}' is not supported.", nameof(dataFormat));
		if (WholeOwner && _owner!.Document == null) _owner.Document = new FlowDocument();
		var paragraphs = TextRangeExtract.Extract(Start.Document, Start.Offset, End.Offset);
		if (dataFormat == DataFormats.Text || dataFormat == DataFormats.UnicodeText)
		{
			Encoding enc = dataFormat == DataFormats.Text ? new UTF8Encoding(false) : new UnicodeEncoding(false, false);
			WriteString(stream, enc, Read(Start.Document, Start.Offset, End.Offset));
		}
		else if (dataFormat == DataFormats.Rtf) WriteString(stream, Encoding.ASCII, TextRangeRtf.Write(paragraphs));
		else if (dataFormat == DataFormats.Xaml)
			WriteString(stream, new UTF8Encoding(false), TextRangeXaml.Write(paragraphs));
		else WritePackage(stream, TextRangeXaml.Write(paragraphs));
	}

	static void WriteString(Stream stream, Encoding encoding, string text)
	{
		var bytes = encoding.GetBytes(text);
		stream.Write(bytes, 0, bytes.Length);
	}

	// XamlPackage is an OPC zip: [Content_Types].xml, _rels/.rels pointing at the entry part, and
	// Xaml/Document.xaml. System.IO.Packaging is not referenced (it is not part of netstandard2.0), so the
	// container is written directly. Only the XAML part is carried: images and other resources are not.
	// The relationship type is WPF's WpfPayload.XamlRelationshipFromPackageToEntryPart; its value is taken from
	// memory, not verified against the dotnet/wpf source.
	const string EntryRelationshipType = "http://schemas.microsoft.com/wpf/2005/10/xaml/entry";
	const string RelsNs = "http://schemas.openxmlformats.org/package/2006/relationships";

	static void WritePackage(Stream stream, string xaml)
	{
		using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
		{
			Entry(zip, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?><Types xmlns=\"http://schemas."
				+ "openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" "
				+ "ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default "
				+ "Extension=\"xaml\" ContentType=\"application/"
				+ "vnd.ms-wpf.xaml+xml\"/></Types>");
			Entry(zip, "_rels/.rels", "<?xml version=\"1.0\" encoding=\"utf-8\"?><Relationships xmlns=\"" + RelsNs
				+ "\"><Relationship Id=\"R0\" Type=\"" + EntryRelationshipType + "\" Target=\"/" + PackageDocumentPart
				+ "\"/></Relationships>");
			Entry(zip, PackageDocumentPart, xaml);
		}
	}

	static void Entry(ZipArchive zip, string name, string content)
	{
		using (var s = zip.CreateEntry(name).Open())
			WriteString(s, new UTF8Encoding(false), content);
	}

	static ZipArchiveEntry? Find(ZipArchive zip, string name) => zip.Entries.FirstOrDefault(x =>
		string.Equals(x.FullName.TrimStart('/'), name.TrimStart('/'), StringComparison.OrdinalIgnoreCase));

	// The entry part is the target of the entry relationship in _rels/.rels, else Xaml/Document.xaml.
	static string EntryPartName(ZipArchive zip)
	{
		var rels = Find(zip, "_rels/.rels");
		if (rels == null) return PackageDocumentPart;
		try
		{
			using (var s = rels.Open())
			{
				var target = System.Xml.Linq.XDocument.Load(s).Descendants()
					.Where(e => e.Name.LocalName == "Relationship"
						&& (string?)e.Attribute("Type") == EntryRelationshipType)
					.Select(e => (string?)e.Attribute("Target")).FirstOrDefault();
				return string.IsNullOrEmpty(target) ? PackageDocumentPart : target!;
			}
		}
		catch (System.Xml.XmlException)
		{
			return PackageDocumentPart;
		}
	}

	static string ReadPackage(Stream stream)
	{
		try
		{
			using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
			{
				var entry = Find(zip, EntryPartName(zip)) ?? Find(zip, PackageDocumentPart);
				if (entry == null) throw new ArgumentException("The XamlPackage has no entry part.");
				using (var s = entry.Open()) return ReadString(s, new UTF8Encoding(false));
			}
		}
		catch (InvalidDataException ex)
		{
			throw new ArgumentException("The data is not a XamlPackage.", ex);
		}
	}
}
