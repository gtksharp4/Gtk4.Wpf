namespace System.Windows.Controls;

public partial class MediaElement
{
	/// <summary>Identifies <see cref="MediaOpened"/>.</summary>
	public static readonly RoutedEvent MediaOpenedEvent = EventManager.RegisterRoutedEvent(
		nameof(MediaOpened), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));

	/// <summary>Identifies <see cref="MediaEnded"/>.</summary>
	public static readonly RoutedEvent MediaEndedEvent = EventManager.RegisterRoutedEvent(
		nameof(MediaEnded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));

	/// <summary>Identifies <see cref="MediaFailed"/>.</summary>
	public static readonly RoutedEvent MediaFailedEvent = EventManager.RegisterRoutedEvent(
		nameof(MediaFailed), RoutingStrategy.Bubble, typeof(EventHandler<ExceptionRoutedEventArgs>),
		typeof(MediaElement));

	/// <summary>Identifies <see cref="BufferingStarted"/>.</summary>
	public static readonly RoutedEvent BufferingStartedEvent = EventManager.RegisterRoutedEvent(
		nameof(BufferingStarted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));

	/// <summary>Identifies <see cref="BufferingEnded"/>.</summary>
	public static readonly RoutedEvent BufferingEndedEvent = EventManager.RegisterRoutedEvent(
		nameof(BufferingEnded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));

	/// <summary>Identifies <see cref="ScriptCommand"/>.</summary>
	public static readonly RoutedEvent ScriptCommandEvent = EventManager.RegisterRoutedEvent(
		nameof(ScriptCommand), RoutingStrategy.Bubble, typeof(EventHandler<MediaScriptCommandRoutedEventArgs>),
		typeof(MediaElement));

	/// <summary>Occurs when the media has opened.</summary>
	public event RoutedEventHandler MediaOpened
	{
		add => AddHandler(MediaOpenedEvent, value);
		remove => RemoveHandler(MediaOpenedEvent, value);
	}

	/// <summary>Occurs when the media has played to its end.</summary>
	public event RoutedEventHandler MediaEnded
	{
		add => AddHandler(MediaEndedEvent, value);
		remove => RemoveHandler(MediaEndedEvent, value);
	}

	/// <summary>Occurs when the media could not be opened or decoded.</summary>
	public event EventHandler<ExceptionRoutedEventArgs> MediaFailed
	{
		add => AddHandler(MediaFailedEvent, value);
		remove => RemoveHandler(MediaFailedEvent, value);
	}

	/// <summary>Occurs when buffering starts. GTK streams never raise it.</summary>
	public event RoutedEventHandler BufferingStarted
	{
		add => AddHandler(BufferingStartedEvent, value);
		remove => RemoveHandler(BufferingStartedEvent, value);
	}

	/// <summary>Occurs when buffering ends. GTK streams never raise it.</summary>
	public event RoutedEventHandler BufferingEnded
	{
		add => AddHandler(BufferingEndedEvent, value);
		remove => RemoveHandler(BufferingEndedEvent, value);
	}

	/// <summary>Occurs when a script command is reached. GTK streams never raise it.</summary>
	public event EventHandler<MediaScriptCommandRoutedEventArgs> ScriptCommand
	{
		add => AddHandler(ScriptCommandEvent, value);
		remove => RemoveHandler(ScriptCommandEvent, value);
	}

	void RaiseOpened() => RaiseEvent(new RoutedEventArgs(MediaOpenedEvent) { Source = this });

	void RaiseEnded() => RaiseEvent(new RoutedEventArgs(MediaEndedEvent) { Source = this });

	void RaiseFailed(Exception error) =>
		RaiseEvent(new ExceptionRoutedEventArgs(MediaFailedEvent, this, error));
}
