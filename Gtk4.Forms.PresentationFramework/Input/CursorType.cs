namespace System.Windows.Input;

/// <summary>
/// Identifies a standard WPF cursor shape.
/// </summary>
public enum CursorType
{
	/// <summary>No cursor is displayed.</summary>
	None,
	/// <summary>Standard arrow cursor.</summary>
	Arrow,
	/// <summary>Arrow with a small hourglass.</summary>
	AppStarting,
	/// <summary>Four-pointed arrow for moving objects.</summary>
	Cross,
	/// <summary>Hand (finger pointer) cursor, typically used over links.</summary>
	Hand,
	/// <summary>Arrow with a question-mark balloon.</summary>
	Help,
	/// <summary>I-beam text-insertion cursor.</summary>
	IBeam,
	/// <summary>Slashed circle indicating an operation is not allowed.</summary>
	No,
	/// <summary>Four-directional resize cursor.</summary>
	SizeAll,
	/// <summary>Diagonal resize (northeast–southwest).</summary>
	SizeNESW,
	/// <summary>Vertical resize (north–south).</summary>
	SizeNS,
	/// <summary>Diagonal resize (northwest–southeast).</summary>
	SizeNWSE,
	/// <summary>Horizontal resize (west–east).</summary>
	SizeWE,
	/// <summary>Vertical split cursor.</summary>
	UpArrow,
	/// <summary>Hourglass (busy) cursor.</summary>
	Wait,
	/// <summary>Pen / stylus cursor.</summary>
	Pen,
	/// <summary>Scroll all directions cursor.</summary>
	ScrollAll,
	/// <summary>Scroll north cursor.</summary>
	ScrollN,
	/// <summary>Scroll south cursor.</summary>
	ScrollS,
	/// <summary>Scroll east cursor.</summary>
	ScrollE,
	/// <summary>Scroll west cursor.</summary>
	ScrollW,
	/// <summary>Scroll north–south cursor.</summary>
	ScrollNS,
	/// <summary>Scroll east–west cursor.</summary>
	ScrollEW,
	/// <summary>Scroll northeast cursor.</summary>
	ScrollNE,
	/// <summary>Scroll northwest cursor.</summary>
	ScrollNW,
	/// <summary>Scroll southeast cursor.</summary>
	ScrollSE,
	/// <summary>Scroll southwest cursor.</summary>
	ScrollSW,
	/// <summary>Arrow cursor with a no-drop badge.</summary>
	ArrowCD,
	/// <summary>West-east scroll cursor (WPF <c>CursorType.ScrollWE</c>).</summary>
	ScrollWE,
}
