using System.Collections.Generic;

namespace System.Windows
{
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

		// Per-target state so the same Trigger object can attach to multiple elements.
		readonly Dictionary<FrameworkElement, TriggerState> _states =
			new Dictionary<FrameworkElement, TriggerState>();

		/// <inheritdoc/>
		internal override void Attach(FrameworkElement target)
		{
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
			Dictionary<DependencyProperty, object?>? _applied;

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
				bool active = _applied != null;
				if (match && !active) Apply();
				else if (!match && active) UnApply();
			}

			bool IsMatch(object? current) =>
				Equals(current, _trigger.Value);

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
				_target.RemoveChangeHook(_dp, _hook);
				UnApply();
			}
		}
	}
}
