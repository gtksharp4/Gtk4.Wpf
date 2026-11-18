namespace System.Windows.Media;

/// <summary>
/// Abstract base for all WPF drawing objects.
/// Mirrors <c>System.Windows.Media.Drawing</c>.
/// </summary>
/// <remarks>
/// <b>What paints:</b> <see cref="Draw"/> is called by concrete subclasses with a live
/// <see cref="Cairo.Context"/>; the default base does nothing.
/// <b>Object-model only:</b> <see cref="Bounds"/> is always available without a surface.
/// </remarks>
public abstract class Drawing
{
	/// <summary>
	/// The axis-aligned bounding box of this drawing's content.
	/// </summary>
	public abstract Rect Bounds { get; }

	/// <summary>
	/// Draws this drawing onto <paramref name="cr"/>.
	/// The context is not saved/restored by the base; callers must do so when needed.
	/// </summary>
	internal abstract void Draw(Cairo.Context cr);
}
