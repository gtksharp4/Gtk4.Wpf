using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Windows.Annotations;

public sealed partial class Annotation : IXmlSerializable
{
	/// <inheritdoc/>
	public XmlSchema? GetSchema() => null;

	/// <inheritdoc/>
	public void WriteXml(XmlWriter writer)
	{
		if (writer == null) throw new ArgumentNullException(nameof(writer));
		if (_type == null) throw new InvalidOperationException("The annotation has no type and cannot be serialized.");
		ContentLocator.DeclareNamespaces(writer);
		writer.WriteAttributeString(AnnotationXmlConstants.Id, XmlConvert.ToString(_id));
		writer.WriteAttributeString(AnnotationXmlConstants.CreationTime,
			XmlConvert.ToString(_created, XmlDateTimeSerializationMode.Local));
		writer.WriteAttributeString(AnnotationXmlConstants.LastModificationTime,
			XmlConvert.ToString(_modified, XmlDateTimeSerializationMode.Local));
		writer.WriteStartAttribute(AnnotationXmlConstants.TypeName);
		writer.WriteQualifiedName(_type.Name, _type.Namespace);
		writer.WriteEndAttribute();
		if (_authors.Count > 0)
		{
			writer.WriteStartElement(AnnotationXmlConstants.Authors, AnnotationXmlConstants.CoreNamespace);
			foreach (var author in _authors)
				if (author != null)
					writer.WriteElementString(AnnotationXmlConstants.BasePrefix, AnnotationXmlConstants.StringAuthor,
						AnnotationXmlConstants.BaseNamespace, author);
			writer.WriteEndElement();
		}
		WriteResources(writer, AnnotationXmlConstants.Anchors, _anchors);
		WriteResources(writer, AnnotationXmlConstants.Cargos, _cargos);
	}

	static void WriteResources(XmlWriter writer, string name, System.Collections.Generic.IEnumerable<AnnotationResource> items)
	{
		bool any = false;
		foreach (var item in items)
		{
			if (!any) writer.WriteStartElement(name, AnnotationXmlConstants.CoreNamespace);
			any = true;
			item.WriteElement(writer);
		}
		if (any) writer.WriteEndElement();
	}

	/// <inheritdoc/>
	public void ReadXml(XmlReader reader)
	{
		if (reader == null) throw new ArgumentNullException(nameof(reader));
		ReadFrom((XmlElement)new XmlDocument().ReadNode(reader)!);
	}

	internal static bool IsNamespaceDeclaration(XmlAttribute attribute) =>
		attribute.Prefix == "xmlns" || attribute.Prefix == "xml"
		|| (attribute.Prefix.Length == 0 && attribute.LocalName == "xmlns");

	internal void ReadFrom(XmlElement element)
	{
		_quiet = true;
		try
		{
			ReadAttributes(element);
			foreach (var node in element.ChildNodes)
			{
				if (!(node is XmlElement child)) continue;
				switch (child.LocalName)
				{
					case AnnotationXmlConstants.Anchors: ReadResources(child, _anchors); break;
					case AnnotationXmlConstants.Cargos: ReadResources(child, _cargos); break;
					case AnnotationXmlConstants.Authors: ReadAuthors(child); break;
					default: throw new XmlException("Invalid content in 'Annotation'.");
				}
			}
		}
		finally
		{
			_quiet = false;
		}
	}

	void ReadAttributes(XmlElement element)
	{
		var id = Guid.Empty;
		var created = DateTime.MinValue;
		var modified = DateTime.MinValue;
		XmlQualifiedName? type = null;
		foreach (XmlAttribute attribute in element.Attributes)
		{
			var value = attribute.Value;
			if (IsNamespaceDeclaration(attribute)) continue;
			if (string.IsNullOrEmpty(value)) continue;
			switch (attribute.LocalName)
			{
				case AnnotationXmlConstants.Id: id = XmlConvert.ToGuid(value); break;
				case AnnotationXmlConstants.CreationTime:
					created = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local);
					break;
				case AnnotationXmlConstants.LastModificationTime:
					modified = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local);
					break;
				case AnnotationXmlConstants.TypeName: type = ParseType(element, value); break;
				default: throw new XmlException("Unexpected attribute '" + attribute.LocalName + "' on 'Annotation'.");
			}
		}
		Require(id != Guid.Empty, AnnotationXmlConstants.Id);
		Require(created != DateTime.MinValue, AnnotationXmlConstants.CreationTime);
		Require(modified != DateTime.MinValue, AnnotationXmlConstants.LastModificationTime);
		Require(type != null, AnnotationXmlConstants.TypeName);
		_id = id;
		_created = created;
		_modified = modified;
		_type = type;
	}

	static void Require(bool present, string attribute)
	{
		if (!present) throw new XmlException("The required attribute '" + attribute + "' is missing on 'Annotation'.");
	}

	static XmlQualifiedName ParseType(XmlElement element, string value)
	{
		var bits = value.Split(':');
		if (bits.Length == 1 && bits[0].Trim().Length > 0) return new XmlQualifiedName(bits[0].Trim());
		if (bits.Length == 2 && bits[0].Trim().Length > 0 && bits[1].Trim().Length > 0)
			return new XmlQualifiedName(bits[1].Trim(), element.GetNamespaceOfPrefix(bits[0].Trim()));
		throw new FormatException("Invalid value for the attribute 'Type'.");
	}

	static void ReadResources(XmlElement parent, System.Collections.ObjectModel.Collection<AnnotationResource> into)
	{
		foreach (var node in parent.ChildNodes)
		{
			if (!(node is XmlElement child)) continue;
			var resource = new AnnotationResource();
			resource.ReadFrom(child);
			into.Add(resource);
		}
	}

	void ReadAuthors(XmlElement parent)
	{
		foreach (var node in parent.ChildNodes)
		{
			if (!(node is XmlElement child)) continue;
			if (child.LocalName != AnnotationXmlConstants.StringAuthor) throw new XmlException("Invalid content in 'Annotation'.");
			if (!child.IsEmpty) _authors.Add(child.InnerText);
		}
	}
}
