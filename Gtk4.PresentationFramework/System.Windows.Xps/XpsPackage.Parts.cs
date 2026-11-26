using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace System.Windows.Xps.Packaging;

/// <summary>A package relationship, with its target resolved to an absolute part name.</summary>
sealed class XpsRelationship
{
	internal XpsRelationship(string id, string type, string target)
	{
		Id = id;
		Type = type;
		Target = target;
	}

	internal string Id { get; }

	internal string Type { get; }

	internal string Target { get; }
}

sealed partial class XpsPackage
{
	/// <summary>Relationships of <paramref name="partName"/> (<c>/</c> for the package itself).</summary>
	internal IEnumerable<XpsRelationship> Relationships(string partName)
	{
		string part = NormalizePart(partName);
		int slash = part.LastIndexOf('/');
		string rels = part == "/"
			? XpsNames.RootRelationships
			: part.Substring(0, slash + 1) + "_rels/" + part.Substring(slash + 1) + ".rels";
		if (!PartExists(rels))
			return Enumerable.Empty<XpsRelationship>();
		XNamespace ns = XpsNames.Relationships;
		return ReadXml(rels).Root!.Elements(ns + "Relationship")
			.Where(e => !string.Equals((string?)e.Attribute("TargetMode"), "External", StringComparison.Ordinal))
			.Select(e => new XpsRelationship((string?)e.Attribute("Id") ?? string.Empty,
				(string?)e.Attribute("Type") ?? string.Empty,
				ResolveTarget(part, (string?)e.Attribute("Target") ?? string.Empty)))
			.ToList();
	}

	/// <summary>The content type of a part: its override, else the default for its extension.</summary>
	internal string ContentTypeOf(string partName)
	{
		string part = NormalizePart(partName);
		string types = "/" + XpsNames.ContentTypesEntry;
		if (!_entries.ContainsKey(types))
			return "application/octet-stream";
		XNamespace ns = XpsNames.ContentTypes;
		var root = ReadXml(types).Root!;
		var over = root.Elements(ns + "Override")
			.FirstOrDefault(e => string.Equals((string?)e.Attribute("PartName"), part,
				StringComparison.OrdinalIgnoreCase));
		if (over != null)
			return (string?)over.Attribute("ContentType") ?? "application/octet-stream";
		int dot = part.LastIndexOf('.');
		string ext = dot < 0 ? string.Empty : part.Substring(dot + 1);
		var def = root.Elements(ns + "Default")
			.FirstOrDefault(e => string.Equals((string?)e.Attribute("Extension"), ext,
				StringComparison.OrdinalIgnoreCase));
		return (string?)def?.Attribute("ContentType") ?? "application/octet-stream";
	}

	/// <summary>Removes every part; the package is then rebuilt with <see cref="WriteParts"/>.</summary>
	internal void Clear()
	{
		if (!CanWrite)
			throw new InvalidOperationException("The XPS document was opened read-only.");
		foreach (var entry in _zip!.Entries.ToList())
			entry.Delete();
		IndexEntries();
	}

	/// <summary>Writes <paramref name="parts"/> (name to bytes and content type) plus the content types part.</summary>
	internal void WriteParts(IDictionary<string, (byte[] Data, string ContentType)> parts)
	{
		XNamespace ns = XpsNames.ContentTypes;
		var types = new XElement(ns + "Types");
		var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var kv in parts)
		{
			int dot = kv.Key.LastIndexOf('.');
			string ext = dot < 0 ? string.Empty : kv.Key.Substring(dot + 1);
			bool xml = kv.Value.ContentType == XpsNames.SequenceType || kv.Value.ContentType == XpsNames.DocumentType
				|| kv.Value.ContentType == XpsNames.PageType;
			if (xml)
				types.Add(new XElement(ns + "Override", new XAttribute("PartName", kv.Key),
					new XAttribute("ContentType", kv.Value.ContentType)));
			else if (ext.Length > 0 && extensions.Add(ext))
				types.Add(new XElement(ns + "Default", new XAttribute("Extension", ext),
					new XAttribute("ContentType", kv.Value.ContentType)));
			Add(kv.Key.TrimStart('/'), kv.Value.Data);
		}
		Add(XpsNames.ContentTypesEntry, Serialize(new XDocument(types)));
		IndexEntries();
	}

	void Add(string entryName, byte[] data)
	{
		var entry = _zip!.CreateEntry(entryName, CompressionLevel.Optimal);
		using (var s = entry.Open())
			s.Write(data, 0, data.Length);
	}

	internal static byte[] Serialize(XDocument doc)
	{
		using (var ms = new System.IO.MemoryStream())
		{
			var settings = new System.Xml.XmlWriterSettings { Encoding = new System.Text.UTF8Encoding(false) };
			using (var w = System.Xml.XmlWriter.Create(ms, settings))
				doc.Save(w);
			return ms.ToArray();
		}
	}
}
