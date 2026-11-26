using System;

namespace System.Windows;

/// <summary>Standard handler delegate for routed events.</summary>
public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

/// <summary>
/// Base event-argument class for routed events.  Subclass to carry additional data.
/// </summary>
public class RoutedEventArgs : EventArgs
{
	object? _source;
	object? _originalSource;

	/// <summary>The <see cref="RoutedEvent"/> identifier.</summary>
	public RoutedEvent RoutedEvent { get; set; }

	/// <summary>
	/// The element that raised the event, as seen by the handler currently running. The first
	/// assignment also fixes <see cref="OriginalSource"/>; a later change moves only this value.
	/// </summary>
	public object? Source
	{
		get => _source;
		set
		{
			if (_source == null && _originalSource == null)
			{
				_source = _originalSource = value;
				OnSetSource(value!);
			}
			else if (!ReferenceEquals(_source, value))
			{
				_source = value;
				OnSetSource(value!);
			}
		}
	}

	/// <summary>The element that first raised the event (the innermost hit element for mouse input).</summary>
	public object? OriginalSource => _originalSource;

	/// <summary>
	/// When set to <c>true</c> by a handler, suppresses further routing (Bubble/Tunnel).
	/// </summary>
	public bool Handled { get; set; }

	/// <summary>Creates an instance; <see cref="RoutedEvent"/> must be set before raising.</summary>
	public RoutedEventArgs() { RoutedEvent = null!; }

	/// <summary>Creates an instance with a pre-set <see cref="RoutedEvent"/>.</summary>
	public RoutedEventArgs(RoutedEvent routedEvent)
	{
		RoutedEvent = routedEvent ?? throw new ArgumentNullException(nameof(routedEvent));
	}

	/// <summary>Creates an instance with a pre-set event and source (also the original source).</summary>
	public RoutedEventArgs(RoutedEvent routedEvent, object source) : this(routedEvent)
	{
		_source = _originalSource = source;
	}

	/// <summary>Called when <see cref="Source"/> changes; override to react to a new source.</summary>
	protected virtual void OnSetSource(object source) { }
}
