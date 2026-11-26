using System.Windows.Documents;

namespace System.Windows.Annotations;

/// <summary>
/// The text a flow-content annotation resolved to. Gap: only the bounding start and end are exposed, so a
/// highlight with several segments reports the span that covers them all.
/// </summary>
public sealed class TextAnchor
{
	readonly TextPointer _start;
	readonly TextPointer _end;

	internal TextAnchor(TextPointer start, TextPointer end)
	{
		_start = start;
		_end = end;
	}

	/// <summary>The start of the anchored text; a <see cref="TextPointer"/>.</summary>
	public ContentPosition BoundingStart => _start;

	/// <summary>The end of the anchored text; a <see cref="TextPointer"/>.</summary>
	public ContentPosition BoundingEnd => _end;

	/// <inheritdoc/>
	public override bool Equals(object? obj) =>
		obj is TextAnchor other && _start.IsInSameDocument(other._start)
		&& _start.CompareTo(other._start) == 0 && _end.CompareTo(other._end) == 0;

	/// <inheritdoc/>
	public override int GetHashCode() => _start.Document.GetHashCode() ^ (_start.Offset * 397) ^ _end.Offset;
}
