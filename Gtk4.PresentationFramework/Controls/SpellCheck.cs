using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>
/// Spell-checking support for text controls: the attached properties plus the per-control instance
/// exposed as <c>TextBox.SpellCheck</c>. The values are stored only; GTK spell checking is not applied.
/// </summary>
public sealed class SpellCheck
{
	readonly DependencyObject _owner;

	internal SpellCheck(DependencyObject owner) => _owner = owner;

	/// <summary>Identifies the <c>SpellCheck.IsEnabled</c> attached property.</summary>
	public static readonly DependencyProperty IsEnabledProperty =
		DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(SpellCheck),
			new PropertyMetadata(false));

	/// <summary>Identifies the <c>SpellCheck.SpellingReform</c> attached property.</summary>
	public static readonly DependencyProperty SpellingReformProperty =
		DependencyProperty.RegisterAttached("SpellingReform", typeof(SpellingReform), typeof(SpellCheck),
			new PropertyMetadata(SpellingReform.PreAndPostreform));

	/// <summary>Identifies the <c>SpellCheck.CustomDictionaries</c> attached property.</summary>
	public static readonly DependencyProperty CustomDictionariesProperty =
		DependencyProperty.RegisterAttached("CustomDictionaries", typeof(IList), typeof(SpellCheck),
			new PropertyMetadata(null));

	/// <summary>Gets or sets whether spell checking is enabled on the owning control.</summary>
	public bool IsEnabled
	{
		get => GetIsEnabled(_owner);
		set => SetIsEnabled(_owner, value);
	}

	/// <summary>Gets or sets the spelling reform used by the owning control.</summary>
	public SpellingReform SpellingReform
	{
		get => GetSpellingReform(_owner);
		set => SetSpellingReform(_owner, value);
	}

	/// <summary>Gets the custom dictionaries of the owning control.</summary>
	public IList CustomDictionaries => GetCustomDictionaries(_owner);

	/// <summary>Gets whether spell checking is enabled on <paramref name="element"/>.</summary>
	public static bool GetIsEnabled(DependencyObject element)
		=> (bool)(Require(element).GetValue(IsEnabledProperty) ?? false);

	/// <summary>Sets whether spell checking is enabled on <paramref name="element"/>.</summary>
	public static void SetIsEnabled(DependencyObject element, bool value)
		=> Require(element).SetValue(IsEnabledProperty, value);

	internal static SpellingReform GetSpellingReform(DependencyObject element)
		=> (SpellingReform)Require(element).GetValue(SpellingReformProperty)!;

	/// <summary>Sets the spelling reform of <paramref name="element"/>.</summary>
	public static void SetSpellingReform(DependencyObject element, SpellingReform value)
		=> Require(element).SetValue(SpellingReformProperty, value);

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

/// <summary>Spelling rules used by <see cref="SpellCheck"/> (applies to French and German).</summary>
public enum SpellingReform
{
	/// <summary>Pre- and post-reform spellings are both accepted.</summary>
	PreAndPostreform = 0,
	/// <summary>Pre-reform spellings only.</summary>
	Prereform = 1,
	/// <summary>Post-reform spellings only.</summary>
	Postreform = 2,
}
