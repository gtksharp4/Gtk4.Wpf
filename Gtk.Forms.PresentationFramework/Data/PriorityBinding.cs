using System.Collections.Generic;
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

	internal static void ClearForDp(FrameworkElement target, DependencyProperty dp)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		int idx = list.FindIndex(r => r.TargetProperty == dp);
		if (idx >= 0) { list[idx].Deactivate(); list.RemoveAt(idx); }
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
			foreach (var b in _pb.Bindings)
			{
				var src = ResolveSource(target, b);
				var value = BindingEngine.ResolvePath(src, b.Path);
				if (value != null && !ReferenceEquals(value, DependencyProperty.UnsetValue))
				{
					target.SetValue(_dp, value);
					return;
				}
			}
			// No child resolved — apply FallbackValue.
			var fb = _pb.FallbackValue;
			if (!ReferenceEquals(fb, DependencyProperty.UnsetValue))
				target.SetValue(_dp, fb);
		}

		void SubscribeSources(FrameworkElement target)
		{
			foreach (var b in _pb.Bindings)
			{
				var src = ResolveSource(target, b);
				if (src is INotifyPropertyChanged inpc)
				{
					PropertyChangedEventHandler handler = (s, e) =>
					{
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

		static object? ResolveSource(FrameworkElement target, Binding b)
		{
			if (b.Source != null) return b.Source;
			if (b.ElementName != null) return target.FindName(b.ElementName) as FrameworkElement;
			if (b.RelativeSource != null) return b.RelativeSource.Resolve(target);
			return target.DataContext;
		}
	}
}
