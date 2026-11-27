using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Data;

/// <summary>
/// Tries each child <see cref="Binding"/> in order and uses the first one that
/// produces a valid (non-<c>null</c>, non-<see cref="DependencyProperty.UnsetValue"/>)
/// value.  If no child resolves, <see cref="BindingBase.FallbackValue"/> is used.
/// Re-evaluates when any active child source raises
/// <see cref="INotifyPropertyChanged.PropertyChanged"/>.
/// </summary>
public class PriorityBinding : BindingBase
{
	/// <summary>Ordered list of child bindings.</summary>
	public IList<Binding> Bindings { get; } = new List<Binding>();
}

// ── Engine ──────────────────────────────────────────────────────────────────

internal static class PriorityBindingEngine
{
	static readonly ConditionalWeakTable<FrameworkElement, List<PriorityRecord>> _records =
		new ConditionalWeakTable<FrameworkElement, List<PriorityRecord>>();

	internal static PriorityBindingExpression SetBinding(
		FrameworkElement target,
		DependencyProperty dp,
		PriorityBinding pb)
	{
		var list = _records.GetOrCreateValue(target);
		int idx = list.FindIndex(r => r.TargetProperty == dp);
		if (idx >= 0) { list[idx].Deactivate(); list.RemoveAt(idx); }

		var expr = new PriorityBindingExpression(target, dp, pb);
		var record = new PriorityRecord(target, dp, pb, expr);
		list.Add(record);
		record.Activate();
		return expr;
	}

	/// <summary>
	/// Re-evaluates the records whose child bindings read the DataContext, so a DataContext inherited or
	/// set after the binding was created is picked up (WPF re-resolves child sources on a DataContext change).
	/// </summary>
	internal static void RefreshDataContextBindings(FrameworkElement target)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		foreach (var r in list.ToArray())
			if (r.UsesDataContext) r.Activate();
	}

	internal static void ClearForDp(FrameworkElement target, DependencyProperty dp)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		int idx = list.FindIndex(r => r.TargetProperty == dp);
		if (idx >= 0) { list[idx].Deactivate(); list.RemoveAt(idx); }
	}

	/// <summary>
	/// Deactivates all priority-binding records on <paramref name="target"/>,
	/// detaching their INPC subscriptions.  Called by
	/// <see cref="BindingEngine.ClearAllBindings"/>.
	/// </summary>
	internal static void ClearAll(FrameworkElement target)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		foreach (var r in list) r.Deactivate();
		list.Clear();
	}

	// ── Record ───────────────────────────────────────────────────────────────

	internal sealed class PriorityRecord
	{
		readonly WeakReference<FrameworkElement> _targetRef;
		readonly DependencyProperty _dp;
		readonly PriorityBinding _pb;

		readonly List<(INotifyPropertyChanged inpc, PropertyChangedEventHandler handler)> _subs =
			new List<(INotifyPropertyChanged, PropertyChangedEventHandler)>();

		internal DependencyProperty TargetProperty => _dp;

		internal bool UsesDataContext => _pb.Bindings.Any(
			b => b.Source == null && b.ElementName == null && b.RelativeSource == null);

		internal PriorityRecord(
			FrameworkElement target,
			DependencyProperty dp,
			PriorityBinding pb,
			PriorityBindingExpression expr)
		{
			_targetRef = new WeakReference<FrameworkElement>(target);
			_dp = dp;
			_pb = pb;
		}

		internal void Activate()
		{
			if (!_targetRef.TryGetTarget(out var target)) return;
			Detach();
			SubscribeSources(target);
			Push(target);
		}

		internal void Deactivate() => Detach();

		internal void Push(FrameworkElement target)
		{
			// Re-subscribe sources so a source that changed identity (e.g. DataContext swap)
			// gets its new INPC wired up before we evaluate the chain.
			Detach();
			SubscribeSources(target);

			foreach (var b in _pb.Bindings)
			{
				var src = ResolveSource(target, b);
				var value = BindingEngine.ResolvePath(src, b.PathString);
				if (value != null && !ReferenceEquals(value, DependencyProperty.UnsetValue))
				{
					BindingEngine.SetTargetConverted(target, _dp, value, _pb.FallbackValue);
					return;
				}
			}
			// No child resolved — apply FallbackValue.
			var fb = _pb.FallbackValue;
			if (!ReferenceEquals(fb, DependencyProperty.UnsetValue))
				BindingEngine.SetFallbackOrDefault(target, _dp, fb);
		}

		void SubscribeSources(FrameworkElement target)
		{
			foreach (var b in _pb.Bindings)
			{
				var src = ResolveSource(target, b);
				if (src is INotifyPropertyChanged inpc)
				{
					// Filter by first path segment so only the relevant property
					// triggers a re-push (mirrors BindingRecord / DataTriggerState).
					var watchPath = PropertyPath.ResolveOwner(src, b.PathString);
					string? watchedProp = string.IsNullOrEmpty(watchPath) ? null : watchPath!.Split('.')[0];

					PropertyChangedEventHandler handler = (s, e) =>
					{
						if (watchedProp != null
						    && e.PropertyName != null
						    && e.PropertyName != watchedProp) return;
						if (_targetRef.TryGetTarget(out var t)) Push(t);
					};
					inpc.PropertyChanged += handler;
					_subs.Add((inpc, handler));
				}
			}
		}

		void Detach()
		{
			foreach (var (inpc, handler) in _subs)
				inpc.PropertyChanged -= handler;
			_subs.Clear();
		}

		static object? ResolveSource(FrameworkElement target, Binding b) =>
			CollectionViewSource.UnwrapView(RawResolveSource(target, b));

		static object? RawResolveSource(FrameworkElement target, Binding b)
		{
			if (b.Source != null) return b.Source;
			if (b.ElementName != null) return target.FindName(b.ElementName) as FrameworkElement;
			if (b.RelativeSource != null) return b.RelativeSource.Resolve(target);
			return target.DataContext;
		}
	}
}
