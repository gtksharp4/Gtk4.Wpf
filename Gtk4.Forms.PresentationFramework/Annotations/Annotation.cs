using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Serialization;

namespace System.Windows.Annotations;

/// <summary>One annotation: a type, authors, anchors (what it is attached to) and cargos (its content).</summary>
[XmlRoot(Namespace = AnnotationXmlConstants.CoreNamespace, ElementName = AnnotationXmlConstants.Annotation)]
public sealed partial class Annotation
{
	readonly OwnedCollection<AnnotationResource> _anchors = new OwnedCollection<AnnotationResource>();
	readonly OwnedCollection<AnnotationResource> _cargos = new OwnedCollection<AnnotationResource>();
	readonly ObservableCollection<string> _authors = new ObservableCollection<string>();
	Guid _id;
	XmlQualifiedName? _type;
	DateTime _created;
	DateTime _modified;
	bool _quiet;

	/// <summary>Creates an empty annotation; used by serialization.</summary>
	public Annotation()
	{
		_created = DateTime.MinValue;
		_modified = DateTime.MinValue;
		Hook();
	}

	/// <summary>Creates an annotation of the given type with a new id, stamped now.</summary>
	public Annotation(XmlQualifiedName annotationType)
	{
		CheckType(annotationType);
		_id = Guid.NewGuid();
		_type = annotationType;
		_created = DateTime.Now;
		_modified = _created;
		Hook();
	}

	/// <summary>Creates an annotation with an explicit id and timestamps.</summary>
	public Annotation(XmlQualifiedName annotationType, Guid id, DateTime creationTime, DateTime lastModificationTime)
	{
		CheckType(annotationType);
		if (id == Guid.Empty) throw new ArgumentException("The id must not be empty.", nameof(id));
		if (lastModificationTime < creationTime)
			throw new ArgumentException("The modification time precedes the creation time.", nameof(lastModificationTime));
		_id = id;
		_type = annotationType;
		_created = creationTime;
		_modified = lastModificationTime;
		Hook();
	}

	/// <summary>Raised when an author is added or removed.</summary>
	public event AnnotationAuthorChangedEventHandler? AuthorChanged;

	/// <summary>Raised when an anchor is added, removed or modified.</summary>
	public event AnnotationResourceChangedEventHandler? AnchorChanged;

	/// <summary>Raised when a cargo is added, removed or modified.</summary>
	public event AnnotationResourceChangedEventHandler? CargoChanged;

	/// <summary>The annotation id.</summary>
	public Guid Id => _id;

	/// <summary>The annotation type, such as the base schema's Highlight.</summary>
	public XmlQualifiedName AnnotationType => _type!;

	/// <summary>When the annotation was created.</summary>
	public DateTime CreationTime => _created;

	/// <summary>When the annotation last changed.</summary>
	public DateTime LastModificationTime => _modified;

	/// <summary>The authors.</summary>
	public Collection<string> Authors => _authors;

	/// <summary>What the annotation is attached to.</summary>
	public Collection<AnnotationResource> Anchors => _anchors;

	/// <summary>The content of the annotation.</summary>
	public Collection<AnnotationResource> Cargos => _cargos;

	static void CheckType(XmlQualifiedName type)
	{
		if (type == null) throw new ArgumentNullException("annotationType");
		if (string.IsNullOrEmpty(type.Name)) throw new ArgumentException("A type name is required.", "annotationType.Name");
		if (string.IsNullOrEmpty(type.Namespace))
			throw new ArgumentException("A type namespace is required.", "annotationType.Namespace");
	}

	void Hook()
	{
		_anchors.ItemChanged += (s, e) => Resource((AnnotationResource)s!, AnnotationAction.Modified, AnchorChanged);
		_cargos.ItemChanged += (s, e) => Resource((AnnotationResource)s!, AnnotationAction.Modified, CargoChanged);
		_anchors.CollectionChanged += (s, e) => Resources(e, AnchorChanged);
		_cargos.CollectionChanged += (s, e) => Resources(e, CargoChanged);
		_authors.CollectionChanged += OnAuthorsChanged;
	}

	void Resources(NotifyCollectionChangedEventArgs e, AnnotationResourceChangedEventHandler? handler)
	{
		if (e.Action == NotifyCollectionChangedAction.Replace)
			foreach (AnnotationResource old in e.OldItems!) Resource(old, AnnotationAction.Removed, handler);
		var (action, items) = Classify(e);
		if (items != null)
			foreach (AnnotationResource item in items) Resource(item, action, handler);
	}

	void OnAuthorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Replace)
			foreach (var old in e.OldItems!) Author(old, AnnotationAction.Removed);
		var (action, items) = Classify(e);
		if (items != null)
			foreach (var item in items) Author(item, action);
	}

	static (AnnotationAction, IList?) Classify(NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
			case NotifyCollectionChangedAction.Add:
			case NotifyCollectionChangedAction.Replace:
				return (AnnotationAction.Added, e.NewItems);
			case NotifyCollectionChangedAction.Remove:
				return (AnnotationAction.Removed, e.OldItems);
			default:
				return (AnnotationAction.Modified, null);
		}
	}

	void Author(object author, AnnotationAction action)
	{
		if (_quiet) return;
		_modified = DateTime.Now;
		AuthorChanged?.Invoke(this, new AnnotationAuthorChangedEventArgs(this, action, author));
	}

	void Resource(AnnotationResource resource, AnnotationAction action, AnnotationResourceChangedEventHandler? handler)
	{
		if (_quiet) return;
		_modified = DateTime.Now;
		handler?.Invoke(this, new AnnotationResourceChangedEventArgs(this, action, resource));
	}
}
