namespace System.Windows.Controls;

/// <summary>Specifies how an <see cref="InkCanvas"/> reacts to input.</summary>
public enum InkCanvasEditingMode
{
	/// <summary>No editing.</summary>
	None = 0,
	/// <summary>Draws ink.</summary>
	Ink = 1,
	/// <summary>Recognises gestures only.</summary>
	GestureOnly = 2,
	/// <summary>Draws ink and recognises gestures.</summary>
	InkAndGesture = 3,
	/// <summary>Selects strokes and elements.</summary>
	Select = 4,
	/// <summary>Erases the touched part of a stroke.</summary>
	EraseByPoint = 5,
	/// <summary>Erases whole strokes.</summary>
	EraseByStroke = 6,
}
