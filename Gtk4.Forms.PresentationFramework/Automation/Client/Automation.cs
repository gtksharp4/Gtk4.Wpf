using System.Windows.Automation.Peers;

namespace System.Windows.Automation;

/// <summary>Static helpers of the UI Automation client: view conditions, comparisons and event handlers.</summary>
public static partial class Automation
{
	/// <summary>Matches every element.</summary>
	public static readonly Condition RawViewCondition = Condition.TrueCondition;

	/// <summary>Matches the elements that are controls.</summary>
	public static readonly Condition ControlViewCondition =
		new PropertyCondition(AutomationElementIdentifiers.IsControlElementProperty, true);

	/// <summary>Matches the elements that are controls and carry content.</summary>
	public static readonly Condition ContentViewCondition = new AndCondition(
		new PropertyCondition(AutomationElementIdentifiers.IsControlElementProperty, true),
		new PropertyCondition(AutomationElementIdentifiers.IsContentElementProperty, true));

	/// <summary>Returns whether the two elements are the same.</summary>
	public static bool Compare(AutomationElement el1, AutomationElement el2) =>
		el1 == null || el2 == null ? ReferenceEquals(el1, el2) : el1.Equals(el2);

	/// <summary>Returns whether the two runtime ids are equal.</summary>
	public static bool Compare(int[] runtimeId1, int[] runtimeId2)
	{
		if (runtimeId1 == null) throw new ArgumentNullException(nameof(runtimeId1));
		if (runtimeId2 == null) throw new ArgumentNullException(nameof(runtimeId2));
		if (runtimeId1.Length != runtimeId2.Length) return false;
		for (int i = 0; i < runtimeId1.Length; i++)
			if (runtimeId1[i] != runtimeId2[i])
				return false;
		return true;
	}

	/// <summary>Returns the programmatic name of a pattern.</summary>
	public static string PatternName(AutomationPattern pattern) =>
		(pattern ?? throw new ArgumentNullException(nameof(pattern))).ProgrammaticName;

	/// <summary>Returns the programmatic name of a property.</summary>
	public static string PropertyName(AutomationProperty property) =>
		(property ?? throw new ArgumentNullException(nameof(property))).ProgrammaticName;

	// The peer event each client event id comes from.
	internal static readonly (AutomationEvents Source, AutomationEvent Client)[] EventMap =
	{
		(AutomationEvents.ToolTipOpened, AutomationElementIdentifiers.ToolTipOpenedEvent),
		(AutomationEvents.ToolTipClosed, AutomationElementIdentifiers.ToolTipClosedEvent),
		(AutomationEvents.MenuOpened, AutomationElementIdentifiers.MenuOpenedEvent),
		(AutomationEvents.MenuClosed, AutomationElementIdentifiers.MenuClosedEvent),
		(AutomationEvents.AutomationFocusChanged, AutomationElementIdentifiers.AutomationFocusChangedEvent),
		(AutomationEvents.InvokePatternOnInvoked, InvokePatternIdentifiers.InvokedEvent),
		(AutomationEvents.SelectionItemPatternOnElementAddedToSelection,
			SelectionItemPatternIdentifiers.ElementAddedToSelectionEvent),
		(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection,
			SelectionItemPatternIdentifiers.ElementRemovedFromSelectionEvent),
		(AutomationEvents.SelectionItemPatternOnElementSelected, SelectionItemPatternIdentifiers.ElementSelectedEvent),
		(AutomationEvents.SelectionPatternOnInvalidated, SelectionPatternIdentifiers.InvalidatedEvent),
		(AutomationEvents.TextPatternOnTextSelectionChanged, TextPatternIdentifiers.TextSelectionChangedEvent),
		(AutomationEvents.TextPatternOnTextChanged, TextPatternIdentifiers.TextChangedEvent),
		(AutomationEvents.AsyncContentLoaded, AutomationElementIdentifiers.AsyncContentLoadedEvent),
		(AutomationEvents.PropertyChanged, AutomationElementIdentifiers.AutomationPropertyChangedEvent),
		(AutomationEvents.StructureChanged, AutomationElementIdentifiers.StructureChangedEvent),
		(AutomationEvents.LiveRegionChanged, AutomationElementIdentifiers.LiveRegionChangedEvent),
	};

	internal static AutomationEvents? SourceOf(AutomationEvent clientEvent)
	{
		foreach (var entry in EventMap)
			if (entry.Client.Id == clientEvent.Id)
				return entry.Source;
		return null;
	}

	internal static AutomationEvent? ClientOf(AutomationEvents source)
	{
		foreach (var entry in EventMap)
			if (entry.Source == source)
				return entry.Client;
		return null;
	}
}
