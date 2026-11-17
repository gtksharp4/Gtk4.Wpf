using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Base class for actions that can be run from an <see cref="EventTrigger"/>.
/// </summary>
public abstract class TriggerAction
{
	/// <summary>Invokes this action on <paramref name="target"/>.</summary>
	internal abstract void Invoke(FrameworkElement target);
}

/// <summary>Collection of <see cref="TriggerAction"/> items.</summary>
public class TriggerActionCollection : List<TriggerAction> { }

/// <summary>
/// Event trigger: when <see cref="RoutedEvent"/> fires on the target, runs <see cref="Actions"/>.
/// WPF restricts element-level triggers (FrameworkElement.Triggers) to EventTrigger only.
/// </summary>
public class EventTrigger : TriggerBase
{
	/// <summary>The routed event that fires this trigger.</summary>
	public RoutedEvent? RoutedEvent { get; set; }

	/// <summary>Actions to invoke when <see cref="RoutedEvent"/> fires.</summary>
	public TriggerActionCollection Actions { get; } = new TriggerActionCollection();

	readonly Dictionary<FrameworkElement, RoutedEventHandler> _handlers =
		new Dictionary<FrameworkElement, RoutedEventHandler>();

	/// <inheritdoc/>
	internal override void Attach(FrameworkElement target)
	{
		if (RoutedEvent == null) return;
		// Idempotent: a re-attach removes the prior handler so it is not orphaned
		// (double-attach would leave a dangling RoutedEventHandler subscription).
		if (_handlers.ContainsKey(target)) Detach(target);

		RoutedEventHandler handler = (s, e) => RunActions(target);
		_handlers[target] = handler;
		target.AddHandler(RoutedEvent, handler);
	}

	/// <inheritdoc/>
	internal override void Detach(FrameworkElement target)
	{
		if (RoutedEvent == null) return;
		if (!_handlers.TryGetValue(target, out var handler)) return;
		_handlers.Remove(target);
		target.RemoveHandler(RoutedEvent, handler);
	}

	void RunActions(FrameworkElement target)
	{
		foreach (var action in Actions)
			action.Invoke(target);
	}
}

/// <summary>
/// Minimal setter-style trigger action: sets a DP to a value on the target via the
/// trigger layer (D8) rather than the local layer, so it does not permanently shadow
/// style setters or other layer values (E6).
/// (Full BeginStoryboard is a later phase.)
/// </summary>
public class SetPropertyAction : TriggerAction
{
	/// <summary>The dependency property to set.</summary>
	public DependencyProperty? Property { get; set; }

	/// <summary>The value to assign.</summary>
	public object? Value { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Writes through the trigger layer (D8), keyed by this action instance, so the
	/// value does not permanently beat style setters the way a local value would.
	/// </remarks>
	internal override void Invoke(FrameworkElement target)
	{
		if (Property == null) return;
		// E6: use the trigger layer (D8), not SetValue (local layer).
		target.SetTriggerValue(Property, this, Value);
	}
}