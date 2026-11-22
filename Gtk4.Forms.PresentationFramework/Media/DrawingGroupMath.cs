namespace System.Windows.Media;

/// <summary>
/// Pure helpers for <see cref="DrawingGroup"/> bound computation.
/// Extracted for unit-testability without a surface.
/// </summary>
internal static class DrawingGroupMath
{
	/// <summary>
	/// Computes the union of the bounding rectangles of all drawings in
	/// <paramref name="drawings"/>.  Returns <see cref="Rect.Empty"/> when the
	/// collection is empty.
	/// </summary>
	internal static Rect Union(System.Collections.Generic.IEnumerable<Drawing> drawings)
	{
		Rect result = Rect.Empty;
		foreach (var d in drawings)
		{
			var b = d.Bounds;
			if (b.IsEmpty) continue;
			result = result.IsEmpty ? b : Rect.Union(result, b);
		}
		return result;
	}
}
