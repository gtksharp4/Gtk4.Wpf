namespace System.Windows.Controls.Primitives;

/// <summary>The animation a <see cref="Popup"/> plays when it opens.</summary>
public enum PopupAnimation
{
	/// <summary>No animation.</summary>
	None = 0,

	/// <summary>The popup fades in.</summary>
	Fade = 1,

	/// <summary>The popup scrolls in.</summary>
	Scroll = 2,

	/// <summary>The popup slides in.</summary>
	Slide = 3,
}
