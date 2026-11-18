namespace System.Windows.Media;

/// <summary>
/// Describes the cap style applied to the ends of each dash segment in a dashed stroke.
/// WPF applies dash caps to each individual dash; Cairo does not distinguish dash caps
/// from line caps — this enum is accepted by the API for WPF compatibility but the
/// value is not currently applied to Cairo (the per-dash cap has no native Cairo equivalent).
/// </summary>
public enum PenDashCap
{
	/// <summary>No cap on each dash end.</summary>
	Flat,

	/// <summary>Rounded cap on each dash end.</summary>
	Round,

	/// <summary>Triangle cap on each dash end.</summary>
	Triangle,
}
