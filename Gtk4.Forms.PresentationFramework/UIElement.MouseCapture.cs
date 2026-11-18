using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// Mouse capture surface: <see cref="CaptureMouse"/>, <see cref="ReleaseMouseCapture"/>,
/// <see cref="IsMouseCaptured"/> and the Got/LostMouseCapture events. The capture state machine
/// lives in <see cref="Mouse"/> (<c>Mouse.Capture.cs</c>), which also documents the GTK 4 gap.
/// </summary>
public abstract partial class UIElement
{
	static readonly DependencyPropertyKey _isMouseCapturedKey =
		DependencyProperty.RegisterReadOnly(nameof(IsMouseCaptured), typeof(bool), typeof(UIElement),
			new PropertyMetadata(false));

	/// <summary>Identifies the read-only <see cref="IsMouseCaptured"/> dependency property.</summary>
	public static readonly DependencyProperty IsMouseCapturedProperty = _isMouseCapturedKey.DependencyProperty;

	/// <summary>Gets whether this element has captured the mouse.</summary>
	public bool IsMouseCaptured => (bool)(GetValue(IsMouseCapturedProperty) ?? false);

	/// <summary>Occurs when <see cref="IsMouseCaptured"/> changes.</summary>
	public event DependencyPropertyChangedEventHandler? IsMouseCapturedChanged;

	/// <summary>Raised on the element that gains mouse capture.</summary>
	public static readonly RoutedEvent GotMouseCaptureEvent =
		EventManager.RegisterRoutedEvent("GotMouseCapture", RoutingStrategy.Bubble,
			typeof(MouseEventHandler), typeof(UIElement));

	/// <summary>Raised on the element that loses mouse capture.</summary>
	public static readonly RoutedEvent LostMouseCaptureEvent =
		EventManager.RegisterRoutedEvent("LostMouseCapture", RoutingStrategy.Bubble,
			typeof(MouseEventHandler), typeof(UIElement));

	/// <summary>Occurs when this element captures the mouse.</summary>
	public event MouseEventHandler? GotMouseCapture
	{
		add    => AddHandler(GotMouseCaptureEvent, value!);
		remove => RemoveHandler(GotMouseCaptureEvent, value!);
	}

	/// <summary>Occurs when this element loses mouse capture.</summary>
	public event MouseEventHandler? LostMouseCapture
	{
		add    => AddHandler(LostMouseCaptureEvent, value!);
		remove => RemoveHandler(LostMouseCaptureEvent, value!);
	}

	/// <summary>Attempts to capture the mouse to this element.</summary>
	/// <returns><c>true</c> if captured; <c>false</c> when the element is disabled or hidden.</returns>
	public bool CaptureMouse() => Mouse.Capture(this);

	/// <summary>Releases mouse capture if this element holds it.</summary>
	public void ReleaseMouseCapture()
	{
		if (ReferenceEquals(Mouse.Captured, this))
			Mouse.Capture(null);
	}

	/// <summary>
	/// Whether this element may take capture. WPF requires <c>IsEnabled</c> and <c>IsVisible</c>;
	/// this layer has no <c>UIElement.IsEnabled</c>/<c>IsVisible</c> DPs yet, so the GTK 4
	/// equivalents are read from the backing widget and its GTK ancestors (<c>sensitive</c> and
	/// <c>visible</c>, both inherited in effect like WPF's <c>IsEnabled</c>/<c>IsVisible</c>). An
	/// element without a created widget is treated as enabled and visible.
	/// </summary>
	internal bool CanCaptureMouse
	{
		get
		{
			for (Gtk.Widget? w = _widget; w != null; w = w.Parent)
				if (!w.Sensitive || !w.Visible) return false;
			return true;
		}
	}

	/// <summary>
	/// Whether the backing widget exists. Unlike <see cref="IsWidgetCreated"/> (which some
	/// controls override to track an inner container) this is exactly <c>_widget != null</c>.
	/// </summary>
	internal bool HasBackingWidget => _widget != null;

	/// <summary>Sets <see cref="IsMouseCaptured"/> and raises <see cref="IsMouseCapturedChanged"/>.</summary>
	internal void SetMouseCaptured(bool captured)
	{
		bool old = IsMouseCaptured;
		if (old == captured) return;
		SetValue(_isMouseCapturedKey, captured);
		IsMouseCapturedChanged?.Invoke(this,
			new DependencyPropertyChangedEventArgs(IsMouseCapturedProperty, old, captured));
	}

	/// <summary>Raises <see cref="GotMouseCaptureEvent"/> or <see cref="LostMouseCaptureEvent"/>.</summary>
	internal void RaiseMouseCaptureEvent(RoutedEvent routedEvent)
	{
		var e = new MouseEventArgs(routedEvent, Mouse._lastPosition, Mouse.LeftButton, Mouse.RightButton)
		{
			PositionSource = Mouse._lastPositionSource,
		};
		RaiseEvent(e);
	}
}
