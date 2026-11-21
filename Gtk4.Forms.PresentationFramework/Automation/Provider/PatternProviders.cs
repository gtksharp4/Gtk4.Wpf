namespace System.Windows.Automation.Provider;

/// <summary>Supports controls that start a single, unambiguous action.</summary>
public interface IInvokeProvider
{
	void Invoke();
}

/// <summary>Supports controls that hold a string value.</summary>
public interface IValueProvider
{
	bool IsReadOnly { get; }

	string Value { get; }

	void SetValue(string value);
}

/// <summary>Supports controls that cycle through a set of states.</summary>
public interface IToggleProvider
{
	ToggleState ToggleState { get; }

	void Toggle();
}

/// <summary>Supports controls that hold selectable items.</summary>
public interface ISelectionProvider
{
	bool CanSelectMultiple { get; }

	bool IsSelectionRequired { get; }

	IRawElementProviderSimple[] GetSelection();
}

/// <summary>Supports the items of a selection container.</summary>
public interface ISelectionItemProvider
{
	bool IsSelected { get; }

	IRawElementProviderSimple SelectionContainer { get; }

	void Select();

	void AddToSelection();

	void RemoveFromSelection();
}

/// <summary>Supports controls that expand and collapse.</summary>
public interface IExpandCollapseProvider
{
	ExpandCollapseState ExpandCollapseState { get; }

	void Expand();

	void Collapse();
}

/// <summary>Supports controls that hold a numeric value in a range.</summary>
public interface IRangeValueProvider
{
	bool IsReadOnly { get; }

	double LargeChange { get; }

	double Maximum { get; }

	double Minimum { get; }

	double SmallChange { get; }

	double Value { get; }

	void SetValue(double value);
}

/// <summary>Supports controls that scroll their content.</summary>
public interface IScrollProvider
{
	double HorizontalScrollPercent { get; }

	double HorizontalViewSize { get; }

	bool HorizontallyScrollable { get; }

	double VerticalScrollPercent { get; }

	double VerticalViewSize { get; }

	bool VerticallyScrollable { get; }

	void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount);

	void SetScrollPercent(double horizontalPercent, double verticalPercent);
}

/// <summary>Supports elements that can be scrolled into view.</summary>
public interface IScrollItemProvider
{
	void ScrollIntoView();
}

/// <summary>Supports windows.</summary>
public interface IWindowProvider
{
	bool IsModal { get; }

	bool IsTopmost { get; }

	bool Maximizable { get; }

	bool Minimizable { get; }

	WindowInteractionState InteractionState { get; }

	WindowVisualState VisualState { get; }

	void Close();

	void SetVisualState(WindowVisualState state);

	bool WaitForInputIdle(int milliseconds);
}
