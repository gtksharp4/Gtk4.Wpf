namespace System.Windows.Controls;

/// <summary>
/// Arguments for the <see cref="ScrollViewer.ScrollChanged"/> event, carrying
/// the new and previous offset/extent/viewport metrics.
/// </summary>
public class ScrollChangedEventArgs : RoutedEventArgs
{
	/// <summary>Initialises the args; all metrics must be supplied.</summary>
	internal ScrollChangedEventArgs(
		RoutedEvent routedEvent,
		double horizontalOffset,
		double verticalOffset,
		double extentWidth,
		double extentHeight,
		double viewportWidth,
		double viewportHeight,
		double horizontalChange,
		double verticalChange,
		double extentWidthChange,
		double extentHeightChange,
		double viewportWidthChange,
		double viewportHeightChange)
		: base(routedEvent)
	{
		HorizontalOffset = horizontalOffset;
		VerticalOffset = verticalOffset;
		ExtentWidth = extentWidth;
		ExtentHeight = extentHeight;
		ViewportWidth = viewportWidth;
		ViewportHeight = viewportHeight;
		HorizontalChange = horizontalChange;
		VerticalChange = verticalChange;
		ExtentWidthChange = extentWidthChange;
		ExtentHeightChange = extentHeightChange;
		ViewportWidthChange = viewportWidthChange;
		ViewportHeightChange = viewportHeightChange;
	}

	/// <summary>Current horizontal scroll offset.</summary>
	public double HorizontalOffset { get; }

	/// <summary>Current vertical scroll offset.</summary>
	public double VerticalOffset { get; }

	/// <summary>Total horizontal extent of the scrollable content.</summary>
	public double ExtentWidth { get; }

	/// <summary>Total vertical extent of the scrollable content.</summary>
	public double ExtentHeight { get; }

	/// <summary>Width of the visible viewport.</summary>
	public double ViewportWidth { get; }

	/// <summary>Height of the visible viewport.</summary>
	public double ViewportHeight { get; }

	/// <summary>Change in <see cref="HorizontalOffset"/> since last event.</summary>
	public double HorizontalChange { get; }

	/// <summary>Change in <see cref="VerticalOffset"/> since last event.</summary>
	public double VerticalChange { get; }

	/// <summary>Change in <see cref="ExtentWidth"/> since last event.</summary>
	public double ExtentWidthChange { get; }

	/// <summary>Change in <see cref="ExtentHeight"/> since last event.</summary>
	public double ExtentHeightChange { get; }

	/// <summary>Change in <see cref="ViewportWidth"/> since last event.</summary>
	public double ViewportWidthChange { get; }

	/// <summary>Change in <see cref="ViewportHeight"/> since last event.</summary>
	public double ViewportHeightChange { get; }
}
