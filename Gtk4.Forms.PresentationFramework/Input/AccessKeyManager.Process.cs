using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Input;

public sealed partial class AccessKeyManager
{
	/// <summary>
	/// Invokes the element registered for <paramref name="key"/> in <paramref name="scope"/>. With several
	/// candidates the enabled one after the keyboard-focused one wins and <c>IsMultiple</c> is set, so a
	/// button only takes focus. Returns whether an element was invoked.
	/// </summary>
	public static bool ProcessKey(object scope, string key, bool isMultiple)
	{
		key = Normalize(key);
		var targets = new List<UIElement>();
		var scopes = new List<UIElement>();
		bool existsElsewhere = false;
		foreach (var element in Alive(key))
		{
			if (!IsTargetable(element)) continue;
			if (!ReferenceEquals(ScopeOf(element), scope))
			{
				existsElsewhere = true;
				continue;
			}
			var args = new AccessKeyPressedEventArgs(key);
			element.RaiseEvent(args);
			if (args.Target != null)
			{
				if (IsTargetable(args.Target) && !targets.Contains(args.Target)) targets.Add(args.Target);
			}
			else if (args.Scope is UIElement scopeElement)
				scopes.Add(scopeElement);
		}
		if (scopes.Count > 0)
			targets.RemoveAll(t => !IsInside(t, scopes[scopes.Count - 1]));
		if (targets.Count == 0) return false;

		int focused = targets.FindIndex(t => t.IsKeyboardFocused);
		var pick = targets[targets.Count == 1 ? 0 : (focused + 1) % targets.Count];
		pick.InvokeAccessKey(new AccessKeyEventArgs(key, isMultiple || targets.Count > 1 || existsElsewhere, true));
		return true;
	}

	/// <summary>
	/// The key whose AccessKeyPressed target is <paramref name="target"/> (WPF's
	/// <c>InternalGetAccessKeyCharacter</c>), or null.
	/// </summary>
	internal static string? GetKeyForTarget(UIElement target)
	{
		List<string> keys;
		lock (_lock)
			keys = new List<string>(_keys.Keys);
		foreach (var key in keys)
		{
			foreach (var element in Alive(key))
			{
				var args = new AccessKeyPressedEventArgs(key);
				element.RaiseEvent(args);
				if (ReferenceEquals(args.Target, target)) return key;
			}
		}
		return null;
	}

	internal static object? ScopeOf(UIElement element) => Window.GetWindow(element);

	// Visible and enabled, and not inside a closed submenu or an unselected tab page (WPF has no such elements).
	internal static bool IsTargetable(UIElement element)
	{
		if (!element.IsEnabled || !element.IsVisible) return false;
		for (var p = element.LogicalParent; p != null; p = p.LogicalParent)
			if (p is MenuItem { IsSubmenuOpen: false } || p is TabItem { IsSelected: false }) return false;
		return true;
	}

	static bool IsInside(UIElement element, UIElement ancestor)
	{
		for (var p = element.LogicalParent; p != null; p = p.LogicalParent)
			if (ReferenceEquals(p, ancestor)) return true;
		return false;
	}
}
