namespace System.Windows;

/// <summary>
/// Provides a minimal set of system-colour brushes and their corresponding
/// <see cref="Media.Color"/> values, mirroring WPF's
/// <c>System.Windows.SystemColors</c>.
/// <para>
/// Values are fixed to sensible GTK-neutral defaults; a later round will query
/// the actual GTK theme palette.  Brushes are pre-frozen shared instances
/// (safe to use as <see cref="Media.Brush"/> references in templates and
/// default styles).
/// </para>
/// </summary>
public static class SystemColors
{
	// ── Control (button/widget face) ─────────────────────────────────────────

	/// <summary>System control-face colour (#F0F0F0, light grey).</summary>
	public static Media.Color ControlColor { get; } =
		Media.Color.FromRgb(0xF0, 0xF0, 0xF0);

	/// <summary>Brush for the control-face colour.</summary>
	public static Media.SolidColorBrush ControlBrush { get; } =
		new Media.SolidColorBrush(ControlColor);

	// ── Window background ────────────────────────────────────────────────────

	/// <summary>System window-background colour (#FFFFFF, white).</summary>
	public static Media.Color WindowColor { get; } =
		Media.Color.FromRgb(0xFF, 0xFF, 0xFF);

	/// <summary>Brush for the window-background colour.</summary>
	public static Media.SolidColorBrush WindowBrush { get; } =
		new Media.SolidColorBrush(WindowColor);

	// ── Highlight (selection) ─────────────────────────────────────────────────

	/// <summary>System highlight colour (#0078D7, Windows blue).</summary>
	public static Media.Color HighlightColor { get; } =
		Media.Color.FromRgb(0x00, 0x78, 0xD7);

	/// <summary>Brush for the highlight (selection) colour.</summary>
	public static Media.SolidColorBrush HighlightBrush { get; } =
		new Media.SolidColorBrush(HighlightColor);

	// ── Control text ──────────────────────────────────────────────────────────

	/// <summary>System control-text colour (#000000, black).</summary>
	public static Media.Color ControlTextColor { get; } =
		Media.Color.FromRgb(0x00, 0x00, 0x00);

	/// <summary>Brush for the control-text colour.</summary>
	public static Media.SolidColorBrush ControlTextBrush { get; } =
		new Media.SolidColorBrush(ControlTextColor);

	// ── Window text ───────────────────────────────────────────────────────────

	/// <summary>System window-text colour (#000000, black).</summary>
	public static Media.Color WindowTextColor { get; } =
		Media.Color.FromRgb(0x00, 0x00, 0x00);

	/// <summary>Brush for the window-text colour.</summary>
	public static Media.SolidColorBrush WindowTextBrush { get; } =
		new Media.SolidColorBrush(WindowTextColor);
}