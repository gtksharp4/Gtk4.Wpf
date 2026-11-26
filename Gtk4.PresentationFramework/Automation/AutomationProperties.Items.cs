using System.Windows.Automation.Peers;

namespace System.Windows.Automation;

/// <summary>
/// <c>AcceleratorKey</c>, <c>ItemStatus</c> and <c>HeadingLevel</c>, registered as WPF does:
/// not inherited, no effect on measure or render.
/// </summary>
public static partial class AutomationProperties
{
	/// <summary>Identifies the <c>AutomationProperties.AcceleratorKey</c> attached property.</summary>
	public static readonly DependencyProperty AcceleratorKeyProperty =
		DependencyProperty.RegisterAttached(
			"AcceleratorKey",
			typeof(string),
			typeof(AutomationProperties),
			new UIPropertyMetadata(string.Empty),
			IsNotNull);

	/// <summary>Gets the accelerator key text of <paramref name="element"/>.</summary>
	public static string GetAcceleratorKey(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (string)(element.GetValue(AcceleratorKeyProperty) ?? string.Empty);
	}

	/// <summary>Sets the accelerator key text of <paramref name="element"/>.</summary>
	public static void SetAcceleratorKey(DependencyObject element, string value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(AcceleratorKeyProperty, value);
	}

	/// <summary>Identifies the <c>AutomationProperties.ItemStatus</c> attached property.</summary>
	public static readonly DependencyProperty ItemStatusProperty =
		DependencyProperty.RegisterAttached(
			"ItemStatus",
			typeof(string),
			typeof(AutomationProperties),
			new UIPropertyMetadata(string.Empty),
			IsNotNull);

	/// <summary>Gets the item status text of <paramref name="element"/>.</summary>
	public static string GetItemStatus(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (string)(element.GetValue(ItemStatusProperty) ?? string.Empty);
	}

	/// <summary>Sets the item status text of <paramref name="element"/>.</summary>
	public static void SetItemStatus(DependencyObject element, string value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(ItemStatusProperty, value);
	}

	/// <summary>Identifies the <c>AutomationProperties.HeadingLevel</c> attached property.</summary>
	public static readonly DependencyProperty HeadingLevelProperty =
		DependencyProperty.RegisterAttached(
			"HeadingLevel",
			typeof(AutomationHeadingLevel),
			typeof(AutomationProperties),
			new UIPropertyMetadata(AutomationHeadingLevel.None));

	/// <summary>Gets the heading level of <paramref name="element"/>.</summary>
	public static AutomationHeadingLevel GetHeadingLevel(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (AutomationHeadingLevel)(element.GetValue(HeadingLevelProperty) ?? AutomationHeadingLevel.None);
	}

	/// <summary>Sets the heading level of <paramref name="element"/>.</summary>
	public static void SetHeadingLevel(DependencyObject element, AutomationHeadingLevel value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(HeadingLevelProperty, value);
	}

	static bool IsNotNull(object? value) => value != null;
}
