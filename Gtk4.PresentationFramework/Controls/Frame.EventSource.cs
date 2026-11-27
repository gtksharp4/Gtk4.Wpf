namespace System.Windows.Controls;

public partial class Frame
{
	/// <summary>
	/// An event bubbling out of the navigated content reaches the outer tree with this frame as its Source
	/// (WPF <c>Frame.AdjustEventSource</c>: the content is not a logical child of the frame).
	/// </summary>
	internal override object? AdjustEventSource(RoutedEventArgs e) => this;
}
