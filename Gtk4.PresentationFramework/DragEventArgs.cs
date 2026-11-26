namespace System.Windows;

/// <summary>Delegate type for drag-event handlers (DragEnter/DragOver/DragLeave/Drop).</summary>
public delegate void DragEventHandler(object sender, DragEventArgs e);

/// <summary>Delegate type for <c>QueryContinueDrag</c> events.</summary>
public delegate void QueryContinueDragEventHandler(object sender, QueryContinueDragEventArgs e);

/// <summary>Delegate type for <c>GiveFeedback</c> events.</summary>
public delegate void GiveFeedbackEventHandler(object sender, GiveFeedbackEventArgs e);

/// <summary>
/// Event arguments for drag-and-drop events:
/// <c>DragEnter</c>, <c>DragOver</c>, <c>DragLeave</c>, and <c>Drop</c>.
/// </summary>
public class DragEventArgs : RoutedEventArgs
{
	readonly Point _position;

	/// <summary>
	/// Initialises a new <see cref="DragEventArgs"/>.
	/// </summary>
	/// <param name="routedEvent">The routed event being raised.</param>
	/// <param name="data">The data payload.</param>
	/// <param name="allowedEffects">Effects permitted by the drag source.</param>
	/// <param name="keyStates">Keyboard and mouse modifier states.</param>
	/// <param name="position">Drop position relative to the target element.</param>
	public DragEventArgs(
		RoutedEvent routedEvent,
		IDataObject data,
		DragDropEffects allowedEffects,
		DragDropKeyStates keyStates,
		Point position)
		: base(routedEvent)
	{
		ArgumentNullException.ThrowIfNull(data);
		Data           = data;
		AllowedEffects = allowedEffects;
		Effects        = allowedEffects;
		KeyStates      = keyStates;
		_position      = position;
	}

	/// <summary>The data being dragged.</summary>
	public IDataObject Data { get; }

	/// <summary>Effects permitted by the drag source.</summary>
	public DragDropEffects AllowedEffects { get; }

	/// <summary>
	/// Gets or sets the target-proposed drop effect.
	/// Drop handlers set this to indicate the desired operation.
	/// </summary>
	public DragDropEffects Effects { get; set; }

	/// <summary>Keyboard and mouse modifier states at the time of the event.</summary>
	public DragDropKeyStates KeyStates { get; }

	/// <summary>
	/// Returns the drop position relative to <paramref name="relativeTo"/>.
	/// When <paramref name="relativeTo"/> is <see langword="null"/> returns the raw position.
	/// </summary>
	public Point GetPosition(IInputElement? relativeTo) => _position;
}

/// <summary>
/// Event arguments for the <c>QueryContinueDrag</c> event, raised continuously while
/// the user drags to allow the source to cancel the operation.
/// </summary>
public class QueryContinueDragEventArgs : RoutedEventArgs
{
	/// <summary>
	/// Initialises a new <see cref="QueryContinueDragEventArgs"/>.
	/// </summary>
	/// <param name="routedEvent">The routed event being raised.</param>
	/// <param name="escapePressed">Whether Escape was pressed since the last query.</param>
	/// <param name="keyStates">Current keyboard and mouse modifier states.</param>
	public QueryContinueDragEventArgs(
		RoutedEvent routedEvent,
		bool escapePressed,
		DragDropKeyStates keyStates)
		: base(routedEvent)
	{
		EscapePressed = escapePressed;
		KeyStates     = keyStates;
	}

	/// <summary>Whether Escape was pressed.</summary>
	public bool EscapePressed { get; }

	/// <summary>Current keyboard and mouse modifier states.</summary>
	public DragDropKeyStates KeyStates { get; }

	/// <summary>Gets or sets the action to take for the drag operation.</summary>
	public DragAction Action { get; set; } = DragAction.Continue;
}

/// <summary>
/// Event arguments for the <c>GiveFeedback</c> event, raised to allow the drag
/// source to provide custom cursors based on the current <see cref="Effects"/>.
/// </summary>
public class GiveFeedbackEventArgs : RoutedEventArgs
{
	/// <summary>
	/// Initialises a new <see cref="GiveFeedbackEventArgs"/>.
	/// </summary>
	/// <param name="routedEvent">The routed event being raised.</param>
	/// <param name="effects">The drop effects currently in effect.</param>
	public GiveFeedbackEventArgs(RoutedEvent routedEvent, DragDropEffects effects)
		: base(routedEvent)
	{
		Effects = effects;
	}

	/// <summary>The current drop effects.</summary>
	public DragDropEffects Effects { get; }

	/// <summary>
	/// When set to <see langword="true"/>, the default system cursor feedback is suppressed.
	/// </summary>
	public bool UseDefaultCursors { get; set; } = true;
}

/// <summary>Specifies how the drag-and-drop operation should proceed.</summary>
public enum DragAction
{
	/// <summary>The operation should continue.</summary>
	Continue = 0,
	/// <summary>The operation should stop with a drop.</summary>
	Drop     = 1,
	/// <summary>The operation should be cancelled.</summary>
	Cancel   = 2,
}
