using System.Windows.Markup;
using System.Windows.Xps.Packaging;

namespace System.Windows.Documents;

/// <summary>
/// A reference from a <see cref="FixedDocumentSequence"/> to a <see cref="FixedDocument"/>, either by
/// <see cref="Source"/> URI (loaded on demand) or by an in-memory document set with <see cref="SetDocument"/>.
/// Mirrors WPF <c>DocumentReference</c> (without the async loading API).
/// </summary>
public sealed class DocumentReference : FrameworkElement, IUriContext
{
	/// <summary>Identifies the Source property.</summary>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
		nameof(Source), typeof(Uri), typeof(DocumentReference), new PropertyMetadata(null, OnSourceChanged));

	FixedDocument? _document;
	bool _fromSource;
	Uri? _baseUri;

	/// <summary>Creates an empty reference.</summary>
	public DocumentReference() { }

	/// <summary>The URI of the document part, relative to the base URI of the sequence that holds it.</summary>
	public Uri? Source
	{
		get => (Uri?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	Uri? IUriContext.BaseUri
	{
		get => _baseUri;
		set => _baseUri = value;
	}

	static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var reference = (DocumentReference)d;
		if (reference._fromSource)
		{
			reference._document = null;
			reference._fromSource = false;
		}
	}

	/// <summary>
	/// Returns the document, loading it from <see cref="Source"/> when it is not loaded yet or when
	/// <paramref name="forceReload"/> is true. Returns null when there is neither a document nor a source.
	/// </summary>
	/// <exception cref="InvalidOperationException">The source cannot be resolved against the base URI.</exception>
	public FixedDocument? GetDocument(bool forceReload)
	{
		if (_document != null && !(forceReload && _fromSource))
			return _document;
		var source = Source;
		if (source == null)
			return _document;
		_document = FixedPartLoader.LoadDocument(FixedPartLoader.Resolve(_baseUri, source));
		_fromSource = true;
		return _document;
	}

	/// <summary>Sets an in-memory document; it replaces <see cref="Source"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="fixedDocument"/> is null.</exception>
	/// <exception cref="InvalidOperationException">A source is set already.</exception>
	public void SetDocument(FixedDocument fixedDocument)
	{
		if (fixedDocument == null)
			throw new ArgumentNullException(nameof(fixedDocument));
		if (Source != null)
			throw new InvalidOperationException("The reference already has a source.");
		_document = fixedDocument;
		_fromSource = false;
	}

	/// <summary>The document a writer should save: the in-memory one, or null for an unloaded source.</summary>
	internal FixedDocument? CurrentDocument => _document;

	/// <summary>The resolved absolute source URI, or null.</summary>
	internal Uri? ResolvedSource => Source == null ? null : FixedPartLoader.Resolve(_baseUri, Source);

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget() => new Gtk.Box(Gtk.Orientation.Vertical, 0);
}
