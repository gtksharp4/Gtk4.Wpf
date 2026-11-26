namespace System.Windows.Input;

/// <summary>
/// Provides attached properties that control how keyboard tab-focus and directional-focus
/// navigate through element trees. Mirrors the WPF <c>System.Windows.Input.KeyboardNavigation</c>
/// static class surface.
/// </summary>
/// <remarks>
/// <b>GTK limitation:</b> GTK 4 does not expose a per-widget tab-order integer; it sequences
/// focusable widgets by visual tree order. <see cref="GetTabIndex"/> / <see cref="SetTabIndex"/>
/// store the value in the DP system for managed tab-order computation (see
/// <see cref="TabOrderHelper"/>) but do not affect the native GTK tab sequence.
/// </remarks>
public static class KeyboardNavigation
{
	// ── TabNavigation attached DP ─────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>TabNavigation</c> attached property.
	/// Default: <see cref="KeyboardNavigationMode.Continue"/>.
	/// </summary>
	public static readonly System.Windows.DependencyProperty TabNavigationProperty =
		System.Windows.DependencyProperty.RegisterAttached(
			"TabNavigation",
			typeof(KeyboardNavigationMode),
			typeof(KeyboardNavigation),
			new System.Windows.PropertyMetadata(KeyboardNavigationMode.Continue));

	/// <summary>Gets the tab-navigation mode for <paramref name="element"/>.</summary>
	public static KeyboardNavigationMode GetTabNavigation(System.Windows.DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (KeyboardNavigationMode)(element.GetValue(TabNavigationProperty)
			?? KeyboardNavigationMode.Continue);
	}

	/// <summary>Sets the tab-navigation mode on <paramref name="element"/>.</summary>
	public static void SetTabNavigation(
		System.Windows.DependencyObject element,
		KeyboardNavigationMode value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(TabNavigationProperty, value);
	}

	// ── DirectionalNavigation attached DP ────────────────────────────────────

	/// <summary>
	/// Identifies the <c>DirectionalNavigation</c> attached property (arrow-key navigation).
	/// Default: <see cref="KeyboardNavigationMode.Continue"/>.
	/// </summary>
	public static readonly System.Windows.DependencyProperty DirectionalNavigationProperty =
		System.Windows.DependencyProperty.RegisterAttached(
			"DirectionalNavigation",
			typeof(KeyboardNavigationMode),
			typeof(KeyboardNavigation),
			new System.Windows.PropertyMetadata(KeyboardNavigationMode.Continue));

	/// <summary>Gets the directional-navigation mode for <paramref name="element"/>.</summary>
	public static KeyboardNavigationMode GetDirectionalNavigation(
		System.Windows.DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (KeyboardNavigationMode)(element.GetValue(DirectionalNavigationProperty)
			?? KeyboardNavigationMode.Continue);
	}

	/// <summary>Sets the directional-navigation mode on <paramref name="element"/>.</summary>
	public static void SetDirectionalNavigation(
		System.Windows.DependencyObject element,
		KeyboardNavigationMode value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(DirectionalNavigationProperty, value);
	}

	// ── IsTabStop attached ────────────────────────────────────────────────────

	/// <summary>
	/// The <c>KeyboardNavigation.IsTabStop</c> attached property. Independent of
	/// <see cref="System.Windows.UIElement.IsTabStopProperty"/> — WPF exposes the primary
	/// tab-stop flag on the element (<c>UIElement.IsTabStop</c>); this attached form mirrors
	/// WPF's surface for XAML that writes <c>KeyboardNavigation.IsTabStop</c> but does not
	/// cross-propagate to the element's own DP.
	/// </summary>
	public static readonly System.Windows.DependencyProperty IsTabStopProperty =
		System.Windows.DependencyProperty.RegisterAttached(
			"IsTabStop",
			typeof(bool),
			typeof(KeyboardNavigation),
			new System.Windows.PropertyMetadata(true));

	/// <summary>Gets the IsTabStop attached value from <paramref name="element"/>.</summary>
	public static bool GetIsTabStop(System.Windows.DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (bool)(element.GetValue(IsTabStopProperty) ?? true);
	}

	/// <summary>Sets the IsTabStop attached value on <paramref name="element"/>.</summary>
	public static void SetIsTabStop(System.Windows.DependencyObject element, bool value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(IsTabStopProperty, value);
	}

	// ── TabIndex attached ─────────────────────────────────────────────────────

	/// <summary>
	/// The <c>KeyboardNavigation.TabIndex</c> attached property. Independent of
	/// <see cref="System.Windows.UIElement.TabIndexProperty"/> (does not cross-propagate).
	/// </summary>
	public static readonly System.Windows.DependencyProperty TabIndexProperty =
		System.Windows.DependencyProperty.RegisterAttached(
			"TabIndex",
			typeof(int),
			typeof(KeyboardNavigation),
			new System.Windows.PropertyMetadata(int.MaxValue));

	/// <summary>Gets the tab index attached to <paramref name="element"/>.</summary>
	public static int GetTabIndex(System.Windows.DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (int)(element.GetValue(TabIndexProperty) ?? int.MaxValue);
	}

	/// <summary>Sets the tab index on <paramref name="element"/>.</summary>
	public static void SetTabIndex(System.Windows.DependencyObject element, int value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(TabIndexProperty, value);
	}
}
