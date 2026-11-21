using System.Windows.Automation.Provider;

namespace System.Windows.Automation;

/// <summary>Reads and sets a numeric value in a range, such as a slider.</summary>
public class RangeValuePattern : BasePattern
{
	public static readonly AutomationPattern Pattern = RangeValuePatternIdentifiers.Pattern;
	public static readonly AutomationProperty ValueProperty = RangeValuePatternIdentifiers.ValueProperty;
	public static readonly AutomationProperty IsReadOnlyProperty = RangeValuePatternIdentifiers.IsReadOnlyProperty;
	public static readonly AutomationProperty MinimumProperty = RangeValuePatternIdentifiers.MinimumProperty;
	public static readonly AutomationProperty MaximumProperty = RangeValuePatternIdentifiers.MaximumProperty;
	public static readonly AutomationProperty LargeChangeProperty = RangeValuePatternIdentifiers.LargeChangeProperty;
	public static readonly AutomationProperty SmallChangeProperty = RangeValuePatternIdentifiers.SmallChangeProperty;

	readonly IRangeValueProvider _provider;
	readonly RangeValuePatternInformation _cached;
	readonly bool _hasCache;

	internal RangeValuePattern(AutomationElement element, IRangeValueProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	RangeValuePatternInformation Read() => new RangeValuePatternInformation(_provider);

	public RangeValuePatternInformation Current => Run(Read);

	public RangeValuePatternInformation Cached => _hasCache ? _cached : throw NotCached();

	/// <exception cref="ElementNotEnabledException">The element is disabled.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The value is outside the range.</exception>
	public void SetValue(double value) => Run(() => _provider.SetValue(value));

	public struct RangeValuePatternInformation
	{
		internal RangeValuePatternInformation(IRangeValueProvider provider)
		{
			Value = provider.Value;
			IsReadOnly = provider.IsReadOnly;
			Minimum = provider.Minimum;
			Maximum = provider.Maximum;
			LargeChange = provider.LargeChange;
			SmallChange = provider.SmallChange;
		}

		public double Value { get; }

		public bool IsReadOnly { get; }

		public double Minimum { get; }

		public double Maximum { get; }

		public double LargeChange { get; }

		public double SmallChange { get; }
	}
}

/// <summary>Scrolls a control that has scrollable content.</summary>
public class ScrollPattern : BasePattern
{
	/// <summary>The percent that means a direction does not scroll.</summary>
	public const double NoScroll = ScrollPatternIdentifiers.NoScroll;

	public static readonly AutomationPattern Pattern = ScrollPatternIdentifiers.Pattern;
	public static readonly AutomationProperty HorizontalScrollPercentProperty =
		ScrollPatternIdentifiers.HorizontalScrollPercentProperty;
	public static readonly AutomationProperty HorizontalViewSizeProperty =
		ScrollPatternIdentifiers.HorizontalViewSizeProperty;
	public static readonly AutomationProperty VerticalScrollPercentProperty =
		ScrollPatternIdentifiers.VerticalScrollPercentProperty;
	public static readonly AutomationProperty VerticalViewSizeProperty =
		ScrollPatternIdentifiers.VerticalViewSizeProperty;
	public static readonly AutomationProperty HorizontallyScrollableProperty =
		ScrollPatternIdentifiers.HorizontallyScrollableProperty;
	public static readonly AutomationProperty VerticallyScrollableProperty =
		ScrollPatternIdentifiers.VerticallyScrollableProperty;

	readonly IScrollProvider _provider;
	readonly ScrollPatternInformation _cached;
	readonly bool _hasCache;

	internal ScrollPattern(AutomationElement element, IScrollProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	ScrollPatternInformation Read() => new ScrollPatternInformation(_provider);

	public ScrollPatternInformation Current => Run(Read);

	public ScrollPatternInformation Cached => _hasCache ? _cached : throw NotCached();

	public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount) =>
		Run(() => _provider.Scroll(horizontalAmount, verticalAmount));

	public void ScrollHorizontal(ScrollAmount amount) => Scroll(amount, ScrollAmount.NoAmount);

	public void ScrollVertical(ScrollAmount amount) => Scroll(ScrollAmount.NoAmount, amount);

	/// <exception cref="ArgumentOutOfRangeException">A percent is outside 0 to 100.</exception>
	public void SetScrollPercent(double horizontalPercent, double verticalPercent) =>
		Run(() => _provider.SetScrollPercent(horizontalPercent, verticalPercent));

	public struct ScrollPatternInformation
	{
		internal ScrollPatternInformation(IScrollProvider provider)
		{
			HorizontalScrollPercent = provider.HorizontalScrollPercent;
			HorizontalViewSize = provider.HorizontalViewSize;
			HorizontallyScrollable = provider.HorizontallyScrollable;
			VerticalScrollPercent = provider.VerticalScrollPercent;
			VerticalViewSize = provider.VerticalViewSize;
			VerticallyScrollable = provider.VerticallyScrollable;
		}

		public double HorizontalScrollPercent { get; }

		public double HorizontalViewSize { get; }

		public bool HorizontallyScrollable { get; }

		public double VerticalScrollPercent { get; }

		public double VerticalViewSize { get; }

		public bool VerticallyScrollable { get; }
	}
}
