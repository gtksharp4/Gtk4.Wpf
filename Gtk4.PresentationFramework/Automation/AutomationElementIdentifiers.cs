namespace System.Windows.Automation;

/// <summary>Identifiers for the properties and events common to all automation elements.</summary>
public static class AutomationElementIdentifiers
{
	static AutomationProperty P(int id, string name) =>
		AutomationProperty.Register(id, "AutomationElementIdentifiers." + name + "Property");

	static AutomationEvent E(int id, string name) =>
		AutomationEvent.Register(id, "AutomationElementIdentifiers." + name + "Event");

	public static readonly AutomationProperty BoundingRectangleProperty = P(30001, "BoundingRectangle");
	public static readonly AutomationProperty ControlTypeProperty = P(30003, "ControlType");
	public static readonly AutomationProperty LocalizedControlTypeProperty = P(30004, "LocalizedControlType");
	public static readonly AutomationProperty NameProperty = P(30005, "Name");
	public static readonly AutomationProperty AcceleratorKeyProperty = P(30006, "AcceleratorKey");
	public static readonly AutomationProperty AccessKeyProperty = P(30007, "AccessKey");
	public static readonly AutomationProperty HasKeyboardFocusProperty = P(30008, "HasKeyboardFocus");
	public static readonly AutomationProperty IsKeyboardFocusableProperty = P(30009, "IsKeyboardFocusable");
	public static readonly AutomationProperty IsEnabledProperty = P(30010, "IsEnabled");
	public static readonly AutomationProperty AutomationIdProperty = P(30011, "AutomationId");
	public static readonly AutomationProperty ClassNameProperty = P(30012, "ClassName");
	public static readonly AutomationProperty HelpTextProperty = P(30013, "HelpText");
	public static readonly AutomationProperty ClickablePointProperty = P(30014, "ClickablePoint");
	public static readonly AutomationProperty CultureProperty = P(30015, "Culture");
	public static readonly AutomationProperty IsControlElementProperty = P(30016, "IsControlElement");
	public static readonly AutomationProperty IsContentElementProperty = P(30017, "IsContentElement");
	public static readonly AutomationProperty LabeledByProperty = P(30018, "LabeledBy");
	public static readonly AutomationProperty IsPasswordProperty = P(30019, "IsPassword");
	public static readonly AutomationProperty NativeWindowHandleProperty = P(30020, "NativeWindowHandle");
	public static readonly AutomationProperty ItemTypeProperty = P(30021, "ItemType");
	public static readonly AutomationProperty IsOffscreenProperty = P(30022, "IsOffscreen");
	public static readonly AutomationProperty OrientationProperty = P(30023, "Orientation");
	public static readonly AutomationProperty FrameworkIdProperty = P(30024, "FrameworkId");
	public static readonly AutomationProperty IsRequiredForFormProperty = P(30025, "IsRequiredForForm");
	public static readonly AutomationProperty ItemStatusProperty = P(30026, "ItemStatus");
	public static readonly AutomationProperty ProcessIdProperty = P(30002, "ProcessId");
	public static readonly AutomationProperty RuntimeIdProperty = P(30000, "RuntimeId");

	public static readonly AutomationEvent ToolTipOpenedEvent = E(20000, "ToolTipOpened");
	public static readonly AutomationEvent ToolTipClosedEvent = E(20001, "ToolTipClosed");
	public static readonly AutomationEvent MenuOpenedEvent = E(20003, "MenuOpened");
	public static readonly AutomationEvent MenuClosedEvent = E(20007, "MenuClosed");
	public static readonly AutomationEvent AutomationFocusChangedEvent = E(20005, "AutomationFocusChanged");
	public static readonly AutomationEvent AsyncContentLoadedEvent = E(20006, "AsyncContentLoaded");
	public static readonly AutomationEvent AutomationPropertyChangedEvent = E(20004, "AutomationPropertyChanged");
	public static readonly AutomationEvent StructureChangedEvent = E(20002, "StructureChanged");
	public static readonly AutomationEvent LayoutInvalidatedEvent = E(20008, "LayoutInvalidated");
	public static readonly AutomationEvent LiveRegionChangedEvent = E(20024, "LiveRegionChanged");
}
