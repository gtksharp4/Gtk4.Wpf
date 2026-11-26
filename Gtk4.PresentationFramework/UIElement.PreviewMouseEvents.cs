using System.Windows.Input;

namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>Occurs when a mouse button is pressed over this element (tunnel phase of MouseDown).</summary>
	public event MouseButtonEventHandler? PreviewMouseDown
	{
		add    => AddHandler(PreviewMouseDownEvent, value!);
		remove => RemoveHandler(PreviewMouseDownEvent, value!);
	}

	/// <summary>Occurs when a mouse button is released over this element (tunnel phase of MouseUp).</summary>
	public event MouseButtonEventHandler? PreviewMouseUp
	{
		add    => AddHandler(PreviewMouseUpEvent, value!);
		remove => RemoveHandler(PreviewMouseUpEvent, value!);
	}
}
