using System;

namespace System.Windows;

/// <summary>
/// Base class for objects that have a modifiable and a frozen (read-only) state.
/// Mirrors <c>System.Windows.Freezable</c>.
/// </summary>
/// <remarks>
/// A frozen <see cref="Freezable"/> rejects all mutations with
/// <see cref="InvalidOperationException"/>. Cloning always produces an unfrozen copy.
/// </remarks>
public abstract partial class Freezable : DependencyObject
{
	bool _isFrozen;

	// ── Freeze / state ───────────────────────────────────────────────────────────

	/// <summary>Gets whether this instance can currently be frozen.</summary>
	public virtual bool CanFreeze => !_isFrozen && !BlockedByBindingOrChild();

	/// <summary>Gets whether this instance is frozen (read-only).</summary>
	public bool IsFrozen => _isFrozen;

	/// <summary>
	/// Freezes this instance. After this call all mutation attempts throw
	/// <see cref="InvalidOperationException"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Thrown when <see cref="CanFreeze"/> is <c>false</c>.
	/// </exception>
	public void Freeze()
	{
		if (_isFrozen) return;
		if (!CanFreeze)
			throw new InvalidOperationException("Cannot freeze this Freezable instance.");
		OnFreeze();
		_isFrozen = true;
	}

	/// <summary>
	/// Called by <see cref="Freeze"/> just before the frozen flag is set.
	/// Override to freeze subordinate objects.
	/// </summary>
	protected virtual void OnFreeze() { }

	// ── Change notification ──────────────────────────────────────────────────────

	/// <summary>Fired when this unfrozen instance is mutated.</summary>
	public event EventHandler? Changed;

	/// <summary>
	/// Call from subclasses whenever a property that is NOT a
	/// <see cref="DependencyProperty"/> changes.  Throws if frozen.
	/// </summary>
	protected void OnChanged()
	{
		ThrowIfFrozen();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Called by the DP system when a DP value on this Freezable changes.
	/// Fires <see cref="Changed"/> if the instance is not frozen.
	/// Override to react to child Freezable changes.
	/// </summary>
	protected virtual void OnFreezablePropertyChanged(
		DependencyObject? oldValue, DependencyObject? newValue) { }

	// ── Mutation guard ───────────────────────────────────────────────────────────

	/// <summary>
	/// Called by <see cref="DependencyObject.SetValueCore"/> before writing a new local
	/// value. <see cref="Freezable"/> overrides this to throw when frozen, preventing
	/// any DP write from succeeding.
	/// </summary>
	protected override void OnBeforeSetValue(DependencyProperty dp) => ThrowIfFrozen();

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (!_isFrozen)
			Changed?.Invoke(this, EventArgs.Empty);
	}

	// ── Cloning ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Creates a deep, unfrozen clone of this instance with all DP values
	/// as of the time of the call.
	/// </summary>
	public Freezable Clone() => CloneCore(currentValueOnly: false);

	/// <summary>
	/// Creates a deep, unfrozen clone of this instance using the current
	/// animated values (same as <see cref="Clone"/> for non-animated objects).
	/// </summary>
	public Freezable CloneCurrentValue() => CloneCore(currentValueOnly: true);

	/// <summary>
	/// Creates a frozen deep clone of this instance.
	/// </summary>
	public Freezable GetAsFrozen()
	{
		var clone = CloneCore(currentValueOnly: false);
		clone.Freeze();
		return clone;
	}

	Freezable CloneCore(bool currentValueOnly)
	{
		var clone = CreateInstanceCore();
		CopyLocalDependencyProperties(clone);
		CloneOverride(clone, currentValueOnly);
		return clone;
	}

	/// <summary>
	/// Override to copy non-DP state to <paramref name="destination"/> during cloning.
	/// The base implementation does nothing; <paramref name="currentValueOnly"/> is
	/// provided for subclasses that animate non-DP fields.
	/// </summary>
	protected virtual void CloneOverride(Freezable destination, bool currentValueOnly) { }

	/// <summary>
	/// Must be overridden to return a new, default-initialised instance of the
	/// concrete type (the prototype that <see cref="Clone"/> stamps values onto).
	/// </summary>
	protected abstract Freezable CreateInstanceCore();

	// ── Helpers ──────────────────────────────────────────────────────────────────

	/// <summary>Throws <see cref="InvalidOperationException"/> when frozen.</summary>
	protected void ThrowIfFrozen()
	{
		if (_isFrozen)
			throw new InvalidOperationException(
				"Cannot modify a frozen Freezable. Call Clone() first.");
	}

	void CopyLocalDependencyProperties(Freezable destination)
	{
		foreach (var kv in GetLocalValueSnapshot())
			destination.SetValue(kv.Key, kv.Value is Freezable f ? f.Clone() : kv.Value);
	}
}
