using System.Windows.Automation.Provider;

namespace System.Windows.Automation;

/// <summary>Starts the single action of a control, such as a button click.</summary>
public class InvokePattern : BasePattern
{
	public static readonly AutomationPattern Pattern = InvokePatternIdentifiers.Pattern;
	public static readonly AutomationEvent InvokedEvent = InvokePatternIdentifiers.InvokedEvent;

	readonly IInvokeProvider _provider;

	internal InvokePattern(AutomationElement element, IInvokeProvider provider, bool cached) : base(element)
	{
		_provider = provider;
	}

	/// <exception cref="ElementNotEnabledException">The element is disabled.</exception>
	public void Invoke() => Run(_provider.Invoke);
}

/// <summary>Reads and sets the string value of a control, such as a text box.</summary>
public class ValuePattern : BasePattern
{
	public static readonly AutomationPattern Pattern = ValuePatternIdentifiers.Pattern;
	public static readonly AutomationProperty ValueProperty = ValuePatternIdentifiers.ValueProperty;
	public static readonly AutomationProperty IsReadOnlyProperty = ValuePatternIdentifiers.IsReadOnlyProperty;

	readonly IValueProvider _provider;
	readonly ValuePatternInformation _cached;
	readonly bool _hasCache;

	internal ValuePattern(AutomationElement element, IValueProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	ValuePatternInformation Read() => new ValuePatternInformation(_provider.Value, _provider.IsReadOnly);

	public ValuePatternInformation Current => Run(Read);

	public ValuePatternInformation Cached => _hasCache ? _cached : throw NotCached();

	/// <exception cref="ElementNotEnabledException">The element is disabled.</exception>
	/// <exception cref="InvalidOperationException">The value is read-only.</exception>
	public void SetValue(string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		Run(() => _provider.SetValue(value));
	}

	public struct ValuePatternInformation
	{
		internal ValuePatternInformation(string value, bool isReadOnly)
		{
			Value = value;
			IsReadOnly = isReadOnly;
		}

		public string Value { get; }

		public bool IsReadOnly { get; }
	}
}

/// <summary>Reads and changes the state of a control that cycles through states, such as a check box.</summary>
public class TogglePattern : BasePattern
{
	public static readonly AutomationPattern Pattern = TogglePatternIdentifiers.Pattern;
	public static readonly AutomationProperty ToggleStateProperty = TogglePatternIdentifiers.ToggleStateProperty;

	readonly IToggleProvider _provider;
	readonly TogglePatternInformation _cached;
	readonly bool _hasCache;

	internal TogglePattern(AutomationElement element, IToggleProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	TogglePatternInformation Read() => new TogglePatternInformation(_provider.ToggleState);

	public TogglePatternInformation Current => Run(Read);

	public TogglePatternInformation Cached => _hasCache ? _cached : throw NotCached();

	/// <exception cref="ElementNotEnabledException">The element is disabled.</exception>
	public void Toggle() => Run(_provider.Toggle);

	public struct TogglePatternInformation
	{
		internal TogglePatternInformation(ToggleState state) => ToggleState = state;

		public ToggleState ToggleState { get; }
	}
}

/// <summary>Expands and collapses a control, such as a combo box or tree item.</summary>
public class ExpandCollapsePattern : BasePattern
{
	public static readonly AutomationPattern Pattern = ExpandCollapsePatternIdentifiers.Pattern;
	public static readonly AutomationProperty ExpandCollapseStateProperty =
		ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty;

	readonly IExpandCollapseProvider _provider;
	readonly ExpandCollapsePatternInformation _cached;
	readonly bool _hasCache;

	internal ExpandCollapsePattern(AutomationElement element, IExpandCollapseProvider provider, bool cached)
		: base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	ExpandCollapsePatternInformation Read() => new ExpandCollapsePatternInformation(_provider.ExpandCollapseState);

	public ExpandCollapsePatternInformation Current => Run(Read);

	public ExpandCollapsePatternInformation Cached => _hasCache ? _cached : throw NotCached();

	/// <exception cref="ElementNotEnabledException">The element is disabled.</exception>
	public void Expand() => Run(_provider.Expand);

	/// <exception cref="ElementNotEnabledException">The element is disabled.</exception>
	public void Collapse() => Run(_provider.Collapse);

	public struct ExpandCollapsePatternInformation
	{
		internal ExpandCollapsePatternInformation(ExpandCollapseState state) => ExpandCollapseState = state;

		public ExpandCollapseState ExpandCollapseState { get; }
	}
}
