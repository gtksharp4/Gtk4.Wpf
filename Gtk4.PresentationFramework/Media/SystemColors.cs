using System.Windows.Media;

namespace System.Windows;

/// <summary>The system colours as WPF's <c>SystemColors</c> exposes them: colour, shared brush and resource keys.
/// GTK 4 has no portable system-colour API, so every value is the Windows default (Aero) palette.
/// The brushes are cached shared instances; this layer's Brush has no Freeze, so they are not frozen.</summary>
public static partial class SystemColors
{
	/// <summary>The active window border colour.</summary>
	public static Color ActiveBorderColor => SystemPalette.ActiveBorder.Color;

	/// <summary>The active window title bar colour.</summary>
	public static Color ActiveCaptionColor => SystemPalette.ActiveCaption.Color;

	/// <summary>The text in the active window title bar colour.</summary>
	public static Color ActiveCaptionTextColor => SystemPalette.ActiveCaptionText.Color;

	/// <summary>The MDI application workspace colour.</summary>
	public static Color AppWorkspaceColor => SystemPalette.AppWorkspace.Color;

	/// <summary>The control face colour.</summary>
	public static Color ControlColor => SystemPalette.Control.Color;

	/// <summary>The control shadow colour.</summary>
	public static Color ControlDarkColor => SystemPalette.ControlDark.Color;

	/// <summary>The control dark shadow colour.</summary>
	public static Color ControlDarkDarkColor => SystemPalette.ControlDarkDark.Color;

	/// <summary>The control light edge colour.</summary>
	public static Color ControlLightColor => SystemPalette.ControlLight.Color;

	/// <summary>The control highlight colour.</summary>
	public static Color ControlLightLightColor => SystemPalette.ControlLightLight.Color;

	/// <summary>The control text colour.</summary>
	public static Color ControlTextColor => SystemPalette.ControlText.Color;

	/// <summary>The desktop colour.</summary>
	public static Color DesktopColor => SystemPalette.Desktop.Color;

	/// <summary>The right side of the active title bar gradient colour.</summary>
	public static Color GradientActiveCaptionColor => SystemPalette.GradientActiveCaption.Color;

	/// <summary>The right side of the inactive title bar gradient colour.</summary>
	public static Color GradientInactiveCaptionColor => SystemPalette.GradientInactiveCaption.Color;

	/// <summary>The disabled text colour.</summary>
	public static Color GrayTextColor => SystemPalette.GrayText.Color;

	/// <summary>The selected items colour.</summary>
	public static Color HighlightColor => SystemPalette.Highlight.Color;

	/// <summary>The text of selected items colour.</summary>
	public static Color HighlightTextColor => SystemPalette.HighlightText.Color;

	/// <summary>The hot-tracked items colour.</summary>
	public static Color HotTrackColor => SystemPalette.HotTrack.Color;

	/// <summary>The inactive window border colour.</summary>
	public static Color InactiveBorderColor => SystemPalette.InactiveBorder.Color;

	/// <summary>The inactive window title bar colour.</summary>
	public static Color InactiveCaptionColor => SystemPalette.InactiveCaption.Color;

	/// <summary>The text in the inactive window title bar colour.</summary>
	public static Color InactiveCaptionTextColor => SystemPalette.InactiveCaptionText.Color;

	/// <summary>The selected items in an unfocused control colour.</summary>
	public static Color InactiveSelectionHighlightColor => SystemPalette.InactiveSelectionHighlight.Color;

	/// <summary>The text of selected items in an unfocused control colour.</summary>
	public static Color InactiveSelectionHighlightTextColor => SystemPalette.InactiveSelectionHighlightText.Color;

	/// <summary>The tooltip background colour.</summary>
	public static Color InfoColor => SystemPalette.Info.Color;

	/// <summary>The tooltip text colour.</summary>
	public static Color InfoTextColor => SystemPalette.InfoText.Color;

	/// <summary>The menu background colour.</summary>
	public static Color MenuColor => SystemPalette.Menu.Color;

	/// <summary>The menu bar background colour.</summary>
	public static Color MenuBarColor => SystemPalette.MenuBar.Color;

	/// <summary>The highlighted menu items colour.</summary>
	public static Color MenuHighlightColor => SystemPalette.MenuHighlight.Color;

	/// <summary>The menu text colour.</summary>
	public static Color MenuTextColor => SystemPalette.MenuText.Color;

	/// <summary>The scroll bar background colour.</summary>
	public static Color ScrollBarColor => SystemPalette.ScrollBar.Color;

	/// <summary>The window background colour.</summary>
	public static Color WindowColor => SystemPalette.Window.Color;

	/// <summary>The window frame colour.</summary>
	public static Color WindowFrameColor => SystemPalette.WindowFrame.Color;

	/// <summary>The window text colour.</summary>
	public static Color WindowTextColor => SystemPalette.WindowText.Color;
}
