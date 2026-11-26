using System.Collections.ObjectModel;
using System.Windows.Data;

namespace System.Windows;

/// <summary>
/// Abstract base for all trigger types (<see cref="Trigger"/>, <see cref="DataTrigger"/>,
/// <see cref="MultiTrigger"/>, <see cref="EventTrigger"/>).
/// </summary>
public abstract class TriggerBase
{
	/// <summary>
	/// Subscribes this trigger to the given <paramref name="target"/>.
	/// Called when a <see cref="Style"/> is applied or when the element is loaded.
	/// </summary>
	internal abstract void Attach(FrameworkElement target);

	/// <summary>
	/// Removes subscriptions and un-applies any active setters on <paramref name="target"/>.
	/// Called when a <see cref="Style"/> is replaced/nulled or the element is unloaded.
	/// </summary>
	internal abstract void Detach(FrameworkElement target);

	/// <summary>Seals the setters this trigger owns (called when its style is sealed).</summary>
	internal virtual void Seal() { }
}

/// <summary>Collection of <see cref="TriggerBase"/> items.</summary>
public class TriggerCollection : Collection<TriggerBase>
{
	bool _sealed;

	internal bool IsSealed
	{
		get => _sealed;
		set
		{
			_sealed = value;
			if (value) foreach (var trigger in this) trigger.Seal();
		}
	}

	void CheckSealed()
	{
		if (IsSealed) throw new InvalidOperationException("Cannot modify a sealed Style.");
	}

	/// <inheritdoc/>
	protected override void InsertItem(int index, TriggerBase item)
	{
		CheckSealed();
		base.InsertItem(index, item);
	}

	/// <inheritdoc/>
	protected override void SetItem(int index, TriggerBase item)
	{
		CheckSealed();
		base.SetItem(index, item);
	}

	/// <inheritdoc/>
	protected override void RemoveItem(int index)
	{
		CheckSealed();
		base.RemoveItem(index);
	}

	/// <inheritdoc/>
	protected override void ClearItems()
	{
		CheckSealed();
		base.ClearItems();
	}
}

/// <summary>
/// A single property=value setter used inside a <see cref="Trigger"/>,
/// <see cref="DataTrigger"/>, or <see cref="MultiTrigger"/>.
/// Reuses <see cref="Setter"/> (resolved DP or name) for consistency.
/// </summary>
public class Condition
{
	/// <summary>The dependency property to compare (used by <see cref="MultiTrigger"/>).</summary>
	public DependencyProperty? Property { get; set; }

	/// <summary>String property name, resolved at attach time when <see cref="Property"/> is null.</summary>
	public string? PropertyName { get; set; }

	/// <summary>
	/// Binding expression whose resolved value is compared to <see cref="Value"/>
	/// (used by <see cref="MultiDataTrigger"/>).
	/// </summary>
	public Binding? Binding { get; set; }

	/// <summary>The value to match (string or typed).</summary>
	public object? Value { get; set; }

	/// <summary>Resolves the DP, consulting <paramref name="targetType"/> for name resolution.</summary>
	internal DependencyProperty? Resolve(Type targetType)
	{
		if (Property != null) return Property;
		if (PropertyName != null)
			return DependencyProperty.FindByName(PropertyName, targetType);
		return null;
	}
}