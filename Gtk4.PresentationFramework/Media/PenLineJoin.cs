namespace System.Windows.Media;

/// <summary>
/// Describes how two lines are joined at a vertex in WPF.
/// Maps to <see cref="Cairo.LineJoin"/> when applied.
/// </summary>
public enum PenLineJoin
{
	/// <summary>Mitered join; lines extend to a sharp point (Cairo: <c>Miter</c>).</summary>
	Miter,

	/// <summary>Beveled join; corner is cut flat (Cairo: <c>Bevel</c>).</summary>
	Bevel,

	/// <summary>Rounded join; corner is a circular arc (Cairo: <c>Round</c>).</summary>
	Round,
}
