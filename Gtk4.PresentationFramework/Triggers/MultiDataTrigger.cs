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
public partial class MultiDataTrigger : TriggerBase
{
	/// <summary>Binding-based conditions that must all hold for this trigger to fire.</summary>
	public ConditionCollection Conditions { get; } = new ConditionCollection();

	/// <summary>Setters applied when all conditions are active.</summary>
	public SetterBaseCollection Setters { get; } = new SetterBaseCollection();

	internal override void Seal() => Setters.IsSealed = true;

	/// <summary>Actions run when all conditions become active.</summary>
	public TriggerActionCollection EnterActions { get; } = new TriggerActionCollection();

	/// <summary>Actions run when any condition stops being active.</summary>
	public TriggerActionCollection ExitActions { get; } = new TriggerActionCollection();

	readonly Dictionary<FrameworkElement, MultiDataTriggerState> _states =
		new Dictionary<FrameworkElement, MultiDataTriggerState>();

	/// <inheritdoc/>
	internal override void Attach(FrameworkElement target)
	{
		Setters.EnsureOnlySetters();
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
}
