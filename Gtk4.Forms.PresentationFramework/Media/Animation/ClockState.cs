namespace System.Windows.Media.Animation;

/// <summary>State of a <see cref="Clock"/>.</summary>
public enum ClockState
{
	/// <summary>The clock is running (or in its delay period).</summary>
	Active,
	/// <summary>The clock has completed its active period and holds its end value.</summary>
	Filling,
	/// <summary>The clock has stopped (FillBehavior.Stop or explicitly stopped).</summary>
	Stopped
}
