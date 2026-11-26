using System.Windows.Automation.Provider;

namespace System.Windows.Automation;

/// <summary>Reads a grid of rows and columns, such as a data grid.</summary>
public class GridPattern : BasePattern
{
	public static readonly AutomationPattern Pattern = GridPatternIdentifiers.Pattern;
	public static readonly AutomationProperty RowCountProperty = GridPatternIdentifiers.RowCountProperty;
	public static readonly AutomationProperty ColumnCountProperty = GridPatternIdentifiers.ColumnCountProperty;

	readonly IGridProvider _provider;
	readonly GridPatternInformation _cached;
	readonly bool _hasCache;

	internal GridPattern(AutomationElement element, IGridProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	GridPatternInformation Read() => new GridPatternInformation(_provider.RowCount, _provider.ColumnCount);

	public GridPatternInformation Current => Run(Read);

	public GridPatternInformation Cached => _hasCache ? _cached : throw NotCached();

	/// <exception cref="ArgumentOutOfRangeException">The row or column is outside the grid.</exception>
	public AutomationElement GetItem(int row, int column) =>
		Run(() => AutomationElement.FromProvider(_provider.GetItem(row, column), CacheRequest.ActiveOrNull)!);

	public struct GridPatternInformation
	{
		internal GridPatternInformation(int rows, int columns)
		{
			RowCount = rows;
			ColumnCount = columns;
		}

		public int RowCount { get; }

		public int ColumnCount { get; }
	}
}

/// <summary>Reads the position of an item in a grid.</summary>
public class GridItemPattern : BasePattern
{
	public static readonly AutomationPattern Pattern = GridItemPatternIdentifiers.Pattern;
	public static readonly AutomationProperty RowProperty = GridItemPatternIdentifiers.RowProperty;
	public static readonly AutomationProperty ColumnProperty = GridItemPatternIdentifiers.ColumnProperty;
	public static readonly AutomationProperty RowSpanProperty = GridItemPatternIdentifiers.RowSpanProperty;
	public static readonly AutomationProperty ColumnSpanProperty = GridItemPatternIdentifiers.ColumnSpanProperty;
	public static readonly AutomationProperty ContainingGridProperty =
		GridItemPatternIdentifiers.ContainingGridProperty;

	readonly IGridItemProvider _provider;
	readonly GridItemPatternInformation _cached;
	readonly bool _hasCache;

	internal GridItemPattern(AutomationElement element, IGridItemProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	GridItemPatternInformation Read() => new GridItemPatternInformation(_provider.Row, _provider.Column,
		_provider.RowSpan, _provider.ColumnSpan, AutomationElement.FromProvider(_provider.ContainingGrid));

	public GridItemPatternInformation Current => Run(Read);

	public GridItemPatternInformation Cached => _hasCache ? _cached : throw NotCached();

	public struct GridItemPatternInformation
	{
		internal GridItemPatternInformation(int row, int column, int rowSpan, int columnSpan, AutomationElement? grid)
		{
			Row = row;
			Column = column;
			RowSpan = rowSpan;
			ColumnSpan = columnSpan;
			ContainingGrid = grid!;
		}

		public int Row { get; }

		public int Column { get; }

		public int RowSpan { get; }

		public int ColumnSpan { get; }

		public AutomationElement ContainingGrid { get; }
	}
}

/// <summary>Reads the headers of a table.</summary>
public class TablePattern : BasePattern
{
	public static readonly AutomationPattern Pattern = TablePatternIdentifiers.Pattern;
	public static readonly AutomationProperty RowHeadersProperty = TablePatternIdentifiers.RowHeadersProperty;
	public static readonly AutomationProperty ColumnHeadersProperty = TablePatternIdentifiers.ColumnHeadersProperty;
	public static readonly AutomationProperty RowOrColumnMajorProperty =
		TablePatternIdentifiers.RowOrColumnMajorProperty;

	readonly ITableProvider _provider;
	readonly TablePatternInformation _cached;
	readonly bool _hasCache;

	internal TablePattern(AutomationElement element, ITableProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	TablePatternInformation Read() => new TablePatternInformation(
		AutomationElement.FromProviders(_provider.GetRowHeaders()),
		AutomationElement.FromProviders(_provider.GetColumnHeaders()), _provider.RowOrColumnMajor);

	public TablePatternInformation Current => Run(Read);

	public TablePatternInformation Cached => _hasCache ? _cached : throw NotCached();

	public struct TablePatternInformation
	{
		readonly AutomationElement[] _rows;
		readonly AutomationElement[] _columns;

		internal TablePatternInformation(AutomationElement[] rows, AutomationElement[] columns, RowOrColumnMajor major)
		{
			_rows = rows;
			_columns = columns;
			RowOrColumnMajor = major;
		}

		public RowOrColumnMajor RowOrColumnMajor { get; }

		public AutomationElement[] GetRowHeaders() => (AutomationElement[])_rows.Clone();

		public AutomationElement[] GetColumnHeaders() => (AutomationElement[])_columns.Clone();
	}
}

/// <summary>Reads the headers of a table item.</summary>
public class TableItemPattern : BasePattern
{
	public static readonly AutomationPattern Pattern = TableItemPatternIdentifiers.Pattern;
	public static readonly AutomationProperty RowHeaderItemsProperty =
		TableItemPatternIdentifiers.RowHeaderItemsProperty;
	public static readonly AutomationProperty ColumnHeaderItemsProperty =
		TableItemPatternIdentifiers.ColumnHeaderItemsProperty;

	readonly ITableItemProvider _provider;
	readonly TableItemPatternInformation _cached;
	readonly bool _hasCache;

	internal TableItemPattern(AutomationElement element, ITableItemProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	TableItemPatternInformation Read() => new TableItemPatternInformation(
		AutomationElement.FromProviders(_provider.GetRowHeaderItems()),
		AutomationElement.FromProviders(_provider.GetColumnHeaderItems()));

	public TableItemPatternInformation Current => Run(Read);

	public TableItemPatternInformation Cached => _hasCache ? _cached : throw NotCached();

	public struct TableItemPatternInformation
	{
		readonly AutomationElement[] _rows;
		readonly AutomationElement[] _columns;

		internal TableItemPatternInformation(AutomationElement[] rows, AutomationElement[] columns)
		{
			_rows = rows;
			_columns = columns;
		}

		public AutomationElement[] GetRowHeaderItems() => (AutomationElement[])_rows.Clone();

		public AutomationElement[] GetColumnHeaderItems() => (AutomationElement[])_columns.Clone();
	}
}
