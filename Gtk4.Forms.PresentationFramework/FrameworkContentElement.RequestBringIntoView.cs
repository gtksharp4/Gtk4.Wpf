namespace System.Windows;

public partial class FrameworkContentElement
{
	/// <summary>
	/// Raises <see cref="FrameworkElement.RequestBringIntoViewEvent"/> on this element with the whole element as
	/// the target (<see cref="Rect.Empty"/>); it bubbles through the content parents to the hosting viewer.
	/// </summary>
	public void BringIntoView() => this.RaiseEvent(new RequestBringIntoViewEventArgs(this, Rect.Empty));
}
