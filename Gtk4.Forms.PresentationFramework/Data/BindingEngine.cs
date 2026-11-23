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
	static readonly ConditionalWeakTable<DependencyObject, List<BindingRecord>> _bindings =
		new ConditionalWeakTable<DependencyObject, List<BindingRecord>>();

	// Per-element map of DP → live BindingExpression, for GetBindingExpression.
	static readonly ConditionalWeakTable<DependencyObject,
		Dictionary<DependencyProperty, BindingExpressionBase>> _expressions =
		new ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, BindingExpressionBase>>();

	/// <summary>
	/// Registers <paramref name="binding"/> on <paramref name="dp"/> of
	/// <paramref name="target"/> and applies it immediately.
	/// Returns the live <see cref="BindingExpressionBase"/>.
	/// </summary>
	internal static BindingExpressionBase? SetBinding(
		DependencyObject target,
		DependencyProperty dp,
		BindingBase bindingBase)
	{
		var fe = target as FrameworkElement;
		if (fe == null && bindingBase is not Binding)
			throw new NotSupportedException("Only Binding is supported on a content element.");

		var list = _bindings.GetOrCreateValue(target);

		// Clear any existing simple-binding record for this DP.
		int idx = list.FindIndex(r => r.TargetProperty == dp);
		if (idx >= 0)
		{
			list[idx].Deactivate();
			list.RemoveAt(idx);
		}

		// Also clear any existing multi-binding record for this DP (R2: cross-engine leak).
		if (fe != null) MultiBindingEngine.ClearForDp(fe, dp);

		// Clear any priority-binding record too.
		if (fe != null) PriorityBindingEngine.ClearForDp(fe, dp);

		// Remove old expression entry.
		var exprMap = _expressions.GetOrCreateValue(target);
		if (fe != null && exprMap.TryGetValue(dp, out var replaced)) BindingGroupEnlister.Withdraw(fe, replaced);
		exprMap.Remove(dp);

		if (bindingBase is MultiBinding mb)
		{
			var mbExpr = MultiBindingEngine.SetBinding(fe!, dp, mb);
			exprMap[dp] = mbExpr;
			return mbExpr;
		}

		if (bindingBase is PriorityBinding pb)
		{
			var pbExpr = PriorityBindingEngine.SetBinding(fe!, dp, pb);
			exprMap[dp] = pbExpr;
			return pbExpr;
		}

		if (bindingBase is Binding binding)
		{
			var expr = new BindingExpression(target, dp, binding);
			var record = new BindingRecord(target, dp, binding, expr);
			expr.Record = record;
			list.Add(record);
			if (binding.ElementName != null) TrackElementName(record);
			exprMap[dp] = expr;
			record.Activate();
			if (fe != null) BindingGroupEnlister.Enlist(fe, expr);
			return expr;
		}

		return null;
	}

	/// <summary>Returns the live expression for <paramref name="dp"/>, or null.</summary>
	internal static BindingExpressionBase? GetBindingExpression(
		DependencyObject target,
		DependencyProperty dp)
	{
		if (!_expressions.TryGetValue(target, out var map)) return null;
		map.TryGetValue(dp, out var expr);
		return expr;
	}

	// ── Inner record ────────────────────────────────────────────────────────

	internal sealed partial class BindingRecord
	{
		readonly WeakReference<DependencyObject> _targetRef;
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
			DependencyObject target,
			DependencyProperty dp,
			Binding binding,
			BindingExpression expr)
		{
			_targetRef = new WeakReference<DependencyObject>(target);
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
				_watchedProperty = FirstPropertyName(PropertyPath.ResolveOwner(source, _binding.PathString));
			}
			else
				AttachDpSource(source); // BindingEngine.SourceDp.cs

			if (mode == BindingMode.TwoWay
			    && _trigger != UpdateSourceTrigger.Explicit)
				SubscribeTargetChanges(target);
		}

		internal void Deactivate()
		{
			UntrackElementName(this);
			DetachInpc();
			UnsubscribeTargetChanges();
			DetachNotifyErrors();
			_stepErrors = new List<ValidationError>();
			if (_targetRef.TryGetTarget(out var target))
				Validation.SetErrorsFor(target, this, Array.Empty<ValidationError>(), _binding.NotifyOnValidationError);
			_published = false;
		}

		internal object? GetResolvedSource(DependencyObject target) => ResolveSource(target);

		object? ResolveSource(DependencyObject target) =>
			CollectionViewSource.UnwrapView(ResolveRawSource(target));

		object? ResolveRawSource(DependencyObject target)
		{
			if (_binding.Source != null) return _binding.Source;
			if (_binding.ElementName != null)
				return _elementSource = BindingTarget.FindName(target, _binding.ElementName);
			if (_binding.RelativeSource != null)
				return BindingTarget.ResolveRelative(target, _binding.RelativeSource);
			return BindingTarget.DataContextOf(target);
		}
	}
}
