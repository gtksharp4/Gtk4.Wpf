using System;

namespace System.Windows.Media;

/// <summary>
/// A visual that supports direct drawing via <see cref="RenderOpen"/>.
/// Mirrors <c>System.Windows.Media.DrawingVisual</c>.
/// </summary>
/// <remarks>
/// The drawing commands recorded by the returned <see cref="DrawingContext"/> are
/// accessible for inspection via <see cref="Drawing"/>. Actual Cairo rendering is a
/// Phase-2 item. <see cref="DrawingVisual"/> participates in the visual tree through
/// its <see cref="VisualCollection"/> children.
/// </remarks>
public class DrawingVisual : Visual
{
	VisualCollection? _children;
	DrawingContext? _drawing;

	/// <summary>The last <see cref="DrawingContext"/> committed to this visual.</summary>
	public DrawingContext? Drawing => _drawing;

	/// <summary>
	/// Opens a <see cref="DrawingContext"/> for recording drawing commands.
	/// Dispose the returned context (or call <see cref="DrawingContext.Close"/>) to commit
	/// the drawing to this visual and replace any previous recording.
	/// </summary>
	/// <remarks>
	/// The commit happens inside a <see cref="DrawingContextScope"/> wrapper returned here.
	/// Callers use the standard <c>using</c> pattern.
	/// </remarks>
	public DrawingContextScope RenderOpen()
	{
		var ctx = new DrawingContext();
		return new DrawingContextScope(ctx, this);
	}

	internal void CommitDrawing(DrawingContext ctx) => _drawing = ctx;

	// ── Visual children ───────────────────────────────────────────────────────────

	/// <summary>The collection of visual children of this <see cref="DrawingVisual"/>.</summary>
	public VisualCollection Children => _children ??= new VisualCollection(this);

	/// <inheritdoc/>
	protected internal override int VisualChildrenCount =>
		_children == null ? 0 : _children.Count;

	/// <inheritdoc/>
	protected internal override Visual GetVisualChild(int index)
	{
		if (_children == null)
			throw new ArgumentOutOfRangeException(nameof(index));
		return _children[index];
	}

}
