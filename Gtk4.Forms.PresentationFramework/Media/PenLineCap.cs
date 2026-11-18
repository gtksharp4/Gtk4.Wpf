namespace System.Windows.Media;

/// <summary>
/// Describes the shape at the end of a line or segment in WPF.
/// Maps to <see cref="Cairo.LineCap"/> when applied; see <c>StrokeStyle</c>
/// for the Cairo mapping.
/// </summary>
public enum PenLineCap
{
	/// <summary>No cap; line ends at the exact endpoint (Cairo: <c>Butt</c>).</summary>
	Flat,

	/// <summary>Square cap that extends past the endpoint by half the line width (Cairo: <c>Square</c>).</summary>
	Square,

	/// <summary>Rounded cap; semicircle centred at the endpoint (Cairo: <c>Round</c>).</summary>
	Round,

	/// <summary>
	/// Triangle cap (no Cairo equivalent; falls back to <c>Flat</c> in this implementation).
	/// </summary>
	Triangle,
}
