using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>
/// Spell-checking attached properties for text controls. The values are stored only; GTK spell checking
/// is not applied.
/// </summary>
public static class SpellCheck
{
	/// <summary>Identifies the <c>SpellCheck.IsEnabled</c> attached property.</summary>
	public static readonly DependencyProperty IsEnabledProperty =
		DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(SpellCheck),
			new PropertyMetadata(false));

	/// <summary>Identifies the <c>SpellCheck.CustomDictionaries</c> attached property.</summary>
	public static readonly DependencyProperty CustomDictionariesProperty =
		DependencyProperty.RegisterAttached("CustomDictionaries", typeof(IList), typeof(SpellCheck),
			new PropertyMetadata(null));

	/// <summary>Gets whether spell checking is enabled on <paramref name="element"/>.</summary>
	public static bool GetIsEnabled(DependencyObject element)
		=> (bool)(Require(element).GetValue(IsEnabledProperty) ?? false);

	/// <summary>Sets whether spell checking is enabled on <paramref name="element"/>.</summary>
	public static void SetIsEnabled(DependencyObject element, bool value)
		=> Require(element).SetValue(IsEnabledProperty, value);

	/// <summary>Gets the custom dictionaries of <paramref name="element"/>, creating the list on first use.</summary>
	public static IList GetCustomDictionaries(DependencyObject element)
	{
		Require(element);
		if (element.GetValue(CustomDictionariesProperty) is not IList list)
		{
			list = new List<Uri>();
			element.SetValue(CustomDictionariesProperty, list);
		}
		return list;
	}

	static DependencyObject Require(DependencyObject element)
		=> element ?? throw new ArgumentNullException(nameof(element));
}
