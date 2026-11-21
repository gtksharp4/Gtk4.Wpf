using System.Windows.Media;

namespace System.Windows;

/// <summary>The Windows default (Aero) palette behind <see cref="SystemColors"/>.</summary>
static class SystemPalette
{
	internal static readonly SystemColorEntry ActiveBorder = Make("ActiveBorder", 180, 180, 180);
	internal static readonly SystemColorEntry ActiveCaption = Make("ActiveCaption", 153, 180, 209);
	internal static readonly SystemColorEntry ActiveCaptionText = Make("ActiveCaptionText", 0, 0, 0);
	internal static readonly SystemColorEntry AppWorkspace = Make("AppWorkspace", 171, 171, 171);
	internal static readonly SystemColorEntry Control = Make("Control", 240, 240, 240);
	internal static readonly SystemColorEntry ControlDark = Make("ControlDark", 160, 160, 160);
	internal static readonly SystemColorEntry ControlDarkDark = Make("ControlDarkDark", 105, 105, 105);
	internal static readonly SystemColorEntry ControlLight = Make("ControlLight", 227, 227, 227);
	internal static readonly SystemColorEntry ControlLightLight = Make("ControlLightLight", 255, 255, 255);
	internal static readonly SystemColorEntry ControlText = Make("ControlText", 0, 0, 0);
	internal static readonly SystemColorEntry Desktop = Make("Desktop", 0, 0, 0);
	internal static readonly SystemColorEntry GradientActiveCaption = Make("GradientActiveCaption", 185, 209, 234);
	internal static readonly SystemColorEntry GradientInactiveCaption = Make("GradientInactiveCaption", 215, 228, 242);
	internal static readonly SystemColorEntry GrayText = Make("GrayText", 109, 109, 109);
	internal static readonly SystemColorEntry Highlight = Make("Highlight", 0, 120, 215);
	internal static readonly SystemColorEntry HighlightText = Make("HighlightText", 255, 255, 255);
	internal static readonly SystemColorEntry HotTrack = Make("HotTrack", 0, 102, 204);
	internal static readonly SystemColorEntry InactiveBorder = Make("InactiveBorder", 244, 247, 252);
	internal static readonly SystemColorEntry InactiveCaption = Make("InactiveCaption", 191, 205, 219);
	internal static readonly SystemColorEntry InactiveCaptionText = Make("InactiveCaptionText", 67, 78, 84);
	internal static readonly SystemColorEntry InactiveSelectionHighlight
		= Make("InactiveSelectionHighlight", 240, 240, 240);
	internal static readonly SystemColorEntry InactiveSelectionHighlightText
		= Make("InactiveSelectionHighlightText", 0, 0, 0);
	internal static readonly SystemColorEntry Info = Make("Info", 255, 255, 225);
	internal static readonly SystemColorEntry InfoText = Make("InfoText", 0, 0, 0);
	internal static readonly SystemColorEntry Menu = Make("Menu", 240, 240, 240);
	internal static readonly SystemColorEntry MenuBar = Make("MenuBar", 240, 240, 240);
	internal static readonly SystemColorEntry MenuHighlight = Make("MenuHighlight", 51, 153, 255);
	internal static readonly SystemColorEntry MenuText = Make("MenuText", 0, 0, 0);
	internal static readonly SystemColorEntry ScrollBar = Make("ScrollBar", 200, 200, 200);
	internal static readonly SystemColorEntry Window = Make("Window", 255, 255, 255);
	internal static readonly SystemColorEntry WindowFrame = Make("WindowFrame", 100, 100, 100);
	internal static readonly SystemColorEntry WindowText = Make("WindowText", 0, 0, 0);

	static SystemColorEntry Make(string n, byte r, byte g, byte b) => new SystemColorEntry(n, r, g, b);

	internal static readonly SystemColorEntry[] All =
	{
		ActiveBorder,
		ActiveCaption,
		ActiveCaptionText,
		AppWorkspace,
		Control,
		ControlDark,
		ControlDarkDark,
		ControlLight,
		ControlLightLight,
		ControlText,
		Desktop,
		GradientActiveCaption,
		GradientInactiveCaption,
		GrayText,
		Highlight,
		HighlightText,
		HotTrack,
		InactiveBorder,
		InactiveCaption,
		InactiveCaptionText,
		InactiveSelectionHighlight,
		InactiveSelectionHighlightText,
		Info,
		InfoText,
		Menu,
		MenuBar,
		MenuHighlight,
		MenuText,
		ScrollBar,
		Window,
		WindowFrame,
		WindowText,
	};
}
