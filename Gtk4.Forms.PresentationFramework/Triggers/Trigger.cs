using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Property trigger: when <see cref="Property"/> equals <see cref="Value"/> on the target,
/// applies <see cref="Setters"/> into the trigger-value layer (D8 — above style, below local).
/// Un-applies when the condition is no longer met.
/// </summary>
public class Trigger : TriggerBase
{
	/// <summary>The dependency property to watch.</summary>
	public DependencyProperty? Property { get; set; }

	/// <summary>String property name, resolved at attach time when <see cref="Property"/> is null.</summary>
	public string? PropertyName { get; set; }

	/// <summary>The value that activates this trigger.</summary>
	public object? Value { get; set; }

	/// <summary>Setters applied when the condition is active.</summary>
	public SetterBaseCollection Setters { get; } = new SetterBaseCollection();

	internal override void Seal() => Setters.IsSealed = true;

	/// <summary>Actions run when the condition becomes active.</summary>
	public TriggerActionCollection EnterActions { get; } = new TriggerActionCollection();

	/// <summary>Actions run when the condition stops being active.</summary>
	public TriggerActionCollection ExitActions { get; } = new TriggerActionCollection();

	// Per-target state so the same Trigger object can attach to multiple elements.
	readonly Dictionary<FrameworkElement, TriggerState> _states =
		new Dictionary<FrameworkElement, TriggerState>();

	/// <inheritdoc/>
	internal override void Attach(FrameworkElement target)
	{
		Setters.EnsureOnlySetters();
		// Idempotent: a re-attach replaces the prior state so its change hook is
		// removed instead of orphaned (double-attach would leak a TriggerState).
		if (_states.ContainsKey(target)) Detach(target);

		var dp = ResolveProperty(target);
		if (dp == null) return;

		var state = new TriggerState(this, target, dp);
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

	DependencyProperty? ResolveProperty(FrameworkElement target)
	{
		if (Property != null) return Property;
		if (PropertyName != null)
			return DependencyProperty.FindByName(PropertyName, target.GetType());
		return null;
	}

	sealed class TriggerState
	{
		readonly Trigger _trigger;
		readonly FrameworkElement _target;
		readonly DependencyProperty _dp;
		// E1: use this TriggerState instance as the contributor key.
		List<DependencyProperty>? _appliedDps;

		readonly PropertyChangedCallback _hook;

		internal TriggerState(Trigger trigger, FrameworkElement target, DependencyProperty dp)
		{
			_trigger = trigger;
			_target = target;
			_dp = dp;
			_hook = OnPropertyChanged;
			_target.AddChangeHook(_dp, _hook);
		}

		void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
			Evaluate();

		internal void Evaluate()
		{
			bool match = IsMatch(_target.GetValue(_dp));
			bool active = _appliedDps != null;
			if (match && !active) Apply();
			else if (!match && active) UnApply(runActions: true);
		}

		bool IsMatch(object? current) =>
			Equals(current, _trigger.Value);

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
		/// <c>false</c> on style teardown (Dispose/Detach) — setters are removed
		/// silently without running ExitActions (mirrors WPF behaviour).
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
			_target.RemoveChangeHook(_dp, _hook);
			UnApply(runActions: false);
		}
	}
}