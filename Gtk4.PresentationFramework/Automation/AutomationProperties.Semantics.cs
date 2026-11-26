namespace System.Windows.Automation;

/// <summary>
/// Object-model-only <see cref="AutomationProperties"/> attached properties
/// (<c>LabeledBy</c>, <c>IsRequiredForForm</c>, <c>LiveSetting</c>) — stored in the DP
/// system with no GTK accessible push. Split from <c>AutomationProperties.cs</c> to keep
/// each file ≤200 lines.
/// </summary>
public static partial class AutomationProperties
{
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
}
