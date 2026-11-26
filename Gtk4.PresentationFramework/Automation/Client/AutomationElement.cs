using System.Collections.Generic;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;

namespace System.Windows.Automation;

/// <summary>
/// A UI Automation element of the running application, backed in process by an
/// <see cref="AutomationPeer"/>. Every call runs on the GTK thread through the dispatcher.
/// </summary>
public partial class AutomationElement
{
	sealed class NotSupportedValue
	{
		public override string ToString() => "NotSupported";
	}

	/// <summary>The value reported for a property the element does not support.</summary>
	public static readonly object NotSupported = new NotSupportedValue();

	readonly ElementCache? _cache;

	internal AutomationElement(AutomationPeer peer, ElementCache? cache)
	{
		Peer = peer;
		_cache = cache;
	}

	internal AutomationPeer Peer { get; }

	/// <summary>Wraps <paramref name="peer"/>, caching what <paramref name="request"/> asks for.</summary>
	internal static AutomationElement Wrap(AutomationPeer peer, CacheRequest? request)
	{
		var element = new AutomationElement(peer, null);
		return request == null || request.IsEmpty ? element : element.WithCache(request);
	}

	// ── Entry points ─────────────────────────────────────────────────────────────

	/// <summary>The root of the tree: its children are the application windows.</summary>
	public static AutomationElement RootElement
	{
		get
		{
			var request = CacheRequest.ActiveOrNull;
			return AutomationThread.Call(() => Wrap(DesktopAutomationPeer.Instance, request));
		}
	}

	/// <summary>The element that has keyboard focus, or <c>null</c>.</summary>
	public static AutomationElement FocusedElement
	{
		get
		{
			var request = CacheRequest.ActiveOrNull;
			return AutomationThread.Call(() =>
			{
				for (var e = Keyboard.FocusedElement as UIElement; e != null; e = e.VisualParent as UIElement)
					if (e.GetAutomationPeer() is { } peer)
						return Wrap(peer, request);
				return null!;
			});
		}
	}

	/// <summary>
	/// Returns the element at <paramref name="pt"/>. GTK 4 has no screen position for a window, so the point is
	/// in window coordinates, like <see cref="BoundingRectangle"/>.
	/// </summary>
	public static AutomationElement FromPoint(Point pt)
	{
		var request = CacheRequest.ActiveOrNull;
		return AutomationThread.Call(() =>
			Wrap(DesktopAutomationPeer.Instance.GetPeerFromPoint(pt) ?? DesktopAutomationPeer.Instance, request));
	}

	/// <summary>Not supported: GTK windows have no native handle UI Automation could resolve.</summary>
	public static AutomationElement FromHandle(IntPtr hwnd) =>
		throw new PlatformNotSupportedException("GTK windows have no window handle to resolve an element from.");

	/// <summary>Returns the element of a provider created by this layer's peers.</summary>
	public static AutomationElement FromLocalProvider(IRawElementProviderSimple localImpl)
	{
		if (localImpl == null) throw new ArgumentNullException(nameof(localImpl));
		var request = CacheRequest.ActiveOrNull;
		return AutomationThread.Call(() => FromProvider(localImpl, request)
			?? throw new ArgumentException("The provider does not come from an automation peer.", nameof(localImpl)));
	}

	internal static AutomationElement? FromProvider(IRawElementProviderSimple? provider,
		CacheRequest? request = null) =>
		provider is PeerProvider peer ? Wrap(peer.Peer, request) : null;

	internal static AutomationElement[] FromProviders(IRawElementProviderSimple[]? providers)
	{
		var result = new List<AutomationElement>();
		if (providers != null)
			foreach (var provider in providers)
				if (FromProvider(provider) is { } element)
					result.Add(element);
		return result.ToArray();
	}

	// ── Identity ─────────────────────────────────────────────────────────────────

	internal static int[] RuntimeIdOf(AutomationPeer peer) => new[] { 42, peer.RuntimeId };

	/// <summary>Returns the id that tells this element apart from every other.</summary>
	public int[] GetRuntimeId() => AutomationThread.Call(() => RuntimeIdOf(Peer));

	public override bool Equals(object? obj) => obj is AutomationElement other && ReferenceEquals(other.Peer, Peer);

	public override int GetHashCode() => Peer.RuntimeId;

	public static bool operator ==(AutomationElement? left, AutomationElement? right) =>
		left is null ? right is null : left.Equals(right);

	public static bool operator !=(AutomationElement? left, AutomationElement? right) => !(left == right);

	// ── Availability and focus ───────────────────────────────────────────────────

	/// <summary>Throws when the element's window has closed.</summary>
	internal void EnsureAvailable()
	{
		if (Peer is not UIElementAutomationPeer ui) return;
		DependencyObject? top = ui.Owner;
		while (top is UIElement e && e.VisualParent is { } parent)
			top = parent;
		if (top is Window { IsClosedWindow: true })
			throw new ElementNotAvailableException();
	}

	/// <summary>Moves keyboard focus to the element.</summary>
	/// <exception cref="InvalidOperationException">The element cannot take focus.</exception>
	public void SetFocus() => AutomationThread.Call(() =>
	{
		EnsureAvailable();
		Peer.SetFocus();
	});

	/// <summary>Returns a point inside the element that a click reaches.</summary>
	/// <exception cref="NoClickablePointException">There is none.</exception>
	public Point GetClickablePoint() =>
		TryGetClickablePoint(out var point) ? point : throw new NoClickablePointException();

	public bool TryGetClickablePoint(out Point pt)
	{
		var result = AutomationThread.Call(() =>
		{
			EnsureAvailable();
			return ReadPropertyCore(AutomationElementIdentifiers.ClickablePointProperty, ignoreDefault: true);
		});
		pt = result is Point point ? point : default;
		return result is Point;
	}
}
