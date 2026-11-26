namespace System.Windows.Media.Animation;

/// <summary>
/// Specifies how a <see cref="Timeline"/> behaves once it reaches the end of its active period.
/// </summary>
public enum FillBehavior
{
	/// <summary>
	/// The timeline holds the final value at the end of its active period (default).
	/// </summary>
	HoldEnd,

	/// <summary>
	/// The animation is deactivated and the base (pre-animation) value is restored.
	/// </summary>
	Stop
}