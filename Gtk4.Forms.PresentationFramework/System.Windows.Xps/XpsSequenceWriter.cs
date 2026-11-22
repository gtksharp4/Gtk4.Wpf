using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.Linq;

namespace System.Windows.Xps.Packaging;

/// <summary>Collects the parts of a new package for a fixed document sequence, then writes them.</summary>
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

	void CopyPage(XpsPackage source, string sourcePart, string pagePart)
	{
		string xml = Encoding.UTF8.GetString(source.ReadPart(sourcePart)).TrimStart('﻿');
		var rels = new XElement(RelNs + "Relationships");
		foreach (var rel in source.Relationships(sourcePart))
		{
			string target = AddResource(source, rel.Target, out string? renamed);
			if (renamed != null)
				xml = xml.Replace(rel.Target, renamed);
			rels.Add(new XElement(RelNs + "Relationship", new XAttribute("Type", rel.Type),
				new XAttribute("Target", target), new XAttribute("Id", rel.Id)));
		}
		_parts[pagePart] = (new UTF8Encoding(false).GetBytes(xml), XpsNames.PageType);
		AddRelationships(pagePart, rels);
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
