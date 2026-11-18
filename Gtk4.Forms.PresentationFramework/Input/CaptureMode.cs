namespace System.Windows.Input;

/// <summary>Specifies the mouse capture mode.</summary>
public enum CaptureMode
{
	/// <summary>No mouse capture; mouse input goes to the element under the mouse.</summary>
	None,
	/// <summary>Mouse capture is applied to a single element.</summary>
	Element,
	/// <summary>
	/// Mouse capture is applied to a subtree of elements: input over an element inside the
	/// capturing subtree goes to that element, otherwise to the capturing element.
	/// </summary>
	SubTree,
}
