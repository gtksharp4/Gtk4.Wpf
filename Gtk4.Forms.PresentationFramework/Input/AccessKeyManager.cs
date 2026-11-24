using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Input;

/// <summary>
/// Registers elements under access keys and invokes them (WPF's <c>AccessKeyManager</c>). Keys are normalized
/// with <c>ToUpperInvariant</c> and held by weak reference. A <see cref="Window"/> stands in for WPF's
/// <c>PresentationSource</c> as the scope. <c>ProcessKey</c> is in <c>AccessKeyManager.Process.cs</c>, the
/// class handlers in <c>AccessKeyManager.Handlers.cs</c>.
/// </summary>
public sealed partial class AccessKeyManager
{
	static readonly Dictionary<string, List<WeakReference>> _keys = new();
	static readonly object _lock = new();

	static AccessKeyManager()
	{
		RegisterClassHandlers();
	}

	AccessKeyManager()
	{
	}

	/// <summary>Identifies the <c>AccessKeyPressed</c> routed event (Bubble).</summary>
	public static readonly RoutedEvent AccessKeyPressedEvent = EventManager.RegisterRoutedEvent(
		"AccessKeyPressed", RoutingStrategy.Bubble, typeof(AccessKeyPressedEventHandler), typeof(AccessKeyManager));

	/// <summary>Adds a handler for <see cref="AccessKeyPressedEvent"/> on <paramref name="element"/>.</summary>
	public static void AddAccessKeyPressedHandler(DependencyObject element, AccessKeyPressedEventHandler handler)
		=> Owner(element).AddHandler(AccessKeyPressedEvent, handler);

	/// <summary>Removes a handler for <see cref="AccessKeyPressedEvent"/> from <paramref name="element"/>.</summary>
	public static void RemoveAccessKeyPressedHandler(DependencyObject element, AccessKeyPressedEventHandler handler)
		=> Owner(element).RemoveHandler(AccessKeyPressedEvent, handler);

	static UIElement Owner(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return element as UIElement ?? throw new ArgumentException("Not a UIElement.", nameof(element));
	}

	/// <summary>Registers <paramref name="element"/> for <paramref name="key"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="element"/> is null.</exception>
	/// <exception cref="ArgumentException"><paramref name="key"/> is not exactly one text element.</exception>
	public static void Register(string key, IInputElement element)
	{
		key = Normalize(key);
		if (element == null) throw new ArgumentNullException(nameof(element));
		lock (_lock)
		{
			if (!_keys.TryGetValue(key, out var list))
				_keys[key] = list = new List<WeakReference>();
			list.RemoveAll(w => !w.IsAlive);
			if (!list.Exists(w => ReferenceEquals(w.Target, element)))
				list.Add(new WeakReference(element));
		}
	}

	/// <summary>Removes the registration of <paramref name="element"/> under <paramref name="key"/>.</summary>
	public static void Unregister(string key, IInputElement element)
	{
		key = Normalize(key);
		if (element == null) throw new ArgumentNullException(nameof(element));
		lock (_lock)
		{
			if (!_keys.TryGetValue(key, out var list)) return;
			list.RemoveAll(w => !w.IsAlive || ReferenceEquals(w.Target, element));
			if (list.Count == 0) _keys.Remove(key);
		}
	}

	/// <summary>Whether a targetable element in <paramref name="scope"/> is registered for <paramref name="key"/>.</summary>
	/// <remarks>WPF also answers for elements in the scope that are not targetable; this layer does not.</remarks>
	public static bool IsKeyRegistered(object scope, string key)
	{
		key = Normalize(key);
		foreach (var element in Alive(key))
			if (IsTargetable(element) && ReferenceEquals(ScopeOf(element), scope))
				return true;
		return false;
	}

	internal static int RegisteredCount(string key)
	{
		lock (_lock)
			return Alive(Normalize(key)).Count;
	}

	static string Normalize(string key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		key = key.ToUpperInvariant();
		if (new StringInfo(key).LengthInTextElements != 1)
			throw new ArgumentException("The key must be exactly one Unicode character.", nameof(key));
		return key;
	}

	// Live registrations for a normalized key, in registration order; dead references are purged.
	static List<UIElement> Alive(string key)
	{
		var result = new List<UIElement>();
		lock (_lock)
		{
			if (!_keys.TryGetValue(key, out var list)) return result;
			list.RemoveAll(w => !w.IsAlive);
			foreach (var w in list)
				if (w.Target is UIElement e) result.Add(e);
			if (list.Count == 0) _keys.Remove(key);
		}
		return result;
	}
}
