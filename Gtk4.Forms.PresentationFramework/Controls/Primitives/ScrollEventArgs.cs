namespace System.Windows.Controls.Primitives;

/// <summary>Identifies the type of action that caused a <see cref="ScrollBar.Scroll"/> event.</summary>
public enum ScrollEventType
{
	/// <summary>Thumb moved to the start (top/left).</summary>
	First,
	/// <summary>Thumb moved to the end (bottom/right).</summary>
	Last,
	/// <summary>Scrolled toward the start by a large amount (page).</summary>
	LargeDecrement,
	/// <summary>Scrolled toward the end by a large amount (page).</summary>
	LargeIncrement,
	/// <summary>Scrolled toward the start by a small amount (line).</summary>
	SmallDecrement,
	/// <summary>Scrolled toward the end by a small amount (line).</summary>
	SmallIncrement,
	/// <summary>Thumb dragged to a new position.</summary>
	ThumbPosition,
	/// <summary>Thumb is being dragged.</summary>
	ThumbTrack,
	/// <summary>Thumb drag ended.</summary>
	EndScroll,
}

/// <summary>Arguments for the <see cref="ScrollBar.Scroll"/> routed event.</summary>
public class ScrollEventArgs : RoutedEventArgs
{
	/// <summary>Initialises a new instance.</summary>
	/// <param name="scrollEventType">What caused the scroll.</param>
	/// <param name="newValue">The new <see cref="ScrollBar.Value"/>.</param>
	public ScrollEventArgs(ScrollEventType scrollEventType, double newValue)
	{
		ScrollEventType = scrollEventType;
		NewValue = newValue;
	}

	/// <summary>Gets the type of scroll action.</summary>
	public ScrollEventType ScrollEventType { get; }

	/// <summary>Gets the new scroll value.</summary>
	public double NewValue { get; }
}
