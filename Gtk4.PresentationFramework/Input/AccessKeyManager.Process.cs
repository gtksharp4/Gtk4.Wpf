using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Input;

internal enum ProcessKeyResult
{
	NoMatch,
	LastMatch,
	MoreMatches,
}

public sealed partial class AccessKeyManager
{
	/// <summary>
	/// Invokes the element registered for <paramref name="key"/> in <paramref name="scope"/>. With several
	/// candidates the enabled one after the keyboard-focused one wins and <c>IsMultiple</c> is set, so a
	/// button only takes focus. Returns whether more elements share the key (WPF's <c>MoreMatches</c>).
	/// </summary>
	public static bool ProcessKey(object scope, string key, bool isMultiple)
		=> ProcessKeyForScope(scope, key, isMultiple) == ProcessKeyResult.MoreMatches;

	/// <summary>
	/// The body of <c>ProcessKey</c>. Only candidates whose AccessKeyPressed scope equals the scope of the
	/// <paramref name="sender"/> (the focused element; open submenus narrow it) take part.
	/// </summary>
	internal static ProcessKeyResult ProcessKeyForScope(
		object scope, string key, bool isMultiple, UIElement? sender = null)
	{
		key = Normalize(key);
		object? senderScope = sender == null ? scope : InfoFor(sender, key, scope).Scope;
		var targets = new List<UIElement>();
		foreach (var element in Alive(key))
		{
			if (!IsTargetable(element) || !ReferenceEquals(ScopeOf(element), scope)) continue;
			var (target, candidateScope) = InfoFor(element, key, scope);
			if (target == null || !IsTargetable(target) || targets.Contains(target)) continue;
			if (!ReferenceEquals(candidateScope, senderScope)) continue;
			targets.Add(target);
		}
		if (targets.Count == 0) return ProcessKeyResult.NoMatch;

		int focused = targets.FindIndex(t => t.IsKeyboardFocused);
		var pick = targets[targets.Count == 1 ? 0 : (focused + 1) % targets.Count];
		pick.InvokeAccessKey(new AccessKeyEventArgs(key, isMultiple || targets.Count > 1, true));
		return targets.Count > 1 ? ProcessKeyResult.MoreMatches : ProcessKeyResult.LastMatch;
	}

	// The target an element names and the scope it sits in (an element's own handlers, else the window).
	static (UIElement? Target, object? Scope) InfoFor(UIElement element, string key, object window)
	{
		var args = new AccessKeyPressedEventArgs(key);
		element.RaiseEvent(args);
		return (args.Target, args.Scope ?? window);
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
}
