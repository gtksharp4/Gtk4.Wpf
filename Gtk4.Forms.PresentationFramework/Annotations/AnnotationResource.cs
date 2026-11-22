using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Windows.Annotations;

/// <summary>An anchor or cargo of an <see cref="Annotation"/>: locators plus XML content.</summary>
[XmlRoot(Namespace = AnnotationXmlConstants.CoreNamespace, ElementName = AnnotationXmlConstants.Resource)]
public sealed class AnnotationResource : IXmlSerializable, INotifyPropertyChanged, IOwned
{
	readonly OwnedCollection<ContentLocatorBase> _locators = new OwnedCollection<ContentLocatorBase>();
	readonly NonNullCollection<XmlElement> _contents = new NonNullCollection<XmlElement>();
	PropertyChangedEventHandler? _changed;
	Guid _id;
	string? _name;
	bool _owned;

	/// <summary>Creates a resource with a new id.</summary>
	public AnnotationResource() : this(Guid.NewGuid())
	{
	}

	/// <summary>Creates a named resource with a new id.</summary>
	public AnnotationResource(string name) : this(Guid.NewGuid())
	{
		_name = name ?? throw new ArgumentNullException(nameof(name));
	}

	/// <summary>Creates a resource with the given id.</summary>
	/// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
	public AnnotationResource(Guid id)
	{
		if (id == Guid.Empty) throw new ArgumentException("The id must not be empty.", nameof(id));
		_id = id;
		_locators.CollectionChanged += (s, e) => Fire("Locators");
		_contents.CollectionChanged += (s, e) => Fire("Contents");
	}

	/// <summary>The resource id.</summary>
	public Guid Id => _id;

	/// <summary>The resource name.</summary>
	public string? Name
	{
		get => _name;
		set
		{
			bool changed = _name != value;
			_name = value;
			if (changed) Fire(nameof(Name));
		}
	}

	/// <summary>Locators that identify the content the resource refers to.</summary>
	public Collection<ContentLocatorBase> ContentLocators => _locators;

	/// <summary>The inline XML content.</summary>
	public Collection<XmlElement> Contents => _contents;

	event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
	{
		add => _changed += value;
		remove => _changed -= value;
	}

	bool IOwned.Owned
	{
		get => _owned;
		set => _owned = value;
	}

	void Fire(string name) => _changed?.Invoke(this, new PropertyChangedEventArgs(name));

	/// <inheritdoc/>
	public XmlSchema? GetSchema() => null;

	/// <inheritdoc/>
	public void WriteXml(XmlWriter writer)
	{
		if (writer == null) throw new ArgumentNullException(nameof(writer));
		if (string.IsNullOrEmpty(writer.LookupPrefix(AnnotationXmlConstants.CoreNamespace)))
			writer.WriteAttributeString("xmlns", AnnotationXmlConstants.CorePrefix, null,
				AnnotationXmlConstants.CoreNamespace);
		writer.WriteAttributeString(AnnotationXmlConstants.Id, XmlConvert.ToString(_id));
		if (_name != null) writer.WriteAttributeString(AnnotationXmlConstants.ResourceName, _name);
		foreach (var locator in _locators) locator.WriteElement(writer);
		foreach (var content in _contents) content.WriteTo(writer);
	}

	/// <inheritdoc/>
	public void ReadXml(XmlReader reader)
	{
		if (reader == null) throw new ArgumentNullException(nameof(reader));
		ReadFrom((XmlElement)new XmlDocument().ReadNode(reader)!);
	}

	internal void WriteElement(XmlWriter writer)
	{
		writer.WriteStartElement(AnnotationXmlConstants.CorePrefix, AnnotationXmlConstants.Resource,
			AnnotationXmlConstants.CoreNamespace);
		WriteXml(writer);
		writer.WriteEndElement();
	}

	internal void ReadFrom(XmlElement element)
	{
		var id = Guid.Empty;
		foreach (XmlAttribute attribute in element.Attributes)
		{
			if (Annotation.IsNamespaceDeclaration(attribute)) continue;
			if (attribute.LocalName == AnnotationXmlConstants.Id) id = XmlConvert.ToGuid(attribute.Value);
			else if (attribute.LocalName == AnnotationXmlConstants.ResourceName) _name = attribute.Value;
			else throw new XmlException("Unexpected attribute '" + attribute.LocalName + "' on 'Resource'.");
		}
		if (id == Guid.Empty) throw new XmlException("The required attribute 'Id' is missing on 'Resource'.");
		_id = id;
		foreach (var node in element.ChildNodes)
		{
			if (!(node is XmlElement child)) continue;
			var locator = ContentLocatorBase.ReadElement(child);
			if (locator != null) _locators.Add(locator);
			else _contents.Add(child);
		}
	}
}
