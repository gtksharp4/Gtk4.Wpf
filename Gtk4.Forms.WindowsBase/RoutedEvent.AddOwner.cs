using System;

namespace System.Windows;

public sealed partial class RoutedEvent
{
	/// <summary>
	/// Makes this event available on <paramref name="ownerType"/> too, so
	/// <see cref="EventManager.GetRoutedEvent"/> finds it there. Returns this same event.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="ownerType"/> is null.</exception>
	public RoutedEvent AddOwner(Type ownerType)
	{
		if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));
		EventManager.RegisterOwner(this, ownerType);
		return this;
	}
}

public static partial class EventManager
{
	internal static void RegisterOwner(RoutedEvent routedEvent, Type ownerType)
	{
		lock (_lock)
			_registry[(routedEvent.Name, ownerType)] = routedEvent;
	}
}
