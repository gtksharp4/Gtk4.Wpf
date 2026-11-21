namespace System.Windows.Automation.Peers;

/// <summary>The control type a peer reports.</summary>
public enum AutomationControlType
{
	Button, Calendar, CheckBox, ComboBox, Edit, Hyperlink, Image, ListItem, List, Menu, MenuBar, MenuItem,
	ProgressBar, RadioButton, ScrollBar, Slider, Spinner, StatusBar, Tab, TabItem, Text, ToolBar, ToolTip,
	Tree, TreeItem, Custom, Group, Thumb, DataGrid, DataItem, Document, SplitButton, Window, Pane, Header,
	HeaderItem, Table, TitleBar, Separator,
}

/// <summary>The patterns a peer can provide through <see cref="AutomationPeer.GetPattern"/>.</summary>
public enum PatternInterface
{
	Invoke, Selection, Value, RangeValue, Scroll, ScrollItem, ExpandCollapse, Grid, GridItem, MultipleView,
	Window, SelectionItem, Dock, Table, TableItem, Toggle, Transform, Text, ItemContainer, VirtualizedItem,
	SynchronizedInput,
}

/// <summary>The orientation a peer reports.</summary>
public enum AutomationOrientation
{
	None,
	Horizontal,
	Vertical,
}

/// <summary>The events a peer can raise.</summary>
public enum AutomationEvents
{
	ToolTipOpened,
	ToolTipClosed,
	MenuOpened,
	MenuClosed,
	AutomationFocusChanged,
	InvokePatternOnInvoked,
	SelectionItemPatternOnElementAddedToSelection,
	SelectionItemPatternOnElementRemovedFromSelection,
	SelectionItemPatternOnElementSelected,
	SelectionPatternOnInvalidated,
	TextPatternOnTextSelectionChanged,
	TextPatternOnTextChanged,
	AsyncContentLoaded,
	PropertyChanged,
	StructureChanged,
	InputReachedTarget,
	InputReachedOtherElement,
	InputDiscarded,
	LiveRegionChanged,
}

/// <summary>The kind of a notification raised by <see cref="AutomationPeer.RaiseNotificationEvent"/>.</summary>
public enum AutomationNotificationKind
{
	ItemAdded,
	ItemRemoved,
	ActionCompleted,
	ActionAborted,
	Other,
}

/// <summary>How a notification is processed by a client.</summary>
public enum AutomationNotificationProcessing
{
	ImportantAll,
	ImportantMostRecent,
	All,
	MostRecent,
	CurrentThenMostRecent,
}
