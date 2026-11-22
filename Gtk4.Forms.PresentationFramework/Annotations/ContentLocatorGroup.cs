using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Windows.Annotations;

/// <summary>Alternative <see cref="ContentLocator"/>s that all describe the same content.</summary>
[XmlRoot(Namespace = AnnotationXmlConstants.CoreNamespace, ElementName = AnnotationXmlConstants.ContentLocatorGroup)]
public sealed class ContentLocatorGroup : ContentLocatorBase, IXmlSerializable
{
	readonly OwnedCollection<ContentLocator> _locators = new OwnedCollection<ContentLocator>();

	/// <summary>Creates an empty group.</summary>
	public ContentLocatorGroup() => _locators.CollectionChanged += (s, e) => FireChanged(nameof(Locators));

	/// <summary>The locators of the group.</summary>
	public Collection<ContentLocator> Locators => _locators;

	/// <inheritdoc/>
	public override object Clone()
	{
		var copy = new ContentLocatorGroup();
		foreach (var locator in Locators) copy.Locators.Add((ContentLocator)locator.Clone());
		return copy;
	}

	internal override IEnumerable<ContentLocator> Flatten() => Locators;

	/// <inheritdoc/>
	public XmlSchema? GetSchema() => null;

	/// <inheritdoc/>
	public void WriteXml(XmlWriter writer)
	{
		if (writer == null) throw new ArgumentNullException(nameof(writer));
		if (string.IsNullOrEmpty(writer.LookupPrefix(AnnotationXmlConstants.CoreNamespace)))
			writer.WriteAttributeString("xmlns", AnnotationXmlConstants.CorePrefix, null,
				AnnotationXmlConstants.CoreNamespace);
		foreach (var locator in Locators) locator.WriteElement(writer);
	}

	/// <inheritdoc/>
	public void ReadXml(XmlReader reader)
	{
		if (reader == null) throw new ArgumentNullException(nameof(reader));
		ReadFrom((XmlElement)new XmlDocument().ReadNode(reader)!);
	}

	internal override void WriteElement(XmlWriter writer)
	{
		writer.WriteStartElement(AnnotationXmlConstants.CorePrefix, AnnotationXmlConstants.ContentLocatorGroup,
			AnnotationXmlConstants.CoreNamespace);
		WriteXml(writer);
		writer.WriteEndElement();
	}

	internal void ReadFrom(XmlElement element)
	{
		foreach (var node in element.ChildNodes)
		{
			if (!(node is XmlElement child)) continue;
			if (!(ReadElement(child) is ContentLocator locator))
				throw new XmlException("Invalid content in 'ContentLocatorGroup'.");
			Locators.Add(locator);
		}
	}
}
