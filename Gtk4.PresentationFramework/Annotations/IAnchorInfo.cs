namespace System.Windows.Annotations;

/// <summary>Describes how an annotation resolved against the displayed content.</summary>
public interface IAnchorInfo
{
	/// <summary>The anchor resource that resolved.</summary>
	AnnotationResource Anchor { get; }

	/// <summary>The annotation.</summary>
	Annotation Annotation { get; }

	/// <summary>The resolved anchor; a <see cref="TextAnchor"/> for flow content.</summary>
	object ResolvedAnchor { get; }
}

sealed class AnchorInfo : IAnchorInfo
{
	internal AnchorInfo(Annotation annotation, AnnotationResource anchor, object resolved)
	{
		Annotation = annotation;
		Anchor = anchor;
		ResolvedAnchor = resolved;
	}

	public AnnotationResource Anchor { get; }

	public Annotation Annotation { get; }

	public object ResolvedAnchor { get; }
}
