using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Windows;

public abstract partial class UIElement
{
	static readonly List<UIElement> _layoutUpdatedSubscribers = new List<UIElement>();
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
			if (_layoutUpdated == null) _layoutUpdatedSubscribers.Add(this);
			_layoutUpdated += value;
		}
		remove
		{
			_layoutUpdated -= value;
			if (_layoutUpdated == null) _layoutUpdatedSubscribers.Remove(this);
		}
	}

	internal static void NoteLayoutWork() => _layoutWorkPending = true;

	internal static void FireLayoutUpdated()
	{
		if (!_layoutWorkPending || _firingLayoutUpdated) return;
		_layoutWorkPending = false;
		if (_layoutUpdatedSubscribers.Count == 0) return;
		_firingLayoutUpdated = true;
		try
		{
			foreach (var el in _layoutUpdatedSubscribers.ToArray())
				el._layoutUpdated?.Invoke(null, EventArgs.Empty);
		}
		finally
		{
			_firingLayoutUpdated = false;
		}
	}
}
