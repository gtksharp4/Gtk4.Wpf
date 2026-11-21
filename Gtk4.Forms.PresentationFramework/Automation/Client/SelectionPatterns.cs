using System.Windows.Automation.Provider;

namespace System.Windows.Automation;

/// <summary>Reads the selection of a container, such as a list box.</summary>
public class SelectionPattern : BasePattern
{
	public static readonly AutomationPattern Pattern = SelectionPatternIdentifiers.Pattern;
	public static readonly AutomationProperty SelectionProperty = SelectionPatternIdentifiers.SelectionProperty;
	public static readonly AutomationProperty CanSelectMultipleProperty =
		SelectionPatternIdentifiers.CanSelectMultipleProperty;
	public static readonly AutomationProperty IsSelectionRequiredProperty =
		SelectionPatternIdentifiers.IsSelectionRequiredProperty;
	public static readonly AutomationEvent InvalidatedEvent = SelectionPatternIdentifiers.InvalidatedEvent;

	readonly ISelectionProvider _provider;
	readonly SelectionPatternInformation _cached;
	readonly bool _hasCache;

	internal SelectionPattern(AutomationElement element, ISelectionProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	SelectionPatternInformation Read() => new SelectionPatternInformation(
		AutomationElement.FromProviders(_provider.GetSelection()), _provider.CanSelectMultiple,
		_provider.IsSelectionRequired);

	public SelectionPatternInformation Current => Run(Read);

	public SelectionPatternInformation Cached => _hasCache ? _cached : throw NotCached();

	public struct SelectionPatternInformation
	{
		readonly AutomationElement[] _selection;

		internal SelectionPatternInformation(AutomationElement[] selection, bool canSelectMultiple, bool required)
		{
			_selection = selection;
			CanSelectMultiple = canSelectMultiple;
			IsSelectionRequired = required;
		}

		public bool CanSelectMultiple { get; }

		public bool IsSelectionRequired { get; }

		public AutomationElement[] GetSelection() => (AutomationElement[])_selection.Clone();
	}
}

/// <summary>Selects an item of a container, such as a list box item.</summary>
public class SelectionItemPattern : BasePattern
{
	public static readonly AutomationPattern Pattern = SelectionItemPatternIdentifiers.Pattern;
	public static readonly AutomationProperty IsSelectedProperty = SelectionItemPatternIdentifiers.IsSelectedProperty;
	public static readonly AutomationProperty SelectionContainerProperty =
		SelectionItemPatternIdentifiers.SelectionContainerProperty;
	public static readonly AutomationEvent ElementAddedToSelectionEvent =
		SelectionItemPatternIdentifiers.ElementAddedToSelectionEvent;
	public static readonly AutomationEvent ElementRemovedFromSelectionEvent =
		SelectionItemPatternIdentifiers.ElementRemovedFromSelectionEvent;
	public static readonly AutomationEvent ElementSelectedEvent = SelectionItemPatternIdentifiers.ElementSelectedEvent;

	readonly ISelectionItemProvider _provider;
	readonly SelectionItemPatternInformation _cached;
	readonly bool _hasCache;

	internal SelectionItemPattern(AutomationElement element, ISelectionItemProvider provider, bool cached)
		: base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	SelectionItemPatternInformation Read() => new SelectionItemPatternInformation(
		_provider.IsSelected, AutomationElement.FromProvider(_provider.SelectionContainer));

	public SelectionItemPatternInformation Current => Run(Read);

	public SelectionItemPatternInformation Cached => _hasCache ? _cached : throw NotCached();

	/// <exception cref="ElementNotEnabledException">The element is disabled.</exception>
	public void Select() => Run(_provider.Select);

	/// <exception cref="ElementNotEnabledException">The element is disabled.</exception>
	public void AddToSelection() => Run(_provider.AddToSelection);

	/// <exception cref="ElementNotEnabledException">The element is disabled.</exception>
	public void RemoveFromSelection() => Run(_provider.RemoveFromSelection);

	public struct SelectionItemPatternInformation
	{
		internal SelectionItemPatternInformation(bool isSelected, AutomationElement? container)
		{
			IsSelected = isSelected;
			SelectionContainer = container!;
		}

		public bool IsSelected { get; }

		public AutomationElement SelectionContainer { get; }
	}
}
