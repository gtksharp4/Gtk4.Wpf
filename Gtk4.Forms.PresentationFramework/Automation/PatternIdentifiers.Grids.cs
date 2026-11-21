namespace System.Windows.Automation;

/// <summary>Identifiers for the Grid pattern.</summary>
public static class GridPatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10006, "GridPattern");
	public static readonly AutomationProperty RowCountProperty = Ids.Property(30062, "GridPattern", "RowCount");
	public static readonly AutomationProperty ColumnCountProperty = Ids.Property(30063, "GridPattern", "ColumnCount");
}

/// <summary>Identifiers for the GridItem pattern.</summary>
public static class GridItemPatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10007, "GridItemPattern");
	public static readonly AutomationProperty RowProperty = Ids.Property(30064, "GridItemPattern", "Row");
	public static readonly AutomationProperty ColumnProperty = Ids.Property(30065, "GridItemPattern", "Column");
	public static readonly AutomationProperty RowSpanProperty = Ids.Property(30066, "GridItemPattern", "RowSpan");
	public static readonly AutomationProperty ColumnSpanProperty =
		Ids.Property(30067, "GridItemPattern", "ColumnSpan");
	public static readonly AutomationProperty ContainingGridProperty =
		Ids.Property(30068, "GridItemPattern", "ContainingGrid");
}

/// <summary>Identifiers for the Table pattern.</summary>
public static class TablePatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10012, "TablePattern");
	public static readonly AutomationProperty RowHeadersProperty = Ids.Property(30081, "TablePattern", "RowHeaders");
	public static readonly AutomationProperty ColumnHeadersProperty =
		Ids.Property(30082, "TablePattern", "ColumnHeaders");
	public static readonly AutomationProperty RowOrColumnMajorProperty =
		Ids.Property(30083, "TablePattern", "RowOrColumnMajor");
}

/// <summary>Identifiers for the TableItem pattern.</summary>
public static class TableItemPatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10013, "TableItemPattern");
	public static readonly AutomationProperty RowHeaderItemsProperty =
		Ids.Property(30084, "TableItemPattern", "RowHeaderItems");
	public static readonly AutomationProperty ColumnHeaderItemsProperty =
		Ids.Property(30085, "TableItemPattern", "ColumnHeaderItems");
}

/// <summary>Identifiers for the Text pattern.</summary>
public static class TextPatternIdentifiers
{
	public static readonly AutomationPattern Pattern = Ids.Pattern(10014, "TextPattern");
	public static readonly AutomationEvent TextSelectionChangedEvent =
		Ids.Event(20014, "TextPattern", "TextSelectionChanged");
	public static readonly AutomationEvent TextChangedEvent = Ids.Event(20015, "TextPattern", "TextChanged");
}
