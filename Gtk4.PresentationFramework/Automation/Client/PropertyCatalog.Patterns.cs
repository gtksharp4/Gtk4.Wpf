using System.Collections.Generic;
using System.Windows.Automation.Provider;

namespace System.Windows.Automation;

/// <summary>A pattern property, read from the provider of its pattern.</summary>
sealed class PatternPropertyEntry
{
	internal PatternPropertyEntry(AutomationProperty property, AutomationPattern pattern, Type type,
		Func<object, object?> get, Func<object?>? defaultValue = null)
	{
		Default = defaultValue;
		Property = property;
		Pattern = pattern;
		Type = type;
		Get = get;
	}

	internal AutomationProperty Property { get; }

	internal AutomationPattern Pattern { get; }

	internal Type Type { get; }

	internal Func<object, object?> Get { get; }

	/// <summary>The WPF schema default for an unsupported pattern; <c>null</c> means the type default.</summary>
	internal Func<object?>? Default { get; }
}

static partial class PropertyCatalog
{
	static readonly Dictionary<int, PatternPropertyEntry> _pattern = BuildPatternEntries();

	internal static PatternPropertyEntry? LookupPattern(AutomationProperty property)
	{
		lock (_pattern) return _pattern.TryGetValue(property.Id, out var entry) ? entry : null;
	}

	internal static IEnumerable<PatternPropertyEntry> PatternEntries => _pattern.Values;

	static Dictionary<int, PatternPropertyEntry> BuildPatternEntries()
	{
		var map = new Dictionary<int, PatternPropertyEntry>();
		void Add<TProvider>(AutomationProperty p, AutomationPattern pattern, Type t, Func<TProvider, object?> get,
			Func<object?>? defaultValue = null) =>
			map[p.Id] = new PatternPropertyEntry(p, pattern, t, o => get((TProvider)o), defaultValue);

		var value = ValuePatternIdentifiers.Pattern;
		Add<IValueProvider>(ValuePatternIdentifiers.ValueProperty, value, typeof(string), x => x.Value);
		Add<IValueProvider>(ValuePatternIdentifiers.IsReadOnlyProperty, value, typeof(bool), x => x.IsReadOnly,
			() => true);

		var toggle = TogglePatternIdentifiers.Pattern;
		Add<IToggleProvider>(TogglePatternIdentifiers.ToggleStateProperty, toggle, typeof(ToggleState),
			x => x.ToggleState, () => ToggleState.Indeterminate);

		var expand = ExpandCollapsePatternIdentifiers.Pattern;
		Add<IExpandCollapseProvider>(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, expand,
			typeof(ExpandCollapseState), x => x.ExpandCollapseState,
			() => ExpandCollapseState.LeafNode);

		var range = RangeValuePatternIdentifiers.Pattern;
		Add<IRangeValueProvider>(RangeValuePatternIdentifiers.ValueProperty, range, typeof(double), x => x.Value);
		Add<IRangeValueProvider>(RangeValuePatternIdentifiers.IsReadOnlyProperty, range, typeof(bool),
			x => x.IsReadOnly, () => true);
		Add<IRangeValueProvider>(RangeValuePatternIdentifiers.MinimumProperty, range, typeof(double),
			x => x.Minimum);
		Add<IRangeValueProvider>(RangeValuePatternIdentifiers.MaximumProperty, range, typeof(double),
			x => x.Maximum);
		Add<IRangeValueProvider>(RangeValuePatternIdentifiers.LargeChangeProperty, range, typeof(double),
			x => x.LargeChange);
		Add<IRangeValueProvider>(RangeValuePatternIdentifiers.SmallChangeProperty, range, typeof(double),
			x => x.SmallChange);

		var scroll = ScrollPatternIdentifiers.Pattern;
		Add<IScrollProvider>(ScrollPatternIdentifiers.HorizontalScrollPercentProperty, scroll, typeof(double),
			x => x.HorizontalScrollPercent);
		Add<IScrollProvider>(ScrollPatternIdentifiers.VerticalScrollPercentProperty, scroll, typeof(double),
			x => x.VerticalScrollPercent);
		Add<IScrollProvider>(ScrollPatternIdentifiers.HorizontalViewSizeProperty, scroll, typeof(double),
			x => x.HorizontalViewSize);
		Add<IScrollProvider>(ScrollPatternIdentifiers.VerticalViewSizeProperty, scroll, typeof(double),
			x => x.VerticalViewSize);
		Add<IScrollProvider>(ScrollPatternIdentifiers.HorizontallyScrollableProperty, scroll, typeof(bool),
			x => x.HorizontallyScrollable);
		Add<IScrollProvider>(ScrollPatternIdentifiers.VerticallyScrollableProperty, scroll, typeof(bool),
			x => x.VerticallyScrollable);

		var window = WindowPatternIdentifiers.Pattern;
		Add<IWindowProvider>(WindowPatternIdentifiers.CanMaximizeProperty, window, typeof(bool),
			x => x.Maximizable);
		Add<IWindowProvider>(WindowPatternIdentifiers.CanMinimizeProperty, window, typeof(bool),
			x => x.Minimizable);
		Add<IWindowProvider>(WindowPatternIdentifiers.IsModalProperty, window, typeof(bool), x => x.IsModal);
		Add<IWindowProvider>(WindowPatternIdentifiers.IsTopmostProperty, window, typeof(bool), x => x.IsTopmost);
		Add<IWindowProvider>(WindowPatternIdentifiers.WindowVisualStateProperty, window,
			typeof(WindowVisualState), x => x.VisualState);
		Add<IWindowProvider>(WindowPatternIdentifiers.WindowInteractionStateProperty, window,
			typeof(WindowInteractionState), x => x.InteractionState);

		var selection = SelectionPatternIdentifiers.Pattern;
		Add<ISelectionProvider>(SelectionPatternIdentifiers.CanSelectMultipleProperty, selection, typeof(bool),
			x => x.CanSelectMultiple);
		Add<ISelectionProvider>(SelectionPatternIdentifiers.IsSelectionRequiredProperty, selection, typeof(bool),
			x => x.IsSelectionRequired);
		Add<ISelectionProvider>(SelectionPatternIdentifiers.SelectionProperty, selection,
			typeof(AutomationElement[]), x => AutomationElement.FromProviders(x.GetSelection()),
			() => new AutomationElement[0]);

		var item = SelectionItemPatternIdentifiers.Pattern;
		Add<ISelectionItemProvider>(SelectionItemPatternIdentifiers.IsSelectedProperty, item, typeof(bool),
			x => x.IsSelected);
		Add<ISelectionItemProvider>(SelectionItemPatternIdentifiers.SelectionContainerProperty, item,
			typeof(AutomationElement), x => AutomationElement.FromProvider(x.SelectionContainer));
		return map;
	}
}
