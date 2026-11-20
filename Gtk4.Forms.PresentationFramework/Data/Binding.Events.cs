namespace System.Windows.Data;

public partial class Binding
{
	/// <summary>Identifies the <c>TargetUpdated</c> routed event (Bubble).</summary>
	public static readonly RoutedEvent TargetUpdatedEvent =
		EventManager.RegisterRoutedEvent(
			"TargetUpdated", RoutingStrategy.Bubble, typeof(EventHandler<DataTransferEventArgs>), typeof(Binding));

	/// <summary>Identifies the <c>SourceUpdated</c> routed event (Bubble).</summary>
	public static readonly RoutedEvent SourceUpdatedEvent =
		EventManager.RegisterRoutedEvent(
			"SourceUpdated", RoutingStrategy.Bubble, typeof(EventHandler<DataTransferEventArgs>), typeof(Binding));

	/// <summary>
	/// Gets or sets whether <see cref="TargetUpdatedEvent"/> is raised when a value is transferred to the target.
	/// </summary>
	public bool NotifyOnTargetUpdated { get; set; }

	/// <summary>
	/// Gets or sets whether <see cref="SourceUpdatedEvent"/> is raised when a value is transferred to the source.
	/// </summary>
	public bool NotifyOnSourceUpdated { get; set; }

	/// <summary>Adds a <see cref="TargetUpdatedEvent"/> handler to <paramref name="element"/>.</summary>
	public static void AddTargetUpdatedHandler(DependencyObject element, EventHandler<DataTransferEventArgs> handler)
	{
		if (element is UIElement uie) uie.AddHandler(TargetUpdatedEvent, handler);
	}

	/// <summary>Removes a <see cref="TargetUpdatedEvent"/> handler from <paramref name="element"/>.</summary>
	public static void RemoveTargetUpdatedHandler(DependencyObject element, EventHandler<DataTransferEventArgs> handler)
	{
		if (element is UIElement uie) uie.RemoveHandler(TargetUpdatedEvent, handler);
	}

	/// <summary>Adds a <see cref="SourceUpdatedEvent"/> handler to <paramref name="element"/>.</summary>
	public static void AddSourceUpdatedHandler(DependencyObject element, EventHandler<DataTransferEventArgs> handler)
	{
		if (element is UIElement uie) uie.AddHandler(SourceUpdatedEvent, handler);
	}

	/// <summary>Removes a <see cref="SourceUpdatedEvent"/> handler from <paramref name="element"/>.</summary>
	public static void RemoveSourceUpdatedHandler(DependencyObject element, EventHandler<DataTransferEventArgs> handler)
	{
		if (element is UIElement uie) uie.RemoveHandler(SourceUpdatedEvent, handler);
	}
}
