using System.Windows.Input;

namespace System.Windows;

/// <summary>WPF's protected virtual mouse hooks, driven by class handlers so they run before instance handlers.</summary>
public abstract partial class UIElement
{
	/// <summary>Called when the pointer enters the element.</summary>
	protected virtual void OnMouseEnter(MouseEventArgs e) { }
	/// <summary>Called when the pointer leaves the element.</summary>
	protected virtual void OnMouseLeave(MouseEventArgs e) { }
	/// <summary>Called when the pointer moves over the element.</summary>
	protected virtual void OnMouseMove(MouseEventArgs e) { }
	/// <summary>Called on PreviewMouseMove.</summary>
	protected virtual void OnPreviewMouseMove(MouseEventArgs e) { }
	/// <summary>Called when a mouse button is pressed.</summary>
	protected virtual void OnMouseDown(MouseButtonEventArgs e) { }
	/// <summary>Called when a mouse button is released.</summary>
	protected virtual void OnMouseUp(MouseButtonEventArgs e) { }
	/// <summary>Called on PreviewMouseDown.</summary>
	protected virtual void OnPreviewMouseDown(MouseButtonEventArgs e) { }
	/// <summary>Called on PreviewMouseUp.</summary>
	protected virtual void OnPreviewMouseUp(MouseButtonEventArgs e) { }
	/// <summary>Called when the left button is pressed.</summary>
	protected virtual void OnMouseLeftButtonDown(MouseButtonEventArgs e) { }
	/// <summary>Called when the left button is released.</summary>
	protected virtual void OnMouseLeftButtonUp(MouseButtonEventArgs e) { }
	/// <summary>Called when the right button is pressed.</summary>
	protected virtual void OnMouseRightButtonDown(MouseButtonEventArgs e) { }
	/// <summary>Called when the right button is released.</summary>
	protected virtual void OnMouseRightButtonUp(MouseButtonEventArgs e) { }
	/// <summary>Called on PreviewMouseLeftButtonDown.</summary>
	protected virtual void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) { }
	/// <summary>Called on PreviewMouseLeftButtonUp.</summary>
	protected virtual void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e) { }
	/// <summary>Called on PreviewMouseRightButtonDown.</summary>
	protected virtual void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e) { }
	/// <summary>Called on PreviewMouseRightButtonUp.</summary>
	protected virtual void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e) { }
	/// <summary>Called when the wheel turns over the element.</summary>
	protected virtual void OnMouseWheel(MouseWheelEventArgs e) { }
	/// <summary>Called on PreviewMouseWheel.</summary>
	protected virtual void OnPreviewMouseWheel(MouseWheelEventArgs e) { }
	/// <summary>Called when the element captures the mouse.</summary>
	protected virtual void OnGotMouseCapture(MouseEventArgs e) { }
	/// <summary>Called when the element loses mouse capture.</summary>
	protected virtual void OnLostMouseCapture(MouseEventArgs e) { }

	// Registered from the static constructor. Handled events skip these (handledEventsToo = false), as in WPF.
	static void RegisterMouseVirtualHandlers()
	{
		Btn(PreviewMouseDownEvent, (u, e) => u.OnPreviewMouseDown(e));
		Btn(MouseDownEvent, (u, e) => u.OnMouseDown(e));
		Btn(PreviewMouseUpEvent, (u, e) => u.OnPreviewMouseUp(e));
		Btn(MouseUpEvent, (u, e) => u.OnMouseUp(e));
		Btn(PreviewMouseLeftButtonDownEvent, (u, e) => u.OnPreviewMouseLeftButtonDown(e));
		Btn(MouseLeftButtonDownEvent, (u, e) => u.OnMouseLeftButtonDown(e));
		Btn(PreviewMouseLeftButtonUpEvent, (u, e) => u.OnPreviewMouseLeftButtonUp(e));
		Btn(MouseLeftButtonUpEvent, (u, e) => u.OnMouseLeftButtonUp(e));
		Btn(PreviewMouseRightButtonDownEvent, (u, e) => u.OnPreviewMouseRightButtonDown(e));
		Btn(MouseRightButtonDownEvent, (u, e) => u.OnMouseRightButtonDown(e));
		Btn(PreviewMouseRightButtonUpEvent, (u, e) => u.OnPreviewMouseRightButtonUp(e));
		Btn(MouseRightButtonUpEvent, (u, e) => u.OnMouseRightButtonUp(e));
		Mov(MouseEnterEvent, (u, e) => u.OnMouseEnter(e));
		Mov(MouseLeaveEvent, (u, e) => u.OnMouseLeave(e));
		Mov(MouseMoveEvent, (u, e) => u.OnMouseMove(e));
		Mov(PreviewMouseMoveEvent, (u, e) => u.OnPreviewMouseMove(e));
		Mov(GotMouseCaptureEvent, (u, e) => u.OnGotMouseCapture(e));
		Mov(LostMouseCaptureEvent, (u, e) => u.OnLostMouseCapture(e));
		Whl(MouseWheelEvent, (u, e) => u.OnMouseWheel(e));
		Whl(PreviewMouseWheelEvent, (u, e) => u.OnPreviewMouseWheel(e));
	}

	static void Btn(RoutedEvent ev, Action<UIElement, MouseButtonEventArgs> call) =>
		EventManager.RegisterClassHandler(typeof(UIElement), ev,
			new MouseButtonEventHandler((s, e) => { if (s is UIElement u) call(u, e); }), false);

	static void Mov(RoutedEvent ev, Action<UIElement, MouseEventArgs> call) =>
		EventManager.RegisterClassHandler(typeof(UIElement), ev,
			new MouseEventHandler((s, e) => { if (s is UIElement u) call(u, e); }), false);

	static void Whl(RoutedEvent ev, Action<UIElement, MouseWheelEventArgs> call) =>
		EventManager.RegisterClassHandler(typeof(UIElement), ev,
			new MouseWheelEventHandler((s, e) => { if (s is UIElement u) call(u, e); }), false);
}
