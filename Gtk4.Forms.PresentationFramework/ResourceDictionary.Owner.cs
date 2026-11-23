namespace System.Windows;

public partial class ResourceDictionary
{
	WeakReference? _owner;

	/// <summary>The element whose <c>Resources</c> this is: the inheritance context of its object values.</summary>
	internal DependencyObject? Owner
	{
		get => _owner?.Target as DependencyObject;
		set
		{
			var old = Owner;
			if (old != null && !ReferenceEquals(old, value))
				foreach (var item in _inner.Values)
					if (item is Freezable f && ReferenceEquals(f.InheritanceContext, old)) f.InheritanceContext = null;
			_owner = value == null ? null : new WeakReference(value);
			foreach (var entry in _inner.Values) ClaimEntry(entry);
		}
	}

	void ClaimEntry(object? value)
	{
		if (value is DependencyObject entry && !Data.BindingTarget.IsElement(entry) && Owner is { } owner
		    && !(entry is Freezable { IsFrozen: true }))
			entry.AddContextHolder(owner);
	}
}
