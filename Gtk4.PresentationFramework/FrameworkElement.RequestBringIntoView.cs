namespace System.Windows;

/// <summary>
/// RequestBringIntoView. <see cref="BringIntoView()"/> raises it on the element and it bubbles to a viewer that
/// can scroll or page to the target. Handled here: FlowDocumentPageViewer and FlowDocumentReader (the page that
/// holds a document element) and FlowDocumentScrollViewer (scrolls its window). Gap: ScrollViewer does not handle
/// the request yet, so an unhandled request reaches the root and is dropped.
/// </summary>
public abstract partial class FrameworkElement
{
	/// <summary>Bubble event raised to ask an ancestor viewer to bring an element into view.</summary>
	public static readonly RoutedEvent RequestBringIntoViewEvent = EventManager.RegisterRoutedEvent(
		"RequestBringIntoView", RoutingStrategy.Bubble, typeof(RequestBringIntoViewEventHandler),
		typeof(FrameworkElement));

	/// <summary>Occurs when this element or a descendant asks to be brought into view.</summary>
	public event RequestBringIntoViewEventHandler RequestBringIntoView
	{
		add => AddHandler(RequestBringIntoViewEvent, value);
		remove => RemoveHandler(RequestBringIntoViewEvent, value);
	}

	/// <summary>Tries to bring the whole element into view.</summary>
	public void BringIntoView() => BringIntoView(Rect.Empty);

	/// <summary>Tries to bring the given region of the element into view.</summary>
	public void BringIntoView(Rect targetRectangle) =>
		RaiseEvent(new RequestBringIntoViewEventArgs(this, targetRectangle));
}
