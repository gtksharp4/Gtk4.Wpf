using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace System.Windows;

/// <summary>
/// Tracks the roots between <c>BeginInit</c> and <c>EndInit</c> (the generated <c>InitializeComponent</c> brackets
/// the root) so events that WPF defers until initialization ends can queue their delivery per root.
/// </summary>
internal static class InitScope
{
	[ThreadStatic] static List<FrameworkElement>? _open;
	[ThreadStatic] static Dictionary<FrameworkElement, List<Action>>? _deferred;

	internal static bool Active => _open is { Count: > 0 };

	/// <summary>The innermost root currently between <c>BeginInit</c> and <c>EndInit</c>.</summary>
	internal static FrameworkElement? Current => _open is { Count: > 0 } ? _open[_open.Count - 1] : null;

	internal static bool IsOpen(FrameworkElement? root) => root != null && _open != null && _open.Contains(root);

	internal static void Enter(FrameworkElement root) => (_open ??= new List<FrameworkElement>()).Add(root);

	/// <summary>Drops all open scopes and queued actions (tests that abandon a BeginInit).</summary>
	internal static void Reset()
	{
		_open = null;
		_deferred = null;
	}

	/// <summary>Undoes <see cref="Enter"/> of a root whose initialization failed; drops its queued actions.</summary>
	internal static void Abandon(FrameworkElement root)
	{
		_open?.Remove(root);
		_deferred?.Remove(root);
	}

	/// <summary>
	/// Leaves the scope of <paramref name="root"/> and runs the actions queued for it, in order. Every action runs;
	/// the first exception is rethrown afterwards.
	/// </summary>
	internal static void Exit(FrameworkElement root)
	{
		_open?.Remove(root);
		if (_deferred == null || !_deferred.TryGetValue(root, out var queued)) return;
		_deferred.Remove(root);
		ExceptionDispatchInfo? first = null;
		foreach (var action in queued)
		{
			try { action(); }
			catch (Exception ex) { first ??= ExceptionDispatchInfo.Capture(ex); }
		}
		first?.Throw();
	}

	/// <summary>Queues <paramref name="action"/> for the end of <paramref name="root"/>'s initialization.</summary>
	internal static void Defer(FrameworkElement root, Action action)
	{
		_deferred ??= new Dictionary<FrameworkElement, List<Action>>();
		if (!_deferred.TryGetValue(root, out var list)) _deferred[root] = list = new List<Action>();
		list.Add(action);
	}
}
