namespace System.Windows.Controls.Primitives;

/// <summary>Specifies where tick marks are drawn on a <see cref="System.Windows.Controls.Slider"/>.</summary>
public enum TickPlacement
{
	/// <summary>No tick marks are displayed.</summary>
	None,

	/// <summary>
	/// Tick marks appear above the track for a horizontal slider,
	/// or to the left for a vertical slider.
	/// </summary>
	TopLeft,

	/// <summary>
	/// Tick marks appear below the track for a horizontal slider,
	/// or to the right for a vertical slider.
	/// </summary>
	BottomRight,

	/// <summary>Tick marks appear on both sides of the track.</summary>
	Both,
}
