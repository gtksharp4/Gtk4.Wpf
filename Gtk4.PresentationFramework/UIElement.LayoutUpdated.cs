using System;
using System.Collections.Generic;

namespace System.Windows;

public abstract partial class UIElement
{
	// Weak: a subscribed element must not be kept alive by the static registry.
	static readonly List<WeakReference<UIElement>> _layoutUpdatedSubscribers = new List<WeakReference<UIElement>>();
	static bool _layoutWorkPending;
	static bool _firingLayoutUpdated;

	EventHandler? _layoutUpdated;

	/// <summary>
	/// Occurs after a layout pass in which any element was measured or arranged. A plain CLR event
	/// (not routed); every subscriber is notified and the sender is <c>null</c>.
	/// </summary>
	public event EventHandler LayoutUpdated
	{
		add
		{
			if (value == null) return;
			if (_layoutUpdated == null) _layoutUpdatedSubscribers.Add(new WeakReference<UIElement>(this));
			_layoutUpdated += value;
		}
		remove
		{
			_layoutUpdated -= value;
			if (_layoutUpdated == null) PruneLayoutUpdatedSubscribers(this);
		}
	}

	internal static void NoteLayoutWork() => _layoutWorkPending = true;

	// Drops dead entries and, when given, the entry for <paramref name="remove"/>.
	static void PruneLayoutUpdatedSubscribers(UIElement? remove)
	{
		_layoutUpdatedSubscribers.RemoveAll(w => !w.TryGetTarget(out var el) || ReferenceEquals(el, remove));
	}

	internal static void FireLayoutUpdated()
	{
		if (!_layoutWorkPending || _firingLayoutUpdated) return;
		_layoutWorkPending = false;
		if (_layoutUpdatedSubscribers.Count == 0) return;
		_firingLayoutUpdated = true;
		try
		{
			var live = new List<UIElement>();
			foreach (var w in _layoutUpdatedSubscribers.ToArray())
				if (w.TryGetTarget(out var el)) live.Add(el);
			PruneLayoutUpdatedSubscribers(null);
			foreach (var el in live)
				el._layoutUpdated?.Invoke(null, EventArgs.Empty);
		}
		finally
		{
			_firingLayoutUpdated = false;
		}
	}
}
