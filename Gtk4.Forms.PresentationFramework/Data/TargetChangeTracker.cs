using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows.Data;

/// <summary>
/// Tracks per-DP write-back callbacks on a target <see cref="FrameworkElement"/>.
/// The partial class <c>FrameworkElement.Binding.cs</c> overrides
/// <c>OnPropertyChanged</c> and routes every DP change here; only entries whose
/// DP matches the changed property are invoked.
/// </summary>
internal static class TargetChangeTracker
{
	// Per-element list of (DP, callback) subscriptions.
	static readonly ConditionalWeakTable<FrameworkElement, List<(DependencyProperty dp, PropertyChangedCallback cb)>>
		_subs = new ConditionalWeakTable<FrameworkElement, List<(DependencyProperty, PropertyChangedCallback)>>();

	internal static void Subscribe(
		FrameworkElement target,
		DependencyProperty dp,
		PropertyChangedCallback callback)
	{
		var list = _subs.GetOrCreateValue(target);
		list.Add((dp, callback));
	}

	internal static void Unsubscribe(
		FrameworkElement target,
		DependencyProperty dp,
		PropertyChangedCallback callback)
	{
		if (!_subs.TryGetValue(target, out var list)) return;
		list.RemoveAll(e => ReferenceEquals(e.dp, dp) && ReferenceEquals(e.cb, callback));
	}

	/// <summary>
	/// Called by the <c>FrameworkElement</c> partial override of
	/// <see cref="DependencyObject.OnPropertyChanged"/> for every DP change.
	/// Invokes any registered callback whose DP matches.
	/// </summary>
	internal static void Dispatch(FrameworkElement target, DependencyPropertyChangedEventArgs e)
	{
		if (!_subs.TryGetValue(target, out var list)) return;
		// Snapshot to tolerate re-entrancy (a write-back may cause further changes).
		var snap = new List<(DependencyProperty dp, PropertyChangedCallback cb)>(list);
		foreach (var (dp, cb) in snap)
		{
			if (ReferenceEquals(dp, e.Property))
				cb(target, e);
		}
	}
}