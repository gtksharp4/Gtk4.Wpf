using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows;

/// <summary>Collection of <see cref="SetterBase"/> items.</summary>
public class SetterBaseCollection : Collection<SetterBase>
{
	bool _sealed;

	internal bool IsSealed
	{
		get => _sealed;
		set
		{
			_sealed = value;
			if (value) foreach (var setter in this) setter.Seal();
		}
	}

	void CheckSealed()
	{
		if (IsSealed) throw new InvalidOperationException("Cannot modify a sealed Style.");
	}

	/// <inheritdoc/>
	protected override void InsertItem(int index, SetterBase item)
	{
		CheckSealed();
		base.InsertItem(index, item);
	}

	/// <inheritdoc/>
	protected override void SetItem(int index, SetterBase item)
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

	/// <summary>Trigger setters can only be <see cref="Setter"/> objects, as in WPF.</summary>
	internal void EnsureOnlySetters()
	{
		foreach (var setter in this)
			if (setter is not Setter)
				throw new InvalidOperationException(
					"Trigger setters can only contain Setter objects; EventSetter is not supported.");
	}

	/// <summary>Un-applies the trigger-layer state every setter created for <paramref name="contributor"/>.</summary>
	internal void UnapplyTriggerLayer(DependencyObject target, object contributor)
	{
		foreach (var setter in this)
			setter.Unapply(target, contributor);
	}
}

/// <summary>Base class for all setter types.</summary>
public abstract class SetterBase
{
	/// <summary>Whether the setter is sealed (it belongs to a sealed style) and can no longer be modified.</summary>
	public bool IsSealed { get; private set; }

	internal void Seal() => IsSealed = true;

	/// <summary>Throws when the setter is sealed.</summary>
	protected void CheckSealed()
	{
		if (IsSealed) throw new InvalidOperationException("Cannot modify a sealed Style.");
	}

	/// <summary>Applies this setter to <paramref name="target"/> as a local value (direct apply).</summary>
	internal abstract void Apply(DependencyObject target);

	/// <summary>Applies this setter into the style-value layer of <paramref name="target"/> (D6).</summary>
	internal abstract void ApplyToStyleLayer(DependencyObject target);

	/// <summary>
	/// D8: Applies this setter into the trigger-value layer of <paramref name="target"/>,
	/// keyed by <paramref name="contributor"/> (the trigger-state instance).
	/// Appends the resolved DP to <paramref name="appliedDps"/> for un-apply bookkeeping.
	/// </summary>
	internal abstract void ApplyToTriggerLayer(
		DependencyObject target,
		object contributor,
		List<DependencyProperty> appliedDps);

	/// <summary>
	/// Removes live state (resource links, bindings, handlers) this setter attached to
	/// <paramref name="target"/>; <paramref name="contributor"/> is the trigger state, or null for the style.
	/// </summary>
	internal virtual void Unapply(DependencyObject target, object? contributor) { }
}
