using System.Collections.Generic;
using System.Windows.Data;

namespace System.Windows;

/// <summary>
/// Data trigger: fires when a <see cref="Binding"/> expression evaluates to <see cref="Value"/>.
/// Supports single-segment and nested (dot-separated) binding paths via
/// <see cref="PathWatcher"/>, which re-evaluates the trigger whenever any segment
/// along the path raises <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>.
/// </summary>
public partial class DataTrigger : TriggerBase
{
	/// <summary>Binding that provides the value to compare.</summary>
	public Binding? Binding { get; set; }

	/// <summary>The value that activates this trigger.</summary>
	public object? Value { get; set; }

	/// <summary>Setters applied when the condition is active.</summary>
	public SetterBaseCollection Setters { get; } = new SetterBaseCollection();

	internal override void Seal() => Setters.IsSealed = true;

	/// <summary>Actions run when the condition becomes active.</summary>
	public TriggerActionCollection EnterActions { get; } = new TriggerActionCollection();

	/// <summary>Actions run when the condition stops being active.</summary>
	public TriggerActionCollection ExitActions { get; } = new TriggerActionCollection();

	readonly Dictionary<FrameworkElement, DataTriggerState> _states =
		new Dictionary<FrameworkElement, DataTriggerState>();

	/// <inheritdoc/>
	internal override void Attach(FrameworkElement target)
	{
		Setters.EnsureOnlySetters();
		if (Binding == null) return;
		var state = new DataTriggerState(this, target);
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
