namespace System.Windows.Automation;

/// <summary>
/// Specifies the live-region politeness level for screen readers.
/// Maps to the WPF <c>AutomationProperties.LiveSetting</c> attached property.
/// </summary>
/// <remarks>
/// GTK 4 has no persistent live-region property, so the value is not pushed. As in WPF the app raises
/// <c>AutomationEvents.LiveRegionChanged</c> on the peer; the peer then calls <c>gtk_accessible_announce</c>
/// with its name: high priority for <see cref="Assertive"/>, medium for <see cref="Polite"/>.
/// </remarks>
public enum AutomationLiveSetting
{
	/// <summary>The region is not live (no automatic announcements).</summary>
	Off = 0,

	/// <summary>Updates are announced when the user is idle.</summary>
	Polite = 1,

	/// <summary>Updates are announced immediately, interrupting the user.</summary>
	Assertive = 2,
}
