namespace System.Windows.Input;

/// <summary>
/// Provides the set of standard WPF mouse cursors, pre-mapped to GTK 4 / CSS cursor names.
/// </summary>
public static class Cursors
{
	/// <summary>Standard arrow cursor.</summary>
	public static Cursor Arrow { get; } = new Cursor(CursorType.Arrow);

	/// <summary>Arrow with a small hourglass (application starting).</summary>
	public static Cursor AppStarting { get; } = new Cursor(CursorType.AppStarting);

	/// <summary>Four-pointed crosshair cursor.</summary>
	public static Cursor Cross { get; } = new Cursor(CursorType.Cross);

	/// <summary>Hand / pointer cursor (links, buttons).</summary>
	public static Cursor Hand { get; } = new Cursor(CursorType.Hand);

	/// <summary>Arrow with a question-mark balloon.</summary>
	public static Cursor Help { get; } = new Cursor(CursorType.Help);

	/// <summary>I-beam text-insertion cursor.</summary>
	public static Cursor IBeam { get; } = new Cursor(CursorType.IBeam);

	/// <summary>No cursor is displayed.</summary>
	public static Cursor None { get; } = new Cursor(CursorType.None);

	/// <summary>Slashed circle — operation not allowed.</summary>
	public static Cursor No { get; } = new Cursor(CursorType.No);

	/// <summary>Four-directional resize cursor.</summary>
	public static Cursor SizeAll { get; } = new Cursor(CursorType.SizeAll);

	/// <summary>Diagonal resize — northeast/southwest.</summary>
	public static Cursor SizeNESW { get; } = new Cursor(CursorType.SizeNESW);

	/// <summary>Vertical resize — north/south.</summary>
	public static Cursor SizeNS { get; } = new Cursor(CursorType.SizeNS);

	/// <summary>Diagonal resize — northwest/southeast.</summary>
	public static Cursor SizeNWSE { get; } = new Cursor(CursorType.SizeNWSE);

	/// <summary>Horizontal resize — west/east.</summary>
	public static Cursor SizeWE { get; } = new Cursor(CursorType.SizeWE);

	/// <summary>Vertical up-arrow cursor.</summary>
	public static Cursor UpArrow { get; } = new Cursor(CursorType.UpArrow);

	/// <summary>Hourglass / busy cursor.</summary>
	public static Cursor Wait { get; } = new Cursor(CursorType.Wait);

	/// <summary>Pen / stylus cursor.</summary>
	public static Cursor Pen { get; } = new Cursor(CursorType.Pen);

	/// <summary>Scroll-all-directions cursor.</summary>
	public static Cursor ScrollAll { get; } = new Cursor(CursorType.ScrollAll);

	/// <summary>Scroll north cursor.</summary>
	public static Cursor ScrollN { get; } = new Cursor(CursorType.ScrollN);

	/// <summary>Scroll south cursor.</summary>
	public static Cursor ScrollS { get; } = new Cursor(CursorType.ScrollS);

	/// <summary>Scroll east cursor.</summary>
	public static Cursor ScrollE { get; } = new Cursor(CursorType.ScrollE);

	/// <summary>Scroll west cursor.</summary>
	public static Cursor ScrollW { get; } = new Cursor(CursorType.ScrollW);

	/// <summary>Scroll north–south cursor.</summary>
	public static Cursor ScrollNS { get; } = new Cursor(CursorType.ScrollNS);

	/// <summary>Scroll east–west cursor.</summary>
	public static Cursor ScrollEW { get; } = new Cursor(CursorType.ScrollEW);

	/// <summary>Scroll west-east cursor.</summary>
	public static Cursor ScrollWE { get; } = new Cursor(CursorType.ScrollWE);

	/// <summary>Scroll northeast cursor.</summary>
	public static Cursor ScrollNE { get; } = new Cursor(CursorType.ScrollNE);

	/// <summary>Scroll northwest cursor.</summary>
	public static Cursor ScrollNW { get; } = new Cursor(CursorType.ScrollNW);

	/// <summary>Scroll southeast cursor.</summary>
	public static Cursor ScrollSE { get; } = new Cursor(CursorType.ScrollSE);

	/// <summary>Scroll southwest cursor.</summary>
	public static Cursor ScrollSW { get; } = new Cursor(CursorType.ScrollSW);

	/// <summary>Arrow with a no-drop badge.</summary>
	public static Cursor ArrowCD { get; } = new Cursor(CursorType.ArrowCD);
}
