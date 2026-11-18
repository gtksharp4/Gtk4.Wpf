namespace System.Windows.Automation;

/// <summary>
/// Specifies the live-region politeness level for screen readers.
/// Maps to the WPF <c>AutomationProperties.LiveSetting</c> attached property.
/// </summary>
/// <remarks>
/// Object-model only: this value is stored in the DP system but not pushed to a GTK
/// accessible property. GTK 4 exposes live-region semantics only via
/// <c>GtkWidget.Announce()</c> (not a persistent property); callers should trigger
/// announcements explicitly when content changes.
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
