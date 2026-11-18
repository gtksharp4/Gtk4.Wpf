using System.Collections.ObjectModel;

namespace System.Windows.Media;

/// <summary>
/// An ordered, mutable collection of <see cref="Drawing"/> objects.
/// Mirrors <c>System.Windows.Media.DrawingCollection</c>.
/// </summary>
public class DrawingCollection : Collection<Drawing> { }

/// <summary>
/// A composite drawing that groups child drawings, with optional transform, clip and opacity.
/// Mirrors <c>System.Windows.Media.DrawingGroup</c>.
/// </summary>
/// <remarks>
/// <b>What paints:</b> <see cref="Draw"/> applies an optional <see cref="Opacity"/>, saves/restores
/// the Cairo state, optionally clips to <see cref="ClipGeometry"/>, applies <see cref="Transform"/>,
/// then draws each child in order.
/// <b>Object-model only:</b> <see cref="Children"/> and <see cref="Bounds"/> are available without a surface.
/// </remarks>
public class DrawingGroup : Drawing
{
	DrawingCollection? _children;

	/// <summary>The ordered child drawings.</summary>
	public DrawingCollection Children => _children ??= new DrawingCollection();

	/// <summary>Optional transform applied before drawing children.</summary>
	public Transform? Transform { get; set; }

	/// <summary>Optional geometry used to clip children.</summary>
	public Geometry? ClipGeometry { get; set; }

	/// <summary>Opacity multiplier (0–1) applied to the whole group.</summary>
	public double Opacity { get; set; } = 1.0;

	/// <inheritdoc/>
	public override Rect Bounds
	{
		get
		{
			if (_children == null || _children.Count == 0)
				return Rect.Empty;
			return DrawingGroupMath.Union(_children);
		}
	}

	/// <inheritdoc/>
	internal override void Draw(Cairo.Context cr)
	{
		if (_children == null || _children.Count == 0) return;

		cr.Save();

		if (Opacity < 1.0)
			cr.PushGroup();

		if (ClipGeometry != null)
		{
			ClipGeometry.BuildPath(cr);
			cr.Clip();
		}

		if (Transform != null)
		{
			var m = Transform.Value;
			var cm = new Cairo.Matrix(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
			cr.Transform(cm);
		}

		foreach (var child in _children)
			child.Draw(cr);

		if (Opacity < 1.0)
		{
			cr.PopGroupToSource();
			cr.PaintWithAlpha(Opacity);
		}

		cr.Restore();
	}
}

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
