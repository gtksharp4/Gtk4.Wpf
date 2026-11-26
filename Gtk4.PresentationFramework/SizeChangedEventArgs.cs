namespace System.Windows;

/// <summary>Represents the method that handles <see cref="FrameworkElement.SizeChanged"/>.</summary>
public delegate void SizeChangedEventHandler(object sender, SizeChangedEventArgs e);

/// <summary>
/// Event data for <see cref="FrameworkElement.SizeChanged"/>. As in WPF there is no public
/// constructor: instances are created by the layout system.
/// </summary>
public class SizeChangedEventArgs : RoutedEventArgs
{
	internal SizeChangedEventArgs(UIElement element, SizeChangedInfo info)
		: base(FrameworkElement.SizeChangedEvent)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (info == null) throw new ArgumentNullException(nameof(info));
		Source = element;
		PreviousSize = info.PreviousSize;
		NewSize = element.RenderSize;
		WidthChanged = info.WidthChanged;
		HeightChanged = info.HeightChanged;
	}

	/// <summary>The size before the change.</summary>
	public Size PreviousSize { get; }

	/// <summary>The new size.</summary>
	public Size NewSize { get; }

	/// <summary>Whether the width component changed.</summary>
	public bool WidthChanged { get; }

	/// <summary>Whether the height component changed.</summary>
	public bool HeightChanged { get; }
}
