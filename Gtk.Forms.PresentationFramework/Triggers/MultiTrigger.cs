using System.Collections.Generic;

namespace System.Windows
{
	/// <summary>
	/// Multi-condition trigger: applies <see cref="Setters"/> only when ALL
	/// <see cref="Conditions"/> are satisfied simultaneously.
	/// </summary>
	public class MultiTrigger : TriggerBase
	{
		/// <summary>All conditions that must hold for the trigger to fire.</summary>
		public ConditionCollection Conditions { get; } = new ConditionCollection();

		/// <summary>Setters applied when all conditions are active.</summary>
		public SetterBaseCollection Setters { get; } = new SetterBaseCollection();

		readonly Dictionary<FrameworkElement, MultiTriggerState> _states =
			new Dictionary<FrameworkElement, MultiTriggerState>();

		/// <inheritdoc/>
		internal override void Attach(FrameworkElement target)
		{
			var state = new MultiTriggerState(this, target);
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

		sealed class MultiTriggerState
		{
			readonly MultiTrigger _trigger;
			readonly FrameworkElement _target;
			readonly List<(DependencyProperty dp, PropertyChangedCallback hook)> _hooks =
				new List<(DependencyProperty, PropertyChangedCallback)>();
			Dictionary<DependencyProperty, object?>? _applied;

			internal MultiTriggerState(MultiTrigger trigger, FrameworkElement target)
			{
				_trigger = trigger;
				_target = target;

				// Subscribe a hook for each condition's DP.
				foreach (var cond in trigger.Conditions)
				{
					var dp = cond.Resolve(target.GetType());
					if (dp == null) continue;
					PropertyChangedCallback hook = (d, e) => Evaluate();
					_target.AddChangeHook(dp, hook);
					_hooks.Add((dp, hook));
				}
			}

			internal void Evaluate()
			{
				bool allMatch = AllConditionsMet();
				bool active = _applied != null;
				if (allMatch && !active) Apply();
				else if (!allMatch && active) UnApply();
			}

			bool AllConditionsMet()
			{
				foreach (var cond in _trigger.Conditions)
				{
					var dp = cond.Resolve(_target.GetType());
					if (dp == null) return false;
					if (!Equals(_target.GetValue(dp), cond.Value)) return false;
				}
				return true;
			}

			void Apply()
			{
				_applied = new Dictionary<DependencyProperty, object?>();
				foreach (var setter in _trigger.Setters)
					setter.ApplyToTriggerLayer(_target, _applied);
			}

			void UnApply()
			{
				if (_applied == null) return;
				var snap = _applied;
				_applied = null;
				_target.ClearTriggerValues(snap);
			}

			internal void Dispose()
			{
				foreach (var (dp, hook) in _hooks)
					_target.RemoveChangeHook(dp, hook);
				UnApply();
			}
		}
	}

	/// <summary>Collection of <see cref="Condition"/> objects for <see cref="MultiTrigger"/>.</summary>
	public class ConditionCollection : List<Condition> { }
}
