using System;

namespace System.Windows;

public sealed partial class DependencyProperty
{
	/// <summary>
	/// Adds <paramref name="ownerType"/> as an owner of this property and returns the same
	/// instance, so <see cref="FindByName"/> resolves it for that type.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="ownerType"/> is null.</exception>
	/// <exception cref="ArgumentException">The (name, owner) pair is already registered.</exception>
	public DependencyProperty AddOwner(Type ownerType)
	{
		if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));
		lock (_registryLock)
		{
			var key = (Name, ownerType);
			if (_registry.ContainsKey(key))
				throw new ArgumentException(
					$"DependencyProperty '{Name}' is already registered on '{ownerType.FullName}'.");
			_registry[key] = this;
		}
		return this;
	}

	/// <summary>
	/// Adds <paramref name="ownerType"/> as an owner and overrides this property's metadata
	/// for it (and its subclasses) with <paramref name="typeMetadata"/>.
	/// </summary>
	public DependencyProperty AddOwner(Type ownerType, PropertyMetadata typeMetadata)
	{
		if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));
		if (typeMetadata == null) throw new ArgumentNullException(nameof(typeMetadata));
		lock (_registryLock)
			if (_registry.ContainsKey((Name, ownerType)))
				throw new ArgumentException(
					$"DependencyProperty '{Name}' is already registered on '{ownerType.FullName}'.");
		// Register the owner only after the override succeeds, so a failed call leaves no stale entry.
		OverrideMetadata(ownerType, typeMetadata);
		return AddOwner(ownerType);
	}
}
