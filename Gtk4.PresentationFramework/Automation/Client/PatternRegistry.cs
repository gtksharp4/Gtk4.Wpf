using System.Collections.Generic;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace System.Windows.Automation;

/// <summary>Maps a pattern id to the provider interface a peer exposes and the client class that wraps it.</summary>
static class PatternRegistry
{
	sealed class Entry
	{
		internal Entry(AutomationPattern pattern, Type provider,
			Func<AutomationElement, object, bool, BasePattern> create)
		{
			Pattern = pattern;
			Provider = provider;
			Create = create;
		}

		internal AutomationPattern Pattern { get; }

		internal Type Provider { get; }

		internal Func<AutomationElement, object, bool, BasePattern> Create { get; }
	}

	static readonly Dictionary<int, Entry> _entries = Build();

	internal static IEnumerable<AutomationPattern> All
	{
		get
		{
			foreach (var entry in _entries.Values)
				yield return entry.Pattern;
		}
	}

	internal static PatternInterface InterfaceOf(AutomationPattern pattern) =>
		PatternMap.ToInterface(pattern.Id)
		?? throw new ArgumentException("The pattern is not supported.", nameof(pattern));

	/// <summary>Wraps the provider of <paramref name="pattern"/>, or <c>null</c> when the element has none.</summary>
	internal static BasePattern? Create(AutomationPattern pattern, AutomationElement element, bool cached)
	{
		if (!_entries.TryGetValue(pattern.Id, out var entry)) return null;
		var provider = element.Peer.GetPattern(InterfaceOf(pattern));
		return provider != null && entry.Provider.IsInstanceOfType(provider)
			? entry.Create(element, provider, cached) : null;
	}

	static Dictionary<int, Entry> Build()
	{
		var map = new Dictionary<int, Entry>();
		void Add<TProvider>(AutomationPattern pattern, Func<AutomationElement, TProvider, bool, BasePattern> create) =>
			map[pattern.Id] = new Entry(pattern, typeof(TProvider), (e, p, c) => create(e, (TProvider)p, c));
		Add<IInvokeProvider>(InvokePattern.Pattern, (e, p, c) => new InvokePattern(e, p, c));
		Add<IValueProvider>(ValuePattern.Pattern, (e, p, c) => new ValuePattern(e, p, c));
		Add<IToggleProvider>(TogglePattern.Pattern, (e, p, c) => new TogglePattern(e, p, c));
		Add<ISelectionProvider>(SelectionPattern.Pattern, (e, p, c) => new SelectionPattern(e, p, c));
		Add<ISelectionItemProvider>(SelectionItemPattern.Pattern, (e, p, c) => new SelectionItemPattern(e, p, c));
		Add<IExpandCollapseProvider>(ExpandCollapsePattern.Pattern, (e, p, c) => new ExpandCollapsePattern(e, p, c));
		Add<IRangeValueProvider>(RangeValuePattern.Pattern, (e, p, c) => new RangeValuePattern(e, p, c));
		Add<IScrollProvider>(ScrollPattern.Pattern, (e, p, c) => new ScrollPattern(e, p, c));
		Add<IWindowProvider>(WindowPattern.Pattern, (e, p, c) => new WindowPattern(e, p, c));
		Add<IGridProvider>(GridPattern.Pattern, (e, p, c) => new GridPattern(e, p, c));
		Add<IGridItemProvider>(GridItemPattern.Pattern, (e, p, c) => new GridItemPattern(e, p, c));
		Add<ITableProvider>(TablePattern.Pattern, (e, p, c) => new TablePattern(e, p, c));
		Add<ITableItemProvider>(TableItemPattern.Pattern, (e, p, c) => new TableItemPattern(e, p, c));
		Add<ITextProvider>(TextPattern.Pattern, (e, p, c) => new TextPattern(e, p, c));
		return map;
	}
}
