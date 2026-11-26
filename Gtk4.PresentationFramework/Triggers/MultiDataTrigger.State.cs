using System.Collections.Generic;

namespace System.Windows;

public partial class MultiDataTrigger
{
	sealed class MultiDataTriggerState
	{
		readonly MultiDataTrigger _trigger;
		readonly FrameworkElement _target;
		readonly List<(Condition Condition, TriggerBindingSource Source)> _sources =
			new List<(Condition, TriggerBindingSource)>();
		List<DependencyProperty>? _appliedDps;

		internal MultiDataTriggerState(MultiDataTrigger trigger, FrameworkElement target)
		{
			_trigger = trigger;
			_target = target;
			foreach (var condition in trigger.Conditions)
				if (condition.Binding != null)
					_sources.Add((condition, new TriggerBindingSource(condition.Binding, target, Evaluate)));
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
			foreach (var (condition, source) in _sources)
				if (!TriggerValueComparer.AreEqual(source.Value, condition.Value)) return false;
			return _sources.Count == _trigger.Conditions.Count;
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
			foreach (var (_, source) in _sources) source.Dispose();
			_sources.Clear();
			UnApply(runActions: false);
		}
	}
}
