using System;
using System.Windows.Automation;

namespace Gtk4.Wpf.Testing;

/// <summary>
/// Semantic actions through the UI Automation patterns, the way most WPF UIA tests drive an app.
/// They skip native input handling: use <see cref="UiInput"/> for tests that depend on key or mouse events.
/// </summary>
public static class ElementActions
{
	public static InvokePattern AsInvoke(this AutomationElement e) => Pattern<InvokePattern>(e, InvokePattern.Pattern);

	public static ValuePattern AsValue(this AutomationElement e) => Pattern<ValuePattern>(e, ValuePattern.Pattern);

	public static TogglePattern AsToggle(this AutomationElement e) => Pattern<TogglePattern>(e, TogglePattern.Pattern);

	public static SelectionItemPattern AsSelectionItem(this AutomationElement e) =>
		Pattern<SelectionItemPattern>(e, SelectionItemPattern.Pattern);

	public static ExpandCollapsePattern AsExpandCollapse(this AutomationElement e) =>
		Pattern<ExpandCollapsePattern>(e, ExpandCollapsePattern.Pattern);

	public static RangeValuePattern AsRangeValue(this AutomationElement e) =>
		Pattern<RangeValuePattern>(e, RangeValuePattern.Pattern);

	/// <summary>Invokes the element (a button press). A list item with no Invoke pattern is selected instead.</summary>
	public static void Click(this AutomationElement element)
	{
		if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object? invoke)) ((InvokePattern)invoke!).Invoke();
		else element.Select();
	}

	/// <summary>Replaces the element's text through the Value pattern.</summary>
	public static void Type(this AutomationElement element, string text) => element.AsValue().SetValue(text);

	/// <summary>The element's current text: the Value pattern when it has one, else its name.</summary>
	public static string GetText(this AutomationElement element) =>
		element.TryGetCurrentPattern(ValuePattern.Pattern, out object? value)
			? ((ValuePattern)value!).Current.Value
			: element.Current.Name;

	/// <summary>Turns a check box or toggle on.</summary>
	public static void Check(this AutomationElement element) => element.SetToggle(ToggleState.On);

	/// <summary>Turns a check box or toggle off.</summary>
	public static void Uncheck(this AutomationElement element) => element.SetToggle(ToggleState.Off);

	/// <summary>Selects a list, tab, tree or combo item.</summary>
	public static void Select(this AutomationElement element) => element.AsSelectionItem().Select();

	public static void Expand(this AutomationElement element) => element.AsExpandCollapse().Expand();

	public static void Collapse(this AutomationElement element) => element.AsExpandCollapse().Collapse();

	static void SetToggle(this AutomationElement element, ToggleState wanted)
	{
		var toggle = element.AsToggle();
		// A tri-state control needs at most two steps to reach On or Off.
		for (int i = 0; i < 3 && toggle.Current.ToggleState != wanted; i++)
			toggle.Toggle();
		if (toggle.Current.ToggleState != wanted)
			throw new InvalidOperationException($"The element did not reach ToggleState.{wanted}.");
	}

	static T Pattern<T>(AutomationElement element, AutomationPattern pattern) where T : class
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (T)element.GetCurrentPattern(pattern);
	}
}
