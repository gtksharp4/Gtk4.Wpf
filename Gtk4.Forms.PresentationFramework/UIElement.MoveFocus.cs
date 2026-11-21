using System.Collections.Generic;
using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// <see cref="MoveFocus"/> and <see cref="PredictFocus"/>. Tab-order moves follow the logical tree sorted by
/// <see cref="KeyboardNavigation.TabIndexProperty"/> (controls only); directional moves pick the nearest
/// focusable element in
/// that direction by GTK widget bounds, and fall back to tab order while the tree has no layout.
/// </summary>
public abstract partial class UIElement
{
	/// <summary>Moves keyboard focus away from this element in the requested direction.</summary>
	/// <returns><c>true</c> if focus moved to another element.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="request"/> is null.</exception>
	public bool MoveFocus(TraversalRequest request)
	{
		if (request == null) throw new ArgumentNullException(nameof(request));
		var target = FindFocusTarget(request.FocusNavigationDirection, request.Wrapped) as UIElement;
		return target != null && target.Focus();
	}

	/// <summary>The element that would get focus for <paramref name="direction"/>, without moving it.</summary>
	public DependencyObject? PredictFocus(FocusNavigationDirection direction)
	{
		if (direction < FocusNavigationDirection.Next || direction > FocusNavigationDirection.Down)
			throw new ComponentModel.InvalidEnumArgumentException(nameof(direction), (int)direction,
				typeof(FocusNavigationDirection));
		return FindFocusTarget(direction, false);
	}

	UIElement? FindFocusTarget(FocusNavigationDirection direction, bool wrapped)
	{
		var root = this;
		while (root.LogicalParent != null) root = root.LogicalParent;
		var order = new List<UIElement>();
		CollectFocusCandidates(root, order);
		order = TabOrderHelper.Order(order, e => KeyboardNavigation.GetTabIndex(e), e => true);

		int at = order.IndexOf(this);
		switch (direction)
		{
			case FocusNavigationDirection.First: return order.Count > 0 ? order[0] : null;
			case FocusNavigationDirection.Last: return order.Count > 0 ? order[order.Count - 1] : null;
			case FocusNavigationDirection.Next:
				return Step(order, at, 1, wrapped);
			case FocusNavigationDirection.Previous:
				return Step(order, at, -1, wrapped);
		}
		return NearestInDirection(order, direction)
			?? Step(order, at, direction is FocusNavigationDirection.Down or FocusNavigationDirection.Right ? 1 : -1,
				wrapped);
	}

	static UIElement? Step(List<UIElement> order, int at, int delta, bool wrapped)
	{
		if (order.Count == 0) return null;
		int next = at < 0 ? (delta > 0 ? 0 : order.Count - 1) : at + delta;
		if (next < 0 || next >= order.Count)
		{
			if (!wrapped) return null;
			next = (next + order.Count) % order.Count;
		}
		return order[next];
	}

	static void CollectFocusCandidates(DependencyObject node, List<UIElement> into)
	{
		if (node is UIElement e)
		{
			if (!e.IsVisibilityChainVisible) return;
			if (e is Controls.Control && e.Focusable && e.IsEnabled && KeyboardNavigation.GetIsTabStop(e))
				into.Add(e);
		}
		foreach (var child in LogicalTreeHelper.GetChildren(node))
		{
			if (child is DependencyObject d) CollectFocusCandidates(d, into);
		}
	}

	bool TryGetBounds(UIElement relativeTo, out double x, out double y, out double w, out double h)
	{
		x = y = w = h = 0;
		if (!IsWidgetCreated || !relativeTo.IsWidgetCreated) return false;
		if (!FocusWidget.ComputeBounds(relativeTo.Widget, out var b)) return false;
		x = b.X; y = b.Y; w = b.Width; h = b.Height;
		return w > 0 && h > 0;
	}

	UIElement? NearestInDirection(List<UIElement> order, FocusNavigationDirection direction)
	{
		var root = this;
		while (root.LogicalParent != null) root = root.LogicalParent;
		if (!TryGetBounds(root, out var sx, out var sy, out var sw, out var sh)) return null;
		double scx = sx + sw / 2, scy = sy + sh / 2;
		UIElement? best = null;
		double bestScore = double.MaxValue;
		foreach (var c in order)
		{
			if (ReferenceEquals(c, this) || !c.TryGetBounds(root, out var x, out var y, out var w, out var h)) continue;
			double dx = x + w / 2 - scx, dy = y + h / 2 - scy;
			bool inDirection = direction switch
			{
				FocusNavigationDirection.Left => dx < 0,
				FocusNavigationDirection.Right => dx > 0,
				FocusNavigationDirection.Up => dy < 0,
				_ => dy > 0,
			};
			if (!inDirection) continue;
			bool horizontal = direction is FocusNavigationDirection.Left or FocusNavigationDirection.Right;
			double along = horizontal ? Math.Abs(dx) : Math.Abs(dy);
			double across = horizontal ? Math.Abs(dy) : Math.Abs(dx);
			double score = along + 2 * across;
			if (score < bestScore) { bestScore = score; best = c; }
		}
		return best;
	}
}
