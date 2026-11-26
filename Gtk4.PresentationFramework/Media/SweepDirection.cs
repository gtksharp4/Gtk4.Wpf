namespace System.Windows.Media;

/// <summary>
/// Specifies the direction an arc is drawn.
/// Mirrors <c>System.Windows.Media.SweepDirection</c>.
/// </summary>
public enum SweepDirection
{
	/// <summary>Arc drawn counter-clockwise (SVG sweep-flag=0).</summary>
	Counterclockwise = 0,

	/// <summary>Arc drawn clockwise (SVG sweep-flag=1).</summary>
	Clockwise = 1,
}
