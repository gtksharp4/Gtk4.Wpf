namespace System.Windows.Input;

/// <summary>Specifies the direction in which focus moves.</summary>
public enum FocusNavigationDirection
{
	/// <summary>Move to the next element in tab order.</summary>
	Next,
	/// <summary>Move to the previous element in tab order.</summary>
	Previous,
	/// <summary>Move to the first element in tab order.</summary>
	First,
	/// <summary>Move to the last element in tab order.</summary>
	Last,
	/// <summary>Move to the element on the left.</summary>
	Left,
	/// <summary>Move to the element on the right.</summary>
	Right,
	/// <summary>Move to the element above.</summary>
	Up,
	/// <summary>Move to the element below.</summary>
	Down,
}
