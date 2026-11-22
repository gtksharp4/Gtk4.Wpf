namespace System.Windows.Controls;

/// <summary>Specifies the behavior of a <see cref="MediaElement"/> that is loaded or unloaded.</summary>
public enum MediaState
{
	/// <summary>The media is controlled through the element's own Play/Pause/Stop/Close methods.</summary>
	Manual = 0,

	/// <summary>The media plays.</summary>
	Play = 1,

	/// <summary>The media is released and its resources freed.</summary>
	Close = 2,

	/// <summary>The media is paused at its current position.</summary>
	Pause = 3,

	/// <summary>The media stops and returns to the start.</summary>
	Stop = 4,
}
