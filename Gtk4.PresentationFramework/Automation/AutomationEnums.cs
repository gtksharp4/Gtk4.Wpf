namespace System.Windows.Automation;

/// <summary>The check state of a toggle control.</summary>
public enum ToggleState
{
	Off,
	On,
	Indeterminate,
}

/// <summary>The expand/collapse state of a control.</summary>
public enum ExpandCollapseState
{
	Collapsed,
	Expanded,
	PartiallyExpanded,
	LeafNode,
}

/// <summary>The amount to scroll by.</summary>
public enum ScrollAmount
{
	LargeDecrement,
	SmallDecrement,
	NoAmount,
	LargeIncrement,
	SmallIncrement,
}

/// <summary>Whether a table is read by row or by column first.</summary>
public enum RowOrColumnMajor
{
	RowMajor,
	ColumnMajor,
	Indeterminate,
}

/// <summary>The visual state of a window.</summary>
public enum WindowVisualState
{
	Normal,
	Maximized,
	Minimized,
}

/// <summary>The interaction state of a window.</summary>
public enum WindowInteractionState
{
	Running,
	Closing,
	ReadyForUserInteraction,
	BlockedByModalWindow,
	NotResponding,
}

/// <summary>The orientation of a control.</summary>
public enum OrientationType
{
	None,
	Horizontal,
	Vertical,
}

/// <summary>How an automation element's structure changed.</summary>
public enum StructureChangeType
{
	ChildAdded,
	ChildRemoved,
	ChildrenInvalidated,
	ChildrenBulkAdded,
	ChildrenBulkRemoved,
	ChildrenReordered,
}

/// <summary>The edge or area an element docks to.</summary>
public enum DockPosition
{
	Top,
	Left,
	Bottom,
	Right,
	Fill,
	None,
}

/// <summary>The kind of text selection a control supports.</summary>
public enum SupportedTextSelection
{
	None,
	Single,
	Multiple,
}
