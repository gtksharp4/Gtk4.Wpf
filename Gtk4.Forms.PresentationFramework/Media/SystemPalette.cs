using System.Windows.Media;

namespace System.Windows;

/// <summary>The Windows default (Aero) palette behind <see cref="SystemColors"/>.</summary>
static class SystemPalette
{
	internal static readonly SystemColorEntry ActiveBorder = new SystemColorEntry("ActiveBorder", 180, 180, 180);
	internal static readonly SystemColorEntry ActiveCaption = new SystemColorEntry("ActiveCaption", 153, 180, 209);
	internal static readonly SystemColorEntry ActiveCaptionText = new SystemColorEntry("ActiveCaptionText", 0, 0, 0);
	internal static readonly SystemColorEntry AppWorkspace = new SystemColorEntry("AppWorkspace", 171, 171, 171);
	internal static readonly SystemColorEntry Control = new SystemColorEntry("Control", 240, 240, 240);
	internal static readonly SystemColorEntry ControlDark = new SystemColorEntry("ControlDark", 160, 160, 160);
	internal static readonly SystemColorEntry ControlDarkDark = new SystemColorEntry("ControlDarkDark", 105, 105, 105);
	internal static readonly SystemColorEntry ControlLight = new SystemColorEntry("ControlLight", 227, 227, 227);
	internal static readonly SystemColorEntry ControlLightLight = new SystemColorEntry("ControlLightLight", 255, 255, 255);
	internal static readonly SystemColorEntry ControlText = new SystemColorEntry("ControlText", 0, 0, 0);
	internal static readonly SystemColorEntry Desktop = new SystemColorEntry("Desktop", 0, 0, 0);
	internal static readonly SystemColorEntry GradientActiveCaption = new SystemColorEntry("GradientActiveCaption", 185, 209, 234);
	internal static readonly SystemColorEntry GradientInactiveCaption = new SystemColorEntry("GradientInactiveCaption", 215, 228, 242);
	internal static readonly SystemColorEntry GrayText = new SystemColorEntry("GrayText", 109, 109, 109);
	internal static readonly SystemColorEntry Highlight = new SystemColorEntry("Highlight", 0, 120, 215);
	internal static readonly SystemColorEntry HighlightText = new SystemColorEntry("HighlightText", 255, 255, 255);
	internal static readonly SystemColorEntry HotTrack = new SystemColorEntry("HotTrack", 0, 102, 204);
	internal static readonly SystemColorEntry InactiveBorder = new SystemColorEntry("InactiveBorder", 244, 247, 252);
	internal static readonly SystemColorEntry InactiveCaption = new SystemColorEntry("InactiveCaption", 191, 205, 219);
	internal static readonly SystemColorEntry InactiveCaptionText = new SystemColorEntry("InactiveCaptionText", 67, 78, 84);
	internal static readonly SystemColorEntry InactiveSelectionHighlight = new SystemColorEntry("InactiveSelectionHighlight", 240, 240, 240);
	internal static readonly SystemColorEntry InactiveSelectionHighlightText = new SystemColorEntry("InactiveSelectionHighlightText", 0, 0, 0);
	internal static readonly SystemColorEntry Info = new SystemColorEntry("Info", 255, 255, 225);
	internal static readonly SystemColorEntry InfoText = new SystemColorEntry("InfoText", 0, 0, 0);
	internal static readonly SystemColorEntry Menu = new SystemColorEntry("Menu", 240, 240, 240);
	internal static readonly SystemColorEntry MenuBar = new SystemColorEntry("MenuBar", 240, 240, 240);
	internal static readonly SystemColorEntry MenuHighlight = new SystemColorEntry("MenuHighlight", 51, 153, 255);
	internal static readonly SystemColorEntry MenuText = new SystemColorEntry("MenuText", 0, 0, 0);
	internal static readonly SystemColorEntry ScrollBar = new SystemColorEntry("ScrollBar", 200, 200, 200);
	internal static readonly SystemColorEntry Window = new SystemColorEntry("Window", 255, 255, 255);
	internal static readonly SystemColorEntry WindowFrame = new SystemColorEntry("WindowFrame", 100, 100, 100);
	internal static readonly SystemColorEntry WindowText = new SystemColorEntry("WindowText", 0, 0, 0);

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
