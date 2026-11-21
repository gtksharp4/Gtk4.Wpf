using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace System.Windows.Data;

/// <summary>
/// Binding engine: OneWay, TwoWay, OneTime, OneWayToSource.
/// Round 7: backs each binding with a live <see cref="BindingExpression"/>;
/// adds RelativeSource resolution, StringFormat, FallbackValue, TargetNullValue,
/// and UpdateSourceTrigger (PropertyChanged / Explicit).
/// </summary>
internal static partial class BindingEngine
{
	// Per-element list of active records.
	static readonly ConditionalWeakTable<FrameworkElement, List<BindingRecord>> _bindings =
		new ConditionalWeakTable<FrameworkElement, List<BindingRecord>>();

	// Per-element map of DP → live BindingExpression, for GetBindingExpression.
	static readonly ConditionalWeakTable<FrameworkElement, Dictionary<DependencyProperty, BindingExpressionBase>> _expressions =
		new ConditionalWeakTable<FrameworkElement, Dictionary<DependencyProperty, BindingExpressionBase>>();

	/// <summary>
	/// Registers <paramref name="binding"/> on <paramref name="dp"/> of
	/// <paramref name="target"/> and applies it immediately.
	/// Returns the live <see cref="BindingExpressionBase"/>.
	/// </summary>
	internal static BindingExpressionBase? SetBinding(
		FrameworkElement target,
		DependencyProperty dp,
		BindingBase bindingBase)
	{
		var list = _bindings.GetOrCreateValue(target);

		// Clear any existing simple-binding record for this DP.
		int idx = list.FindIndex(r => r.TargetProperty == dp);
		if (idx >= 0)
		{
			list[idx].Deactivate();
			list.RemoveAt(idx);
		}

		// Also clear any existing multi-binding record for this DP (R2: cross-engine leak).
		MultiBindingEngine.ClearForDp(target, dp);

		// Clear any priority-binding record too.
		PriorityBindingEngine.ClearForDp(target, dp);

		// Remove old expression entry.
		var exprMap = _expressions.GetOrCreateValue(target);
		exprMap.Remove(dp);

		if (bindingBase is MultiBinding mb)
		{
			var mbExpr = MultiBindingEngine.SetBinding(target, dp, mb);
			exprMap[dp] = mbExpr;
			return mbExpr;
		}

		if (bindingBase is PriorityBinding pb)
		{
			var pbExpr = PriorityBindingEngine.SetBinding(target, dp, pb);
			exprMap[dp] = pbExpr;
			return pbExpr;
		}

		if (bindingBase is Binding binding)
		{
			var expr = new BindingExpression(target, dp, binding);
			var record = new BindingRecord(target, dp, binding, expr);
			expr.Record = record;
			list.Add(record);
			exprMap[dp] = expr;
			record.Activate();
			return expr;
		}

		return null;
	}

	/// <summary>Returns the live expression for <paramref name="dp"/>, or null.</summary>
	internal static BindingExpressionBase? GetBindingExpression(
		FrameworkElement target,
		DependencyProperty dp)
	{
		if (!_expressions.TryGetValue(target, out var map)) return null;
		map.TryGetValue(dp, out var expr);
		return expr;
	}

	// ── Inner record ────────────────────────────────────────────────────────

	internal sealed partial class BindingRecord
	{
		readonly WeakReference<FrameworkElement> _targetRef;
		readonly DependencyProperty _dp;
		readonly Binding _binding;
		readonly BindingExpression _expr;

		INotifyPropertyChanged? _inpcSource;
		string? _watchedProperty;
		bool _pushing;

		internal DependencyProperty TargetProperty => _dp;

		internal bool UsesDataContext =>
			_binding.Source == null
			&& _binding.ElementName == null
			&& _binding.RelativeSource == null;

		internal bool UsesTemplatedParent =>
			_binding.RelativeSource?.Mode == RelativeSourceMode.TemplatedParent;

		internal BindingRecord(
			FrameworkElement target,
			DependencyProperty dp,
			Binding binding,
			BindingExpression expr)
		{
			_targetRef = new WeakReference<FrameworkElement>(target);
			_dp = dp;
			_binding = binding;
			_expr = expr;
		}

		internal void Activate()
		{
			if (!_targetRef.TryGetTarget(out var target)) return;
			DetachInpc();

			var (mode, trigger) = BindingDefaults.Resolve(
				_binding.Mode, _binding.UpdateSourceTrigger, _dp, target.GetType());
			_trigger = trigger;
			_watchViews = mode != BindingMode.OneTime;
			if (mode == BindingMode.OneWayToSource)
			{
				SubscribeTargetChanges(target);
				WriteBack(target);
				return;
			}

			var source = ResolveSource(target);
			// PushToTarget handles FallbackValue / TargetNullValue internally (R5).
			PushToTarget(target, source);

			if (mode == BindingMode.OneTime) return;

			if (source is INotifyPropertyChanged inpc)
			{
				_inpcSource = inpc;
				_inpcSource.PropertyChanged += OnSourcePropertyChanged;
				_watchedProperty = FirstPropertyName(_binding.Path);
			}
			else
				AttachDpSource(source); // BindingEngine.SourceDp.cs

			if (mode == BindingMode.TwoWay
			    && _trigger != UpdateSourceTrigger.Explicit)
				SubscribeTargetChanges(target);
		}

		internal void Deactivate()
		{
			DetachInpc();
			UnsubscribeTargetChanges();
			DetachNotifyErrors();
			_stepErrors = new List<ValidationError>();
			if (_targetRef.TryGetTarget(out var target))
				Validation.SetErrorsFor(target, this, Array.Empty<ValidationError>(), _binding.NotifyOnValidationError);
			_published = false;
		}

		internal object? GetResolvedSource(FrameworkElement target) => ResolveSource(target);

		object? ResolveSource(FrameworkElement target) =>
			CollectionViewSource.UnwrapView(ResolveRawSource(target));

		object? ResolveRawSource(FrameworkElement target)
		{
			if (_binding.Source != null) return _binding.Source;
			if (_binding.ElementName != null)
				return target.FindName(_binding.ElementName) as FrameworkElement;
			if (_binding.RelativeSource != null)
				return _binding.RelativeSource.Resolve(target);
			return target.DataContext;
		}
	}
}
