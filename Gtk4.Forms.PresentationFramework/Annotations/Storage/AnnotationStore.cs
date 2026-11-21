using System.Collections.Generic;

namespace System.Windows.Annotations.Storage;

/// <summary>Base class of annotation stores.</summary>
public abstract class AnnotationStore : IDisposable
{
	readonly object _lock = new object();
	bool _disposed;

	/// <summary>Raised after an annotation is added to or deleted from the store.</summary>
	public event StoreContentChangedEventHandler? StoreContentChanged;

	/// <summary>Raised when an author of a stored annotation changes.</summary>
	public event AnnotationAuthorChangedEventHandler? AuthorChanged;

	/// <summary>Raised when an anchor of a stored annotation changes.</summary>
	public event AnnotationResourceChangedEventHandler? AnchorChanged;

	/// <summary>Raised when a cargo of a stored annotation changes.</summary>
	public event AnnotationResourceChangedEventHandler? CargoChanged;

	/// <summary>Adds an annotation.</summary>
	public abstract void AddAnnotation(Annotation newAnnotation);

	/// <summary>Deletes an annotation and returns it, or null when there is none with that id.</summary>
	public abstract Annotation? DeleteAnnotation(Guid annotationId);

	/// <summary>The annotations whose anchors start with <paramref name="anchorLocator"/>.</summary>
	public abstract IList<Annotation> GetAnnotations(ContentLocator anchorLocator);

	/// <summary>All annotations.</summary>
	public abstract IList<Annotation> GetAnnotations();

	/// <summary>The annotation with the given id, or null.</summary>
	public abstract Annotation? GetAnnotation(Guid annotationId);

	/// <summary>Writes pending changes to the backing storage.</summary>
	public abstract void Flush();

	/// <summary>Whether every change is written to the backing storage at once.</summary>
	public abstract bool AutoFlush { get; set; }

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the store.</summary>
	protected virtual void Dispose(bool disposing)
	{
		lock (_lock) _disposed = true;
	}

	/// <summary>Raises <see cref="AuthorChanged"/>, flushing first under AutoFlush.</summary>
	protected virtual void OnAuthorChanged(AnnotationAuthorChangedEventArgs args)
	{
		if (args.Author == null) return;
		AnnotationAuthorChangedEventHandler? handler;
		lock (_lock) handler = AuthorChanged;
		if (AutoFlush) Flush();
		handler?.Invoke(this, args);
	}

	/// <summary>Raises <see cref="AnchorChanged"/>, flushing first under AutoFlush.</summary>
	protected virtual void OnAnchorChanged(AnnotationResourceChangedEventArgs args)
	{
		if (args.Resource == null) return;
		AnnotationResourceChangedEventHandler? handler;
		lock (_lock) handler = AnchorChanged;
		if (AutoFlush) Flush();
		handler?.Invoke(this, args);
	}

	/// <summary>Raises <see cref="CargoChanged"/>, flushing first under AutoFlush.</summary>
	protected virtual void OnCargoChanged(AnnotationResourceChangedEventArgs args)
	{
		if (args.Resource == null) return;
		AnnotationResourceChangedEventHandler? handler;
		lock (_lock) handler = CargoChanged;
		if (AutoFlush) Flush();
		handler?.Invoke(this, args);
	}

	/// <summary>Raises <see cref="StoreContentChanged"/>, flushing first under AutoFlush.</summary>
	protected virtual void OnStoreContentChanged(StoreContentChangedEventArgs e)
	{
		StoreContentChangedEventHandler? handler;
		lock (_lock) handler = StoreContentChanged;
		if (AutoFlush) Flush();
		handler?.Invoke(this, e);
	}

	/// <summary>The lock that guards the store.</summary>
	protected object SyncRoot => _lock;

	/// <summary>True once the store is disposed.</summary>
	protected bool IsDisposed => _disposed;
}

/// <summary>Represents the method that handles <see cref="AnnotationStore.StoreContentChanged"/>.</summary>
public delegate void StoreContentChangedEventHandler(object sender, StoreContentChangedEventArgs e);

/// <summary>What happened to the store's content.</summary>
public enum StoreContentAction
{
	/// <summary>An annotation was added.</summary>
	Added,

	/// <summary>An annotation was deleted.</summary>
	Deleted
}

/// <summary>Data for <see cref="AnnotationStore.StoreContentChanged"/>.</summary>
public class StoreContentChangedEventArgs : EventArgs
{
	/// <summary>Creates the event data.</summary>
	public StoreContentChangedEventArgs(StoreContentAction action, Annotation annotation)
	{
		Action = action;
		Annotation = annotation;
	}

	/// <summary>Whether the annotation was added or deleted.</summary>
	public StoreContentAction Action { get; }

	/// <summary>The annotation.</summary>
	public Annotation Annotation { get; }
}
