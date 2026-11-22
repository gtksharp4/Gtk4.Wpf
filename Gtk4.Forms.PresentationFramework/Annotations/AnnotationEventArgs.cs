namespace System.Windows.Annotations;

/// <summary>Represents the method that handles <see cref="Annotation.AnchorChanged"/> and
/// <see cref="Annotation.CargoChanged"/>.</summary>
public delegate void AnnotationResourceChangedEventHandler(object sender, AnnotationResourceChangedEventArgs e);

/// <summary>Represents the method that handles <see cref="Annotation.AuthorChanged"/>.</summary>
public delegate void AnnotationAuthorChangedEventHandler(object sender, AnnotationAuthorChangedEventArgs e);

/// <summary>Data for an anchor or cargo change.</summary>
public sealed class AnnotationResourceChangedEventArgs : EventArgs
{
	/// <summary>Creates the event data.</summary>
	public AnnotationResourceChangedEventArgs(Annotation annotation, AnnotationAction action, AnnotationResource resource)
	{
		Annotation = annotation;
		Action = action;
		Resource = resource;
	}

	/// <summary>The annotation that changed.</summary>
	public Annotation Annotation { get; }

	/// <summary>How the resource changed.</summary>
	public AnnotationAction Action { get; }

	/// <summary>The anchor or cargo that was added, removed or modified.</summary>
	public AnnotationResource Resource { get; }
}

/// <summary>Data for an author change.</summary>
public sealed class AnnotationAuthorChangedEventArgs : EventArgs
{
	/// <summary>Creates the event data.</summary>
	public AnnotationAuthorChangedEventArgs(Annotation annotation, AnnotationAction action, object author)
	{
		Annotation = annotation;
		Action = action;
		Author = author;
	}

	/// <summary>The annotation that changed.</summary>
	public Annotation Annotation { get; }

	/// <summary>How the author list changed.</summary>
	public AnnotationAction Action { get; }

	/// <summary>The author that was added or removed.</summary>
	public object Author { get; }
}
