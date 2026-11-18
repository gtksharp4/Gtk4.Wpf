namespace System.Windows.Automation;

/// <summary>
/// Attached dependency properties that expose WPF UI-automation metadata.
/// </summary>
/// <remarks>
/// <para><b>GTK accessible push (best-effort):</b></para>
/// <list type="bullet">
///   <item>
///     <term><see cref="NameProperty"/></term>
///     <description>
///       Pushed to <c>Gtk.AccessibleProperty.Label</c> via
///       <c>Gtk.AccessibleExtensions.UpdateProperty</c> when the target element's
///       widget is realized. Requires GtkSharp4 ≥ 4.22 (which exposes
///       <c>Gtk.IAccessible</c> on <c>Gtk.Widget</c>).
///     </description>
///   </item>
///   <item>
///     <term><see cref="HelpTextProperty"/></term>
///     <description>
///       Pushed to <c>Gtk.AccessibleProperty.HelpText</c> via the same mechanism.
///     </description>
///   </item>
/// </list>
/// <para><b>Object-model only (no GTK push):</b></para>
/// <list type="bullet">
///   <item>
///     <term><see cref="AutomationIdProperty"/></term>
///     <description>Stored in the DP system; no equivalent GTK accessible property.</description>
///   </item>
///   <item>
///     <term><see cref="LabeledByProperty"/></term>
///     <description>
///       Stored in the DP system; full <c>Gtk.AccessibleRelation.LabelledBy</c> push
///       requires the labelling widget to be realized first, which cannot be guaranteed
///       at DP-change time.
///     </description>
///   </item>
///   <item>
///     <term><see cref="IsRequiredForFormProperty"/></term>
///     <description>Stored in the DP system; no direct GTK accessible property mapping.</description>
///   </item>
///   <item>
///     <term><see cref="LiveSettingProperty"/></term>
///     <description>
///       Stored in the DP system; GTK 4 live-region support requires calling
///       <c>GtkWidget.Announce()</c> on content change, not a persistent property.
///     </description>
///   </item>
/// </list>
/// </remarks>
public static class AutomationProperties
{
	// ── Name ──────────────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>AutomationProperties.Name</c> attached property.
	/// Default: <c>null</c>.
	/// <para>Pushed to <c>Gtk.AccessibleProperty.Label</c> on widget realization.</para>
	/// </summary>
	public static readonly DependencyProperty NameProperty =
		DependencyProperty.RegisterAttached(
			"Name",
			typeof(string),
			typeof(AutomationProperties),
			new PropertyMetadata(null, OnNameOrHelpTextChanged));

	/// <summary>Gets the accessible name override for <paramref name="element"/>.</summary>
	public static string? GetName(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return element.GetValue(NameProperty) as string;
	}

	/// <summary>Sets the accessible name override for <paramref name="element"/>.</summary>
	public static void SetName(DependencyObject element, string? value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(NameProperty, value);
	}

	// ── HelpText ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>AutomationProperties.HelpText</c> attached property.
	/// Default: <c>null</c>.
	/// <para>Pushed to <c>Gtk.AccessibleProperty.HelpText</c> on widget realization.</para>
	/// </summary>
	public static readonly DependencyProperty HelpTextProperty =
		DependencyProperty.RegisterAttached(
			"HelpText",
			typeof(string),
			typeof(AutomationProperties),
			new PropertyMetadata(null, OnNameOrHelpTextChanged));

	/// <summary>Gets the accessible help text for <paramref name="element"/>.</summary>
	public static string? GetHelpText(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return element.GetValue(HelpTextProperty) as string;
	}

	/// <summary>Sets the accessible help text for <paramref name="element"/>.</summary>
	public static void SetHelpText(DependencyObject element, string? value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(HelpTextProperty, value);
	}

	// ── AutomationId ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>AutomationProperties.AutomationId</c> attached property.
	/// Default: <c>null</c>. Object-model only — no GTK accessible push.
	/// </summary>
	public static readonly DependencyProperty AutomationIdProperty =
		DependencyProperty.RegisterAttached(
			"AutomationId",
			typeof(string),
			typeof(AutomationProperties),
			new PropertyMetadata(null));

	/// <summary>Gets the automation identifier for <paramref name="element"/>.</summary>
	public static string? GetAutomationId(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return element.GetValue(AutomationIdProperty) as string;
	}

	/// <summary>Sets the automation identifier for <paramref name="element"/>.</summary>
	public static void SetAutomationId(DependencyObject element, string? value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(AutomationIdProperty, value);
	}

	// ── LabeledBy ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>AutomationProperties.LabeledBy</c> attached property.
	/// Default: <c>null</c>. Object-model only — no GTK accessible push.
	/// </summary>
	public static readonly DependencyProperty LabeledByProperty =
		DependencyProperty.RegisterAttached(
			"LabeledBy",
			typeof(UIElement),
			typeof(AutomationProperties),
			new PropertyMetadata(null));

	/// <summary>Gets the <see cref="UIElement"/> that labels <paramref name="element"/>.</summary>
	public static UIElement? GetLabeledBy(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return element.GetValue(LabeledByProperty) as UIElement;
	}

	/// <summary>Sets the <see cref="UIElement"/> that labels <paramref name="element"/>.</summary>
	public static void SetLabeledBy(DependencyObject element, UIElement? value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(LabeledByProperty, value);
	}

	// ── IsRequiredForForm ─────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>AutomationProperties.IsRequiredForForm</c> attached property.
	/// Default: <c>false</c>. Object-model only — no GTK accessible push.
	/// </summary>
	public static readonly DependencyProperty IsRequiredForFormProperty =
		DependencyProperty.RegisterAttached(
			"IsRequiredForForm",
			typeof(bool),
			typeof(AutomationProperties),
			new PropertyMetadata(false));

	/// <summary>Gets whether <paramref name="element"/> is required to complete a form.</summary>
	public static bool GetIsRequiredForForm(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (bool)(element.GetValue(IsRequiredForFormProperty) ?? false);
	}

	/// <summary>Sets whether <paramref name="element"/> is required to complete a form.</summary>
	public static void SetIsRequiredForForm(DependencyObject element, bool value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(IsRequiredForFormProperty, value);
	}

	// ── LiveSetting ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>AutomationProperties.LiveSetting</c> attached property.
	/// Default: <see cref="AutomationLiveSetting.Off"/>. Object-model only — no GTK accessible push.
	/// </summary>
	public static readonly DependencyProperty LiveSettingProperty =
		DependencyProperty.RegisterAttached(
			"LiveSetting",
			typeof(AutomationLiveSetting),
			typeof(AutomationProperties),
			new PropertyMetadata(AutomationLiveSetting.Off));

	/// <summary>Gets the live-region politeness level for <paramref name="element"/>.</summary>
	public static AutomationLiveSetting GetLiveSetting(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (AutomationLiveSetting)(element.GetValue(LiveSettingProperty) ?? AutomationLiveSetting.Off);
	}

	/// <summary>Sets the live-region politeness level for <paramref name="element"/>.</summary>
	public static void SetLiveSetting(DependencyObject element, AutomationLiveSetting value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(LiveSettingProperty, value);
	}

	// ── GTK change callback ───────────────────────────────────────────────────────

	static void OnNameOrHelpTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement el && el.IsWidgetCreated)
			AutomationAccessiblePush.Push(el.Widget, el);
	}
}
