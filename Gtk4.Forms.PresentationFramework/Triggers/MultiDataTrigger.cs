using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace System.Windows;

/// <summary>
/// Multi-condition data trigger: applies <see cref="Setters"/> (and runs
/// <see cref="EnterActions"/> / <see cref="ExitActions"/>) only when ALL
/// binding-based <see cref="Conditions"/> hold simultaneously.
/// Each <see cref="Condition"/> carries a <see cref="Condition.Binding"/> and a
/// <see cref="Condition.Value"/>; the trigger fires when every resolved binding value
/// equals the corresponding condition value.
/// </summary>
public class MultiDataTrigger : TriggerBase
{
	/// <summary>Binding-based conditions that must all hold for this trigger to fire.</summary>
	public ConditionCollection Conditions { get; } = new ConditionCollection();

	/// <summary>Setters applied when all conditions are active.</summary>
	public SetterBaseCollection Setters { get; } = new SetterBaseCollection();

	/// <summary>Actions run when all conditions become active.</summary>
	public TriggerActionCollection EnterActions { get; } = new TriggerActionCollection();

	/// <summary>Actions run when any condition stops being active.</summary>
	public TriggerActionCollection ExitActions { get; } = new TriggerActionCollection();

	readonly Dictionary<FrameworkElement, MultiDataTriggerState> _states =
		new Dictionary<FrameworkElement, MultiDataTriggerState>();

	/// <inheritdoc/>
	internal override void Attach(FrameworkElement target)
	{
		var state = new MultiDataTriggerState(this, target);
		_states[target] = state;
		state.Evaluate();
	}

	/// <inheritdoc/>
	internal override void Detach(FrameworkElement target)
	{
		if (!_states.TryGetValue(target, out var state)) return;
		_states.Remove(target);
		state.Dispose();
	}

	// ── Per-target state ─────────────────────────────────────────────────────

	sealed class MultiDataTriggerState
	{
		readonly MultiDataTrigger _trigger;
		readonly FrameworkElement _target;
		List<DependencyProperty>? _appliedDps;

		// One ConditionWatch per condition.
		readonly List<ConditionWatch> _watches = new List<ConditionWatch>();
		PropertyChangedCallback? _dcHook;

		internal MultiDataTriggerState(MultiDataTrigger trigger, FrameworkElement target)
		{
			_trigger = trigger;
			_target = target;
			Subscribe();
		}

		void Subscribe()
		{
			_dcHook = OnDcChanged;
			_target.AddChangeHook(FrameworkElement.DataContextProperty, _dcHook);
			SubscribeSources();
		}

		void OnDcChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UnsubscribeSources();
			SubscribeSources();
			Evaluate();
		}

		void SubscribeSources()
		{
			foreach (var cond in _trigger.Conditions)
			{
				var watch = new ConditionWatch(cond, _target, Evaluate);
				_watches.Add(watch);
			}
		}

		void UnsubscribeSources()
		{
			foreach (var w in _watches) w.Dispose();
			_watches.Clear();
		}

		internal void Evaluate()
		{
			bool allMatch = AllConditionsMet();
			bool active = _appliedDps != null;
			if (allMatch && !active) Apply();
			else if (!allMatch && active) UnApply(runActions: true);
		}

		bool AllConditionsMet()
		{
			foreach (var cond in _trigger.Conditions)
			{
				var source = GetSource(cond);
				var current = BindingEngine.ResolvePath(source, cond.Binding?.Path);
				if (!Equals(current, cond.Value)) return false;
			}
			return true;
		}

		object? GetSource(Condition cond)
		{
			if (cond.Binding?.Source != null) return cond.Binding.Source;
			return _target.DataContext;
		}

		void Apply()
		{
			_appliedDps = new List<DependencyProperty>();
			foreach (var setter in _trigger.Setters)
				setter.ApplyToTriggerLayer(_target, this, _appliedDps);
			foreach (var action in _trigger.EnterActions)
				action.Invoke(_target);
		}

		/// <param name="runActions">
		/// <c>true</c> on a genuine active→inactive condition transition;
		/// <c>false</c> on style teardown — setters removed silently, no ExitActions.
		/// </param>
		void UnApply(bool runActions)
		{
			if (_appliedDps == null) return;
			var dps = _appliedDps;
			_appliedDps = null;
			_trigger.Setters.UnapplyTriggerLayer(_target, this);
			_target.ClearTriggerValues(dps, this);
			if (runActions)
			{
				foreach (var action in _trigger.ExitActions)
					action.Invoke(_target);
			}
		}

		internal void Dispose()
		{
			if (_dcHook != null)
				_target.RemoveChangeHook(FrameworkElement.DataContextProperty, _dcHook);
			UnsubscribeSources();
			UnApply(runActions: false);
		}
	}

	// ── Per-condition INPC watch ─────────────────────────────────────────────

	sealed class ConditionWatch
	{
		readonly Condition _cond;
		readonly FrameworkElement _target;
		readonly Action _onChange;
		INotifyPropertyChanged? _inpcSource;
		string? _watchedProp;

		internal ConditionWatch(Condition cond, FrameworkElement target, Action onChange)
		{
			_cond = cond;
			_target = target;
			_onChange = onChange;
			SubscribeSource(GetSource());
		}

		object? GetSource()
		{
			if (_cond.Binding?.Source != null) return _cond.Binding.Source;
			return _target.DataContext;
		}

		void SubscribeSource(object? source)
		{
			if (source is INotifyPropertyChanged inpc)
			{
				_inpcSource = inpc;
				var path = _cond.Binding?.Path;
				_watchedProp = string.IsNullOrEmpty(path) ? null : path!.Split('.')[0];
				_inpcSource.PropertyChanged += OnSourceChanged;
			}
		}

		void OnSourceChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (_watchedProp != null && e.PropertyName != null &&
			    e.PropertyName != _watchedProp) return;
			_onChange();
		}

		internal void Dispose()
		{
			if (_inpcSource != null)
			{
				_inpcSource.PropertyChanged -= OnSourceChanged;
				_inpcSource = null;
			}
		}
	}
}
