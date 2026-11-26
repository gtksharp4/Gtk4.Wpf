using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.Linq;

namespace System.Windows.Xps.Packaging;

/// <summary>
/// Collects the parts of a new package for a fixed document sequence, then writes them. A copied page keeps its XML
/// but its ImageSource, FontUri and Source attributes are rewritten as absolute part names of the new package.
/// </summary>
/// <remarks>
/// Gap: resource dictionary and other resource parts are copied verbatim; references inside them, and their own
/// relationships, are not rewritten.
/// </remarks>
sealed class XpsSequenceWriter
{
	static readonly XNamespace Ns = XpsNames.Markup;
	static readonly XNamespace RelNs = XpsNames.Relationships;
	readonly SortedDictionary<string, (byte[] Data, string ContentType)> _parts =
		new SortedDictionary<string, (byte[], string)>(StringComparer.Ordinal);
	readonly XElement _sequence = new XElement(Ns + "FixedDocumentSequence");
	int _documents;

	internal void AddDocument(FixedDocument? document)
	{
		if (document == null)
			return;
		int number = ++_documents;
		var root = new XElement(Ns + "FixedDocument");
		int page = 0;
		foreach (var content in document.Pages)
			AddPage(root, number, ++page, content);
		string docPart = "/Documents/" + number + "/FixedDocument.fdoc";
		_parts[docPart] = (XpsPackage.Serialize(new XDocument(root)), XpsNames.DocumentType);
		_sequence.Add(new XElement(Ns + "DocumentReference", new XAttribute("Source", docPart.TrimStart('/'))));
	}

	void AddPage(XElement document, int number, int page, PageContent content)
	{
		string pagePart = "/Documents/" + number + "/Pages/" + page + ".fpage";
		document.Add(new XElement(Ns + "PageContent", new XAttribute("Source", "Pages/" + page + ".fpage")));
		var source = content.Source;
		if (source != null && XpsPackage.TryResolve(FixedPartLoader.Resolve(((IUriContext)content).BaseUri, source),
			out var package, out string part))
		{
			CopyPage(package!, part, pagePart);
			return;
		}
		var root = content.GetPageRoot(false) ?? throw new InvalidOperationException("The page has no content.");
		if (root.SourcePart != null && XpsPackage.TryResolve(root.SourcePart, out package, out part))
			CopyPage(package!, part, pagePart);
		else
			SerializePage(root, pagePart);
	}

	// Attributes that name a package part. Their values are resolved against the source page and written back as
	// the absolute part name of the new package.
	static readonly string[] UriAttributes = { "ImageSource", "FontUri", "Source" };

	void CopyPage(XpsPackage source, string sourcePart, string pagePart)
	{
		var xml = XDocument.Parse(Encoding.UTF8.GetString(source.ReadPart(sourcePart)).TrimStart('\uFEFF'));
		var rels = new XElement(RelNs + "Relationships");
		var targets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (var rel in source.Relationships(sourcePart))
		{
			targets[rel.Target] = AddResource(source, rel.Target, out _);
			rels.Add(new XElement(RelNs + "Relationship", new XAttribute("Type", rel.Type),
				new XAttribute("Target", targets[rel.Target]), new XAttribute("Id", rel.Id)));
		}
		foreach (var attribute in xml.Descendants().SelectMany(e => e.Attributes()).ToList())
			RewriteReference(source, sourcePart, attribute, targets, rels);
		_parts[pagePart] = (XpsPackage.Serialize(xml), XpsNames.PageType);
		AddRelationships(pagePart, rels);
	}

	void RewriteReference(XpsPackage source, string sourcePart, XAttribute attribute,
		Dictionary<string, string> targets, XElement rels)
	{
		string value = attribute.Value;
		if (!UriAttributes.Contains(attribute.Name.LocalName) || value.Length == 0 || value[0] == '{')
			return;
		string uri = value.Split(' ')[0];
		string part = XpsPackage.ResolveTarget(sourcePart, uri);
		if (!source.PartExists(part))
			return;
		if (!targets.TryGetValue(part, out string? target))
		{
			target = targets[part] = AddResource(source, part, out _);
			rels.Add(new XElement(RelNs + "Relationship", new XAttribute("Type", XpsNames.RequiredResource),
				new XAttribute("Target", target), new XAttribute("Id", "R" + Guid.NewGuid().ToString("N"))));
		}
		attribute.Value = target + value.Substring(uri.Length);
	}

	// A resource keeps its path unless another package already put different bytes there.
	string AddResource(XpsPackage source, string part, out string? renamed)
	{
		renamed = null;
		var data = source.ReadPart(part);
		string target = part;
		for (int n = 1; _parts.TryGetValue(target, out var existing) && !existing.Data.SequenceEqual(data); n++)
		{
			int slash = part.LastIndexOf('/');
			target = part.Substring(0, slash + 1) + "r" + n + "-" + part.Substring(slash + 1);
			renamed = target;
		}
		_parts[target] = (data, source.ContentTypeOf(part));
		return target;
	}

	void SerializePage(FixedPage page, string pagePart)
	{
		var rels = new XElement(RelNs + "Relationships");
		var xml = XpsPageWriter.Write(page, (package, font) =>
		{
			string target = AddResource(package, font, out _);
			rels.Add(new XElement(RelNs + "Relationship", new XAttribute("Type", XpsNames.RequiredResource),
				new XAttribute("Target", target), new XAttribute("Id", "R" + (rels.Elements().Count() + 1))));
		});
		_parts[pagePart] = (XpsPackage.Serialize(new XDocument(xml)), XpsNames.PageType);
		if (rels.HasElements)
			AddRelationships(pagePart, rels);
	}

	void AddRelationships(string part, XElement rels)
	{
		int slash = part.LastIndexOf('/');
		_parts[part.Substring(0, slash + 1) + "_rels/" + part.Substring(slash + 1) + ".rels"] =
			(XpsPackage.Serialize(new XDocument(rels)), XpsNames.RelationshipsType);
	}

	internal void Save(XpsPackage package)
	{
		_parts["/FixedDocumentSequence.fdseq"] = (XpsPackage.Serialize(new XDocument(_sequence)),
			XpsNames.SequenceType);
		var rootRels = new XElement(RelNs + "Relationships",
			new XElement(RelNs + "Relationship", new XAttribute("Type", XpsNames.FixedRepresentation),
				new XAttribute("Target", "/FixedDocumentSequence.fdseq"), new XAttribute("Id", "R1")));
		_parts[XpsNames.RootRelationships] =
			(XpsPackage.Serialize(new XDocument(rootRels)), XpsNames.RelationshipsType);
		package.Clear();
		package.WriteParts(_parts);
	}
}
