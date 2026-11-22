namespace System.Windows;

/// <summary>Data for <see cref="FrameworkElement.RequestBringIntoViewEvent"/>.</summary>
public sealed class RequestBringIntoViewEventArgs : RoutedEventArgs
{
	/// <summary>Creates the arguments for a request to bring <paramref name="child"/> into view.</summary>
	internal RequestBringIntoViewEventArgs(DependencyObject child, Rect rectangle)
		: base(FrameworkElement.RequestBringIntoViewEvent)
	{
		TargetObject = child;
		TargetRect = rectangle;
	}

	/// <summary>The element to bring into view.</summary>
	public DependencyObject TargetObject { get; }

	/// <summary>The region of the element to bring into view; <see cref="Rect.Empty"/> for all of it.</summary>
	public Rect TargetRect { get; }
}
