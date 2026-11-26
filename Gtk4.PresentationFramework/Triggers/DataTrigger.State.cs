using System.Collections.Generic;

namespace System.Windows;

public partial class DataTrigger
{
	/// <summary>
	/// Per-element live state for a <see cref="DataTrigger"/>: a <see cref="TriggerBindingSource"/> reports every
	/// change of the bound value and the condition is re-evaluated each time.
	/// </summary>
	sealed class DataTriggerState
	{
		readonly DataTrigger _trigger;
		readonly FrameworkElement _target;
		readonly TriggerBindingSource _source;
		List<DependencyProperty>? _appliedDps;

		internal DataTriggerState(DataTrigger trigger, FrameworkElement target)
		{
			_trigger = trigger;
			_target = target;
			_source = new TriggerBindingSource(trigger.Binding!, target, Evaluate);
		}

		internal void Evaluate()
		{
			bool match = TriggerValueComparer.AreEqual(_source.Value, _trigger.Value);
			bool active = _appliedDps != null;
			if (match && !active) Apply();
			else if (!match && active) UnApply(runActions: true);
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
		/// <c>true</c> on a genuine active→inactive transition;
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
			_source.Dispose();
			UnApply(runActions: false);
		}
	}
}
