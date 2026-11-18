namespace System.Windows.Media;

/// <summary>
/// Describes how content fills its allocated space.
/// Mirrors <c>System.Windows.Media.Stretch</c>.
/// Used by <see cref="TileBrush"/> and its subclasses.
/// </summary>
public enum Stretch
{
	/// <summary>Content preserves its natural size; no scaling.</summary>
	None,
	/// <summary>Content is scaled to fill the destination without preserving aspect ratio.</summary>
	Fill,
	/// <summary>Content is scaled uniformly to fit inside the destination (may leave empty space).</summary>
	Uniform,
	/// <summary>Content is scaled uniformly to fill the destination (may clip).</summary>
	UniformToFill,
}
