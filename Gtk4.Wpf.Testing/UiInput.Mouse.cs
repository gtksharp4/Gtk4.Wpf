using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;

namespace Gtk4.Wpf.Testing;

public sealed partial class UiInput
{
	/// <summary>
	/// Mouse events at the centre of an element (or an explicit point in its own coordinates). Enter and leave are
	/// raised on the target only, as the layer's pointer seam does. Pass the <see cref="UIElement"/> for elements
	/// with no automation peer, such as a <c>Border</c>.
	/// </summary>
	public sealed class MouseInput
	{
		readonly UiInput _input;

		internal MouseInput(UiInput input) => _input = input;

		/// <summary>Moves the pointer onto the element: <c>MouseEnter</c> then <c>MouseMove</c>.</summary>
		public void MoveTo(AutomationElement element, Point? at = null) => MoveTo(_input.Resolve(element), at);

		public void MoveTo(UIElement target, Point? at = null) => GtkThread.Invoke(() =>
		{
			RequireEnabled(target);
			target.SimulateMouseEnterForTest();
			target.RaiseMouseMove(at ?? Centre(target));
		});

		/// <summary>Moves the pointer off the element: <c>MouseLeave</c>.</summary>
		public void Leave(AutomationElement element) => Leave(_input.Resolve(element));

		public void Leave(UIElement target) => GtkThread.Invoke(target.SimulateMouseLeaveForTest);

		/// <summary>Moves onto the element, presses and releases <paramref name="button"/>.</summary>
		public void Click(AutomationElement element, MouseButton button = MouseButton.Left, int clickCount = 1) =>
			Click(_input.Resolve(element), button, clickCount);

		public void Click(UIElement target, MouseButton button = MouseButton.Left, int clickCount = 1) =>
			GtkThread.Invoke(() =>
			{
				RequireEnabled(target);
				var point = Centre(target);
				target.SimulateMouseEnterForTest();
				target.RaiseMouseMove(point);
				target.RaiseMouseButton(button, point, clickCount, pressed: true);
				target.RaiseMouseButton(button, point, clickCount, pressed: false);
			});

		public void DoubleClick(AutomationElement element, MouseButton button = MouseButton.Left)
		{
			Click(element, button, 1);
			Click(element, button, 2);
		}

		static Point Centre(UIElement element) =>
			element is FrameworkElement fe ? new Point(fe.ActualWidth / 2, fe.ActualHeight / 2) : new Point(0, 0);
	}
}
