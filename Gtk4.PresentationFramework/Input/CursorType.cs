namespace System.Windows.Input;

/// <summary>
/// Identifies a standard WPF cursor shape.
/// </summary>
public enum CursorType
{
	/// <summary>No cursor is displayed.</summary>
	None = 0,
	/// <summary>Slashed circle indicating an operation is not allowed.</summary>
	No = 1,
	/// <summary>Standard arrow cursor.</summary>
	Arrow = 2,
	/// <summary>Arrow with a small hourglass.</summary>
	AppStarting = 3,
	/// <summary>Crosshair cursor.</summary>
	Cross = 4,
	/// <summary>Arrow with a question-mark balloon.</summary>
	Help = 5,
	/// <summary>I-beam text-insertion cursor.</summary>
	IBeam = 6,
	/// <summary>Four-directional resize cursor.</summary>
	SizeAll = 7,
	/// <summary>Diagonal resize (northeast-southwest).</summary>
	SizeNESW = 8,
	/// <summary>Vertical resize (north-south).</summary>
	SizeNS = 9,
	/// <summary>Diagonal resize (northwest-southeast).</summary>
	SizeNWSE = 10,
	/// <summary>Horizontal resize (west-east).</summary>
	SizeWE = 11,
	/// <summary>Up arrow cursor.</summary>
	UpArrow = 12,
	/// <summary>Hourglass (busy) cursor.</summary>
	Wait = 13,
	/// <summary>Hand (finger pointer) cursor, typically used over links.</summary>
	Hand = 14,
	/// <summary>Pen / stylus cursor.</summary>
	Pen = 15,
	/// <summary>Scroll north-south cursor.</summary>
	ScrollNS = 16,
	/// <summary>Scroll west-east cursor.</summary>
	ScrollWE = 17,
	/// <summary>Scroll all directions cursor.</summary>
	ScrollAll = 18,
	/// <summary>Scroll north cursor.</summary>
	ScrollN = 19,
	/// <summary>Scroll south cursor.</summary>
	ScrollS = 20,
	/// <summary>Scroll west cursor.</summary>
	ScrollW = 21,
	/// <summary>Scroll east cursor.</summary>
	ScrollE = 22,
	/// <summary>Scroll northwest cursor.</summary>
	ScrollNW = 23,
	/// <summary>Scroll northeast cursor.</summary>
	ScrollNE = 24,
	/// <summary>Scroll southwest cursor.</summary>
	ScrollSW = 25,
	/// <summary>Scroll southeast cursor.</summary>
	ScrollSE = 26,
	/// <summary>Arrow cursor with a no-drop badge.</summary>
	ArrowCD = 27,
	/// <summary>Legacy alias of <see cref="ScrollWE"/>.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	ScrollEW = ScrollWE,
}
