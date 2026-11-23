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
			_owner = value == null ? null : new WeakReference(value);
			foreach (var entry in _inner.Values) ClaimEntry(entry);
		}
	}

	void ClaimEntry(object? value)
	{
		if (value is DependencyObject entry && Owner is { } owner && !ReferenceEquals(entry, owner))
			entry.InheritanceContext = owner;
	}
}
