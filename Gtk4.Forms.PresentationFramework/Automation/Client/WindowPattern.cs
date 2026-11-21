using System.Windows.Automation.Provider;

namespace System.Windows.Automation;

/// <summary>Controls a window: its visual state, closing, and its capabilities.</summary>
public class WindowPattern : BasePattern
{
	public static readonly AutomationPattern Pattern = WindowPatternIdentifiers.Pattern;
	public static readonly AutomationProperty CanMaximizeProperty = WindowPatternIdentifiers.CanMaximizeProperty;
	public static readonly AutomationProperty CanMinimizeProperty = WindowPatternIdentifiers.CanMinimizeProperty;
	public static readonly AutomationProperty IsModalProperty = WindowPatternIdentifiers.IsModalProperty;
	public static readonly AutomationProperty IsTopmostProperty = WindowPatternIdentifiers.IsTopmostProperty;
	public static readonly AutomationProperty WindowVisualStateProperty =
		WindowPatternIdentifiers.WindowVisualStateProperty;
	public static readonly AutomationProperty WindowInteractionStateProperty =
		WindowPatternIdentifiers.WindowInteractionStateProperty;
	public static readonly AutomationEvent WindowOpenedEvent = WindowPatternIdentifiers.WindowOpenedEvent;
	public static readonly AutomationEvent WindowClosedEvent = WindowPatternIdentifiers.WindowClosedEvent;

	readonly IWindowProvider _provider;
	readonly WindowPatternInformation _cached;
	readonly bool _hasCache;

	internal WindowPattern(AutomationElement element, IWindowProvider provider, bool cached) : base(element)
	{
		_provider = provider;
		if (cached)
		{
			_cached = Read();
			_hasCache = true;
		}
	}

	WindowPatternInformation Read() => new WindowPatternInformation(_provider);

	public WindowPatternInformation Current => Run(Read);

	public WindowPatternInformation Cached => _hasCache ? _cached : throw NotCached();

	/// <exception cref="ElementNotEnabledException">The window is disabled.</exception>
	public void Close() => Run(_provider.Close);

	/// <exception cref="ElementNotEnabledException">The window is disabled.</exception>
	/// <exception cref="InvalidOperationException">The window cannot take that state.</exception>
	public void SetWindowVisualState(WindowVisualState state) => Run(() => _provider.SetVisualState(state));

	/// <summary>Always <c>true</c>: the layer has no input queue to wait on.</summary>
	public bool WaitForInputIdle(int milliseconds) => Run(() => _provider.WaitForInputIdle(milliseconds));

	public struct WindowPatternInformation
	{
		internal WindowPatternInformation(IWindowProvider provider)
		{
			CanMaximize = provider.Maximizable;
			CanMinimize = provider.Minimizable;
			IsModal = provider.IsModal;
			IsTopmost = provider.IsTopmost;
			WindowVisualState = provider.VisualState;
			WindowInteractionState = provider.InteractionState;
		}

		public bool CanMaximize { get; }

		public bool CanMinimize { get; }

		public bool IsModal { get; }

		public bool IsTopmost { get; }

		public WindowVisualState WindowVisualState { get; }

		public WindowInteractionState WindowInteractionState { get; }
	}
}
