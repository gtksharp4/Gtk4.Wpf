using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Windows.Data;

/// <summary>
/// Manages <see cref="MultiBinding"/> instances on target <see cref="FrameworkElement"/>s.
/// Resolves each child <see cref="Binding"/>, subscribes to source INPC, and calls the
/// <see cref="IMultiValueConverter"/> to push a combined value to the target DP.
/// <para>
/// TwoWay: write-back needs <see cref="MultiBinding.Converter"/> (its ConvertBack); without one
/// no target-change subscription is made. The resolved UpdateSourceTrigger decides when it runs.
/// </para>
/// </summary>
internal static partial class MultiBindingEngine
{
	static readonly ConditionalWeakTable<FrameworkElement, List<MultiRecord>> _records =
		new ConditionalWeakTable<FrameworkElement, List<MultiRecord>>();

	/// <summary>
	/// Registers <paramref name="mb"/> on <paramref name="dp"/> of <paramref name="target"/>
	/// and returns the live expression object.
	/// </summary>
	internal static MultiBindingExpression SetBinding(
		FrameworkElement target,
		DependencyProperty dp,
		MultiBinding mb)
	{
		var list = _records.GetOrCreateValue(target);

		int idx = list.FindIndex(r => r.TargetProperty == dp);
		if (idx >= 0)
		{
			list[idx].Deactivate();
			list.RemoveAt(idx);
		}

		var expr = new MultiBindingExpression(target, dp, mb);
		var record = new MultiRecord(target, dp, mb, expr);
		list.Add(record);
		record.Activate();
		return expr;
	}

	/// <summary>
	/// Removes and deactivates any existing multi-binding record for <paramref name="dp"/>
	/// on <paramref name="target"/>. Called by <see cref="BindingEngine.SetBinding"/> before
	/// installing a simple <see cref="Binding"/> over a prior <see cref="MultiBinding"/>.
	/// </summary>
	internal static void ClearForDp(FrameworkElement target, DependencyProperty dp)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		int idx = list.FindIndex(r => r.TargetProperty == dp);
		if (idx >= 0)
		{
			list[idx].Deactivate();
			list.RemoveAt(idx);
		}
	}

	/// <summary>Re-evaluates all multi-binding records on <paramref name="target"/> that use the DataContext.</summary>
	internal static void RefreshDataContextBindings(FrameworkElement target)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		foreach (var r in list)
			r.RefreshIfUsesDataContext(target);
	}

	/// <summary>Deactivates all multi-binding records on <paramref name="target"/>.</summary>
	internal static void ClearAll(FrameworkElement target)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		foreach (var r in list) r.Deactivate();
		list.Clear();
	}

	/// <summary>Re-evaluates the multi-binding for <paramref name="dp"/> on target.</summary>
	internal static void Refresh(FrameworkElement target, DependencyProperty dp)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		foreach (var r in list)
		{
			if (ReferenceEquals(r.TargetProperty, dp))
				r.Push(target);
		}
	}

	/// <summary>Writes the target value back through the multi-binding for <paramref name="dp"/>.</summary>
	internal static void UpdateSource(FrameworkElement target, DependencyProperty dp)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		foreach (var r in list)
		{
			if (ReferenceEquals(r.TargetProperty, dp))
				r.WriteBackNow(target);
		}
	}

	// ── Inner record ────────────────────────────────────────────────────────

	internal sealed partial class MultiRecord
	{
		readonly WeakReference<FrameworkElement> _targetRef;
		readonly DependencyProperty _dp;
		readonly MultiBinding _mb;

		// Per-child INPC subscriptions.
		readonly List<(INotifyPropertyChanged inpc, PropertyChangedEventHandler handler)> _subs =
			new List<(INotifyPropertyChanged, PropertyChangedEventHandler)>();

		// TwoWay: callback subscribed to target DP changes.
		PropertyChangedCallback? _targetChangedCallback;
		bool _pushing;

		internal DependencyProperty TargetProperty => _dp;

		internal MultiRecord(
			FrameworkElement target,
			DependencyProperty dp,
			MultiBinding mb,
			MultiBindingExpression expr)
		{
			_targetRef = new WeakReference<FrameworkElement>(target);
			_dp = dp;
			_mb = mb;
		}

		internal void Activate()
		{
			if (!_targetRef.TryGetTarget(out var target)) return;
			Detach();
			DetachTwoWay();
			SubscribeSources(target);
			Push(target);

			// TwoWay needs a converter's ConvertBack; the resolved trigger decides when it runs.
			var (mode, trigger) = BindingDefaults.Resolve(
				_mb.Mode, _mb.UpdateSourceTrigger, _dp, target.GetType());
			if (mode == BindingMode.TwoWay && _mb.Converter != null)
				AttachTwoWay(target, trigger);
		}

		internal void Deactivate()
		{
			DetachTwoWay();
			Detach();
		}

		/// <summary>
		/// Re-subscribes sources and re-pushes if any child binding uses DataContext.
		/// Called when the target's DataContext changes (R3).
		/// </summary>
		internal void RefreshIfUsesDataContext(FrameworkElement target)
		{
			bool any = false;
			foreach (var b in _mb.Bindings)
			{
				if (b.Source == null && b.ElementName == null && b.RelativeSource == null)
				{ any = true; break; }
			}
			if (!any) return;
			Detach();
			SubscribeSources(target);
			Push(target);
		}
	}
}