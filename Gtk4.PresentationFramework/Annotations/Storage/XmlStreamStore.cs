using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.Windows.Annotations.Storage;

/// <summary>
/// An <see cref="AnnotationStore"/> kept in memory and persisted as WPF annotation XML in a stream. An empty
/// stream is a store with no annotations. Gap: namespaces declared compatible in the constructor's
/// <c>knownNamespaces</c> are accepted but not rewritten to their canonical namespace.
/// </summary>
public sealed partial class XmlStreamStore : AnnotationStore
{
	static readonly Dictionary<Uri, IList<Uri>?> _predefined = new Dictionary<Uri, IList<Uri>?>
	{
		{ new Uri(AnnotationXmlConstants.CoreNamespace), null },
		{ new Uri(AnnotationXmlConstants.BaseNamespace), null },
		{ new Uri(AnnotationXmlConstants.PresentationNamespace), null }
	};

	readonly List<Annotation> _annotations = new List<Annotation>();
	readonly List<Uri> _ignored = new List<Uri>();
	Stream? _stream;
	bool _dirty;
	bool _autoFlush;

	/// <summary>Opens a store over a readable, seekable stream.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
	/// <exception cref="ArgumentException">The stream cannot seek.</exception>
	public XmlStreamStore(Stream stream)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		if (!stream.CanSeek) throw new ArgumentException("The stream does not support seeking.", nameof(stream));
		SetStream(stream, null);
	}

	/// <summary>Opens a store and accepts the given extra namespaces (each with its compatible ones).</summary>
	public XmlStreamStore(Stream stream, IDictionary<Uri, IList<Uri>> knownNamespaces)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		SetStream(stream, knownNamespaces);
	}

	/// <inheritdoc/>
	public override bool AutoFlush
	{
		get
		{
			lock (SyncRoot) return _autoFlush;
		}
		set
		{
			lock (SyncRoot)
			{
				_autoFlush = value;
				if (value) Flush();
			}
		}
	}

	/// <summary>Namespaces met while loading that were neither well known nor declared known.</summary>
	public IList<Uri> IgnoredNamespaces => _ignored;

	/// <summary>The namespaces the store understands without being told.</summary>
	public static IList<Uri> WellKnownNamespaces => _predefined.Keys.ToList();

	/// <summary>The namespaces declared compatible with a well-known one, or null.</summary>
	public static IList<Uri>? GetWellKnownCompatibleNamespaces(Uri name)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));
		return _predefined.TryGetValue(name, out var list) ? list : null;
	}

	/// <inheritdoc/>
	public override void AddAnnotation(Annotation newAnnotation)
	{
		if (newAnnotation == null) throw new ArgumentNullException(nameof(newAnnotation));
		lock (SyncRoot)
		{
			CheckStatus();
			if (Find(newAnnotation.Id) != null)
				throw new ArgumentException("An annotation with this id already exists.", nameof(newAnnotation));
			_annotations.Add(newAnnotation);
			Attach(newAnnotation);
		}
		OnStoreContentChanged(new StoreContentChangedEventArgs(StoreContentAction.Added, newAnnotation));
	}

	/// <inheritdoc/>
	public override Annotation? DeleteAnnotation(Guid annotationId)
	{
		Annotation? found;
		lock (SyncRoot)
		{
			CheckStatus();
			found = Find(annotationId);
			if (found != null)
			{
				_annotations.Remove(found);
				Detach(found);
			}
		}
		if (found != null) OnStoreContentChanged(new StoreContentChangedEventArgs(StoreContentAction.Deleted, found));
		return found;
	}

	/// <inheritdoc/>
	public override Annotation? GetAnnotation(Guid annotationId)
	{
		lock (SyncRoot)
		{
			CheckStatus();
			return Find(annotationId);
		}
	}

	/// <inheritdoc/>
	public override IList<Annotation> GetAnnotations()
	{
		lock (SyncRoot)
		{
			CheckStatus();
			return _annotations.ToList();
		}
	}

	/// <inheritdoc/>
	public override IList<Annotation> GetAnnotations(ContentLocator anchorLocator)
	{
		if (anchorLocator == null) throw new ArgumentNullException(nameof(anchorLocator));
		lock (SyncRoot)
		{
			CheckStatus();
			return _annotations.Where(a => a.Anchors.Any(r => r.ContentLocators
				.Any(l => l.Flatten().Any(f => f.StartsWith(anchorLocator))))).ToList();
		}
	}

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!disposing) return;
		lock (SyncRoot)
		{
			foreach (var annotation in _annotations) Detach(annotation);
			_annotations.Clear();
			_stream = null;
		}
	}

	/// <inheritdoc/>
	protected override void OnStoreContentChanged(StoreContentChangedEventArgs e)
	{
		lock (SyncRoot) _dirty = true;
		base.OnStoreContentChanged(e);
	}

	Annotation? Find(Guid id) => _annotations.FirstOrDefault(a => a.Id == id);

	void CheckStatus()
	{
		if (IsDisposed) throw new ObjectDisposedException(null, "The annotation store is closed.");
		if (_stream == null) throw new InvalidOperationException("The store has no stream.");
	}

	void Attach(Annotation annotation)
	{
		annotation.AuthorChanged += HandleAuthor;
		annotation.AnchorChanged += HandleAnchor;
		annotation.CargoChanged += HandleCargo;
	}

	void Detach(Annotation annotation)
	{
		annotation.AuthorChanged -= HandleAuthor;
		annotation.AnchorChanged -= HandleAnchor;
		annotation.CargoChanged -= HandleCargo;
	}

	void HandleAuthor(object sender, AnnotationAuthorChangedEventArgs e)
	{
		lock (SyncRoot) _dirty = true;
		OnAuthorChanged(e);
	}

	void HandleAnchor(object sender, AnnotationResourceChangedEventArgs e)
	{
		lock (SyncRoot) _dirty = true;
		OnAnchorChanged(e);
	}

	void HandleCargo(object sender, AnnotationResourceChangedEventArgs e)
	{
		lock (SyncRoot) _dirty = true;
		OnCargoChanged(e);
	}
}
