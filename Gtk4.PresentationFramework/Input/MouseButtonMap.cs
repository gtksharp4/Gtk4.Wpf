namespace System.Windows.Input;

/// <summary>Pure mapping from GDK button numbers to WPF <see cref="MouseButton"/> values.</summary>
/// <remarks>
/// GDK numbers the primary/middle/secondary buttons 1/2/3 on every backend, but the extended
/// (back/forward) buttons differ: X11 and Wayland report 8/9, while the Win32 backend
/// (<c>WM_XBUTTONDOWN</c> XBUTTON1/XBUTTON2) and the macOS backend (<c>buttonNumber + 1</c>)
/// report 4/5. GTK 4 delivers wheel motion as separate scroll events — never as button 4–7
/// presses — so 4/5 can only be the extended buttons. Any other number has no WPF
/// <see cref="MouseButton"/>; WPF raises nothing for such buttons.
/// </remarks>
internal static class MouseButtonMap
{
	/// <summary>
	/// Maps <paramref name="gdkButton"/> to the WPF button. Returns <see langword="false"/> for a
	/// number WPF has no button for (the caller then raises no mouse-button event).
	/// </summary>
	internal static bool TryFromGdk(uint gdkButton, out MouseButton button)
	{
		switch (gdkButton)
		{
			case 1u: button = MouseButton.Left; return true;
			case 2u: button = MouseButton.Middle; return true;
			case 3u: button = MouseButton.Right; return true;
			case 4u:
			case 8u: button = MouseButton.XButton1; return true;
			case 5u:
			case 9u: button = MouseButton.XButton2; return true;
			default: button = MouseButton.Left; return false;
		}
	}
}
