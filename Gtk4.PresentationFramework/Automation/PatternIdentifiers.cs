namespace System.Windows.Automation;

static class Ids
{
	internal static void EnsureAll()
	{
		foreach (var t in new[]
		{
			typeof(AutomationElementIdentifiers), typeof(InvokePatternIdentifiers),
			typeof(SelectionPatternIdentifiers), typeof(ValuePatternIdentifiers),
			typeof(RangeValuePatternIdentifiers), typeof(ScrollPatternIdentifiers),
			typeof(ExpandCollapsePatternIdentifiers), typeof(WindowPatternIdentifiers),
			typeof(SelectionItemPatternIdentifiers), typeof(TogglePatternIdentifiers),
			typeof(ScrollItemPatternIdentifiers), typeof(GridPatternIdentifiers),
			typeof(GridItemPatternIdentifiers), typeof(TablePatternIdentifiers), typeof(TableItemPatternIdentifiers),
			typeof(TextPatternIdentifiers),
		})
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
	}

	internal static AutomationPattern Pattern(int id, string name) =>
		AutomationPattern.Register(id, name + "Identifiers.Pattern");

	internal static AutomationProperty Property(int id, string owner, string name) =>
		AutomationProperty.Register(id, owner + "Identifiers." + name + "Property");

	internal static AutomationEvent Event(int id, string owner, string name) =>
		AutomationEvent.Register(id, owner + "Identifiers." + name + "Event");
}

/// <summary>Identifiers for the Invoke pattern.</summary>
public static class InvokePatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10000, "InvokePattern");
	public static readonly AutomationEvent InvokedEvent = Ids.Event(20009, "InvokePattern", "Invoked");
}

/// <summary>Identifiers for the Selection pattern.</summary>
public static class SelectionPatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10001, "SelectionPattern");
	public static readonly AutomationProperty SelectionProperty = Ids.Property(30059, "SelectionPattern", "Selection");
	public static readonly AutomationProperty CanSelectMultipleProperty =
		Ids.Property(30060, "SelectionPattern", "CanSelectMultiple");
	public static readonly AutomationProperty IsSelectionRequiredProperty =
		Ids.Property(30061, "SelectionPattern", "IsSelectionRequired");
	public static readonly AutomationEvent InvalidatedEvent = Ids.Event(20013, "SelectionPattern", "Invalidated");
}

/// <summary>Identifiers for the Value pattern.</summary>
public static class ValuePatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10002, "ValuePattern");
	public static readonly AutomationProperty ValueProperty = Ids.Property(30045, "ValuePattern", "Value");
	public static readonly AutomationProperty IsReadOnlyProperty = Ids.Property(30046, "ValuePattern", "IsReadOnly");
}

/// <summary>Identifiers for the RangeValue pattern.</summary>
public static class RangeValuePatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10003, "RangeValuePattern");
	public static readonly AutomationProperty ValueProperty = Ids.Property(30047, "RangeValuePattern", "Value");
	public static readonly AutomationProperty IsReadOnlyProperty =
		Ids.Property(30048, "RangeValuePattern", "IsReadOnly");
	public static readonly AutomationProperty MinimumProperty = Ids.Property(30049, "RangeValuePattern", "Minimum");
	public static readonly AutomationProperty MaximumProperty = Ids.Property(30050, "RangeValuePattern", "Maximum");
	public static readonly AutomationProperty LargeChangeProperty =
		Ids.Property(30051, "RangeValuePattern", "LargeChange");
	public static readonly AutomationProperty SmallChangeProperty =
		Ids.Property(30052, "RangeValuePattern", "SmallChange");
}

/// <summary>Identifiers for the Scroll pattern.</summary>
public static class ScrollPatternIdentifiers
{
	/// <summary>The value reported when a direction cannot scroll.</summary>
	public const double NoScroll = -1;

	public static readonly AutomationPattern Pattern = Ids.Pattern(10004, "ScrollPattern");
	public static readonly AutomationProperty HorizontalScrollPercentProperty =
		Ids.Property(30053, "ScrollPattern", "HorizontalScrollPercent");
	public static readonly AutomationProperty HorizontalViewSizeProperty =
		Ids.Property(30054, "ScrollPattern", "HorizontalViewSize");
	public static readonly AutomationProperty VerticalScrollPercentProperty =
		Ids.Property(30055, "ScrollPattern", "VerticalScrollPercent");
	public static readonly AutomationProperty VerticalViewSizeProperty =
		Ids.Property(30056, "ScrollPattern", "VerticalViewSize");
	public static readonly AutomationProperty HorizontallyScrollableProperty =
		Ids.Property(30057, "ScrollPattern", "HorizontallyScrollable");
	public static readonly AutomationProperty VerticallyScrollableProperty =
		Ids.Property(30058, "ScrollPattern", "VerticallyScrollable");
}

/// <summary>Identifiers for the ExpandCollapse pattern.</summary>
public static class ExpandCollapsePatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10005, "ExpandCollapsePattern");
	public static readonly AutomationProperty ExpandCollapseStateProperty =
		Ids.Property(30070, "ExpandCollapsePattern", "ExpandCollapseState");
}

/// <summary>Identifiers for the Window pattern.</summary>
public static class WindowPatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10009, "WindowPattern");
	public static readonly AutomationProperty CanMaximizeProperty =
		Ids.Property(30073, "WindowPattern", "CanMaximize");
	public static readonly AutomationProperty CanMinimizeProperty =
		Ids.Property(30074, "WindowPattern", "CanMinimize");
	public static readonly AutomationProperty WindowVisualStateProperty =
		Ids.Property(30075, "WindowPattern", "WindowVisualState");
	public static readonly AutomationProperty WindowInteractionStateProperty =
		Ids.Property(30076, "WindowPattern", "WindowInteractionState");
	public static readonly AutomationProperty IsModalProperty = Ids.Property(30077, "WindowPattern", "IsModal");
	public static readonly AutomationProperty IsTopmostProperty = Ids.Property(30078, "WindowPattern", "IsTopmost");
	public static readonly AutomationEvent WindowOpenedEvent = Ids.Event(20016, "WindowPattern", "WindowOpened");
	public static readonly AutomationEvent WindowClosedEvent = Ids.Event(20017, "WindowPattern", "WindowClosed");
}

/// <summary>Identifiers for the SelectionItem pattern.</summary>
public static class SelectionItemPatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10010, "SelectionItemPattern");
	public static readonly AutomationProperty IsSelectedProperty =
		Ids.Property(30079, "SelectionItemPattern", "IsSelected");
	public static readonly AutomationProperty SelectionContainerProperty =
		Ids.Property(30080, "SelectionItemPattern", "SelectionContainer");
	public static readonly AutomationEvent ElementAddedToSelectionEvent =
		Ids.Event(20010, "SelectionItemPattern", "ElementAddedToSelection");
	public static readonly AutomationEvent ElementRemovedFromSelectionEvent =
		Ids.Event(20011, "SelectionItemPattern", "ElementRemovedFromSelection");
	public static readonly AutomationEvent ElementSelectedEvent =
		Ids.Event(20012, "SelectionItemPattern", "ElementSelected");
}

/// <summary>Identifiers for the Toggle pattern.</summary>
public static class TogglePatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10015, "TogglePattern");
	public static readonly AutomationProperty ToggleStateProperty =
		Ids.Property(30086, "TogglePattern", "ToggleState");
}

/// <summary>Identifiers for the ScrollItem pattern.</summary>
public static class ScrollItemPatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10017, "ScrollItemPattern");
}
