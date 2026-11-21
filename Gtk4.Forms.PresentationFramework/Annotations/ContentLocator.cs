using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Windows.Annotations;

/// <summary>An ordered list of <see cref="ContentLocatorPart"/>s that finds a piece of content.</summary>
[XmlRoot(Namespace = AnnotationXmlConstants.CoreNamespace, ElementName = AnnotationXmlConstants.ContentLocator)]
public sealed class ContentLocator : ContentLocatorBase, IXmlSerializable
{
	readonly OwnedCollection<ContentLocatorPart> _parts = new OwnedCollection<ContentLocatorPart>();

	/// <summary>Creates an empty locator.</summary>
	public ContentLocator() => _parts.CollectionChanged += (s, e) => FireChanged(nameof(Parts));

	/// <summary>The parts, outermost first.</summary>
	public Collection<ContentLocatorPart> Parts => _parts;

	/// <summary>True when this locator begins with parts that match every part of <paramref name="locator"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="locator"/> is null.</exception>
	public bool StartsWith(ContentLocator locator)
	{
		if (locator == null) throw new ArgumentNullException(nameof(locator));
		if (Parts.Count < locator.Parts.Count) return false;
		for (int i = 0; i < locator.Parts.Count; i++)
			if (!locator.Parts[i].Matches(Parts[i])) return false;
		return true;
	}

	/// <inheritdoc/>
	public override object Clone()
	{
		var copy = new ContentLocator();
		foreach (var part in Parts) copy.Parts.Add((ContentLocatorPart)part.Clone());
		return copy;
	}

	internal override IEnumerable<ContentLocator> Flatten()
	{
		yield return this;
	}

	/// <inheritdoc/>
	public XmlSchema? GetSchema() => null;

	/// <inheritdoc/>
	public void WriteXml(XmlWriter writer)
	{
		if (writer == null) throw new ArgumentNullException(nameof(writer));
		DeclareNamespaces(writer);
		foreach (var part in Parts)
		{
			var prefix = writer.LookupPrefix(part.PartType.Namespace);
			if (string.IsNullOrEmpty(prefix)) prefix = "tmp";
			writer.WriteStartElement(prefix, part.PartType.Name, part.PartType.Namespace);
			foreach (var pair in part.NameValuePairs)
			{
				writer.WriteStartElement(AnnotationXmlConstants.Item, AnnotationXmlConstants.CoreNamespace);
				writer.WriteAttributeString(AnnotationXmlConstants.ItemName, pair.Key);
				writer.WriteAttributeString(AnnotationXmlConstants.ItemValue, pair.Value);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
	}

	/// <inheritdoc/>
	public void ReadXml(XmlReader reader)
	{
		if (reader == null) throw new ArgumentNullException(nameof(reader));
		ReadFrom((XmlElement)new XmlDocument().ReadNode(reader)!);
	}

	internal static void DeclareNamespaces(XmlWriter writer)
	{
		if (string.IsNullOrEmpty(writer.LookupPrefix(AnnotationXmlConstants.CoreNamespace)))
			writer.WriteAttributeString("xmlns", AnnotationXmlConstants.CorePrefix, null,
				AnnotationXmlConstants.CoreNamespace);
		if (string.IsNullOrEmpty(writer.LookupPrefix(AnnotationXmlConstants.BaseNamespace)))
			writer.WriteAttributeString("xmlns", AnnotationXmlConstants.BasePrefix, null,
				AnnotationXmlConstants.BaseNamespace);
	}

	internal override void WriteElement(XmlWriter writer)
	{
		writer.WriteStartElement(AnnotationXmlConstants.CorePrefix, AnnotationXmlConstants.ContentLocator,
			AnnotationXmlConstants.CoreNamespace);
		WriteXml(writer);
		writer.WriteEndElement();
	}

	internal void ReadFrom(XmlElement element)
	{
		foreach (var node in element.ChildNodes)
		{
			if (!(node is XmlElement partElement)) continue;
			var part = new ContentLocatorPart(new XmlQualifiedName(partElement.LocalName, partElement.NamespaceURI));
			foreach (var child in partElement.ChildNodes)
			{
				if (!(child is XmlElement item)) continue;
				if (item.LocalName != AnnotationXmlConstants.Item
					|| item.NamespaceURI != AnnotationXmlConstants.CoreNamespace)
					throw new XmlException("Invalid content in the locator part '" + part.PartType.Name + "'.");
				if (!item.HasAttribute(AnnotationXmlConstants.ItemName))
					throw new XmlException("The required attribute 'Name' is missing on 'Item'.");
				if (!item.HasAttribute(AnnotationXmlConstants.ItemValue))
					throw new XmlException("The required attribute 'Value' is missing on 'Item'.");
				part.NameValuePairs.Add(item.GetAttribute(AnnotationXmlConstants.ItemName),
					item.GetAttribute(AnnotationXmlConstants.ItemValue));
			}
			Parts.Add(part);
		}
	}
}
