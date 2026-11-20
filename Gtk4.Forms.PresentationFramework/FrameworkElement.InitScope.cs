using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Tracks the elements between <c>BeginInit</c> and <c>EndInit</c> (the generated
/// <c>InitializeComponent</c> brackets the root) so events that WPF defers until initialization
/// ends can queue their delivery.
/// </summary>
internal static class InitScope
{
	[ThreadStatic] static int _depth;
	[ThreadStatic] static List<Action>? _deferred;

	internal static bool Active => _depth > 0;

	internal static void Enter() => _depth++;

	/// <summary>Drops all open scopes and queued actions (tests that abandon a BeginInit).</summary>
	internal static void Reset()
	{
		_depth = 0;
		_deferred = null;
	}

	/// <summary>Leaves a scope; when the outermost one ends, runs the queued actions in order.</summary>
	internal static void Exit()
	{
		if (_depth > 0) _depth--;
		if (_depth > 0 || _deferred == null) return;
		var queued = _deferred;
		_deferred = null;
		foreach (var action in queued) action();
	}

	/// <summary>Queues <paramref name="action"/> for the end of the outermost initialization scope.</summary>
	internal static void Defer(Action action) => (_deferred ??= new List<Action>()).Add(action);
}
